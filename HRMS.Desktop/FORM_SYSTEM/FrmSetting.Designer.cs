namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmSetting
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSetting));
            this.gcGeneral = new DevExpress.XtraEditors.GroupControl();
            this.lblNgonNgu = new DevExpress.XtraEditors.LabelControl();
            this.cboLanguage = new DevExpress.XtraEditors.ComboBoxEdit();
            this.gcAdmin = new DevExpress.XtraEditors.GroupControl();
            this.btnManage = new DevExpress.XtraEditors.SimpleButton();
            this.btnOllamaConfig = new DevExpress.XtraEditors.SimpleButton();
            this.btnLuu = new DevExpress.XtraEditors.SimpleButton();
            this.btnDong = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.gcGeneral)).BeginInit();
            this.gcGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLanguage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcAdmin)).BeginInit();
            this.gcAdmin.SuspendLayout();
            this.SuspendLayout();
            // 
            // gcGeneral
            // 
            this.gcGeneral.AppearanceCaption.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.gcGeneral.AppearanceCaption.Options.UseFont = true;
            this.gcGeneral.Controls.Add(this.cboLanguage);
            this.gcGeneral.Controls.Add(this.lblNgonNgu);
            this.gcGeneral.Location = new System.Drawing.Point(20, 20);
            this.gcGeneral.Name = "gcGeneral";
            this.gcGeneral.Size = new System.Drawing.Size(460, 90);
            this.gcGeneral.TabIndex = 0;
            this.gcGeneral.Text = "Cài Đặt Chung";
            // 
            // lblNgonNgu
            // 
            this.lblNgonNgu.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.lblNgonNgu.Appearance.Options.UseFont = true;
            this.lblNgonNgu.Location = new System.Drawing.Point(20, 45);
            this.lblNgonNgu.Name = "lblNgonNgu";
            this.lblNgonNgu.Size = new System.Drawing.Size(155, 23);
            this.lblNgonNgu.TabIndex = 0;
            this.lblNgonNgu.Text = "Ngôn Ngữ Hệ Thống:";
            // 
            // cboLanguage
            // 
            this.cboLanguage.Location = new System.Drawing.Point(190, 42);
            this.cboLanguage.Name = "cboLanguage";
            this.cboLanguage.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.cboLanguage.Properties.Appearance.Options.UseFont = true;
            this.cboLanguage.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboLanguage.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboLanguage.Size = new System.Drawing.Size(250, 30);
            this.cboLanguage.TabIndex = 1;
            // 
            // gcAdmin
            // 
            this.gcAdmin.AppearanceCaption.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.gcAdmin.AppearanceCaption.Options.UseFont = true;
            this.gcAdmin.Controls.Add(this.btnOllamaConfig);
            this.gcAdmin.Controls.Add(this.btnManage);
            this.gcAdmin.Location = new System.Drawing.Point(20, 120);
            this.gcAdmin.Name = "gcAdmin";
            this.gcAdmin.Size = new System.Drawing.Size(460, 100);
            this.gcAdmin.TabIndex = 1;
            this.gcAdmin.Text = "Công Cụ Quản Trị";
            // 
            // btnManage
            // 
            this.btnManage.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnManage.Appearance.Options.UseFont = true;
            this.btnManage.Location = new System.Drawing.Point(20, 45);
            this.btnManage.Name = "btnManage";
            this.btnManage.Size = new System.Drawing.Size(200, 38);
            this.btnManage.TabIndex = 0;
            this.btnManage.Text = "Quản lý CSDL";
            this.btnManage.Click += new System.EventHandler(this.btnManage_Click);
            // 
            // btnOllamaConfig
            // 
            this.btnOllamaConfig.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnOllamaConfig.Appearance.Options.UseFont = true;
            this.btnOllamaConfig.Location = new System.Drawing.Point(240, 45);
            this.btnOllamaConfig.Name = "btnOllamaConfig";
            this.btnOllamaConfig.Size = new System.Drawing.Size(200, 38);
            this.btnOllamaConfig.TabIndex = 1;
            this.btnOllamaConfig.Text = "Cấu hình AI";
            this.btnOllamaConfig.Click += new System.EventHandler(this.btnOllamaConfig_Click);
            // 
            // btnLuu
            // 
            this.btnLuu.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnLuu.Appearance.Options.UseFont = true;
            this.btnLuu.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnLuu.ImageOptions.SvgImage")));
            this.btnLuu.Location = new System.Drawing.Point(220, 240);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(120, 40);
            this.btnLuu.TabIndex = 2;
            this.btnLuu.Text = "Lưu";
            this.btnLuu.Click += new System.EventHandler(this.btnLuu_Click);
            // 
            // btnDong
            // 
            this.btnDong.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnDong.Appearance.Options.UseFont = true;
            this.btnDong.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("btnDong.ImageOptions.SvgImage")));
            this.btnDong.Location = new System.Drawing.Point(360, 240);
            this.btnDong.Name = "btnDong";
            this.btnDong.Size = new System.Drawing.Size(120, 40);
            this.btnDong.TabIndex = 3;
            this.btnDong.Text = "Đóng";
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);
            // 
            // FrmSetting
            // 
            this.AcceptButton = this.btnLuu;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 300);
            this.Controls.Add(this.btnDong);
            this.Controls.Add(this.btnLuu);
            this.Controls.Add(this.gcAdmin);
            this.Controls.Add(this.gcGeneral);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cấu Hình Hệ Thống";
            this.Load += new System.EventHandler(this.FrmSetting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gcGeneral)).EndInit();
            this.gcGeneral.ResumeLayout(false);
            this.gcGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLanguage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcAdmin)).EndInit();
            this.gcAdmin.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.GroupControl gcGeneral;
        private DevExpress.XtraEditors.LabelControl lblNgonNgu;
        private DevExpress.XtraEditors.ComboBoxEdit cboLanguage;
        private DevExpress.XtraEditors.GroupControl gcAdmin;
        private DevExpress.XtraEditors.SimpleButton btnLuu;
        private DevExpress.XtraEditors.SimpleButton btnDong;
        private DevExpress.XtraEditors.SimpleButton btnManage;
        private DevExpress.XtraEditors.SimpleButton btnOllamaConfig;
    }
}
