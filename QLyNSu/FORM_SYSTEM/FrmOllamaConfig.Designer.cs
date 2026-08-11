namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmOllamaConfig
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

        private void InitializeComponent()
        {
            this.gcMain = new DevExpress.XtraEditors.GroupControl();
            this.lblUrl = new DevExpress.XtraEditors.LabelControl();
            this.txtUrl = new DevExpress.XtraEditors.TextEdit();
            this.lblModel = new DevExpress.XtraEditors.LabelControl();
            this.txtModel = new DevExpress.XtraEditors.TextEdit();
            this.lblQdrant = new DevExpress.XtraEditors.LabelControl();
            this.txtQdrant = new DevExpress.XtraEditors.TextEdit();
            this.btnTest = new DevExpress.XtraEditors.SimpleButton();
            this.gcQdrant = new DevExpress.XtraEditors.GroupControl();
            this.btnTestQdrant = new DevExpress.XtraEditors.SimpleButton();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.gcMain)).BeginInit();
            this.gcMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcQdrant)).BeginInit();
            this.gcQdrant.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtUrl.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtModel.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQdrant.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // gcMain
            // 
            this.gcMain.AppearanceCaption.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.gcMain.AppearanceCaption.Options.UseFont = true;
            this.gcMain.Controls.Add(this.btnTest);
            this.gcMain.Controls.Add(this.txtModel);
            this.gcMain.Controls.Add(this.lblModel);
            this.gcMain.Controls.Add(this.txtUrl);
            this.gcMain.Controls.Add(this.lblUrl);
            this.gcMain.Location = new System.Drawing.Point(20, 20);
            this.gcMain.Name = "gcMain";
            this.gcMain.Size = new System.Drawing.Size(550, 180);
            this.gcMain.TabIndex = 0;
            this.gcMain.Text = "Cấu Hình Kết Nối Ollama";
            // 
            // lblUrl
            // 
            this.lblUrl.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.lblUrl.Appearance.Options.UseFont = true;
            this.lblUrl.Location = new System.Drawing.Point(30, 50);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(120, 23);
            this.lblUrl.TabIndex = 0;
            this.lblUrl.Text = "Ollama Host URL:";
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(170, 47);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.txtUrl.Properties.Appearance.Options.UseFont = true;
            this.txtUrl.Size = new System.Drawing.Size(350, 30);
            this.txtUrl.TabIndex = 1;
            // 
            // lblModel
            // 
            this.lblModel.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.lblModel.Appearance.Options.UseFont = true;
            this.lblModel.Location = new System.Drawing.Point(30, 95);
            this.lblModel.Name = "lblModel";
            this.lblModel.Size = new System.Drawing.Size(110, 23);
            this.lblModel.TabIndex = 2;
            this.lblModel.Text = "Model Name:";
            // 
            // txtModel
            // 
            this.txtModel.Location = new System.Drawing.Point(170, 92);
            this.txtModel.Name = "txtModel";
            this.txtModel.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.txtModel.Properties.Appearance.Options.UseFont = true;
            this.txtModel.Size = new System.Drawing.Size(350, 30);
            this.txtModel.TabIndex = 3;
            // 
            // 
            // gcQdrant
            // 
            this.gcQdrant.AppearanceCaption.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.gcQdrant.AppearanceCaption.Options.UseFont = true;
            this.gcQdrant.Controls.Add(this.btnTestQdrant);
            this.gcQdrant.Controls.Add(this.txtQdrant);
            this.gcQdrant.Controls.Add(this.lblQdrant);
            this.gcQdrant.Location = new System.Drawing.Point(20, 210);
            this.gcQdrant.Name = "gcQdrant";
            this.gcQdrant.Size = new System.Drawing.Size(550, 140);
            this.gcQdrant.TabIndex = 8;
            this.gcQdrant.Text = "Cấu Hình Kết Nối Qdrant";
            // 
            // lblQdrant
            // 
            this.lblQdrant.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.lblQdrant.Appearance.Options.UseFont = true;
            this.lblQdrant.Location = new System.Drawing.Point(30, 50);
            this.lblQdrant.Name = "lblQdrant";
            this.lblQdrant.Size = new System.Drawing.Size(120, 23);
            this.lblQdrant.TabIndex = 5;
            this.lblQdrant.Text = "Qdrant URL:";
            // 
            // txtQdrant
            // 
            this.txtQdrant.Location = new System.Drawing.Point(170, 47);
            this.txtQdrant.Name = "txtQdrant";
            this.txtQdrant.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F);
            this.txtQdrant.Properties.Appearance.Options.UseFont = true;
            this.txtQdrant.Size = new System.Drawing.Size(350, 30);
            this.txtQdrant.TabIndex = 6;
            // 
            // btnTestQdrant
            // 
            this.btnTestQdrant.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnTestQdrant.Appearance.Options.UseFont = true;
            this.btnTestQdrant.Location = new System.Drawing.Point(170, 95);
            this.btnTestQdrant.Name = "btnTestQdrant";
            this.btnTestQdrant.Size = new System.Drawing.Size(140, 30);
            this.btnTestQdrant.TabIndex = 9;
            this.btnTestQdrant.Text = "Kiểm Tra Kết Nối";
            this.btnTestQdrant.Click += new System.EventHandler(this.btnTestQdrant_Click);
            // 
            // btnTest
            // 
            this.btnTest.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnTest.Appearance.Options.UseFont = true;
            this.btnTest.Location = new System.Drawing.Point(170, 135);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(140, 30);
            this.btnTest.TabIndex = 7;
            this.btnTest.Text = "Kiểm Tra Kết Nối";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnSave
            // 
            this.btnSave.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnSave.Appearance.Options.UseFont = true;
            this.btnSave.Location = new System.Drawing.Point(300, 370);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(120, 40);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Lưu Thiết Lập";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Appearance.Options.UseFont = true;
            this.btnCancel.Location = new System.Drawing.Point(440, 370);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 40);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Đóng";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FrmOllamaConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 430);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.gcQdrant);
            this.Controls.Add(this.gcMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmOllamaConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cấu Hình AI Server";
            this.Load += new System.EventHandler(this.FrmOllamaConfig_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gcMain)).EndInit();
            this.gcMain.ResumeLayout(false);
            this.gcMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcQdrant)).EndInit();
            this.gcQdrant.ResumeLayout(false);
            this.gcQdrant.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtUrl.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtModel.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQdrant.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        private DevExpress.XtraEditors.GroupControl gcMain;
        private DevExpress.XtraEditors.GroupControl gcQdrant;
        private DevExpress.XtraEditors.LabelControl lblUrl;
        private DevExpress.XtraEditors.TextEdit txtUrl;
        private DevExpress.XtraEditors.LabelControl lblModel;
        private DevExpress.XtraEditors.TextEdit txtModel;
        private DevExpress.XtraEditors.LabelControl lblQdrant;
        private DevExpress.XtraEditors.TextEdit txtQdrant;
        private DevExpress.XtraEditors.SimpleButton btnTest;
        private DevExpress.XtraEditors.SimpleButton btnTestQdrant;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
    }
}
