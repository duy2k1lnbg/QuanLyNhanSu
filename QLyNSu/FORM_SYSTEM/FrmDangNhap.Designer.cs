using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmDangNhap
    {
        private Panel panelCard;
        private LabelControl lblTitle;
        private TextEdit txtTenDangNhap;
        private TextEdit txtMatKhau;
        private SimpleButton btnDangNhap;
        private SimpleButton btnShowPassword;
        private LabelControl lblThongBao;

        private void InitializeComponent()
        {
            this.panelCard = new System.Windows.Forms.Panel();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.txtTenDangNhap = new DevExpress.XtraEditors.TextEdit();
            this.txtMatKhau = new DevExpress.XtraEditors.TextEdit();
            this.btnShowPassword = new DevExpress.XtraEditors.SimpleButton();
            this.btnDangNhap = new DevExpress.XtraEditors.SimpleButton();
            this.lblThongBao = new DevExpress.XtraEditors.LabelControl();
            this.panelCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenDangNhap.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMatKhau.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelCard
            // 
            this.panelCard.BackColor = System.Drawing.Color.White;
            this.panelCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCard.Controls.Add(this.lblTitle);
            this.panelCard.Controls.Add(this.txtTenDangNhap);
            this.panelCard.Controls.Add(this.txtMatKhau);
            this.panelCard.Controls.Add(this.btnShowPassword);
            this.panelCard.Controls.Add(this.btnDangNhap);
            this.panelCard.Controls.Add(this.lblThongBao);
            this.panelCard.Location = new System.Drawing.Point(0, 0);
            this.panelCard.Name = "panelCard";
            this.panelCard.Padding = new System.Windows.Forms.Padding(15);
            this.panelCard.Size = new System.Drawing.Size(669, 457);
            this.panelCard.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Location = new System.Drawing.Point(120, 40);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(182, 41);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "ĐĂNG NHẬP";
            // 
            // txtTenDangNhap
            // 
            this.txtTenDangNhap.Location = new System.Drawing.Point(60, 120);
            this.txtTenDangNhap.Name = "txtTenDangNhap";
            this.txtTenDangNhap.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
            this.txtTenDangNhap.Properties.Appearance.Options.UseForeColor = true;
            this.txtTenDangNhap.Properties.NullValuePrompt = "Tên đăng nhập";
            this.txtTenDangNhap.Size = new System.Drawing.Size(280, 22);
            this.txtTenDangNhap.TabIndex = 1;
            // 
            // txtMatKhau
            // 
            this.txtMatKhau.Location = new System.Drawing.Point(60, 170);
            this.txtMatKhau.Name = "txtMatKhau";
            this.txtMatKhau.Properties.Appearance.ForeColor = System.Drawing.Color.Black;
            this.txtMatKhau.Properties.Appearance.Options.UseForeColor = true;
            this.txtMatKhau.Properties.NullValuePrompt = "Mật khẩu";
            this.txtMatKhau.Properties.PasswordChar = '●';
            this.txtMatKhau.Size = new System.Drawing.Size(240, 22);
            this.txtMatKhau.TabIndex = 2;
            // 
            // btnShowPassword
            // 
            this.btnShowPassword.Location = new System.Drawing.Point(305, 170);
            this.btnShowPassword.Name = "btnShowPassword";
            this.btnShowPassword.Size = new System.Drawing.Size(35, 30);
            this.btnShowPassword.TabIndex = 3;
            this.btnShowPassword.Text = "👁";
            this.btnShowPassword.Click += new System.EventHandler(this.btnShowPassword_Click);
            // 
            // btnDangNhap
            // 
            this.btnDangNhap.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(123)))), ((int)(((byte)(255)))));
            this.btnDangNhap.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnDangNhap.Appearance.Options.UseBackColor = true;
            this.btnDangNhap.Appearance.Options.UseForeColor = true;
            this.btnDangNhap.Location = new System.Drawing.Point(60, 230);
            this.btnDangNhap.Name = "btnDangNhap";
            this.btnDangNhap.Size = new System.Drawing.Size(280, 45);
            this.btnDangNhap.TabIndex = 4;
            this.btnDangNhap.Text = "ĐĂNG NHẬP";
            this.btnDangNhap.Click += new System.EventHandler(this.btnDangNhap_Click);
            // 
            // lblThongBao
            // 
            this.lblThongBao.Appearance.ForeColor = System.Drawing.Color.Red;
            this.lblThongBao.Appearance.Options.UseForeColor = true;
            this.lblThongBao.Location = new System.Drawing.Point(60, 290);
            this.lblThongBao.Name = "lblThongBao";
            this.lblThongBao.Size = new System.Drawing.Size(0, 16);
            this.lblThongBao.TabIndex = 5;
            // 
            // FrmDangNhap
            // 
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.panelCard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmDangNhap";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FrmDangNhap_Load);
            this.panelCard.ResumeLayout(false);
            this.panelCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenDangNhap.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMatKhau.Properties)).EndInit();
            this.ResumeLayout(false);

        }
    }
}