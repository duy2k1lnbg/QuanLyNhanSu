using System.Threading.Tasks;
using Bu.CLASS_SYSTEM;
using DA;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_NHANSU
{
    public partial class FrmPheDuyetYeuCau : XtraForm
    {
        private List<LeaveRow> _rawLeaves = new List<LeaveRow>();
        private List<AttRow> _rawAtts = new List<AttRow>();
        private List<OtRow> _rawOts = new List<OtRow>();
        private List<AdvanceRow> _rawAdvances = new List<AdvanceRow>();
        private List<ProfileChangeRow> _rawProfiles = new List<ProfileChangeRow>();

        // Dynamic Tab: Tạm ứng lương
        private XtraTabPage tabAdvance;
        private GridControl gcAdvance;
        private GridView gvAdvance;
        private SimpleButton btnApproveAdvance;
        private SimpleButton btnRejectAdvance;
        private SimpleButton btnDisburseAdvance;
        private SimpleButton btnExportAdvance;
        private ComboBoxEdit cboAdvanceStatus;
        private TextEdit txtAdvanceSearch;

        // Dynamic Tab: Thay đổi hồ sơ
        private XtraTabPage tabProfile;
        private GridControl gcProfile;
        private GridView gvProfile;
        private SimpleButton btnApproveProfile;
        private SimpleButton btnRejectProfile;
        private SimpleButton btnViewProfileDiff;
        private SimpleButton btnExportProfile;
        private ComboBoxEdit cboProfileGroup;
        private ComboBoxEdit cboProfileStatus;
        private TextEdit txtProfileSearch;

        public FrmPheDuyetYeuCau()
        {
            InitializeComponent();
        }

        private async void FrmPheDuyetYeuCau_Load(object sender, EventArgs e)
        {
            ApplyApprovalPermissions();
            if (!UserSession.CanView("F_NV_PHEDUYET") && !UserSession.CanView("F_APPROVE_ADVANCE") && !UserSession.CanView("F_APPROVE_PROFILE") && !UserSession.CanView("F_PAYROLL_DISBURSE_ADVANCE")) { Close(); return; }

            InitializeAdditionalTabs();

            SetupGridViewAppearance(gvLeave);
            SetupGridViewAppearance(gvAttendance);
            SetupGridViewAppearance(gvOvertime);
            SetupGridViewAppearance(gvAdvance);
            SetupGridViewAppearance(gvProfile);

            TranslateStatusCombo(cboLeaveStatus);
            TranslateStatusCombo(cboAttStatus);
            TranslateStatusCombo(cboOtStatus);

            cboLeaveStatus.SelectedIndex = 1; // Default PENDING
            cboAttStatus.SelectedIndex = 1;   // Default PENDING
            cboOtStatus.SelectedIndex = 1;    // Default PENDING

            QLyNSu.Functions.TranslationManager.Translate(this);

            await LoadAllRequests();

            if (IsDisposed || Disposing) return;
        }

        private bool CanApproveRequests()
        {
            return UserSession.CanView("F_NV_PHEDUYET") && UserSession.CanEdit("F_NV_PHEDUYET");
        }

        private void ApplyApprovalPermissions()
        {
            bool viewGeneral = UserSession.CanView("F_NV_PHEDUYET");
            bool approveGeneral = CanApproveRequests();
            bool exportGeneral = viewGeneral && UserSession.CanPrint("F_NV_PHEDUYET");

            bool viewAdvance = viewGeneral || UserSession.CanView("F_APPROVE_ADVANCE");
            bool approveAdvance = approveGeneral || UserSession.CanEdit("F_APPROVE_ADVANCE");

            bool canDisburse = UserSession.CanEdit("F_PAYROLL_DISBURSE_ADVANCE");

            bool viewProfile = viewGeneral || UserSession.CanView("F_APPROVE_PROFILE");
            bool approveProfile = approveGeneral || UserSession.CanEdit("F_APPROVE_PROFILE");

            btnRefreshAll.Enabled = viewGeneral || viewAdvance || viewProfile || canDisburse;
            btnApproveLeave.Enabled = btnRejectLeave.Enabled = approveGeneral;
            btnApproveAtt.Enabled = btnRejectAtt.Enabled = approveGeneral;
            btnApproveOt.Enabled = btnRejectOt.Enabled = approveGeneral;
            btnExportLeave.Enabled = btnExportAtt.Enabled = btnExportOt.Enabled = exportGeneral;

            if (btnApproveAdvance != null) btnApproveAdvance.Enabled = approveAdvance;
            if (btnRejectAdvance != null) btnRejectAdvance.Enabled = approveAdvance;
            if (btnDisburseAdvance != null) btnDisburseAdvance.Enabled = canDisburse;
            if (btnExportAdvance != null) btnExportAdvance.Enabled = exportGeneral || (viewAdvance && UserSession.CanPrint("F_APPROVE_ADVANCE"));

            if (btnApproveProfile != null) btnApproveProfile.Enabled = approveProfile;
            if (btnRejectProfile != null) btnRejectProfile.Enabled = approveProfile;
            if (btnExportProfile != null) btnExportProfile.Enabled = exportGeneral || (viewProfile && UserSession.CanPrint("F_APPROVE_PROFILE"));
        }

        private void InitializeAdditionalTabs()
        {
            if (tabAdvance != null) return;

            // 1. Tab Tạm ứng lương
            tabAdvance = new XtraTabPage { Text = "💵 Tạm Ứng Lương" };

            var pnlAdvTop = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(248, 249, 250) };
            var lblAdvSt = new LabelControl { Text = "Trạng thái:", Location = new Point(15, 13) };
            cboAdvanceStatus = new ComboBoxEdit { Location = new Point(85, 10), Width = 150 };
            cboAdvanceStatus.Properties.Items.AddRange(new object[] { "Tất cả", "Chờ duyệt", "Đã duyệt", "Đã chi tiền", "Từ chối" });
            cboAdvanceStatus.SelectedIndex = 1; // Default: Chờ duyệt

            var lblAdvSearch = new LabelControl { Text = "Tìm kiếm:", Location = new Point(260, 13) };
            txtAdvanceSearch = new TextEdit { Location = new Point(325, 10), Width = 220 };
            pnlAdvTop.Controls.AddRange(new Control[] { lblAdvSt, cboAdvanceStatus, lblAdvSearch, txtAdvanceSearch });

            var pnlAdvActions = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.FromArgb(248, 249, 250) };
            btnApproveAdvance = new SimpleButton { Text = "✔ Phê Duyệt", Width = 115, Height = 34, Location = new Point(15, 7) };
            btnApproveAdvance.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRejectAdvance = new SimpleButton { Text = "✖ Từ Chối", Width = 110, Height = 34, Location = new Point(138, 7) };
            btnRejectAdvance.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnDisburseAdvance = new SimpleButton { Text = "💰 Xác Nhận Chi Tiền", Width = 160, Height = 34, Location = new Point(256, 7) };
            btnDisburseAdvance.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExportAdvance = new SimpleButton { Text = "📥 Xuất Excel", Width = 110, Height = 34, Location = new Point(424, 7) };

            pnlAdvActions.Controls.AddRange(new Control[] { btnApproveAdvance, btnRejectAdvance, btnDisburseAdvance, btnExportAdvance });

            gcAdvance = new GridControl { Dock = DockStyle.Fill };
            gvAdvance = new GridView(gcAdvance);
            gcAdvance.MainView = gvAdvance;

            tabAdvance.Controls.Add(gcAdvance);
            tabAdvance.Controls.Add(pnlAdvActions);
            tabAdvance.Controls.Add(pnlAdvTop);
            tabMain.TabPages.Add(tabAdvance);

            // 2. Tab Thay đổi hồ sơ
            tabProfile = new XtraTabPage { Text = "📋 Thay Đổi Hồ Sơ" };

            var pnlProfTop = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.FromArgb(248, 249, 250) };
            var lblProfGrp = new LabelControl { Text = "Nhóm hồ sơ:", Location = new Point(15, 13) };
            cboProfileGroup = new ComboBoxEdit { Location = new Point(100, 10), Width = 170 };
            cboProfileGroup.Properties.Items.AddRange(new object[] { "Tất cả", "Thông tin cá nhân", "Bảo hiểm", "Người phụ thuộc", "Ảnh đại diện" });
            cboProfileGroup.SelectedIndex = 0;

            var lblProfSt = new LabelControl { Text = "Trạng thái:", Location = new Point(290, 13) };
            cboProfileStatus = new ComboBoxEdit { Location = new Point(360, 10), Width = 130 };
            cboProfileStatus.Properties.Items.AddRange(new object[] { "Tất cả", "Chờ duyệt", "Đã duyệt/áp dụng", "Từ chối" });
            cboProfileStatus.SelectedIndex = 1; // Default: Chờ duyệt

            var lblProfSearch = new LabelControl { Text = "Tìm kiếm:", Location = new Point(510, 13) };
            txtProfileSearch = new TextEdit { Location = new Point(575, 10), Width = 200 };
            pnlProfTop.Controls.AddRange(new Control[] { lblProfGrp, cboProfileGroup, lblProfSt, cboProfileStatus, lblProfSearch, txtProfileSearch });

            var pnlProfActions = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.FromArgb(248, 249, 250) };
            btnViewProfileDiff = new SimpleButton { Text = "🔍 So Sánh / Xem Ảnh", Width = 160, Height = 34, Location = new Point(15, 7) };
            btnViewProfileDiff.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnApproveProfile = new SimpleButton { Text = "✔ Phê Duyệt & Áp Dụng", Width = 170, Height = 34, Location = new Point(183, 7) };
            btnApproveProfile.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRejectProfile = new SimpleButton { Text = "✖ Từ Chối", Width = 110, Height = 34, Location = new Point(361, 7) };
            btnRejectProfile.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExportProfile = new SimpleButton { Text = "📥 Xuất Excel", Width = 110, Height = 34, Location = new Point(479, 7) };

            pnlProfActions.Controls.AddRange(new Control[] { btnViewProfileDiff, btnApproveProfile, btnRejectProfile, btnExportProfile });

            gcProfile = new GridControl { Dock = DockStyle.Fill };
            gvProfile = new GridView(gcProfile);
            gcProfile.MainView = gvProfile;

            tabProfile.Controls.Add(gcProfile);
            tabProfile.Controls.Add(pnlProfActions);
            tabProfile.Controls.Add(pnlProfTop);
            tabMain.TabPages.Add(tabProfile);

            // Wire up event handlers
            cboAdvanceStatus.SelectedIndexChanged += (s, e) => FilterAdvanceGrid();
            txtAdvanceSearch.EditValueChanged += (s, e) => FilterAdvanceGrid();
            btnApproveAdvance.Click += async (s, e) => { await ProcessAdvanceApproval(true); if (IsDisposed || Disposing) return; };
            btnRejectAdvance.Click += async (s, e) => { await ProcessAdvanceApproval(false); if (IsDisposed || Disposing) return; };
            btnDisburseAdvance.Click += async (s, e) => { await ProcessAdvanceDisburse(); if (IsDisposed || Disposing) return; };
            btnExportAdvance.Click += (s, e) => ExportGrid(gvAdvance, "TamUngLuong");

            cboProfileGroup.SelectedIndexChanged += (s, e) => FilterProfileGrid();
            cboProfileStatus.SelectedIndexChanged += (s, e) => FilterProfileGrid();
            txtProfileSearch.EditValueChanged += (s, e) => FilterProfileGrid();
            btnViewProfileDiff.Click += (s, e) => ViewProfileDiff();
            btnApproveProfile.Click += async (s, e) => { await ProcessProfileApproval(true); if (IsDisposed || Disposing) return; };
            btnRejectProfile.Click += async (s, e) => { await ProcessProfileApproval(false); if (IsDisposed || Disposing) return; };
            btnExportProfile.Click += (s, e) => ExportGrid(gvProfile, "ThayDoiHoSo");

            ApplyApprovalPermissions();
        }

        private void TranslateStatusCombo(ComboBoxEdit cbo)
        {
            if (cbo == null || cbo.Properties == null) return;
            for (int i = 0; i < cbo.Properties.Items.Count; i++)
            {
                string original = cbo.Properties.Items[i]?.ToString();
                if (!string.IsNullOrEmpty(original))
                {
                    cbo.Properties.Items[i] = QLyNSu.Functions.TranslationManager.Translate(original);
                }
            }
        }

        private void SetupGridViewAppearance(GridView gv)
        {
            if (gv == null) return;
            gv.OptionsBehavior.Editable = false;
            gv.OptionsView.ShowGroupPanel = false;
            gv.OptionsView.EnableAppearanceEvenRow = true;
            gv.OptionsSelection.EnableAppearanceFocusedCell = false;
            gv.OptionsSelection.MultiSelect = false;
            gv.RowHeight = 28;
            gv.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gv.Appearance.Row.Font = new Font("Segoe UI", 9F);

            gv.CustomColumnDisplayText += (s, e) =>
            {
                if (e.Column.FieldName == "TRANGTHAI" && e.Value != null)
                {
                    string st = Convert.ToString(e.Value);
                    if (st == "PENDING") e.DisplayText = QLyNSu.Functions.TranslationManager.Translate("Chờ duyệt");
                    else if (st == "APPROVED") e.DisplayText = QLyNSu.Functions.TranslationManager.Translate("Đã duyệt");
                    else if (st == "REJECTED") e.DisplayText = QLyNSu.Functions.TranslationManager.Translate("Từ chối");
                    else if (st == "DISBURSED") e.DisplayText = QLyNSu.Functions.TranslationManager.Translate("Đã chi tiền");
                }
            };

            gv.RowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == "TRANGTHAI")
                {
                    string st = Convert.ToString(e.CellValue);
                    if (st == "PENDING")
                    {
                        e.Appearance.BackColor = Color.FromArgb(255, 248, 220);
                        e.Appearance.ForeColor = Color.FromArgb(180, 90, 0);
                        e.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    }
                    else if (st == "APPROVED")
                    {
                        e.Appearance.BackColor = Color.FromArgb(232, 250, 236);
                        e.Appearance.ForeColor = Color.FromArgb(24, 134, 75);
                        e.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    }
                    else if (st == "REJECTED")
                    {
                        e.Appearance.BackColor = Color.FromArgb(254, 236, 236);
                        e.Appearance.ForeColor = Color.Red;
                        e.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    }
                    else if (st == "DISBURSED")
                    {
                        e.Appearance.BackColor = Color.FromArgb(230, 245, 255);
                        e.Appearance.ForeColor = Color.FromArgb(12, 110, 180);
                        e.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    }
                }
            };
        }

        #region LOAD ALL REQUESTS (DIRECT ORACLE)
        public async Task LoadAllRequests()
        {
            ApplyApprovalPermissions();
            if (!UserSession.CanView("F_NV_PHEDUYET") && !UserSession.CanView("F_APPROVE_ADVANCE") && !UserSession.CanView("F_APPROVE_PROFILE") && !UserSession.CanView("F_PAYROLL_DISBURSE_ADVANCE")) return;
            try
            {
                using (var db = new MyEntities())
                {
                    // 1. Đơn Nghỉ Phép (TB_YEUCAU_NGHIPHEP)
                    string leaveSql = @"
                        SELECT 
                            Y.ID AS ID,
                            Y.MANV AS MANV,
                            NV.EMPLOYEE_CODE AS EMPLOYEE_CODE,
                            NV.HOTEN AS EMPLOYEE_NAME,
                            PB.TENPB AS DEPARTMENT_NAME,
                            Y.LOAIPHEP AS LOAI_NGHI,
                            TO_CHAR(Y.TUNGAY, 'DD/MM/YYYY') AS TU_NGAY,
                            TO_CHAR(Y.DENNGAY, 'DD/MM/YYYY') AS DEN_NGAY,
                            Y.SONGAY AS SO_NGAY,
                            Y.LYDO AS LYDO,
                            Y.TRANGTHAI AS TRANGTHAI,
                            TO_CHAR(Y.CREATED_DATE, 'DD/MM/YYYY HH24:MI') AS NGAY_TAO,
                            NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET,
                            Y.GHICHUDUYET AS LYDO_TUCHOI
                        FROM HR.TB_YEUCAU_NGHIPHEP Y
                        LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                        LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                        LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                        ORDER BY Y.ID DESC";

                    _rawLeaves = await Functions.FormDataLoader.ReadAsync(this, () => db.Database.SqlQuery<LeaveRow>(leaveSql).ToList());

                    // 2. Giải Trình Chấm Công (TB_YEUCAU_DIEUCHINHCONG)
                    string attSql = @"
                        SELECT 
                            Y.ID AS ID,
                            Y.MANV AS MANV,
                            NV.EMPLOYEE_CODE AS EMPLOYEE_CODE,
                            NV.HOTEN AS EMPLOYEE_NAME,
                            PB.TENPB AS DEPARTMENT_NAME,
                            TO_CHAR(Y.NGAY, 'DD/MM/YYYY') AS NGAY_CONG,
                            Y.GIO_VAO AS GIO_VAO_MOI,
                            Y.GIO_RA AS GIO_RA_MOI,
                            Y.LYDO AS LYDO,
                            Y.TRANGTHAI AS TRANGTHAI,
                            TO_CHAR(Y.CREATED_DATE, 'DD/MM/YYYY HH24:MI') AS NGAY_TAO,
                            NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET,
                            Y.GHICHUDUYET AS LYDO_TUCHOI
                        FROM HR.TB_YEUCAU_DIEUCHINHCONG Y
                        LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                        LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                        LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                        ORDER BY Y.ID DESC";

                    _rawAtts = await Functions.FormDataLoader.ReadAsync(this, () => db.Database.SqlQuery<AttRow>(attSql).ToList());

                    // 3. Đăng Ký Làm Thêm Giờ (TB_YEUCAU_TANGCA)
                    string otSql = @"
                        SELECT 
                            Y.ID AS ID,
                            Y.MANV AS MANV,
                            NV.EMPLOYEE_CODE AS EMPLOYEE_CODE,
                            NV.HOTEN AS EMPLOYEE_NAME,
                            PB.TENPB AS DEPARTMENT_NAME,
                            TO_CHAR(Y.NGAY, 'DD/MM/YYYY') AS NGAY_TANGCA,
                            Y.GIOTANGCA AS SO_GIO,
                            NVL(L.HESOLOAICA, 1.0) AS HE_SO,
                            Y.LYDO AS NOIDUNG,
                            Y.TRANGTHAI AS TRANGTHAI,
                            TO_CHAR(Y.CREATED_DATE, 'DD/MM/YYYY HH24:MI') AS NGAY_TAO,
                            NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET,
                            Y.GHICHUDUYET AS LYDO_TUCHOI
                        FROM HR.TB_YEUCAU_TANGCA Y
                        LEFT JOIN HR.TB_LOAICA L ON Y.IDCA = L.IDLOAICA
                        LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                        LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                        LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                        ORDER BY Y.ID DESC";

                    _rawOts = await Functions.FormDataLoader.ReadAsync(this, () => db.Database.SqlQuery<OtRow>(otSql).ToList());

                    // 4. Đơn Tạm Ứng Lương (TB_YEUCAU_UNGLUONG)
                    string advSql = @"
                        SELECT 
                            y.ID AS ID,
                            y.MANV AS MANV,
                            nv.EMPLOYEE_CODE AS EMPLOYEE_CODE,
                            nv.HOTEN AS EMPLOYEE_NAME,
                            pb.TENPB AS DEPARTMENT_NAME,
                            y.SOTIEN AS SOTIEN,
                            TO_CHAR(y.NGAY_DENGHI, 'DD/MM/YYYY') AS NGAY_DENGHI,
                            (LPAD(y.THANG, 2, '0') || '/' || y.NAM) AS KY_LUONG,
                            y.LYDO AS LYDO,
                            y.TRANGTHAI AS TRANGTHAI,
                            TO_CHAR(y.CREATED_AT, 'DD/MM/YYYY HH24:MI') AS NGAY_TAO,
                            NVL(u_app.FULLNAME, u_app.USERNAME) AS NGUOI_DUYET,
                            TO_CHAR(y.NGAYDUYET, 'DD/MM/YYYY HH24:MI') AS NGAY_DUYET,
                            y.GHICHUDUYET AS LYDO_TUCHOI,
                            y.SO_CHUNG_TU AS SO_CHUNG_TU,
                            TO_CHAR(y.NGAY_CHI, 'DD/MM/YYYY') AS NGAY_CHI,
                            NVL(u_dis.FULLNAME, u_dis.USERNAME) AS DISBURSED_BY_NAME,
                            y.IDUL_LINK AS IDUL_LINK
                        FROM HR.TB_YEUCAU_UNGLUONG y
                        LEFT JOIN HR.TB_NHANVIEN nv ON y.MANV = nv.MANV
                        LEFT JOIN HR.TB_PHONGBAN pb ON nv.IDPB = pb.IDPB
                        LEFT JOIN HR.TB_SYS_USER u_app ON y.NGUOIDUYET = u_app.IDUSER
                        LEFT JOIN HR.TB_SYS_USER u_dis ON y.NGUOI_CHI = u_dis.IDUSER
                        ORDER BY y.CREATED_AT DESC";

                    _rawAdvances = await Functions.FormDataLoader.ReadAsync(this, () => db.Database.SqlQuery<AdvanceRow>(advSql).ToList());

                    // 5. Đơn Thay Đổi Hồ Sơ (TB_YEUCAU_THAYDOI_HOSO)
                    string profSql = @"
                        SELECT 
                            y.ID AS ID,
                            y.MANV AS MANV,
                            nv.EMPLOYEE_CODE AS EMPLOYEE_CODE,
                            nv.HOTEN AS EMPLOYEE_NAME,
                            pb.TENPB AS DEPARTMENT_NAME,
                            y.GROUP_CODE AS GROUP_CODE,
                            y.REQ_TYPE AS REQ_TYPE,
                            y.CURRENT_DATA AS CURRENT_DATA,
                            y.PROPOSED_DATA AS PROPOSED_DATA,
                            CASE WHEN y.PROPOSED_BLOB IS NOT NULL THEN 1 ELSE 0 END AS HAS_BLOB_VAL,
                            y.REASON AS REASON,
                            y.STATUS AS TRANGTHAI,
                            TO_CHAR(y.CREATED_AT, 'DD/MM/YYYY HH24:MI') AS NGAY_TAO,
                            NVL(u_res.FULLNAME, u_res.USERNAME) AS NGUOI_DUYET,
                            TO_CHAR(y.RESOLVED_AT, 'DD/MM/YYYY HH24:MI') AS NGAY_DUYET,
                            y.REJECT_REASON AS LYDO_TUCHOI
                        FROM HR.TB_YEUCAU_THAYDOI_HOSO y
                        LEFT JOIN HR.TB_NHANVIEN nv ON y.MANV = nv.MANV
                        LEFT JOIN HR.TB_PHONGBAN pb ON nv.IDPB = pb.IDPB
                        LEFT JOIN HR.TB_SYS_USER u_res ON y.RESOLVED_BY = u_res.IDUSER
                        ORDER BY y.CREATED_AT DESC";

                    var profRows = await Functions.FormDataLoader.ReadAsync(this, () => db.Database.SqlQuery<ProfileChangeRowRaw>(profSql).ToList());
                    _rawProfiles = profRows.Select(r => new ProfileChangeRow
                    {
                        ID = r.ID,
                        MANV = r.MANV,
                        EMPLOYEE_CODE = r.EMPLOYEE_CODE,
                        EMPLOYEE_NAME = r.EMPLOYEE_NAME,
                        DEPARTMENT_NAME = r.DEPARTMENT_NAME,
                        GROUP_CODE = r.GROUP_CODE,
                        REQ_TYPE = r.REQ_TYPE,
                        CURRENT_DATA = r.CURRENT_DATA,
                        PROPOSED_DATA = r.PROPOSED_DATA,
                        HAS_BLOB = r.HAS_BLOB_VAL == 1,
                        REASON = r.REASON,
                        TRANGTHAI = r.TRANGTHAI,
                        NGAY_TAO = r.NGAY_TAO,
                        NGUOI_DUYET = r.NGUOI_DUYET,
                        NGAY_DUYET = r.NGAY_DUYET,
                        LYDO_TUCHOI = r.LYDO_TUCHOI
                    }).ToList();

                    // Cập nhật các chỉ số KPI
                    int pLeave = _rawLeaves.Count(x => x.TRANGTHAI == "PENDING");
                    int pAtt = _rawAtts.Count(x => x.TRANGTHAI == "PENDING");
                    int pOt = _rawOts.Count(x => x.TRANGTHAI == "PENDING");
                    int pAdv = _rawAdvances.Count(x => x.TRANGTHAI == "PENDING");
                    int pProf = _rawProfiles.Count(x => x.TRANGTHAI == "PENDING");

                    Functions.TranslationManager.SetTranslatedText(lblPendingTotalVal, (pLeave + pAtt + pOt + pAdv + pProf).ToString("N0"));
                    Functions.TranslationManager.SetTranslatedText(lblPendingLeaveVal, pLeave.ToString("N0"));
                    Functions.TranslationManager.SetTranslatedText(lblPendingAttVal, pAtt.ToString("N0"));
                    Functions.TranslationManager.SetTranslatedText(lblPendingOtVal, (pOt + pAdv + pProf).ToString("N0"));

                    // Áp dụng bộ lọc cho các bảng
                    FilterLeaveGrid();
                    FilterAttGrid();
                    FilterOtGrid();
                    FilterAdvanceGrid();
                    FilterProfileGrid();
                }
            }
            catch (OperationCanceledException) { return; }
            catch (Exception ex)
            {
                Functions.FormDataLoader.ShowLoadError(this, ex);
            }
        }

        private async void btnRefreshAll_Click(object sender, EventArgs e)
        {
            await LoadAllRequests();
            if (IsDisposed || Disposing) return;
        }
        #endregion

        #region LEAVE TAB LOGIC
        private void cboLeaveStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterLeaveGrid();
        }

        private void txtLeaveSearch_EditValueChanged(object sender, EventArgs e)
        {
            FilterLeaveGrid();
        }

        private void FilterLeaveGrid()
        {
            if (_rawLeaves == null) return;
            var list = _rawLeaves.AsEnumerable();

            int statusIdx = cboLeaveStatus != null ? cboLeaveStatus.SelectedIndex : 0;
            if (statusIdx == 1) list = list.Where(x => x.TRANGTHAI == "PENDING");
            else if (statusIdx == 2) list = list.Where(x => x.TRANGTHAI == "APPROVED");
            else if (statusIdx == 3) list = list.Where(x => x.TRANGTHAI == "REJECTED");

            string search = txtLeaveSearch?.Text?.Trim()?.ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(x => (x.EMPLOYEE_NAME != null && x.EMPLOYEE_NAME.ToLower().Contains(search))
                                    || (x.EMPLOYEE_CODE != null && x.EMPLOYEE_CODE.ToLower().Contains(search))
                                    || (x.LYDO != null && x.LYDO.ToLower().Contains(search)));
            }

            gcLeave.DataSource = ConvertListToDataTable(list.ToList());
            gvLeave.PopulateColumns();
            ConfigureLeaveColumns();
        }

        private void ConfigureLeaveColumns()
        {
            SetColumn(gvLeave, "ID", "Mã Đơn", 70);
            SetColumn(gvLeave, "MANV", "Mã NV", 65);
            SetColumn(gvLeave, "EMPLOYEE_CODE", "Mã Nhân Sự", 100);
            SetColumn(gvLeave, "EMPLOYEE_NAME", "Họ và Tên", 150);
            SetColumn(gvLeave, "DEPARTMENT_NAME", "Phòng Ban", 130);
            SetColumn(gvLeave, "LOAI_NGHI", "Loại Phép", 110);
            SetColumn(gvLeave, "TU_NGAY", "Từ Ngày", 85);
            SetColumn(gvLeave, "DEN_NGAY", "Đến Ngày", 85);
            SetColumn(gvLeave, "SO_NGAY", "Số Ngày", 70);
            SetColumn(gvLeave, "LYDO", "Lý Do Xin Nghỉ", 200);
            SetColumn(gvLeave, "TRANGTHAI", "Trạng Thái", 100);
            SetColumn(gvLeave, "NGAY_TAO", "Thời Gian Gửi", 120);
            SetColumn(gvLeave, "NGUOI_DUYET", "Người Duyệt", 120);
            SetColumn(gvLeave, "LYDO_TUCHOI", "Ghi Chú Duyệt", 150);
            gvLeave.BestFitColumns();
        }

        private async void btnApproveLeave_Click(object sender, EventArgs e)
        {
            await ProcessLeaveApproval(true);
            if (IsDisposed || Disposing) return;
        }

        private async void btnRejectLeave_Click(object sender, EventArgs e)
        {
            await ProcessLeaveApproval(false);
            if (IsDisposed || Disposing) return;
        }

        private void btnExportLeave_Click(object sender, EventArgs e)
        {
            ExportGrid(gvLeave, "DonNghiPhep");
        }

        private async Task ProcessLeaveApproval(bool isApprove)
        {
            if (!CanApproveRequests())
            {
                ApplyApprovalPermissions();
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Bạn chưa được cấp quyền duyệt/từ chối yêu cầu."), "Thông báo");
                return;
            }
            int row = gvLeave.FocusedRowHandle;
            if (row < 0)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Vui lòng chọn một đơn xin nghỉ phép trên bảng."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal id = Convert.ToDecimal(gvLeave.GetRowCellValue(row, "ID"));
            decimal? manv = gvLeave.GetRowCellValue(row, "MANV") != null ? Convert.ToDecimal(gvLeave.GetRowCellValue(row, "MANV")) : (decimal?)null;
            string empName = Convert.ToString(gvLeave.GetRowCellValue(row, "EMPLOYEE_NAME"));
            string currentStatus = Convert.ToString(gvLeave.GetRowCellValue(row, "TRANGTHAI"));

            // Chặn tự duyệt/từ chối
            decimal? currentEmpId = UserSession.CurrentUser?.MANV;
            if (currentEmpId.HasValue && manv.HasValue && currentEmpId.Value == manv.Value)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Bạn không thể tự phê duyệt hoặc từ chối đơn của chính mình!"), QLyNSu.Functions.TranslationManager.Translate("Cảnh báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (currentStatus != "PENDING")
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Chỉ có thể xử lý các đơn đang ở trạng thái PENDING (Chờ duyệt)."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string reason = null;
            if (!isApprove)
            {
                reason = XtraInputBox.Show(QLyNSu.Functions.TranslationManager.Translate("Nhập lý do từ chối đơn nghỉ phép:"), QLyNSu.Functions.TranslationManager.Translate("Lý Do Từ Chối"), "Không phù hợp lịch công tác");
                if (string.IsNullOrWhiteSpace(reason)) return;
            }

            try
            {
                decimal currentUserId = GetCurrentUserId(null);
                var attApprovalService = new Bu.CLASS_CHAMCONG.AttendanceApprovalService();

                if (isApprove)
                {
                    await Task.Run(() => attApprovalService.ApproveLeave(id, currentUserId, currentEmpId));
                }
                else
                {
                    using (var db = new MyEntities())
                    {
                        db.Database.ExecuteSqlCommand(
                            "UPDATE HR.TB_YEUCAU_NGHIPHEP SET TRANGTHAI = 'REJECTED', NGUOIDUYET = :p0, NGAYDUYET = SYSDATE, GHICHUDUYET = :p1 WHERE ID = :p2 AND TRANGTHAI = 'PENDING'",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", currentUserId),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p1", reason ?? "Từ chối"),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p2", id)
                        );
                    }
                }

                WriteAudit("TB_YEUCAU_NGHIPHEP", id.ToString(), isApprove ? "APPROVE_LEAVE" : "REJECT_LEAVE", $"Đã {(isApprove ? "duyệt" : "từ chối")} đơn nghỉ phép của [{empName}]");

                string successMsg = isApprove ? QLyNSu.Functions.TranslationManager.Translate("Đã phê duyệt đơn nghỉ phép thành công!") : QLyNSu.Functions.TranslationManager.Translate("Đã từ chối đơn nghỉ phép thành công!");
                XtraMessageBox.Show(successMsg, QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAllRequests();
                if (IsDisposed || Disposing) return;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Lỗi:") + " " + ex.Message, QLyNSu.Functions.TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region ATTENDANCE TAB LOGIC
        private void cboAttStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAttGrid();
        }

        private void txtAttSearch_EditValueChanged(object sender, EventArgs e)
        {
            FilterAttGrid();
        }

        private void FilterAttGrid()
        {
            if (_rawAtts == null) return;
            var list = _rawAtts.AsEnumerable();

            int statusIdx = cboAttStatus != null ? cboAttStatus.SelectedIndex : 0;
            if (statusIdx == 1) list = list.Where(x => x.TRANGTHAI == "PENDING");
            else if (statusIdx == 2) list = list.Where(x => x.TRANGTHAI == "APPROVED");
            else if (statusIdx == 3) list = list.Where(x => x.TRANGTHAI == "REJECTED");

            string search = txtAttSearch?.Text?.Trim()?.ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(x => (x.EMPLOYEE_NAME != null && x.EMPLOYEE_NAME.ToLower().Contains(search))
                                    || (x.EMPLOYEE_CODE != null && x.EMPLOYEE_CODE.ToLower().Contains(search))
                                    || (x.LYDO != null && x.LYDO.ToLower().Contains(search)));
            }

            gcAttendance.DataSource = ConvertListToDataTable(list.ToList());
            gvAttendance.PopulateColumns();
            ConfigureAttColumns();
        }

        private void ConfigureAttColumns()
        {
            SetColumn(gvAttendance, "ID", "Mã Đơn", 70);
            SetColumn(gvAttendance, "MANV", "Mã NV", 65);
            SetColumn(gvAttendance, "EMPLOYEE_CODE", "Mã Nhân Sự", 100);
            SetColumn(gvAttendance, "EMPLOYEE_NAME", "Họ và Tên", 150);
            SetColumn(gvAttendance, "DEPARTMENT_NAME", "Phòng Ban", 130);
            SetColumn(gvAttendance, "NGAY_CONG", "Ngày Công", 90);
            SetColumn(gvAttendance, "GIO_VAO_MOI", "Giờ Vào Đề Xuất", 110);
            SetColumn(gvAttendance, "GIO_RA_MOI", "Giờ Ra Đề Xuất", 110);
            SetColumn(gvAttendance, "LYDO", "Lý Do Giải Trình", 200);
            SetColumn(gvAttendance, "TRANGTHAI", "Trạng Thái", 100);
            SetColumn(gvAttendance, "NGAY_TAO", "Thời Gian Gửi", 120);
            SetColumn(gvAttendance, "NGUOI_DUYET", "Người Duyệt", 120);
            SetColumn(gvAttendance, "LYDO_TUCHOI", "Ghi Chú Duyệt", 150);
            gvAttendance.BestFitColumns();
        }

        private async void btnApproveAtt_Click(object sender, EventArgs e)
        {
            await ProcessAttendanceApproval(true);
            if (IsDisposed || Disposing) return;
        }

        private async void btnRejectAtt_Click(object sender, EventArgs e)
        {
            await ProcessAttendanceApproval(false);
            if (IsDisposed || Disposing) return;
        }

        private void btnExportAtt_Click(object sender, EventArgs e)
        {
            ExportGrid(gvAttendance, "GiaiTrinhChamCong");
        }

        private async Task ProcessAttendanceApproval(bool isApprove)
        {
            if (!CanApproveRequests())
            {
                ApplyApprovalPermissions();
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Bạn chưa được cấp quyền duyệt/từ chối yêu cầu."), "Thông báo");
                return;
            }
            int row = gvAttendance.FocusedRowHandle;
            if (row < 0)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Vui lòng chọn một bản ghi giải trình chấm công trên bảng."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal id = Convert.ToDecimal(gvAttendance.GetRowCellValue(row, "ID"));
            decimal? manv = gvAttendance.GetRowCellValue(row, "MANV") != null ? Convert.ToDecimal(gvAttendance.GetRowCellValue(row, "MANV")) : (decimal?)null;
            string empName = Convert.ToString(gvAttendance.GetRowCellValue(row, "EMPLOYEE_NAME"));
            string currentStatus = Convert.ToString(gvAttendance.GetRowCellValue(row, "TRANGTHAI"));

            // Chặn tự duyệt/từ chối
            decimal? currentEmpId = UserSession.CurrentUser?.MANV;
            if (currentEmpId.HasValue && manv.HasValue && currentEmpId.Value == manv.Value)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Bạn không thể tự phê duyệt hoặc từ chối yêu cầu của chính mình!"), QLyNSu.Functions.TranslationManager.Translate("Cảnh báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (currentStatus != "PENDING")
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Chỉ có thể xử lý các đơn đang ở trạng thái PENDING (Chờ duyệt)."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string reason = null;
            if (!isApprove)
            {
                reason = XtraInputBox.Show(QLyNSu.Functions.TranslationManager.Translate("Nhập lý do từ chối giải trình chấm công:"), QLyNSu.Functions.TranslationManager.Translate("Lý Do Từ Chối"), "Không hợp lệ theo quy chế");
                if (string.IsNullOrWhiteSpace(reason)) return;
            }

            try
            {
                decimal currentUserId = GetCurrentUserId(null);
                var attApprovalService = new Bu.CLASS_CHAMCONG.AttendanceApprovalService();

                if (isApprove)
                {
                    await Task.Run(() => attApprovalService.ApproveAttendanceCorrection(id, currentUserId, currentEmpId));
                }
                else
                {
                    using (var db = new MyEntities())
                    {
                        db.Database.ExecuteSqlCommand(
                            "UPDATE HR.TB_YEUCAU_DIEUCHINHCONG SET TRANGTHAI = 'REJECTED', NGUOIDUYET = :p0, NGAYDUYET = SYSDATE, GHICHUDUYET = :p1 WHERE ID = :p2 AND TRANGTHAI = 'PENDING'",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", currentUserId),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p1", reason ?? "Từ chối"),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p2", id)
                        );
                    }
                }

                WriteAudit("TB_YEUCAU_DIEUCHINHCONG", id.ToString(), isApprove ? "APPROVE_ATTENDANCE" : "REJECT_ATTENDANCE", $"Đã {(isApprove ? "duyệt" : "từ chối")} giải trình công của [{empName}]");

                string successMsg = isApprove ? QLyNSu.Functions.TranslationManager.Translate("Đã phê duyệt giải trình chấm công thành công!") : QLyNSu.Functions.TranslationManager.Translate("Đã từ chối giải trình chấm công thành công!");
                XtraMessageBox.Show(successMsg, QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAllRequests();
                if (IsDisposed || Disposing) return;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Lỗi:") + " " + ex.Message, QLyNSu.Functions.TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region OVERTIME TAB LOGIC
        private void cboOtStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterOtGrid();
        }

        private void txtOtSearch_EditValueChanged(object sender, EventArgs e)
        {
            FilterOtGrid();
        }

        private void FilterOtGrid()
        {
            if (_rawOts == null) return;
            var list = _rawOts.AsEnumerable();

            int statusIdx = cboOtStatus != null ? cboOtStatus.SelectedIndex : 0;
            if (statusIdx == 1) list = list.Where(x => x.TRANGTHAI == "PENDING");
            else if (statusIdx == 2) list = list.Where(x => x.TRANGTHAI == "APPROVED");
            else if (statusIdx == 3) list = list.Where(x => x.TRANGTHAI == "REJECTED");

            string search = txtOtSearch?.Text?.Trim()?.ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(x => (x.EMPLOYEE_NAME != null && x.EMPLOYEE_NAME.ToLower().Contains(search))
                                    || (x.EMPLOYEE_CODE != null && x.EMPLOYEE_CODE.ToLower().Contains(search))
                                    || (x.NOIDUNG != null && x.NOIDUNG.ToLower().Contains(search)));
            }

            gcOvertime.DataSource = ConvertListToDataTable(list.ToList());
            gvOvertime.PopulateColumns();
            ConfigureOtColumns();
        }

        private void ConfigureOtColumns()
        {
            SetColumn(gvOvertime, "ID", "Mã Đơn", 70);
            SetColumn(gvOvertime, "MANV", "Mã NV", 65);
            SetColumn(gvOvertime, "EMPLOYEE_CODE", "Mã Nhân Sự", 100);
            SetColumn(gvOvertime, "EMPLOYEE_NAME", "Họ và Tên", 150);
            SetColumn(gvOvertime, "DEPARTMENT_NAME", "Phòng Ban", 130);
            SetColumn(gvOvertime, "NGAY_TANGCA", "Ngày Làm Thêm", 100);
            SetColumn(gvOvertime, "SO_GIO", "Số Giờ", 70);
            SetColumn(gvOvertime, "HE_SO", "Hệ Số", 60);
            SetColumn(gvOvertime, "NOIDUNG", "Nội Dung Công Việc", 200);
            SetColumn(gvOvertime, "TRANGTHAI", "Trạng Thái", 100);
            SetColumn(gvOvertime, "NGAY_TAO", "Thời Gian Gửi", 120);
            SetColumn(gvOvertime, "NGUOI_DUYET", "Người Duyệt", 120);
            SetColumn(gvOvertime, "LYDO_TUCHOI", "Ghi Chú Duyệt", 150);
            gvOvertime.BestFitColumns();
        }

        private async void btnApproveOt_Click(object sender, EventArgs e)
        {
            await ProcessOvertimeApproval(true);
            if (IsDisposed || Disposing) return;
        }

        private async void btnRejectOt_Click(object sender, EventArgs e)
        {
            await ProcessOvertimeApproval(false);
            if (IsDisposed || Disposing) return;
        }

        private void btnExportOt_Click(object sender, EventArgs e)
        {
            ExportGrid(gvOvertime, "DangKyTangCa");
        }

        private async Task ProcessOvertimeApproval(bool isApprove)
        {
            if (!CanApproveRequests())
            {
                ApplyApprovalPermissions();
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Bạn chưa được cấp quyền duyệt/từ chối yêu cầu."), "Thông báo");
                return;
            }
            int row = gvOvertime.FocusedRowHandle;
            if (row < 0)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Vui lòng chọn một đơn đăng ký tăng ca trên bảng."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal id = Convert.ToDecimal(gvOvertime.GetRowCellValue(row, "ID"));
            string empName = Convert.ToString(gvOvertime.GetRowCellValue(row, "EMPLOYEE_NAME"));
            string currentStatus = Convert.ToString(gvOvertime.GetRowCellValue(row, "TRANGTHAI"));

            if (currentStatus != "PENDING")
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Chỉ có thể xử lý các đơn đang ở trạng thái PENDING (Chờ duyệt)."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string reason = null;
            if (!isApprove)
            {
                reason = XtraInputBox.Show(QLyNSu.Functions.TranslationManager.Translate("Nhập lý do từ chối đăng ký làm thêm giờ:"), QLyNSu.Functions.TranslationManager.Translate("Lý Do Từ Chối"), "Kế hoạch sản xuất không yêu cầu");
                if (string.IsNullOrWhiteSpace(reason)) return;
            }

            try
            {
                decimal currentUserId = GetCurrentUserId(null);
                await Functions.FormDataLoader.RunAsync(this, () => {
                    CommitOvertimeDecision(id, currentUserId, isApprove, reason);
                    WriteAudit("TB_YEUCAU_TANGCA", id.ToString(), isApprove ? "APPROVE_OVERTIME" : "REJECT_OVERTIME", $"Đã {(isApprove ? "duyệt" : "từ chối")} đăng ký tăng ca của [{empName}]");
                });
                if (IsDisposed || Disposing) return;
                XtraMessageBox.Show($"Đã {(isApprove ? "phê duyệt" : "từ chối")} đăng ký làm thêm giờ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAllRequests();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                    XtraMessageBox.Show(ErrorHelper.ResolveUserFriendlyMessage(ex, "duyệt/từ chối tăng ca"), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CommitOvertimeDecision(decimal id, decimal currentUserId, bool isApprove, string reason)
        {
            using (var db = new MyEntities())
            using (var transaction = db.Database.BeginTransaction())
            {
                var reqItem = db.Database.SqlQuery<OtDetailDto>(
                    @"SELECT Y.ID, Y.MANV, Y.NGAY, Y.GIOTANGCA, Y.IDCA, NVL(L.HESOLOAICA, 1.0) AS HE_SO, Y.LYDO, Y.TRANGTHAI 
                      FROM HR.TB_YEUCAU_TANGCA Y 
                      LEFT JOIN HR.TB_LOAICA L ON Y.IDCA = L.IDLOAICA 
                      WHERE Y.ID = :p0",
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p0", id)
                ).FirstOrDefault();

                if (reqItem == null)
                {
                    throw new BusinessException("OVERTIME_NOT_FOUND", "Không tìm thấy đề xuất tăng ca.");
                }

                if (reqItem.TRANGTHAI != "PENDING")
                {
                    throw new BusinessException("OVERTIME_ALREADY_PROCESSED", "Đề xuất không còn ở trạng thái chờ duyệt hoặc đã có người khác xử lý.");
                }

                // Security: Ngăn chặn tự phê duyệt / tự từ chối của chính mình
                var approverEmpId = db.Database.SqlQuery<decimal?>(
                    "SELECT EMPLOYEE_ID FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p0", currentUserId)
                ).FirstOrDefault();

                if (approverEmpId.HasValue && approverEmpId.Value == reqItem.MANV)
                {
                    throw new BusinessException("OVERTIME_SELF_APPROVAL", "Bạn không thể tự phê duyệt hoặc từ chối đề xuất tăng ca của chính mình!");
                }

                if (isApprove && (!reqItem.NGAY.HasValue || !reqItem.MANV.HasValue || !reqItem.GIOTANGCA.HasValue || reqItem.GIOTANGCA.Value <= 0))
                    throw new BusinessException("INVALID_OVERTIME", "Đề xuất tăng ca thiếu ngày, nhân viên hoặc số giờ hợp lệ.");

                // 1. Cập nhật trạng thái
                int affected = db.Database.ExecuteSqlCommand(
                    "UPDATE HR.TB_YEUCAU_TANGCA SET TRANGTHAI = :p0, NGUOIDUYET = :p1, NGAYDUYET = SYSDATE, GHICHUDUYET = :p2 WHERE ID = :p3 AND TRANGTHAI = 'PENDING'",
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p0", isApprove ? "APPROVED" : "REJECTED"),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p1", currentUserId),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p2", (object)reason ?? (isApprove ? "Đã duyệt qua Desktop" : "Từ chối")),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p3", id)
                );

                if (affected == 0)
                {
                    throw new BusinessException("OVERTIME_ALREADY_PROCESSED", "Đề xuất không còn ở trạng thái chờ duyệt hoặc đã có người khác xử lý.");
                }

                // 2. Nếu phê duyệt -> ghi nhận vào TB_TANGCA kèm liên kết OT_REQUEST_ID
                if (isApprove && reqItem.NGAY.HasValue && reqItem.MANV.HasValue && reqItem.GIOTANGCA.HasValue && reqItem.GIOTANGCA.Value > 0)
                {
                    try
                    {
                        db.Database.ExecuteSqlCommand(@"
                            INSERT INTO HR.TB_TANGCA 
                            (MANV, NGAY, THANG, NAM, SOGIO, HESOTC, GHICHU, IDLOAICA, OT_REQUEST_ID, CREATED_DATE, CREATED_BY)
                            VALUES (:p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, SYSDATE, :p9)",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", reqItem.MANV.Value),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p1", (decimal)reqItem.NGAY.Value.Day),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p2", (decimal)reqItem.NGAY.Value.Month),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p3", (decimal)reqItem.NGAY.Value.Year),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p4", reqItem.GIOTANGCA.Value),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p5", reqItem.HE_SO ?? 1.0m),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p6", reqItem.LYDO ?? "Duyệt từ yêu cầu tăng ca Desktop"),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p7", (object)reqItem.IDCA ?? DBNull.Value),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p8", id),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p9", currentUserId)
                        );
                    }
                    catch (Exception exTc)
                    {
                        throw new BusinessException("OVERTIME_SYNC_FAILED", "Không ghi được giờ tăng ca; yêu cầu chưa được phê duyệt.", exTc);
                    }
                }

                transaction.Commit();
            }
        }
        #endregion

        #region SALARY ADVANCE TAB LOGIC
        private void FilterAdvanceGrid()
        {
            if (_rawAdvances == null) return;
            var list = _rawAdvances.AsEnumerable();

            int statusIdx = cboAdvanceStatus != null ? cboAdvanceStatus.SelectedIndex : 0;
            if (statusIdx == 1) list = list.Where(x => x.TRANGTHAI == "PENDING");
            else if (statusIdx == 2) list = list.Where(x => x.TRANGTHAI == "APPROVED");
            else if (statusIdx == 3) list = list.Where(x => x.TRANGTHAI == "DISBURSED");
            else if (statusIdx == 4) list = list.Where(x => x.TRANGTHAI == "REJECTED");

            string search = txtAdvanceSearch?.Text?.Trim()?.ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(x => (x.EMPLOYEE_NAME != null && x.EMPLOYEE_NAME.ToLower().Contains(search))
                                    || (x.EMPLOYEE_CODE != null && x.EMPLOYEE_CODE.ToLower().Contains(search))
                                    || (x.LYDO != null && x.LYDO.ToLower().Contains(search))
                                    || (x.SO_CHUNG_TU != null && x.SO_CHUNG_TU.ToLower().Contains(search)));
            }

            gcAdvance.DataSource = ConvertListToDataTable(list.ToList());
            gvAdvance.PopulateColumns();
            ConfigureAdvanceColumns();
        }

        private void ConfigureAdvanceColumns()
        {
            SetColumn(gvAdvance, "ID", "Mã Đơn", 70);
            SetColumn(gvAdvance, "MANV", "Mã NV", 65);
            SetColumn(gvAdvance, "EMPLOYEE_CODE", "Mã Nhân Sự", 100);
            SetColumn(gvAdvance, "EMPLOYEE_NAME", "Họ và Tên", 150);
            SetColumn(gvAdvance, "DEPARTMENT_NAME", "Phòng Ban", 130);
            SetColumn(gvAdvance, "SOTIEN", "Số Tiền Ứng", 110);
            SetColumn(gvAdvance, "NGAY_DENGHI", "Ngày Đề Nghị", 95);
            SetColumn(gvAdvance, "KY_LUONG", "Kỳ Lương", 80);
            SetColumn(gvAdvance, "LYDO", "Lý Do Tạm Ứng", 180);
            SetColumn(gvAdvance, "TRANGTHAI", "Trạng Thái", 100);
            SetColumn(gvAdvance, "NGAY_TAO", "Thời Gian Gửi", 120);
            SetColumn(gvAdvance, "NGUOI_DUYET", "Người Duyệt", 120);
            SetColumn(gvAdvance, "NGAY_DUYET", "Ngày Duyệt", 110);
            SetColumn(gvAdvance, "SO_CHUNG_TU", "Số Chứng Từ", 120);
            SetColumn(gvAdvance, "NGAY_CHI", "Ngày Chi Tiền", 110);
            SetColumn(gvAdvance, "DISBURSED_BY_NAME", "Người Chi Tiền", 130);
            SetColumn(gvAdvance, "IDUL_LINK", "Mã Phiếu Chi", 85);
            SetColumn(gvAdvance, "LYDO_TUCHOI", "Ghi Chú Duyệt", 150);

            if (gvAdvance.Columns["SOTIEN"] != null)
            {
                gvAdvance.Columns["SOTIEN"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gvAdvance.Columns["SOTIEN"].DisplayFormat.FormatString = "N0";
            }
            gvAdvance.BestFitColumns();
        }

        private async Task ProcessAdvanceApproval(bool isApprove)
        {
            bool canApprove = CanApproveRequests() || UserSession.CanEdit("F_APPROVE_ADVANCE");
            if (!canApprove)
            {
                ApplyApprovalPermissions();
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Bạn chưa được cấp quyền duyệt/từ chối yêu cầu tạm ứng."), "Thông báo");
                return;
            }
            int row = gvAdvance.FocusedRowHandle;
            if (row < 0)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Vui lòng chọn một đơn tạm ứng lương trên bảng."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal id = Convert.ToDecimal(gvAdvance.GetRowCellValue(row, "ID"));
            decimal? manv = gvAdvance.GetRowCellValue(row, "MANV") != null ? Convert.ToDecimal(gvAdvance.GetRowCellValue(row, "MANV")) : (decimal?)null;
            string empName = Convert.ToString(gvAdvance.GetRowCellValue(row, "EMPLOYEE_NAME"));
            string currentStatus = Convert.ToString(gvAdvance.GetRowCellValue(row, "TRANGTHAI"));

            decimal? currentEmpId = UserSession.CurrentUser?.MANV;
            if (currentEmpId.HasValue && manv.HasValue && currentEmpId.Value == manv.Value)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Bạn không thể tự phê duyệt hoặc từ chối đơn của chính mình!"), QLyNSu.Functions.TranslationManager.Translate("Cảnh báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (currentStatus != "PENDING")
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Chỉ có thể xử lý các đơn đang ở trạng thái PENDING (Chờ duyệt)."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string reason = null;
            if (!isApprove)
            {
                reason = XtraInputBox.Show(QLyNSu.Functions.TranslationManager.Translate("Nhập lý do từ chối tạm ứng lương:"), QLyNSu.Functions.TranslationManager.Translate("Lý Do Từ Chối"), "Không đủ điều kiện tạm ứng");
                if (string.IsNullOrWhiteSpace(reason)) return;
            }

            try
            {
                decimal currentUserId = GetCurrentUserId(null);
                string username = UserSession.CurrentUser?.USERNAME ?? "Admin";
                var advService = new Bu.CLASS_PAYROLL.SalaryAdvanceService();

                if (isApprove)
                {
                    await Task.Run(() => advService.ApproveRequest((long)id, (long)currentUserId, username, currentEmpId));
                }
                else
                {
                    await Task.Run(() => advService.RejectRequest((long)id, (long)currentUserId, reason, currentEmpId));
                }

                WriteAudit("TB_YEUCAU_UNGLUONG", id.ToString(), isApprove ? "APPROVE_ADVANCE" : "REJECT_ADVANCE", $"Đã {(isApprove ? "duyệt" : "từ chối")} đơn tạm ứng lương của [{empName}]");

                string successMsg = isApprove ? QLyNSu.Functions.TranslationManager.Translate("Đã phê duyệt đơn tạm ứng lương thành công!") : QLyNSu.Functions.TranslationManager.Translate("Đã từ chối đơn tạm ứng lương thành công!");
                XtraMessageBox.Show(successMsg, QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAllRequests();
                if (IsDisposed || Disposing) return;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Lỗi:") + " " + ex.Message, QLyNSu.Functions.TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ProcessAdvanceDisburse()
        {
            bool canDisburse = UserSession.CanEdit("F_PAYROLL_DISBURSE_ADVANCE");
            if (!canDisburse)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Bạn chưa được cấp quyền kế toán chi tiền tạm ứng (F_PAYROLL_DISBURSE_ADVANCE)."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int row = gvAdvance.FocusedRowHandle;
            if (row < 0)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Vui lòng chọn một đơn tạm ứng lương trên bảng."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal id = Convert.ToDecimal(gvAdvance.GetRowCellValue(row, "ID"));
            string currentStatus = Convert.ToString(gvAdvance.GetRowCellValue(row, "TRANGTHAI"));
            string empName = Convert.ToString(gvAdvance.GetRowCellValue(row, "EMPLOYEE_NAME"));
            decimal amount = Convert.ToDecimal(gvAdvance.GetRowCellValue(row, "SOTIEN"));

            if (currentStatus != "APPROVED")
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Chỉ có thể giải ngân cho đơn đã được cấp quản lý phê duyệt (APPROVED)."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string defaultVoucher = $"PC-UL-{DateTime.Now:yyyyMMdd}-{id}";
            string voucherNo = XtraInputBox.Show(QLyNSu.Functions.TranslationManager.Translate($"Nhập số chứng từ/phiếu chi tiền cho nhân viên [{empName}] (Số tiền: {amount:N0} VNĐ):"), QLyNSu.Functions.TranslationManager.Translate("Xác Nhận Chi Tiền"), defaultVoucher);
            if (string.IsNullOrWhiteSpace(voucherNo)) return;

            try
            {
                decimal currentUserId = GetCurrentUserId(null);
                var advService = new Bu.CLASS_PAYROLL.SalaryAdvanceService();
                long idUl = await Task.Run(() => advService.DisburseAdvance((long)id, (long)currentUserId, DateTime.Today, voucherNo.Trim(), "Kế toán giải ngân qua Desktop"));

                WriteAudit("TB_YEUCAU_UNGLUONG", id.ToString(), "DISBURSE_ADVANCE", $"Đã chi tạm ứng {amount:N0} VNĐ cho [{empName}] - Phiếu chi #{idUl}");

                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate($"Giải ngân tạm ứng thành công! Mã phiếu ứng lương: #{idUl}"), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAllRequests();
                if (IsDisposed || Disposing) return;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Lỗi giải ngân:") + " " + ex.Message, QLyNSu.Functions.TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region PROFILE CHANGE TAB LOGIC
        private void FilterProfileGrid()
        {
            if (_rawProfiles == null) return;
            var list = _rawProfiles.AsEnumerable();

            int grpIdx = cboProfileGroup != null ? cboProfileGroup.SelectedIndex : 0;
            if (grpIdx == 1) list = list.Where(x => x.GROUP_CODE == "PERSONAL_INFO");
            else if (grpIdx == 2) list = list.Where(x => x.GROUP_CODE == "INSURANCE_INFO");
            else if (grpIdx == 3) list = list.Where(x => x.GROUP_CODE == "DEPENDENTS");
            else if (grpIdx == 4) list = list.Where(x => x.GROUP_CODE == "AVATAR");

            int statusIdx = cboProfileStatus != null ? cboProfileStatus.SelectedIndex : 0;
            if (statusIdx == 1) list = list.Where(x => x.TRANGTHAI == "PENDING");
            else if (statusIdx == 2) list = list.Where(x => x.TRANGTHAI == "APPROVED");
            else if (statusIdx == 3) list = list.Where(x => x.TRANGTHAI == "REJECTED");

            string search = txtProfileSearch?.Text?.Trim()?.ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                list = list.Where(x => (x.EMPLOYEE_NAME != null && x.EMPLOYEE_NAME.ToLower().Contains(search))
                                    || (x.EMPLOYEE_CODE != null && x.EMPLOYEE_CODE.ToLower().Contains(search))
                                    || (x.REASON != null && x.REASON.ToLower().Contains(search)));
            }

            gcProfile.DataSource = ConvertListToDataTable(list.ToList());
            gvProfile.PopulateColumns();
            ConfigureProfileColumns();
        }

        private void ConfigureProfileColumns()
        {
            SetColumn(gvProfile, "ID", "Mã Đơn", 70);
            SetColumn(gvProfile, "MANV", "Mã NV", 65);
            SetColumn(gvProfile, "EMPLOYEE_CODE", "Mã Nhân Sự", 100);
            SetColumn(gvProfile, "EMPLOYEE_NAME", "Họ và Tên", 150);
            SetColumn(gvProfile, "DEPARTMENT_NAME", "Phòng Ban", 130);
            SetColumn(gvProfile, "GROUP_CODE", "Nhóm Hồ Sơ", 120);
            SetColumn(gvProfile, "REQ_TYPE", "Loại Yêu Cầu", 110);
            SetColumn(gvProfile, "REASON", "Lý Do Đề Xuất", 180);
            SetColumn(gvProfile, "TRANGTHAI", "Trạng Thái", 100);
            SetColumn(gvProfile, "NGAY_TAO", "Thời Gian Gửi", 120);
            SetColumn(gvProfile, "NGUOI_DUYET", "Người Xử Lý", 120);
            SetColumn(gvProfile, "NGAY_DUYET", "Thời Gian Duyệt", 120);
            SetColumn(gvProfile, "LYDO_TUCHOI", "Lý Do Từ Chối", 150);
            gvProfile.BestFitColumns();
        }

        private async Task ProcessProfileApproval(bool isApprove)
        {
            bool canApprove = CanApproveRequests() || UserSession.CanEdit("F_APPROVE_PROFILE");
            if (!canApprove)
            {
                ApplyApprovalPermissions();
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Bạn chưa được cấp quyền duyệt/từ chối yêu cầu thay đổi hồ sơ."), "Thông báo");
                return;
            }
            int row = gvProfile.FocusedRowHandle;
            if (row < 0)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Vui lòng chọn một yêu cầu thay đổi hồ sơ trên bảng."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal id = Convert.ToDecimal(gvProfile.GetRowCellValue(row, "ID"));
            decimal? manv = gvProfile.GetRowCellValue(row, "MANV") != null ? Convert.ToDecimal(gvProfile.GetRowCellValue(row, "MANV")) : (decimal?)null;
            string empName = Convert.ToString(gvProfile.GetRowCellValue(row, "EMPLOYEE_NAME"));
            string currentStatus = Convert.ToString(gvProfile.GetRowCellValue(row, "TRANGTHAI"));

            decimal? currentEmpId = UserSession.CurrentUser?.MANV;
            if (currentEmpId.HasValue && manv.HasValue && currentEmpId.Value == manv.Value)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Bạn không thể tự phê duyệt hoặc từ chối đơn của chính mình!"), QLyNSu.Functions.TranslationManager.Translate("Cảnh báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (currentStatus != "PENDING")
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Chỉ có thể xử lý các đơn đang ở trạng thái PENDING (Chờ duyệt)."), QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string reason = null;
            if (!isApprove)
            {
                reason = XtraInputBox.Show(QLyNSu.Functions.TranslationManager.Translate("Nhập lý do từ chối yêu cầu thay đổi hồ sơ:"), QLyNSu.Functions.TranslationManager.Translate("Lý Do Từ Chối"), "Thông tin không khớp giấy tờ gốc");
                if (string.IsNullOrWhiteSpace(reason)) return;
            }

            try
            {
                decimal currentUserId = GetCurrentUserId(null);
                var profService = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();

                if (isApprove)
                {
                    await Task.Run(() => profService.ApproveRequest((long)id, (long)currentUserId, currentEmpId));
                }
                else
                {
                    await Task.Run(() => profService.RejectRequest((long)id, (long)currentUserId, reason, currentEmpId));
                }

                WriteAudit("TB_YEUCAU_THAYDOI_HOSO", id.ToString(), isApprove ? "APPROVE_PROFILE" : "REJECT_PROFILE", $"Đã {(isApprove ? "duyệt" : "từ chối")} thay đổi hồ sơ của [{empName}]");

                string successMsg = isApprove ? QLyNSu.Functions.TranslationManager.Translate("Đã phê duyệt và cập nhật hồ sơ thành công!") : QLyNSu.Functions.TranslationManager.Translate("Đã từ chối yêu cầu thay đổi hồ sơ thành công!");
                XtraMessageBox.Show(successMsg, QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAllRequests();
                if (IsDisposed || Disposing) return;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Lỗi:") + " " + ex.Message, QLyNSu.Functions.TranslationManager.Translate("Lỗi"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewProfileDiff()
        {
            int row = gvProfile.FocusedRowHandle;
            if (row < 0) return;

            string groupCode = Convert.ToString(gvProfile.GetRowCellValue(row, "GROUP_CODE"));
            string currentData = Convert.ToString(gvProfile.GetRowCellValue(row, "CURRENT_DATA"));
            string proposedData = Convert.ToString(gvProfile.GetRowCellValue(row, "PROPOSED_DATA"));
            bool hasBlob = Convert.ToBoolean(gvProfile.GetRowCellValue(row, "HAS_BLOB"));
            decimal id = Convert.ToDecimal(gvProfile.GetRowCellValue(row, "ID"));
            string empName = Convert.ToString(gvProfile.GetRowCellValue(row, "EMPLOYEE_NAME"));

            if (groupCode == "AVATAR" || hasBlob)
            {
                try
                {
                    var profService = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                    byte[] bytes = profService.GetProposedAvatar((long)id);
                    if (bytes != null && bytes.Length > 0)
                    {
                        using (var ms = new System.IO.MemoryStream(bytes))
                        {
                            Image img = Image.FromStream(ms);
                            Form formImg = new Form
                            {
                                Text = $"Ảnh đại diện đề xuất - [{empName}] (Đơn #{id})",
                                Size = new Size(400, 450),
                                StartPosition = FormStartPosition.CenterParent
                            };
                            PictureBox pb = new PictureBox
                            {
                                Image = img,
                                Dock = DockStyle.Fill,
                                SizeMode = PictureBoxSizeMode.Zoom
                            };
                            formImg.Controls.Add(pb);
                            formImg.ShowDialog(this);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Không thể tải ảnh đại diện: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Hiển thị hộp thoại so sánh JSON dữ liệu Trước / Sau
            Form diffForm = new Form
            {
                Text = $"So sánh hồ sơ: Trước vs Sau đề xuất - [{empName}] (Nhóm: {groupCode})",
                Size = new Size(820, 520),
                StartPosition = FormStartPosition.CenterParent
            };
            TableLayoutPanel tlp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            LabelControl lblBefore = new LabelControl { Text = "DỮ LIỆU HIỆN TẠI (TRƯỚC KHI ĐỔI)", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.DarkSlateGray };
            LabelControl lblAfter = new LabelControl { Text = "DỮ LIỆU ĐỀ XUẤT (SẼ ÁP DỤNG NẾU DUYỆT)", Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), ForeColor = Color.DarkBlue };

            TextBox txtBefore = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill, Text = currentData ?? "(Không có dữ liệu gốc)", Font = new Font("Consolas", 9.5F) };
            TextBox txtAfter = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill, Text = proposedData ?? "(Không có dữ liệu đề xuất)", Font = new Font("Consolas", 9.5F), BackColor = Color.FromArgb(245, 255, 245) };

            tlp.Controls.Add(lblBefore, 0, 0);
            tlp.Controls.Add(lblAfter, 1, 0);
            tlp.Controls.Add(txtBefore, 0, 1);
            tlp.Controls.Add(txtAfter, 1, 1);

            diffForm.Controls.Add(tlp);
            diffForm.ShowDialog(this);
        }
        #endregion

        #region UTILITIES
        private decimal GetCurrentUserId(MyEntities db)
        {
            if (UserSession.CurrentUser != null && UserSession.CurrentUser.IDUSER > 0)
            {
                return (decimal)UserSession.CurrentUser.IDUSER;
            }
            throw new BusinessException("UNAUTHENTICATED", "Không có tài khoản đăng nhập hợp lệ để duyệt yêu cầu.");
        }

        private void SetColumn(GridView gv, string fieldName, string caption, int width)
        {
            GridColumn col = gv.Columns[fieldName];
            if (col == null)
            {
                col = gv.Columns.AddField(fieldName);
            }
            col.Caption = QLyNSu.Functions.TranslationManager.Translate(caption);
            col.Width = width;
            col.Visible = true;
        }

        private void WriteAudit(string tableName, string recordId, string action, string detail)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    string sql = @"
                        INSERT INTO HR.TB_SYS_LOG (
                            MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI, DU_LIEU_MOI, IP_ADDRESS, TEN_MAY_TINH, THOIGIAN, MODULE_NAME, CHANGED_FIELDS
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4, :p5, '127.0.0.1', 'DESKTOP_WINFORMS', SYSDATE, 'APPROVAL', :p6
                        )";

                    db.Database.ExecuteSqlCommand(
                        sql,
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", UserSession.CurrentUser?.MANV?.ToString() ?? "1"),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", UserSession.CurrentUser?.USERNAME ?? "ADMIN"),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p2", action),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p3", tableName),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p4", recordId),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p5", detail),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p6", action)
                    );
                }
            }
            catch { }
        }

        private void ExportGrid(GridView gv, string defaultFileName)
        {
            if (!UserSession.CanView("F_NV_PHEDUYET") || !UserSession.CanPrint("F_NV_PHEDUYET"))
            {
                ApplyApprovalPermissions();
                XtraMessageBox.Show("Bạn chưa được cấp quyền xuất danh sách yêu cầu.", "Thông báo");
                return;
            }
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Files (*.xlsx)|*.xlsx";
                sfd.FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    gv.ExportToXlsx(sfd.FileName);
                    XtraMessageBox.Show(QLyNSu.Functions.TranslationManager.Translate("Xuất file Excel thành công:\n") + sfd.FileName, QLyNSu.Functions.TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private DataTable ConvertListToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            System.Reflection.PropertyInfo[] Props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in Props)
            {
                Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dataTable.Columns.Add(prop.Name, propType);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
        #endregion

        #region DTOs
        private class AttDetailDto
        {
            public decimal ID { get; set; }
            public decimal? MANV { get; set; }
            public DateTime? NGAY { get; set; }
            public string GIO_VAO { get; set; }
            public string GIO_RA { get; set; }
        }

        private class OtDetailDto
        {
            public decimal ID { get; set; }
            public decimal? MANV { get; set; }
            public DateTime? NGAY { get; set; }
            public decimal? GIOTANGCA { get; set; }
            public decimal? IDCA { get; set; }
            public decimal? HE_SO { get; set; }
            public string LYDO { get; set; }
            public string TRANGTHAI { get; set; }
        }

        private class LeaveRow
        {
            public decimal ID { get; set; }
            public decimal? MANV { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public string EMPLOYEE_NAME { get; set; }
            public string DEPARTMENT_NAME { get; set; }
            public string LOAI_NGHI { get; set; }
            public string TU_NGAY { get; set; }
            public string DEN_NGAY { get; set; }
            public decimal? SO_NGAY { get; set; }
            public string LYDO { get; set; }
            public string TRANGTHAI { get; set; }
            public string NGAY_TAO { get; set; }
            public string NGUOI_DUYET { get; set; }
            public string LYDO_TUCHOI { get; set; }
        }

        private class AttRow
        {
            public decimal ID { get; set; }
            public decimal? MANV { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public string EMPLOYEE_NAME { get; set; }
            public string DEPARTMENT_NAME { get; set; }
            public string NGAY_CONG { get; set; }
            public string GIO_VAO_MOI { get; set; }
            public string GIO_RA_MOI { get; set; }
            public string LYDO { get; set; }
            public string TRANGTHAI { get; set; }
            public string NGAY_TAO { get; set; }
            public string NGUOI_DUYET { get; set; }
            public string LYDO_TUCHOI { get; set; }
        }

        private class OtRow
        {
            public decimal ID { get; set; }
            public decimal? MANV { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public string EMPLOYEE_NAME { get; set; }
            public string DEPARTMENT_NAME { get; set; }
            public string NGAY_TANGCA { get; set; }
            public decimal? SO_GIO { get; set; }
            public decimal? HE_SO { get; set; }
            public string NOIDUNG { get; set; }
            public string TRANGTHAI { get; set; }
            public string NGAY_TAO { get; set; }
            public string NGUOI_DUYET { get; set; }
            public string LYDO_TUCHOI { get; set; }
        }

        private class AdvanceRow
        {
            public decimal ID { get; set; }
            public decimal? MANV { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public string EMPLOYEE_NAME { get; set; }
            public string DEPARTMENT_NAME { get; set; }
            public decimal SOTIEN { get; set; }
            public string NGAY_DENGHI { get; set; }
            public string KY_LUONG { get; set; }
            public string LYDO { get; set; }
            public string TRANGTHAI { get; set; }
            public string NGAY_TAO { get; set; }
            public string NGUOI_DUYET { get; set; }
            public string NGAY_DUYET { get; set; }
            public string LYDO_TUCHOI { get; set; }
            public string SO_CHUNG_TU { get; set; }
            public string NGAY_CHI { get; set; }
            public string DISBURSED_BY_NAME { get; set; }
            public decimal? IDUL_LINK { get; set; }
        }

        private class ProfileChangeRowRaw
        {
            public decimal ID { get; set; }
            public decimal? MANV { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public string EMPLOYEE_NAME { get; set; }
            public string DEPARTMENT_NAME { get; set; }
            public string GROUP_CODE { get; set; }
            public string REQ_TYPE { get; set; }
            public string CURRENT_DATA { get; set; }
            public string PROPOSED_DATA { get; set; }
            public decimal HAS_BLOB_VAL { get; set; }
            public string REASON { get; set; }
            public string TRANGTHAI { get; set; }
            public string NGAY_TAO { get; set; }
            public string NGUOI_DUYET { get; set; }
            public string NGAY_DUYET { get; set; }
            public string LYDO_TUCHOI { get; set; }
        }

        private class ProfileChangeRow
        {
            public decimal ID { get; set; }
            public decimal? MANV { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public string EMPLOYEE_NAME { get; set; }
            public string DEPARTMENT_NAME { get; set; }
            public string GROUP_CODE { get; set; }
            public string REQ_TYPE { get; set; }
            public string CURRENT_DATA { get; set; }
            public string PROPOSED_DATA { get; set; }
            public bool HAS_BLOB { get; set; }
            public string REASON { get; set; }
            public string TRANGTHAI { get; set; }
            public string NGAY_TAO { get; set; }
            public string NGUOI_DUYET { get; set; }
            public string NGAY_DUYET { get; set; }
            public string LYDO_TUCHOI { get; set; }
        }
        #endregion
    }
}
