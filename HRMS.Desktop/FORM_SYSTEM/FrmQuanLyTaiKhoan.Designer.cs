namespace QLyNSu.FORM_SYSTEM
{
    partial class FrmQuanLyTaiKhoan
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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.btnRefreshAll = new DevExpress.XtraEditors.SimpleButton();
            this.pnlDashboard = new System.Windows.Forms.Panel();
            this.pnlCard1 = new System.Windows.Forms.Panel();
            this.lblCard1Title = new DevExpress.XtraEditors.LabelControl();
            this.lblTotalEmpVal = new DevExpress.XtraEditors.LabelControl();
            this.pnlCard2 = new System.Windows.Forms.Panel();
            this.lblCard2Title = new DevExpress.XtraEditors.LabelControl();
            this.lblAccCreatedVal = new DevExpress.XtraEditors.LabelControl();
            this.pnlCard3 = new System.Windows.Forms.Panel();
            this.lblCard3Title = new DevExpress.XtraEditors.LabelControl();
            this.lblNoAccVal = new DevExpress.XtraEditors.LabelControl();
            this.pnlCard4 = new System.Windows.Forms.Panel();
            this.lblCard4Title = new DevExpress.XtraEditors.LabelControl();
            this.lblMobOnVal = new DevExpress.XtraEditors.LabelControl();
            this.pnlCard5 = new System.Windows.Forms.Panel();
            this.lblCard5Title = new DevExpress.XtraEditors.LabelControl();
            this.lblMobOffVal = new DevExpress.XtraEditors.LabelControl();
            this.pnlCard6 = new System.Windows.Forms.Panel();
            this.lblCard6Title = new DevExpress.XtraEditors.LabelControl();
            this.lblLockedVal = new DevExpress.XtraEditors.LabelControl();
            this.pnlCard7 = new System.Windows.Forms.Panel();
            this.lblCard7Title = new DevExpress.XtraEditors.LabelControl();
            this.lblSysVal = new DevExpress.XtraEditors.LabelControl();
            this.tabMain = new DevExpress.XtraTab.XtraTabControl();
            this.tabDanhSach = new DevExpress.XtraTab.XtraTabPage();
            this.gcUsers = new DevExpress.XtraGrid.GridControl();
            this.gvUsers = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.pnlActions = new System.Windows.Forms.Panel();
            this.btnThem = new DevExpress.XtraEditors.SimpleButton();
            this.btnSua = new DevExpress.XtraEditors.SimpleButton();
            this.btnKhoaMoKhoa = new DevExpress.XtraEditors.SimpleButton();
            this.btnResetMatKhau = new DevExpress.XtraEditors.SimpleButton();
            this.btnToggleMobile = new DevExpress.XtraEditors.SimpleButton();
            this.btnLienKetNV = new DevExpress.XtraEditors.SimpleButton();
            this.btnChuyenSangCapPhat = new DevExpress.XtraEditors.SimpleButton();
            this.btnXuatExcel = new DevExpress.XtraEditors.SimpleButton();
            this.pnlFilter = new System.Windows.Forms.Panel();
            this.lblSearch = new DevExpress.XtraEditors.LabelControl();
            this.txtSearch = new DevExpress.XtraEditors.TextEdit();
            this.lblDept = new DevExpress.XtraEditors.LabelControl();
            this.cboPhongBan = new DevExpress.XtraEditors.LookUpEdit();
            this.lblStatus = new DevExpress.XtraEditors.LabelControl();
            this.cboTrangThai = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblMob = new DevExpress.XtraEditors.LabelControl();
            this.cboMobileFilter = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnLoc = new DevExpress.XtraEditors.SimpleButton();
            this.btnXoaLoc = new DevExpress.XtraEditors.SimpleButton();
            this.tabChiTiet = new DevExpress.XtraTab.XtraTabPage();
            this.pnlDetailContent = new System.Windows.Forms.Panel();
            this.grpAccountInfo = new DevExpress.XtraEditors.GroupControl();
            this.lblDetailId = new DevExpress.XtraEditors.LabelControl();
            this.txtDetailId = new DevExpress.XtraEditors.TextEdit();
            this.lblDetailUsername = new DevExpress.XtraEditors.LabelControl();
            this.txtDetailUsername = new DevExpress.XtraEditors.TextEdit();
            this.lblDetailFullName = new DevExpress.XtraEditors.LabelControl();
            this.txtDetailFullName = new DevExpress.XtraEditors.TextEdit();
            this.lblDetailClientType = new DevExpress.XtraEditors.LabelControl();
            this.cboDetailClientType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.chkDetailDisabled = new DevExpress.XtraEditors.CheckEdit();
            this.lblDetailMobileBadge = new DevExpress.XtraEditors.LabelControl();
            this.btnSaveDetail = new DevExpress.XtraEditors.SimpleButton();
            this.grpEmployeeInfo = new DevExpress.XtraEditors.GroupControl();
            this.lblDetailEmpId = new DevExpress.XtraEditors.LabelControl();
            this.txtDetailEmpId = new DevExpress.XtraEditors.TextEdit();
            this.lblDetailEmpCode = new DevExpress.XtraEditors.LabelControl();
            this.txtDetailEmpCode = new DevExpress.XtraEditors.TextEdit();
            this.lblDetailEmpName = new DevExpress.XtraEditors.LabelControl();
            this.txtDetailEmpName = new DevExpress.XtraEditors.TextEdit();
            this.lblDetailDept = new DevExpress.XtraEditors.LabelControl();
            this.txtDetailDept = new DevExpress.XtraEditors.TextEdit();
            this.lblDetailPos = new DevExpress.XtraEditors.LabelControl();
            this.txtDetailPos = new DevExpress.XtraEditors.TextEdit();
            this.tabCapPhatHangLoat = new DevExpress.XtraTab.XtraTabPage();
            this.tabBulkSub = new DevExpress.XtraTab.XtraTabControl();
            this.tabSubCandidates = new DevExpress.XtraTab.XtraTabPage();
            this.gcCandidates = new DevExpress.XtraGrid.GridControl();
            this.gvCandidates = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.tabSubResults = new DevExpress.XtraTab.XtraTabPage();
            this.gcBulkResults = new DevExpress.XtraGrid.GridControl();
            this.gvBulkResults = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.pnlBulkActions = new System.Windows.Forms.Panel();
            this.btnBulkSelectAll = new DevExpress.XtraEditors.SimpleButton();
            this.btnBulkDeselectAll = new DevExpress.XtraEditors.SimpleButton();
            this.btnExecuteBulk = new DevExpress.XtraEditors.SimpleButton();
            this.lblBulkResultsTitle = new DevExpress.XtraEditors.LabelControl();
            this.grpBulkFilter = new DevExpress.XtraEditors.GroupControl();
            this.lblBulkDept = new DevExpress.XtraEditors.LabelControl();
            this.cboBulkDept = new DevExpress.XtraEditors.LookUpEdit();
            this.chkBulkOnlyWithoutAcc = new DevExpress.XtraEditors.CheckEdit();
            this.chkBulkEnableMobile = new DevExpress.XtraEditors.CheckEdit();
            this.lblBulkDefaultPass = new DevExpress.XtraEditors.LabelControl();
            this.txtBulkDefaultPass = new DevExpress.XtraEditors.TextEdit();
            this.btnBulkRefresh = new DevExpress.XtraEditors.SimpleButton();
            this.pnlBulkMetrics = new System.Windows.Forms.Panel();
            this.lblBulkEligible = new DevExpress.XtraEditors.LabelControl();
            this.lblBulkReady = new DevExpress.XtraEditors.LabelControl();
            this.lblBulkExisting = new DevExpress.XtraEditors.LabelControl();
            this.lblBulkSystem = new DevExpress.XtraEditors.LabelControl();
            this.lblBulkInactive = new DevExpress.XtraEditors.LabelControl();
            this.pnlHeader.SuspendLayout();
            this.pnlDashboard.SuspendLayout();
            this.pnlCard1.SuspendLayout();
            this.pnlCard2.SuspendLayout();
            this.pnlCard3.SuspendLayout();
            this.pnlCard4.SuspendLayout();
            this.pnlCard5.SuspendLayout();
            this.pnlCard6.SuspendLayout();
            this.pnlCard7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabMain)).BeginInit();
            this.tabMain.SuspendLayout();
            this.tabDanhSach.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcUsers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvUsers)).BeginInit();
            this.pnlActions.SuspendLayout();
            this.pnlFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboPhongBan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTrangThai.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboMobileFilter.Properties)).BeginInit();
            this.tabChiTiet.SuspendLayout();
            this.pnlDetailContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpAccountInfo)).BeginInit();
            this.grpAccountInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailId.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailUsername.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailFullName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDetailClientType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDetailDisabled.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpEmployeeInfo)).BeginInit();
            this.grpEmployeeInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailEmpId.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailEmpCode.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailEmpName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailDept.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailPos.Properties)).BeginInit();
            this.tabCapPhatHangLoat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabBulkSub)).BeginInit();
            this.tabBulkSub.SuspendLayout();
            this.tabSubCandidates.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcCandidates)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvCandidates)).BeginInit();
            this.tabSubResults.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcBulkResults)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvBulkResults)).BeginInit();
            this.pnlBulkActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpBulkFilter)).BeginInit();
            this.grpBulkFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboBulkDept.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBulkOnlyWithoutAcc.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBulkEnableMobile.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtBulkDefaultPass.Properties)).BeginInit();
            this.pnlBulkMetrics.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.btnRefreshAll);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.pnlHeader.Size = new System.Drawing.Size(1325, 48);
            this.pnlHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(76)))), ((int)(((byte)(120)))));
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Appearance.Options.UseForeColor = true;
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(326, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "🔐 QUẢN TRỊ TÀI KHOẢN MOBILE";
            // 
            // btnRefreshAll
            // 
            this.btnRefreshAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshAll.Location = new System.Drawing.Point(2235, 10);
            this.btnRefreshAll.Name = "btnRefreshAll";
            this.btnRefreshAll.Size = new System.Drawing.Size(130, 28);
            this.btnRefreshAll.TabIndex = 1;
            this.btnRefreshAll.Text = "Làm Mới Toàn Bộ";
            this.btnRefreshAll.Click += new System.EventHandler(this.btnRefreshAll_Click);
            // 
            // pnlDashboard
            // 
            this.pnlDashboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.pnlDashboard.Controls.Add(this.pnlCard1);
            this.pnlDashboard.Controls.Add(this.pnlCard2);
            this.pnlDashboard.Controls.Add(this.pnlCard3);
            this.pnlDashboard.Controls.Add(this.pnlCard4);
            this.pnlDashboard.Controls.Add(this.pnlCard5);
            this.pnlDashboard.Controls.Add(this.pnlCard6);
            this.pnlDashboard.Controls.Add(this.pnlCard7);
            this.pnlDashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDashboard.Location = new System.Drawing.Point(0, 48);
            this.pnlDashboard.Name = "pnlDashboard";
            this.pnlDashboard.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.pnlDashboard.Size = new System.Drawing.Size(1325, 76);
            this.pnlDashboard.TabIndex = 1;
            // 
            // pnlCard1
            // 
            this.pnlCard1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.pnlCard1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCard1.Controls.Add(this.lblCard1Title);
            this.pnlCard1.Controls.Add(this.lblTotalEmpVal);
            this.pnlCard1.Location = new System.Drawing.Point(12, 6);
            this.pnlCard1.Name = "pnlCard1";
            this.pnlCard1.Size = new System.Drawing.Size(168, 64);
            this.pnlCard1.TabIndex = 0;
            // 
            // lblCard1Title
            // 
            this.lblCard1Title.Appearance.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblCard1Title.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(80)))), ((int)(((byte)(160)))));
            this.lblCard1Title.Appearance.Options.UseFont = true;
            this.lblCard1Title.Appearance.Options.UseForeColor = true;
            this.lblCard1Title.Location = new System.Drawing.Point(6, 6);
            this.lblCard1Title.Name = "lblCard1Title";
            this.lblCard1Title.Size = new System.Drawing.Size(101, 17);
            this.lblCard1Title.TabIndex = 0;
            this.lblCard1Title.Text = "TỔNG NHÂN SỰ";
            // 
            // lblTotalEmpVal
            // 
            this.lblTotalEmpVal.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTotalEmpVal.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(80)))), ((int)(((byte)(160)))));
            this.lblTotalEmpVal.Appearance.Options.UseFont = true;
            this.lblTotalEmpVal.Appearance.Options.UseForeColor = true;
            this.lblTotalEmpVal.Location = new System.Drawing.Point(8, 24);
            this.lblTotalEmpVal.Name = "lblTotalEmpVal";
            this.lblTotalEmpVal.Size = new System.Drawing.Size(16, 37);
            this.lblTotalEmpVal.TabIndex = 1;
            this.lblTotalEmpVal.Text = "0";
            // 
            // pnlCard2
            // 
            this.pnlCard2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(250)))), ((int)(((byte)(242)))));
            this.pnlCard2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCard2.Controls.Add(this.lblCard2Title);
            this.pnlCard2.Controls.Add(this.lblAccCreatedVal);
            this.pnlCard2.Location = new System.Drawing.Point(188, 6);
            this.pnlCard2.Name = "pnlCard2";
            this.pnlCard2.Size = new System.Drawing.Size(168, 64);
            this.pnlCard2.TabIndex = 1;
            // 
            // lblCard2Title
            // 
            this.lblCard2Title.Appearance.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblCard2Title.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(134)))), ((int)(((byte)(75)))));
            this.lblCard2Title.Appearance.Options.UseFont = true;
            this.lblCard2Title.Appearance.Options.UseForeColor = true;
            this.lblCard2Title.Location = new System.Drawing.Point(6, 6);
            this.lblCard2Title.Name = "lblCard2Title";
            this.lblCard2Title.Size = new System.Drawing.Size(116, 17);
            this.lblCard2Title.TabIndex = 0;
            this.lblCard2Title.Text = "ĐÃ CÓ TÀI KHOẢN";
            // 
            // lblAccCreatedVal
            // 
            this.lblAccCreatedVal.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblAccCreatedVal.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(134)))), ((int)(((byte)(75)))));
            this.lblAccCreatedVal.Appearance.Options.UseFont = true;
            this.lblAccCreatedVal.Appearance.Options.UseForeColor = true;
            this.lblAccCreatedVal.Location = new System.Drawing.Point(8, 24);
            this.lblAccCreatedVal.Name = "lblAccCreatedVal";
            this.lblAccCreatedVal.Size = new System.Drawing.Size(16, 37);
            this.lblAccCreatedVal.TabIndex = 1;
            this.lblAccCreatedVal.Text = "0";
            // 
            // pnlCard3
            // 
            this.pnlCard3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(243)))), ((int)(((byte)(235)))));
            this.pnlCard3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCard3.Controls.Add(this.lblCard3Title);
            this.pnlCard3.Controls.Add(this.lblNoAccVal);
            this.pnlCard3.Location = new System.Drawing.Point(364, 6);
            this.pnlCard3.Name = "pnlCard3";
            this.pnlCard3.Size = new System.Drawing.Size(168, 64);
            this.pnlCard3.TabIndex = 2;
            // 
            // lblCard3Title
            // 
            this.lblCard3Title.Appearance.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblCard3Title.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(194)))), ((int)(((byte)(85)))), ((int)(((byte)(20)))));
            this.lblCard3Title.Appearance.Options.UseFont = true;
            this.lblCard3Title.Appearance.Options.UseForeColor = true;
            this.lblCard3Title.Location = new System.Drawing.Point(6, 6);
            this.lblCard3Title.Name = "lblCard3Title";
            this.lblCard3Title.Size = new System.Drawing.Size(134, 17);
            this.lblCard3Title.TabIndex = 0;
            this.lblCard3Title.Text = "CHƯA CÓ TÀI KHOẢN";
            // 
            // lblNoAccVal
            // 
            this.lblNoAccVal.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblNoAccVal.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(194)))), ((int)(((byte)(85)))), ((int)(((byte)(20)))));
            this.lblNoAccVal.Appearance.Options.UseFont = true;
            this.lblNoAccVal.Appearance.Options.UseForeColor = true;
            this.lblNoAccVal.Location = new System.Drawing.Point(8, 24);
            this.lblNoAccVal.Name = "lblNoAccVal";
            this.lblNoAccVal.Size = new System.Drawing.Size(16, 37);
            this.lblNoAccVal.TabIndex = 1;
            this.lblNoAccVal.Text = "0";
            // 
            // pnlCard4
            // 
            this.pnlCard4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.pnlCard4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCard4.Controls.Add(this.lblCard4Title);
            this.pnlCard4.Controls.Add(this.lblMobOnVal);
            this.pnlCard4.Location = new System.Drawing.Point(540, 6);
            this.pnlCard4.Name = "pnlCard4";
            this.pnlCard4.Size = new System.Drawing.Size(168, 64);
            this.pnlCard4.TabIndex = 3;
            // 
            // lblCard4Title
            // 
            this.lblCard4Title.Appearance.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblCard4Title.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(140)))), ((int)(((byte)(150)))));
            this.lblCard4Title.Appearance.Options.UseFont = true;
            this.lblCard4Title.Appearance.Options.UseForeColor = true;
            this.lblCard4Title.Location = new System.Drawing.Point(6, 6);
            this.lblCard4Title.Name = "lblCard4Title";
            this.lblCard4Title.Size = new System.Drawing.Size(104, 17);
            this.lblCard4Title.TabIndex = 0;
            this.lblCard4Title.Text = "MOBILE: ĐÃ BẬT";
            // 
            // lblMobOnVal
            // 
            this.lblMobOnVal.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblMobOnVal.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(14)))), ((int)(((byte)(140)))), ((int)(((byte)(150)))));
            this.lblMobOnVal.Appearance.Options.UseFont = true;
            this.lblMobOnVal.Appearance.Options.UseForeColor = true;
            this.lblMobOnVal.Location = new System.Drawing.Point(8, 24);
            this.lblMobOnVal.Name = "lblMobOnVal";
            this.lblMobOnVal.Size = new System.Drawing.Size(16, 37);
            this.lblMobOnVal.TabIndex = 1;
            this.lblMobOnVal.Text = "0";
            // 
            // pnlCard5
            // 
            this.pnlCard5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.pnlCard5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCard5.Controls.Add(this.lblCard5Title);
            this.pnlCard5.Controls.Add(this.lblMobOffVal);
            this.pnlCard5.Location = new System.Drawing.Point(716, 6);
            this.pnlCard5.Name = "pnlCard5";
            this.pnlCard5.Size = new System.Drawing.Size(168, 64);
            this.pnlCard5.TabIndex = 4;
            // 
            // lblCard5Title
            // 
            this.lblCard5Title.Appearance.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblCard5Title.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this.lblCard5Title.Appearance.Options.UseFont = true;
            this.lblCard5Title.Appearance.Options.UseForeColor = true;
            this.lblCard5Title.Location = new System.Drawing.Point(6, 6);
            this.lblCard5Title.Name = "lblCard5Title";
            this.lblCard5Title.Size = new System.Drawing.Size(123, 17);
            this.lblCard5Title.TabIndex = 0;
            this.lblCard5Title.Text = "MOBILE: ĐANG TẮT";
            // 
            // lblMobOffVal
            // 
            this.lblMobOffVal.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblMobOffVal.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(110)))), ((int)(((byte)(110)))));
            this.lblMobOffVal.Appearance.Options.UseFont = true;
            this.lblMobOffVal.Appearance.Options.UseForeColor = true;
            this.lblMobOffVal.Location = new System.Drawing.Point(8, 24);
            this.lblMobOffVal.Name = "lblMobOffVal";
            this.lblMobOffVal.Size = new System.Drawing.Size(16, 37);
            this.lblMobOffVal.TabIndex = 1;
            this.lblMobOffVal.Text = "0";
            // 
            // pnlCard6
            // 
            this.pnlCard6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(238)))), ((int)(((byte)(238)))));
            this.pnlCard6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCard6.Controls.Add(this.lblCard6Title);
            this.pnlCard6.Controls.Add(this.lblLockedVal);
            this.pnlCard6.Location = new System.Drawing.Point(892, 6);
            this.pnlCard6.Name = "pnlCard6";
            this.pnlCard6.Size = new System.Drawing.Size(168, 64);
            this.pnlCard6.TabIndex = 5;
            // 
            // lblCard6Title
            // 
            this.lblCard6Title.Appearance.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblCard6Title.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblCard6Title.Appearance.Options.UseFont = true;
            this.lblCard6Title.Appearance.Options.UseForeColor = true;
            this.lblCard6Title.Location = new System.Drawing.Point(6, 6);
            this.lblCard6Title.Name = "lblCard6Title";
            this.lblCard6Title.Size = new System.Drawing.Size(86, 17);
            this.lblCard6Title.TabIndex = 0;
            this.lblCard6Title.Text = "BỊ TẠM KHÓA";
            // 
            // lblLockedVal
            // 
            this.lblLockedVal.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblLockedVal.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.lblLockedVal.Appearance.Options.UseFont = true;
            this.lblLockedVal.Appearance.Options.UseForeColor = true;
            this.lblLockedVal.Location = new System.Drawing.Point(8, 24);
            this.lblLockedVal.Name = "lblLockedVal";
            this.lblLockedVal.Size = new System.Drawing.Size(16, 37);
            this.lblLockedVal.TabIndex = 1;
            this.lblLockedVal.Text = "0";
            // 
            // pnlCard7
            // 
            this.pnlCard7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(250)))), ((int)(((byte)(230)))));
            this.pnlCard7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCard7.Controls.Add(this.lblCard7Title);
            this.pnlCard7.Controls.Add(this.lblSysVal);
            this.pnlCard7.Location = new System.Drawing.Point(1068, 6);
            this.pnlCard7.Name = "pnlCard7";
            this.pnlCard7.Size = new System.Drawing.Size(168, 64);
            this.pnlCard7.TabIndex = 6;
            // 
            // lblCard7Title
            // 
            this.lblCard7Title.Appearance.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblCard7Title.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(120)))), ((int)(((byte)(10)))));
            this.lblCard7Title.Appearance.Options.UseFont = true;
            this.lblCard7Title.Appearance.Options.UseForeColor = true;
            this.lblCard7Title.Location = new System.Drawing.Point(6, 6);
            this.lblCard7Title.Name = "lblCard7Title";
            this.lblCard7Title.Size = new System.Drawing.Size(143, 17);
            this.lblCard7Title.TabIndex = 0;
            this.lblCard7Title.Text = "TÀI KHOẢN HỆ THỐNG";
            // 
            // lblSysVal
            // 
            this.lblSysVal.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblSysVal.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(120)))), ((int)(((byte)(10)))));
            this.lblSysVal.Appearance.Options.UseFont = true;
            this.lblSysVal.Appearance.Options.UseForeColor = true;
            this.lblSysVal.Location = new System.Drawing.Point(8, 24);
            this.lblSysVal.Name = "lblSysVal";
            this.lblSysVal.Size = new System.Drawing.Size(16, 37);
            this.lblSysVal.TabIndex = 1;
            this.lblSysVal.Text = "0";
            // 
            // tabMain
            // 
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 124);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedTabPage = this.tabDanhSach;
            this.tabMain.Size = new System.Drawing.Size(1325, 717);
            this.tabMain.TabIndex = 0;
            this.tabMain.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.tabDanhSach,
            this.tabChiTiet,
            this.tabCapPhatHangLoat});
            this.tabMain.SelectedPageChanged += new DevExpress.XtraTab.TabPageChangedEventHandler(this.tabMain_SelectedPageChanged);
            // 
            // tabDanhSach
            // 
            this.tabDanhSach.Controls.Add(this.gcUsers);
            this.tabDanhSach.Controls.Add(this.pnlActions);
            this.tabDanhSach.Controls.Add(this.pnlFilter);
            this.tabDanhSach.Name = "tabDanhSach";
            this.tabDanhSach.Size = new System.Drawing.Size(1323, 687);
            this.tabDanhSach.Text = "📋 Danh Sách Tài Khoản";
            // 
            // gcUsers
            // 
            this.gcUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcUsers.Location = new System.Drawing.Point(0, 169);
            this.gcUsers.MainView = this.gvUsers;
            this.gcUsers.Name = "gcUsers";
            this.gcUsers.Size = new System.Drawing.Size(1654, 401);
            this.gcUsers.TabIndex = 0;
            this.gcUsers.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvUsers});
            // 
            // gvUsers
            // 
            this.gvUsers.GridControl = this.gcUsers;
            this.gvUsers.Name = "gvUsers";
            this.gvUsers.OptionsBehavior.Editable = false;
            this.gvUsers.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gvUsers.OptionsView.ShowGroupPanel = false;
            this.gvUsers.DoubleClick += new System.EventHandler(this.gvUsers_DoubleClick);
            // 
            // pnlActions
            // 
            this.pnlActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.pnlActions.Controls.Add(this.btnThem);
            this.pnlActions.Controls.Add(this.btnSua);
            this.pnlActions.Controls.Add(this.btnKhoaMoKhoa);
            this.pnlActions.Controls.Add(this.btnResetMatKhau);
            this.pnlActions.Controls.Add(this.btnToggleMobile);
            this.pnlActions.Controls.Add(this.btnLienKetNV);
            this.pnlActions.Controls.Add(this.btnChuyenSangCapPhat);
            this.pnlActions.Controls.Add(this.btnXuatExcel);
            this.pnlActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlActions.Location = new System.Drawing.Point(0, 628);
            this.pnlActions.Name = "pnlActions";
            this.pnlActions.Padding = new System.Windows.Forms.Padding(8);
            this.pnlActions.Size = new System.Drawing.Size(1654, 231);
            this.pnlActions.TabIndex = 1;
            // 
            // btnThem
            // 
            this.btnThem.Location = new System.Drawing.Point(12, 9);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(115, 30);
            this.btnThem.TabIndex = 0;
            this.btnThem.Text = "Thêm Tài Khoản";
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            // 
            // btnSua
            // 
            this.btnSua.Location = new System.Drawing.Point(133, 9);
            this.btnSua.Name = "btnSua";
            this.btnSua.Size = new System.Drawing.Size(95, 30);
            this.btnSua.TabIndex = 1;
            this.btnSua.Text = "Xem / Sửa";
            this.btnSua.Click += new System.EventHandler(this.btnSua_Click);
            // 
            // btnKhoaMoKhoa
            // 
            this.btnKhoaMoKhoa.Location = new System.Drawing.Point(234, 9);
            this.btnKhoaMoKhoa.Name = "btnKhoaMoKhoa";
            this.btnKhoaMoKhoa.Size = new System.Drawing.Size(115, 30);
            this.btnKhoaMoKhoa.TabIndex = 2;
            this.btnKhoaMoKhoa.Text = "Khóa / Mở Khóa";
            this.btnKhoaMoKhoa.Click += new System.EventHandler(this.btnKhoaMoKhoa_Click);
            // 
            // btnResetMatKhau
            // 
            this.btnResetMatKhau.Location = new System.Drawing.Point(355, 9);
            this.btnResetMatKhau.Name = "btnResetMatKhau";
            this.btnResetMatKhau.Size = new System.Drawing.Size(105, 30);
            this.btnResetMatKhau.TabIndex = 3;
            this.btnResetMatKhau.Text = "Đổi Mật Khẩu";
            this.btnResetMatKhau.Click += new System.EventHandler(this.btnResetMatKhau_Click);
            // 
            // btnToggleMobile
            // 
            this.btnToggleMobile.Location = new System.Drawing.Point(466, 9);
            this.btnToggleMobile.Name = "btnToggleMobile";
            this.btnToggleMobile.Size = new System.Drawing.Size(110, 30);
            this.btnToggleMobile.TabIndex = 4;
            this.btnToggleMobile.Text = "Bật/Tắt Mobile";
            this.btnToggleMobile.Click += new System.EventHandler(this.btnToggleMobile_Click);
            // 
            // btnLienKetNV
            // 
            this.btnLienKetNV.Location = new System.Drawing.Point(582, 9);
            this.btnLienKetNV.Name = "btnLienKetNV";
            this.btnLienKetNV.Size = new System.Drawing.Size(115, 30);
            this.btnLienKetNV.TabIndex = 5;
            this.btnLienKetNV.Text = "Liên Kết 1:1 NV";
            this.btnLienKetNV.Click += new System.EventHandler(this.btnLienKetNV_Click);
            // 
            // btnChuyenSangCapPhat
            // 
            this.btnChuyenSangCapPhat.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnChuyenSangCapPhat.Appearance.ForeColor = System.Drawing.Color.Purple;
            this.btnChuyenSangCapPhat.Appearance.Options.UseFont = true;
            this.btnChuyenSangCapPhat.Appearance.Options.UseForeColor = true;
            this.btnChuyenSangCapPhat.Location = new System.Drawing.Point(703, 9);
            this.btnChuyenSangCapPhat.Name = "btnChuyenSangCapPhat";
            this.btnChuyenSangCapPhat.Size = new System.Drawing.Size(166, 30);
            this.btnChuyenSangCapPhat.TabIndex = 6;
            this.btnChuyenSangCapPhat.Text = "⚡ Cấp Phát Hàng Loạt";
            this.btnChuyenSangCapPhat.Click += new System.EventHandler(this.btnChuyenSangCapPhat_Click);
            // 
            // btnXuatExcel
            // 
            this.btnXuatExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnXuatExcel.Location = new System.Drawing.Point(4727, 18);
            this.btnXuatExcel.Name = "btnXuatExcel";
            this.btnXuatExcel.Size = new System.Drawing.Size(90, 30);
            this.btnXuatExcel.TabIndex = 7;
            this.btnXuatExcel.Text = "Xuất Excel";
            this.btnXuatExcel.Click += new System.EventHandler(this.btnXuatExcel_Click);
            // 
            // pnlFilter
            // 
            this.pnlFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.pnlFilter.Controls.Add(this.lblSearch);
            this.pnlFilter.Controls.Add(this.txtSearch);
            this.pnlFilter.Controls.Add(this.lblDept);
            this.pnlFilter.Controls.Add(this.cboPhongBan);
            this.pnlFilter.Controls.Add(this.lblStatus);
            this.pnlFilter.Controls.Add(this.cboTrangThai);
            this.pnlFilter.Controls.Add(this.lblMob);
            this.pnlFilter.Controls.Add(this.cboMobileFilter);
            this.pnlFilter.Controls.Add(this.btnLoc);
            this.pnlFilter.Controls.Add(this.btnXoaLoc);
            this.pnlFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilter.Location = new System.Drawing.Point(0, 0);
            this.pnlFilter.Name = "pnlFilter";
            this.pnlFilter.Padding = new System.Windows.Forms.Padding(8);
            this.pnlFilter.Size = new System.Drawing.Size(1654, 135);
            this.pnlFilter.TabIndex = 2;
            // 
            // lblSearch
            // 
            this.lblSearch.Location = new System.Drawing.Point(12, 14);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(58, 16);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Tìm kiếm:";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(72, 10);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Properties.NullValuePrompt = "Mã NV, Họ tên, Username...";
            this.txtSearch.Size = new System.Drawing.Size(180, 22);
            this.txtSearch.TabIndex = 1;
            // 
            // lblDept
            // 
            this.lblDept.Location = new System.Drawing.Point(264, 14);
            this.lblDept.Name = "lblDept";
            this.lblDept.Size = new System.Drawing.Size(65, 16);
            this.lblDept.TabIndex = 2;
            this.lblDept.Text = "Phòng ban:";
            // 
            // cboPhongBan
            // 
            this.cboPhongBan.Location = new System.Drawing.Point(334, 10);
            this.cboPhongBan.Name = "cboPhongBan";
            this.cboPhongBan.Size = new System.Drawing.Size(160, 22);
            this.cboPhongBan.TabIndex = 3;
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(504, 14);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(64, 16);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Trạng thái:";
            // 
            // cboTrangThai
            // 
            this.cboTrangThai.EditValue = "Tất cả trạng thái";
            this.cboTrangThai.Location = new System.Drawing.Point(568, 10);
            this.cboTrangThai.Name = "cboTrangThai";
            this.cboTrangThai.Properties.Items.AddRange(new object[] {
            "Tất cả trạng thái",
            "Đang hoạt động",
            "Bị tạm khóa",
            "Đã có tài khoản",
            "Chưa có tài khoản"});
            this.cboTrangThai.Size = new System.Drawing.Size(140, 22);
            this.cboTrangThai.TabIndex = 5;
            // 
            // lblMob
            // 
            this.lblMob.Location = new System.Drawing.Point(718, 14);
            this.lblMob.Name = "lblMob";
            this.lblMob.Size = new System.Drawing.Size(42, 16);
            this.lblMob.TabIndex = 6;
            this.lblMob.Text = "Mobile:";
            // 
            // cboMobileFilter
            // 
            this.cboMobileFilter.EditValue = "Tất cả Mobile";
            this.cboMobileFilter.Location = new System.Drawing.Point(768, 10);
            this.cboMobileFilter.Name = "cboMobileFilter";
            this.cboMobileFilter.Properties.Items.AddRange(new object[] {
            "Tất cả Mobile",
            "Mobile: Đã bật",
            "Mobile: Đang tắt",
            "Mobile: Bị chặn (ADMIN)"});
            this.cboMobileFilter.Size = new System.Drawing.Size(130, 22);
            this.cboMobileFilter.TabIndex = 7;
            // 
            // btnLoc
            // 
            this.btnLoc.Location = new System.Drawing.Point(908, 10);
            this.btnLoc.Name = "btnLoc";
            this.btnLoc.Size = new System.Drawing.Size(64, 24);
            this.btnLoc.TabIndex = 8;
            this.btnLoc.Text = "Lọc";
            this.btnLoc.Click += new System.EventHandler(this.btnLoc_Click);
            // 
            // btnXoaLoc
            // 
            this.btnXoaLoc.Location = new System.Drawing.Point(978, 10);
            this.btnXoaLoc.Name = "btnXoaLoc";
            this.btnXoaLoc.Size = new System.Drawing.Size(64, 24);
            this.btnXoaLoc.TabIndex = 9;
            this.btnXoaLoc.Text = "Xóa lọc";
            this.btnXoaLoc.Click += new System.EventHandler(this.btnXoaLoc_Click);
            // 
            // tabChiTiet
            // 
            this.tabChiTiet.Controls.Add(this.pnlDetailContent);
            this.tabChiTiet.Name = "tabChiTiet";
            this.tabChiTiet.Size = new System.Drawing.Size(1323, 687);
            this.tabChiTiet.Text = "🔍 Chi Tiết & Liên Kết 1:1";
            // 
            // pnlDetailContent
            // 
            this.pnlDetailContent.AutoScroll = true;
            this.pnlDetailContent.Controls.Add(this.grpAccountInfo);
            this.pnlDetailContent.Controls.Add(this.grpEmployeeInfo);
            this.pnlDetailContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetailContent.Location = new System.Drawing.Point(0, 0);
            this.pnlDetailContent.Name = "pnlDetailContent";
            this.pnlDetailContent.Padding = new System.Windows.Forms.Padding(16);
            this.pnlDetailContent.Size = new System.Drawing.Size(1323, 687);
            this.pnlDetailContent.TabIndex = 0;
            // 
            // grpAccountInfo
            // 
            this.grpAccountInfo.Controls.Add(this.lblDetailId);
            this.grpAccountInfo.Controls.Add(this.txtDetailId);
            this.grpAccountInfo.Controls.Add(this.lblDetailUsername);
            this.grpAccountInfo.Controls.Add(this.txtDetailUsername);
            this.grpAccountInfo.Controls.Add(this.lblDetailFullName);
            this.grpAccountInfo.Controls.Add(this.txtDetailFullName);
            this.grpAccountInfo.Controls.Add(this.lblDetailClientType);
            this.grpAccountInfo.Controls.Add(this.cboDetailClientType);
            this.grpAccountInfo.Controls.Add(this.chkDetailDisabled);
            this.grpAccountInfo.Controls.Add(this.lblDetailMobileBadge);
            this.grpAccountInfo.Controls.Add(this.btnSaveDetail);
            this.grpAccountInfo.Location = new System.Drawing.Point(16, 16);
            this.grpAccountInfo.Name = "grpAccountInfo";
            this.grpAccountInfo.Size = new System.Drawing.Size(560, 340);
            this.grpAccountInfo.TabIndex = 0;
            this.grpAccountInfo.Text = "Thông Tin Tài Khoản";
            // 
            // lblDetailId
            // 
            this.lblDetailId.Location = new System.Drawing.Point(20, 40);
            this.lblDetailId.Name = "lblDetailId";
            this.lblDetailId.Size = new System.Drawing.Size(47, 16);
            this.lblDetailId.TabIndex = 0;
            this.lblDetailId.Text = "ID User:";
            // 
            // txtDetailId
            // 
            this.txtDetailId.Location = new System.Drawing.Point(140, 36);
            this.txtDetailId.Name = "txtDetailId";
            this.txtDetailId.Properties.ReadOnly = true;
            this.txtDetailId.Size = new System.Drawing.Size(390, 22);
            this.txtDetailId.TabIndex = 1;
            // 
            // lblDetailUsername
            // 
            this.lblDetailUsername.Location = new System.Drawing.Point(20, 78);
            this.lblDetailUsername.Name = "lblDetailUsername";
            this.lblDetailUsername.Size = new System.Drawing.Size(91, 16);
            this.lblDetailUsername.TabIndex = 2;
            this.lblDetailUsername.Text = "Tên đăng nhập:";
            // 
            // txtDetailUsername
            // 
            this.txtDetailUsername.Location = new System.Drawing.Point(140, 74);
            this.txtDetailUsername.Name = "txtDetailUsername";
            this.txtDetailUsername.Properties.ReadOnly = true;
            this.txtDetailUsername.Size = new System.Drawing.Size(390, 22);
            this.txtDetailUsername.TabIndex = 3;
            // 
            // lblDetailFullName
            // 
            this.lblDetailFullName.Location = new System.Drawing.Point(20, 116);
            this.lblDetailFullName.Name = "lblDetailFullName";
            this.lblDetailFullName.Size = new System.Drawing.Size(88, 16);
            this.lblDetailFullName.TabIndex = 4;
            this.lblDetailFullName.Text = "Họ tên hiển thị:";
            // 
            // txtDetailFullName
            // 
            this.txtDetailFullName.Location = new System.Drawing.Point(140, 112);
            this.txtDetailFullName.Name = "txtDetailFullName";
            this.txtDetailFullName.Size = new System.Drawing.Size(390, 22);
            this.txtDetailFullName.TabIndex = 5;
            // 
            // lblDetailClientType
            // 
            this.lblDetailClientType.Location = new System.Drawing.Point(20, 154);
            this.lblDetailClientType.Name = "lblDetailClientType";
            this.lblDetailClientType.Size = new System.Drawing.Size(93, 16);
            this.lblDetailClientType.TabIndex = 6;
            this.lblDetailClientType.Text = "Client cho phép:";
            // 
            // cboDetailClientType
            // 
            this.cboDetailClientType.Location = new System.Drawing.Point(140, 150);
            this.cboDetailClientType.Name = "cboDetailClientType";
            this.cboDetailClientType.Properties.Items.AddRange(new object[] {
            "ALL",
            "MOBILE",
            "DESKTOP",
            "WEB",
            "SYSTEM"});
            this.cboDetailClientType.Size = new System.Drawing.Size(390, 22);
            this.cboDetailClientType.TabIndex = 7;
            // 
            // chkDetailDisabled
            // 
            this.chkDetailDisabled.Location = new System.Drawing.Point(140, 188);
            this.chkDetailDisabled.Name = "chkDetailDisabled";
            this.chkDetailDisabled.Properties.Caption = "Khóa tài khoản này";
            this.chkDetailDisabled.Size = new System.Drawing.Size(180, 24);
            this.chkDetailDisabled.TabIndex = 8;
            // 
            // lblDetailMobileBadge
            // 
            this.lblDetailMobileBadge.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDetailMobileBadge.Appearance.Options.UseFont = true;
            this.lblDetailMobileBadge.Location = new System.Drawing.Point(140, 224);
            this.lblDetailMobileBadge.Name = "lblDetailMobileBadge";
            this.lblDetailMobileBadge.Size = new System.Drawing.Size(147, 20);
            this.lblDetailMobileBadge.TabIndex = 9;
            this.lblDetailMobileBadge.Text = "Mobile Access Status";
            // 
            // btnSaveDetail
            // 
            this.btnSaveDetail.Location = new System.Drawing.Point(140, 262);
            this.btnSaveDetail.Name = "btnSaveDetail";
            this.btnSaveDetail.Size = new System.Drawing.Size(220, 36);
            this.btnSaveDetail.TabIndex = 10;
            this.btnSaveDetail.Text = "Lưu Cập Nhật Tài Khoản";
            this.btnSaveDetail.Click += new System.EventHandler(this.btnSaveDetail_Click);
            // 
            // grpEmployeeInfo
            // 
            this.grpEmployeeInfo.Controls.Add(this.lblDetailEmpId);
            this.grpEmployeeInfo.Controls.Add(this.txtDetailEmpId);
            this.grpEmployeeInfo.Controls.Add(this.lblDetailEmpCode);
            this.grpEmployeeInfo.Controls.Add(this.txtDetailEmpCode);
            this.grpEmployeeInfo.Controls.Add(this.lblDetailEmpName);
            this.grpEmployeeInfo.Controls.Add(this.txtDetailEmpName);
            this.grpEmployeeInfo.Controls.Add(this.lblDetailDept);
            this.grpEmployeeInfo.Controls.Add(this.txtDetailDept);
            this.grpEmployeeInfo.Controls.Add(this.lblDetailPos);
            this.grpEmployeeInfo.Controls.Add(this.txtDetailPos);
            this.grpEmployeeInfo.Location = new System.Drawing.Point(596, 16);
            this.grpEmployeeInfo.Name = "grpEmployeeInfo";
            this.grpEmployeeInfo.Size = new System.Drawing.Size(560, 340);
            this.grpEmployeeInfo.TabIndex = 1;
            this.grpEmployeeInfo.Text = "Thông Tin Nhân Sự Liên Kết (1:1)";
            // 
            // lblDetailEmpId
            // 
            this.lblDetailEmpId.Location = new System.Drawing.Point(20, 40);
            this.lblDetailEmpId.Name = "lblDetailEmpId";
            this.lblDetailEmpId.Size = new System.Drawing.Size(90, 16);
            this.lblDetailEmpId.TabIndex = 0;
            this.lblDetailEmpId.Text = "Mã NV (MANV):";
            // 
            // txtDetailEmpId
            // 
            this.txtDetailEmpId.Location = new System.Drawing.Point(140, 36);
            this.txtDetailEmpId.Name = "txtDetailEmpId";
            this.txtDetailEmpId.Properties.ReadOnly = true;
            this.txtDetailEmpId.Size = new System.Drawing.Size(390, 22);
            this.txtDetailEmpId.TabIndex = 1;
            // 
            // lblDetailEmpCode
            // 
            this.lblDetailEmpCode.Location = new System.Drawing.Point(20, 78);
            this.lblDetailEmpCode.Name = "lblDetailEmpCode";
            this.lblDetailEmpCode.Size = new System.Drawing.Size(72, 16);
            this.lblDetailEmpCode.TabIndex = 2;
            this.lblDetailEmpCode.Text = "Mã nhân sự:";
            // 
            // txtDetailEmpCode
            // 
            this.txtDetailEmpCode.Location = new System.Drawing.Point(140, 74);
            this.txtDetailEmpCode.Name = "txtDetailEmpCode";
            this.txtDetailEmpCode.Properties.ReadOnly = true;
            this.txtDetailEmpCode.Size = new System.Drawing.Size(390, 22);
            this.txtDetailEmpCode.TabIndex = 3;
            // 
            // lblDetailEmpName
            // 
            this.lblDetailEmpName.Location = new System.Drawing.Point(20, 116);
            this.lblDetailEmpName.Name = "lblDetailEmpName";
            this.lblDetailEmpName.Size = new System.Drawing.Size(101, 16);
            this.lblDetailEmpName.TabIndex = 4;
            this.lblDetailEmpName.Text = "Họ tên nhân viên:";
            // 
            // txtDetailEmpName
            // 
            this.txtDetailEmpName.Location = new System.Drawing.Point(140, 112);
            this.txtDetailEmpName.Name = "txtDetailEmpName";
            this.txtDetailEmpName.Properties.ReadOnly = true;
            this.txtDetailEmpName.Size = new System.Drawing.Size(390, 22);
            this.txtDetailEmpName.TabIndex = 5;
            // 
            // lblDetailDept
            // 
            this.lblDetailDept.Location = new System.Drawing.Point(20, 154);
            this.lblDetailDept.Name = "lblDetailDept";
            this.lblDetailDept.Size = new System.Drawing.Size(65, 16);
            this.lblDetailDept.TabIndex = 6;
            this.lblDetailDept.Text = "Phòng ban:";
            // 
            // txtDetailDept
            // 
            this.txtDetailDept.Location = new System.Drawing.Point(140, 150);
            this.txtDetailDept.Name = "txtDetailDept";
            this.txtDetailDept.Properties.ReadOnly = true;
            this.txtDetailDept.Size = new System.Drawing.Size(390, 22);
            this.txtDetailDept.TabIndex = 7;
            // 
            // lblDetailPos
            // 
            this.lblDetailPos.Location = new System.Drawing.Point(20, 192);
            this.lblDetailPos.Name = "lblDetailPos";
            this.lblDetailPos.Size = new System.Drawing.Size(51, 16);
            this.lblDetailPos.TabIndex = 8;
            this.lblDetailPos.Text = "Chức vụ:";
            // 
            // txtDetailPos
            // 
            this.txtDetailPos.Location = new System.Drawing.Point(140, 188);
            this.txtDetailPos.Name = "txtDetailPos";
            this.txtDetailPos.Properties.ReadOnly = true;
            this.txtDetailPos.Size = new System.Drawing.Size(390, 22);
            this.txtDetailPos.TabIndex = 9;
            // 
            // tabCapPhatHangLoat
            // 
            this.tabCapPhatHangLoat.Controls.Add(this.tabBulkSub);
            this.tabCapPhatHangLoat.Controls.Add(this.pnlBulkActions);
            this.tabCapPhatHangLoat.Controls.Add(this.grpBulkFilter);
            this.tabCapPhatHangLoat.Name = "tabCapPhatHangLoat";
            this.tabCapPhatHangLoat.Size = new System.Drawing.Size(1323, 687);
            this.tabCapPhatHangLoat.Text = "⚡ Cấp Tài Khoản Hàng Loạt";
            // 
            // tabBulkSub
            // 
            this.tabBulkSub.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabBulkSub.Location = new System.Drawing.Point(0, 131);
            this.tabBulkSub.Name = "tabBulkSub";
            this.tabBulkSub.SelectedTabPage = this.tabSubCandidates;
            this.tabBulkSub.Size = new System.Drawing.Size(1323, 481);
            this.tabBulkSub.TabIndex = 0;
            this.tabBulkSub.TabPages.AddRange(new DevExpress.XtraTab.XtraTabPage[] {
            this.tabSubCandidates,
            this.tabSubResults});
            // 
            // tabSubCandidates
            // 
            this.tabSubCandidates.Controls.Add(this.gcCandidates);
            this.tabSubCandidates.Name = "tabSubCandidates";
            this.tabSubCandidates.Size = new System.Drawing.Size(1321, 451);
            this.tabSubCandidates.Text = "👥 1. Danh Sách Ứng Viên Cấp Phát";
            // 
            // gcCandidates
            // 
            this.gcCandidates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcCandidates.Location = new System.Drawing.Point(0, 0);
            this.gcCandidates.MainView = this.gvCandidates;
            this.gcCandidates.Name = "gcCandidates";
            this.gcCandidates.Size = new System.Drawing.Size(1321, 451);
            this.gcCandidates.TabIndex = 0;
            this.gcCandidates.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvCandidates});
            // 
            // gvCandidates
            // 
            this.gvCandidates.GridControl = this.gcCandidates;
            this.gvCandidates.Name = "gvCandidates";
            this.gvCandidates.OptionsView.ShowGroupPanel = false;
            // 
            // tabSubResults
            // 
            this.tabSubResults.Controls.Add(this.gcBulkResults);
            this.tabSubResults.Name = "tabSubResults";
            this.tabSubResults.Size = new System.Drawing.Size(1321, 466);
            this.tabSubResults.Text = "📋 2. Kết Quả Cấp Phát";
            // 
            // gcBulkResults
            // 
            this.gcBulkResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcBulkResults.Location = new System.Drawing.Point(0, 0);
            this.gcBulkResults.MainView = this.gvBulkResults;
            this.gcBulkResults.Name = "gcBulkResults";
            this.gcBulkResults.Size = new System.Drawing.Size(1321, 466);
            this.gcBulkResults.TabIndex = 0;
            this.gcBulkResults.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvBulkResults});
            // 
            // gvBulkResults
            // 
            this.gvBulkResults.GridControl = this.gcBulkResults;
            this.gvBulkResults.Name = "gvBulkResults";
            this.gvBulkResults.OptionsBehavior.Editable = false;
            this.gvBulkResults.OptionsView.ShowGroupPanel = false;
            // 
            // pnlBulkActions
            // 
            this.pnlBulkActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(244)))), ((int)(((byte)(250)))));
            this.pnlBulkActions.Controls.Add(this.btnBulkSelectAll);
            this.pnlBulkActions.Controls.Add(this.btnBulkDeselectAll);
            this.pnlBulkActions.Controls.Add(this.btnExecuteBulk);
            this.pnlBulkActions.Controls.Add(this.lblBulkResultsTitle);
            this.pnlBulkActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBulkActions.Location = new System.Drawing.Point(0, 784);
            this.pnlBulkActions.Name = "pnlBulkActions";
            this.pnlBulkActions.Padding = new System.Windows.Forms.Padding(8);
            this.pnlBulkActions.Size = new System.Drawing.Size(1654, 75);
            this.pnlBulkActions.TabIndex = 1;
            // 
            // btnBulkSelectAll
            // 
            this.btnBulkSelectAll.Location = new System.Drawing.Point(12, 10);
            this.btnBulkSelectAll.Name = "btnBulkSelectAll";
            this.btnBulkSelectAll.Size = new System.Drawing.Size(140, 28);
            this.btnBulkSelectAll.TabIndex = 0;
            this.btnBulkSelectAll.Text = "Chọn Tất Cả Hợp Lệ";
            this.btnBulkSelectAll.Click += new System.EventHandler(this.btnBulkSelectAll_Click);
            // 
            // btnBulkDeselectAll
            // 
            this.btnBulkDeselectAll.Location = new System.Drawing.Point(160, 10);
            this.btnBulkDeselectAll.Name = "btnBulkDeselectAll";
            this.btnBulkDeselectAll.Size = new System.Drawing.Size(120, 28);
            this.btnBulkDeselectAll.TabIndex = 1;
            this.btnBulkDeselectAll.Text = "Bỏ Chọn Tất Cả";
            this.btnBulkDeselectAll.Click += new System.EventHandler(this.btnBulkDeselectAll_Click);
            // 
            // btnExecuteBulk
            // 
            this.btnExecuteBulk.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExecuteBulk.Appearance.ForeColor = System.Drawing.Color.DarkGreen;
            this.btnExecuteBulk.Appearance.Options.UseFont = true;
            this.btnExecuteBulk.Appearance.Options.UseForeColor = true;
            this.btnExecuteBulk.Location = new System.Drawing.Point(290, 10);
            this.btnExecuteBulk.Name = "btnExecuteBulk";
            this.btnExecuteBulk.Size = new System.Drawing.Size(170, 28);
            this.btnExecuteBulk.TabIndex = 2;
            this.btnExecuteBulk.Text = "⚡ Thực Thi Cấp Phát";
            this.btnExecuteBulk.Click += new System.EventHandler(this.btnExecuteBulk_Click);
            // 
            // lblBulkResultsTitle
            // 
            this.lblBulkResultsTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblBulkResultsTitle.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.lblBulkResultsTitle.Appearance.Options.UseFont = true;
            this.lblBulkResultsTitle.Appearance.Options.UseForeColor = true;
            this.lblBulkResultsTitle.Location = new System.Drawing.Point(480, 16);
            this.lblBulkResultsTitle.Name = "lblBulkResultsTitle";
            this.lblBulkResultsTitle.Size = new System.Drawing.Size(399, 20);
            this.lblBulkResultsTitle.TabIndex = 3;
            this.lblBulkResultsTitle.Text = "* Chỉ các nhân sự được tích chọn [Hợp Lệ] mới được cấp phát.";
            // 
            // grpBulkFilter
            // 
            this.grpBulkFilter.Controls.Add(this.lblBulkDept);
            this.grpBulkFilter.Controls.Add(this.cboBulkDept);
            this.grpBulkFilter.Controls.Add(this.chkBulkOnlyWithoutAcc);
            this.grpBulkFilter.Controls.Add(this.chkBulkEnableMobile);
            this.grpBulkFilter.Controls.Add(this.lblBulkDefaultPass);
            this.grpBulkFilter.Controls.Add(this.txtBulkDefaultPass);
            this.grpBulkFilter.Controls.Add(this.btnBulkRefresh);
            this.grpBulkFilter.Controls.Add(this.pnlBulkMetrics);
            this.grpBulkFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpBulkFilter.Location = new System.Drawing.Point(0, 0);
            this.grpBulkFilter.Name = "grpBulkFilter";
            this.grpBulkFilter.Size = new System.Drawing.Size(1654, 131);
            this.grpBulkFilter.TabIndex = 2;
            this.grpBulkFilter.Text = "Bộ Lọc & Tiêu Chí Cấp Phát";
            // 
            // lblBulkDept
            // 
            this.lblBulkDept.Location = new System.Drawing.Point(14, 35);
            this.lblBulkDept.Name = "lblBulkDept";
            this.lblBulkDept.Size = new System.Drawing.Size(65, 16);
            this.lblBulkDept.TabIndex = 0;
            this.lblBulkDept.Text = "Phòng ban:";
            // 
            // cboBulkDept
            // 
            this.cboBulkDept.Location = new System.Drawing.Point(84, 31);
            this.cboBulkDept.Name = "cboBulkDept";
            this.cboBulkDept.Size = new System.Drawing.Size(185, 22);
            this.cboBulkDept.TabIndex = 1;
            // 
            // chkBulkOnlyWithoutAcc
            // 
            this.chkBulkOnlyWithoutAcc.EditValue = true;
            this.chkBulkOnlyWithoutAcc.Location = new System.Drawing.Point(285, 31);
            this.chkBulkOnlyWithoutAcc.Name = "chkBulkOnlyWithoutAcc";
            this.chkBulkOnlyWithoutAcc.Properties.Caption = "Chỉ nhân sự chưa có tài khoản";
            this.chkBulkOnlyWithoutAcc.Size = new System.Drawing.Size(232, 24);
            this.chkBulkOnlyWithoutAcc.TabIndex = 2;
            // 
            // chkBulkEnableMobile
            // 
            this.chkBulkEnableMobile.EditValue = true;
            this.chkBulkEnableMobile.Location = new System.Drawing.Point(546, 31);
            this.chkBulkEnableMobile.Name = "chkBulkEnableMobile";
            this.chkBulkEnableMobile.Properties.Caption = "Tự động kích hoạt Mobile";
            this.chkBulkEnableMobile.Size = new System.Drawing.Size(190, 24);
            this.chkBulkEnableMobile.TabIndex = 3;
            // 
            // lblBulkDefaultPass
            // 
            this.lblBulkDefaultPass.Location = new System.Drawing.Point(769, 36);
            this.lblBulkDefaultPass.Name = "lblBulkDefaultPass";
            this.lblBulkDefaultPass.Size = new System.Drawing.Size(113, 16);
            this.lblBulkDefaultPass.TabIndex = 4;
            this.lblBulkDefaultPass.Text = "Mật khẩu mặc định:";
            // 
            // txtBulkDefaultPass
            // 
            this.txtBulkDefaultPass.EditValue = "123456";
            this.txtBulkDefaultPass.Location = new System.Drawing.Point(891, 32);
            this.txtBulkDefaultPass.Name = "txtBulkDefaultPass";
            this.txtBulkDefaultPass.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 9.5F);
            this.txtBulkDefaultPass.Properties.Appearance.Options.UseFont = true;
            this.txtBulkDefaultPass.Size = new System.Drawing.Size(96, 26);
            this.txtBulkDefaultPass.TabIndex = 5;
            this.txtBulkDefaultPass.EditValueChanged += new System.EventHandler(this.txtBulkDefaultPass_EditValueChanged);
            // 
            // btnBulkRefresh
            // 
            this.btnBulkRefresh.Location = new System.Drawing.Point(1010, 32);
            this.btnBulkRefresh.Name = "btnBulkRefresh";
            this.btnBulkRefresh.Size = new System.Drawing.Size(110, 28);
            this.btnBulkRefresh.TabIndex = 6;
            this.btnBulkRefresh.Text = "🔄 Tải Lại";
            this.btnBulkRefresh.Click += new System.EventHandler(this.btnBulkRefresh_Click);
            // 
            // pnlBulkMetrics
            // 
            this.pnlBulkMetrics.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.pnlBulkMetrics.Controls.Add(this.lblBulkEligible);
            this.pnlBulkMetrics.Controls.Add(this.lblBulkReady);
            this.pnlBulkMetrics.Controls.Add(this.lblBulkExisting);
            this.pnlBulkMetrics.Controls.Add(this.lblBulkSystem);
            this.pnlBulkMetrics.Controls.Add(this.lblBulkInactive);
            this.pnlBulkMetrics.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBulkMetrics.Location = new System.Drawing.Point(2, 61);
            this.pnlBulkMetrics.Name = "pnlBulkMetrics";
            this.pnlBulkMetrics.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
            this.pnlBulkMetrics.Size = new System.Drawing.Size(1650, 68);
            this.pnlBulkMetrics.TabIndex = 7;
            // 
            // lblBulkEligible
            // 
            this.lblBulkEligible.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblBulkEligible.Appearance.Options.UseFont = true;
            this.lblBulkEligible.Location = new System.Drawing.Point(16, 8);
            this.lblBulkEligible.Name = "lblBulkEligible";
            this.lblBulkEligible.Size = new System.Drawing.Size(100, 20);
            this.lblBulkEligible.TabIndex = 0;
            this.lblBulkEligible.Text = "Tổng hợp lệ: 0";
            // 
            // lblBulkReady
            // 
            this.lblBulkReady.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblBulkReady.Appearance.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblBulkReady.Appearance.Options.UseFont = true;
            this.lblBulkReady.Appearance.Options.UseForeColor = true;
            this.lblBulkReady.Location = new System.Drawing.Point(160, 8);
            this.lblBulkReady.Name = "lblBulkReady";
            this.lblBulkReady.Size = new System.Drawing.Size(106, 20);
            this.lblBulkReady.TabIndex = 1;
            this.lblBulkReady.Text = "Sẵn sàng tạo: 0";
            // 
            // lblBulkExisting
            // 
            this.lblBulkExisting.Appearance.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblBulkExisting.Appearance.Options.UseForeColor = true;
            this.lblBulkExisting.Location = new System.Drawing.Point(310, 8);
            this.lblBulkExisting.Name = "lblBulkExisting";
            this.lblBulkExisting.Size = new System.Drawing.Size(68, 16);
            this.lblBulkExisting.TabIndex = 2;
            this.lblBulkExisting.Text = "Đã có TK: 0";
            // 
            // lblBulkSystem
            // 
            this.lblBulkSystem.Appearance.ForeColor = System.Drawing.Color.DarkGoldenrod;
            this.lblBulkSystem.Appearance.Options.UseForeColor = true;
            this.lblBulkSystem.Location = new System.Drawing.Point(430, 8);
            this.lblBulkSystem.Name = "lblBulkSystem";
            this.lblBulkSystem.Size = new System.Drawing.Size(99, 16);
            this.lblBulkSystem.TabIndex = 3;
            this.lblBulkSystem.Text = "Admin/System: 0";
            // 
            // lblBulkInactive
            // 
            this.lblBulkInactive.Appearance.ForeColor = System.Drawing.Color.DarkRed;
            this.lblBulkInactive.Appearance.Options.UseForeColor = true;
            this.lblBulkInactive.Location = new System.Drawing.Point(570, 8);
            this.lblBulkInactive.Name = "lblBulkInactive";
            this.lblBulkInactive.Size = new System.Drawing.Size(83, 16);
            this.lblBulkInactive.TabIndex = 4;
            this.lblBulkInactive.Text = "Đã thôi việc: 0";
            // 
            // FrmQuanLyTaiKhoan
            // 
            this.ClientSize = new System.Drawing.Size(1325, 841);
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.pnlDashboard);
            this.Controls.Add(this.pnlHeader);
            this.Name = "FrmQuanLyTaiKhoan";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hệ thống Quản lý Tài khoản, Phân quyền & Cấp phát";
            this.Load += new System.EventHandler(this.FrmQuanLyTaiKhoan_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlDashboard.ResumeLayout(false);
            this.pnlCard1.ResumeLayout(false);
            this.pnlCard1.PerformLayout();
            this.pnlCard2.ResumeLayout(false);
            this.pnlCard2.PerformLayout();
            this.pnlCard3.ResumeLayout(false);
            this.pnlCard3.PerformLayout();
            this.pnlCard4.ResumeLayout(false);
            this.pnlCard4.PerformLayout();
            this.pnlCard5.ResumeLayout(false);
            this.pnlCard5.PerformLayout();
            this.pnlCard6.ResumeLayout(false);
            this.pnlCard6.PerformLayout();
            this.pnlCard7.ResumeLayout(false);
            this.pnlCard7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabMain)).EndInit();
            this.tabMain.ResumeLayout(false);
            this.tabDanhSach.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcUsers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvUsers)).EndInit();
            this.pnlActions.ResumeLayout(false);
            this.pnlFilter.ResumeLayout(false);
            this.pnlFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSearch.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboPhongBan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTrangThai.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboMobileFilter.Properties)).EndInit();
            this.tabChiTiet.ResumeLayout(false);
            this.pnlDetailContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grpAccountInfo)).EndInit();
            this.grpAccountInfo.ResumeLayout(false);
            this.grpAccountInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailId.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailUsername.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailFullName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDetailClientType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDetailDisabled.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grpEmployeeInfo)).EndInit();
            this.grpEmployeeInfo.ResumeLayout(false);
            this.grpEmployeeInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailEmpId.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailEmpCode.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailEmpName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailDept.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDetailPos.Properties)).EndInit();
            this.tabCapPhatHangLoat.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tabBulkSub)).EndInit();
            this.tabBulkSub.ResumeLayout(false);
            this.tabSubCandidates.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcCandidates)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvCandidates)).EndInit();
            this.tabSubResults.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcBulkResults)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvBulkResults)).EndInit();
            this.pnlBulkActions.ResumeLayout(false);
            this.pnlBulkActions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grpBulkFilter)).EndInit();
            this.grpBulkFilter.ResumeLayout(false);
            this.grpBulkFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboBulkDept.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBulkOnlyWithoutAcc.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBulkEnableMobile.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtBulkDefaultPass.Properties)).EndInit();
            this.pnlBulkMetrics.ResumeLayout(false);
            this.pnlBulkMetrics.PerformLayout();
            this.ResumeLayout(false);

        }

        // Controls Declaration
        private System.Windows.Forms.Panel pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.SimpleButton btnRefreshAll;

        private System.Windows.Forms.Panel pnlDashboard;
        private System.Windows.Forms.Panel pnlCard1;
        private System.Windows.Forms.Panel pnlCard2;
        private System.Windows.Forms.Panel pnlCard3;
        private System.Windows.Forms.Panel pnlCard4;
        private System.Windows.Forms.Panel pnlCard5;
        private System.Windows.Forms.Panel pnlCard6;
        private System.Windows.Forms.Panel pnlCard7;

        private DevExpress.XtraEditors.LabelControl lblCard1Title;
        private DevExpress.XtraEditors.LabelControl lblCard2Title;
        private DevExpress.XtraEditors.LabelControl lblCard3Title;
        private DevExpress.XtraEditors.LabelControl lblCard4Title;
        private DevExpress.XtraEditors.LabelControl lblCard5Title;
        private DevExpress.XtraEditors.LabelControl lblCard6Title;
        private DevExpress.XtraEditors.LabelControl lblCard7Title;

        private DevExpress.XtraEditors.LabelControl lblTotalEmpVal;
        private DevExpress.XtraEditors.LabelControl lblAccCreatedVal;
        private DevExpress.XtraEditors.LabelControl lblNoAccVal;
        private DevExpress.XtraEditors.LabelControl lblMobOnVal;
        private DevExpress.XtraEditors.LabelControl lblMobOffVal;
        private DevExpress.XtraEditors.LabelControl lblLockedVal;
        private DevExpress.XtraEditors.LabelControl lblSysVal;

        private DevExpress.XtraTab.XtraTabControl tabMain;
        private DevExpress.XtraTab.XtraTabPage tabDanhSach;
        private DevExpress.XtraTab.XtraTabPage tabChiTiet;
        private DevExpress.XtraTab.XtraTabPage tabCapPhatHangLoat;

        // Tab 1
        private System.Windows.Forms.Panel pnlFilter;
        private DevExpress.XtraEditors.LabelControl lblSearch;
        private DevExpress.XtraEditors.LabelControl lblDept;
        private DevExpress.XtraEditors.LabelControl lblStatus;
        private DevExpress.XtraEditors.LabelControl lblMob;
        private DevExpress.XtraEditors.TextEdit txtSearch;
        private DevExpress.XtraEditors.LookUpEdit cboPhongBan;
        private DevExpress.XtraEditors.ComboBoxEdit cboTrangThai;
        private DevExpress.XtraEditors.ComboBoxEdit cboMobileFilter;
        private DevExpress.XtraEditors.SimpleButton btnLoc;
        private DevExpress.XtraEditors.SimpleButton btnXoaLoc;
        private DevExpress.XtraGrid.GridControl gcUsers;
        private DevExpress.XtraGrid.Views.Grid.GridView gvUsers;
        private System.Windows.Forms.Panel pnlActions;
        private DevExpress.XtraEditors.SimpleButton btnThem;
        private DevExpress.XtraEditors.SimpleButton btnSua;
        private DevExpress.XtraEditors.SimpleButton btnKhoaMoKhoa;
        private DevExpress.XtraEditors.SimpleButton btnResetMatKhau;
        private DevExpress.XtraEditors.SimpleButton btnToggleMobile;
        private DevExpress.XtraEditors.SimpleButton btnLienKetNV;
        private DevExpress.XtraEditors.SimpleButton btnChuyenSangCapPhat;
        private DevExpress.XtraEditors.SimpleButton btnXuatExcel;

        // Tab 2
        private System.Windows.Forms.Panel pnlDetailContent;
        private DevExpress.XtraEditors.GroupControl grpAccountInfo;
        private DevExpress.XtraEditors.LabelControl lblDetailId;
        private DevExpress.XtraEditors.LabelControl lblDetailUsername;
        private DevExpress.XtraEditors.LabelControl lblDetailFullName;
        private DevExpress.XtraEditors.LabelControl lblDetailClientType;
        private DevExpress.XtraEditors.LabelControl lblDetailMobileBadge;
        private DevExpress.XtraEditors.TextEdit txtDetailId;
        private DevExpress.XtraEditors.TextEdit txtDetailUsername;
        private DevExpress.XtraEditors.TextEdit txtDetailFullName;
        private DevExpress.XtraEditors.CheckEdit chkDetailDisabled;
        private DevExpress.XtraEditors.ComboBoxEdit cboDetailClientType;
        private DevExpress.XtraEditors.SimpleButton btnSaveDetail;

        private DevExpress.XtraEditors.GroupControl grpEmployeeInfo;
        private DevExpress.XtraEditors.LabelControl lblDetailEmpId;
        private DevExpress.XtraEditors.LabelControl lblDetailEmpCode;
        private DevExpress.XtraEditors.LabelControl lblDetailEmpName;
        private DevExpress.XtraEditors.LabelControl lblDetailDept;
        private DevExpress.XtraEditors.LabelControl lblDetailPos;
        private DevExpress.XtraEditors.TextEdit txtDetailEmpId;
        private DevExpress.XtraEditors.TextEdit txtDetailEmpCode;
        private DevExpress.XtraEditors.TextEdit txtDetailEmpName;
        private DevExpress.XtraEditors.TextEdit txtDetailDept;
        private DevExpress.XtraEditors.TextEdit txtDetailPos;

        // Tab 3
        private DevExpress.XtraEditors.GroupControl grpBulkFilter;
        private DevExpress.XtraEditors.LabelControl lblBulkDept;
        private DevExpress.XtraEditors.LabelControl lblBulkDefaultPass;
        private DevExpress.XtraEditors.LookUpEdit cboBulkDept;
        private DevExpress.XtraEditors.CheckEdit chkBulkOnlyWithoutAcc;
        private DevExpress.XtraEditors.CheckEdit chkBulkEnableMobile;
        private DevExpress.XtraEditors.TextEdit txtBulkDefaultPass;
        private DevExpress.XtraEditors.SimpleButton btnBulkRefresh;
        private System.Windows.Forms.Panel pnlBulkMetrics;
        private DevExpress.XtraEditors.LabelControl lblBulkEligible;
        private DevExpress.XtraEditors.LabelControl lblBulkReady;
        private DevExpress.XtraEditors.LabelControl lblBulkExisting;
        private DevExpress.XtraEditors.LabelControl lblBulkSystem;
        private DevExpress.XtraEditors.LabelControl lblBulkInactive;
        private DevExpress.XtraGrid.GridControl gcCandidates;
        private DevExpress.XtraGrid.Views.Grid.GridView gvCandidates;
        private System.Windows.Forms.Panel pnlBulkActions;
        private DevExpress.XtraEditors.SimpleButton btnBulkSelectAll;
        private DevExpress.XtraEditors.SimpleButton btnBulkDeselectAll;
        private DevExpress.XtraEditors.SimpleButton btnExecuteBulk;
        private DevExpress.XtraEditors.LabelControl lblBulkResultsTitle;
        private DevExpress.XtraGrid.GridControl gcBulkResults;
        private DevExpress.XtraGrid.Views.Grid.GridView gvBulkResults;
        private DevExpress.XtraTab.XtraTabControl tabBulkSub;
        private DevExpress.XtraTab.XtraTabPage tabSubCandidates;
        private DevExpress.XtraTab.XtraTabPage tabSubResults;
    }
}
