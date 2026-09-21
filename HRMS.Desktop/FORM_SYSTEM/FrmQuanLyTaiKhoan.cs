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

namespace QLyNSu.FORM_SYSTEM
{
    public partial class FrmQuanLyTaiKhoan : XtraForm
    {
        private DataTable _dtUsers;
        private DataTable _dtCandidates;
        private DataTable _dtBulkResults;

        public FrmQuanLyTaiKhoan()
        {
            InitializeComponent();
        }

        private void FrmQuanLyTaiKhoan_Load(object sender, EventArgs e)
        {
            if (this.DesignMode || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            SetupGridAppearance(gvUsers);
            SetupGridAppearance(gvCandidates);
            SetupGridAppearance(gvBulkResults);

            LoadDepartments();
            LoadAllData();
            LoadBulkPreview();

            // Tự động tải lại danh sách ứng viên khi thay đổi tiêu chí lọc
            cboBulkDept.EditValueChanged += (s, ev) =>
            {
                if (tabBulkSub != null) tabBulkSub.SelectedTabPage = tabSubCandidates;
                LoadBulkPreview();
            };
            chkBulkOnlyWithoutAcc.CheckedChanged += (s, ev) =>
            {
                if (tabBulkSub != null) tabBulkSub.SelectedTabPage = tabSubCandidates;
                LoadBulkPreview();
            };

            QLyNSu.Functions.TranslationManager.Translate(this);
        }

        private void SetupGridAppearance(GridView gv)
        {
            gv.OptionsView.EnableAppearanceEvenRow = true;
            gv.RowHeight = 26;
            gv.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gv.Appearance.Row.Font = new Font("Segoe UI", 9F);
        }

        private void tabMain_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (e.Page == tabCapPhatHangLoat && _dtCandidates == null)
            {
                LoadBulkPreview();
            }
        }

        #region DATA LOADING & DIRECT ORACLE ACCESS
        private void LoadDepartments()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var depts = db.Database.SqlQuery<DeptItem>("SELECT IDPB, TENPB FROM HR.TB_PHONGBAN ORDER BY TENPB").ToList();
                    depts.Insert(0, new DeptItem { IDPB = 0, TENPB = QLyNSu.Functions.TranslationManager.Translate("-- Tất cả phòng ban --") });

                    cboPhongBan.Properties.DataSource = depts;
                    cboPhongBan.Properties.DisplayMember = "TENPB";
                    cboPhongBan.Properties.ValueMember = "IDPB";
                    cboPhongBan.EditValue = 0;

                    cboBulkDept.Properties.DataSource = depts;
                    cboBulkDept.Properties.DisplayMember = "TENPB";
                    cboBulkDept.Properties.ValueMember = "IDPB";
                    cboBulkDept.EditValue = 0;
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi tải danh mục phòng ban: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadAllData()
        {
            LoadStatsDirectOracle();
            LoadUsersDirectOracle();
        }

        private void LoadStatsDirectOracle()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    int totalEmp = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM HR.TB_NHANVIEN WHERE (DATHOIVIEC IS NULL OR DATHOIVIEC = 0) AND DELETED_BY IS NULL"
                    ).FirstOrDefault();

