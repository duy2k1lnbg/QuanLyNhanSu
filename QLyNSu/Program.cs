using DevExpress.Skins;
using DevExpress.UserSkins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu
{
    internal static class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        public static System.Drawing.Icon AppIcon { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            System.IO.File.WriteAllText(@"d:\QL_NS\QuanLyNhanSu\seed_debug.txt", "Started with args: " + string.Join(", ", Environment.GetCommandLineArgs()));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Load application icon from Resources
            string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "hrm_icon.ico");
            if (System.IO.File.Exists(iconPath))
            {
                try
                {
                    AppIcon = new System.Drawing.Icon(iconPath);
                }
                catch { }
            }

            // Load saved display language at startup
            QLyNSu.Functions.TranslationManager.LoadLanguage();
            QLyNSu.Functions.TranslationManager.LoadDictionaryFromDB();
            QLyNSu.Functions.TranslationManager.InitializeHook();

            // Override DB Config from AppData JSON if it exists
            try
            {
                string overrideFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QLyNSu_DBConfig.json");
                if (System.IO.File.Exists(overrideFile))
                {
                    string content = System.IO.File.ReadAllText(overrideFile);
                    var dict = new System.Collections.Generic.Dictionary<string, string>();

                    if (content.Contains("ActiveProfileId"))
                    {
                        var configData = Newtonsoft.Json.JsonConvert.DeserializeObject<QLyNSu.FORM_SYSTEM.DatabaseConfigData>(content);
                        if (configData != null && !string.IsNullOrEmpty(configData.ActiveProfileId))
                        {
                            var activeProfile = configData.Profiles?.FirstOrDefault(p => p.Id == configData.ActiveProfileId);
                            if (activeProfile != null)
                            {
                                string dataSource = activeProfile.ServerIP;
                                if (!string.IsNullOrEmpty(activeProfile.Port)) dataSource += $":{activeProfile.Port}";
                                if (!string.IsNullOrEmpty(activeProfile.Database)) dataSource += $"/{activeProfile.Database}";

                                string authParams = string.Equals(activeProfile.AuthType, "OS", StringComparison.OrdinalIgnoreCase) ? "Integrated Security=yes;" : $"USER ID={activeProfile.Username};PASSWORD={activeProfile.Password};";
                                string roleParams = (string.IsNullOrEmpty(activeProfile.Role) || string.Equals(activeProfile.Role, "Default", StringComparison.OrdinalIgnoreCase)) ? "" : $"DBA Privilege={activeProfile.Role};";

                                string providerHR = $"DATA SOURCE={dataSource};{authParams}{roleParams}PERSIST SECURITY INFO=True";
                                string providerAI = $"USER ID=AI_READONLY;PASSWORD=AI;DATA SOURCE={dataSource};PERSIST SECURITY INFO=True";
                                
                                dict["MyEntities"] = $"metadata=res://*/QLNhanSu.csdl|res://*/QLNhanSu.ssdl|res://*/QLNhanSu.msl;provider=Oracle.ManagedDataAccess.Client;provider connection string=\"{providerHR}\"";
                                dict["AIEntities"] = $"metadata=res://*/AIEntities.csdl|res://*/AIEntities.ssdl|res://*/AIEntities.msl;provider=Oracle.ManagedDataAccess.Client;provider connection string=\"{providerAI}\"";
                            }
                        }
                    }
                    else
                    {
                        dict = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(content);
                    }

                    if (dict != null)
                    {
                        if (dict.ContainsKey("MyEntities")) DA.MyEntities.GlobalConnectionString = dict["MyEntities"];
                        if (dict.ContainsKey("AIEntities")) DA.AiEntities.GlobalConnectionString = dict["AIEntities"];
                    }
                }
            }
            catch { }

            string[] cmdArgs = Environment.GetCommandLineArgs();
            if (cmdArgs.Contains("--seed"))
            {
                System.IO.File.AppendAllText(@"d:\QL_NS\QuanLyNhanSu\seed_debug.txt", "\nEntered seed block");
                AllocConsole();
                Console.WriteLine("========================================");
                Console.WriteLine("    QDRANT SEEDER TOOL (AI_READONLY)   ");
                Console.WriteLine("========================================");
                try
                {
                    var vectorService = Bu.Services.AI_Services.AiServiceLocator.GetService<Bu.Services.AI_Services.Interfaces.IVectorService>();
                    Console.WriteLine("Clearing old data in Qdrant...");
                    vectorService.Clear();
                    Console.WriteLine("Qdrant collection cleared and recreated.");

                    using (var db = new DA.AiEntities())
                    {
                        Console.WriteLine("Connecting to Oracle AI_READONLY...");
                        var employees = db.V_AI_EMPLOYEE.ToList();
                        Console.WriteLine($"Found {employees.Count} employees.");
                        int count = 0;
                        foreach (var emp in employees)
                        {
                            string text = $"Nhân viên {emp.HOTEN} (Mã NV: {emp.MANV}), sinh ngày {emp.NGAYSINH:dd/MM/yyyy}, " +
                                          $"thuộc phòng ban {emp.TEN_PHONGBAN}, chức vụ {emp.TEN_CHUCVU}, bộ phận {emp.TEN_BOPHAN}, " +
                                          $"số điện thoại {emp.DIENTHOAI}, địa chỉ {emp.DIACHI}.";
                            vectorService.Add(text, "EMPLOYEE");
                            count++;
                            Console.WriteLine($"[{count}/{employees.Count}] Seeded employee: {emp.HOTEN}");
                        }
                        
                        var insurances = db.V_AI_INSURANCE.ToList();
                        Console.WriteLine($"Found {insurances.Count} insurance records.");
                        int insCount = 0;
                        foreach (var ins in insurances)
                        {
                            string text = $"Nhân viên {ins.HOTEN} (Mã NV: {ins.MANV}) có số bảo hiểm là {ins.SOBH}, " +
                                          $"cấp ngày {ins.NGAYCAP:dd/MM/yyyy} tại {ins.NOICAP}, đăng ký khám tại {ins.NOIKHAMBENH}.";
                            vectorService.Add(text, "INSURANCE");
                            insCount++;
                            Console.WriteLine($"[{insCount}/{insurances.Count}] Seeded insurance: {ins.HOTEN}");
                        }

                        var allowances = db.V_AI_ALLOWANCE.ToList();
                        Console.WriteLine($"Found {allowances.Count} allowance records.");
                        int alCount = 0;
                        foreach (var al in allowances)
                        {
                            string text = $"Nhân viên {al.HOTEN} (Mã NV: {al.MANV}) nhận phụ cấp {al.TENPC} với số tiền {al.SOTIEN:N0} VNĐ cho kỳ công {al.KYCONG}.";
                            vectorService.Add(text, "ALLOWANCE");
                            alCount++;
                            Console.WriteLine($"[{alCount}/{allowances.Count}] Seeded allowance: {al.HOTEN}");
                        }

                        Console.WriteLine("========================================");
                        Console.WriteLine($"SEEDING COMPLETE! Total vectors inserted: {count + insCount + alCount}");
                        Console.WriteLine("========================================");
                        System.IO.File.WriteAllText(@"d:\QL_NS\QuanLyNhanSu\seed_results.txt", $"Employees: {employees.Count}, Insurances: {insurances.Count}, Allowances: {allowances.Count}, Total Inserted: {count + insCount + alCount}");
                    }
                }
                catch (Exception ex)
                {
                    System.IO.File.WriteAllText(@"d:\QL_NS\QuanLyNhanSu\seed_error.log", $"ERROR: {ex.Message}\n");
                    var inner = ex.InnerException;
                    while (inner != null)
                    {
                        System.IO.File.AppendAllText(@"d:\QL_NS\QuanLyNhanSu\seed_error.log", $"INNER: {inner.Message}\n");
                        inner = inner.InnerException;
                    }
                }
                Console.WriteLine("Exiting seeder...");
                return;
            }


            // Register DevExpress skins and enable form skinning
            DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.Skins.SkinManager.EnableFormSkins();

            // Set global DevExpress settings for a modern premium aesthetic
            // DevExpress.XtraEditors.WindowsFormsSettings.ForceDirectXPaint();
            // DevExpress.XtraEditors.WindowsFormsSettings.DefaultFont = new System.Drawing.Font("Segoe UI", 9.5F);
            // DevExpress.XtraEditors.WindowsFormsSettings.DefaultMenuFont = new System.Drawing.Font("Segoe UI", 9.5F);
            // DevExpress.XtraEditors.WindowsFormsSettings.ScrollUIMode = DevExpress.XtraEditors.ScrollUIMode.Touch;

            // Apply "The Bezier" skin with "Gleam" palette as default
            // DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("The Bezier", "Gleam");

            // 1. Show Splash Form
            using (var splash = new FrmStartup())
            {
                splash.ShowDialog();
            }

            // 2. Show Login Form
            using (var login = new FORM_SYSTEM.FrmDangNhap())
            {
                if (login.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // 3. Login successful, run MainForm
                    Application.Run(new MainForm());
                }
            }

            QLyNSu.Functions.TranslationManager.ShutdownHook();
        }
    }
}
