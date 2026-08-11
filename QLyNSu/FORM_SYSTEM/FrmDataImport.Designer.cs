namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmDataImport
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
            this.lblConnection = new DevExpress.XtraEditors.LabelControl();
            this.grpImportSource = new DevExpress.XtraEditors.GroupControl();
            this.lblImportFile = new DevExpress.XtraEditors.LabelControl();
            this.lblEncoding = new DevExpress.XtraEditors.LabelControl();
            this.lblFormat = new DevExpress.XtraEditors.LabelControl();
            this.grpImportDDL = new DevExpress.XtraEditors.GroupControl();
            this.grpExecuteDataInsertion = new DevExpress.XtraEditors.GroupControl();
            this.grpAdvanced = new DevExpress.XtraEditors.GroupControl();
            this.lblSourceSchema = new DevExpress.XtraEditors.LabelControl();
            this.lblTargetSchema = new DevExpress.XtraEditors.LabelControl();
            this.lblRows = new DevExpress.XtraEditors.LabelControl();
            this.btnEditFilter = new DevExpress.XtraEditors.SimpleButton();
            this.grpExecutionLogging = new DevExpress.XtraEditors.GroupControl();
            this.lblErrorHandling = new DevExpress.XtraEditors.LabelControl();
            this.lblLogFileOutput = new DevExpress.XtraEditors.LabelControl();
            this.btnBack = new DevExpress.XtraEditors.SimpleButton();
            this.btnNext = new DevExpress.XtraEditors.SimpleButton();
            this.btnFinish = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.cboConnection = new DevExpress.XtraEditors.ComboBoxEdit();
            this.txtImportFile = new DevExpress.XtraEditors.ButtonEdit();
            this.cboEncoding = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cboFormat = new DevExpress.XtraEditors.ComboBoxEdit();
            this.chkImportDDL = new DevExpress.XtraEditors.CheckEdit();
            this.chkExecuteDDL = new DevExpress.XtraEditors.CheckEdit();
            this.chkDropExisting = new DevExpress.XtraEditors.CheckEdit();
            this.chkIgnoreCreateErrors = new DevExpress.XtraEditors.CheckEdit();
            this.chkIncludeGrants = new DevExpress.XtraEditors.CheckEdit();
            this.chkIncludeStorage = new DevExpress.XtraEditors.CheckEdit();
            this.chkIncludeTablespace = new DevExpress.XtraEditors.CheckEdit();
            this.chkApplyPartitioning = new DevExpress.XtraEditors.CheckEdit();
            this.chkExecuteDataInsertion = new DevExpress.XtraEditors.CheckEdit();
            this.rdoTruncate = new DevExpress.XtraEditors.CheckEdit();
            this.rdoAppend = new DevExpress.XtraEditors.CheckEdit();
            this.rdoReplace = new DevExpress.XtraEditors.CheckEdit();
            this.chkDisableTriggers = new DevExpress.XtraEditors.CheckEdit();
            this.chkDisableConstraints = new DevExpress.XtraEditors.CheckEdit();
            this.chkRemapSchema = new DevExpress.XtraEditors.CheckEdit();
            this.cboSourceSchema = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cboTargetSchema = new DevExpress.XtraEditors.ComboBoxEdit();
            this.chkCommitEvery = new DevExpress.XtraEditors.CheckEdit();
            this.spinCommitRows = new DevExpress.XtraEditors.SpinEdit();
            this.chkObjectSelection = new DevExpress.XtraEditors.CheckEdit();
            this.cboErrorHandling = new DevExpress.XtraEditors.ComboBoxEdit();
            this.txtLogFileOutput = new DevExpress.XtraEditors.ButtonEdit();
            this.chkProceedToSummary = new DevExpress.XtraEditors.CheckEdit();
            ((System.ComponentModel.ISupportInitialize)(this.grpImportSource)).BeginInit();
            this.grpImportSource.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpImportDDL)).BeginInit();
            this.grpImportDDL.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpExecuteDataInsertion)).BeginInit();
            this.grpExecuteDataInsertion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpAdvanced)).BeginInit();
            this.grpAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpExecutionLogging)).BeginInit();
            this.grpExecutionLogging.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboConnection.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtImportFile.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboEncoding.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboFormat.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkImportDDL.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkExecuteDDL.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDropExisting.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIgnoreCreateErrors.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIncludeGrants.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIncludeStorage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIncludeTablespace.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkApplyPartitioning.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkExecuteDataInsertion.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rdoTruncate.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rdoAppend.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rdoReplace.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDisableTriggers.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDisableConstraints.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRemapSchema.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSourceSchema.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTargetSchema.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkCommitEvery.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinCommitRows.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkObjectSelection.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboErrorHandling.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLogFileOutput.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkProceedToSummary.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // lblConnection
            // 
            this.lblConnection.Location = new System.Drawing.Point(12, 15);
            this.lblConnection.Name = "lblConnection";
            this.lblConnection.Size = new System.Drawing.Size(206, 16);
            this.lblConnection.TabIndex = 0;
            this.lblConnection.Text = "Import / Restore Data - Connection:";
            // 
            // grpImportSource
            // 
            this.grpImportSource.AutoSize = true;
            this.grpImportSource.Controls.Add(this.lblImportFile);
            this.grpImportSource.Controls.Add(this.txtImportFile);
            this.grpImportSource.Controls.Add(this.lblEncoding);
            this.grpImportSource.Controls.Add(this.cboEncoding);
            this.grpImportSource.Controls.Add(this.lblFormat);
            this.grpImportSource.Controls.Add(this.cboFormat);
            this.grpImportSource.Location = new System.Drawing.Point(12, 45);
            this.grpImportSource.Name = "grpImportSource";
            this.grpImportSource.Size = new System.Drawing.Size(867, 82);
            this.grpImportSource.TabIndex = 2;
            this.grpImportSource.Text = "Import Source";
            // 
            // lblImportFile
            // 
            this.lblImportFile.Location = new System.Drawing.Point(15, 34);
            this.lblImportFile.Name = "lblImportFile";
            this.lblImportFile.Size = new System.Drawing.Size(67, 16);
            this.lblImportFile.TabIndex = 0;
            this.lblImportFile.Text = "Import File:";
            // 
            // lblEncoding
            // 
            this.lblEncoding.Location = new System.Drawing.Point(15, 60);
            this.lblEncoding.Name = "lblEncoding";
            this.lblEncoding.Size = new System.Drawing.Size(56, 16);
            this.lblEncoding.TabIndex = 2;
            this.lblEncoding.Text = "Encoding:";
            // 
            // lblFormat
            // 
            this.lblFormat.Location = new System.Drawing.Point(313, 60);
            this.lblFormat.Name = "lblFormat";
            this.lblFormat.Size = new System.Drawing.Size(46, 16);
            this.lblFormat.TabIndex = 4;
            this.lblFormat.Text = "Format:";
            // 
            // grpImportDDL
            // 
            this.grpImportDDL.Controls.Add(this.chkExecuteDDL);
            this.grpImportDDL.Controls.Add(this.chkDropExisting);
            this.grpImportDDL.Controls.Add(this.chkIgnoreCreateErrors);
            this.grpImportDDL.Controls.Add(this.chkIncludeGrants);
            this.grpImportDDL.Controls.Add(this.chkIncludeStorage);
            this.grpImportDDL.Controls.Add(this.chkIncludeTablespace);
            this.grpImportDDL.Controls.Add(this.chkApplyPartitioning);
            this.grpImportDDL.Location = new System.Drawing.Point(12, 135);
            this.grpImportDDL.Name = "grpImportDDL";
            this.grpImportDDL.ShowCaption = false;
            this.grpImportDDL.Size = new System.Drawing.Size(867, 100);
            this.grpImportDDL.TabIndex = 4;
            // 
            // grpExecuteDataInsertion
            // 
            this.grpExecuteDataInsertion.AutoSize = true;
            this.grpExecuteDataInsertion.Controls.Add(this.rdoTruncate);
            this.grpExecuteDataInsertion.Controls.Add(this.rdoAppend);
            this.grpExecuteDataInsertion.Controls.Add(this.rdoReplace);
            this.grpExecuteDataInsertion.Controls.Add(this.chkDisableTriggers);
            this.grpExecuteDataInsertion.Controls.Add(this.chkDisableConstraints);
            this.grpExecuteDataInsertion.Controls.Add(this.grpAdvanced);
            this.grpExecuteDataInsertion.Controls.Add(this.chkCommitEvery);
            this.grpExecuteDataInsertion.Controls.Add(this.spinCommitRows);
            this.grpExecuteDataInsertion.Controls.Add(this.lblRows);
            this.grpExecuteDataInsertion.Controls.Add(this.chkObjectSelection);
            this.grpExecuteDataInsertion.Controls.Add(this.btnEditFilter);
            this.grpExecuteDataInsertion.Location = new System.Drawing.Point(12, 245);
            this.grpExecuteDataInsertion.Name = "grpExecuteDataInsertion";
            this.grpExecuteDataInsertion.ShowCaption = false;
            this.grpExecuteDataInsertion.Size = new System.Drawing.Size(867, 180);
            this.grpExecuteDataInsertion.TabIndex = 6;
            // 
            // grpAdvanced
            // 
            this.grpAdvanced.Controls.Add(this.chkRemapSchema);
            this.grpAdvanced.Controls.Add(this.lblSourceSchema);
            this.grpAdvanced.Controls.Add(this.cboSourceSchema);
            this.grpAdvanced.Controls.Add(this.lblTargetSchema);
            this.grpAdvanced.Controls.Add(this.cboTargetSchema);
            this.grpAdvanced.Location = new System.Drawing.Point(15, 100);
            this.grpAdvanced.Name = "grpAdvanced";
            this.grpAdvanced.Size = new System.Drawing.Size(455, 70);
            this.grpAdvanced.TabIndex = 5;
            this.grpAdvanced.Text = "Advanced Options (Mapping && Filtering)";
            // 
            // lblSourceSchema
            // 
            this.lblSourceSchema.Location = new System.Drawing.Point(31, 51);
            this.lblSourceSchema.Name = "lblSourceSchema";
            this.lblSourceSchema.Size = new System.Drawing.Size(95, 16);
            this.lblSourceSchema.TabIndex = 1;
            this.lblSourceSchema.Text = "Source Schema:";
            // 
            // lblTargetSchema
            // 
            this.lblTargetSchema.Location = new System.Drawing.Point(265, 51);
            this.lblTargetSchema.Name = "lblTargetSchema";
            this.lblTargetSchema.Size = new System.Drawing.Size(115, 16);
            this.lblTargetSchema.TabIndex = 3;
            this.lblTargetSchema.Text = "->  Target Schema:";
            // 
            // lblRows
            // 
            this.lblRows.Location = new System.Drawing.Point(736, 104);
            this.lblRows.Name = "lblRows";
            this.lblRows.Size = new System.Drawing.Size(31, 16);
            this.lblRows.TabIndex = 8;
            this.lblRows.Text = "Rows";
            // 
            // btnEditFilter
            // 
            this.btnEditFilter.Location = new System.Drawing.Point(666, 130);
            this.btnEditFilter.Name = "btnEditFilter";
            this.btnEditFilter.Size = new System.Drawing.Size(75, 23);
            this.btnEditFilter.TabIndex = 10;
            this.btnEditFilter.Text = "Edit Filter...";
            // 
            // grpExecutionLogging
            // 
            this.grpExecutionLogging.AutoSize = true;
            this.grpExecutionLogging.Controls.Add(this.lblErrorHandling);
            this.grpExecutionLogging.Controls.Add(this.cboErrorHandling);
            this.grpExecutionLogging.Controls.Add(this.lblLogFileOutput);
            this.grpExecutionLogging.Controls.Add(this.txtLogFileOutput);
            this.grpExecutionLogging.Location = new System.Drawing.Point(12, 435);
            this.grpExecutionLogging.Name = "grpExecutionLogging";
            this.grpExecutionLogging.Size = new System.Drawing.Size(867, 80);
            this.grpExecutionLogging.TabIndex = 7;
            this.grpExecutionLogging.Text = "Execution && Logging";
            // 
            // lblErrorHandling
            // 
            this.lblErrorHandling.Location = new System.Drawing.Point(19, 31);
            this.lblErrorHandling.Name = "lblErrorHandling";
            this.lblErrorHandling.Size = new System.Drawing.Size(87, 16);
            this.lblErrorHandling.TabIndex = 0;
            this.lblErrorHandling.Text = "Error Handling:";
            // 
            // lblLogFileOutput
            // 
            this.lblLogFileOutput.Location = new System.Drawing.Point(15, 55);
            this.lblLogFileOutput.Name = "lblLogFileOutput";
            this.lblLogFileOutput.Size = new System.Drawing.Size(91, 16);
            this.lblLogFileOutput.TabIndex = 2;
            this.lblLogFileOutput.Text = "Log File Output:";
            // 
            // btnBack
            // 
            this.btnBack.Enabled = false;
            this.btnBack.Location = new System.Drawing.Point(375, 525);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(75, 25);
            this.btnBack.TabIndex = 9;
            this.btnBack.Text = "< Back";
            // 
            // btnNext
            // 
            this.btnNext.Enabled = false;
            this.btnNext.Location = new System.Drawing.Point(460, 525);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 25);
            this.btnNext.TabIndex = 10;
            this.btnNext.Text = "Next >";
            // 
            // btnFinish
            // 
            this.btnFinish.Location = new System.Drawing.Point(545, 525);
            this.btnFinish.Name = "btnFinish";
            this.btnFinish.Size = new System.Drawing.Size(75, 25);
            this.btnFinish.TabIndex = 11;
            this.btnFinish.Text = "Finish";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(630, 525);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            // 
            // cboConnection
            // 
            this.cboConnection.Location = new System.Drawing.Point(237, 12);
            this.cboConnection.Name = "cboConnection";
            this.cboConnection.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboConnection.Size = new System.Drawing.Size(642, 22);
            this.cboConnection.TabIndex = 1;
            // 
            // txtImportFile
            // 
            this.txtImportFile.Location = new System.Drawing.Point(88, 32);
            this.txtImportFile.Name = "txtImportFile";
            this.txtImportFile.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.txtImportFile.Size = new System.Drawing.Size(774, 22);
            this.txtImportFile.TabIndex = 1;
            // 
            // cboEncoding
            // 
            this.cboEncoding.Location = new System.Drawing.Point(88, 57);
            this.cboEncoding.Name = "cboEncoding";
            this.cboEncoding.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboEncoding.Size = new System.Drawing.Size(200, 22);
            this.cboEncoding.TabIndex = 3;
            // 
            // cboFormat
            // 
            this.cboFormat.Location = new System.Drawing.Point(363, 57);
            this.cboFormat.Name = "cboFormat";
            this.cboFormat.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboFormat.Size = new System.Drawing.Size(257, 22);
            this.cboFormat.TabIndex = 5;
            // 
            // chkImportDDL
            // 
            this.chkImportDDL.Location = new System.Drawing.Point(20, 127);
            this.chkImportDDL.Name = "chkImportDDL";
            this.chkImportDDL.Properties.Caption = "Import DDL (Structure)";
            this.chkImportDDL.Size = new System.Drawing.Size(94, 24);
            this.chkImportDDL.TabIndex = 3;
            // 
            // chkExecuteDDL
            // 
            this.chkExecuteDDL.Location = new System.Drawing.Point(30, 20);
            this.chkExecuteDDL.Name = "chkExecuteDDL";
            this.chkExecuteDDL.Properties.Caption = "Execute DDL Statements";
            this.chkExecuteDDL.Size = new System.Drawing.Size(244, 24);
            this.chkExecuteDDL.TabIndex = 0;
            // 
            // chkDropExisting
            // 
            this.chkDropExisting.Location = new System.Drawing.Point(30, 45);
            this.chkDropExisting.Name = "chkDropExisting";
            this.chkDropExisting.Properties.Caption = "Drop Existing Objects First (CASCADE)";
            this.chkDropExisting.Size = new System.Drawing.Size(258, 24);
            this.chkDropExisting.TabIndex = 1;
            // 
            // chkIgnoreCreateErrors
            // 
            this.chkIgnoreCreateErrors.Location = new System.Drawing.Point(30, 70);
            this.chkIgnoreCreateErrors.Name = "chkIgnoreCreateErrors";
            this.chkIgnoreCreateErrors.Properties.Caption = "Ignore Create Errors";
            this.chkIgnoreCreateErrors.Size = new System.Drawing.Size(244, 24);
            this.chkIgnoreCreateErrors.TabIndex = 2;
            // 
            // chkIncludeGrants
            // 
            this.chkIncludeGrants.Location = new System.Drawing.Point(300, 20);
            this.chkIncludeGrants.Name = "chkIncludeGrants";
            this.chkIncludeGrants.Properties.Caption = "Include Grants";
            this.chkIncludeGrants.Size = new System.Drawing.Size(123, 24);
            this.chkIncludeGrants.TabIndex = 3;
            // 
            // chkIncludeStorage
            // 
            this.chkIncludeStorage.Location = new System.Drawing.Point(300, 45);
            this.chkIncludeStorage.Name = "chkIncludeStorage";
            this.chkIncludeStorage.Properties.Caption = "Include Storage";
            this.chkIncludeStorage.Size = new System.Drawing.Size(123, 24);
            this.chkIncludeStorage.TabIndex = 4;
            // 
            // chkIncludeTablespace
            // 
            this.chkIncludeTablespace.Location = new System.Drawing.Point(500, 20);
            this.chkIncludeTablespace.Name = "chkIncludeTablespace";
            this.chkIncludeTablespace.Properties.Caption = "Include Tablespace";
            this.chkIncludeTablespace.Size = new System.Drawing.Size(148, 24);
            this.chkIncludeTablespace.TabIndex = 5;
            // 
            // chkApplyPartitioning
            // 
            this.chkApplyPartitioning.Location = new System.Drawing.Point(500, 45);
            this.chkApplyPartitioning.Name = "chkApplyPartitioning";
            this.chkApplyPartitioning.Properties.Caption = "Apply Partitioning";
            this.chkApplyPartitioning.Size = new System.Drawing.Size(139, 24);
            this.chkApplyPartitioning.TabIndex = 6;
            // 
            // chkExecuteDataInsertion
            // 
            this.chkExecuteDataInsertion.Location = new System.Drawing.Point(20, 237);
            this.chkExecuteDataInsertion.Name = "chkExecuteDataInsertion";
            this.chkExecuteDataInsertion.Properties.Caption = "Execute Data Insertion";
            this.chkExecuteDataInsertion.Size = new System.Drawing.Size(157, 24);
            this.chkExecuteDataInsertion.TabIndex = 5;
            // 
            // rdoTruncate
            // 
            this.rdoTruncate.Location = new System.Drawing.Point(40, 20);
            this.rdoTruncate.Name = "rdoTruncate";
            this.rdoTruncate.Properties.Caption = "Truncate Table First (Recommended for Clean-up)";
            this.rdoTruncate.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.rdoTruncate.Properties.RadioGroupIndex = 1;
            this.rdoTruncate.Size = new System.Drawing.Size(331, 24);
            this.rdoTruncate.TabIndex = 0;
            this.rdoTruncate.TabStop = false;
            // 
            // rdoAppend
            // 
            this.rdoAppend.Location = new System.Drawing.Point(40, 45);
            this.rdoAppend.Name = "rdoAppend";
            this.rdoAppend.Properties.Caption = "Append / Insert";
            this.rdoAppend.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.rdoAppend.Properties.RadioGroupIndex = 1;
            this.rdoAppend.Size = new System.Drawing.Size(319, 24);
            this.rdoAppend.TabIndex = 1;
            this.rdoAppend.TabStop = false;
            // 
            // rdoReplace
            // 
            this.rdoReplace.Location = new System.Drawing.Point(40, 70);
            this.rdoReplace.Name = "rdoReplace";
            this.rdoReplace.Properties.Caption = "Replace";
            this.rdoReplace.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.rdoReplace.Properties.RadioGroupIndex = 1;
            this.rdoReplace.Size = new System.Drawing.Size(319, 24);
            this.rdoReplace.TabIndex = 2;
            this.rdoReplace.TabStop = false;
            // 
            // chkDisableTriggers
            // 
            this.chkDisableTriggers.Location = new System.Drawing.Point(500, 29);
            this.chkDisableTriggers.Name = "chkDisableTriggers";
            this.chkDisableTriggers.Properties.Caption = "Disable Triggers during import";
            this.chkDisableTriggers.Size = new System.Drawing.Size(226, 24);
            this.chkDisableTriggers.TabIndex = 3;
            // 
            // chkDisableConstraints
            // 
            this.chkDisableConstraints.Location = new System.Drawing.Point(500, 54);
            this.chkDisableConstraints.Name = "chkDisableConstraints";
            this.chkDisableConstraints.Properties.Caption = "Disable Constraints during import";
            this.chkDisableConstraints.Size = new System.Drawing.Size(241, 24);
            this.chkDisableConstraints.TabIndex = 4;
            // 
            // chkRemapSchema
            // 
            this.chkRemapSchema.Location = new System.Drawing.Point(10, 20);
            this.chkRemapSchema.Name = "chkRemapSchema";
            this.chkRemapSchema.Properties.Caption = "Remap Schema (Source -> Target)";
            this.chkRemapSchema.Size = new System.Drawing.Size(275, 24);
            this.chkRemapSchema.TabIndex = 0;
            // 
            // cboSourceSchema
            // 
            this.cboSourceSchema.Location = new System.Drawing.Point(132, 48);
            this.cboSourceSchema.Name = "cboSourceSchema";
            this.cboSourceSchema.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboSourceSchema.Size = new System.Drawing.Size(80, 22);
            this.cboSourceSchema.TabIndex = 2;
            // 
            // cboTargetSchema
            // 
            this.cboTargetSchema.Location = new System.Drawing.Point(386, 48);
            this.cboTargetSchema.Name = "cboTargetSchema";
            this.cboTargetSchema.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboTargetSchema.Size = new System.Drawing.Size(50, 22);
            this.cboTargetSchema.TabIndex = 4;
            // 
            // chkCommitEvery
            // 
            this.chkCommitEvery.Location = new System.Drawing.Point(500, 99);
            this.chkCommitEvery.Name = "chkCommitEvery";
            this.chkCommitEvery.Properties.Caption = "Include Commit Every";
            this.chkCommitEvery.Size = new System.Drawing.Size(160, 24);
            this.chkCommitEvery.TabIndex = 6;
            // 
            // spinCommitRows
            // 
            this.spinCommitRows.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spinCommitRows.Location = new System.Drawing.Point(666, 101);
            this.spinCommitRows.Name = "spinCommitRows";
            this.spinCommitRows.Size = new System.Drawing.Size(60, 22);
            this.spinCommitRows.TabIndex = 7;
            // 
            // chkObjectSelection
            // 
            this.chkObjectSelection.Location = new System.Drawing.Point(500, 129);
            this.chkObjectSelection.Name = "chkObjectSelection";
            this.chkObjectSelection.Properties.Caption = "Object Selection Filter";
            this.chkObjectSelection.Size = new System.Drawing.Size(160, 24);
            this.chkObjectSelection.TabIndex = 9;
            // 
            // cboErrorHandling
            // 
            this.cboErrorHandling.Location = new System.Drawing.Point(112, 27);
            this.cboErrorHandling.Name = "cboErrorHandling";
            this.cboErrorHandling.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboErrorHandling.Size = new System.Drawing.Size(200, 22);
            this.cboErrorHandling.TabIndex = 1;
            // 
            // txtLogFileOutput
            // 
            this.txtLogFileOutput.Location = new System.Drawing.Point(112, 52);
            this.txtLogFileOutput.Name = "txtLogFileOutput";
            this.txtLogFileOutput.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.txtLogFileOutput.Size = new System.Drawing.Size(750, 22);
            this.txtLogFileOutput.TabIndex = 3;
            // 
            // chkProceedToSummary
            // 
            this.chkProceedToSummary.Location = new System.Drawing.Point(12, 530);
            this.chkProceedToSummary.Name = "chkProceedToSummary";
            this.chkProceedToSummary.Properties.Caption = "Proceed to summary";
            this.chkProceedToSummary.Size = new System.Drawing.Size(165, 24);
            this.chkProceedToSummary.TabIndex = 8;
            // 
            // FrmRestoreWizard
            // 
            this.ClientSize = new System.Drawing.Size(891, 560);
            this.Controls.Add(this.lblConnection);
            this.Controls.Add(this.cboConnection);
            this.Controls.Add(this.grpImportSource);
            this.Controls.Add(this.chkImportDDL);
            this.Controls.Add(this.grpImportDDL);
            this.Controls.Add(this.chkExecuteDataInsertion);
            this.Controls.Add(this.grpExecuteDataInsertion);
            this.Controls.Add(this.grpExecutionLogging);
            this.Controls.Add(this.chkProceedToSummary);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnFinish);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmRestoreWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import / Restore Data Wizard";
            ((System.ComponentModel.ISupportInitialize)(this.grpImportSource)).EndInit();
            this.grpImportSource.ResumeLayout(false);
            this.grpImportSource.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpImportDDL)).EndInit();
            this.grpImportDDL.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grpExecuteDataInsertion)).EndInit();
            this.grpExecuteDataInsertion.ResumeLayout(false);
            this.grpExecuteDataInsertion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpAdvanced)).EndInit();
            this.grpAdvanced.ResumeLayout(false);
            this.grpAdvanced.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpExecutionLogging)).EndInit();
            this.grpExecutionLogging.ResumeLayout(false);
            this.grpExecutionLogging.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboConnection.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtImportFile.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboEncoding.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboFormat.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkImportDDL.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkExecuteDDL.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDropExisting.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIgnoreCreateErrors.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIncludeGrants.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIncludeStorage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIncludeTablespace.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkApplyPartitioning.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkExecuteDataInsertion.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rdoTruncate.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rdoAppend.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rdoReplace.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDisableTriggers.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDisableConstraints.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRemapSchema.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSourceSchema.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTargetSchema.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkCommitEvery.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinCommitRows.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkObjectSelection.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboErrorHandling.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLogFileOutput.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkProceedToSummary.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl lblConnection;
        private DevExpress.XtraEditors.ComboBoxEdit cboConnection;
        
        private DevExpress.XtraEditors.GroupControl grpImportSource;
        private DevExpress.XtraEditors.LabelControl lblImportFile;
        private DevExpress.XtraEditors.ButtonEdit txtImportFile;
        private DevExpress.XtraEditors.LabelControl lblEncoding;
        private DevExpress.XtraEditors.ComboBoxEdit cboEncoding;
        private DevExpress.XtraEditors.LabelControl lblFormat;
        private DevExpress.XtraEditors.ComboBoxEdit cboFormat;

        private DevExpress.XtraEditors.GroupControl grpImportDDL;
        private DevExpress.XtraEditors.CheckEdit chkImportDDL;
        private DevExpress.XtraEditors.CheckEdit chkExecuteDDL;
        private DevExpress.XtraEditors.CheckEdit chkDropExisting;
        private DevExpress.XtraEditors.CheckEdit chkIgnoreCreateErrors;
        private DevExpress.XtraEditors.CheckEdit chkIncludeGrants;
        private DevExpress.XtraEditors.CheckEdit chkIncludeStorage;
        private DevExpress.XtraEditors.CheckEdit chkIncludeTablespace;
        private DevExpress.XtraEditors.CheckEdit chkApplyPartitioning;

        private DevExpress.XtraEditors.GroupControl grpExecuteDataInsertion;
        private DevExpress.XtraEditors.CheckEdit chkExecuteDataInsertion;
        private DevExpress.XtraEditors.CheckEdit rdoTruncate;
        private DevExpress.XtraEditors.CheckEdit rdoAppend;
        private DevExpress.XtraEditors.CheckEdit rdoReplace;
        private DevExpress.XtraEditors.CheckEdit chkDisableTriggers;
        private DevExpress.XtraEditors.CheckEdit chkDisableConstraints;
        
        private DevExpress.XtraEditors.GroupControl grpAdvanced;
        private DevExpress.XtraEditors.CheckEdit chkRemapSchema;
        private DevExpress.XtraEditors.LabelControl lblSourceSchema;
        private DevExpress.XtraEditors.ComboBoxEdit cboSourceSchema;
        private DevExpress.XtraEditors.LabelControl lblTargetSchema;
        private DevExpress.XtraEditors.ComboBoxEdit cboTargetSchema;
        
        private DevExpress.XtraEditors.CheckEdit chkCommitEvery;
        private DevExpress.XtraEditors.SpinEdit spinCommitRows;
        private DevExpress.XtraEditors.LabelControl lblRows;
        private DevExpress.XtraEditors.CheckEdit chkObjectSelection;
        private DevExpress.XtraEditors.SimpleButton btnEditFilter;

        private DevExpress.XtraEditors.GroupControl grpExecutionLogging;
        private DevExpress.XtraEditors.LabelControl lblErrorHandling;
        private DevExpress.XtraEditors.ComboBoxEdit cboErrorHandling;
        private DevExpress.XtraEditors.LabelControl lblLogFileOutput;
        private DevExpress.XtraEditors.ButtonEdit txtLogFileOutput;

        private DevExpress.XtraEditors.CheckEdit chkProceedToSummary;
        private DevExpress.XtraEditors.SimpleButton btnBack;
        private DevExpress.XtraEditors.SimpleButton btnNext;
        private DevExpress.XtraEditors.SimpleButton btnFinish;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
    }
}
