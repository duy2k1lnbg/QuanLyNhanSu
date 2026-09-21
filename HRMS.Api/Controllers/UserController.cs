using Bu.CLASS_SYSTEM;
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
    [JwtAuthorize(RequireAdmin = true)]
    [RoutePrefix("api/users")]
    public class UserController : ApiController
    {
        private readonly SYS_USER _userBus = new SYS_USER();

        /// <summary>
        /// GET: api/users
        /// Lấy danh sách tài khoản người dùng và nhóm quyền với đầy đủ thông tin chuẩn hóa
        /// </summary>
        [HttpGet]
        [Route("")]
        [Route("~/api/user")]
        public IHttpActionResult GetAllUsers()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var rawUsers = db.TB_SYS_USER.ToList();
                    var allGroups = db.TB_SYS_GROUP.ToList();

                    var mappings = new List<UserMappingRow>();
                    try
                    {
                        mappings = db.Database.SqlQuery<UserMappingRow>(
                            "SELECT M.USER_ID, M.EMPLOYEE_ID, M.IS_MOBILE_ENABLED, NV.EMPLOYEE_CODE, NV.HOTEN AS EMPLOYEE_NAME " +
                            "FROM HR.TB_USER_EMPLOYEE_MAPPING M " +
                            "LEFT JOIN HR.TB_NHANVIEN NV ON M.EMPLOYEE_ID = NV.MANV"
                        ).ToList();
                    }
                    catch { }

                    // Tra cứu dự phòng hồ sơ nhân viên cho tài khoản có MANV nhưng chưa có trong bảng mapping
                    var fallbackEmployees = new List<EmpCheckRow>();
                    try
                    {
                        fallbackEmployees = db.Database.SqlQuery<EmpCheckRow>(
                            "SELECT MANV, EMPLOYEE_CODE, HOTEN, DATHOIVIEC FROM HR.TB_NHANVIEN"
                        ).ToList();
                    }
                    catch { }

                    var list = rawUsers.Select(u =>
                    {
                        bool isGroup = (u.ISGROUP ?? 0) == 1;
                        bool isDisabled = (u.DISABLED ?? 0) == 1;
                        bool isAdmin = u.USERNAME != null && u.USERNAME.Trim().ToUpper() == "ADMIN";

                        int memberCount = isGroup ? allGroups.Count(g => g.ID_GROUP == u.IDUSER) : 0;
                        
                        List<string> groupNames = new List<string>();
                        if (!isGroup)
                        {
                            var parentGroupIds = allGroups.Where(g => g.MEMBER == u.IDUSER).Select(g => g.ID_GROUP).ToList();
                            groupNames = rawUsers.Where(grp => parentGroupIds.Contains(grp.IDUSER))
                                                 .Select(grp => grp.FULLNAME ?? grp.USERNAME)
                                                 .ToList();
                        }

                        var map = mappings.FirstOrDefault(m => m.USER_ID == u.IDUSER);
                        decimal? manv = null;
                        string empCode = null;
                        string empName = null;
                        bool isMobileEnabled = true;

                        if (isAdmin)
                        {
                            manv = null;
                            empCode = null;
                            empName = null;
                            isMobileEnabled = false;
                        }
                        else if (map != null)
                        {
                            manv = map.EMPLOYEE_ID;
                            empCode = map.EMPLOYEE_CODE;
                            empName = map.EMPLOYEE_NAME;
                            isMobileEnabled = (map.IS_MOBILE_ENABLED ?? 1) == 1;
                        }
                        else if (u.MANV.HasValue && u.MANV.Value > 0)
                        {
                            manv = u.MANV.Value;
                            var fbEmp = fallbackEmployees.FirstOrDefault(e => e.MANV == manv.Value);
                            if (fbEmp != null)
                            {
                                empCode = fbEmp.EMPLOYEE_CODE;
                                empName = fbEmp.HOTEN;
                            }
                            isMobileEnabled = (u.CLIENT_TYPE ?? "ALL").Trim().ToUpperInvariant() != "DESKTOP";
                        }
                        else
                        {
                            isMobileEnabled = (u.CLIENT_TYPE ?? "ALL").Trim().ToUpperInvariant() != "DESKTOP";
                        }

                        return new
                        {
                            IdUser = (int)u.IDUSER,
                            id = (int)u.IDUSER,
                            IDUSER = (int)u.IDUSER,
                            Username = u.USERNAME,
                            username = u.USERNAME,
                            USERNAME = u.USERNAME,
                            FullName = u.FULLNAME ?? u.USERNAME,
                            fullName = u.FULLNAME ?? u.USERNAME,
                            FULLNAME = u.FULLNAME ?? u.USERNAME,
                            IsGroup = isGroup,
                            isGroup = isGroup,
                            ISGROUP = isGroup ? 1 : 0,
                            Disabled = isDisabled,
                            disabled = isDisabled,
                            DISABLED = isDisabled ? 1 : 0,
                            IsAdmin = isAdmin,
                            isAdmin = isAdmin,
                            MemberCount = memberCount,
                            memberCount = memberCount,
                            Groups = groupNames,
                            groups = groupNames,
                            MACTY = u.MACTY,
                            MADVI = u.MADVI,
                            Manv = manv,
                            manv = manv,
                            EmployeeCode = empCode,
                            employeeCode = empCode,
                            EmployeeName = empName,
                            employeeName = empName,
                            IsMobileEnabled = isMobileEnabled,
                            isMobileEnabled = isMobileEnabled,
                            ClientType = u.CLIENT_TYPE ?? "ALL",
                            clientType = u.CLIENT_TYPE ?? "ALL"
                        };
                    }).OrderBy(u => u.IsGroup ? 0 : 1).ThenBy(u => u.Username).ToList();

                    return Ok(list);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh sách người dùng: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh sách người dùng." });
            }
        }

        /// <summary>
        /// POST: api/users
        /// Tạo mới tài khoản người dùng hoặc nhóm quyền
        /// </summary>
        [HttpPost]
        [Route("")]
        [Route("~/api/user")]
        public IHttpActionResult CreateUser([FromBody] CreateUserRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.Username))
                {
                    return BadRequest("Tên đăng nhập / Mã nhóm không được để trống.");
                }

                if (!req.IsGroup && string.IsNullOrWhiteSpace(req.Password))
                {
                    return BadRequest("Mật khẩu tài khoản không được để trống.");
                }

                using (var db = new MyEntities())
                {
                    string cleanUsername = req.Username.Trim();
                    if (db.TB_SYS_USER.Any(u => u.USERNAME.Trim().ToLower() == cleanUsername.ToLower()))
                    {
                        return BadRequest($"Tên tài khoản / Mã nhóm '{cleanUsername}' đã tồn tại trong hệ thống.");
                    }

                    var newUser = new TB_SYS_USER
                    {
                        USERNAME = cleanUsername,
                        FULLNAME = req.FullName ?? cleanUsername,
                        PASSWORD = req.IsGroup ? "" : PasswordHasher.HashPassword(req.Password.Trim()),
                        ISGROUP = req.IsGroup ? 1 : 0,
                        DISABLED = 0,
                        MACTY = "1",
                        MADVI = "1"
                    };

                    db.TB_SYS_USER.Add(newUser);
                    db.SaveChanges();

                    return Ok(new
                    {
                        success = true,
                        IdUser = (int)newUser.IDUSER,
                        id = (int)newUser.IDUSER,
                        Username = newUser.USERNAME,
                        FullName = newUser.FULLNAME,
                        IsGroup = req.IsGroup,
                        message = req.IsGroup ? $"Đã tạo nhóm quyền [{newUser.USERNAME}] thành công." : $"Đã tạo tài khoản [{newUser.USERNAME}] thành công."
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tạo tài khoản / nhóm: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tạo tài khoản / nhóm." });
            }
        }

        /// <summary>
        /// PUT: api/users/{id}
        /// Cập nhật thông tin tài khoản hoặc nhóm quyền
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        [Route("~/api/user/{id:int}")]
        public IHttpActionResult UpdateUser(int id, [FromBody] UpdateUserRequest req)
        {
            try
            {
                if (req == null) return BadRequest("Dữ liệu không hợp lệ.");

                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == id);
                    if (user == null) return NotFound();

                    if (!string.IsNullOrWhiteSpace(req.FullName)) user.FULLNAME = req.FullName;
                    
                    if (req.Disabled.HasValue)
                    {
                        if (user.USERNAME.Trim().ToUpper() == "ADMIN" && req.Disabled.Value)
                        {
                            return BadRequest("Không thể khóa tài khoản Quản trị hệ thống (ADMIN).");
                        }
                        user.DISABLED = req.Disabled.Value ? 1 : 0;
                    }

                    if (!string.IsNullOrWhiteSpace(req.NewPassword))
                    {
                        user.PASSWORD = PasswordHasher.HashPassword(req.NewPassword);
                    }

                    db.SaveChanges();
                    return Ok(new { success = true, message = $"Đã cập nhật thông tin tài khoản #{id} ({user.USERNAME})." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi cập nhật tài khoản: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi cập nhật tài khoản." });
            }
        }

        /// <summary>
        /// PUT: api/users/{id}/toggle-lock
        /// Khóa hoặc mở khóa nhanh tài khoản người dùng
        /// </summary>
        [HttpPost]
        [HttpPut]
        [Route("{id:int}/toggle-lock")]
        [Route("~/api/user/{id:int}/toggle-lock")]
        public IHttpActionResult ToggleLock(int id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == id);
                    if (user == null) return NotFound();

                    if (user.USERNAME.Trim().ToUpper() == "ADMIN")
                    {
                        return BadRequest("Không thể khóa tài khoản Quản trị hệ thống (ADMIN).");
                    }

                    bool willLock = (user.DISABLED ?? 0) == 0;
                    user.DISABLED = willLock ? 1 : 0;
                    db.SaveChanges();

                    WriteAuditLog(db, willLock ? "LOCK_USER" : "UNLOCK_USER", user.IDUSER.ToString(),
                        willLock ? $"Đã khóa tài khoản [{user.USERNAME}]." : $"Đã mở khóa tài khoản [{user.USERNAME}].");

                    return Ok(new
                    {
                        success = true,
                        disabled = willLock,
                        message = willLock ? $"Đã khóa tài khoản [{user.USERNAME}]." : $"Đã mở khóa tài khoản [{user.USERNAME}]."
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi khóa/mở khóa tài khoản: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi thay đổi trạng thái tài khoản." });
            }
        }

        /// <summary>
        /// POST: api/users/{id}/reset-password
        /// Quản trị viên đặt lại mật khẩu cho tài khoản
        /// </summary>
        [HttpPost]
        [Route("{id:int}/reset-password")]
        [Route("~/api/user/{id:int}/reset-password")]
        public IHttpActionResult ResetPassword(int id, [FromBody] ResetPasswordRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.NewPassword))
                {
                    return BadRequest("Vui lòng nhập mật khẩu mới.");
                }

                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == id);
                    if (user == null) return NotFound();

                    user.PASSWORD = PasswordHasher.HashPassword(req.NewPassword);
                    db.SaveChanges();

                    WriteAuditLog(db, "RESET_PASSWORD", user.IDUSER.ToString(),
                        $"Đã đặt lại mật khẩu cho tài khoản [{user.USERNAME}].");

                    return Ok(new { success = true, message = $"Đã đặt lại mật khẩu cho tài khoản [{user.USERNAME}] thành công." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi đặt lại mật khẩu: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi đặt lại mật khẩu." });
            }
        }

        /// <summary>
        /// DELETE: api/users/{id}
        /// Xóa tài khoản hoặc nhóm quyền, đồng thời dọn dẹp quan hệ TB_SYS_GROUP và TB_SYS_RIGHT
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        [Route("~/api/user/{id:int}")]
        public IHttpActionResult DeleteUser(int id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == id);
                    if (user == null) return NotFound();

                    if (user.USERNAME.Trim().ToUpper() == "ADMIN")
                    {
                        return BadRequest("Không thể xóa tài khoản Quản trị hệ thống (ADMIN).");
                    }

                    // 1. Xóa phân quyền trong TB_SYS_RIGHT
                    var rights = db.TB_SYS_RIGHT.Where(r => r.IDUSER == id).ToList();
                    db.TB_SYS_RIGHT.RemoveRange(rights);

                    // 2. Xóa liên kết nhóm trong TB_SYS_GROUP
                    var groupMemberships = db.TB_SYS_GROUP.Where(g => g.MEMBER == id || g.ID_GROUP == id).ToList();
                    db.TB_SYS_GROUP.RemoveRange(groupMemberships);

                    // 3. Xóa phân quyền báo cáo nếu có
                    var reportRights = db.TB_SYS_RIGHT_REPORT.Where(r => r.IDUSER == id).ToList();
                    db.TB_SYS_RIGHT_REPORT.RemoveRange(reportRights);

                    // 4. Xóa liên kết nhân viên trong TB_USER_EMPLOYEE_MAPPING nếu có
                    try
                    {
                        db.Database.ExecuteSqlCommand(
                            "DELETE FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                            new OracleParameter("p0", id)
                        );
                    }
                    catch { }

                    // 5. Xóa đối tượng người dùng / nhóm
                    db.TB_SYS_USER.Remove(user);
                    db.SaveChanges();

                    return Ok(new { success = true, message = $"Đã xóa tài khoản / nhóm [{user.USERNAME}] thành công." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi xóa người dùng/nhóm: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi xóa người dùng/nhóm." });
            }
        }

        /// <summary>
        /// POST: api/users/{id}/link-employee
        /// Liên kết 1-1 an toàn giữa User Account và Employee Profile
        /// </summary>
        [HttpPost]
        [Route("{id:int}/link-employee")]
        [Route("~/api/user/{id:int}/link-employee")]
        public IHttpActionResult LinkEmployee(int id, [FromBody] LinkEmployeeRequest req)
        {
            try
            {
                if (req == null || req.EmployeeId <= 0)
                {
                    return BadRequest("Mã nhân viên (EmployeeId) không hợp lệ.");
                }

                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == id);
                    if (user == null) return NotFound();

                    if ((user.ISGROUP ?? 0) == 1)
                    {
                        return BadRequest("Không thể liên kết hồ sơ nhân viên với nhóm quyền (Group).");
                    }

                    if (user.USERNAME != null && user.USERNAME.Trim().ToUpper() == "ADMIN")
                    {
                        return BadRequest("Tài khoản Quản trị viên tối cao (ADMIN) là tài khoản quản trị hệ thống, không được phép liên kết với hồ sơ nhân viên.");
                    }

                    // 1. Kiểm tra hồ sơ nhân viên có tồn tại không
                    var emp = db.Database.SqlQuery<EmpCheckRow>(
                        "SELECT MANV, EMPLOYEE_CODE, HOTEN, DATHOIVIEC FROM HR.TB_NHANVIEN WHERE MANV = :p0 AND ROWNUM = 1",
                        new OracleParameter("p0", req.EmployeeId)
                    ).FirstOrDefault();

                    if (emp == null)
                    {
                        return BadRequest($"Không tìm thấy hồ sơ nhân viên #{req.EmployeeId}.");
                    }

                    // 2. Kiểm tra Case 2 (Quy tắc 55): Nhân viên này đã được liên kết với tài khoản User khác chưa?
                    var existingEmpMapping = db.Database.SqlQuery<decimal?>(
                        "SELECT USER_ID FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE EMPLOYEE_ID = :p0 AND USER_ID <> :p1 AND ROWNUM = 1",
                        new OracleParameter("p0", req.EmployeeId),
                        new OracleParameter("p1", id)
                    ).FirstOrDefault();

                    if (existingEmpMapping.HasValue && existingEmpMapping.Value > 0)
                    {
                        var otherUser = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == existingEmpMapping.Value);
                        string otherName = otherUser != null ? otherUser.USERNAME : existingEmpMapping.Value.ToString();
                        return BadRequest($"Nhân viên [{emp.HOTEN}] đã được liên kết với tài khoản [{otherName}]. Một nhân viên chỉ được liên kết với 1 tài khoản.");
                    }

                    // 3. Thực hiện liên kết an toàn (MERGE vào TB_USER_EMPLOYEE_MAPPING)
                    int mobileFlag = (req.IsMobileEnabled.HasValue && !req.IsMobileEnabled.Value) ? 0 : 1;

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
                        new OracleParameter("p0", id),
                        new OracleParameter("p1", req.EmployeeId),
                        new OracleParameter("p2", mobileFlag)
                    );

                    // 4. Đồng bộ backward-compatibility sang TB_SYS_USER
                    user.MANV = req.EmployeeId;
                    user.CLIENT_TYPE = mobileFlag == 1 ? "ALL" : "DESKTOP";
                    db.SaveChanges();

                    WriteAuditLog(db, "LINK_EMPLOYEE", user.IDUSER.ToString(),
                        $"Đã liên kết tài khoản [{user.USERNAME}] với nhân viên [{emp.EMPLOYEE_CODE} - {emp.HOTEN}].");

                    return Ok(new
                    {
                        success = true,
                        message = $"Đã liên kết tài khoản [{user.USERNAME}] với nhân viên [{emp.EMPLOYEE_CODE} - {emp.HOTEN}] thành công.",
                        manv = req.EmployeeId,
                        employeeCode = emp.EMPLOYEE_CODE,
                        employeeName = emp.HOTEN,
                        isMobileEnabled = (mobileFlag == 1)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi liên kết nhân viên: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi liên kết tài khoản với nhân viên." });
            }
        }

        /// <summary>
        /// POST: api/users/{id}/unlink-employee
        /// Hủy liên kết giữa User Account và Employee Profile
        /// </summary>
        [HttpPost]
        [Route("{id:int}/unlink-employee")]
        [Route("~/api/user/{id:int}/unlink-employee")]
        public IHttpActionResult UnlinkEmployee(int id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == id);
                    if (user == null) return NotFound();

                    db.Database.ExecuteSqlCommand(
                        "DELETE FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                        new OracleParameter("p0", id)
                    );

                    user.MANV = null;
                    db.SaveChanges();

                    WriteAuditLog(db, "UNLINK_EMPLOYEE", user.IDUSER.ToString(),
                        $"Đã hủy liên kết nhân viên cho tài khoản [{user.USERNAME}].");

                    return Ok(new
                    {
                        success = true,
                        message = $"Đã hủy liên kết nhân viên cho tài khoản [{user.USERNAME}] thành công."
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi hủy liên kết nhân viên: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi hủy liên kết." });
            }
        }

        /// <summary>
        /// POST: api/users/{id}/toggle-mobile
        /// Bật/tắt quyền truy cập ứng dụng di động (Mobile Access)
        /// </summary>
        [HttpPost]
        [Route("{id:int}/toggle-mobile")]
        [Route("~/api/user/{id:int}/toggle-mobile")]
        public IHttpActionResult ToggleMobile(int id, [FromBody] ToggleMobileRequest req)
        {
            try
            {
                bool targetState = req != null && req.IsMobileEnabled;

                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == id);
                    if (user == null) return NotFound();

                    if (user.USERNAME != null && user.USERNAME.Trim().ToUpper() == "ADMIN")
                    {
                        return BadRequest("Tài khoản Quản trị viên tối cao (ADMIN) là tài khoản quản trị hệ thống, không áp dụng quyền truy cập ứng dụng di động.");
                    }

                    int flagVal = targetState ? 1 : 0;

                    // Cập nhật TB_USER_EMPLOYEE_MAPPING nếu đã có bản ghi
                    db.Database.ExecuteSqlCommand(
                        "UPDATE HR.TB_USER_EMPLOYEE_MAPPING SET IS_MOBILE_ENABLED = :p0, UPDATED_AT = SYSDATE WHERE USER_ID = :p1",
                        new OracleParameter("p0", flagVal),
                        new OracleParameter("p1", id)
                    );

                    // Cập nhật CLIENT_TYPE trên TB_SYS_USER
                    user.CLIENT_TYPE = targetState ? "ALL" : "DESKTOP";
                    db.SaveChanges();

                    WriteAuditLog(db, targetState ? "ENABLE_MOBILE" : "DISABLE_MOBILE", user.IDUSER.ToString(),
                        targetState ? $"Đã kích hoạt Mobile Access cho tài khoản [{user.USERNAME}]." : $"Đã vô hiệu hóa Mobile Access cho tài khoản [{user.USERNAME}].");

                    return Ok(new
                    {
                        success = true,
                        isMobileEnabled = targetState,
                        message = targetState ? $"Đã kích hoạt Mobile Access cho tài khoản [{user.USERNAME}]." : $"Đã vô hiệu hóa Mobile Access cho tài khoản [{user.USERNAME}]."
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi chuyển đổi quyền Mobile: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi cập nhật quyền Mobile." });
            }
        }

        private void WriteAuditLog(MyEntities db, string action, string recordId, string description, string changedFields = null)
        {
            try
            {
                var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                decimal? userId = null;
                string username = "ADMIN";
                if (jwtUser != null)
                {
                    if (decimal.TryParse(jwtUser.UserId, out decimal uid)) userId = uid;
                    if (!string.IsNullOrWhiteSpace(jwtUser.Username)) username = jwtUser.Username;
                }

                string sql = @"INSERT INTO HR.TB_SYS_LOG 
                              (MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI, DU_LIEU_MOI, IP_ADDRESS, TEN_MAY_TINH, THOIGIAN, MODULE_NAME, CHANGED_FIELDS) 
                              VALUES (:p0, :p1, :p2, 'TB_SYS_USER', :p3, :p4, '127.0.0.1', 'WEB_SERVER', SYSDATE, 'SYSTEM', :p5)";

                db.Database.ExecuteSqlCommand(sql,
                    new OracleParameter("p0", userId.HasValue ? (object)userId.Value : DBNull.Value),
                    new OracleParameter("p1", username),
                    new OracleParameter("p2", action.ToUpper()),
                    new OracleParameter("p3", recordId ?? (object)DBNull.Value),
                    new OracleParameter("p4", description ?? (object)DBNull.Value),
                    new OracleParameter("p5", changedFields ?? (object)DBNull.Value));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Không thể ghi audit log: " + ex.Message);
            }
        }

        /// <summary>
        /// GET: api/users/stats
        /// Thống kê chỉ số tài khoản và phân quyền phục vụ Dashboard Cards
        /// </summary>
        [HttpGet]
        [Route("stats")]
        [Route("~/api/user/stats")]
        public IHttpActionResult GetUserStats()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    int totalEmployees = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM HR.TB_NHANVIEN WHERE (DATHOIVIEC IS NULL OR DATHOIVIEC = 0) AND DELETED_BY IS NULL"
                    ).FirstOrDefault();

                    int accountsCreated = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM HR.TB_SYS_USER WHERE (ISGROUP IS NULL OR ISGROUP = 0)"
                    ).FirstOrDefault();

                    int employeesWithoutAccount = db.Database.SqlQuery<int>(@"
                        SELECT COUNT(*) FROM HR.TB_NHANVIEN NV
                        WHERE (NV.DATHOIVIEC IS NULL OR NV.DATHOIVIEC = 0)
                          AND NV.DELETED_BY IS NULL
                          AND NOT EXISTS (
                              SELECT 1 FROM HR.TB_USER_EMPLOYEE_MAPPING M WHERE M.EMPLOYEE_ID = NV.MANV
                          )
                          AND NOT EXISTS (
                              SELECT 1 FROM HR.TB_SYS_USER U WHERE U.MANV = NV.MANV AND (U.ISGROUP IS NULL OR U.ISGROUP = 0)
                          )"
                    ).FirstOrDefault();

                    int mobileEnabled = db.Database.SqlQuery<int>(@"
                        SELECT COUNT(*) FROM HR.TB_SYS_USER U
                        LEFT JOIN HR.TB_USER_EMPLOYEE_MAPPING M ON U.IDUSER = M.USER_ID
                        WHERE (U.ISGROUP IS NULL OR U.ISGROUP = 0)
                          AND UPPER(TRIM(U.USERNAME)) <> 'ADMIN'
                          AND (M.IS_MOBILE_ENABLED = 1 OR (M.IS_MOBILE_ENABLED IS NULL AND UPPER(NVL(U.CLIENT_TYPE, 'ALL')) <> 'DESKTOP'))"
                    ).FirstOrDefault();

                    int mobileDisabled = db.Database.SqlQuery<int>(@"
                        SELECT COUNT(*) FROM HR.TB_SYS_USER U
                        LEFT JOIN HR.TB_USER_EMPLOYEE_MAPPING M ON U.IDUSER = M.USER_ID
                        WHERE (U.ISGROUP IS NULL OR U.ISGROUP = 0)
                          AND UPPER(TRIM(U.USERNAME)) <> 'ADMIN'
                          AND (M.IS_MOBILE_ENABLED = 0 OR (M.IS_MOBILE_ENABLED IS NULL AND UPPER(NVL(U.CLIENT_TYPE, 'ALL')) = 'DESKTOP'))"
                    ).FirstOrDefault();

                    int lockedAccounts = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM HR.TB_SYS_USER WHERE (ISGROUP IS NULL OR ISGROUP = 0) AND DISABLED = 1"
                    ).FirstOrDefault();

                    int systemAccounts = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM HR.TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' OR (ISGROUP = 1)"
                    ).FirstOrDefault();

                    return Ok(new UserStatsDto
                    {
                        TotalEmployees = totalEmployees,
                        AccountsCreated = accountsCreated,
                        EmployeesWithoutAccount = employeesWithoutAccount,
                        MobileEnabled = mobileEnabled,
                        MobileDisabled = mobileDisabled,
                        LockedAccounts = lockedAccounts,
                        SystemAccounts = systemAccounts
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải thống kê người dùng: " + ex.ToString());
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải thống kê người dùng." });
            }
        }

        /// <summary>
        /// POST: api/users/bulk-provision-preview
        /// Xem trước danh sách và số lượng ứng viên đủ/không đủ điều kiện cấp tài khoản
        /// </summary>
        [HttpPost]
        [Route("bulk-provision-preview")]
        [Route("~/api/user/bulk-provision-preview")]
        public IHttpActionResult BulkProvisionPreview([FromBody] BulkProvisionPreviewRequest req)
        {
            try
            {
                req = req ?? new BulkProvisionPreviewRequest();

                using (var db = new MyEntities())
                {
                    string sql = @"
                        SELECT NV.MANV AS EmployeeId,
                               NV.EMPLOYEE_CODE AS EmployeeCode,
                               NV.HOTEN AS FullName,
                               NV.IDPB AS DepartmentId,
                               PB.TENPB AS DepartmentName,
                               CV.TENCV AS PositionName,
                               NV.DATHOIVIEC AS DaThoiViec,
                               U.IDUSER AS UserId,
                               U.USERNAME AS Username,
                               U.DISABLED AS Disabled,
                               M.IS_MOBILE_ENABLED AS IsMobileEnabled,
                               U.CLIENT_TYPE AS ClientType,
                               CASE WHEN UPPER(TRIM(U.USERNAME)) = 'ADMIN' THEN 1 ELSE 0 END AS IsAdmin
                        FROM HR.TB_NHANVIEN NV
                        LEFT JOIN HR.TB_PHONGBAN PB ON NV.IDPB = PB.IDPB
                        LEFT JOIN HR.TB_CHUCVU CV ON NV.IDCV = CV.IDCV
                        LEFT JOIN HR.TB_USER_EMPLOYEE_MAPPING M ON NV.MANV = M.EMPLOYEE_ID
                        LEFT JOIN HR.TB_SYS_USER U ON (M.USER_ID = U.IDUSER OR (NV.MANV = U.MANV AND (U.ISGROUP IS NULL OR U.ISGROUP = 0)))
                        WHERE NV.DELETED_BY IS NULL";

                    if (req.DepartmentId.HasValue && req.DepartmentId.Value > 0)
                    {
                        sql += $" AND NV.IDPB = {req.DepartmentId.Value}";
                    }

                    var rawList = db.Database.SqlQuery<UserDirectoryRow>(sql).ToList();

                    int totalEligible = 0;
                    int alreadyHaveAccount = 0;
                    int alreadyEnabled = 0;
                    int systemAdmin = 0;
                    int inactive = 0;
                    int readyToCreate = 0;

                    var candidates = new List<BulkProvisionCandidateDto>();

                    foreach (var item in rawList)
                    {
                        bool isInactive = (item.DaThoiViec ?? 0) == 1;
                        bool hasAccount = item.UserId.HasValue && item.UserId.Value > 0;
                        bool isMobileOn = hasAccount && ((item.IsMobileEnabled ?? 1) == 1 && (item.ClientType ?? "ALL").ToUpper() != "DESKTOP");
                        bool isAdmin = item.IsAdmin;

                        if (isInactive) inactive++;
                        if (hasAccount) alreadyHaveAccount++;
                        if (isMobileOn) alreadyEnabled++;
                        if (isAdmin) systemAdmin++;

                        bool isEligible = true;
                        string reason = "Sẵn sàng cấp tài khoản Mobile";

                        if (isInactive)
                        {
                            isEligible = false;
                            reason = "Nhân viên đã thôi việc";
                        }
                        else if (isAdmin)
                        {
                            isEligible = false;
                            reason = "Tài khoản Quản trị hệ thống (ADMIN), không áp dụng";
                        }
                        else if (hasAccount && !req.OnlyMobileDisabled)
                        {
                            if (isMobileOn)
                            {
                                isEligible = false;
                                reason = "Đã có tài khoản và đã bật Mobile";
                            }
                            else
                            {
                                reason = "Đã có tài khoản (chưa bật Mobile)";
                            }
                        }

                        if (isEligible && !hasAccount)
                        {
                            readyToCreate++;
                            totalEligible++;
                        }
                        else if (isEligible && hasAccount)
                        {
                            totalEligible++;
                        }

                        if (req.OnlyWithoutAccount && hasAccount)
                        {
                            continue;
                        }
                        if (req.OnlyMobileDisabled && isMobileOn)
                        {
                            continue;
                        }

                        string suggestedLogin = $"NV{((long)item.EmployeeId):D6}";

                        candidates.Add(new BulkProvisionCandidateDto
                        {
                            EmployeeId = item.EmployeeId,
                            EmployeeCode = item.EmployeeCode,
                            FullName = item.FullName,
                            DepartmentId = item.DepartmentId,
                            DepartmentName = item.DepartmentName ?? "Chưa phân bổ",
                            PositionName = item.PositionName ?? "Nhân viên",
                            ExistingAccount = item.Username,
                            HasAccount = hasAccount,
                            MobileStatus = isAdmin ? "BLOCKED" : (isMobileOn ? "ENABLED" : "DISABLED"),
                            SuggestedLoginName = suggestedLogin,
                            IsEligible = isEligible,
                            Reason = reason
                        });
                    }

                    return Ok(new BulkProvisionPreviewResponse
                    {
                        TotalEligible = totalEligible,
                        AlreadyHaveAccount = alreadyHaveAccount,
                        AlreadyEnabled = alreadyEnabled,
                        SystemAdmin = systemAdmin,
                        Inactive = inactive,
                        ReadyToCreate = readyToCreate,
                        Candidates = candidates
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi xem trước cấp phát: " + ex.ToString());
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi xem trước cấp phát tài khoản." });
            }
        }

        /// <summary>
        /// POST: api/users/bulk-provision
        /// Cấp phát tài khoản nhân viên hàng loạt với Idempotency, BCrypt hash và Audit Log
        /// </summary>
        [HttpPost]
        [Route("bulk-provision")]
        [Route("~/api/user/bulk-provision")]
        public IHttpActionResult BulkProvision([FromBody] BulkProvisionRequest req)
        {
            try
            {
                if (req == null || req.EmployeeIds == null || req.EmployeeIds.Count == 0)
                {
                    return BadRequest("Danh sách nhân viên (EmployeeIds) không được để trống.");
                }

                string defaultPassword = string.IsNullOrWhiteSpace(req.DefaultPassword) ? "123456" : req.DefaultPassword.Trim();
                string hashedPassword = PasswordHasher.HashPassword(defaultPassword);

                var results = new List<BulkProvisionItemResult>();
                int successCount = 0;
                int alreadyExistsCount = 0;
                int skippedCount = 0;
                int failedCount = 0;

                using (var db = new MyEntities())
                {
                    var empIds = req.EmployeeIds.Distinct().ToList();
                    var employees = db.Database.SqlQuery<EmpCheckRow>(
                        "SELECT MANV, EMPLOYEE_CODE, HOTEN, DATHOIVIEC FROM HR.TB_NHANVIEN WHERE DELETED_BY IS NULL"
                    ).ToList();

                    var existingMappings = db.Database.SqlQuery<UserMappingRow>(@"
                        SELECT M.USER_ID, M.EMPLOYEE_ID, M.IS_MOBILE_ENABLED, NV.EMPLOYEE_CODE, NV.HOTEN AS EMPLOYEE_NAME 
                        FROM HR.TB_USER_EMPLOYEE_MAPPING M
                        LEFT JOIN HR.TB_NHANVIEN NV ON M.EMPLOYEE_ID = NV.MANV"
                    ).ToList();

                    var existingUsernames = new HashSet<string>(
                        db.Database.SqlQuery<string>("SELECT USERNAME FROM HR.TB_SYS_USER WHERE USERNAME IS NOT NULL").ToList()
                            .Select(u => (u ?? "").Trim().ToUpper()),
                        StringComparer.OrdinalIgnoreCase
                    );

                    foreach (var empId in empIds)
                    {
                        var emp = employees.FirstOrDefault(e => e.MANV == empId);
                        if (emp == null)
                        {
                            failedCount++;
                            results.Add(new BulkProvisionItemResult
                            {
                                EmployeeId = empId,
                                EmployeeCode = $"NV#{empId}",
                                FullName = "Không xác định",
                                LoginName = null,
                                Result = "FAILED",
                                Message = $"Không tìm thấy hồ sơ nhân sự #{empId}."
                            });
                            continue;
                        }

                        if ((emp.DATHOIVIEC ?? 0) == 1)
                        {
                            skippedCount++;
                            results.Add(new BulkProvisionItemResult
                            {
                                EmployeeId = emp.MANV,
                                EmployeeCode = emp.EMPLOYEE_CODE,
                                FullName = emp.HOTEN,
                                LoginName = null,
                                Result = "SKIPPED_INACTIVE",
                                Message = "Nhân viên đã thôi việc, bỏ qua theo quy định an toàn."
                            });
                            continue;
                        }

                        var mapping = existingMappings.FirstOrDefault(m => m.EMPLOYEE_ID == emp.MANV);
                        if (mapping != null && !req.OverwriteExisting)
                        {
                            var existingUser = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == mapping.USER_ID);
                            string exName = existingUser != null ? existingUser.USERNAME : mapping.USER_ID.ToString();

                            if (req.EnableMobile && (mapping.IS_MOBILE_ENABLED ?? 0) == 0)
                            {
                                db.Database.ExecuteSqlCommand(
                                    "UPDATE HR.TB_USER_EMPLOYEE_MAPPING SET IS_MOBILE_ENABLED = 1, UPDATED_AT = SYSDATE WHERE USER_ID = :p0",
                                    new OracleParameter("p0", mapping.USER_ID)
                                );
                                if (existingUser != null)
                                {
                                    existingUser.CLIENT_TYPE = "ALL";
                                    db.SaveChanges();
                                }
                            }

                            alreadyExistsCount++;
                            results.Add(new BulkProvisionItemResult
                            {
                                EmployeeId = emp.MANV,
                                EmployeeCode = emp.EMPLOYEE_CODE,
                                FullName = emp.HOTEN,
                                LoginName = exName,
                                Result = "ALREADY_EXISTS",
                                Message = $"Đã có tài khoản [{exName}] (Idempotent: giữ nguyên tài khoản)."
                            });
                            continue;
                        }

                        string baseLoginName = $"NV{((long)emp.MANV):D6}";
                        string finalLoginName = baseLoginName;
                        int suffix = 1;
                        while (existingUsernames.Contains(finalLoginName))
                        {
                            finalLoginName = $"{baseLoginName}_{suffix++}";
                        }
                        existingUsernames.Add(finalLoginName);

                        try
                        {
                            var newUser = new TB_SYS_USER
                            {
                                USERNAME = finalLoginName,
                                FULLNAME = emp.HOTEN,
                                PASSWORD = hashedPassword,
                                MANV = emp.MANV,
                                DISABLED = 0,
                                CLIENT_TYPE = req.EnableMobile ? "ALL" : "DESKTOP",
                                ISGROUP = 0,
                                MACTY = "1",
                                MADVI = "1"
                            };

                            db.TB_SYS_USER.Add(newUser);
                            db.SaveChanges();

                            int mobileFlag = req.EnableMobile ? 1 : 0;
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
                                new OracleParameter("p0", newUser.IDUSER),
                                new OracleParameter("p1", emp.MANV),
                                new OracleParameter("p2", mobileFlag)
                            );

                            successCount++;
                            results.Add(new BulkProvisionItemResult
                            {
                                EmployeeId = emp.MANV,
                                EmployeeCode = emp.EMPLOYEE_CODE,
                                FullName = emp.HOTEN,
                                LoginName = finalLoginName,
                                Result = "SUCCESS",
                                Message = req.EnableMobile ? "Cấp tài khoản & kích hoạt Mobile thành công" : "Cấp tài khoản thành công (chưa kích hoạt Mobile)"
                            });
                        }
                        catch (Exception itemEx)
                        {
                            failedCount++;
                            results.Add(new BulkProvisionItemResult
                            {
                                EmployeeId = emp.MANV,
                                EmployeeCode = emp.EMPLOYEE_CODE,
                                FullName = emp.HOTEN,
                                LoginName = finalLoginName,
                                Result = "FAILED",
                                Message = "Lỗi khi lưu tài khoản: " + itemEx.Message
                            });
                        }
                    }

                    WriteAuditLog(
                        db,
                        "BULK_PROVISION",
                        $"BATCH_{DateTime.Now:yyyyMMddHHmmss}",
                        $"Cấp phát tài khoản hàng loạt: Tổng {empIds.Count}, Thành công {successCount}, Đã có {alreadyExistsCount}, Bỏ qua {skippedCount}, Lỗi {failedCount}",
                        Newtonsoft.Json.JsonConvert.SerializeObject(new { total = empIds.Count, success = successCount, alreadyExists = alreadyExistsCount, failed = failedCount })
                    );

                    return Ok(new BulkProvisionResponse
                    {
                        Total = empIds.Count,
                        Success = successCount,
                        AlreadyExists = alreadyExistsCount,
                        Skipped = skippedCount,
                        Failed = failedCount,
                        Results = results
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi thực thi cấp phát hàng loạt: " + ex.ToString());
                return Content(HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi thực thi cấp phát tài khoản." });
            }
        }

        /// <summary>
        /// GET: api/users/groups/{groupId}/members
        /// Lấy danh sách thành viên hiện tại của nhóm và danh sách người dùng chưa thuộc nhóm
        /// </summary>
        [HttpGet]
        [Route("groups/{groupId:int}/members")]
        public IHttpActionResult GetGroupMembers(int groupId)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var group = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == groupId && (u.ISGROUP ?? 0) == 1);
                    if (group == null) return NotFound();

                    var memberIds = db.TB_SYS_GROUP.Where(g => g.ID_GROUP == groupId).Select(g => g.MEMBER).ToList();
                    var allUsers = db.TB_SYS_USER.Where(u => (u.ISGROUP ?? 0) == 0).ToList();

                    var members = allUsers.Where(u => memberIds.Contains(u.IDUSER)).Select(u => new
                    {
                        IdUser = (int)u.IDUSER,
                        id = (int)u.IDUSER,
                        Username = u.USERNAME,
                        username = u.USERNAME,
                        FullName = u.FULLNAME ?? u.USERNAME,
                        fullName = u.FULLNAME ?? u.USERNAME,
                        Disabled = (u.DISABLED ?? 0) == 1
                    }).ToList();

                    var availableUsers = allUsers.Where(u => !memberIds.Contains(u.IDUSER) && (u.DISABLED ?? 0) == 0).Select(u => new
                    {
                        IdUser = (int)u.IDUSER,
                        id = (int)u.IDUSER,
                        Username = u.USERNAME,
                        username = u.USERNAME,
                        FullName = u.FULLNAME ?? u.USERNAME,
                        fullName = u.FULLNAME ?? u.USERNAME
                    }).ToList();

                    return Ok(new
                    {
                        GroupId = groupId,
                        GroupName = group.USERNAME,
                        GroupFullName = group.FULLNAME,
                        Members = members,
                        members = members,
                        AvailableUsers = availableUsers,
                        availableUsers = availableUsers
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải thành viên nhóm: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải thành viên nhóm." });
            }
        }

        /// <summary>
        /// POST: api/users/groups/{groupId}/members/{memberId}
        /// Thêm người dùng vào nhóm quyền (TB_SYS_GROUP)
        /// </summary>
        [HttpPost]
        [Route("groups/{groupId:int}/members/{memberId:int}")]
        public IHttpActionResult AddMemberToGroup(int groupId, int memberId)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    bool exists = db.TB_SYS_GROUP.Any(g => g.ID_GROUP == groupId && g.MEMBER == memberId);
                    if (!exists)
                    {
                        db.TB_SYS_GROUP.Add(new TB_SYS_GROUP
                        {
                            ID_GROUP = groupId,
                            MEMBER = memberId
                        });
                        db.SaveChanges();
                    }

                    return Ok(new { success = true, message = "Đã thêm thành viên vào nhóm thành công." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi thêm thành viên vào nhóm: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi thêm thành viên vào nhóm." });
            }
        }

        /// <summary>
        /// DELETE: api/users/groups/{groupId}/members/{memberId}
        /// Gỡ người dùng khỏi nhóm quyền (TB_SYS_GROUP)
        /// </summary>
        [HttpDelete]
        [Route("groups/{groupId:int}/members/{memberId:int}")]
        public IHttpActionResult RemoveMemberFromGroup(int groupId, int memberId)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var record = db.TB_SYS_GROUP.FirstOrDefault(g => g.ID_GROUP == groupId && g.MEMBER == memberId);
                    if (record != null)
                    {
                        db.TB_SYS_GROUP.Remove(record);
                        db.SaveChanges();
                    }

                    return Ok(new { success = true, message = "Đã gỡ thành viên khỏi nhóm thành công." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi gỡ thành viên khỏi nhóm: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi gỡ thành viên khỏi nhóm." });
            }
        }

        /// <summary>
        /// GET: api/users/{id}/rights
        /// Lấy toàn bộ danh sách chức năng hệ thống kèm cờ HAS_RIGHT cho user/group tương ứng
        /// </summary>
        [HttpGet]
        [Route("{id:int}/rights")]
        [Route("~/api/user/{id:int}/rights")]
        public IHttpActionResult GetUserRights(int id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var allFunctions = db.TB_SYS_FUNCTION.OrderBy(f => f.SORT).ToList();

                    // Lấy tất cả quyền gán trực tiếp của user
                    var userRights = db.TB_SYS_RIGHT
                        .Where(r => r.IDUSER == id)
                        .ToList();
                    var rightsDict = userRights
                        .GroupBy(r => r.FUNCTION_CODE)
                        .ToDictionary(g => g.Key, g => g.First());

                    var result = allFunctions.Select(f =>
                    {
                        rightsDict.TryGetValue(f.FUNCTION_CODE, out var r);
                        bool canView = (r != null && (r.CAN_VIEW == 1 || r.USER_RIGHT == 1));
                        bool canAdd = (r != null && r.CAN_ADD == 1);
                        bool canEdit = (r != null && r.CAN_EDIT == 1);
                        bool canDelete = (r != null && r.CAN_DELETE == 1);
                        bool canPrint = (r != null && r.CAN_PRINT == 1);

                        return new
                        {
                            FUNCTION_CODE = f.FUNCTION_CODE,
                            FuncCode = f.FUNCTION_CODE,
                            funcCode = f.FUNCTION_CODE,
                            DESCRIPTION = f.DESCRIPTION,
                            Description = f.DESCRIPTION,
                            description = f.DESCRIPTION,
                            PARENT = f.PARENT,
                            Parent = f.PARENT,
                            parent = f.PARENT,
                            SORT = f.SORT,
                            HAS_RIGHT = canView,
                            HasRight = canView,
                            hasRight = canView,
                            CAN_VIEW = canView,
                            CanView = canView,
                            canView = canView,
                            CAN_ADD = canAdd,
                            CanAdd = canAdd,
                            canAdd = canAdd,
                            CAN_EDIT = canEdit,
                            CanEdit = canEdit,
                            canEdit = canEdit,
                            CAN_DELETE = canDelete,
                            CanDelete = canDelete,
                            canDelete = canDelete,
                            CAN_PRINT = canPrint,
                            CanPrint = canPrint,
                            canPrint = canPrint
                        };
                    }).ToList();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi lấy quyền người dùng: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi lấy quyền người dùng." });
            }
        }

        /// <summary>
        /// POST: api/users/{id}/rights
        /// Cập nhật quyền chức năng cho user/group (Tương ứng FrmPhanQuyenChucNang trong WinForms)
        /// </summary>
        [HttpPost]
        [Route("{id:int}/rights")]
        [Route("~/api/user/{id:int}/rights")]
        public IHttpActionResult SaveUserRights(int id, [FromBody] SaveRightsRequest req)
        {
            try
            {
                var codes = req != null ? req.GetEffectiveCodes() : new List<string>();
                if (codes == null && (req == null || req.Details == null))
                {
                    return BadRequest("Danh sách quyền không hợp lệ.");
                }

                using (var db = new MyEntities())
                {
                    // Lấy tất cả các quyền hiện có của user
                    var existing = db.TB_SYS_RIGHT.Where(r => r.IDUSER == id).ToList();
                    db.TB_SYS_RIGHT.RemoveRange(existing);
                    db.SaveChanges();

                    if (req != null && req.Details != null && req.Details.Count > 0)
                    {
                        foreach (var d in req.Details)
                        {
                            if (string.IsNullOrEmpty(d.FunctionCode)) continue;
                            bool canView = d.CanView ?? false;
                            bool canAdd = d.CanAdd ?? false;
                            bool canEdit = d.CanEdit ?? false;
                            bool canDelete = d.CanDelete ?? false;
                            bool canPrint = d.CanPrint ?? false;

                            db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT
                            {
                                IDUSER = id,
                                FUNCTION_CODE = d.FunctionCode,
                                USER_RIGHT = canView ? 1 : 0,
                                CAN_VIEW = canView ? 1 : 0,
                                CAN_ADD = canAdd ? 1 : 0,
                                CAN_EDIT = canEdit ? 1 : 0,
                                CAN_DELETE = canDelete ? 1 : 0,
                                CAN_PRINT = canPrint ? 1 : 0
                            });
                        }
                    }
                    else
                    {
                        // Thêm các quyền mới (backward compatible: codes được cấp đủ 5 quyền)
                        foreach (var code in codes.Distinct())
                        {
                            db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT
                            {
                                IDUSER = id,
                                FUNCTION_CODE = code,
                                USER_RIGHT = 1,
                                CAN_VIEW = 1,
                                CAN_ADD = 1,
                                CAN_EDIT = 1,
                                CAN_DELETE = 1,
                                CAN_PRINT = 1
                            });
                        }
                    }
                    db.SaveChanges();

                    int count = (req != null && req.Details != null && req.Details.Count > 0) ? req.Details.Count : codes.Count;
                    return Ok(new { success = true, message = $"Đã cập nhật phân quyền thành công cho tài khoản/nhóm #{id} ({count} quyền)." });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi lưu phân quyền: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi cập nhật phân quyền." });
            }
        }

        /// <summary>
        /// GET: api/users/functions
        /// Danh mục toàn bộ chức năng hệ thống
        /// </summary>
        [HttpGet]
        [Route("functions")]
        public IHttpActionResult GetFunctions()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var functions = db.TB_SYS_FUNCTION.OrderBy(f => f.SORT).ToList();
                    return Ok(functions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi tải danh mục chức năng: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi tải danh mục chức năng." });
            }
        }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public bool IsGroup { get; set; }
    }

    public class UpdateUserRequest
    {
        public string FullName { get; set; }
        public bool? Disabled { get; set; }
        public string NewPassword { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; }
    }

    public class RightDetailItem
    {
        public string FunctionCode { get; set; }
        public bool? CanView { get; set; }
        public bool? CanAdd { get; set; }
        public bool? CanEdit { get; set; }
        public bool? CanDelete { get; set; }
        public bool? CanPrint { get; set; }
    }

    public class SaveRightsRequest
    {
        public List<string> FunctionCodes { get; set; }
        public List<string> FuncCodes { get; set; }
        public List<RightDetailItem> Details { get; set; }

        public List<string> GetEffectiveCodes()
        {
            return FunctionCodes ?? FuncCodes ?? new List<string>();
        }
    }

    public class UserMappingRow
    {
        public decimal USER_ID { get; set; }
        public decimal? EMPLOYEE_ID { get; set; }
        public decimal? IS_MOBILE_ENABLED { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public string EMPLOYEE_NAME { get; set; }
    }

    public class EmpCheckRow
    {
        public decimal MANV { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public string HOTEN { get; set; }
        public decimal? DATHOIVIEC { get; set; }
    }

    public class LinkEmployeeRequest
    {
        public decimal EmployeeId { get; set; }
        public bool? IsMobileEnabled { get; set; }
    }

    public class ToggleMobileRequest
    {
        public bool IsMobileEnabled { get; set; }
    }

    public class UserStatsDto
    {
        public int TotalEmployees { get; set; }
        public int AccountsCreated { get; set; }
        public int EmployeesWithoutAccount { get; set; }
        public int MobileEnabled { get; set; }
        public int MobileDisabled { get; set; }
        public int LockedAccounts { get; set; }
        public int SystemAccounts { get; set; }
    }

    public class BulkProvisionPreviewRequest
    {
        public int? DepartmentId { get; set; }
        public bool OnlyWithoutAccount { get; set; } = true;
        public bool OnlyMobileDisabled { get; set; } = false;
        public string Status { get; set; } = "ACTIVE";
    }

    public class BulkProvisionCandidateDto
    {
        public decimal EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public string ExistingAccount { get; set; }
        public bool HasAccount { get; set; }
        public string MobileStatus { get; set; }
        public string SuggestedLoginName { get; set; }
        public bool IsEligible { get; set; }
        public string Reason { get; set; }
    }

    public class BulkProvisionPreviewResponse
    {
        public int TotalEligible { get; set; }
        public int AlreadyHaveAccount { get; set; }
        public int AlreadyEnabled { get; set; }
        public int SystemAdmin { get; set; }
        public int Inactive { get; set; }
        public int ReadyToCreate { get; set; }
        public List<BulkProvisionCandidateDto> Candidates { get; set; }
    }

    public class BulkProvisionRequest
    {
        public List<decimal> EmployeeIds { get; set; }
        public string DefaultPassword { get; set; }
        public bool EnableMobile { get; set; } = true;
        public bool OverwriteExisting { get; set; } = false;
    }

    public class BulkProvisionItemResult
    {
        public decimal EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string LoginName { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
    }

    public class BulkProvisionResponse
    {
        public int Total { get; set; }
        public int Success { get; set; }
        public int AlreadyExists { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        public List<BulkProvisionItemResult> Results { get; set; }
    }

    public class UserDirectoryRow
    {
        public decimal EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public decimal? DaThoiViec { get; set; }
        public decimal? UserId { get; set; }
        public string Username { get; set; }
        public decimal? Disabled { get; set; }
        public decimal? IsMobileEnabled { get; set; }
        public string ClientType { get; set; }
        public bool IsAdmin { get; set; }
    }
}
