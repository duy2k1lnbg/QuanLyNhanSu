namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmDatabaseConfig
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
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.lstProfiles = new DevExpress.XtraEditors.ListBoxControl();
            this.btnAdd = new DevExpress.XtraEditors.SimpleButton();
            this.btnEdit = new DevExpress.XtraEditors.SimpleButton();
            this.btnDelete = new DevExpress.XtraEditors.SimpleButton();
            this.btnClose = new DevExpress.XtraEditors.SimpleButton();
            this.btnApply = new DevExpress.XtraEditors.SimpleButton();
            
            this.lblProfileName = new DevExpress.XtraEditors.LabelControl();
            this.txtProfileName = new DevExpress.XtraEditors.TextEdit();
            this.btnColor = new DevExpress.XtraEditors.ColorPickEdit();
            this.lblDatabaseType = new DevExpress.XtraEditors.LabelControl();
            this.cboDbType = new DevExpress.XtraEditors.ComboBoxEdit();
            
            this.xtraTabControl1 = new DevExpress.XtraTab.XtraTabControl();
            this.tabUserInfo = new DevExpress.XtraTab.XtraTabPage();
            this.lblAuthType = new DevExpress.XtraEditors.LabelControl();
            this.cboAuthType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblUsername = new DevExpress.XtraEditors.LabelControl();
            this.txtUsername = new DevExpress.XtraEditors.TextEdit();
            this.lblPassword = new DevExpress.XtraEditors.LabelControl();
            this.txtPassword = new DevExpress.XtraEditors.TextEdit();
            this.lblRole = new DevExpress.XtraEditors.LabelControl();
            this.cboRole = new DevExpress.XtraEditors.ComboBoxEdit();
            this.chkSavePassword = new DevExpress.XtraEditors.CheckEdit();
            
            this.lblConnType = new DevExpress.XtraEditors.LabelControl();
            this.cboConnType = new DevExpress.XtraEditors.ComboBoxEdit();
            
            this.xtraTabControl2 = new DevExpress.XtraTab.XtraTabControl();
            this.tabDetails = new DevExpress.XtraTab.XtraTabPage();
            this.lblHostname = new DevExpress.XtraEditors.LabelControl();
            this.txtHostname = new DevExpress.XtraEditors.TextEdit();
            this.lblPort = new DevExpress.XtraEditors.LabelControl();
            this.txtPort = new DevExpress.XtraEditors.TextEdit();
            this.chkSID = new DevExpress.XtraEditors.CheckEdit();
            this.txtSID = new DevExpress.XtraEditors.TextEdit();
            this.chkServiceName = new DevExpress.XtraEditors.CheckEdit();
            this.txtServiceName = new DevExpress.XtraEditors.TextEdit();
            
            this.panelFooter = new DevExpress.XtraEditors.PanelControl();
            this.lblStatus = new DevExpress.XtraEditors.LabelControl();
            this.btnHelp = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.btnConnect = new DevExpress.XtraEditors.SimpleButton();
            this.btnTestConnection = new DevExpress.XtraEditors.SimpleButton();
            this.btnClear = new DevExpress.XtraEditors.SimpleButton();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            this.splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lstProfiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtProfileName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDbType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnColor.Properties)).BeginInit();
            
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.tabUserInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboAuthType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUsername.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboRole.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSavePassword.Properties)).BeginInit();
            
            ((System.ComponentModel.ISupportInitialize)(this.cboConnType.Properties)).BeginInit();
            
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl2)).BeginInit();
            this.xtraTabControl2.SuspendLayout();
            this.tabDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHostname.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSID.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSID.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkServiceName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtServiceName.Properties)).BeginInit();
            
            ((System.ComponentModel.ISupportInitialize)(this.panelFooter)).BeginInit();
            this.panelFooter.SuspendLayout();
            this.SuspendLayout();

            // splitContainerControl1
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl1.Name = "splitContainerControl1";
            this.splitContainerControl1.Panel1.Controls.Add(this.lstProfiles);
            this.splitContainerControl1.Panel1.Controls.Add(this.btnDelete);
            this.splitContainerControl1.Panel1.Controls.Add(this.btnEdit);
            this.splitContainerControl1.Panel1.Controls.Add(this.btnAdd);
            this.splitContainerControl1.Panel1.Controls.Add(this.btnClose);
            this.splitContainerControl1.Panel1.Controls.Add(this.btnApply);
            this.splitContainerControl1.Panel1.Text = "Panel1";
            this.splitContainerControl1.Panel2.Controls.Add(this.lblProfileName);
            this.splitContainerControl1.Panel2.Controls.Add(this.txtProfileName);
            this.splitContainerControl1.Panel2.Controls.Add(this.btnColor);
            this.splitContainerControl1.Panel2.Controls.Add(this.lblDatabaseType);
            this.splitContainerControl1.Panel2.Controls.Add(this.cboDbType);
            this.splitContainerControl1.Panel2.Controls.Add(this.xtraTabControl1);
            this.splitContainerControl1.Panel2.Controls.Add(this.lblConnType);
            this.splitContainerControl1.Panel2.Controls.Add(this.cboConnType);
            this.splitContainerControl1.Panel2.Controls.Add(this.xtraTabControl2);
            this.splitContainerControl1.Panel2.Padding = new System.Windows.Forms.Padding(10);
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(850, 480);
            this.splitContainerControl1.SplitterPosition = 200;
            this.splitContainerControl1.TabIndex = 0;

            // lstProfiles
            this.lstProfiles.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right);
            this.lstProfiles.Location = new System.Drawing.Point(0, 0);
            this.lstProfiles.Name = "lstProfiles";
            this.lstProfiles.Size = new System.Drawing.Size(200, 390);
            this.lstProfiles.TabIndex = 0;

            // btnAdd
            this.btnAdd.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.btnAdd.Location = new System.Drawing.Point(10, 400);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(55, 30);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";

            // btnEdit
            this.btnEdit.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.btnEdit.Location = new System.Drawing.Point(70, 400);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(55, 30);
            this.btnEdit.TabIndex = 2;
            this.btnEdit.Text = "Edit";

            // btnDelete
            this.btnDelete.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.btnDelete.Location = new System.Drawing.Point(130, 400);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(55, 30);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Delete";
            
            // btnApply
            this.btnApply.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.btnApply.Location = new System.Drawing.Point(10, 440);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(175, 30);
            this.btnApply.TabIndex = 4;
            this.btnApply.Text = "Apply / Connect";

            // btnClose
            this.btnClose.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            this.btnClose.Location = new System.Drawing.Point(10, 440);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(175, 30);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";

            // lblProfileName
            this.lblProfileName.Location = new System.Drawing.Point(15, 15);
            this.lblProfileName.Name = "lblProfileName";
            this.lblProfileName.Size = new System.Drawing.Size(31, 16);
            this.lblProfileName.TabIndex = 0;
            this.lblProfileName.Text = "Name";

            // txtProfileName
            this.txtProfileName.Location = new System.Drawing.Point(120, 12);
            this.txtProfileName.Name = "txtProfileName";
            this.txtProfileName.Size = new System.Drawing.Size(400, 22);
            this.txtProfileName.TabIndex = 1;
            
            // btnColor
            this.btnColor.Location = new System.Drawing.Point(530, 12);
            this.btnColor.Name = "btnColor";
            this.btnColor.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.btnColor.Size = new System.Drawing.Size(80, 22);
            this.btnColor.TabIndex = 2;

            // lblDatabaseType
            this.lblDatabaseType.Location = new System.Drawing.Point(15, 45);
            this.lblDatabaseType.Name = "lblDatabaseType";
            this.lblDatabaseType.Size = new System.Drawing.Size(85, 16);
            this.lblDatabaseType.TabIndex = 3;
            this.lblDatabaseType.Text = "Database Type";

            // cboDbType
            this.cboDbType.Location = new System.Drawing.Point(120, 42);
            this.cboDbType.Name = "cboDbType";
            this.cboDbType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboDbType.Properties.Items.AddRange(new object[] { "Oracle" });
            this.cboDbType.Size = new System.Drawing.Size(120, 22);
            this.cboDbType.TabIndex = 4;

            // xtraTabControl1
            this.xtraTabControl1.Location = new System.Drawing.Point(15, 75);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.SelectedTabPage = this.tabUserInfo;
            this.xtraTabControl1.Size = new System.Drawing.Size(600, 145);
            this.xtraTabControl1.TabIndex = 5;
            this.xtraTabControl1.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { this.tabUserInfo });

            // tabUserInfo
            this.tabUserInfo.Controls.Add(this.lblAuthType);
            this.tabUserInfo.Controls.Add(this.cboAuthType);
            this.tabUserInfo.Controls.Add(this.lblUsername);
            this.tabUserInfo.Controls.Add(this.txtUsername);
            this.tabUserInfo.Controls.Add(this.lblPassword);
            this.tabUserInfo.Controls.Add(this.txtPassword);
            this.tabUserInfo.Controls.Add(this.lblRole);
            this.tabUserInfo.Controls.Add(this.cboRole);
            this.tabUserInfo.Controls.Add(this.chkSavePassword);
            this.tabUserInfo.Name = "tabUserInfo";
            this.tabUserInfo.Size = new System.Drawing.Size(598, 115);
            this.tabUserInfo.Text = "User Info";

            // lblAuthType
            this.lblAuthType.Location = new System.Drawing.Point(15, 15);
            this.lblAuthType.Name = "lblAuthType";
            this.lblAuthType.Size = new System.Drawing.Size(113, 16);
            this.lblAuthType.TabIndex = 0;
            this.lblAuthType.Text = "Authentication Type";

            // cboAuthType
            this.cboAuthType.Location = new System.Drawing.Point(150, 12);
            this.cboAuthType.Name = "cboAuthType";
            this.cboAuthType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboAuthType.Properties.Items.AddRange(new object[] { "Default", "OS" });
            this.cboAuthType.Size = new System.Drawing.Size(120, 22);
            this.cboAuthType.TabIndex = 1;

            // lblUsername
            this.lblUsername.Location = new System.Drawing.Point(15, 45);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(58, 16);
            this.lblUsername.TabIndex = 2;
            this.lblUsername.Text = "Username";

            // txtUsername
            this.txtUsername.Location = new System.Drawing.Point(150, 42);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(200, 22);
            this.txtUsername.TabIndex = 3;

            // lblRole
            this.lblRole.Location = new System.Drawing.Point(370, 45);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new System.Drawing.Size(25, 16);
            this.lblRole.TabIndex = 4;
            this.lblRole.Text = "Role";

            // cboRole
            this.cboRole.Location = new System.Drawing.Point(410, 42);
            this.cboRole.Name = "cboRole";
            this.cboRole.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboRole.Properties.Items.AddRange(new object[] { "default", "SYSDBA", "SYSOPER" });
            this.cboRole.Size = new System.Drawing.Size(100, 22);
            this.cboRole.TabIndex = 5;

            // lblPassword
            this.lblPassword.Location = new System.Drawing.Point(15, 75);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(55, 16);
            this.lblPassword.TabIndex = 6;
            this.lblPassword.Text = "Password";

            // txtPassword
            this.txtPassword.Location = new System.Drawing.Point(150, 72);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Properties.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(200, 22);
            this.txtPassword.TabIndex = 7;

            // chkSavePassword
            this.chkSavePassword.Location = new System.Drawing.Point(370, 72);
            this.chkSavePassword.Name = "chkSavePassword";
            this.chkSavePassword.Properties.Caption = "Save Password";
            this.chkSavePassword.Size = new System.Drawing.Size(120, 20);
            this.chkSavePassword.TabIndex = 8;
            
            // lblConnType
            this.lblConnType.Location = new System.Drawing.Point(15, 235);
            this.lblConnType.Name = "lblConnType";
            this.lblConnType.Size = new System.Drawing.Size(95, 16);
            this.lblConnType.TabIndex = 6;
            this.lblConnType.Text = "Connection Type";

            // cboConnType
            this.cboConnType.Location = new System.Drawing.Point(120, 232);
            this.cboConnType.Name = "cboConnType";
            this.cboConnType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboConnType.Properties.Items.AddRange(new object[] { "Basic" });
            this.cboConnType.Size = new System.Drawing.Size(120, 22);
            this.cboConnType.TabIndex = 7;

            // xtraTabControl2
            this.xtraTabControl2.Location = new System.Drawing.Point(15, 265);
            this.xtraTabControl2.Name = "xtraTabControl2";
            this.xtraTabControl2.SelectedTabPage = this.tabDetails;
            this.xtraTabControl2.Size = new System.Drawing.Size(600, 185);
            this.xtraTabControl2.TabIndex = 8;
            this.xtraTabControl2.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] { this.tabDetails });

            // tabDetails
            this.tabDetails.Controls.Add(this.lblHostname);
            this.tabDetails.Controls.Add(this.txtHostname);
            this.tabDetails.Controls.Add(this.lblPort);
            this.tabDetails.Controls.Add(this.txtPort);
            this.tabDetails.Controls.Add(this.chkSID);
            this.tabDetails.Controls.Add(this.txtSID);
            this.tabDetails.Controls.Add(this.chkServiceName);
            this.tabDetails.Controls.Add(this.txtServiceName);
            this.tabDetails.Name = "tabDetails";
            this.tabDetails.Size = new System.Drawing.Size(598, 155);
            this.tabDetails.Text = "Details";

            // lblHostname
            this.lblHostname.Location = new System.Drawing.Point(15, 15);
            this.lblHostname.Name = "lblHostname";
            this.lblHostname.Size = new System.Drawing.Size(57, 16);
            this.lblHostname.TabIndex = 0;
            this.lblHostname.Text = "Hostname";

            // txtHostname
            this.txtHostname.Location = new System.Drawing.Point(150, 12);
            this.txtHostname.Name = "txtHostname";
            this.txtHostname.Size = new System.Drawing.Size(300, 22);
            this.txtHostname.TabIndex = 1;

            // lblPort
            this.lblPort.Location = new System.Drawing.Point(15, 45);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(23, 16);
            this.lblPort.TabIndex = 2;
            this.lblPort.Text = "Port";

            // txtPort
            this.txtPort.Location = new System.Drawing.Point(150, 42);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(300, 22);
            this.txtPort.TabIndex = 3;

            // chkSID
            this.chkSID.Location = new System.Drawing.Point(15, 75);
            this.chkSID.Name = "chkSID";
            this.chkSID.Properties.Caption = "SID";
            this.chkSID.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.chkSID.Properties.RadioGroupIndex = 1;
            this.chkSID.Size = new System.Drawing.Size(120, 20);
            this.chkSID.TabIndex = 4;
            
            // txtSID
            this.txtSID.Location = new System.Drawing.Point(150, 75);
            this.txtSID.Name = "txtSID";
            this.txtSID.Size = new System.Drawing.Size(300, 22);
            this.txtSID.TabIndex = 5;

            // chkServiceName
            this.chkServiceName.Location = new System.Drawing.Point(15, 105);
            this.chkServiceName.Name = "chkServiceName";
            this.chkServiceName.Properties.Caption = "Service name";
            this.chkServiceName.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.chkServiceName.Properties.RadioGroupIndex = 1;
            this.chkServiceName.Size = new System.Drawing.Size(120, 20);
            this.chkServiceName.TabIndex = 6;

            // txtServiceName
            this.txtServiceName.Location = new System.Drawing.Point(150, 105);
            this.txtServiceName.Name = "txtServiceName";
            this.txtServiceName.Size = new System.Drawing.Size(300, 22);
            this.txtServiceName.TabIndex = 7;

            // panelFooter
            this.panelFooter.Controls.Add(this.lblStatus);
            this.panelFooter.Controls.Add(this.btnHelp);
            this.panelFooter.Controls.Add(this.btnCancel);
            this.panelFooter.Controls.Add(this.btnConnect);
            this.panelFooter.Controls.Add(this.btnTestConnection);
            this.panelFooter.Controls.Add(this.btnClear);
            this.panelFooter.Controls.Add(this.btnSave);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelFooter.Location = new System.Drawing.Point(0, 480);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.Size = new System.Drawing.Size(850, 60);
            this.panelFooter.TabIndex = 1;

            // lblStatus
            this.lblStatus.Location = new System.Drawing.Point(100, 22);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(45, 16);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Status :";

            // btnHelp
            this.btnHelp.Location = new System.Drawing.Point(10, 15);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(75, 30);
            this.btnHelp.TabIndex = 1;
            this.btnHelp.Text = "Help";

            // btnClear
            this.btnClear.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            this.btnClear.Location = new System.Drawing.Point(370, 15);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 30);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear";

            // btnTestConnection
            this.btnTestConnection.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            this.btnTestConnection.Location = new System.Drawing.Point(455, 15);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(120, 30);
            this.btnTestConnection.TabIndex = 3;
            this.btnTestConnection.Text = "Test";

            // btnSave
            this.btnSave.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            this.btnSave.Location = new System.Drawing.Point(585, 15);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";

            // btnCancel
            this.btnCancel.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            this.btnCancel.Location = new System.Drawing.Point(670, 15);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";

            // btnConnect
            this.btnConnect.Anchor = (System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            this.btnConnect.Location = new System.Drawing.Point(755, 15);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 30);
            this.btnConnect.TabIndex = 6;
            this.btnConnect.Text = "Connect";

            // FrmDatabaseConfig
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 540);
            this.Controls.Add(this.splitContainerControl1);
            this.Controls.Add(this.panelFooter);
            this.Name = "FrmDatabaseConfig";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New / Select Database Connection";

            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lstProfiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtProfileName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDbType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnColor.Properties)).EndInit();
            
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.xtraTabControl1.ResumeLayout(false);
            this.tabUserInfo.ResumeLayout(false);
            this.tabUserInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboAuthType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUsername.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboRole.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSavePassword.Properties)).EndInit();
            
            ((System.ComponentModel.ISupportInitialize)(this.cboConnType.Properties)).EndInit();
            
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl2)).EndInit();
            this.xtraTabControl2.ResumeLayout(false);
            this.tabDetails.ResumeLayout(false);
            this.tabDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtHostname.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSID.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSID.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkServiceName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtServiceName.Properties)).EndInit();
            
            ((System.ComponentModel.ISupportInitialize)(this.panelFooter)).EndInit();
            this.panelFooter.ResumeLayout(false);
            this.panelFooter.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private DevExpress.XtraEditors.ListBoxControl lstProfiles;
        
        private DevExpress.XtraEditors.LabelControl lblProfileName;
        private DevExpress.XtraEditors.TextEdit txtProfileName;
        private DevExpress.XtraEditors.LabelControl lblDatabaseType;
        private DevExpress.XtraEditors.ComboBoxEdit cboDbType;
        private DevExpress.XtraEditors.ColorPickEdit btnColor;

        private DevExpress.XtraTab.XtraTabControl xtraTabControl1;
        private DevExpress.XtraTab.XtraTabPage tabUserInfo;
        private DevExpress.XtraEditors.LabelControl lblAuthType;
        private DevExpress.XtraEditors.ComboBoxEdit cboAuthType;
        private DevExpress.XtraEditors.LabelControl lblUsername;
        private DevExpress.XtraEditors.TextEdit txtUsername;
        private DevExpress.XtraEditors.LabelControl lblPassword;
        private DevExpress.XtraEditors.TextEdit txtPassword;
        private DevExpress.XtraEditors.LabelControl lblRole;
        private DevExpress.XtraEditors.ComboBoxEdit cboRole;
        private DevExpress.XtraEditors.CheckEdit chkSavePassword;

        private DevExpress.XtraEditors.LabelControl lblConnType;
        private DevExpress.XtraEditors.ComboBoxEdit cboConnType;
        
        private DevExpress.XtraTab.XtraTabControl xtraTabControl2;
        private DevExpress.XtraTab.XtraTabPage tabDetails;
        private DevExpress.XtraEditors.LabelControl lblHostname;
        private DevExpress.XtraEditors.TextEdit txtHostname;
        private DevExpress.XtraEditors.LabelControl lblPort;
        private DevExpress.XtraEditors.TextEdit txtPort;
        private DevExpress.XtraEditors.CheckEdit chkSID;
        private DevExpress.XtraEditors.TextEdit txtSID;
        private DevExpress.XtraEditors.CheckEdit chkServiceName;
        private DevExpress.XtraEditors.TextEdit txtServiceName;

        private DevExpress.XtraEditors.PanelControl panelFooter;
        private DevExpress.XtraEditors.LabelControl lblStatus;
        
        private DevExpress.XtraEditors.SimpleButton btnAdd;
        private DevExpress.XtraEditors.SimpleButton btnEdit;
        private DevExpress.XtraEditors.SimpleButton btnDelete;
        
        private DevExpress.XtraEditors.SimpleButton btnClose;
        private DevExpress.XtraEditors.SimpleButton btnApply;
        private DevExpress.XtraEditors.SimpleButton btnHelp;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnClear;
        private DevExpress.XtraEditors.SimpleButton btnTestConnection;
        private DevExpress.XtraEditors.SimpleButton btnConnect;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
    }
}
