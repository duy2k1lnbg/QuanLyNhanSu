using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmDangNhap : XtraForm
    {
        private Timer glowTimer = new Timer();
        private float glowIntensity = 0;
        private bool glowIncreasing = true;

        private float cardShadowOffset = 5;
        private bool cardHover = false;

        public FrmDangNhap()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.AcceptButton = btnDangNhap;

            this.BackgroundImage = Properties.Resources.background; // tên image trong resource
            this.BackgroundImageLayout = ImageLayout.Stretch;

            // Button hover
            btnDangNhap.MouseEnter += BtnDangNhap_MouseEnter;
            btnDangNhap.MouseLeave += BtnDangNhap_MouseLeave;

            // Card hover
            panelCard.MouseEnter += PanelCard_MouseEnter;
            panelCard.MouseLeave += PanelCard_MouseLeave;

            // TextEdit glow animation
            txtTenDangNhap.Enter += TextEdit_Enter;
            txtTenDangNhap.Leave += TextEdit_Leave;
            txtMatKhau.Enter += TextEdit_Enter;
            txtMatKhau.Leave += TextEdit_Leave;

            // Timer animation
            glowTimer.Interval = 20;
            glowTimer.Tick += GlowTimer_Tick;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Vẽ background
            if (this.BackgroundImage != null)
            {
                e.Graphics.DrawImage(this.BackgroundImage, this.ClientRectangle);
            }
            else
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(58, 123, 255),
                    Color.FromArgb(142, 84, 233),
                    45F))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            }

            // Overlay gradient để card nổi bật
            using (LinearGradientBrush overlay = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(100, 58, 123, 255),
                Color.FromArgb(100, 142, 84, 233),
                45F))
            {
                e.Graphics.FillRectangle(overlay, this.ClientRectangle);
            }

            // Shadow cho card
            Rectangle shadowRect = panelCard.Bounds;
            shadowRect.Offset((int)cardShadowOffset, (int)cardShadowOffset);
            using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
            {
                e.Graphics.FillRectangle(shadowBrush, shadowRect);
            }
        }

        private void FrmDangNhap_Load(object sender, EventArgs e)
        {
            CenterCard();
            txtTenDangNhap.Focus();
        }

        private void CenterCard()
        {
            panelCard.Left = (this.ClientSize.Width - panelCard.Width) / 2;
            panelCard.Top = (this.ClientSize.Height - panelCard.Height) / 2;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterCard();
        }

        //private void btnDangNhap_Click(object sender, EventArgs e)
        //{
        //    lblThongBao.Visible = false;
        //    btnDangNhap.Enabled = false;
        //    btnDangNhap.Text = "Đang xử lý...";

        //    if (txtTenDangNhap.Text == "admin" && txtMatKhau.Text == "123")
        //        DialogResult = DialogResult.OK;
        //    else
        //    {
        //        lblThongBao.Text = "Tên đăng nhập hoặc mật khẩu không đúng.";
        //        lblThongBao.Visible = true;
        //    }

        //    btnDangNhap.Enabled = true;
        //    btnDangNhap.Text = "ĐĂNG NHẬP";
        //}

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            lblThongBao.Visible = false;

            string username = txtTenDangNhap.Text.Trim();
            string password = txtMatKhau.Text.Trim();

            // Kiểm tra rỗng
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblThongBao.Text = "Tên đăng nhập và mật khẩu không được để trống!";
                lblThongBao.Appearance.ForeColor = Color.Red;
                lblThongBao.Visible = true;
                return; // Không tiếp tục xử lý
            }

            btnDangNhap.Enabled = false;
            btnDangNhap.Text = "Đang xử lý...";

            // Kiểm tra đăng nhập
            if (username == "admin" && password == "123")
            {
                lblThongBao.Text = "Đăng nhập thành công!";
                lblThongBao.Appearance.ForeColor = Color.Green;
                lblThongBao.Visible = true;

                // Optional: đóng form sau 1 giây
                var t = new Timer();
                t.Interval = 1000;
                t.Tick += (s, ev) =>
                {
                    t.Stop();
                    this.DialogResult = DialogResult.OK; // hoặc this.Close();
                };
                t.Start();
            }
            else
            {
                lblThongBao.Text = "Tên đăng nhập hoặc mật khẩu không đúng.";
                lblThongBao.Appearance.ForeColor = Color.Red;
                lblThongBao.Visible = true;
            }

            btnDangNhap.Enabled = true;
            btnDangNhap.Text = "ĐĂNG NHẬP";
        }

        private void btnShowPassword_Click(object sender, EventArgs e)
        {
            txtMatKhau.Properties.PasswordChar =
                txtMatKhau.Properties.PasswordChar == '\0' ? '●' : '\0';
        }

        // Button hover animation
        private void BtnDangNhap_MouseEnter(object sender, EventArgs e)
        {
            btnDangNhap.Appearance.BackColor = Color.FromArgb(70, 140, 255);
        }
        private void BtnDangNhap_MouseLeave(object sender, EventArgs e)
        {
            btnDangNhap.Appearance.BackColor = Color.FromArgb(58, 123, 255);
        }

        // Card hover animation
        private void PanelCard_MouseEnter(object sender, EventArgs e)
        {
            cardHover = true;
            AnimateCardShadow(true);
        }
        private void PanelCard_MouseLeave(object sender, EventArgs e)
        {
            cardHover = false;
            AnimateCardShadow(false);
        }

        private async void AnimateCardShadow(bool hover)
        {
            float targetOffset = hover ? 15f : 5f;
            while ((hover && cardShadowOffset < targetOffset) || (!hover && cardShadowOffset > targetOffset))
            {
                cardShadowOffset += hover ? 1f : -1f;
                this.Invalidate();
                await System.Threading.Tasks.Task.Delay(15);
            }
            cardShadowOffset = targetOffset;
            this.Invalidate();
        }

        // TextEdit glow animation
        private void TextEdit_Enter(object sender, EventArgs e)
        {
            glowIntensity = 0;
            glowIncreasing = true;
            glowTimer.Tag = sender as TextEdit;
            glowTimer.Start();
        }

        private void TextEdit_Leave(object sender, EventArgs e)
        {
            glowTimer.Stop();
            var txt = sender as TextEdit;
            txt.Properties.Appearance.BackColor = Color.White;
            txt.Invalidate();
        }

        private void GlowTimer_Tick(object sender, EventArgs e)
        {
            var timer = sender as Timer;
            var txt = timer.Tag as TextEdit;
            if (txt == null) return;

            if (glowIncreasing)
                glowIntensity += 0.05f;
            else
                glowIntensity -= 0.05f;

            if (glowIntensity >= 1f) glowIncreasing = false;
            if (glowIntensity <= 0f) glowIncreasing = true;

            int r = 230 + (int)(25 * glowIntensity);
            int g = 245 + (int)(10 * glowIntensity);
            int b = 255;
            txt.Properties.Appearance.BackColor = Color.FromArgb(r, g, b);
        }
    }
}