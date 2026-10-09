namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmAiScopeGrantDetail
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
            this.pnlTop = new DevExpress.XtraEditors.PanelControl();
            this.lblSubjectTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblAiRightStatus = new DevExpress.XtraEditors.LabelControl();
            this.pnlSecurityNotice = new DevExpress.XtraEditors.PanelControl();
            this.lblSecurityWarning = new DevExpress.XtraEditors.LabelControl();
            this.gridControlGrants = new DevExpress.XtraGrid.GridControl();
            this.gridViewGrants = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colCapabilityCode = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDescription = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colSystemStatus = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colRequiredRight = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDirectEffect = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repoCboEffect = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.colDirectScopeType = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repoCboScopeType = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.colDirectScopeKey = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repoLookUpScopeKey = new DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit();
            this.colDirectValidFrom = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repoDateFrom = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            this.colDirectValidTo = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repoDateTo = new DevExpress.XtraEditors.Repository.RepositoryItemDateEdit();
            this.colInherited = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colEffective = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colExplanation = new DevExpress.XtraGrid.Columns.GridColumn();
            this.pnlBottom = new DevExpress.XtraEditors.PanelControl();
            this.btnLuu = new DevExpress.XtraEditors.SimpleButton();
            this.btnLamMoi = new DevExpress.XtraEditors.SimpleButton();
            this.btnDong = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.pnlTop)).BeginInit();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlSecurityNotice)).BeginInit();
            this.pnlSecurityNotice.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlGrants)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewGrants)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoCboEffect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoCboScopeType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoLookUpScopeKey)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoDateFrom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoDateFrom.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoDateTo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoDateTo.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlBottom)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.lblSubjectTitle);
            this.pnlTop.Controls.Add(this.lblAiRightStatus);
            this.pnlTop.Controls.Add(this.pnlSecurityNotice);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1240, 105);
            this.pnlTop.TabIndex = 0;
            // 
            // lblSubjectTitle
            // 
            this.lblSubjectTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubjectTitle.Appearance.Options.UseFont = true;
            this.lblSubjectTitle.Location = new System.Drawing.Point(16, 12);
            this.lblSubjectTitle.Name = "lblSubjectTitle";
            this.lblSubjectTitle.Size = new System.Drawing.Size(325, 20);
            this.lblSubjectTitle.TabIndex = 0;
            this.lblSubjectTitle.Text = "Quản trị quyền tra cứu AI: [Subject Name]";
            // 
            // lblAiRightStatus
            // 
            this.lblAiRightStatus.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAiRightStatus.Appearance.Options.UseFont = true;
            this.lblAiRightStatus.Location = new System.Drawing.Point(16, 36);
            this.lblAiRightStatus.Name = "lblAiRightStatus";
            this.lblAiRightStatus.Size = new System.Drawing.Size(260, 15);
            this.lblAiRightStatus.TabIndex = 1;
            this.lblAiRightStatus.Text = "Quyền giao diện AI (F_SYSTEM_AI): Đang kiểm tra...";
            // 
            // pnlSecurityNotice
            // 
            this.pnlSecurityNotice.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(248)))), ((int)(((byte)(225)))));
            this.pnlSecurityNotice.Appearance.Options.UseBackColor = true;
            this.pnlSecurityNotice.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.pnlSecurityNotice.Controls.Add(this.lblSecurityWarning);
            this.pnlSecurityNotice.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlSecurityNotice.Location = new System.Drawing.Point(2, 57);
            this.pnlSecurityNotice.Name = "pnlSecurityNotice";
            this.pnlSecurityNotice.Size = new System.Drawing.Size(1236, 46);
            this.pnlSecurityNotice.TabIndex = 2;
            // 
            // lblSecurityWarning
            // 
            this.lblSecurityWarning.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSecurityWarning.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(54)))), ((int)(((byte)(12)))));
            this.lblSecurityWarning.Appearance.Options.UseFont = true;
            this.lblSecurityWarning.Appearance.Options.UseForeColor = true;
            this.lblSecurityWarning.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSecurityWarning.Location = new System.Drawing.Point(2, 2);
            this.lblSecurityWarning.Name = "lblSecurityWarning";
            this.lblSecurityWarning.Padding = new System.Windows.Forms.Padding(10, 4, 10, 4);
            this.lblSecurityWarning.Size = new System.Drawing.Size(1232, 42);
            this.lblSecurityWarning.TabIndex = 0;
            this.lblSecurityWarning.Text = "LƯU Ý AN NINH VÀ CƠ CHẾ DENY HIỆN TẠI:\r\n- Bước cấp quyền truy vấn PKG_AI_AUTH ch" +
    "ặn cả capability khi có DENY phù hợp với tài khoản/nhóm; không hỗ trợ chỉ loại tr" +
    "ừ một phòng ban.\r\n- Quyền F_SYSTEM_AI chỉ cấp quyền vào tính năng AI, không thay " +
    "thế quyền nghiệp vụ nền và phạm vi dữ liệu.";
            // 
            // gridControlGrants
            // 
            this.gridControlGrants.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlGrants.Location = new System.Drawing.Point(0, 105);
            this.gridControlGrants.MainView = this.gridViewGrants;
            this.gridControlGrants.Name = "gridControlGrants";
            this.gridControlGrants.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repoCboEffect,
            this.repoCboScopeType,
            this.repoLookUpScopeKey,
            this.repoDateFrom,
            this.repoDateTo});
            this.gridControlGrants.Size = new System.Drawing.Size(1240, 500);
            this.gridControlGrants.TabIndex = 1;
            this.gridControlGrants.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewGrants});
            // 
            // gridViewGrants
            // 
            this.gridViewGrants.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colCapabilityCode,
            this.colDescription,
            this.colSystemStatus,
            this.colRequiredRight,
            this.colDirectEffect,
            this.colDirectScopeType,
            this.colDirectScopeKey,
            this.colDirectValidFrom,
            this.colDirectValidTo,
            this.colInherited,
            this.colEffective,
            this.colExplanation});
            this.gridViewGrants.GridControl = this.gridControlGrants;
            this.gridViewGrants.Name = "gridViewGrants";
            this.gridViewGrants.OptionsView.ColumnAutoWidth = false;
            this.gridViewGrants.OptionsView.EnableAppearanceEvenRow = true;
            this.gridViewGrants.OptionsView.RowAutoHeight = true;
            this.gridViewGrants.OptionsView.ShowGroupPanel = false;
            this.gridViewGrants.RowHeight = 28;
            // 
            // colCapabilityCode
            // 
            this.colCapabilityCode.Caption = "Mã Capability";
            this.colCapabilityCode.FieldName = "CapabilityCode";
            this.colCapabilityCode.MinWidth = 130;
            this.colCapabilityCode.Name = "colCapabilityCode";
            this.colCapabilityCode.OptionsColumn.AllowEdit = false;
            this.colCapabilityCode.OptionsColumn.ReadOnly = true;
            this.colCapabilityCode.Visible = true;
            this.colCapabilityCode.VisibleIndex = 0;
            this.colCapabilityCode.Width = 145;
            // 
            // colDescription
            // 
            this.colDescription.Caption = "Nghiệp vụ AI tra cứu";
            this.colDescription.FieldName = "Description";
            this.colDescription.MinWidth = 180;
            this.colDescription.Name = "colDescription";
            this.colDescription.OptionsColumn.AllowEdit = false;
            this.colDescription.OptionsColumn.ReadOnly = true;
            this.colDescription.Visible = true;
            this.colDescription.VisibleIndex = 1;
            this.colDescription.Width = 200;
            // 
            // colSystemStatus
            // 
            this.colSystemStatus.Caption = "Hệ thống";
            this.colSystemStatus.FieldName = "AvailabilityStatus";
            this.colSystemStatus.MinWidth = 90;
            this.colSystemStatus.Name = "colSystemStatus";
            this.colSystemStatus.OptionsColumn.AllowEdit = false;
            this.colSystemStatus.OptionsColumn.ReadOnly = true;
            this.colSystemStatus.Visible = true;
            this.colSystemStatus.VisibleIndex = 2;
            this.colSystemStatus.Width = 95;
            // 
            // colRequiredRight
            // 
            this.colRequiredRight.Caption = "Quyền nghiệp vụ nền bắt buộc";
            this.colRequiredRight.FieldName = "FunctionRightDisplay";
            this.colRequiredRight.MinWidth = 160;
            this.colRequiredRight.Name = "colRequiredRight";
            this.colRequiredRight.OptionsColumn.AllowEdit = false;
            this.colRequiredRight.OptionsColumn.ReadOnly = true;
            this.colRequiredRight.Visible = true;
            this.colRequiredRight.VisibleIndex = 3;
            this.colRequiredRight.Width = 175;
            // 
            // colDirectEffect
            // 
            this.colDirectEffect.Caption = "Quyền AI (Trực tiếp)";
            this.colDirectEffect.ColumnEdit = this.repoCboEffect;
            this.colDirectEffect.FieldName = "DirectEffect";
            this.colDirectEffect.MinWidth = 100;
            this.colDirectEffect.Name = "colDirectEffect";
            this.colDirectEffect.Visible = true;
            this.colDirectEffect.VisibleIndex = 4;
            this.colDirectEffect.Width = 110;
            // 
            // repoCboEffect
            // 
            this.repoCboEffect.AutoHeight = false;
            this.repoCboEffect.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repoCboEffect.Items.AddRange(new object[] {
            "NONE",
            "ALLOW",
            "DENY"});
            this.repoCboEffect.Name = "repoCboEffect";
            this.repoCboEffect.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // colDirectScopeType
            // 
            this.colDirectScopeType.Caption = "Phạm vi dữ liệu";
            this.colDirectScopeType.ColumnEdit = this.repoCboScopeType;
            this.colDirectScopeType.FieldName = "DirectScopeType";
            this.colDirectScopeType.MinWidth = 100;
            this.colDirectScopeType.Name = "colDirectScopeType";
            this.colDirectScopeType.Visible = true;
            this.colDirectScopeType.VisibleIndex = 5;
            this.colDirectScopeType.Width = 115;
            // 
            // repoCboScopeType
            // 
            this.repoCboScopeType.AutoHeight = false;
            this.repoCboScopeType.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repoCboScopeType.Items.AddRange(new object[] {
            "SELF",
            "DEPARTMENT",
            "COMPANY",
            "ALL"});
            this.repoCboScopeType.Name = "repoCboScopeType";
            this.repoCboScopeType.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // colDirectScopeKey
            // 
            this.colDirectScopeKey.Caption = "Đối tượng phạm vi";
            this.colDirectScopeKey.ColumnEdit = this.repoLookUpScopeKey;
            this.colDirectScopeKey.FieldName = "DirectScopeKey";
            this.colDirectScopeKey.MinWidth = 140;
            this.colDirectScopeKey.Name = "colDirectScopeKey";
            this.colDirectScopeKey.Visible = true;
            this.colDirectScopeKey.VisibleIndex = 6;
            this.colDirectScopeKey.Width = 160;
            // 
            // repoLookUpScopeKey
            // 
            this.repoLookUpScopeKey.AutoHeight = false;
            this.repoLookUpScopeKey.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repoLookUpScopeKey.DisplayMember = "Name";
            this.repoLookUpScopeKey.Name = "repoLookUpScopeKey";
            this.repoLookUpScopeKey.NullText = "-- Chọn đối tượng --";
            this.repoLookUpScopeKey.ValueMember = "Id";
            // 
            // colDirectValidFrom
            // 
            this.colDirectValidFrom.Caption = "Hiệu lực từ";
            this.colDirectValidFrom.ColumnEdit = this.repoDateFrom;
            this.colDirectValidFrom.FieldName = "DirectValidFrom";
            this.colDirectValidFrom.MinWidth = 95;
            this.colDirectValidFrom.Name = "colDirectValidFrom";
            this.colDirectValidFrom.Visible = true;
            this.colDirectValidFrom.VisibleIndex = 7;
            this.colDirectValidFrom.Width = 100;
            // 
            // repoDateFrom
            // 
            this.repoDateFrom.AutoHeight = false;
            this.repoDateFrom.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repoDateFrom.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repoDateFrom.DisplayFormat.FormatString = "dd/MM/yyyy";
            this.repoDateFrom.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.repoDateFrom.EditFormat.FormatString = "dd/MM/yyyy";
            this.repoDateFrom.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.repoDateFrom.Mask.EditMask = "dd/MM/yyyy";
            this.repoDateFrom.Name = "repoDateFrom";
            // 
            // colDirectValidTo
            // 
            this.colDirectValidTo.Caption = "Hết hạn";
            this.colDirectValidTo.ColumnEdit = this.repoDateTo;
            this.colDirectValidTo.FieldName = "DirectValidTo";
            this.colDirectValidTo.MinWidth = 95;
            this.colDirectValidTo.Name = "colDirectValidTo";
            this.colDirectValidTo.Visible = true;
            this.colDirectValidTo.VisibleIndex = 8;
            this.colDirectValidTo.Width = 100;
            // 
            // repoDateTo
            // 
            this.repoDateTo.AutoHeight = false;
            this.repoDateTo.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repoDateTo.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repoDateTo.DisplayFormat.FormatString = "dd/MM/yyyy";
            this.repoDateTo.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.repoDateTo.EditFormat.FormatString = "dd/MM/yyyy";
            this.repoDateTo.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.repoDateTo.Mask.EditMask = "dd/MM/yyyy";
            this.repoDateTo.Name = "repoDateTo";
            // 
            // colInherited
            // 
            this.colInherited.Caption = "Kế thừa từ nhóm";
            this.colInherited.FieldName = "InheritedSummary";
            this.colInherited.MinWidth = 130;
            this.colInherited.Name = "colInherited";
            this.colInherited.OptionsColumn.AllowEdit = false;
            this.colInherited.OptionsColumn.ReadOnly = true;
            this.colInherited.Visible = true;
            this.colInherited.VisibleIndex = 9;
            this.colInherited.Width = 145;
            // 
            // colEffective
            // 
            this.colEffective.Caption = "Kết quả có hiệu lực (Effective)";
            this.colEffective.FieldName = "EffectiveScopeSummary";
            this.colEffective.MinWidth = 160;
            this.colEffective.Name = "colEffective";
            this.colEffective.OptionsColumn.AllowEdit = false;
            this.colEffective.OptionsColumn.ReadOnly = true;
            this.colEffective.Visible = true;
            this.colEffective.VisibleIndex = 10;
            this.colEffective.Width = 185;
            // 
            // colExplanation
            // 
            this.colExplanation.Caption = "Diễn giải cơ chế an ninh";
            this.colExplanation.FieldName = "ExplanationNotes";
            this.colExplanation.MinWidth = 180;
            this.colExplanation.Name = "colExplanation";
            this.colExplanation.OptionsColumn.AllowEdit = false;
            this.colExplanation.OptionsColumn.ReadOnly = true;
            this.colExplanation.Visible = true;
            this.colExplanation.VisibleIndex = 11;
            this.colExplanation.Width = 230;
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.btnLuu);
            this.pnlBottom.Controls.Add(this.btnLamMoi);
            this.pnlBottom.Controls.Add(this.btnDong);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 605);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(1240, 50);
            this.pnlBottom.TabIndex = 2;
            // 
            // btnLuu
            // 
            this.btnLuu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLuu.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLuu.Appearance.Options.UseFont = true;
            this.btnLuu.Location = new System.Drawing.Point(920, 10);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(120, 30);
            this.btnLuu.TabIndex = 0;
            this.btnLuu.Text = "Lưu phân quyền";
            this.btnLuu.Click += new System.EventHandler(this.btnLuu_Click);
            // 
            // btnLamMoi
            // 
            this.btnLamMoi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLamMoi.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLamMoi.Appearance.Options.UseFont = true;
            this.btnLamMoi.Location = new System.Drawing.Point(1050, 10);
            this.btnLamMoi.Name = "btnLamMoi";
            this.btnLamMoi.Size = new System.Drawing.Size(90, 30);
            this.btnLamMoi.TabIndex = 1;
            this.btnLamMoi.Text = "Làm mới";
            this.btnLamMoi.Click += new System.EventHandler(this.btnLamMoi_Click);
            // 
            // btnDong
            // 
            this.btnDong.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDong.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDong.Appearance.Options.UseFont = true;
            this.btnDong.Location = new System.Drawing.Point(1150, 10);
            this.btnDong.Name = "btnDong";
            this.btnDong.Size = new System.Drawing.Size(80, 30);
            this.btnDong.TabIndex = 2;
            this.btnDong.Text = "Đóng";
            this.btnDong.Click += new System.EventHandler(this.btnDong_Click);
            // 
            // FrmAiScopeGrantDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1240, 655);
            this.Controls.Add(this.gridControlGrants);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.Name = "FrmAiScopeGrantDetail";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Phân quyền tra cứu AI";
            this.Load += new System.EventHandler(this.FrmAiScopeGrantDetail_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pnlTop)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlSecurityNotice)).EndInit();
            this.pnlSecurityNotice.ResumeLayout(false);
            this.pnlSecurityNotice.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlGrants)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewGrants)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoCboEffect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoCboScopeType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoLookUpScopeKey)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoDateFrom.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoDateFrom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoDateTo.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repoDateTo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlBottom)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl pnlTop;
        private DevExpress.XtraEditors.LabelControl lblSubjectTitle;
        private DevExpress.XtraEditors.LabelControl lblAiRightStatus;
        private DevExpress.XtraEditors.PanelControl pnlSecurityNotice;
        private DevExpress.XtraEditors.LabelControl lblSecurityWarning;
        private DevExpress.XtraGrid.GridControl gridControlGrants;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewGrants;
        private DevExpress.XtraGrid.Columns.GridColumn colCapabilityCode;
        private DevExpress.XtraGrid.Columns.GridColumn colDescription;
        private DevExpress.XtraGrid.Columns.GridColumn colSystemStatus;
        private DevExpress.XtraGrid.Columns.GridColumn colRequiredRight;
        private DevExpress.XtraGrid.Columns.GridColumn colDirectEffect;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repoCboEffect;
        private DevExpress.XtraGrid.Columns.GridColumn colDirectScopeType;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repoCboScopeType;
        private DevExpress.XtraGrid.Columns.GridColumn colDirectScopeKey;
        private DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit repoLookUpScopeKey;
        private DevExpress.XtraGrid.Columns.GridColumn colDirectValidFrom;
        private DevExpress.XtraEditors.Repository.RepositoryItemDateEdit repoDateFrom;
        private DevExpress.XtraGrid.Columns.GridColumn colDirectValidTo;
        private DevExpress.XtraEditors.Repository.RepositoryItemDateEdit repoDateTo;
        private DevExpress.XtraGrid.Columns.GridColumn colInherited;
        private DevExpress.XtraGrid.Columns.GridColumn colEffective;
        private DevExpress.XtraGrid.Columns.GridColumn colExplanation;
        private DevExpress.XtraEditors.PanelControl pnlBottom;
        private DevExpress.XtraEditors.SimpleButton btnLuu;
        private DevExpress.XtraEditors.SimpleButton btnLamMoi;
        private DevExpress.XtraEditors.SimpleButton btnDong;
    }
}
