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
            this.btnHelp = new DevExpress.XtraEditors.SimpleButton();
                        this.gcAdvanced = new DevExpress.XtraEditors.GroupControl();
            this.lblTemp = new DevExpress.XtraEditors.LabelControl();
            this.txtTemp = new DevExpress.XtraEditors.TextEdit();
            this.lblMaxTokens = new DevExpress.XtraEditors.LabelControl();
            this.txtMaxTokens = new DevExpress.XtraEditors.TextEdit();
            this.lblCtx = new DevExpress.XtraEditors.LabelControl();
            this.txtCtx = new DevExpress.XtraEditors.TextEdit();
            this.lblTopK = new DevExpress.XtraEditors.LabelControl();
            this.txtTopK = new DevExpress.XtraEditors.TextEdit();
            this.lblTopP = new DevExpress.XtraEditors.LabelControl();
            this.txtTopP = new DevExpress.XtraEditors.TextEdit();
            this.lblRepeat = new DevExpress.XtraEditors.LabelControl();
            this.txtRepeat = new DevExpress.XtraEditors.TextEdit();
            this.gcQdrant = new DevExpress.XtraEditors.GroupControl();
            this.btnTestQdrant = new DevExpress.XtraEditors.SimpleButton();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.gcMain)).BeginInit();
            this.gcMain.SuspendLayout();
                        ((System.ComponentModel.ISupportInitialize)(this.gcAdvanced)).BeginInit();
            this.gcAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTemp.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaxTokens.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCtx.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTopK.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTopP.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRepeat.Properties)).BeginInit();
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
            
            // gcAdvanced
            this.gcAdvanced.AppearanceCaption.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.gcAdvanced.AppearanceCaption.Options.UseFont = true;
            this.gcAdvanced.Controls.Add(this.lblTemp);
            this.gcAdvanced.Controls.Add(this.txtTemp);
            this.gcAdvanced.Controls.Add(this.lblMaxTokens);
            this.gcAdvanced.Controls.Add(this.txtMaxTokens);
            this.gcAdvanced.Controls.Add(this.lblCtx);
            this.gcAdvanced.Controls.Add(this.txtCtx);
            this.gcAdvanced.Controls.Add(this.lblTopK);
            this.gcAdvanced.Controls.Add(this.txtTopK);
            this.gcAdvanced.Controls.Add(this.lblTopP);
            this.gcAdvanced.Controls.Add(this.txtTopP);
            this.gcAdvanced.Controls.Add(this.lblRepeat);
            this.gcAdvanced.Controls.Add(this.txtRepeat);
            this.gcAdvanced.Location = new System.Drawing.Point(20, 210);
            this.gcAdvanced.Name = "gcAdvanced";
            this.gcAdvanced.Size = new System.Drawing.Size(550, 190);
            this.gcAdvanced.TabIndex = 4;
            this.gcAdvanced.Text = "Tham Số Cấu Hình Nâng Cao (AI Thinking)";
            // lblTemp
            this.lblTemp.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTemp.Appearance.Options.UseFont = true;
            this.lblTemp.Location = new System.Drawing.Point(30, 45);
            this.lblTemp.Name = "lblTemp";
            this.lblTemp.Size = new System.Drawing.Size(120, 20);
            this.lblTemp.Text = "Chat Temperature:";
            // txtTemp
            this.txtTemp.Location = new System.Drawing.Point(170, 42);
            this.txtTemp.Name = "txtTemp";
            this.txtTemp.Size = new System.Drawing.Size(100, 26);
            // lblMaxTokens
            this.lblMaxTokens.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMaxTokens.Appearance.Options.UseFont = true;
            this.lblMaxTokens.Location = new System.Drawing.Point(300, 45);
            this.lblMaxTokens.Name = "lblMaxTokens";
            this.lblMaxTokens.Size = new System.Drawing.Size(100, 20);
            this.lblMaxTokens.Text = "Max Tokens:";
            // txtMaxTokens
            this.txtMaxTokens.Location = new System.Drawing.Point(420, 42);
            this.txtMaxTokens.Name = "txtMaxTokens";
            this.txtMaxTokens.Size = new System.Drawing.Size(100, 26);
            // lblCtx
            this.lblCtx.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCtx.Appearance.Options.UseFont = true;
            this.lblCtx.Location = new System.Drawing.Point(30, 95);
            this.lblCtx.Name = "lblCtx";
            this.lblCtx.Size = new System.Drawing.Size(120, 20);
            this.lblCtx.Text = "Context Window:";
            // txtCtx
            this.txtCtx.Location = new System.Drawing.Point(170, 92);
            this.txtCtx.Name = "txtCtx";
            this.txtCtx.Size = new System.Drawing.Size(100, 26);
            // lblTopK
            this.lblTopK.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTopK.Appearance.Options.UseFont = true;
            this.lblTopK.Location = new System.Drawing.Point(300, 95);
            this.lblTopK.Name = "lblTopK";
            this.lblTopK.Size = new System.Drawing.Size(100, 20);
            this.lblTopK.Text = "Top K:";
            // txtTopK
            this.txtTopK.Location = new System.Drawing.Point(420, 92);
            this.txtTopK.Name = "txtTopK";
            this.txtTopK.Size = new System.Drawing.Size(100, 26);
            // lblTopP
            this.lblTopP.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTopP.Appearance.Options.UseFont = true;
            this.lblTopP.Location = new System.Drawing.Point(30, 145);
            this.lblTopP.Name = "lblTopP";
            this.lblTopP.Size = new System.Drawing.Size(120, 20);
            this.lblTopP.Text = "Top P:";
            // txtTopP
            this.txtTopP.Location = new System.Drawing.Point(170, 142);
            this.txtTopP.Name = "txtTopP";
            this.txtTopP.Size = new System.Drawing.Size(100, 26);
            // lblRepeat
            this.lblRepeat.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRepeat.Appearance.Options.UseFont = true;
            this.lblRepeat.Location = new System.Drawing.Point(300, 145);
            this.lblRepeat.Name = "lblRepeat";
            this.lblRepeat.Size = new System.Drawing.Size(110, 20);
            this.lblRepeat.Text = "Repeat Penalty:";
            // txtRepeat
            this.txtRepeat.Location = new System.Drawing.Point(420, 142);
            this.txtRepeat.Name = "txtRepeat";
            this.txtRepeat.Size = new System.Drawing.Size(100, 26);

            // gcQdrant
            // 
            this.gcQdrant.AppearanceCaption.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.gcQdrant.AppearanceCaption.Options.UseFont = true;
            this.gcQdrant.Controls.Add(this.btnTestQdrant);
            this.gcQdrant.Controls.Add(this.txtQdrant);
            this.gcQdrant.Controls.Add(this.lblQdrant);
            this.gcQdrant.Location = new System.Drawing.Point(20, 420);
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
            // btnHelp
            // 
            this.btnHelp.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnHelp.Appearance.Options.UseFont = true;
            this.btnHelp.Location = new System.Drawing.Point(30, 580);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(120, 40);
            this.btnHelp.TabIndex = 3;
            this.btnHelp.Text = "Hướng Dẫn";
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // btnSave
            // 
            this.btnSave.Appearance.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            this.btnSave.Appearance.Options.UseFont = true;
            this.btnSave.Location = new System.Drawing.Point(300, 580);
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
            this.btnCancel.Location = new System.Drawing.Point(440, 580);
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
            this.ClientSize = new System.Drawing.Size(590, 640);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.gcAdvanced);
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
                        ((System.ComponentModel.ISupportInitialize)(this.gcAdvanced)).EndInit();
            this.gcAdvanced.ResumeLayout(false);
            this.gcAdvanced.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcQdrant)).EndInit();
            this.gcQdrant.ResumeLayout(false);
            this.gcQdrant.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtUrl.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtModel.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQdrant.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTemp.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaxTokens.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCtx.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTopK.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtTopP.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRepeat.Properties)).EndInit();
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
        private DevExpress.XtraEditors.SimpleButton btnHelp;
        private DevExpress.XtraEditors.GroupControl gcAdvanced;
        private DevExpress.XtraEditors.LabelControl lblTemp;
        private DevExpress.XtraEditors.TextEdit txtTemp;
        private DevExpress.XtraEditors.LabelControl lblMaxTokens;
        private DevExpress.XtraEditors.TextEdit txtMaxTokens;
        private DevExpress.XtraEditors.LabelControl lblCtx;
        private DevExpress.XtraEditors.TextEdit txtCtx;
        private DevExpress.XtraEditors.LabelControl lblTopK;
        private DevExpress.XtraEditors.TextEdit txtTopK;
        private DevExpress.XtraEditors.LabelControl lblTopP;
        private DevExpress.XtraEditors.TextEdit txtTopP;
        private DevExpress.XtraEditors.LabelControl lblRepeat;
        private DevExpress.XtraEditors.TextEdit txtRepeat;

    }
}
