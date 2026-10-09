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
                                
                                dict["MyEntities"] = $"metadata=res://*/QLNhanSu.csdl|res://*/QLNhanSu.ssdl|res://*/QLNhanSu.msl;provider=Oracle.ManagedDataAccess.Client;provider connection string=\"{providerHR}\"";
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
                    }
                }
            }
            catch { }



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