                    int accCreated = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM HR.TB_SYS_USER WHERE (ISGROUP IS NULL OR ISGROUP = 0)"
                    ).FirstOrDefault();

                    int noAcc = db.Database.SqlQuery<int>(@"
                        SELECT COUNT(*) FROM HR.TB_NHANVIEN NV
                        WHERE (NV.DATHOIVIEC IS NULL OR NV.DATHOIVIEC = 0)
                          AND NV.DELETED_BY IS NULL
                          AND NOT EXISTS (SELECT 1 FROM HR.TB_USER_EMPLOYEE_MAPPING M WHERE M.EMPLOYEE_ID = NV.MANV)
                          AND NOT EXISTS (SELECT 1 FROM HR.TB_SYS_USER U WHERE U.MANV = NV.MANV AND (U.ISGROUP IS NULL OR U.ISGROUP = 0))"
                    ).FirstOrDefault();

                    int mobOn = db.Database.SqlQuery<int>(@"
                        SELECT COUNT(*) FROM HR.TB_SYS_USER U
                        LEFT JOIN HR.TB_USER_EMPLOYEE_MAPPING M ON U.IDUSER = M.USER_ID
                        WHERE (U.ISGROUP IS NULL OR U.ISGROUP = 0)
                          AND UPPER(TRIM(U.USERNAME)) <> 'ADMIN'
                          AND (M.IS_MOBILE_ENABLED = 1 OR (M.IS_MOBILE_ENABLED IS NULL AND UPPER(NVL(U.CLIENT_TYPE, 'ALL')) = 'MOBILE'))"
                    ).FirstOrDefault();

                    int mobOff = db.Database.SqlQuery<int>(@"
                        SELECT COUNT(*) FROM HR.TB_SYS_USER U
                        LEFT JOIN HR.TB_USER_EMPLOYEE_MAPPING M ON U.IDUSER = M.USER_ID
                        WHERE (U.ISGROUP IS NULL OR U.ISGROUP = 0)
                          AND UPPER(TRIM(U.USERNAME)) <> 'ADMIN'
                          AND (M.IS_MOBILE_ENABLED = 0 OR (M.IS_MOBILE_ENABLED IS NULL AND UPPER(NVL(U.CLIENT_TYPE, 'ALL')) <> 'MOBILE'))"
                    ).FirstOrDefault();

                    int locked = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM HR.TB_SYS_USER WHERE (ISGROUP IS NULL OR ISGROUP = 0) AND DISABLED = 1"
                    ).FirstOrDefault();

                    int sysAcc = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM HR.TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' OR (ISGROUP = 1) OR UPPER(TRIM(NVL(CLIENT_TYPE, 'ALL'))) IN ('SYSTEM', 'DESKTOP', 'WEB')"
                    ).FirstOrDefault();

                    lblTotalEmpVal.Text = totalEmp.ToString("N0");
                    lblAccCreatedVal.Text = accCreated.ToString("N0");
                    lblNoAccVal.Text = noAcc.ToString("N0");
                    lblMobOnVal.Text = mobOn.ToString("N0");
                    lblMobOffVal.Text = mobOff.ToString("N0");
                    lblLockedVal.Text = locked.ToString("N0");
                    lblSysVal.Text = sysAcc.ToString("N0");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi tính toán chỉ số Dashboard: " + ex.Message);
            }
        }

        private void LoadUsersDirectOracle()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    string sql = @"
                        SELECT 
                            U.IDUSER,
                            U.USERNAME,
                            U.FULLNAME,
                            NV.EMPLOYEE_CODE,
                            NV.HOTEN AS EMPLOYEE_NAME,
                            PB.TENPB,
                            CV.TENCV,
                            CASE WHEN U.DISABLED = 1 THEN 'Bị tạm khóa' ELSE 'Hoạt động' END AS ACCOUNT_STATUS,
                            CASE 
                                WHEN UPPER(TRIM(U.USERNAME)) = 'ADMIN' THEN 'Tài khoản hệ thống (Admin)'
                                WHEN (U.ISGROUP = 1) THEN 'Nhóm quyền hệ thống'
                                WHEN UPPER(TRIM(NVL(U.CLIENT_TYPE, 'ALL'))) IN ('SYSTEM', 'DESKTOP', 'WEB') THEN 'Tài khoản hệ thống'
                                WHEN NV.MANV IS NOT NULL OR UPPER(TRIM(NVL(U.CLIENT_TYPE, 'ALL'))) = 'MOBILE' THEN 'Tài khoản người dùng (Nhân viên)'
                                ELSE 'Tài khoản hệ thống'
                            END AS ACCOUNT_TYPE,
                            CASE 
                                WHEN UPPER(TRIM(U.USERNAME)) = 'ADMIN' THEN 'BLOCKED (Cấm Mobile)'
                                WHEN UPPER(TRIM(NVL(U.CLIENT_TYPE, 'ALL'))) IN ('SYSTEM', 'DESKTOP', 'WEB') THEN 'BLOCKED (Tài khoản hệ thống)'
                                WHEN M.IS_MOBILE_ENABLED = 1 OR (M.IS_MOBILE_ENABLED IS NULL AND UPPER(NVL(U.CLIENT_TYPE, 'ALL')) = 'MOBILE') THEN 'ENABLED (Đã bật)'
                                ELSE 'DISABLED (Đang tắt)'
                            END AS MOBILE_STATUS,
                            NVL(U.CLIENT_TYPE, 'ALL') AS CLIENT_TYPE,
                            NV.MANV AS EMPLOYEE_ID
                        FROM HR.TB_SYS_USER U
                        LEFT JOIN HR.TB_USER_EMPLOYEE_MAPPING M ON U.IDUSER = M.USER_ID
                        LEFT JOIN HR.TB_NHANVIEN NV ON (M.EMPLOYEE_ID = NV.MANV OR (M.EMPLOYEE_ID IS NULL AND U.MANV = NV.MANV))
                        LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                        LEFT JOIN HR.TB_CHUCVU CV ON NV.IDCV = CV.IDCV
                        WHERE (U.ISGROUP IS NULL OR U.ISGROUP = 0)
                        ORDER BY U.IDUSER";

                    var rows = db.Database.SqlQuery<UserDisplayRow>(sql).ToList();
                    _dtUsers = ConvertListToDataTable(rows);
                    gcUsers.DataSource = _dtUsers;
                    gvUsers.PopulateColumns();
                    ConfigureUserGridColumns();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi nạp danh sách tài khoản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureUserGridColumns()
        {
            SetColumn(gvUsers, "IDUSER", "ID", 60);
            SetColumn(gvUsers, "USERNAME", "Tên Đăng Nhập", 110);
            SetColumn(gvUsers, "FULLNAME", "Tên Người Dùng", 140);
            SetColumn(gvUsers, "EMPLOYEE_CODE", "Mã Nhân Viên", 100);
            SetColumn(gvUsers, "EMPLOYEE_NAME", "Họ và Tên Nhân Sự", 150);
            SetColumn(gvUsers, "TENPB", "Phòng Ban", 130);
            SetColumn(gvUsers, "TENCV", "Chức Vụ", 120);
            SetColumn(gvUsers, "ACCOUNT_STATUS", "Trạng Thái", 100);
            SetColumn(gvUsers, "ACCOUNT_TYPE", "Loại Tài Khoản", 160);
            SetColumn(gvUsers, "MOBILE_STATUS", "Trạng Thái Mobile", 140);
            SetColumn(gvUsers, "CLIENT_TYPE", "Client Cho Phép", 100);
            SetColumn(gvUsers, "EMPLOYEE_ID", "Mã NV (DB)", 80);
            gvUsers.BestFitColumns();
        }

        private void ApplyUserFilter()
        {
            if (_dtUsers == null) return;

            string filter = "1=1";
            string kw = (txtSearch.Text ?? "").Trim().Replace("'", "''");
            if (!string.IsNullOrEmpty(kw))
            {
                filter += $" AND (USERNAME LIKE '%{kw}%' OR FULLNAME LIKE '%{kw}%' OR EMPLOYEE_CODE LIKE '%{kw}%' OR EMPLOYEE_NAME LIKE '%{kw}%')";
            }

            int deptId = Convert.ToInt32(cboPhongBan.EditValue ?? 0);
            if (deptId > 0)
            {
                string deptName = cboPhongBan.Text.Replace("'", "''");
                filter += $" AND (TENPB = '{deptName}')";
            }

            int statusIdx = cboTrangThai.SelectedIndex;
            if (statusIdx == 1)
                filter += " AND (ACCOUNT_STATUS = 'Hoạt động')";
            else if (statusIdx == 2)
                filter += " AND (ACCOUNT_STATUS = 'Bị tạm khóa')";
            else if (statusIdx == 3)
                filter += " AND (EMPLOYEE_CODE IS NOT NULL)";
            else if (statusIdx == 4)
                filter += " AND (EMPLOYEE_CODE IS NULL)";

            int mobIdx = cboMobileFilter.SelectedIndex;
            if (mobIdx == 1)
                filter += " AND (MOBILE_STATUS LIKE 'ENABLED%')";
            else if (mobIdx == 2)
                filter += " AND (MOBILE_STATUS LIKE 'DISABLED%')";
            else if (mobIdx == 3)
                filter += " AND (MOBILE_STATUS LIKE 'BLOCKED%')";

            DataView dv = _dtUsers.DefaultView;
            dv.RowFilter = filter;
            gcUsers.DataSource = dv;
        }

        private void ResetUserFilter()
        {
            txtSearch.Text = "";
            cboPhongBan.EditValue = 0;
            cboTrangThai.SelectedIndex = 0;
            cboMobileFilter.SelectedIndex = 0;
            if (_dtUsers != null)
            {
                _dtUsers.DefaultView.RowFilter = "";
                gcUsers.DataSource = _dtUsers.DefaultView;
            }
        }

        private void SwitchToDetail()
        {
            int rowHandle = gvUsers.FocusedRowHandle;
            if (rowHandle < 0) return;

            int userId = Convert.ToInt32(gvUsers.GetRowCellValue(rowHandle, "IDUSER"));
            ShowUserDetail(userId);
            tabMain.SelectedTabPage = tabChiTiet;
        }

        private void ShowUserDetail(int userId)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var u = db.TB_SYS_USER.FirstOrDefault(x => x.IDUSER == userId);
                    if (u == null) return;

                    txtDetailId.Text = u.IDUSER.ToString();
                    txtDetailUsername.Text = u.USERNAME;
                    txtDetailFullName.Text = u.FULLNAME ?? u.USERNAME;
                    cboDetailClientType.Text = u.CLIENT_TYPE ?? "ALL";
                    chkDetailDisabled.Checked = (u.DISABLED ?? 0) == 1;

                    bool isAdmin = u.USERNAME != null && u.USERNAME.Trim().ToUpper() == "ADMIN";

                    var map = db.Database.SqlQuery<MappingCheckRow>(
                        "SELECT EMPLOYEE_ID, IS_MOBILE_ENABLED FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", userId)
                    ).FirstOrDefault();

                    decimal? empId = map?.EMPLOYEE_ID ?? u.MANV;

                    if (empId.HasValue && empId.Value > 0)
                    {
                        var emp = db.Database.SqlQuery<EmpDetailRow>(@"
                            SELECT NV.MANV, NV.EMPLOYEE_CODE, NV.HOTEN, PB.TENPB, CV.TENCV
                            FROM HR.TB_NHANVIEN NV
                            LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                            LEFT JOIN HR.TB_CHUCVU CV ON NV.IDCV = CV.IDCV
                            WHERE NV.MANV = :p0",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", empId.Value)
                        ).FirstOrDefault();

                        if (emp != null)
                        {
                            txtDetailEmpId.Text = emp.MANV.ToString();
                            txtDetailEmpCode.Text = emp.EMPLOYEE_CODE ?? $"NV#{emp.MANV}";
                            txtDetailEmpName.Text = emp.HOTEN;
                            txtDetailDept.Text = emp.TENPB ?? "Chưa phân bổ";
                            txtDetailPos.Text = emp.TENCV ?? "Nhân viên";
                        }
                    }
                    else
                    {
                        txtDetailEmpId.Text = "Chưa liên kết";
                        txtDetailEmpCode.Text = "N/A";
                        txtDetailEmpName.Text = "N/A";
                        txtDetailDept.Text = "N/A";
                        txtDetailPos.Text = "N/A";
                    }

                    if (isAdmin)
                    {
                        lblDetailMobileBadge.Text = "⛔ BLOCKED (Tài khoản Quản trị tối cao bị cấm Mobile)";
                        lblDetailMobileBadge.Appearance.ForeColor = Color.Red;
                    }
                    else
                    {
                        bool isMob = map != null ? (map.IS_MOBILE_ENABLED ?? 1) == 1 : (u.CLIENT_TYPE ?? "ALL").ToUpperInvariant() == "MOBILE";
                        lblDetailMobileBadge.Text = isMob ? "✅ ENABLED (Được phép truy cập Mobile)" : "❌ DISABLED (Đang tắt)";
                        lblDetailMobileBadge.Appearance.ForeColor = isMob ? Color.Green : Color.Gray;
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi hiển thị chi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region EVENT HANDLERS - TAB 1 & TAB 2
        private void btnRefreshAll_Click(object sender, EventArgs e)
        {
            LoadAllData();
            LoadBulkPreview();
        }

        private void btnLoc_Click(object sender, EventArgs e)
        {
            ApplyUserFilter();
        }

        private void btnXoaLoc_Click(object sender, EventArgs e)
        {
            ResetUserFilter();
        }

        private void gvUsers_DoubleClick(object sender, EventArgs e)
        {
            SwitchToDetail();
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmUser())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    LoadAllData();
                }
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            SwitchToDetail();
        }

        private void btnKhoaMoKhoa_Click(object sender, EventArgs e)
        {
            int rowHandle = gvUsers.FocusedRowHandle;
            if (rowHandle < 0) return;

            int userId = Convert.ToInt32(gvUsers.GetRowCellValue(rowHandle, "IDUSER"));
            string username = Convert.ToString(gvUsers.GetRowCellValue(rowHandle, "USERNAME"));
            string status = Convert.ToString(gvUsers.GetRowCellValue(rowHandle, "ACCOUNT_STATUS"));

            if (username.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                XtraMessageBox.Show("Không thể khóa tài khoản Quản trị tối cao (ADMIN).", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isCurrentlyLocked = status == "Bị tạm khóa";
            int targetState = isCurrentlyLocked ? 0 : 1;

            try
            {
                using (var db = new MyEntities())
                {
                    db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_SYS_USER SET DISABLED = :p0 WHERE IDUSER = :p1",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", targetState),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", userId)
                    );

                    string action = targetState == 1 ? "LOCK_USER" : "UNLOCK_USER";
                    WriteAuditDirectOracle(action, userId.ToString(), $"{(targetState == 1 ? "Khóa" : "Mở khóa")} tài khoản [{username}]", UserSession.CurrentUser?.USERNAME ?? "ADMIN");

                    XtraMessageBox.Show($"Đã {(targetState == 1 ? "khóa" : "mở khóa")} tài khoản [{username}] thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllData();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi thao tác: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnResetMatKhau_Click(object sender, EventArgs e)
        {
            int rowHandle = gvUsers.FocusedRowHandle;
            if (rowHandle < 0) return;

            int userId = Convert.ToInt32(gvUsers.GetRowCellValue(rowHandle, "IDUSER"));
            string username = Convert.ToString(gvUsers.GetRowCellValue(rowHandle, "USERNAME"));

            string newPass = XtraInputBox.Show("Nhập mật khẩu mới cho tài khoản [" + username + "]:", "Đặt Lại Mật Khẩu", "123456");
            if (string.IsNullOrWhiteSpace(newPass)) return;

            try
            {
                string hashed = PasswordHasher.HashPassword(newPass.Trim());
                using (var db = new MyEntities())
                {
                    db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_SYS_USER SET PASSWORD = :p0, FAILED_LOGIN_COUNT = 0, LOCKOUT_END = NULL WHERE IDUSER = :p1",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", hashed),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", userId)
                    );

                    WriteAuditDirectOracle("RESET_PASSWORD", userId.ToString(), $"Đặt lại mật khẩu cho tài khoản [{username}]", UserSession.CurrentUser?.USERNAME ?? "ADMIN");

                    XtraMessageBox.Show("Đặt lại mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi đặt lại mật khẩu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnToggleMobile_Click(object sender, EventArgs e)
        {
            int rowHandle = gvUsers.FocusedRowHandle;
            if (rowHandle < 0) return;

            int userId = Convert.ToInt32(gvUsers.GetRowCellValue(rowHandle, "IDUSER"));
            string username = Convert.ToString(gvUsers.GetRowCellValue(rowHandle, "USERNAME"));
            string mobStatus = Convert.ToString(gvUsers.GetRowCellValue(rowHandle, "MOBILE_STATUS"));

            if (username.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                XtraMessageBox.Show("Tài khoản Quản trị tối cao (ADMIN) là tài khoản hệ thống, bị cấm tuyệt đối truy cập ứng dụng di động.", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isEnabled = mobStatus.StartsWith("ENABLED");
            int targetState = isEnabled ? 0 : 1;

            try
            {
                using (var db = new MyEntities())
                {
                    int mapCount = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", userId)
                    ).FirstOrDefault();

                    if (mapCount > 0)
                    {
                        db.Database.ExecuteSqlCommand(
                            "UPDATE HR.TB_USER_EMPLOYEE_MAPPING SET IS_MOBILE_ENABLED = :p0, UPDATED_AT = SYSDATE WHERE USER_ID = :p1",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", targetState),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p1", userId)
                        );
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand(
                            "UPDATE HR.TB_SYS_USER SET CLIENT_TYPE = :p0 WHERE IDUSER = :p1",
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p0", targetState == 1 ? "MOBILE" : "DESKTOP"),
                            new Oracle.ManagedDataAccess.Client.OracleParameter("p1", userId)
                        );
                    }

                    string action = targetState == 1 ? "ENABLE_MOBILE" : "DISABLE_MOBILE";
                    WriteAuditDirectOracle(action, userId.ToString(), $"Đổi Mobile Access thành {(targetState == 1 ? "BẬT" : "TẮT")} cho [{username}]", UserSession.CurrentUser?.USERNAME ?? "ADMIN");

                    XtraMessageBox.Show($"Đã {(targetState == 1 ? "kích hoạt" : "tắt")} Mobile Access cho [{username}]!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllData();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi thay đổi Mobile Access: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLienKetNV_Click(object sender, EventArgs e)
        {
            int rowHandle = gvUsers.FocusedRowHandle;
            if (rowHandle < 0) return;

            int userId = Convert.ToInt32(gvUsers.GetRowCellValue(rowHandle, "IDUSER"));
            string username = Convert.ToString(gvUsers.GetRowCellValue(rowHandle, "USERNAME"));

            if (username.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                XtraMessageBox.Show("Tài khoản Quản trị tối cao (ADMIN) là tài khoản hệ thống, không được phép liên kết với hồ sơ nhân sự.", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string manvStr = XtraInputBox.Show("Nhập mã số nội bộ (MANV) của nhân viên cần liên kết với [" + username + "]:", "Liên Kết Hồ Sơ 1:1", "");
            if (string.IsNullOrWhiteSpace(manvStr) || !decimal.TryParse(manvStr.Trim(), out decimal manv)) return;

            try
            {
                using (var db = new MyEntities())
                {
                    var emp = db.Database.SqlQuery<EmpCheckRow>(
                        "SELECT MANV, EMPLOYEE_CODE, HOTEN, DATHOIVIEC FROM HR.TB_NHANVIEN WHERE MANV = :p0 AND DELETED_BY IS NULL",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", manv)
                    ).FirstOrDefault();

                    if (emp == null)
                    {
                        XtraMessageBox.Show($"Không tìm thấy nhân viên với MANV {manv}.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if ((emp.DATHOIVIEC ?? 0) == 1)
                    {
                        XtraMessageBox.Show("Nhân viên này đã thôi việc. Không thể liên kết.", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var existingOtherMap = db.Database.SqlQuery<decimal?>(
                        "SELECT USER_ID FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE EMPLOYEE_ID = :p0 AND USER_ID <> :p1 AND ROWNUM = 1",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", manv),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", userId)
                    ).FirstOrDefault();

                    if (existingOtherMap.HasValue)
                    {
                        XtraMessageBox.Show("Nhân viên này đã được liên kết với một tài khoản khác!", "Ràng buộc 1:1", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string mergeSql = @"
                        MERGE INTO HR.TB_USER_EMPLOYEE_MAPPING M
                        USING (SELECT :p0 AS USER_ID, :p1 AS EMPLOYEE_ID, 1 AS IS_MOBILE_ENABLED FROM DUAL) S
                        ON (M.USER_ID = S.USER_ID)
                        WHEN MATCHED THEN
                            UPDATE SET M.EMPLOYEE_ID = S.EMPLOYEE_ID, M.IS_MOBILE_ENABLED = 1, M.UPDATED_AT = SYSDATE
                        WHEN NOT MATCHED THEN
                            INSERT (USER_ID, EMPLOYEE_ID, IS_MOBILE_ENABLED, CREATED_AT, UPDATED_AT)
                            VALUES (S.USER_ID, S.EMPLOYEE_ID, 1, SYSDATE, SYSDATE)";

                    db.Database.ExecuteSqlCommand(
                        mergeSql,
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", userId),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", manv)
                    );

                    db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_SYS_USER SET MANV = :p0, CLIENT_TYPE = 'MOBILE' WHERE IDUSER = :p1",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", manv),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", userId)
                    );

                    WriteAuditDirectOracle("LINK_EMPLOYEE", userId.ToString(), $"Liên kết tài khoản [{username}] với nhân sự MANV {manv} ({emp.HOTEN})", UserSession.CurrentUser?.USERNAME ?? "ADMIN");

                    XtraMessageBox.Show($"Liên kết tài khoản [{username}] với nhân viên [{emp.HOTEN}] thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllData();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi liên kết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SelectBatchProvisioningTab()
        {
            tabMain.SelectedTabPage = tabCapPhatHangLoat;
            if (tabBulkSub != null)
            {
                tabBulkSub.SelectedTabPage = tabSubCandidates;
            }
            if (_dtCandidates == null)
            {
                LoadBulkPreview();
            }
        }

        private void btnChuyenSangCapPhat_Click(object sender, EventArgs e)
        {
            tabMain.SelectedTabPage = tabCapPhatHangLoat;
            if (tabBulkSub != null)
            {
                tabBulkSub.SelectedTabPage = tabSubCandidates;
            }
        }

        private void btnXuatExcel_Click(object sender, EventArgs e)
        {
            ExportGridToExcel(gvUsers, "DanhSachTaiKhoan");
        }

        private void btnSaveDetail_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtDetailId.Text)) return;
            if (!int.TryParse(txtDetailId.Text, out int userId)) return;

            string fullName = txtDetailFullName.Text.Trim();
            string clientType = (cboDetailClientType.Text ?? "MOBILE").Trim().ToUpperInvariant();
            int disabled = chkDetailDisabled.Checked ? 1 : 0;

            try
            {
                using (var db = new MyEntities())
                {
                    db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_SYS_USER SET FULLNAME = :p0, CLIENT_TYPE = :p1, DISABLED = :p2 WHERE IDUSER = :p3",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", fullName),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", clientType),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p2", disabled),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p3", userId)
                    );

                    WriteAuditDirectOracle("UPDATE_USER", userId.ToString(), $"Cập nhật tài khoản [{txtDetailUsername.Text}] (FullName: {fullName}, Client: {clientType}, Disabled: {disabled})", UserSession.CurrentUser?.USERNAME ?? "ADMIN");

                    XtraMessageBox.Show("Cập nhật thông tin tài khoản thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllData();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi lưu tài khoản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region TAB 3: CẤP PHÁT HÀNG LOẠT (BULK PROVISIONING)
        private void btnBulkRefresh_Click(object sender, EventArgs e)
        {
            if (tabBulkSub != null)
            {
                tabBulkSub.SelectedTabPage = tabSubCandidates;
            }
            LoadBulkPreview();
        }

        private void LoadBulkPreview()
        {
            try
            {
                int deptId = Convert.ToInt32(cboBulkDept.EditValue ?? 0);
                bool onlyWithoutAcc = chkBulkOnlyWithoutAcc.Checked;

                using (var db = new MyEntities())
                {
                    string sql = @"
                        SELECT 
                            NV.MANV AS EmployeeId,
                            NV.EMPLOYEE_CODE AS EmployeeCode,
                            NV.HOTEN AS FullName,
                            NV.IDPB AS DepartmentId,
                            PB.TENPB AS DepartmentName,
                            CV.TENCV AS PositionName,
                            NV.DATHOIVIEC AS DaThoiViec,
                            U.USERNAME AS ExistingAccount,
                            CASE WHEN U.IDUSER IS NOT NULL THEN 1 ELSE 0 END AS HasAccount,
                            CASE 
                                WHEN UPPER(TRIM(U.USERNAME)) = 'ADMIN' THEN 'BLOCKED'
                                WHEN M.IS_MOBILE_ENABLED = 1 THEN 'ENABLED'
                                WHEN M.IS_MOBILE_ENABLED = 0 THEN 'DISABLED'
                                ELSE 'NONE'
                            END AS MobileStatus
                        FROM HR.TB_NHANVIEN NV
                        LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                        LEFT JOIN HR.TB_CHUCVU CV ON NV.IDCV = CV.IDCV
                        LEFT JOIN HR.TB_USER_EMPLOYEE_MAPPING M ON NV.MANV = M.EMPLOYEE_ID
                        LEFT JOIN HR.TB_SYS_USER U ON (M.USER_ID = U.IDUSER OR (M.USER_ID IS NULL AND NV.MANV = U.MANV))
                        WHERE NV.DELETED_BY IS NULL";

                    if (deptId > 0)
                    {
                        sql += $" AND NV.IDPB = {deptId}";
                    }

                    sql += " ORDER BY NV.MANV";

                    var raw = db.Database.SqlQuery<BulkCandidateRaw>(sql).ToList();

                    int eligible = 0;
                    int ready = 0;
                    int existing = 0;
                    int system = 0;
                    int inactive = 0;

                    DataTable dt = new DataTable();
                    dt.Columns.Add("Selected", typeof(bool));
                    dt.Columns.Add("EmployeeId", typeof(decimal));
                    dt.Columns.Add("EmployeeCode", typeof(string));
                    dt.Columns.Add("FullName", typeof(string));
                    dt.Columns.Add("DepartmentName", typeof(string));
                    dt.Columns.Add("PositionName", typeof(string));
                    dt.Columns.Add("ExistingAccount", typeof(string));
                    dt.Columns.Add("MobileStatus", typeof(string));
                    dt.Columns.Add("SuggestedLoginName", typeof(string));
                    dt.Columns.Add("IsEligible", typeof(bool));
                    dt.Columns.Add("Reason", typeof(string));

                    foreach (var item in raw)
                    {
                        bool isInactive = (item.DaThoiViec ?? 0) == 1;
                        bool hasAcc = item.HasAccount == 1;
                        bool isSys = (item.ExistingAccount ?? "").Trim().ToUpper() == "ADMIN";

                        bool isElig = true;
                        string reason = "Đủ điều kiện cấp mới";

                        if (isInactive)
                        {
                            isElig = false;
                            inactive++;
                            reason = "Nhân viên đã thôi việc";
                        }
                        else if (isSys)
                        {
                            isElig = false;
                            system++;
                            reason = "Tài khoản hệ thống (Admin)";
                        }
                        else if (hasAcc)
                        {
                            existing++;
                            if (onlyWithoutAcc)
                            {
                                continue;
                            }
                            isElig = false;
                            reason = "Đã có tài khoản: " + item.ExistingAccount;
                        }

                        if (isElig)
                        {
                            eligible++;
                            ready++;
                        }

                        string suggestedLogin = $"NV{((long)item.EmployeeId):D6}";

                        dt.Rows.Add(
                            isElig,
                            item.EmployeeId,
                            item.EmployeeCode ?? $"NV#{item.EmployeeId}",
                            item.FullName,
                            item.DepartmentName ?? "Chưa phân bổ",
                            item.PositionName ?? "Nhân viên",
                            item.ExistingAccount ?? "(Chưa có)",
                            item.MobileStatus,
                            suggestedLogin,
                            isElig,
                            reason
                        );
                    }

                    lblBulkEligible.Text = $"Tổng hợp lệ: {eligible}";
                    lblBulkReady.Text = $"Sẵn sàng tạo: {ready}";
                    lblBulkExisting.Text = $"Đã có TK: {existing}";
                    lblBulkSystem.Text = $"Admin/System: {system}";
                    lblBulkInactive.Text = $"Đã thôi việc: {inactive}";

                    _dtCandidates = dt;
                    gcCandidates.DataSource = _dtCandidates;
                    gvCandidates.PopulateColumns();
                    ConfigureCandidateGridColumns();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Lỗi xem trước ứng viên cấp phát: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureCandidateGridColumns()
        {
            SetColumn(gvCandidates, "Selected", "Chọn", 50);
            SetColumn(gvCandidates, "EmployeeId", "Mã NV", 65);
            SetColumn(gvCandidates, "EmployeeCode", "Mã Nhân Sự", 100);
            SetColumn(gvCandidates, "FullName", "Họ và Tên Nhân Viên", 150);
            SetColumn(gvCandidates, "DepartmentName", "Phòng Ban", 130);
            SetColumn(gvCandidates, "PositionName", "Chức Vụ", 120);
            SetColumn(gvCandidates, "ExistingAccount", "Tài Khoản Hiện Có", 120);
            SetColumn(gvCandidates, "MobileStatus", "Mobile Status", 100);
            SetColumn(gvCandidates, "SuggestedLoginName", "Username Đề Xuất", 120);
            SetColumn(gvCandidates, "IsEligible", "Hợp Lệ", 65);
            SetColumn(gvCandidates, "Reason", "Ghi Chú Đánh Giá", 180);
            gvCandidates.BestFitColumns();
        }

        private void btnBulkSelectAll_Click(object sender, EventArgs e)
        {
            ToggleSelectAllCandidates(true);
        }

        private void btnBulkDeselectAll_Click(object sender, EventArgs e)
        {
            ToggleSelectAllCandidates(false);
        }

        private void ToggleSelectAllCandidates(bool isSelected)
        {
            if (_dtCandidates == null) return;
            foreach (DataRow r in _dtCandidates.Rows)
            {
                if (Convert.ToBoolean(r["IsEligible"]))
                {
                    r["Selected"] = isSelected;
                }
            }
            gcCandidates.RefreshDataSource();
        }

        private void btnExecuteBulk_Click(object sender, EventArgs e)
        {
            if (_dtCandidates == null) return;

            var selectedRows = _dtCandidates.AsEnumerable().Where(r => Convert.ToBoolean(r["Selected"])).ToList();
            if (selectedRows.Count == 0)
            {
                XtraMessageBox.Show("Vui lòng chọn ít nhất một nhân sự để cấp phát.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = XtraMessageBox.Show(
                $"BẠN SẮP THỰC THI CẤP PHÁT HÀNG LOẠT:\n\n" +
                $"• Số lượng tài khoản sẽ tạo: {selectedRows.Count} tài khoản người dùng (Nhân viên)\n" +
                $"• Mật khẩu khởi tạo: {txtBulkDefaultPass.Text.Trim()}\n" +
                $"• Tự động kích hoạt Mobile: {(chkBulkEnableMobile.Checked ? "CÓ" : "KHÔNG")}\n" +
                $"• Phân loại tài khoản: Tài khoản người dùng (Chỉ đăng nhập Mobile)\n\n" +
                $"Hệ thống sẽ thực thi dưới giao dịch Oracle Transaction đảm bảo an toàn và tính toàn vẹn Idempotency.\nBạn có muốn bắt đầu?",
                "Xác nhận Cấp Phát Hàng Loạt",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            string defaultPass = txtBulkDefaultPass.Text.Trim();
            if (string.IsNullOrEmpty(defaultPass)) defaultPass = "123456";
            string hashedPass = PasswordHasher.HashPassword(defaultPass);
            bool enableMobile = chkBulkEnableMobile.Checked;

            DataTable dtResults = new DataTable();
            dtResults.Columns.Add("EmployeeCode", typeof(string));
            dtResults.Columns.Add("FullName", typeof(string));
            dtResults.Columns.Add("LoginName", typeof(string));
            dtResults.Columns.Add("Result", typeof(string));
            dtResults.Columns.Add("Message", typeof(string));

            int successCnt = 0;
            int alreadyExistsCnt = 0;
            int failedCnt = 0;

            using (var db = new MyEntities())
            {
                using (var trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        var existingUsernames = new HashSet<string>(
                            db.Database.SqlQuery<string>("SELECT USERNAME FROM HR.TB_SYS_USER WHERE USERNAME IS NOT NULL").ToList()
                                .Select(u => (u ?? "").Trim().ToUpper()),
                            StringComparer.OrdinalIgnoreCase
                        );

                        foreach (var row in selectedRows)
                        {
                            decimal empId = Convert.ToDecimal(row["EmployeeId"]);
                            string empCode = Convert.ToString(row["EmployeeCode"]);
                            string empName = Convert.ToString(row["FullName"]);

                            try
                            {
                                // 1. Idempotency Check
                                var existingMap = db.Database.SqlQuery<decimal?>(
                                    "SELECT USER_ID FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE EMPLOYEE_ID = :p0 AND ROWNUM = 1",
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p0", empId)
                                ).FirstOrDefault();

                                if (existingMap.HasValue)
                                {
                                    alreadyExistsCnt++;
                                    dtResults.Rows.Add(empCode, empName, $"NV{((long)empId):D6}", "ALREADY_EXISTS", "Nhân sự đã có tài khoản liên kết hợp lệ");
                                    continue;
                                }

                                // 2. Generate Unique LoginName
                                string baseLogin = $"NV{((long)empId):D6}";
                                string loginName = baseLogin;
                                int retry = 1;
                                while (existingUsernames.Contains(loginName.ToUpper()))
                                {
                                    loginName = $"NV{((long)empId):D6}_{retry}";
                                    retry++;
                                }
                                existingUsernames.Add(loginName.ToUpper());

                                // 3. Create TB_SYS_USER via EF
                                var newUser = new TB_SYS_USER
                                {
                                    USERNAME = loginName,
                                    FULLNAME = empName,
                                    PASSWORD = hashedPass,
                                    MANV = empId,
                                    DISABLED = 0,
                                    CLIENT_TYPE = enableMobile ? "ALL" : "DESKTOP",
                                    ISGROUP = 0,
                                    MACTY = "1",
                                    MADVI = "1"
                                };

                                db.TB_SYS_USER.Add(newUser);
                                db.SaveChanges();

                                // 4. MERGE into TB_USER_EMPLOYEE_MAPPING (1-1)
                                int mobileFlag = enableMobile ? 1 : 0;
                                string mergeSql = @"
                                    MERGE INTO HR.TB_USER_EMPLOYEE_MAPPING M
                                    USING (SELECT :p0 AS USER_ID, :p1 AS EMPLOYEE_ID, :p2 AS IS_MOBILE_ENABLED FROM DUAL) S
                                    ON (M.USER_ID = S.USER_ID)
                                    WHEN MATCHED THEN
                                        UPDATE SET M.EMPLOYEE_ID = S.EMPLOYEE_ID, M.IS_MOBILE_ENABLED = S.IS_MOBILE_ENABLED, M.UPDATED_AT = SYSDATE
                                    WHEN NOT MATCHED THEN
                                        INSERT (USER_ID, EMPLOYEE_ID, IS_MOBILE_ENABLED, CREATED_AT, UPDATED_AT)
                                        VALUES (S.USER_ID, S.EMPLOYEE_ID, S.IS_MOBILE_ENABLED, SYSDATE, SYSDATE)";

                                db.Database.ExecuteSqlCommand(
                                    mergeSql,
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p0", newUser.IDUSER),
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p1", empId),
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p2", mobileFlag)
                                );

                                successCnt++;
                                dtResults.Rows.Add(empCode, empName, loginName, "SUCCESS", enableMobile ? "Đã tạo tài khoản & kích hoạt Mobile thành công" : "Đã tạo tài khoản thành công");
                            }
                            catch (Exception exRow)
                            {
                                failedCnt++;
                                string errMsg = exRow.InnerException?.InnerException?.Message ?? exRow.InnerException?.Message ?? exRow.Message;
                                dtResults.Rows.Add(empCode, empName, $"NV{((long)empId):D6}", "FAILED", errMsg);
                            }
                        }

                        trans.Commit();

                        WriteAuditDirectOracle("BULK_PROVISION", $"{successCnt}_ACCOUNTS", $"Cấp phát hàng loạt {successCnt} tài khoản nhân viên (Thành công: {successCnt}, Đã có: {alreadyExistsCnt}, Lỗi: {failedCnt})", UserSession.CurrentUser?.USERNAME ?? "ADMIN");

                        XtraMessageBox.Show(
                            $"CẤP PHÁT HÀNG LOẠT HOÀN TẤT:\n\n" +
                            $"• Tạo mới thành công: {successCnt} tài khoản (Chỉ dùng cho Mobile)\n" +
                            $"• Đã có sẵn bỏ qua: {alreadyExistsCnt} nhân sự\n" +
                            $"• Lỗi: {failedCnt} nhân sự",
                            "Kết Quả Cấp Phát",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        _dtBulkResults = dtResults;
                        gcBulkResults.DataSource = _dtBulkResults;
                        gvBulkResults.PopulateColumns();
                        ConfigureBulkResultGridColumns();

                        if (tabBulkSub != null)
                        {
                            tabBulkSub.SelectedTabPage = tabSubResults;
                        }

                        LoadAllData();
                        LoadBulkPreview();
                    }
                    catch (Exception exTrans)
                    {
                        trans.Rollback();
                        XtraMessageBox.Show("Lỗi giao dịch cấp phát: " + exTrans.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ConfigureBulkResultGridColumns()
        {
            SetColumn(gvBulkResults, "EmployeeCode", "Mã Nhân Sự", 100);
            SetColumn(gvBulkResults, "FullName", "Họ và Tên", 150);
            SetColumn(gvBulkResults, "LoginName", "Tên Đăng Nhập", 120);
            SetColumn(gvBulkResults, "Result", "Kết Quả", 100);
            SetColumn(gvBulkResults, "Message", "Thông Điệp Chi Tiết", 250);
            gvBulkResults.BestFitColumns();
        }
        #endregion

        #region UTILITIES
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

        private void WriteAuditDirectOracle(string action, string recordId, string details, string actor)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    string sql = @"
                        INSERT INTO HR.TB_SYS_LOG (
                            MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI, DU_LIEU_MOI, IP_ADDRESS, TEN_MAY_TINH, THOIGIAN, MODULE_NAME, CHANGED_FIELDS
                        ) VALUES (
                            :p0, :p1, :p2, 'TB_SYS_USER', :p3, :p4, '127.0.0.1', 'DESKTOP_WINFORMS', SYSDATE, 'SYSTEM', :p5
                        )";

                    db.Database.ExecuteSqlCommand(
                        sql,
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", UserSession.CurrentUser?.MANV?.ToString() ?? "1"),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", actor ?? "ADMIN"),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p2", action),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p3", recordId ?? ""),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p4", details ?? ""),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p5", action)
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi ghi nhật ký audit: " + ex.Message);
            }
        }

        private void ExportGridToExcel(GridView gv, string defaultFileName)
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
        private class DeptItem
        {
            public int IDPB { get; set; }
            public string TENPB { get; set; }
        }

        private class UserDisplayRow
        {
            public decimal IDUSER { get; set; }
            public string USERNAME { get; set; }
            public string FULLNAME { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public string EMPLOYEE_NAME { get; set; }
            public string TENPB { get; set; }
            public string TENCV { get; set; }
            public string ACCOUNT_STATUS { get; set; }
            public string ACCOUNT_TYPE { get; set; }
            public string MOBILE_STATUS { get; set; }
            public string CLIENT_TYPE { get; set; }
            public decimal? EMPLOYEE_ID { get; set; }
        }

        private class BulkCandidateRaw
        {
            public decimal EmployeeId { get; set; }
            public string EmployeeCode { get; set; }
            public string FullName { get; set; }
            public int? DepartmentId { get; set; }
            public string DepartmentName { get; set; }
            public string PositionName { get; set; }
            public decimal? DaThoiViec { get; set; }
            public string ExistingAccount { get; set; }
            public int HasAccount { get; set; }
            public string MobileStatus { get; set; }
        }

        private class EmpDetailRow
        {
            public decimal MANV { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public string HOTEN { get; set; }
            public string TENPB { get; set; }
            public string TENCV { get; set; }
        }

        private class MappingCheckRow
        {
            public decimal? EMPLOYEE_ID { get; set; }
            public decimal? IS_MOBILE_ENABLED { get; set; }
        }

        private class EmpCheckRow
        {
            public decimal MANV { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public string HOTEN { get; set; }
            public decimal? DATHOIVIEC { get; set; }
        }

        #endregion

        private void txtBulkDefaultPass_EditValueChanged(object sender, EventArgs e)
        {

        }
    }
}
