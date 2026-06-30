using System;
using System.Drawing;
using QLyNSu.Functions;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Bu.CLASS_SYSTEM;
using DA;

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

        private void ConfigureInputFocusStyle(TextEdit txt)
        {
            txt.Properties.AppearanceFocused.BorderColor = Color.FromArgb(9, 132, 227);
            txt.Properties.AppearanceFocused.Options.UseBorderColor = true;
        }

        private void FrmDangNhap_Load(object sender, EventArgs e)
        {
            TranslationManager.Translate(this);
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
            lblClose.ForeColor = Color.FromArgb(231, 76, 60); // Red on hover
        }

        private void LblClose_MouseLeave(object sender, EventArgs e)
        {
            lblClose.ForeColor = Color.FromArgb(127, 140, 141); // Normal Gray
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
                var sysUser = new SYS_USER();
                var user = sysUser.Login(username, password);
                if (user != null)
                {
                    UserSession.CurrentUser = user;
                    UserSession.UserRights = sysUser.GetRights(user.IDUSER);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblThongBao.Text = TranslationManager.Translate("Tên đăng nhập hoặc mật khẩu không đúng.");
                    lblThongBao.Visible = true;
                }
            }
            catch (ApplicationException appEx) when (appEx.Message == "ACCOUNT_LOCKED")
            {
                lblThongBao.Text = TranslationManager.Translate("Tài khoản đã bị khóa. Vui lòng liên hệ Quản trị viên.");
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