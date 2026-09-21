using Bu.CLASS_SYSTEM;
using DA;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
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

        public FrmPheDuyetYeuCau()
        {
            InitializeComponent();
        }

        private void FrmPheDuyetYeuCau_Load(object sender, EventArgs e)
        {
            SetupGridViewAppearance(gvLeave);
            SetupGridViewAppearance(gvAttendance);
            SetupGridViewAppearance(gvOvertime);

            cboLeaveStatus.SelectedIndex = 1; // Default PENDING
            cboAttStatus.SelectedIndex = 1;   // Default PENDING
            cboOtStatus.SelectedIndex = 1;    // Default PENDING

            LoadAllRequests();
        }

        private void SetupGridViewAppearance(GridView gv)
        {
            gv.OptionsBehavior.Editable = false;
            gv.OptionsView.ShowGroupPanel = false;
            gv.OptionsView.EnableAppearanceEvenRow = true;
            gv.OptionsSelection.EnableAppearanceFocusedCell = false;
            gv.OptionsSelection.MultiSelect = false;
            gv.RowHeight = 28;
            gv.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gv.Appearance.Row.Font = new Font("Segoe UI", 9F);

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
                }
            };
        }

        #region LOAD ALL REQUESTS (DIRECT ORACLE)
        public void LoadAllRequests()
        {
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

                    _rawLeaves = db.Database.SqlQuery<LeaveRow>(leaveSql).ToList();

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

                    _rawAtts = db.Database.SqlQuery<AttRow>(attSql).ToList();

                    // 3. Đăng Ký Tăng Ca (TB_YEUCAU_TANGCA)
                    string otSql = @"
                        SELECT 
                            Y.ID AS ID,
                            Y.MANV AS MANV,
                            NV.EMPLOYEE_CODE AS EMPLOYEE_CODE,
                            NV.HOTEN AS EMPLOYEE_NAME,
                            PB.TENPB AS DEPARTMENT_NAME,
                            TO_CHAR(Y.NGAY, 'DD/MM/YYYY') AS NGAY_TANGCA,
                            Y.GIOTANGCA AS SO_GIO,
                            1.5 AS HE_SO,
                            Y.LYDO AS NOIDUNG,
                            Y.TRANGTHAI AS TRANGTHAI,
                            TO_CHAR(Y.CREATED_DATE, 'DD/MM/YYYY HH24:MI') AS NGAY_TAO,
                            NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET,
                            Y.GHICHUDUYET AS LYDO_TUCHOI
                        FROM HR.TB_YEUCAU_TANGCA Y
                        LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                        LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                        LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                        ORDER BY Y.ID DESC";

                    _rawOts = db.Database.SqlQuery<OtRow>(otSql).ToList();

                    // Cập nhật các chỉ số KPI
                    int pLeave = _rawLeaves.Count(x => x.TRANGTHAI == "PENDING");
                    int pAtt = _rawAtts.Count(x => x.TRANGTHAI == "PENDING");
                    int pOt = _rawOts.Count(x => x.TRANGTHAI == "PENDING");

                    lblPendingTotalVal.Text = (pLeave + pAtt + pOt).ToString("N0");
                    lblPendingLeaveVal.Text = pLeave.ToString("N0");
                    lblPendingAttVal.Text = pAtt.ToString("N0");
                    lblPendingOtVal.Text = pOt.ToString("N0");

                    // Áp dụng bộ lọc cho các bảng
                    FilterLeaveGrid();
                    FilterAttGrid();
                    FilterOtGrid();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi tải dữ liệu yêu cầu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRefreshAll_Click(object sender, EventArgs e)
        {
            LoadAllRequests();
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

            string status = cboLeaveStatus?.SelectedItem?.ToString();
            if (status == "Chờ duyệt (PENDING)") list = list.Where(x => x.TRANGTHAI == "PENDING");
            else if (status == "Đã duyệt (APPROVED)") list = list.Where(x => x.TRANGTHAI == "APPROVED");
            else if (status == "Đã từ chối (REJECTED)") list = list.Where(x => x.TRANGTHAI == "REJECTED");

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

        private void btnApproveLeave_Click(object sender, EventArgs e)
        {
            ProcessLeaveApproval(true);
        }

        private void btnRejectLeave_Click(object sender, EventArgs e)
        {
            ProcessLeaveApproval(false);
        }

        private void btnExportLeave_Click(object sender, EventArgs e)
        {
            ExportGrid(gvLeave, "DonNghiPhep");
        }

        private void ProcessLeaveApproval(bool isApprove)
        {
            int row = gvLeave.FocusedRowHandle;
            if (row < 0)
            {
                XtraMessageBox.Show("Vui lòng chọn một đơn xin nghỉ phép trên bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal id = Convert.ToDecimal(gvLeave.GetRowCellValue(row, "ID"));
            string empName = Convert.ToString(gvLeave.GetRowCellValue(row, "EMPLOYEE_NAME"));
            string currentStatus = Convert.ToString(gvLeave.GetRowCellValue(row, "TRANGTHAI"));

            if (currentStatus != "PENDING")
            {
                XtraMessageBox.Show("Chỉ có thể xử lý các đơn đang ở trạng thái PENDING (Chờ duyệt).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string reason = null;
            if (!isApprove)
            {
                reason = XtraInputBox.Show("Nhập lý do từ chối đơn nghỉ phép:", "Lý Do Từ Chối", "Không phù hợp lịch công tác");
                if (string.IsNullOrWhiteSpace(reason)) return;
            }

            try
            {
                using (var db = new MyEntities())
                {
                    decimal currentUserId = GetCurrentUserId(db);

                    db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_YEUCAU_NGHIPHEP SET TRANGTHAI = :p0, NGUOIDUYET = :p1, NGAYDUYET = SYSDATE, GHICHUDUYET = :p2 WHERE ID = :p3",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", isApprove ? "APPROVED" : "REJECTED"),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", currentUserId),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p2", (object)reason ?? (isApprove ? "Đã duyệt qua Desktop" : "Từ chối")),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p3", id)
                    );

                    WriteAudit("TB_YEUCAU_NGHIPHEP", id.ToString(), isApprove ? "APPROVE_LEAVE" : "REJECT_LEAVE", $"Đã {(isApprove ? "duyệt" : "từ chối")} đơn nghỉ phép của [{empName}]");

                    XtraMessageBox.Show($"Đã {(isApprove ? "phê duyệt" : "từ chối")} đơn nghỉ phép thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllRequests();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            string status = cboAttStatus?.SelectedItem?.ToString();
            if (status == "Chờ duyệt (PENDING)") list = list.Where(x => x.TRANGTHAI == "PENDING");
            else if (status == "Đã duyệt (APPROVED)") list = list.Where(x => x.TRANGTHAI == "APPROVED");
            else if (status == "Đã từ chối (REJECTED)") list = list.Where(x => x.TRANGTHAI == "REJECTED");

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

        private void btnApproveAtt_Click(object sender, EventArgs e)
        {
            ProcessAttendanceApproval(true);
        }

        private void btnRejectAtt_Click(object sender, EventArgs e)
        {
            ProcessAttendanceApproval(false);
        }

        private void btnExportAtt_Click(object sender, EventArgs e)
        {
            ExportGrid(gvAttendance, "GiaiTrinhChamCong");
        }

        private void ProcessAttendanceApproval(bool isApprove)
        {
            int row = gvAttendance.FocusedRowHandle;
            if (row < 0)
            {
                XtraMessageBox.Show("Vui lòng chọn một bản ghi giải trình chấm công trên bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal id = Convert.ToDecimal(gvAttendance.GetRowCellValue(row, "ID"));
            string empName = Convert.ToString(gvAttendance.GetRowCellValue(row, "EMPLOYEE_NAME"));
            string currentStatus = Convert.ToString(gvAttendance.GetRowCellValue(row, "TRANGTHAI"));

            if (currentStatus != "PENDING")
            {
                XtraMessageBox.Show("Chỉ có thể xử lý các đơn đang ở trạng thái PENDING (Chờ duyệt).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string reason = null;
            if (!isApprove)
            {
                reason = XtraInputBox.Show("Nhập lý do từ chối giải trình chấm công:", "Lý Do Từ Chối", "Không hợp lệ theo quy chế");
                if (string.IsNullOrWhiteSpace(reason)) return;
            }

            try
            {
                using (var db = new MyEntities())
                {
                    decimal currentUserId = GetCurrentUserId(db);

                    // 1. Cập nhật trạng thái
                    db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_YEUCAU_DIEUCHINHCONG SET TRANGTHAI = :p0, NGUOIDUYET = :p1, NGAYDUYET = SYSDATE, GHICHUDUYET = :p2 WHERE ID = :p3",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", isApprove ? "APPROVED" : "REJECTED"),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", currentUserId),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p2", (object)reason ?? (isApprove ? "Đã duyệt qua Desktop" : "Từ chối")),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p3", id)
                    );

                    // 2. Nếu phê duyệt -> tự động đồng bộ giờ vào/ra và ngày công vào TB_BANGCONG_CHITIET
                    if (isApprove)
                    {
                        var reqItem = db.Database.SqlQuery<AttDetailDto>(
                            "SELECT ID, MANV, NGAY, GIO_VAO, GIO_RA FROM HR.TB_YEUCAU_DIEUCHINHCONG WHERE ID = :p0",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", id)
                        ).FirstOrDefault();

                        if (reqItem != null && reqItem.NGAY.HasValue && reqItem.MANV.HasValue)
                        {
                            try
                            {
                                db.Database.ExecuteSqlCommand(
                                    "UPDATE HR.TB_BANGCONG_CHITIET SET GIOVAO = :p0, GIORA = :p1, NGAYCONG = 1, KYHIEU = 'X' " +
                                    "WHERE MANV = :p2 AND TRUNC(NGAY) = TRUNC(:p3)",
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p0", reqItem.GIO_VAO ?? "08:00"),
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p1", reqItem.GIO_RA ?? "17:00"),
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p2", reqItem.MANV.Value),
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p3", reqItem.NGAY.Value)
                                );
                            }
                            catch (Exception exSync)
                            {
                                System.Diagnostics.Trace.TraceWarning("Không thể đồng bộ sang TB_BANGCONG_CHITIET: " + exSync.Message);
                            }
                        }
                    }

                    WriteAudit("TB_YEUCAU_DIEUCHINHCONG", id.ToString(), isApprove ? "APPROVE_ATTENDANCE" : "REJECT_ATTENDANCE", $"Đã {(isApprove ? "duyệt" : "từ chối")} giải trình công của [{empName}]");

                    XtraMessageBox.Show($"Đã {(isApprove ? "phê duyệt" : "từ chối")} giải trình chấm công thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllRequests();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            string status = cboOtStatus?.SelectedItem?.ToString();
            if (status == "Chờ duyệt (PENDING)") list = list.Where(x => x.TRANGTHAI == "PENDING");
            else if (status == "Đã duyệt (APPROVED)") list = list.Where(x => x.TRANGTHAI == "APPROVED");
            else if (status == "Đã từ chối (REJECTED)") list = list.Where(x => x.TRANGTHAI == "REJECTED");

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

        private void btnApproveOt_Click(object sender, EventArgs e)
        {
            ProcessOvertimeApproval(true);
        }

        private void btnRejectOt_Click(object sender, EventArgs e)
        {
            ProcessOvertimeApproval(false);
        }

        private void btnExportOt_Click(object sender, EventArgs e)
        {
            ExportGrid(gvOvertime, "DangKyTangCa");
        }

        private void ProcessOvertimeApproval(bool isApprove)
        {
            int row = gvOvertime.FocusedRowHandle;
            if (row < 0)
            {
                XtraMessageBox.Show("Vui lòng chọn một đơn đăng ký tăng ca trên bảng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal id = Convert.ToDecimal(gvOvertime.GetRowCellValue(row, "ID"));
            string empName = Convert.ToString(gvOvertime.GetRowCellValue(row, "EMPLOYEE_NAME"));
            string currentStatus = Convert.ToString(gvOvertime.GetRowCellValue(row, "TRANGTHAI"));

            if (currentStatus != "PENDING")
            {
                XtraMessageBox.Show("Chỉ có thể xử lý các đơn đang ở trạng thái PENDING (Chờ duyệt).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string reason = null;
            if (!isApprove)
            {
                reason = XtraInputBox.Show("Nhập lý do từ chối đăng ký làm thêm giờ:", "Lý Do Từ Chối", "Kế hoạch sản xuất không yêu cầu");
                if (string.IsNullOrWhiteSpace(reason)) return;
            }

            try
            {
                using (var db = new MyEntities())
                {
                    decimal currentUserId = GetCurrentUserId(db);

                    var reqItem = db.Database.SqlQuery<OtDetailDto>(
                        @"SELECT Y.ID, Y.MANV, Y.NGAY, Y.GIOTANGCA, Y.IDCA, NVL(L.HESO, 1.0) AS HE_SO, Y.LYDO, Y.TRANGTHAI 
                          FROM HR.TB_YEUCAU_TANGCA Y 
                          LEFT JOIN HR.TB_LOAICA L ON Y.IDCA = L.IDLOAICA 
                          WHERE Y.ID = :p0",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", id)
                    ).FirstOrDefault();

                    if (reqItem == null)
                    {
                        XtraMessageBox.Show("Không tìm thấy đề xuất tăng ca.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (reqItem.TRANGTHAI != "PENDING")
                    {
                        XtraMessageBox.Show($"Đề xuất này đang ở trạng thái '{reqItem.TRANGTHAI}', không thể xử lý tiếp!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Security: Ngăn chặn tự phê duyệt / tự từ chối của chính mình
                    var approverEmpId = db.Database.SqlQuery<decimal?>(
                        "SELECT EMPLOYEE_ID FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", currentUserId)
                    ).FirstOrDefault();

                    if (approverEmpId.HasValue && approverEmpId.Value == reqItem.MANV)
                    {
                        XtraMessageBox.Show("Bạn không thể tự phê duyệt hoặc từ chối đề xuất tăng ca của chính mình!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

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
                        XtraMessageBox.Show("Đề xuất không còn ở trạng thái chờ duyệt hoặc đã có người khác xử lý.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
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
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p7", reqItem.IDCA ?? 1),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p8", id),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p9", currentUserId)
                            );
                        }
                        catch (Exception exTc)
                        {
                            System.Diagnostics.Trace.TraceWarning("Không thể tự động thêm vào TB_TANGCA: " + exTc.Message);
                        }
                    }

                    WriteAudit("TB_YEUCAU_TANGCA", id.ToString(), isApprove ? "APPROVE_OVERTIME" : "REJECT_OVERTIME", $"Đã {(isApprove ? "duyệt" : "từ chối")} đăng ký tăng ca của [{empName}]");

                    XtraMessageBox.Show($"Đã {(isApprove ? "phê duyệt" : "từ chối")} đăng ký làm thêm giờ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllRequests();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region UTILITIES
        private decimal GetCurrentUserId(MyEntities db)
        {
            if (UserSession.CurrentUser != null && UserSession.CurrentUser.IDUSER > 0)
            {
                return (decimal)UserSession.CurrentUser.IDUSER;
            }
            var adminId = db.Database.SqlQuery<decimal?>("SELECT IDUSER FROM HR.TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND ROWNUM = 1").FirstOrDefault();
            return adminId ?? 1;
        }

        private void SetColumn(GridView gv, string fieldName, string caption, int width)
        {
            GridColumn col = gv.Columns[fieldName];
            if (col == null)
            {
                col = gv.Columns.AddField(fieldName);
            }
            col.Caption = caption;
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
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Files (*.xlsx)|*.xlsx";
                sfd.FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    gv.ExportToXlsx(sfd.FileName);
                    XtraMessageBox.Show("Xuất file Excel thành công:\n" + sfd.FileName, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        #endregion
    }
}
