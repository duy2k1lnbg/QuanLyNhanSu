using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmDangNhap
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelRight;
        private LabelControl lblLeftTitle;
        private LabelControl lblLeftSub;
        private LabelControl lblLeftSlogan;
        private LabelControl lblLeftVersion;
        private LabelControl lblRightTitle;
        private LabelControl lblRightSub;
        private LabelControl lblClose;
        private LabelControl lblMinimize;
        private TextEdit txtTenDangNhap;
        private TextEdit txtMatKhau;
        private SimpleButton btnShowPassword;
        private CheckEdit chkRememberMe;
        private CheckEdit chkShowPassword;
        private LabelControl lblThongBao;
        private SimpleButton btnDangNhap;
        private SimpleButton btnThoat;
        private SimpleButton btnLanguage;
        private SimpleButton btnServerConfig;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDangNhap));
            this.panelLeft = new System.Windows.Forms.Panel();
            this.lblLeftTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblLeftSub = new DevExpress.XtraEditors.LabelControl();
            this.lblLeftSlogan = new DevExpress.XtraEditors.LabelControl();
            this.lblLeftVersion = new DevExpress.XtraEditors.LabelControl();
            this.btnServerConfig = new DevExpress.XtraEditors.SimpleButton();
            this.panelRight = new System.Windows.Forms.Panel();
            this.lblClose = new DevExpress.XtraEditors.LabelControl();
            this.lblMinimize = new DevExpress.XtraEditors.LabelControl();
            this.lblRightTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblRightSub = new DevExpress.XtraEditors.LabelControl();
            this.txtTenDangNhap = new DevExpress.XtraEditors.TextEdit();
            this.btnLanguage = new DevExpress.XtraEditors.SimpleButton();
            this.txtMatKhau = new DevExpress.XtraEditors.TextEdit();
            this.btnShowPassword = new DevExpress.XtraEditors.SimpleButton();
            this.chkRememberMe = new DevExpress.XtraEditors.CheckEdit();
            this.chkShowPassword = new DevExpress.XtraEditors.CheckEdit();
            this.lblThongBao = new DevExpress.XtraEditors.LabelControl();
            this.btnDangNhap = new DevExpress.XtraEditors.SimpleButton();
            this.btnThoat = new DevExpress.XtraEditors.SimpleButton();
            this.panelLeft.SuspendLayout();
            this.panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenDangNhap.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMatKhau.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRememberMe.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowPassword.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.lblLeftTitle);
            this.panelLeft.Controls.Add(this.lblLeftSub);
            this.panelLeft.Controls.Add(this.lblLeftSlogan);
            this.panelLeft.Controls.Add(this.lblLeftVersion);
            this.panelLeft.Controls.Add(this.btnServerConfig);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(435, 700);
            this.panelLeft.TabIndex = 0;
            // 
            // lblLeftTitle
            // 
            this.lblLeftTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblLeftTitle.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblLeftTitle.Appearance.Options.UseFont = true;
            this.lblLeftTitle.Appearance.Options.UseForeColor = true;
            this.lblLeftTitle.Location = new System.Drawing.Point(40, 110);
            this.lblLeftTitle.Name = "lblLeftTitle";
            this.lblLeftTitle.Size = new System.Drawing.Size(256, 54);
            this.lblLeftTitle.TabIndex = 0;
            this.lblLeftTitle.Text = "HRM SYSTEM";
            // 
            // lblLeftSub
            // 
            this.lblLeftSub.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.lblLeftSub.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblLeftSub.Appearance.Options.UseFont = true;
            this.lblLeftSub.Appearance.Options.UseForeColor = true;
            this.lblLeftSub.Location = new System.Drawing.Point(40, 165);
            this.lblLeftSub.Name = "lblLeftSub";
            this.lblLeftSub.Size = new System.Drawing.Size(253, 23);
            this.lblLeftSub.TabIndex = 1;
            this.lblLeftSub.Text = "Enterprise Management Platform";
            // 
            // lblLeftSlogan
            // 
            this.lblLeftSlogan.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Italic);
            this.lblLeftSlogan.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblLeftSlogan.Appearance.Options.UseFont = true;
            this.lblLeftSlogan.Appearance.Options.UseForeColor = true;
            this.lblLeftSlogan.Location = new System.Drawing.Point(40, 230);
            this.lblLeftSlogan.Name = "lblLeftSlogan";
            this.lblLeftSlogan.Size = new System.Drawing.Size(211, 63);
            this.lblLeftSlogan.TabIndex = 2;
            this.lblLeftSlogan.Text = "⚡   Local AI Assistant Integrated\r\n⚡   Oracle 19c Enterprise DB\r\n⚡    High-Securi" +
    "ty Architecture";
            // 
            // lblLeftVersion
            // 
            this.lblLeftVersion.Appearance.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblLeftVersion.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.lblLeftVersion.Appearance.Options.UseFont = true;
            this.lblLeftVersion.Appearance.Options.UseForeColor = true;
            this.lblLeftVersion.Location = new System.Drawing.Point(40, 420);
            this.lblLeftVersion.Name = "lblLeftVersion";
            this.lblLeftVersion.Size = new System.Drawing.Size(102, 19);
            this.lblLeftVersion.TabIndex = 3;
            this.lblLeftVersion.Text = "Phiên bản v3.5.0";
            // 
            // btnServerConfig
            // 
            this.btnServerConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnServerConfig.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.btnServerConfig.Appearance.Options.UseFont = true;
            this.btnServerConfig.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnServerConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnServerConfig.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnServerConfig.ImageOptions.SvgImage")));
            this.btnServerConfig.Location = new System.Drawing.Point(0, 0);
            this.btnServerConfig.Name = "btnServerConfig";
            this.btnServerConfig.Size = new System.Drawing.Size(40, 40);
            this.btnServerConfig.TabIndex = 10;
            this.btnServerConfig.ToolTip = "Cấu hình cơ sở dữ liệu";
            this.btnServerConfig.Click += new System.EventHandler(this.btnServerConfig_Click);
            // 
            // panelRight
            // 
            this.panelRight.BackColor = System.Drawing.Color.White;
            this.panelRight.Controls.Add(this.lblClose);
            this.panelRight.Controls.Add(this.lblMinimize);
            this.panelRight.Controls.Add(this.lblRightTitle);
            this.panelRight.Controls.Add(this.lblRightSub);
            this.panelRight.Controls.Add(this.txtTenDangNhap);
            this.panelRight.Controls.Add(this.btnLanguage);
            this.panelRight.Controls.Add(this.txtMatKhau);
            this.panelRight.Controls.Add(this.btnShowPassword);
            this.panelRight.Controls.Add(this.chkRememberMe);
            this.panelRight.Controls.Add(this.chkShowPassword);
            this.panelRight.Controls.Add(this.lblThongBao);
            this.panelRight.Controls.Add(this.btnDangNhap);
            this.panelRight.Controls.Add(this.btnThoat);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(435, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(765, 700);
            this.panelRight.TabIndex = 1;
            // 
            // lblClose
            // 
            this.lblClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClose.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblClose.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblClose.Appearance.Options.UseFont = true;
            this.lblClose.Appearance.Options.UseForeColor = true;
            this.lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblClose.Location = new System.Drawing.Point(725, 15);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(17, 28);
            this.lblClose.TabIndex = 0;
            this.lblClose.Text = "✕";
            this.lblClose.Click += new System.EventHandler(this.LblClose_Click);
            this.lblClose.MouseEnter += new System.EventHandler(this.LblClose_MouseEnter);
            this.lblClose.MouseLeave += new System.EventHandler(this.LblClose_MouseLeave);
            // 
            // lblMinimize
            // 
            this.lblMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMinimize.Appearance.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblMinimize.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblMinimize.Appearance.Options.UseFont = true;
            this.lblMinimize.Appearance.Options.UseForeColor = true;
            this.lblMinimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMinimize.Location = new System.Drawing.Point(685, 12);
            this.lblMinimize.Name = "lblMinimize";
            this.lblMinimize.Size = new System.Drawing.Size(23, 31);
            this.lblMinimize.TabIndex = 10;
            this.lblMinimize.Text = "—";
            this.lblMinimize.Click += new System.EventHandler(this.LblMinimize_Click);
            this.lblMinimize.MouseEnter += new System.EventHandler(this.LblClose_MouseEnter);
            this.lblMinimize.MouseLeave += new System.EventHandler(this.LblClose_MouseLeave);
            // 
            // lblRightTitle
            // 
            this.lblRightTitle.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblRightTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblRightTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.lblRightTitle.Appearance.Options.UseFont = true;
            this.lblRightTitle.Appearance.Options.UseForeColor = true;
            this.lblRightTitle.Location = new System.Drawing.Point(157, 120);
            this.lblRightTitle.Name = "lblRightTitle";
            this.lblRightTitle.Size = new System.Drawing.Size(192, 41);
            this.lblRightTitle.TabIndex = 1;
            this.lblRightTitle.Text = "CHÀO MỪNG";
            // 
            // lblRightSub
            // 
            this.lblRightSub.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblRightSub.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblRightSub.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.lblRightSub.Appearance.Options.UseFont = true;
            this.lblRightSub.Appearance.Options.UseForeColor = true;
            this.lblRightSub.Location = new System.Drawing.Point(157, 170);
            this.lblRightSub.Name = "lblRightSub";
            this.lblRightSub.Size = new System.Drawing.Size(314, 23);
            this.lblRightSub.TabIndex = 2;
            this.lblRightSub.Text = "Đăng nhập để tiếp tục truy cập hệ thống";
            // 
            // txtTenDangNhap
            // 
            this.txtTenDangNhap.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtTenDangNhap.Location = new System.Drawing.Point(157, 250);
            this.txtTenDangNhap.Name = "txtTenDangNhap";
            this.txtTenDangNhap.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtTenDangNhap.Properties.Appearance.Options.UseFont = true;
            this.txtTenDangNhap.Properties.AutoHeight = false;
            this.txtTenDangNhap.Properties.NullValuePrompt = "Tên đăng nhập";
            this.txtTenDangNhap.Size = new System.Drawing.Size(479, 50);
            this.txtTenDangNhap.TabIndex = 3;
            // 
            // btnLanguage
            // 
            this.btnLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLanguage.Appearance.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.btnLanguage.Appearance.Options.UseFont = true;
            this.btnLanguage.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnLanguage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLanguage.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnLanguage.ImageOptions.SvgImage")));
            this.btnLanguage.Location = new System.Drawing.Point(722, 657);
            this.btnLanguage.Name = "btnLanguage";
            this.btnLanguage.Size = new System.Drawing.Size(40, 40);
            this.btnLanguage.TabIndex = 9;
            this.btnLanguage.ToolTip = "Ngôn ngữ";
            this.btnLanguage.Click += new System.EventHandler(this.btnLanguage_Click);
            // 
            // txtMatKhau
            // 
            this.txtMatKhau.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtMatKhau.Location = new System.Drawing.Point(157, 320);
            this.txtMatKhau.Name = "txtMatKhau";
            this.txtMatKhau.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtMatKhau.Properties.Appearance.Options.UseFont = true;
            this.txtMatKhau.Properties.AutoHeight = false;
            this.txtMatKhau.Properties.NullValuePrompt = "Mật khẩu";
            this.txtMatKhau.Properties.PasswordChar = '•';
            this.txtMatKhau.Size = new System.Drawing.Size(479, 50);
            this.txtMatKhau.TabIndex = 4;
            this.txtMatKhau.EditValueChanged += new System.EventHandler(this.txtMatKhau_EditValueChanged);
            // 
            // btnShowPassword
            // 
            this.btnShowPassword.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnShowPassword.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnShowPassword.Appearance.Options.UseBackColor = true;
            this.btnShowPassword.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnShowPassword.ImageOptions.SvgImageSize = new System.Drawing.Size(20, 20);
            this.btnShowPassword.Location = new System.Drawing.Point(567, 330);
            this.btnShowPassword.Name = "btnShowPassword";
            this.btnShowPassword.PaintStyle = DevExpress.XtraEditors.Controls.PaintStyles.Light;
            this.btnShowPassword.Size = new System.Drawing.Size(30, 30);
            this.btnShowPassword.TabIndex = 4;
            this.btnShowPassword.Click += new System.EventHandler(this.btnShowPassword_Click);
            // 
            // chkRememberMe
            // 
            this.chkRememberMe.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkRememberMe.Location = new System.Drawing.Point(157, 390);
            this.chkRememberMe.Name = "chkRememberMe";
            this.chkRememberMe.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chkRememberMe.Properties.Appearance.Options.UseFont = true;
            this.chkRememberMe.Properties.Caption = "Ghi nhớ đăng nhập";
            this.chkRememberMe.Size = new System.Drawing.Size(250, 27);
            this.chkRememberMe.TabIndex = 6;
            // 
            // chkShowPassword
            // 
            this.chkShowPassword.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkShowPassword.Location = new System.Drawing.Point(438, 390);
            this.chkShowPassword.Name = "chkShowPassword";
            this.chkShowPassword.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkShowPassword.Properties.Appearance.Options.UseFont = true;
            this.chkShowPassword.Properties.Caption = "Hiển thị mật khẩu";
            this.chkShowPassword.Size = new System.Drawing.Size(198, 24);
            this.chkShowPassword.TabIndex = 6;
            this.chkShowPassword.CheckedChanged += new System.EventHandler(this.ChkShowPassword_CheckedChanged);
            // 
            // lblThongBao
            // 
            this.lblThongBao.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblThongBao.Appearance.ForeColor = System.Drawing.Color.Red;
            this.lblThongBao.Appearance.Options.UseFont = true;
            this.lblThongBao.Appearance.Options.UseForeColor = true;
            this.lblThongBao.Location = new System.Drawing.Point(157, 423);
            this.lblThongBao.Name = "lblThongBao";
            this.lblThongBao.Size = new System.Drawing.Size(0, 20);
            this.lblThongBao.TabIndex = 6;
            this.lblThongBao.Visible = false;
            // 
            // btnDangNhap
            // 
            this.btnDangNhap.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnDangNhap.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(128)))), ((int)(((byte)(185)))));
            this.btnDangNhap.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnDangNhap.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnDangNhap.Appearance.Options.UseBackColor = true;
            this.btnDangNhap.Appearance.Options.UseFont = true;
            this.btnDangNhap.Appearance.Options.UseForeColor = true;
            this.btnDangNhap.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDangNhap.Location = new System.Drawing.Point(157, 470);
            this.btnDangNhap.Name = "btnDangNhap";
            this.btnDangNhap.Size = new System.Drawing.Size(479, 50);
            this.btnDangNhap.TabIndex = 7;
            this.btnDangNhap.Text = "ĐĂNG NHẬP";
            this.btnDangNhap.Click += new System.EventHandler(this.btnDangNhap_Click);
            // 
            // btnThoat
            // 
            this.btnThoat.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnThoat.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.btnThoat.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnThoat.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(140)))), ((int)(((byte)(141)))));
            this.btnThoat.Appearance.Options.UseBackColor = true;
            this.btnThoat.Appearance.Options.UseFont = true;
            this.btnThoat.Appearance.Options.UseForeColor = true;
            this.btnThoat.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnThoat.Location = new System.Drawing.Point(157, 540);
            this.btnThoat.Name = "btnThoat";
            this.btnThoat.Size = new System.Drawing.Size(479, 50);
            this.btnThoat.TabIndex = 8;
            this.btnThoat.Text = "THOÁT";
            this.btnThoat.Click += new System.EventHandler(this.BtnThoat_Click);
            // 
            // FrmDangNhap
            // 
            this.AcceptButton = this.btnDangNhap;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmDangNhap";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hệ thống Đăng nhập";
            this.Load += new System.EventHandler(this.FrmDangNhap_Load);
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            this.panelRight.ResumeLayout(false);
            this.panelRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTenDangNhap.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMatKhau.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRememberMe.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowPassword.Properties)).EndInit();
            this.ResumeLayout(false);

        }
    }
}