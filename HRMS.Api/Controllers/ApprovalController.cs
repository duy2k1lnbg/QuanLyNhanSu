using DA;
using HRMS_API.Filters;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    /// <summary>
    /// Approval Center (Trung tâm Phê duyệt tập trung dành cho HR/Manager trên Web) - Quy tắc 35
    /// Tích hợp và điều phối phê duyệt: Nghỉ phép, Điều chỉnh công, Tăng ca.
    /// </summary>
    [JwtAuthorize(Channel = "WEB")]
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

        private decimal? GetCallerEmployeeId(MyEntities db)
        {
            try
            {
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                if (jwtUser != null && int.TryParse(jwtUser.UserId, out int uId))
                {
                    var mapRow = db.Database.SqlQuery<decimal?>(
                        "SELECT EMPLOYEE_ID FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0 AND ROWNUM = 1",
                        new OracleParameter("p0", (decimal)uId)
                    ).FirstOrDefault();

                    if (mapRow.HasValue && mapRow.Value > 0)
                    {
                        return mapRow.Value;
                    }

                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == uId);
                    if (user != null && user.MANV.HasValue && user.MANV.Value > 0)
                    {
                        return user.MANV.Value;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool IsMobileRequest()
        {
            try
            {
                if (Request.Headers.Contains("X-Client-Type"))
                {
                    var clientType = Request.Headers.GetValues("X-Client-Type").FirstOrDefault();
                    if (string.Equals(clientType, "MOBILE", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                if (jwtUser != null && string.Equals(jwtUser.ClientType, "MOBILE", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        private bool HasPermission(MyEntities db, string functionCode, Bu.CLASS_SECURITY.ChannelAction action)
        {
            var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (jwtUser == null || !decimal.TryParse(jwtUser.UserId, out decimal uid) || uid <= 0) return false;
            string channel = Bu.CLASS_SECURITY.AppChannels.Normalize(jwtUser.ClientType) ?? Bu.CLASS_SECURITY.AppChannels.Web;
            return Bu.CLASS_SECURITY.PlatformAccessGuard.Current.CanExecute(db, uid, channel, functionCode, action, jwtUser.Username);
        }

        private bool HasAnyPermission(MyEntities db, Bu.CLASS_SECURITY.ChannelAction action, params string[] functionCodes)
        {
            if (functionCodes == null || functionCodes.Length == 0) return false;
            foreach (var fc in functionCodes)
            {
                if (HasPermission(db, fc, action)) return true;
            }
            return false;
        }

        private IHttpActionResult ForbiddenResult(string message = "Từ chối truy cập: Bạn không có quyền thực hiện thao tác này.")
        {
            return Content(HttpStatusCode.Forbidden, new { success = false, message });
        }

        /// <summary>
        /// GET: api/approvals/summary
        /// Lấy số lượng yêu cầu đang chờ duyệt theo từng loại để hiển thị Badge và Dashboard.
        /// Trên Mobile: Luôn tuân thủ Self-Scope (chỉ đếm đơn của chính tài khoản đó).
        /// </summary>
        [HttpGet]
        [Route("summary")]
        public IHttpActionResult GetSummary()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!IsMobileRequest() && !HasAnyPermission(db, Bu.CLASS_SECURITY.ChannelAction.View, "F_NV_PHEDUYET", "F_APPROVE_ADVANCE", "F_APPROVE_PROFILE", "F_PAYROLL_DISBURSE_ADVANCE"))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền xem trung tâm phê duyệt.");
                    }

                    int leaveCount = 0;
                    int attendanceCount = 0;
                    int overtimeCount = 0;
                    int insuranceCount = 0;
                    int advanceCount = 0;
                    int profileCount = 0;

                    if (IsMobileRequest())
                    {
                        decimal? empId = GetCallerEmployeeId(db);
                        if (empId.HasValue)
                        {
                            leaveCount = (int)db.Database.SqlQuery<decimal>(
                                "SELECT COUNT(*) FROM HR.TB_YEUCAU_NGHIPHEP WHERE TRANGTHAI = 'PENDING' AND MANV = :p0",
                                new OracleParameter("p0", empId.Value)
                            ).FirstOrDefault();

                            attendanceCount = (int)db.Database.SqlQuery<decimal>(
                                "SELECT COUNT(*) FROM HR.TB_YEUCAU_DIEUCHINHCONG WHERE TRANGTHAI = 'PENDING' AND MANV = :p0",
                                new OracleParameter("p0", empId.Value)
                            ).FirstOrDefault();

                            overtimeCount = (int)db.Database.SqlQuery<decimal>(
                                "SELECT COUNT(*) FROM HR.TB_YEUCAU_TANGCA WHERE TRANGTHAI = 'PENDING' AND MANV = :p0",
                                new OracleParameter("p0", empId.Value)
                            ).FirstOrDefault();

                            insuranceCount = (int)db.Database.SqlQuery<decimal>(
                                "SELECT COUNT(*) FROM HR.TB_BAOHIEM_BIENDONG WHERE (TRANG_THAI = 'DRAFT' OR TRANG_THAI = 'PENDING') AND MANV = :p0",
                                new OracleParameter("p0", empId.Value)
                            ).FirstOrDefault();

                            advanceCount = (int)db.Database.SqlQuery<decimal>(
                                "SELECT COUNT(*) FROM HR.TB_YEUCAU_UNGLUONG WHERE TRANGTHAI = 'PENDING' AND MANV = :p0",
                                new OracleParameter("p0", empId.Value)
                            ).FirstOrDefault();

                            profileCount = (int)db.Database.SqlQuery<decimal>(
                                "SELECT COUNT(*) FROM HR.TB_YEUCAU_THAYDOI_HOSO WHERE STATUS = 'PENDING' AND MANV = :p0",
                                new OracleParameter("p0", empId.Value)
                            ).FirstOrDefault();
                        }
                    }
                    else
                    {
                        leaveCount = (int)db.Database.SqlQuery<decimal>(
                            "SELECT COUNT(*) FROM HR.TB_YEUCAU_NGHIPHEP WHERE TRANGTHAI = 'PENDING'"
                        ).FirstOrDefault();

                        attendanceCount = (int)db.Database.SqlQuery<decimal>(
                            "SELECT COUNT(*) FROM HR.TB_YEUCAU_DIEUCHINHCONG WHERE TRANGTHAI = 'PENDING'"
                        ).FirstOrDefault();

                        overtimeCount = (int)db.Database.SqlQuery<decimal>(
                            "SELECT COUNT(*) FROM HR.TB_YEUCAU_TANGCA WHERE TRANGTHAI = 'PENDING'"
                        ).FirstOrDefault();

                        insuranceCount = (int)db.Database.SqlQuery<decimal>(
                            "SELECT COUNT(*) FROM HR.TB_BAOHIEM_BIENDONG WHERE TRANG_THAI = 'DRAFT' OR TRANG_THAI = 'PENDING'"
                        ).FirstOrDefault();

                        advanceCount = (int)db.Database.SqlQuery<decimal>(
                            "SELECT COUNT(*) FROM HR.TB_YEUCAU_UNGLUONG WHERE TRANGTHAI = 'PENDING'"
                        ).FirstOrDefault();

                        profileCount = (int)db.Database.SqlQuery<decimal>(
                            "SELECT COUNT(*) FROM HR.TB_YEUCAU_THAYDOI_HOSO WHERE STATUS = 'PENDING'"
                        ).FirstOrDefault();
                    }

                    return Ok(new
                    {
                        success = true,
                        totalPending = leaveCount + attendanceCount + overtimeCount + insuranceCount + advanceCount + profileCount,
                        leavePending = leaveCount,
                        attendancePending = attendanceCount,
                        overtimePending = overtimeCount,
                        insurancePending = insuranceCount,
                        advancePending = advanceCount,
                        profilePending = profileCount
                    });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
                    if (!IsMobileRequest() && !HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền xem danh sách đơn xin nghỉ phép.");
                    }

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

                    if (IsMobileRequest())
                    {
                        decimal? empId = GetCallerEmployeeId(db);
                        if (empId.HasValue)
                        {
                            sql += " AND Y.MANV = :pSelfManv";
                            parameters.Add(new OracleParameter("pSelfManv", empId.Value));
                        }
                        else
                        {
                            sql += " AND 1=0";
                        }
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
                    if (!HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.Edit))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền phê duyệt đơn xin nghỉ phép.");
                    }

                    decimal userId = GetCurrentUserId(db);
                    decimal? callerEmpId = GetCallerEmployeeId(db);

                    var attendanceService = new Bu.CLASS_CHAMCONG.AttendanceApprovalService();
                    attendanceService.ApproveLeave(id, userId, callerEmpId);

                    return Ok(new { success = true, message = "Phê duyệt đơn xin nghỉ phép thành công!" });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound();
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ApproveLeave Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi phê duyệt: " + ex.Message });
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
                    if (!HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.Edit))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền từ chối đơn xin nghỉ phép.");
                    }

                    decimal userId = GetCurrentUserId(db);
                    decimal? callerEmpId = GetCallerEmployeeId(db);
                    string reason = req?.Reason ?? "Từ chối bởi cấp quản lý";

                    var leaveReq = db.Database.SqlQuery<LeaveReqInfoRow>(
                        "SELECT ID, MANV, TUNGAY, SONGAY, GIAY_NGHI, LYDO, LOAIPHEP, TRANGTHAI FROM HR.TB_YEUCAU_NGHIPHEP WHERE ID = :p0",
                        new OracleParameter("p0", id)
                    ).FirstOrDefault();

                    if (leaveReq == null)
                    {
                        return NotFound();
                    }

                    if (callerEmpId.HasValue && callerEmpId.Value == leaveReq.MANV)
                    {
                        return BadRequest("Bạn không được phép tự từ chối/phê duyệt đơn xin nghỉ phép của chính mình.");
                    }

                    int affected = db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_YEUCAU_NGHIPHEP SET TRANGTHAI = 'REJECTED', NGUOIDUYET = :p0, NGAYDUYET = SYSDATE, GHICHUDUYET = :p1 WHERE ID = :p2 AND TRANGTHAI IN ('PENDING', 'APPROVED')",
                        new OracleParameter("p0", userId),
                        new OracleParameter("p1", reason),
                        new OracleParameter("p2", id)
                    );

                    if (affected == 0)
                    {
                        return BadRequest("Yêu cầu không tồn tại hoặc đã được xử lý trước đó.");
                    }

                    // Nếu đơn này từng được duyệt (có trừ trong TB_PHEP_SOPHATSINH), thực hiện hoàn phép
                    try
                    {
                        var existingDeduction = db.Database.SqlQuery<decimal>(
                            "SELECT COUNT(*) FROM HR.TB_PHEP_SOPHATSINH WHERE IDDON = :p0 AND LOAI = 'SU_DUNG'",
                            new OracleParameter("p0", id)
                        ).FirstOrDefault();

                        if (existingDeduction > 0)
                        {
                            long seconds = leaveReq.GIAY_NGHI.HasValue && leaveReq.GIAY_NGHI.Value > 0
                                ? leaveReq.GIAY_NGHI.Value
                                : (long)((leaveReq.SONGAY ?? 1) * 28800);

                            db.Database.ExecuteSqlCommand(
                                @"INSERT INTO HR.TB_PHEP_SOPHATSINH (MANV, NGAY, LOAI, GIAY_PHEP, IDDON, LY_DO)
                                  VALUES (:p0, SYSDATE, 'HOAN', :p1, :p2, :p3)",
                                new OracleParameter("p0", leaveReq.MANV),
                                new OracleParameter("p1", Math.Abs(seconds)),
                                new OracleParameter("p2", id),
                                new OracleParameter("p3", "Hoàn phép do từ chối/hủy đơn: " + reason)
                            );
                        }
                    }
                    catch (Exception hoanEx) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(hoanEx)) { throw; }
                    catch (Exception hoanEx)
                    {
                        System.Diagnostics.Trace.TraceWarning("[RejectLeave TB_PHEP_SOPHATSINH Refund Warning]: " + hoanEx.Message);
                    }

                    // Thông báo tới nhân viên
                    try
                    {
                        string connStr = Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
                        using (var conn = new OracleConnection(connStr))
                        {
                            conn.Open();
                            Bu.CLASS_SYSTEM.NotificationService.CreateNotification(
                                conn,
                                null,
                                "Đơn xin nghỉ phép bị từ chối",
                                $"Đơn xin nghỉ phép #{id} của bạn đã bị từ chối. Lý do: {reason}.",
                                "NGHI_PHEP",
                                "HR_SYSTEM",
                                targetType: "PERSONAL",
                                targetManv: (long)leaveReq.MANV,
                                eventType: "LEAVE_REJECTED",
                                entityId: id,
                                discretePushBody: "HRMS: Đơn xin nghỉ phép của bạn đã bị từ chối."
                            );
                        }
                    }
                    catch (Exception exNotif) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(exNotif)) { throw; }
                    catch (Exception exNotif)
                    {
                        System.Diagnostics.Trace.TraceWarning("[RejectLeave Notification Warning]: " + exNotif.Message);
                    }

                    return Ok(new { success = true, message = "Đã từ chối đơn xin nghỉ phép." });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
                    if (!IsMobileRequest() && !HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền xem danh sách điều chỉnh công.");
                    }

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

                    if (IsMobileRequest())
                    {
                        decimal? empId = GetCallerEmployeeId(db);
                        if (empId.HasValue)
                        {
                            sql += " AND Y.MANV = :pSelfManv";
                            parameters.Add(new OracleParameter("pSelfManv", empId.Value));
                        }
                        else
                        {
                            sql += " AND 1=0";
                        }
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/approvals/attendance-corrections Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải danh sách điều chỉnh công." });
            }
        }

        /// <summary>
        /// POST: api/approvals/attendance-corrections/{id}/approve
        /// Khi duyệt: Cập nhật yêu cầu thành APPROVED và tự động tính công theo ca kíp thực tế vào TB_BANGCONG_CHITIET
        /// </summary>
        [HttpPost]
        [Route("attendance-corrections/{id:decimal}/approve")]
        public IHttpActionResult ApproveAttendanceCorrection(decimal id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.Edit))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền phê duyệt điều chỉnh công.");
                    }

                    decimal userId = GetCurrentUserId(db);
                    decimal? callerEmpId = GetCallerEmployeeId(db);

                    var attService = new Bu.CLASS_CHAMCONG.AttendanceApprovalService();
                    attService.ApproveAttendanceCorrection(id, userId, callerEmpId);

                    return Ok(new { success = true, message = "Phê duyệt điều chỉnh công thành công và đã đồng bộ dữ liệu chấm công theo ca!" });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound();
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ApproveAttendanceCorrection Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi phê duyệt điều chỉnh công: " + ex.Message });
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
                    if (!HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.Edit))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền từ chối điều chỉnh công.");
                    }

                    decimal userId = GetCurrentUserId(db);
                    decimal? callerEmpId = GetCallerEmployeeId(db);
                    string reason = req?.Reason ?? "Từ chối bởi cấp quản lý";

                    var reqItem = db.Database.SqlQuery<AttendanceApprovalRow>(
                        "SELECT ID AS ID_YEUCAU, MANV, NGAY AS NGAY_CONG, GIO_VAO AS GIO_VAO_MOI, GIO_RA AS GIO_RA_MOI, TRANGTHAI FROM HR.TB_YEUCAU_DIEUCHINHCONG WHERE ID = :p0",
                        new OracleParameter("p0", id)
                    ).FirstOrDefault();

                    if (reqItem == null)
                    {
                        return NotFound();
                    }

                    if (callerEmpId.HasValue && callerEmpId.Value == reqItem.MANV)
                    {
                        return BadRequest("Bạn không được phép tự từ chối/phê duyệt đơn điều chỉnh công của chính mình.");
                    }

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

                    // Thông báo tới nhân viên
                    try
                    {
                        string connStr = Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
                        using (var conn = new OracleConnection(connStr))
                        {
                            conn.Open();
                            Bu.CLASS_SYSTEM.NotificationService.CreateNotification(
                                conn,
                                null,
                                "Điều chỉnh công bị từ chối",
                                $"Giải trình chấm công #{id} của bạn đã bị từ chối. Lý do: {reason}.",
                                "DIEUCHINH_CONG",
                                "HR_SYSTEM",
                                targetType: "PERSONAL",
                                targetManv: (long)reqItem.MANV,
                                eventType: "ATTENDANCE_CORRECTION_REJECTED",
                                entityId: id,
                                discretePushBody: "HRMS: Yêu cầu điều chỉnh công của bạn đã bị từ chối."
                            );
                        }
                    }
                    catch (Exception exNotif) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(exNotif)) { throw; }
                    catch (Exception exNotif)
                    {
                        System.Diagnostics.Trace.TraceWarning("[RejectAttendanceCorrection Notification Warning]: " + exNotif.Message);
                    }

                    return Ok(new { success = true, message = "Đã từ chối yêu cầu điều chỉnh chấm công." });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
                    if (!IsMobileRequest() && !HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền xem danh sách tăng ca.");
                    }

                    string sql = @"
                        SELECT Y.ID AS ID_YEUCAU, Y.MANV, NV.EMPLOYEE_CODE, NV.HOTEN AS EMPLOYEE_NAME, PB.TENPB AS DEPARTMENT_NAME,
                               Y.NGAY AS NGAY_TANGCA, Y.GIOTANGCA AS SO_GIO, NVL(L.HESOLOAICA, 1.0) AS HE_SO, Y.IDCA, NVL(L.TENLOAICA, 'Ca ngày') AS TEN_CA,
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

                    if (IsMobileRequest())
                    {
                        decimal? empId = GetCallerEmployeeId(db);
                        if (empId.HasValue)
                        {
                            sql += " AND Y.MANV = :pSelfManv";
                            parameters.Add(new OracleParameter("pSelfManv", empId.Value));
                        }
                        else
                        {
                            sql += " AND 1=0";
                        }
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
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
                using (var transaction = db.Database.BeginTransaction())
                {
                    if (!HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.Edit))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền phê duyệt tăng ca.");
                    }

                    decimal userId = GetCurrentUserId(db);

                    var reqItem = db.Database.SqlQuery<OvertimeApprovalRow>(
                        @"SELECT Y.ID AS ID_YEUCAU, Y.MANV, Y.NGAY AS NGAY_TANGCA, Y.GIOTANGCA AS SO_GIO, 
                                 NVL(L.HESOLOAICA, 1.0) AS HE_SO, Y.IDCA, NVL(L.TENLOAICA, 'Ca ngày') AS TEN_CA,
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

                    if (!reqItem.NGAY_TANGCA.HasValue || !reqItem.SO_GIO.HasValue || reqItem.SO_GIO.Value <= 0 || reqItem.MANV <= 0)
                        return BadRequest("Đề xuất tăng ca thiếu ngày, nhân viên hoặc số giờ hợp lệ.");

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
                                new OracleParameter("p7", (object)reqItem.IDCA ?? DBNull.Value),
                                new OracleParameter("p8", reqItem.ID_YEUCAU),
                                new OracleParameter("p9", userId)
                            );
                        }
                        catch (Exception exTc) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(exTc)) { throw; }
                        catch (Exception exTc)
                        {
                            throw new Bu.CLASS_SYSTEM.BusinessException("OVERTIME_SYNC_FAILED", "Không ghi được giờ tăng ca; yêu cầu chưa được phê duyệt.", exTc);
                        }
                    }

                    // 3. Thông báo tới nhân viên
                    try
                    {
                        string connStr = Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
                        using (var conn = new OracleConnection(connStr))
                        {
                            conn.Open();
                            Bu.CLASS_SYSTEM.NotificationService.CreateNotification(
                                conn,
                                null,
                                "Đăng ký tăng ca đã được phê duyệt",
                                $"Đề xuất tăng ca #{id} ({(reqItem.SO_GIO ?? 0):N1} giờ) ngày {(reqItem.NGAY_TANGCA.HasValue ? reqItem.NGAY_TANGCA.Value.ToString("dd/MM/yyyy") : "")} của bạn đã được phê duyệt thành công.",
                                "TANG_CA",
                                "HR_SYSTEM",
                                targetType: "PERSONAL",
                                targetManv: (long)reqItem.MANV,
                                eventType: "OVERTIME_APPROVED",
                                entityId: id,
                                discretePushBody: "HRMS: Đề xuất tăng ca của bạn đã được phê duyệt."
                            );
                        }
                    }
                    catch (Exception exNotif) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(exNotif)) { throw; }
                    catch (Exception exNotif)
                    {
                        System.Diagnostics.Trace.TraceWarning("[ApproveOvertime Notification Warning]: " + exNotif.Message);
                    }

                    transaction.Commit();
                    return Ok(new { success = true, message = "Phê duyệt đề xuất tăng ca thành công và đã đồng bộ vào bảng tăng ca thực tế!" });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
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
                    if (!HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.Edit))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền từ chối tăng ca.");
                    }

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

                    // Thông báo tới nhân viên
                    try
                    {
                        string connStr = Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
                        using (var conn = new OracleConnection(connStr))
                        {
                            conn.Open();
                            Bu.CLASS_SYSTEM.NotificationService.CreateNotification(
                                conn,
                                null,
                                "Đăng ký tăng ca bị từ chối",
                                $"Đề xuất tăng ca #{id} của bạn đã bị từ chối. Lý do: {req.Reason.Trim()}.",
                                "TANG_CA",
                                "HR_SYSTEM",
                                targetType: "PERSONAL",
                                targetManv: (long)reqItem.MANV,
                                eventType: "OVERTIME_REJECTED",
                                entityId: id,
                                discretePushBody: "HRMS: Đề xuất tăng ca của bạn đã bị từ chối."
                            );
                        }
                    }
                    catch (Exception exNotif) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(exNotif)) { throw; }
                    catch (Exception exNotif)
                    {
                        System.Diagnostics.Trace.TraceWarning("[RejectOvertime Notification Warning]: " + exNotif.Message);
                    }

                    return Ok(new { success = true, message = "Đã từ chối đề xuất đăng ký tăng ca." });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[RejectOvertime Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi từ chối." });
            }
        }

        #endregion

        #region Biến động bảo hiểm (Insurance Movement Approvals)

        /// <summary>
        /// GET: api/approvals/insurance-movements?status=PENDING&search=
        /// </summary>
        [HttpGet]
        [Route("insurance-movements")]
        public IHttpActionResult GetInsuranceMovements(string status = "ALL", string search = null)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!IsMobileRequest() && !HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.View))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền xem danh sách biến động bảo hiểm.");
                    }

                    string sql = @"
                        SELECT 
                            B.ID,
                            B.MANV,
                            NV.MANV AS EMPLOYEE_CODE,
                            NV.HOTEN AS EMPLOYEE_NAME,
                            NVL(PB.TENPB, 'Chưa phân bổ') AS DEPARTMENT_NAME,
                            B.MAKYCONG,
                            B.LOAI,
                            B.NGAY_HIEULUC,
                            B.LY_DO,
                            B.TRANG_THAI,
                            B.NGUOI_DUYET,
                            NVL(U.FULLNAME, U.USERNAME) AS TEN_NGUOI_DUYET
                        FROM HR.TB_BAOHIEM_BIENDONG B
                        JOIN HR.TB_NHANVIEN NV ON B.MANV = NV.MANV
                        LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                        LEFT JOIN HR.TB_SYS_USER U ON B.NGUOI_DUYET = U.IDUSER
                        WHERE 1=1";

                    var parameters = new List<OracleParameter>();

                    if (!string.IsNullOrWhiteSpace(status) && !status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                    {
                        if (status.Equals("PENDING", StringComparison.OrdinalIgnoreCase))
                        {
                            sql += " AND (B.TRANG_THAI = 'PENDING' OR B.TRANG_THAI = 'DRAFT')";
                        }
                        else
                        {
                            sql += " AND B.TRANG_THAI = :pStatus";
                            parameters.Add(new OracleParameter("pStatus", status.ToUpper()));
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        sql += " AND (LOWER(NV.HOTEN) LIKE :pSearch OR LOWER(B.LY_DO) LIKE :pSearch)";
                        parameters.Add(new OracleParameter("pSearch", "%" + search.ToLower().Trim() + "%"));
                    }

                    if (IsMobileRequest())
                    {
                        decimal? empId = GetCallerEmployeeId(db);
                        if (empId.HasValue)
                        {
                            sql += " AND B.MANV = :pSelfManv";
                            parameters.Add(new OracleParameter("pSelfManv", empId.Value));
                        }
                        else
                        {
                            sql += " AND 1=0";
                        }
                    }

                    sql += " ORDER BY B.NGAY_HIEULUC DESC, B.ID DESC";

                    var rows = db.Database.SqlQuery<InsuranceMovementRow>(sql, parameters.ToArray()).ToList();

                    var data = rows.Select(r => new
                    {
                        id = r.ID,
                        manv = r.MANV,
                        employeeCode = "NV" + r.EMPLOYEE_CODE.ToString().PadLeft(3, '0'),
                        employeeName = r.EMPLOYEE_NAME,
                        departmentName = r.DEPARTMENT_NAME,
                        maKyCong = r.MAKYCONG,
                        loai = r.LOAI,
                        ngayHieuLuc = r.NGAY_HIEULUC.ToString("dd/MM/yyyy"),
                        lyDo = r.LY_DO,
                        trangThai = r.TRANG_THAI,
                        nguoiDuyet = r.TEN_NGUOI_DUYET,
                        idNguoiDuyet = r.NGUOI_DUYET
                    }).ToList();

                    return Ok(new { success = true, data });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/approvals/insurance-movements Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải danh sách biến động bảo hiểm." });
            }
        }

        /// <summary>
        /// POST: api/approvals/insurance-movements/{id}/approve
        /// </summary>
        [HttpPost]
        [Route("insurance-movements/{id:decimal}/approve")]
        public IHttpActionResult ApproveInsuranceMovement(decimal id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.Edit))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền phê duyệt biến động bảo hiểm.");
                    }

                    decimal userId = GetCurrentUserId(db);
                    decimal? callerEmpId = GetCallerEmployeeId(db);

                    var mv = db.Database.SqlQuery<decimal?>("SELECT MANV FROM HR.TB_BAOHIEM_BIENDONG WHERE ID = :p0", new OracleParameter("p0", id)).FirstOrDefault();
                    if (mv.HasValue && callerEmpId.HasValue && callerEmpId.Value == mv.Value)
                    {
                        return BadRequest("Bạn không được phép tự phê duyệt biến động bảo hiểm của chính mình.");
                    }

                    int affected = db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_BAOHIEM_BIENDONG SET TRANG_THAI = 'APPROVED', NGUOI_DUYET = :p0 WHERE ID = :p1 AND (TRANG_THAI = 'DRAFT' OR TRANG_THAI = 'PENDING')",
                        new OracleParameter("p0", userId),
                        new OracleParameter("p1", id)
                    );

                    if (affected == 0)
                    {
                        return BadRequest("Biến động bảo hiểm không tồn tại hoặc đã được xử lý trước đó.");
                    }

                    return Ok(new { success = true, message = "Phê duyệt biến động bảo hiểm thành công!" });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ApproveInsuranceMovement Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi phê duyệt." });
            }
        }

        /// <summary>
        /// POST: api/approvals/insurance-movements/{id}/reject
        /// </summary>
        [HttpPost]
        [Route("insurance-movements/{id:decimal}/reject")]
        public IHttpActionResult RejectInsuranceMovement(decimal id, [FromBody] RejectActionRequest req)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!HasPermission(db, "F_NV_PHEDUYET", Bu.CLASS_SECURITY.ChannelAction.Edit))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền từ chối biến động bảo hiểm.");
                    }

                    decimal userId = GetCurrentUserId(db);
                    decimal? callerEmpId = GetCallerEmployeeId(db);
                    string reason = req?.Reason ?? "Từ chối bởi cấp quản lý";

                    var mv = db.Database.SqlQuery<decimal?>("SELECT MANV FROM HR.TB_BAOHIEM_BIENDONG WHERE ID = :p0", new OracleParameter("p0", id)).FirstOrDefault();
                    if (mv.HasValue && callerEmpId.HasValue && callerEmpId.Value == mv.Value)
                    {
                        return BadRequest("Bạn không được phép tự từ chối biến động bảo hiểm của chính mình.");
                    }

                    int affected = db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_BAOHIEM_BIENDONG SET TRANG_THAI = 'REJECTED', NGUOI_DUYET = :p0, LY_DO = SUBSTR(LY_DO || ' [Từ chối: ' || :p1 || ']', 1, 500) WHERE ID = :p2 AND (TRANG_THAI = 'DRAFT' OR TRANG_THAI = 'PENDING')",
                        new OracleParameter("p0", userId),
                        new OracleParameter("p1", reason),
                        new OracleParameter("p2", id)
                    );

                    if (affected == 0)
                    {
                        return BadRequest("Biến động bảo hiểm không tồn tại hoặc đã được xử lý trước đó.");
                    }

                    return Ok(new { success = true, message = "Đã từ chối biến động bảo hiểm." });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[RejectInsuranceMovement Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi từ chối." });
            }
        }

        #endregion

        #region Tạm ứng lương (Salary Advance Approvals)

        /// <summary>
        /// GET: api/approvals/advance
        /// </summary>
        [HttpGet]
        [Route("advance")]
        public IHttpActionResult GetAdvanceList(int? thang = null, int? nam = null, string status = null, string search = null, int page = 1, int pageSize = 20)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!IsMobileRequest() && !HasAnyPermission(db, Bu.CLASS_SECURITY.ChannelAction.View, "F_NV_PHEDUYET", "F_APPROVE_ADVANCE"))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền xem danh sách tạm ứng lương.");
                    }

                    decimal? selfEmpId = IsMobileRequest() ? GetCallerEmployeeId(db) : null;
                    var advanceService = new Bu.CLASS_PAYROLL.SalaryAdvanceService();
                    int total;
                    var list = advanceService.GetList(thang, nam, status, search, selfEmpId, page, pageSize, out total);
                    var data = list.Select(r => new
                    {
                        id = r.Id,
                        manv = r.Manv,
                        employeeCode = r.EmployeeCode,
                        employeeName = r.EmployeeName,
                        departmentName = r.DepartmentName,
                        amount = r.Amount,
                        advanceDate = r.AdvanceDate,
                        thang = r.Thang,
                        nam = r.Nam,
                        reason = r.Reason,
                        status = r.Status,
                        approverName = r.ApproverName,
                        approvedDate = r.ApprovedDate,
                        rejectReason = r.RejectReason,
                        disbursedDate = r.DisbursedDate,
                        voucherNo = r.VoucherNo,
                        disbursedByName = r.DisbursedByName,
                        disburseNote = r.DisburseNote,
                        idUlLink = r.IdUlLink,
                        createdAt = r.CreatedAt
                    }).ToList();

                    return Ok(new
                    {
                        success = true,
                        total = total,
                        page = page,
                        pageSize = pageSize,
                        data = data
                    });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/approvals/advance Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải danh sách tạm ứng lương." });
            }
        }

        /// <summary>
        /// POST: api/approvals/advance/{id}/approve
        /// </summary>
        [HttpPost]
        [Route("advance/{id:long}/approve")]
        public IHttpActionResult ApproveAdvance(long id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!HasAnyPermission(db, Bu.CLASS_SECURITY.ChannelAction.Edit, "F_NV_PHEDUYET", "F_APPROVE_ADVANCE"))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền phê duyệt tạm ứng lương.");
                    }

                    decimal userId = GetCurrentUserId(db);
                    string username = GetCurrentUsername();
                    decimal? callerEmpId = GetCallerEmployeeId(db);

                    var advanceService = new Bu.CLASS_PAYROLL.SalaryAdvanceService();
                    advanceService.ApproveRequest(id, (long)userId, username, callerEmpId);

                    return Ok(new { success = true, message = "Phê duyệt đơn tạm ứng lương thành công!" });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound();
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ApproveAdvance Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi phê duyệt tạm ứng lương: " + ex.Message });
            }
        }

        /// <summary>
        /// POST: api/approvals/advance/{id}/reject
        /// </summary>
        [HttpPost]
        [Route("advance/{id:long}/reject")]
        public IHttpActionResult RejectAdvance(long id, [FromBody] RejectActionRequest req)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!HasAnyPermission(db, Bu.CLASS_SECURITY.ChannelAction.Edit, "F_NV_PHEDUYET", "F_APPROVE_ADVANCE"))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền từ chối tạm ứng lương.");
                    }

                    decimal userId = GetCurrentUserId(db);
                    decimal? callerEmpId = GetCallerEmployeeId(db);
                    string reason = req?.Reason ?? "Từ chối bởi cấp quản lý";

                    var advanceService = new Bu.CLASS_PAYROLL.SalaryAdvanceService();
                    advanceService.RejectRequest(id, (long)userId, reason, callerEmpId);

                    return Ok(new { success = true, message = "Đã từ chối đơn tạm ứng lương." });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound();
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[RejectAdvance Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi từ chối tạm ứng lương: " + ex.Message });
            }
        }

        /// <summary>
        /// POST: api/approvals/advance/{id}/disburse
        /// Kế toán giải ngân khoản tạm ứng (Ghi sổ TB_UNGLUONG và chuyển trạng thái DISBURSED)
        /// </summary>
        [HttpPost]
        [Route("advance/{id:long}/disburse")]
        public IHttpActionResult DisburseAdvance(long id, [FromBody] HRMS_API.Models.DisburseAdvanceRequest req)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!HasPermission(db, "F_PAYROLL_DISBURSE_ADVANCE", Bu.CLASS_SECURITY.ChannelAction.Edit))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền giải ngân tạm ứng lương.");
                    }

                    decimal userId = GetCurrentUserId(db);
                    DateTime ngayChi = DateTime.Today;
                    if (!string.IsNullOrWhiteSpace(req?.NgayChi) && DateTime.TryParse(req.NgayChi, out var parsedDate))
                    {
                        ngayChi = parsedDate;
                    }

                    string soChungTu = req?.SoChungTu;
                    string ghiChuChi = req?.GhiChu;

                    if (string.IsNullOrWhiteSpace(soChungTu))
                    {
                        return BadRequest("Vui lòng nhập số chứng từ chi tiền.");
                    }

                    var advanceService = new Bu.CLASS_PAYROLL.SalaryAdvanceService();
                    long idUl = advanceService.DisburseAdvance(id, (long)userId, ngayChi, soChungTu, ghiChuChi);

                    return Ok(new { success = true, message = $"Giải ngân tạm ứng thành công! Mã phiếu chi: #{idUl}", idUl = idUl });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound();
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[DisburseAdvance Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi giải ngân: " + ex.Message });
            }
        }

        #endregion

        #region Thay đổi hồ sơ (Profile Change Approvals)

        /// <summary>
        /// GET: api/approvals/profile-requests
        /// </summary>
        [HttpGet]
        [Route("profile-requests")]
        public IHttpActionResult GetProfileRequests(string groupCode = null, string status = null, string search = null, int page = 1, int pageSize = 20)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!IsMobileRequest() && !HasAnyPermission(db, Bu.CLASS_SECURITY.ChannelAction.View, "F_NV_PHEDUYET", "F_APPROVE_PROFILE"))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền xem danh sách yêu cầu thay đổi hồ sơ.");
                    }

                    decimal? selfEmpId = IsMobileRequest() ? GetCallerEmployeeId(db) : null;
                    var profileService = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                    int total;
                    var list = profileService.GetRequests(groupCode, status, search, selfEmpId, page, pageSize, out total);
                    var data = list.Select(r => new
                    {
                        id = r.Id,
                        manv = r.Manv,
                        employeeCode = r.EmployeeCode,
                        employeeName = r.EmployeeName,
                        departmentName = r.DepartmentName,
                        groupCode = r.GroupCode,
                        reqType = r.ReqType,
                        currentDataJson = r.CurrentDataJson,
                        proposedDataJson = r.ProposedDataJson,
                        hasProposedAvatarBlob = r.HasProposedAvatarBlob,
                        status = r.Status,
                        reason = r.Reason,
                        rejectReason = r.RejectReason,
                        resolvedByName = r.ResolvedByName,
                        resolvedAt = r.ResolvedAt,
                        appliedAt = r.AppliedAt,
                        createdAt = r.CreatedAt
                    }).ToList();

                    return Ok(new
                    {
                        success = true,
                        total = total,
                        page = page,
                        pageSize = pageSize,
                        data = data
                    });
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET /api/approvals/profile-requests Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi khi tải danh sách yêu cầu thay đổi hồ sơ." });
            }
        }

        /// <summary>
        /// GET: api/approvals/profile-requests/{id}/avatar
        /// </summary>
        [HttpGet]
        [Route("profile-requests/{id:long}/avatar")]
        public System.Net.Http.HttpResponseMessage GetProposedAvatar(long id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    bool canView = HasAnyPermission(db, Bu.CLASS_SECURITY.ChannelAction.View, "F_NV_PHEDUYET", "F_APPROVE_PROFILE");
                    if (!canView)
                    {
                        decimal? callerEmpId = GetCallerEmployeeId(db);
                        var reqEmpId = db.Database.SqlQuery<decimal?>("SELECT MANV FROM HR.TB_YEUCAU_THAYDOI_HOSO WHERE ID = :p0", new OracleParameter("p0", (decimal)id)).FirstOrDefault();
                        if (!callerEmpId.HasValue || !reqEmpId.HasValue || callerEmpId.Value != reqEmpId.Value)
                        {
                            return Request.CreateResponse(HttpStatusCode.Forbidden, new { success = false, message = "Từ chối truy cập: Bạn không có quyền xem ảnh đại diện của yêu cầu này." });
                        }
                    }

                    var profileService = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                    byte[] bytes = profileService.GetProposedAvatar(id);
                    if (bytes == null || bytes.Length == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound);
                    }

                    var response = new System.Net.Http.HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new System.Net.Http.ByteArrayContent(bytes);
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                    return response;
                }
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[GET proposed avatar Error]: " + ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// POST: api/approvals/profile-requests/{id}/approve
        /// </summary>
        [HttpPost]
        [Route("profile-requests/{id:long}/approve")]
        public IHttpActionResult ApproveProfileRequest(long id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!HasAnyPermission(db, Bu.CLASS_SECURITY.ChannelAction.Edit, "F_NV_PHEDUYET", "F_APPROVE_PROFILE"))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền phê duyệt yêu cầu thay đổi hồ sơ.");
                    }

                    decimal userId = GetCurrentUserId(db);
                    decimal? callerEmpId = GetCallerEmployeeId(db);

                    var profileService = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                    profileService.ApproveRequest(id, (long)userId, callerEmpId);

                    return Ok(new { success = true, message = "Phê duyệt và cập nhật hồ sơ thành công!" });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound();
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ApproveProfileRequest Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi phê duyệt hồ sơ: " + ex.Message });
            }
        }

        /// <summary>
        /// POST: api/approvals/profile-requests/{id}/reject
        /// </summary>
        [HttpPost]
        [Route("profile-requests/{id:long}/reject")]
        public IHttpActionResult RejectProfileRequest(long id, [FromBody] RejectActionRequest req)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    if (!HasAnyPermission(db, Bu.CLASS_SECURITY.ChannelAction.Edit, "F_NV_PHEDUYET", "F_APPROVE_PROFILE"))
                    {
                        return ForbiddenResult("Từ chối truy cập: Bạn không có quyền từ chối yêu cầu thay đổi hồ sơ.");
                    }

                    decimal userId = GetCurrentUserId(db);
                    decimal? callerEmpId = GetCallerEmployeeId(db);
                    string reason = req?.Reason ?? "Từ chối bởi cấp quản lý";

                    var profileService = new Bu.CLASS_NHANSU.EmployeeProfileRequestService();
                    profileService.RejectRequest(id, (long)userId, reason, callerEmpId);

                    return Ok(new { success = true, message = "Đã từ chối yêu cầu thay đổi hồ sơ." });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound();
            }
            catch (Exception ex) when (Bu.CLASS_SYSTEM.ErrorHelper.IsPayrollFrozen(ex)) { throw; }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[RejectProfileRequest Error]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Lỗi hệ thống khi từ chối hồ sơ: " + ex.Message });
            }
        }

        #endregion
    }

    public class LeaveReqInfoRow
    {
        public decimal ID { get; set; }
        public decimal MANV { get; set; }
        public DateTime? TUNGAY { get; set; }
        public decimal? SONGAY { get; set; }
        public long? GIAY_NGHI { get; set; }
        public string LYDO { get; set; }
        public string LOAIPHEP { get; set; }
        public string TRANGTHAI { get; set; }
    }

    public class InsuranceMovementRow
    {
        public decimal ID { get; set; }
        public decimal MANV { get; set; }
        public decimal EMPLOYEE_CODE { get; set; }
        public string EMPLOYEE_NAME { get; set; }
        public string DEPARTMENT_NAME { get; set; }
        public decimal MAKYCONG { get; set; }
        public string LOAI { get; set; }
        public DateTime NGAY_HIEULUC { get; set; }
        public string LY_DO { get; set; }
        public string TRANG_THAI { get; set; }
        public decimal? NGUOI_DUYET { get; set; }
        public string TEN_NGUOI_DUYET { get; set; }
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
