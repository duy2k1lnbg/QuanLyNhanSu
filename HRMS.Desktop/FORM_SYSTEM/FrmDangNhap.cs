using System;
using System.Drawing;
using QLyNSu.Functions;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Bu.CLASS_SYSTEM;
using DA;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Linq;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmDangNhap : XtraForm
    {
        private bool drag = false;
        private Point startPoint = new Point(0, 0);

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );

        public FrmDangNhap()
        {
            InitializeComponent();

            // Set up dragging handlers to allow moving the borderless form
            this.panelLeft.MouseDown += DragPanel_MouseDown;
            this.panelLeft.MouseMove += DragPanel_MouseMove;
            this.panelLeft.MouseUp += DragPanel_MouseUp;

            this.panelRight.MouseDown += DragPanel_MouseDown;
            this.panelRight.MouseMove += DragPanel_MouseMove;
            this.panelRight.MouseUp += DragPanel_MouseUp;

            // Configure DevExpress TextEdit focus styles programmatically
            ConfigureInputFocusStyle(txtTenDangNhap);
            ConfigureInputFocusStyle(txtMatKhau);

            this.AcceptButton = btnDangNhap;

            // Hide the old small btnShowPassword button as the checkbox replaces it
            btnShowPassword.Visible = false;
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        private string GetMacAddress()
        {
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        return nic.GetPhysicalAddress().ToString();
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        private void ConfigureInputFocusStyle(TextEdit txt)
        {
            txt.Properties.AppearanceFocused.BorderColor = Color.FromArgb(9, 132, 227);
            txt.Properties.AppearanceFocused.Options.UseBorderColor = true;
        }

        private void FrmDangNhap_Load(object sender, EventArgs e)
        {
            TranslationManager.Translate(this);

            // Tải thông tin tài khoản đã ghi nhớ
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\QLyNSu\Login"))
                {
                    if (key != null)
                    {
                        string savedUser = key.GetValue("SavedUsername") as string;
                        if (!string.IsNullOrEmpty(savedUser))
                        {
                            txtTenDangNhap.Text = savedUser;
                            chkRememberMe.Checked = true;
                        }
                    }
                }
            }
            catch { }

            try {
                string bgPath = System.IO.Path.Combine(Application.StartupPath, "Resources", "login_bg.png");
                if (System.IO.File.Exists(bgPath)) {
                    this.panelLeft.BackgroundImage = Image.FromFile(bgPath);
                    this.panelLeft.BackgroundImageLayout = ImageLayout.Stretch;
                    // Make labels transparent so background shows through
                    this.lblLeftTitle.BackColor = Color.Transparent;
                    this.lblLeftSub.BackColor = Color.Transparent;
                    this.lblLeftSlogan.BackColor = Color.Transparent;
                    this.lblLeftVersion.BackColor = Color.Transparent;
                }
            } catch { }
            // Apply rounded corners to the form
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 16, 16));

            // Load modern SVG icons into TextEdit controls on startup
            try
            {
                var userSvg = DevExpress.Images.ImageResourceCache.Default.GetSvgImage("svgimages/icon builder/actions_user.svg");
                if (userSvg != null)
                {
                    txtTenDangNhap.Properties.ContextImageOptions.SvgImage = userSvg;
                }

                var keySvg = DevExpress.Images.ImageResourceCache.Default.GetSvgImage("svgimages/icon builder/security_key.svg");
                if (keySvg != null)
                {
                    txtMatKhau.Properties.ContextImageOptions.SvgImage = keySvg;
                }
            }
            catch (Exception)
            {
                // Fallback silently if icons cannot be resolved
            }

            txtTenDangNhap.Focus();
        }

        // Draw a beautiful custom gradient and decorative translucent shapes on the left branding panel
        private void PanelLeft_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Linear Gradient
            using (LinearGradientBrush brush = new LinearGradientBrush(
                panelLeft.ClientRectangle,
                Color.FromArgb(9, 132, 227),  // Custom Blue
                Color.FromArgb(108, 92, 231), // Custom Purple
                45F))
            {
                g.FillRectangle(brush, panelLeft.ClientRectangle);
            }

            // Decorative Translucent Circles
            using (SolidBrush circleBrush = new SolidBrush(Color.FromArgb(15, 255, 255, 255)))
            {
                g.FillEllipse(circleBrush, -60, -60, 220, 220);
                g.FillEllipse(circleBrush, panelLeft.Width - 120, panelLeft.Height - 120, 180, 180);
                g.FillEllipse(circleBrush, panelLeft.Width / 2 - 80, panelLeft.Height - 80, 120, 120);
            }
        }

        // Draggable Window Logic
        private void DragPanel_MouseDown(object sender, MouseEventArgs e)
        {
            drag = true;
            startPoint = new Point(e.X, e.Y);
        }

        private void DragPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
            }
        }

        private void DragPanel_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
        }

        // Close Label Actions
        private void LblClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LblClose_MouseEnter(object sender, EventArgs e)
        {
            var lbl = sender as DevExpress.XtraEditors.LabelControl;
            if (lbl != null) lbl.ForeColor = Color.FromArgb(231, 76, 60); // Red on hover
        }

        private void LblClose_MouseLeave(object sender, EventArgs e)
        {
            var lbl = sender as DevExpress.XtraEditors.LabelControl;
            if (lbl != null) lbl.ForeColor = Color.FromArgb(127, 140, 141); // Normal Gray
        }

        private void LblMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        // Action Buttons
        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            lblThongBao.Visible = false;
            lblThongBao.Text = string.Empty;

            string username = txtTenDangNhap.Text.Trim();
            string password = txtMatKhau.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblThongBao.Text = TranslationManager.Translate("Vui lòng nhập tên đăng nhập và mật khẩu.");
                lblThongBao.Visible = true;
                return;
            }

            btnDangNhap.Enabled = false;
            btnDangNhap.Text = TranslationManager.Translate("Đang xử lý...");

            try
            {
                string currentIp = GetLocalIPAddress();
                string currentMac = GetMacAddress();
                string pcName = Environment.MachineName;

                // 1. Sử dụng AuthSecurityService dùng chung cho DESKTOP, tạo phiên DB thực trong TB_AUTH_SESSION
                var authService = new Bu.CLASS_SECURITY.AuthSecurityService();
                var loginResult = authService.AuthenticateAsync(
                    usernameOrEmpCode: username,
                    password: password,
                    clientType: Bu.CLASS_SECURITY.AppChannels.Desktop,
                    platform: "WinForms",
                    deviceId: currentMac,
                    deviceName: pcName,
                    clientIp: currentIp,
                    userAgent: "HRMS-Desktop/1.0",
                    correlationId: Guid.NewGuid().ToString("N")
                ).GetAwaiter().GetResult();

                var user = loginResult?.User as DA.TB_SYS_USER;
                if (loginResult != null && loginResult.Success && user != null)
                {

                    // 2. Ghi nhật ký đăng nhập truyền thống TB_SYS_LOGIN_HISTORY
                    try
                    {
                        using (var db = new MyEntities())
                        {
                            db.Database.ExecuteSqlCommand("INSERT INTO HR.TB_SYS_LOGIN_HISTORY (ID_USER, IP_ADDRESS, MAC_ADDRESS, TEN_MAY_TINH, TRANGTHAI, THOIGIAN) VALUES (:p1, :p2, :p3, :p4, :p5, CURRENT_TIMESTAMP)", 
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p1", user.IDUSER),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p2", currentIp ?? (object)DBNull.Value),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p3", currentMac ?? (object)DBNull.Value),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p4", pcName ?? (object)DBNull.Value),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p5", "Thành công"));
                                
                            var insertedId = db.Database.SqlQuery<decimal>("SELECT ID_LOGIN FROM (SELECT ID_LOGIN FROM HR.TB_SYS_LOGIN_HISTORY WHERE ID_USER = :p1 ORDER BY THOIGIAN DESC) WHERE ROWNUM = 1", 
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p1", user.IDUSER)).FirstOrDefault();
                            UserSession.CurrentLoginId = insertedId;
                        }
                    }
                    catch { }

                    // 3. Khởi tạo UserSession từ kết quả xác thực thực tế
                    UserSession.CurrentUser = user;
                    UserSession.CurrentSessionId = loginResult.SessionId;
                    UserSession.CurrentJti = loginResult.Jti;
                    UserSession.CurrentTokenVersion = loginResult.TokenVersion;
                    UserSession.CurrentChannel = Bu.CLASS_SECURITY.AppChannels.Desktop;

                    if (loginResult.DetailedRights != null && loginResult.DetailedRights.Any())
                    {
                        UserSession.DetailedRights = loginResult.DetailedRights;
                        UserSession.UserRights = loginResult.Rights ?? new System.Collections.Generic.List<string>();
                    }
                    else
                    {
                        var sysUser = new SYS_USER();
                        UserSession.DetailedRights = sysUser.GetDetailedRights(user.IDUSER);
                        UserSession.UserRights = sysUser.GetRights(user.IDUSER);
                    }
                    
                    // Set Global Audit & Session properties
                    MyEntities.CurrentAuditUserId = (int)user.IDUSER;
                    MyEntities.CurrentAuditUsername = user.FULLNAME;
                    MyEntities.CurrentSessionId = loginResult.SessionId;

                    // 4. Trao đổi phiên Desktop lấy Bearer Token cho AiApiClient
                    try
                    {
                        AiApiClient.Instance.ExchangeDesktopTokenAsync(
                            username: username,
                            password: password,
                            sessionId: loginResult.SessionId,
                            jti: loginResult.Jti
                        ).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("[FrmDangNhap] Không lấy được Bearer Token cho AiApiClient: " + ex.Message);
                    }

                    // Ghi nhớ đăng nhập
                    try
                    {
                        using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\QLyNSu\Login"))
                        {
                            if (chkRememberMe.Checked)
                            {
                                key.SetValue("SavedUsername", username);
                            }
                            else
                            {
                                key.DeleteValue("SavedUsername", false);
                            }
                            key.DeleteValue("SavedPassword", false);
                        }
                    }
                    catch { }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    string err = loginResult?.ErrorMessage ?? "Tên đăng nhập hoặc mật khẩu không đúng.";
                    lblThongBao.Text = TranslationManager.Translate(err);
                    lblThongBao.Visible = true;
                }
            }
            catch (ApplicationException appEx)
            {
                if (appEx.Message == "ACCOUNT_LOCKED")
                {
                    lblThongBao.Text = TranslationManager.Translate("Tài khoản đã bị khóa vĩnh viễn. Vui lòng liên hệ Quản trị viên.");
                }
                else if (appEx.Message == "ACCOUNT_LOCKED_NOW")
                {
                    lblThongBao.Text = TranslationManager.Translate("Nhập sai 5 lần. Tài khoản bị khóa tạm thời 15 phút.");
                }
                else if (appEx.Message == "WRONG_PASSWORD")
                {
                    lblThongBao.Text = TranslationManager.Translate("Tên đăng nhập hoặc mật khẩu không đúng.");
                }
                else if (appEx.Message.StartsWith("TEMPORARY_LOCKED"))
                {
                    string mins = appEx.Message.Split('|')[1];
                    lblThongBao.Text = TranslationManager.Translate($"Tài khoản đang bị khóa tạm thời. Vui lòng thử lại sau {mins} phút.");
                }
                else if (appEx.Message == "EMPLOYEE_MOBILE_ONLY" || appEx.Message == "FORBIDDEN_DESKTOP")
                {
                    lblThongBao.Text = TranslationManager.Translate("Tài khoản không có quyền đăng nhập trên ứng dụng Desktop. Vui lòng liên hệ Quản trị viên.");
                }
                else
                {
                    lblThongBao.Text = TranslationManager.Translate("Lỗi: ") + appEx.Message;
                }
                lblThongBao.Visible = true;
            }
            catch (Exception ex)
            {
                lblThongBao.Text = TranslationManager.Translate("Lỗi kết nối cơ sở dữ liệu") + ": " + ex.Message;
                lblThongBao.Visible = true;
            }
            finally
            {
                btnDangNhap.Enabled = true;
                btnDangNhap.Text = TranslationManager.Translate("ĐĂNG NHẬP");
            }
        }

        private void BtnThoat_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnShowPassword_Click(object sender, EventArgs e)
        {
            if (txtMatKhau.Properties.PasswordChar == '\0')
            {
                txtMatKhau.Properties.PasswordChar = '●';
                btnShowPassword.Text = "👁";
            }
            else
            {
                txtMatKhau.Properties.PasswordChar = '\0';
                btnShowPassword.Text = "🙈";
            }
        }



        private void ChkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowPassword.Checked)
            {
                txtMatKhau.Properties.PasswordChar = '\0'; // Show password characters
            }
            else
            {
                txtMatKhau.Properties.PasswordChar = '●'; // Hide password characters
            }
        }

        private void txtMatKhau_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void btnLanguage_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmSetting())
            {
                QLyNSu.Functions.TranslationManager.Translate(frm);
                frm.ShowDialog();
            }
        }

        private void btnServerConfig_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmDatabaseConfig())
            {
                QLyNSu.Functions.TranslationManager.Translate(frm);
                frm.ShowDialog();
            }
        }
    }
}