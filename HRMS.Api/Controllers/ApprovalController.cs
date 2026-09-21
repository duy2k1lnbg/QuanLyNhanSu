using DA;
using HRMS_API.Filters;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    /// <summary>
    /// Approval Center (Trung tâm Phê duyệt tập trung dành cho HR/Manager trên Web) - Quy tắc 35
    /// Tích hợp và điều phối phê duyệt: Nghỉ phép, Điều chỉnh công, Tăng ca.
    /// </summary>
    [JwtAuthorize]
    [RoutePrefix("api/approvals")]
    public class ApprovalController : ApiController
    {
        private decimal GetCurrentUserId(MyEntities db)
        {
            try
            {
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                if (jwtUser != null && int.TryParse(jwtUser.UserId, out int uId))
                {
                    return (decimal)uId;
                }
                string uname = jwtUser?.Username ?? "admin";
                var user = db.TB_SYS_USER.FirstOrDefault(u => u.USERNAME.ToLower() == uname.ToLower());
                return user?.IDUSER ?? 1;
            }
            catch
            {
                return 1;
            }
        }

        private string GetCurrentUsername()
        {
            var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            return jwtUser?.Username ?? "Admin";
        }

        /// <summary>
        /// GET: api/approvals/summary
        /// Lấy số lượng yêu cầu đang chờ duyệt theo từng loại để hiển thị Badge và Dashboard
        /// </summary>
        [HttpGet]
        [Route("summary")]
        public IHttpActionResult GetSummary()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    int leaveCount = (int)db.Database.SqlQuery<decimal>(
                        "SELECT COUNT(*) FROM HR.TB_YEUCAU_NGHIPHEP WHERE TRANGTHAI = 'PENDING'"
                    ).FirstOrDefault();

                    int attendanceCount = (int)db.Database.SqlQuery<decimal>(
                        "SELECT COUNT(*) FROM HR.TB_YEUCAU_DIEUCHINHCONG WHERE TRANGTHAI = 'PENDING'"
                    ).FirstOrDefault();

                    int overtimeCount = (int)db.Database.SqlQuery<decimal>(
                        "SELECT COUNT(*) FROM HR.TB_YEUCAU_TANGCA WHERE TRANGTHAI = 'PENDING'"
                    ).FirstOrDefault();

                    return Ok(new
                    {
                        success = true,
                        totalPending = leaveCount + attendanceCount + overtimeCount,
                        leavePending = leaveCount,
                        attendancePending = attendanceCount,
                        overtimePending = overtimeCount
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/approvals/summary Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải dữ liệu tóm tắt phê duyệt." });
            }
        }

        #region Nghỉ phép (Leave Approvals)

        /// <summary>
        /// GET: api/approvals/leave?status=PENDING
        /// </summary>
        [HttpGet]
        [Route("leave")]
        public IHttpActionResult GetLeaveList(string status = null, string search = null, int page = 1, int pageSize = 20)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    string sql = @"
                        SELECT Y.ID AS ID_YEUCAU, Y.MANV, NV.EMPLOYEE_CODE, NV.HOTEN AS EMPLOYEE_NAME, PB.TENPB AS DEPARTMENT_NAME,
                               Y.LOAIPHEP AS LOAI_NGHI, Y.TUNGAY AS TU_NGAY, Y.DENNGAY AS DEN_NGAY, Y.SONGAY AS SO_NGAY, Y.LYDO, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO,
                               NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI
                        FROM HR.TB_YEUCAU_NGHIPHEP Y
                        LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                        LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                        LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                        WHERE (1=1)";

                    var parameters = new List<OracleParameter>();

                    if (!string.IsNullOrWhiteSpace(status) && status.ToUpper() != "ALL")
                    {
                        sql += " AND Y.TRANGTHAI = :pStatus";
                        parameters.Add(new OracleParameter("pStatus", status.ToUpper()));
                    }

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        sql += " AND (LOWER(NV.HOTEN) LIKE :pSearch OR LOWER(NV.EMPLOYEE_CODE) LIKE :pSearch)";
                        parameters.Add(new OracleParameter("pSearch", $"%{search.Trim().ToLower()}%"));
                    }

                    sql += " ORDER BY Y.CREATED_DATE DESC";

                    var allItems = db.Database.SqlQuery<LeaveApprovalRow>(sql, parameters.ToArray()).ToList();
                    int total = allItems.Count;
                    var paged = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    return Ok(new
                    {
                        success = true,
                        total = total,
                        page = page,
                        pageSize = pageSize,
                        data = paged.Select(r => new
                        {
                            id = r.ID_YEUCAU,
                            idYeuCau = r.ID_YEUCAU,
                            manv = r.MANV,
                            employeeCode = r.EMPLOYEE_CODE,
                            employeeName = r.EMPLOYEE_NAME,
                            departmentName = r.DEPARTMENT_NAME ?? "Chưa phân bổ",
                            loaiNghi = r.LOAI_NGHI,
                            tuNgay = r.TU_NGAY.HasValue ? r.TU_NGAY.Value.ToString("dd/MM/yyyy") : "",
                            denNgay = r.DEN_NGAY.HasValue ? r.DEN_NGAY.Value.ToString("dd/MM/yyyy") : "",
                            soNgay = r.SO_NGAY ?? 1,
                            lyDo = r.LYDO,
                            trangThai = r.TRANGTHAI,
                            ngayTao = r.NGAY_TAO.HasValue ? r.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                            nguoiDuyet = r.NGUOI_DUYET,
                            ngayDuyet = r.NGAY_DUYET.HasValue ? r.NGAY_DUYET.Value.ToString("dd/MM/yyyy HH:mm") : "",
                            lyDoTuChoi = r.LYDO_TUCHOI
                        })
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/approvals/leave Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải danh sách đơn xin nghỉ phép." });
            }
        }

        /// <summary>
        /// POST: api/approvals/leave/{id}/approve
        /// </summary>
        [HttpPost]
        [Route("leave/{id:decimal}/approve")]
        public IHttpActionResult ApproveLeave(decimal id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    decimal userId = GetCurrentUserId(db);
                    int affected = db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_YEUCAU_NGHIPHEP SET TRANGTHAI = 'APPROVED', NGUOIDUYET = :p0, NGAYDUYET = SYSDATE WHERE ID = :p1 AND TRANGTHAI = 'PENDING'",
                        new OracleParameter("p0", userId),
                        new OracleParameter("p1", id)
                    );

                    if (affected == 0)
                    {
                        return BadRequest("Yêu cầu không tồn tại hoặc đã được xử lý trước đó.");
                    }

                    return Ok(new { success = true, message = "Phê duyệt đơn xin nghỉ phép thành công!" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ApproveLeave Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi phê duyệt." });
            }
        }

        /// <summary>
        /// POST: api/approvals/leave/{id}/reject
        /// </summary>
        [HttpPost]
        [Route("leave/{id:decimal}/reject")]
        public IHttpActionResult RejectLeave(decimal id, [FromBody] RejectActionRequest req)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    decimal userId = GetCurrentUserId(db);
                    string reason = req?.Reason ?? "Từ chối bởi cấp quản lý";

                    int affected = db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_YEUCAU_NGHIPHEP SET TRANGTHAI = 'REJECTED', NGUOIDUYET = :p0, NGAYDUYET = SYSDATE, GHICHUDUYET = :p1 WHERE ID = :p2 AND TRANGTHAI = 'PENDING'",
                        new OracleParameter("p0", userId),
                        new OracleParameter("p1", reason),
                        new OracleParameter("p2", id)
                    );

                    if (affected == 0)
                    {
                        return BadRequest("Yêu cầu không tồn tại hoặc đã được xử lý trước đó.");
                    }

                    return Ok(new { success = true, message = "Đã từ chối đơn xin nghỉ phép." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[RejectLeave Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi từ chối." });
            }
        }

        #endregion

        #region Điều chỉnh công (Attendance Correction Approvals)

        /// <summary>
        /// GET: api/approvals/attendance-corrections
        /// </summary>
        [HttpGet]
        [Route("attendance-corrections")]
        public IHttpActionResult GetAttendanceCorrections(string status = null, string search = null, int page = 1, int pageSize = 20)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    string sql = @"
                        SELECT Y.ID AS ID_YEUCAU, Y.MANV, NV.EMPLOYEE_CODE, NV.HOTEN AS EMPLOYEE_NAME, PB.TENPB AS DEPARTMENT_NAME,
                               Y.NGAY AS NGAY_CONG, Y.GIO_VAO AS GIO_VAO_MOI, Y.GIO_RA AS GIO_RA_MOI, Y.LYDO, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO,
                               NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI
                        FROM HR.TB_YEUCAU_DIEUCHINHCONG Y
                        LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                        LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                        LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                        WHERE (1=1)";

                    var parameters = new List<OracleParameter>();

                    if (!string.IsNullOrWhiteSpace(status) && status.ToUpper() != "ALL")
                    {
                        sql += " AND Y.TRANGTHAI = :pStatus";
                        parameters.Add(new OracleParameter("pStatus", status.ToUpper()));
                    }

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        sql += " AND (LOWER(NV.HOTEN) LIKE :pSearch OR LOWER(NV.EMPLOYEE_CODE) LIKE :pSearch)";
                        parameters.Add(new OracleParameter("pSearch", $"%{search.Trim().ToLower()}%"));
                    }

                    sql += " ORDER BY Y.CREATED_DATE DESC";

                    var allItems = db.Database.SqlQuery<AttendanceApprovalRow>(sql, parameters.ToArray()).ToList();
                    int total = allItems.Count;
                    var paged = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    return Ok(new
                    {
                        success = true,
                        total = total,
                        page = page,
                        pageSize = pageSize,
                        data = paged.Select(r => new
                        {
                            id = r.ID_YEUCAU,
                            idYeuCau = r.ID_YEUCAU,
                            manv = r.MANV,
                            employeeCode = r.EMPLOYEE_CODE,
                            employeeName = r.EMPLOYEE_NAME,
                            departmentName = r.DEPARTMENT_NAME ?? "Chưa phân bổ",
                            ngayCong = r.NGAY_CONG.HasValue ? r.NGAY_CONG.Value.ToString("dd/MM/yyyy") : "",
                            gioVaoMoi = r.GIO_VAO_MOI,
                            gioRaMoi = r.GIO_RA_MOI,
                            lyDo = r.LYDO,
                            trangThai = r.TRANGTHAI,
                            ngayTao = r.NGAY_TAO.HasValue ? r.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                            nguoiDuyet = r.NGUOI_DUYET,
                            ngayDuyet = r.NGAY_DUYET.HasValue ? r.NGAY_DUYET.Value.ToString("dd/MM/yyyy HH:mm") : "",
                            lyDoTuChoi = r.LYDO_TUCHOI
                        })
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/approvals/attendance-corrections Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải danh sách điều chỉnh công." });
            }
        }

        /// <summary>
        /// POST: api/approvals/attendance-corrections/{id}/approve
        /// Khi duyệt: Cập nhật yêu cầu thành APPROVED và tự động đồng bộ sang bảng chấm công chi tiết TB_BANGCONG_CHITIET
        /// </summary>
        [HttpPost]
        [Route("attendance-corrections/{id:decimal}/approve")]
        public IHttpActionResult ApproveAttendanceCorrection(decimal id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    decimal userId = GetCurrentUserId(db);

                    var reqItem = db.Database.SqlQuery<AttendanceApprovalRow>(
                        "SELECT ID AS ID_YEUCAU, MANV, NGAY AS NGAY_CONG, GIO_VAO AS GIO_VAO_MOI, GIO_RA AS GIO_RA_MOI, TRANGTHAI FROM HR.TB_YEUCAU_DIEUCHINHCONG WHERE ID = :p0",
                        new OracleParameter("p0", id)
                    ).FirstOrDefault();

                    if (reqItem == null || reqItem.TRANGTHAI != "PENDING")
                    {
                        return BadRequest("Yêu cầu không tồn tại hoặc đã được xử lý trước đó.");
                    }

                    // 1. Cập nhật trạng thái yêu cầu
                    db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_YEUCAU_DIEUCHINHCONG SET TRANGTHAI = 'APPROVED', NGUOIDUYET = :p0, NGAYDUYET = SYSDATE WHERE ID = :p1",
                        new OracleParameter("p0", userId),
                        new OracleParameter("p1", id)
                    );

                    // 2. Tự động đồng bộ giờ vào/ra và ngày công vào TB_BANGCONG_CHITIET
                    if (reqItem.NGAY_CONG.HasValue)
                    {
                        try
                        {
                            db.Database.ExecuteSqlCommand(
                                "UPDATE HR.TB_BANGCONG_CHITIET SET GIOVAO = :p0, GIORA = :p1, NGAYCONG = 1, KYHIEU = 'X' " +
                                "WHERE MANV = :p2 AND TRUNC(NGAY) = TRUNC(:p3)",
                                new OracleParameter("p0", reqItem.GIO_VAO_MOI ?? "08:00"),
                                new OracleParameter("p1", reqItem.GIO_RA_MOI ?? "17:00"),
                                new OracleParameter("p2", reqItem.MANV),
                                new OracleParameter("p3", reqItem.NGAY_CONG.Value)
                            );
                        }
                        catch (Exception exSync)
                        {
                            System.Diagnostics.Trace.TraceWarning("Không thể đồng bộ ngay sang TB_BANGCONG_CHITIET: " + exSync.Message);
                        }
                    }

                    return Ok(new { success = true, message = "Phê duyệt điều chỉnh công thành công và đã đồng bộ dữ liệu chấm công!" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ApproveAttendanceCorrection Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi phê duyệt điều chỉnh công." });
            }
        }

        /// <summary>
        /// POST: api/approvals/attendance-corrections/{id}/reject
        /// </summary>
        [HttpPost]
        [Route("attendance-corrections/{id:decimal}/reject")]
        public IHttpActionResult RejectAttendanceCorrection(decimal id, [FromBody] RejectActionRequest req)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    decimal userId = GetCurrentUserId(db);
                    string reason = req?.Reason ?? "Từ chối bởi cấp quản lý";

                    int affected = db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_YEUCAU_DIEUCHINHCONG SET TRANGTHAI = 'REJECTED', NGUOIDUYET = :p0, NGAYDUYET = SYSDATE, GHICHUDUYET = :p1 WHERE ID = :p2 AND TRANGTHAI = 'PENDING'",
                        new OracleParameter("p0", userId),
                        new OracleParameter("p1", reason),
                        new OracleParameter("p2", id)
                    );

                    if (affected == 0)
                    {
                        return BadRequest("Yêu cầu không tồn tại hoặc đã được xử lý trước đó.");
                    }

                    return Ok(new { success = true, message = "Đã từ chối yêu cầu điều chỉnh chấm công." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[RejectAttendanceCorrection Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi từ chối." });
            }
        }

        #endregion

        #region Tăng ca (Overtime Approvals)

        /// <summary>
        /// GET: api/approvals/overtime
        /// </summary>
        [HttpGet]
        [Route("overtime")]
        public IHttpActionResult GetOvertimeList(string status = null, string search = null, int page = 1, int pageSize = 20)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    string sql = @"
                        SELECT Y.ID AS ID_YEUCAU, Y.MANV, NV.EMPLOYEE_CODE, NV.HOTEN AS EMPLOYEE_NAME, PB.TENPB AS DEPARTMENT_NAME,
                               Y.NGAY AS NGAY_TANGCA, Y.GIOTANGCA AS SO_GIO, NVL(L.HESO, 1.0) AS HE_SO, Y.IDCA, NVL(L.TENLOAICA, 'Ca ngày') AS TEN_CA,
                               Y.LYDO AS NOI_DUNG, Y.TRANGTHAI, Y.CREATED_DATE AS NGAY_TAO,
                               NVL(U.FULLNAME, U.USERNAME) AS NGUOI_DUYET, Y.NGAYDUYET AS NGAY_DUYET, Y.GHICHUDUYET AS LYDO_TUCHOI
                        FROM HR.TB_YEUCAU_TANGCA Y
                        LEFT JOIN HR.TB_NHANVIEN NV ON Y.MANV = NV.MANV
                        LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                        LEFT JOIN HR.TB_SYS_USER U ON Y.NGUOIDUYET = U.IDUSER
                        LEFT JOIN HR.TB_LOAICA L ON Y.IDCA = L.IDLOAICA
                        WHERE (1=1)";

                    var parameters = new List<OracleParameter>();

                    if (!string.IsNullOrWhiteSpace(status) && status.ToUpper() != "ALL")
                    {
                        sql += " AND Y.TRANGTHAI = :pStatus";
                        parameters.Add(new OracleParameter("pStatus", status.ToUpper()));
                    }

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        sql += " AND (LOWER(NV.HOTEN) LIKE :pSearch OR LOWER(NV.EMPLOYEE_CODE) LIKE :pSearch)";
                        parameters.Add(new OracleParameter("pSearch", $"%{search.Trim().ToLower()}%"));
                    }

                    sql += " ORDER BY Y.CREATED_DATE DESC";

                    var allItems = db.Database.SqlQuery<OvertimeApprovalRow>(sql, parameters.ToArray()).ToList();
                    int total = allItems.Count;
                    var paged = allItems.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    return Ok(new
                    {
                        success = true,
                        total = total,
                        page = page,
                        pageSize = pageSize,
                        data = paged.Select(r => new
                        {
                            id = r.ID_YEUCAU,
                            idYeuCau = r.ID_YEUCAU,
                            manv = r.MANV,
                            employeeCode = r.EMPLOYEE_CODE,
                            employeeName = r.EMPLOYEE_NAME,
                            departmentName = r.DEPARTMENT_NAME ?? "Chưa phân bổ",
                            ngayTangCa = r.NGAY_TANGCA.HasValue ? r.NGAY_TANGCA.Value.ToString("dd/MM/yyyy") : "",
                            soGio = r.SO_GIO ?? 0,
                            heSo = r.HE_SO ?? 1.0m,
                            idCa = r.IDCA ?? 1,
                            tenCa = r.TEN_CA ?? "Ca ngày",
                            noiDung = r.NOI_DUNG,
                            trangThai = r.TRANGTHAI,
                            ngayTao = r.NGAY_TAO.HasValue ? r.NGAY_TAO.Value.ToString("dd/MM/yyyy HH:mm") : "",
                            nguoiDuyet = r.NGUOI_DUYET,
                            ngayDuyet = r.NGAY_DUYET.HasValue ? r.NGAY_DUYET.Value.ToString("dd/MM/yyyy HH:mm") : "",
                            lyDoTuChoi = r.LYDO_TUCHOI
                        })
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/approvals/overtime Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải danh sách đăng ký tăng ca." });
            }
        }

        /// <summary>
        /// POST: api/approvals/overtime/{id}/approve
        /// Khi duyệt: Cập nhật thành APPROVED và ghi nhận liên kết vào TB_TANGCA với OT_REQUEST_ID
        /// Security: Ngăn chặn tự phê duyệt (Self-Approval) và không cho duyệt request đã REJECTED / CANCELLED.
        /// </summary>
        [HttpPost]
        [Route("overtime/{id:decimal}/approve")]
        public IHttpActionResult ApproveOvertime(decimal id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    decimal userId = GetCurrentUserId(db);

                    var reqItem = db.Database.SqlQuery<OvertimeApprovalRow>(
                        @"SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.NGAY AS NGAY_TANGCA, Y.GIOTANGCA AS SO_GIO, 
                                 NVL(L.HESO, 1.0) AS HE_SO, Y.IDCA, NVL(L.TENLOAICA, 'Ca ngày') AS TEN_CA, 
                                 Y.LYDO AS NOI_DUNG, Y.TRANGTHAI 
                          FROM HR.TB_YEUCAU_TANGCA Y 
                          LEFT JOIN HR.TB_LOAICA L ON Y.IDCA = L.IDLOAICA 
                          WHERE Y.ID = :p0",
                        new OracleParameter("p0", id)
                    ).FirstOrDefault();

                    if (reqItem == null)
                    {
                        return NotFound();
                    }

                    if (reqItem.TRANGTHAI != "PENDING")
                    {
                        return BadRequest($"Không thể phê duyệt đề xuất ở trạng thái '{reqItem.TRANGTHAI}'. Chỉ có thể phê duyệt đề xuất đang Chờ duyệt (PENDING).");
                    }

                    // Security: Kiểm tra tự phê duyệt (Employee không thể tự duyệt request của chính mình)
                    var approverEmpId = db.Database.SqlQuery<decimal?>(
                        "SELECT EMPLOYEE_ID FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                        new OracleParameter("p0", userId)
                    ).FirstOrDefault();

                    if (approverEmpId.HasValue && approverEmpId.Value == reqItem.MANV)
                    {
                        return BadRequest("Người dùng không thể tự phê duyệt đề xuất tăng ca của chính mình.");
                    }

                    // 1. Cập nhật trạng thái sang APPROVED (atomic check PENDING)
                    int updated = db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_YEUCAU_TANGCA SET TRANGTHAI = 'APPROVED', NGUOIDUYET = :p0, NGAYDUYET = SYSDATE WHERE ID = :p1 AND TRANGTHAI = 'PENDING'",
                        new OracleParameter("p0", userId),
                        new OracleParameter("p1", id)
                    );

                    if (updated == 0)
                    {
                        return BadRequest("Yêu cầu không còn ở trạng thái chờ duyệt hoặc đã được xử lý bởi người khác.");
                    }

                    // 2. Ghi nhận thời gian tăng ca vào TB_TANGCA kèm liên kết OT_REQUEST_ID
                    if (reqItem.NGAY_TANGCA.HasValue && reqItem.SO_GIO.HasValue && reqItem.SO_GIO.Value > 0)
                    {
                        try
                        {
                            db.Database.ExecuteSqlCommand(@"
                                INSERT INTO HR.TB_TANGCA 
                                (MANV, NGAY, THANG, NAM, SOGIO, HESOTC, GHICHU, IDLOAICA, OT_REQUEST_ID, CREATED_DATE, CREATED_BY)
                                VALUES (:p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, SYSDATE, :p9)",
                                new OracleParameter("p0", reqItem.MANV),
                                new OracleParameter("p1", (decimal)reqItem.NGAY_TANGCA.Value.Day),
                                new OracleParameter("p2", (decimal)reqItem.NGAY_TANGCA.Value.Month),
                                new OracleParameter("p3", (decimal)reqItem.NGAY_TANGCA.Value.Year),
                                new OracleParameter("p4", reqItem.SO_GIO.Value),
                                new OracleParameter("p5", reqItem.HE_SO ?? 1.0m),
                                new OracleParameter("p6", reqItem.NOI_DUNG ?? "Duyệt từ yêu cầu tăng ca"),
                                new OracleParameter("p7", reqItem.IDCA ?? 1),
                                new OracleParameter("p8", reqItem.ID_YEUCAU),
                                new OracleParameter("p9", userId)
                            );
                        }
                        catch (Exception exTc)
                        {
                            System.Diagnostics.Trace.TraceWarning("Không thể tự động thêm vào TB_TANGCA: " + exTc.Message);
                        }
                    }

                    return Ok(new { success = true, message = "Phê duyệt đề xuất tăng ca thành công và đã đồng bộ vào bảng tăng ca thực tế!" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ApproveOvertime Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi phê duyệt tăng ca." });
            }
        }

        /// <summary>
        /// POST: api/approvals/overtime/{id}/reject
        /// Từ chối đề xuất tăng ca (Yêu cầu phải có lý do từ chối, không được tự từ chối của mình, không từ chối request đã APPROVED)
        /// </summary>
        [HttpPost]
        [Route("overtime/{id:decimal}/reject")]
        public IHttpActionResult RejectOvertime(decimal id, [FromBody] RejectActionRequest req)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    decimal userId = GetCurrentUserId(db);

                    var reqItem = db.Database.SqlQuery<OvertimeApprovalRow>(
                        "SELECT ID AS ID_YEUCAU, MANV, TRANGTHAI FROM HR.TB_YEUCAU_TANGCA WHERE ID = :p0",
                        new OracleParameter("p0", id)
                    ).FirstOrDefault();

                    if (reqItem == null)
                    {
                        return NotFound();
                    }

                    if (reqItem.TRANGTHAI != "PENDING")
                    {
                        return BadRequest($"Không thể từ chối đề xuất ở trạng thái '{reqItem.TRANGTHAI}'. Chỉ có thể từ chối đề xuất đang Chờ duyệt (PENDING).");
                    }

                    // Security: Kiểm tra tự xử lý (Employee không thể tự từ chối request của chính mình)
                    var approverEmpId = db.Database.SqlQuery<decimal?>(
                        "SELECT EMPLOYEE_ID FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                        new OracleParameter("p0", userId)
                    ).FirstOrDefault();

                    if (approverEmpId.HasValue && approverEmpId.Value == reqItem.MANV)
                    {
                        return BadRequest("Người dùng không thể tự từ chối đề xuất tăng ca của chính mình.");
                    }

                    if (string.IsNullOrWhiteSpace(req?.Reason))
                    {
                        return BadRequest("Vui lòng cung cấp lý do từ chối đề xuất tăng ca.");
                    }

                    int affected = db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_YEUCAU_TANGCA SET TRANGTHAI = 'REJECTED', NGUOIDUYET = :p0, NGAYDUYET = SYSDATE, GHICHUDUYET = :p1 WHERE ID = :p2 AND TRANGTHAI = 'PENDING'",
                        new OracleParameter("p0", userId),
                        new OracleParameter("p1", req.Reason.Trim()),
                        new OracleParameter("p2", id)
                    );

                    if (affected == 0)
                    {
                        return BadRequest("Yêu cầu không tồn tại hoặc đã được xử lý trước đó.");
                    }

                    return Ok(new { success = true, message = "Đã từ chối đề xuất đăng ký tăng ca." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[RejectOvertime Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi từ chối." });
            }
        }

        #endregion
    }

    public class RejectActionRequest
    {
        public string Reason { get; set; }
    }

    public class LeaveApprovalRow
    {
        public decimal ID_YEUCAU { get; set; }
        public decimal MANV { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public string EMPLOYEE_NAME { get; set; }
        public string DEPARTMENT_NAME { get; set; }
        public string LOAI_NGHI { get; set; }
        public DateTime? TU_NGAY { get; set; }
        public DateTime? DEN_NGAY { get; set; }
        public decimal? SO_NGAY { get; set; }
        public string LYDO { get; set; }
        public string TRANGTHAI { get; set; }
        public DateTime? NGAY_TAO { get; set; }
        public string NGUOI_DUYET { get; set; }
        public DateTime? NGAY_DUYET { get; set; }
        public string LYDO_TUCHOI { get; set; }
    }

    public class AttendanceApprovalRow
    {
        public decimal ID_YEUCAU { get; set; }
        public decimal MANV { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public string EMPLOYEE_NAME { get; set; }
        public string DEPARTMENT_NAME { get; set; }
        public DateTime? NGAY_CONG { get; set; }
        public string GIO_VAO_MOI { get; set; }
        public string GIO_RA_MOI { get; set; }
        public string LYDO { get; set; }
        public string TRANGTHAI { get; set; }
        public DateTime? NGAY_TAO { get; set; }
        public string NGUOI_DUYET { get; set; }
        public DateTime? NGAY_DUYET { get; set; }
        public string LYDO_TUCHOI { get; set; }
    }

    public class OvertimeApprovalRow
    {
        public decimal ID_YEUCAU { get; set; }
        public decimal MANV { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public string EMPLOYEE_NAME { get; set; }
        public string DEPARTMENT_NAME { get; set; }
        public DateTime? NGAY_TANGCA { get; set; }
        public decimal? SO_GIO { get; set; }
        public decimal? HE_SO { get; set; }
        public decimal? IDCA { get; set; }
        public string TEN_CA { get; set; }
        public string NOI_DUNG { get; set; }
        public string TRANGTHAI { get; set; }
        public DateTime? NGAY_TAO { get; set; }
        public string NGUOI_DUYET { get; set; }
        public DateTime? NGAY_DUYET { get; set; }
        public string LYDO_TUCHOI { get; set; }
    }
}
