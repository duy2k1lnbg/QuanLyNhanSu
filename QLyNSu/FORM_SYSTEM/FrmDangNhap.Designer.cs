using System;
using System.Drawing;
using DevExpress.XtraEditors;

namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmDangNhap
    {
        private PanelControl panelCard;
        private LabelControl lblTitle;
        private TextEdit txtTenDangNhap;
        private TextEdit txtMatKhau;
        private SimpleButton btnDangNhap;
        private SimpleButton btnShowPassword;
        private LabelControl lblThongBao;

        private void InitializeComponent()
        {
            this.panelCard = new PanelControl();
            this.lblTitle = new LabelControl();
            this.txtTenDangNhap = new TextEdit();
            this.txtMatKhau = new TextEdit();
            this.btnDangNhap = new SimpleButton();
            this.btnShowPassword = new SimpleButton();
            this.lblThongBao = new LabelControl();

            ((System.ComponentModel.ISupportInitialize)(this.panelCard)).BeginInit();
            this.panelCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenDangNhap.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMatKhau.Properties)).BeginInit();

            // Form
            this.ClientSize = new Size(900, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Đăng nhập";
            this.Load += new EventHandler(this.FrmDangNhap_Load);

            // Panel Card
            this.panelCard.Size = new Size(400, 320);
            this.panelCard.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelCard.Appearance.BackColor = Color.White;
            this.panelCard.Appearance.Options.UseBackColor = true;

            // Title
            this.lblTitle.Text = "ĐĂNG NHẬP";
            this.lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblTitle.Location = new Point(120, 30);

            // Username
            this.txtTenDangNhap.Location = new Point(60, 90);
            this.txtTenDangNhap.Size = new Size(280, 30);
            this.txtTenDangNhap.Properties.NullValuePrompt = "Tên đăng nhập";
            this.txtTenDangNhap.Properties.NullValuePromptShowForEmptyValue = true;

            // Password
            this.txtMatKhau.Location = new Point(60, 140);
            this.txtMatKhau.Size = new Size(240, 30);
            this.txtMatKhau.Properties.PasswordChar = '●';
            this.txtMatKhau.Properties.NullValuePrompt = "Mật khẩu";
            this.txtMatKhau.Properties.NullValuePromptShowForEmptyValue = true;

            // Show password button
            this.btnShowPassword.Location = new Point(305, 140);
            this.btnShowPassword.Size = new Size(35, 30);
            this.btnShowPassword.Text = "👁";

            // Login button
            this.btnDangNhap.Location = new Point(60, 200);
            this.btnDangNhap.Size = new Size(280, 45);
            this.btnDangNhap.Text = "ĐĂNG NHẬP";
            this.btnDangNhap.Appearance.BackColor = Color.FromArgb(58, 123, 255);
            this.btnDangNhap.Appearance.ForeColor = Color.White;
            this.btnDangNhap.Appearance.Options.UseBackColor = true;
            this.btnDangNhap.Appearance.Options.UseForeColor = true;

            // Thông báo
            this.lblThongBao.Location = new Point(60, 260);
            this.lblThongBao.Appearance.ForeColor = Color.Red;
            this.lblThongBao.Visible = false;

            // Add controls
            this.panelCard.Controls.Add(this.lblTitle);
            this.panelCard.Controls.Add(this.txtTenDangNhap);
            this.panelCard.Controls.Add(this.txtMatKhau);
            this.panelCard.Controls.Add(this.btnShowPassword);
            this.panelCard.Controls.Add(this.btnDangNhap);
            this.panelCard.Controls.Add(this.lblThongBao);

            this.Controls.Add(this.panelCard);

            this.panelCard.ResumeLayout(false);
            this.panelCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenDangNhap.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMatKhau.Properties)).EndInit();
        }
    }
}