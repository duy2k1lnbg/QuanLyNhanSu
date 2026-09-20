namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmAI_Chat
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
            this.flpChat = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlQuickActions = new DevExpress.XtraEditors.PanelControl();
            this.btnActionBirthday = new DevExpress.XtraEditors.SimpleButton();
            this.btnActionSalary = new DevExpress.XtraEditors.SimpleButton();
            this.btnActionEmployee = new DevExpress.XtraEditors.SimpleButton();
            this.btnActionDepartment = new DevExpress.XtraEditors.SimpleButton();
            this.pnlChatInput = new DevExpress.XtraEditors.PanelControl();
            this.txtChatInput = new DevExpress.XtraEditors.TextEdit();
            this.btnChatSend = new DevExpress.XtraEditors.SimpleButton();
            this.pnlChatHeader = new DevExpress.XtraEditors.PanelControl();
            this.btnOpenDashboard = new DevExpress.XtraEditors.SimpleButton();
            this.btnClearChat = new DevExpress.XtraEditors.SimpleButton();
            this.lblChatTitle = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.pnlQuickActions)).BeginInit();
            this.pnlQuickActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlChatInput)).BeginInit();
            this.pnlChatInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtChatInput.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlChatHeader)).BeginInit();
            this.pnlChatHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // flpChat
            // 
            this.flpChat.AutoScroll = true;
            this.flpChat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.flpChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpChat.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpChat.Location = new System.Drawing.Point(0, 62);
            this.flpChat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.flpChat.Name = "flpChat";
            this.flpChat.Padding = new System.Windows.Forms.Padding(12, 12, 12, 12);
            this.flpChat.Size = new System.Drawing.Size(700, 677);
            this.flpChat.TabIndex = 1;
            this.flpChat.WrapContents = false;
            this.flpChat.SizeChanged += new System.EventHandler(this.flpChat_SizeChanged);
            // 
            // pnlQuickActions
            // 
            this.pnlQuickActions.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlQuickActions.Controls.Add(this.btnActionBirthday);
            this.pnlQuickActions.Controls.Add(this.btnActionSalary);
            this.pnlQuickActions.Controls.Add(this.btnActionEmployee);
            this.pnlQuickActions.Controls.Add(this.btnActionDepartment);
            this.pnlQuickActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlQuickActions.Location = new System.Drawing.Point(0, 739);
            this.pnlQuickActions.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlQuickActions.Name = "pnlQuickActions";
            this.pnlQuickActions.Size = new System.Drawing.Size(700, 49);
            this.pnlQuickActions.TabIndex = 2;
            // 
            // btnActionBirthday
            // 
            this.btnActionBirthday.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.25F);
            this.btnActionBirthday.Appearance.Options.UseFont = true;
            this.btnActionBirthday.Location = new System.Drawing.Point(14, 7);
            this.btnActionBirthday.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnActionBirthday.Name = "btnActionBirthday";
            this.btnActionBirthday.Size = new System.Drawing.Size(140, 34);
            this.btnActionBirthday.TabIndex = 0;
            this.btnActionBirthday.Text = "📝 Hết hạn HĐ";
            this.btnActionBirthday.Click += new System.EventHandler(this.QuickAction_Click);
            // 
            // btnActionSalary
            // 
            this.btnActionSalary.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.25F);
            this.btnActionSalary.Appearance.Options.UseFont = true;
            this.btnActionSalary.Location = new System.Drawing.Point(162, 7);
            this.btnActionSalary.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnActionSalary.Name = "btnActionSalary";
            this.btnActionSalary.Size = new System.Drawing.Size(117, 34);
            this.btnActionSalary.TabIndex = 1;
            this.btnActionSalary.Text = "📈 Lên lương";
            this.btnActionSalary.Click += new System.EventHandler(this.QuickAction_Click);
            // 
            // btnActionEmployee
            // 
            this.btnActionEmployee.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.25F);
            this.btnActionEmployee.Appearance.Options.UseFont = true;
            this.btnActionEmployee.Location = new System.Drawing.Point(287, 8);
            this.btnActionEmployee.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnActionEmployee.Name = "btnActionEmployee";
            this.btnActionEmployee.Size = new System.Drawing.Size(117, 34);
            this.btnActionEmployee.TabIndex = 2;
            this.btnActionEmployee.Text = "👥 Nhân viên";
            this.btnActionEmployee.Click += new System.EventHandler(this.QuickAction_Click);
            // 
            // btnActionDepartment
            // 
            this.btnActionDepartment.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.25F);
            this.btnActionDepartment.Appearance.Options.UseFont = true;
            this.btnActionDepartment.Location = new System.Drawing.Point(412, 8);
            this.btnActionDepartment.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnActionDepartment.Name = "btnActionDepartment";
            this.btnActionDepartment.Size = new System.Drawing.Size(117, 34);
            this.btnActionDepartment.TabIndex = 3;
            this.btnActionDepartment.Text = "🏢 Phòng ban";
            this.btnActionDepartment.Click += new System.EventHandler(this.QuickAction_Click);
            // 
            // pnlChatInput
            // 
            this.pnlChatInput.Controls.Add(this.txtChatInput);
            this.pnlChatInput.Controls.Add(this.btnChatSend);
            this.pnlChatInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlChatInput.Location = new System.Drawing.Point(0, 788);
            this.pnlChatInput.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlChatInput.Name = "pnlChatInput";
            this.pnlChatInput.Size = new System.Drawing.Size(700, 74);
            this.pnlChatInput.TabIndex = 3;
            // 
            // txtChatInput
            // 
            this.txtChatInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtChatInput.Location = new System.Drawing.Point(14, 17);
            this.txtChatInput.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtChatInput.Name = "txtChatInput";
            this.txtChatInput.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtChatInput.Properties.Appearance.Options.UseFont = true;
            this.txtChatInput.Size = new System.Drawing.Size(548, 32);
            this.txtChatInput.TabIndex = 0;
            this.txtChatInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtChatInput_KeyDown);
            // 
            // btnChatSend
            // 
            this.btnChatSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChatSend.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnChatSend.Appearance.Options.UseFont = true;
            this.btnChatSend.Location = new System.Drawing.Point(569, 15);
            this.btnChatSend.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnChatSend.Name = "btnChatSend";
            this.btnChatSend.Size = new System.Drawing.Size(117, 37);
            this.btnChatSend.TabIndex = 1;
            this.btnChatSend.Text = "Gửi";
            this.btnChatSend.Click += new System.EventHandler(this.btnChatSend_Click);
            // 
            // pnlChatHeader
            // 
            this.pnlChatHeader.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(248)))));
            this.pnlChatHeader.Appearance.Options.UseBackColor = true;
            this.pnlChatHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlChatHeader.Controls.Add(this.btnOpenDashboard);
            this.pnlChatHeader.Controls.Add(this.btnClearChat);
            this.pnlChatHeader.Controls.Add(this.lblChatTitle);
            this.pnlChatHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlChatHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlChatHeader.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlChatHeader.Name = "pnlChatHeader";
            this.pnlChatHeader.Size = new System.Drawing.Size(700, 62);
            this.pnlChatHeader.TabIndex = 0;
            // 
            // btnOpenDashboard
            // 
            this.btnOpenDashboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenDashboard.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnOpenDashboard.Appearance.Options.UseFont = true;
            this.btnOpenDashboard.Location = new System.Drawing.Point(446, 14);
            this.btnOpenDashboard.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnOpenDashboard.Name = "btnOpenDashboard";
            this.btnOpenDashboard.Size = new System.Drawing.Size(117, 34);
            this.btnOpenDashboard.TabIndex = 2;
            this.btnOpenDashboard.Text = "Dashboard";
            this.btnOpenDashboard.Click += new System.EventHandler(this.btnOpenDashboard_Click);
            // 
            // btnClearChat
            // 
            this.btnClearChat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearChat.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnClearChat.Appearance.Options.UseFont = true;
            this.btnClearChat.Location = new System.Drawing.Point(569, 14);
            this.btnClearChat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnClearChat.Name = "btnClearChat";
            this.btnClearChat.Size = new System.Drawing.Size(117, 34);
            this.btnClearChat.TabIndex = 1;
            this.btnClearChat.Text = "Xóa lịch sử";
            this.btnClearChat.Click += new System.EventHandler(this.btnClearChat_Click);
            // 
            // lblChatTitle
            // 
            this.lblChatTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblChatTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblChatTitle.Appearance.Options.UseFont = true;
            this.lblChatTitle.Appearance.Options.UseForeColor = true;
            this.lblChatTitle.Location = new System.Drawing.Point(18, 17);
            this.lblChatTitle.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lblChatTitle.Name = "lblChatTitle";
            this.lblChatTitle.Size = new System.Drawing.Size(142, 28);
            this.lblChatTitle.TabIndex = 0;
            this.lblChatTitle.Text = "AI HR COPILOT";
            // 
            // FrmAI_Chat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 862);
            this.Controls.Add(this.flpChat);
            this.Controls.Add(this.pnlQuickActions);
            this.Controls.Add(this.pnlChatInput);
            this.Controls.Add(this.pnlChatHeader);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FrmAI_Chat";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AI HR Copilot Assistant";
            this.Load += new System.EventHandler(this.FrmAI_Chat_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pnlQuickActions)).EndInit();
            this.pnlQuickActions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlChatInput)).EndInit();
            this.pnlChatInput.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtChatInput.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlChatHeader)).EndInit();
            this.pnlChatHeader.ResumeLayout(false);
            this.pnlChatHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpChat;
        private DevExpress.XtraEditors.PanelControl pnlQuickActions;
        private DevExpress.XtraEditors.SimpleButton btnActionBirthday;
        private DevExpress.XtraEditors.SimpleButton btnActionSalary;
        private DevExpress.XtraEditors.SimpleButton btnActionEmployee;
        private DevExpress.XtraEditors.SimpleButton btnActionDepartment;
        private DevExpress.XtraEditors.PanelControl pnlChatInput;
        private DevExpress.XtraEditors.TextEdit txtChatInput;
        private DevExpress.XtraEditors.SimpleButton btnChatSend;
        private DevExpress.XtraEditors.PanelControl pnlChatHeader;
        private DevExpress.XtraEditors.LabelControl lblChatTitle;
        private DevExpress.XtraEditors.SimpleButton btnClearChat;
        private DevExpress.XtraEditors.SimpleButton btnOpenDashboard;
    }
}