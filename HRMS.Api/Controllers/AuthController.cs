using Bu.CLASS_SECURITY;
using Bu.CLASS_SYSTEM;
using DA;
using HRMS_API.Filters;
using HRMS_API.Services;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly SYS_USER _userBus = new SYS_USER();
        private readonly IAuthSecurityService _authSecurityService = new AuthSecurityService();

        private string GetClientIpAddress()
        {
            try
            {
                if (Request.Properties.ContainsKey("MS_HttpContext"))
                {
                    var ctx = Request.Properties["MS_HttpContext"] as System.Web.HttpContextWrapper;
                    if (ctx != null)
                    {
                        string ip = ctx.Request.Headers["X-Forwarded-For"];
                        if (!string.IsNullOrEmpty(ip))
                        {
                            return ip.Split(',')[0].Trim();
                        }
                        return ctx.Request.UserHostAddress;
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }

        private string GetHeaderValue(string headerName)
        {
            if (Request.Headers.TryGetValues(headerName, out var values))
            {
                return values.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// POST: api/auth/login
        /// Xác thực đăng nhập an toàn từ Oracle Database với kiểm soát phiên đăng nhập đồng thời (N sessions)
        /// </summary>
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Login([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest("Vui lòng nhập tên đăng nhập và mật khẩu.");
            }

            string clientIp = GetClientIpAddress();
            string userAgent = Request.Headers.UserAgent?.ToString() ?? "Unknown";
            string correlationId = GetHeaderValue("X-Correlation-Id") ?? Guid.NewGuid().ToString("N");
            string platform = !string.IsNullOrWhiteSpace(req.Platform) ? req.Platform : (GetHeaderValue("X-Platform") ?? "WEB");
            string deviceId = !string.IsNullOrWhiteSpace(req.DeviceId) ? req.DeviceId : GetHeaderValue("X-Device-Id");
            string deviceName = !string.IsNullOrWhiteSpace(req.DeviceName) ? req.DeviceName : GetHeaderValue("X-Device-Name");
            string clientType = !string.IsNullOrWhiteSpace(req.ClientType) ? req.ClientType : "ALL";

            try
            {
                var loginResult = await _authSecurityService.AuthenticateAsync(
                    req.Username,
                    req.Password,
                    clientType,
                    platform,
                    deviceId,
                    deviceName,
                    clientIp,
                    userAgent,
                    correlationId
                );

                if (!loginResult.Success)
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        success = false,
                        message = loginResult.ErrorMessage,
                        isLockedOut = loginResult.IsLockedOut,
                        lockoutMinutes = loginResult.LockoutRemainingMinutes
                    });
                }

                // Tạo Token phiên làm việc chuẩn JSON Web Token (HMAC-SHA256) chứa JTI và TokenVersion
                string token = JwtService.GenerateToken(
                    (int)loginResult.UserId,
                    loginResult.Username,
                    loginResult.FullName,
                    loginResult.IsAdmin,
                    loginResult.Rights,
                    loginResult.MaCty,
                    loginResult.MaDvi,
                    loginResult.Manv.HasValue ? loginResult.Manv.Value.ToString() : null,
                    loginResult.ClientType,
                    loginResult.Jti,
                    loginResult.TokenVersion
                );

                return Ok(new
                {
                    success = true,
                    Success = true,
                    token = token,
                    Token = token,
                    sessionId = loginResult.SessionId,
                    user = new
                    {
                        IdUser = (int)loginResult.UserId,
                        id = (int)loginResult.UserId,
                        Username = loginResult.Username,
                        username = loginResult.Username,
                        FullName = loginResult.FullName,
                        fullName = loginResult.FullName,
                        IsAdmin = loginResult.IsAdmin,
                        isAdmin = loginResult.IsAdmin,
                        Rights = loginResult.Rights,
                        rights = loginResult.Rights,
                        DetailedRights = loginResult.DetailedRights,
                        detailedRights = loginResult.DetailedRights,
                        manv = loginResult.Manv,
                        Manv = loginResult.Manv,
                        employeeCode = loginResult.EmployeeCode,
                        EmployeeCode = loginResult.EmployeeCode,
                        isMobileEnabled = loginResult.IsMobileEnabled,
                        IsMobileEnabled = loginResult.IsMobileEnabled,
                        clientType = loginResult.ClientType,
                        ClientType = loginResult.ClientType
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi hệ thống khi đăng nhập: " + ex.ToString());
                string detail = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Lỗi kết nối CSDL hoặc máy chủ: " + detail,
                    detail = detail
                });
            }
        }

        /// <summary>
        /// GET: api/auth/me
        /// Kiểm tra tính hợp lệ của phiên đăng nhập và làm mới thông tin quyền hạn trực tiếp từ CSDL
        /// </summary>
        [HttpGet]
        [Route("me")]
        public IHttpActionResult GetCurrentUser()
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader == null || string.IsNullOrWhiteSpace(authHeader.Parameter))
            {
                return Unauthorized();
            }

            string token = authHeader.Parameter;
            if (!JwtService.ValidateToken(token, out var jwtClaims, out _))
            {
                return Unauthorized();
            }

            if (!decimal.TryParse(jwtClaims.UserId, out decimal currentUserId) || currentUserId <= 0)
            {
                return Unauthorized();
            }

            // Kiểm tra trạng thái Session trong DB (Thu hồi lập tức / Security Version / Lockout)
            if (!_authSecurityService.ValidateSession(jwtClaims.Jti, jwtClaims.TokenVersion, currentUserId))
            {
                return Content(HttpStatusCode.Unauthorized, new
                {
                    success = false,
                    code = "SESSION_REVOKED_OR_EXPIRED",
                    message = "Phiên đăng nhập đã bị thu hồi hoặc tài khoản đã thay đổi bảo mật."
                });
            }

            // Làm mới thông tin và quyền hạn trực tiếp từ CSDL Oracle
            try
            {
                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == currentUserId);
                    if (user == null || (user.DISABLED ?? 0) == 1)
                    {
                        return Unauthorized();
                    }

                    bool isAdmin = user.USERNAME.Trim().ToUpper() == "ADMIN";
                    List<string> freshRights = new List<string>();

                    if (isAdmin)
                    {
                        freshRights.Add("*");
                        freshRights.AddRange(db.TB_SYS_FUNCTION.Select(f => f.FUNCTION_CODE).ToList());
                    }
                    else
                    {
                        // 1. Direct rights
                        var direct = db.TB_SYS_RIGHT.Where(r => r.IDUSER == user.IDUSER && (r.CAN_VIEW == 1 || r.USER_RIGHT == 1)).Select(r => r.FUNCTION_CODE).ToList();
                        freshRights.AddRange(direct);

                        // 2. Group rights
                        var groupIds = db.TB_SYS_GROUP.Where(g => g.MEMBER == user.IDUSER).Select(g => g.ID_GROUP).ToList();
                        if (groupIds.Any())
                        {
                            var groupRights = db.TB_SYS_RIGHT.Where(r => groupIds.Contains(r.IDUSER) && (r.CAN_VIEW == 1 || r.USER_RIGHT == 1)).Select(r => r.FUNCTION_CODE).ToList();
                            freshRights.AddRange(groupRights);
                        }

                        if (!freshRights.Contains("F_DB_NHANSU")) freshRights.Add("F_DB_NHANSU");
                        if (!freshRights.Contains("F_SYSTEM_AI")) freshRights.Add("F_SYSTEM_AI");
                    }

                    freshRights = freshRights.Distinct().ToList();

                    // Lấy chi tiết 5 quyền (Xem, Thêm, Sửa, Xóa, In)
                    var freshDetailedRights = _userBus.GetDetailedRights(user.IDUSER);
                    if (isAdmin)
                    {
                        foreach (var key in freshDetailedRights.Keys.ToList())
                        {
                            freshDetailedRights[key].CAN_VIEW = true;
                            freshDetailedRights[key].CAN_ADD = true;
                            freshDetailedRights[key].CAN_EDIT = true;
                            freshDetailedRights[key].CAN_DELETE = true;
                            freshDetailedRights[key].CAN_PRINT = true;
                        }
                    }

                    bool isRootAdmin = user.USERNAME != null && user.USERNAME.Trim().ToUpper() == "ADMIN";
                    decimal? freshManv = isRootAdmin ? null : user.MANV;
                    string freshEmpCode = null;
                    bool freshMobileEnabled = !isRootAdmin;

                    if (!isRootAdmin)
                    {
                        try
                        {
                            var mapping = db.Database.SqlQuery<MappingRow>(
                                "SELECT USER_ID, EMPLOYEE_ID, IS_MOBILE_ENABLED FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0 AND ROWNUM = 1",
                                new OracleParameter("p0", user.IDUSER)
                            ).FirstOrDefault();

                            if (mapping != null)
                            {
                                if (mapping.EMPLOYEE_ID.HasValue && mapping.EMPLOYEE_ID.Value > 0)
                                {
                                    freshManv = mapping.EMPLOYEE_ID.Value;
                                }
                                if (mapping.IS_MOBILE_ENABLED.HasValue)
                                {
                                    freshMobileEnabled = (mapping.IS_MOBILE_ENABLED.Value == 1);
                                }
                            }

                            if (freshManv.HasValue && freshManv.Value > 0)
                            {
                                freshEmpCode = db.Database.SqlQuery<string>(
                                    "SELECT EMPLOYEE_CODE FROM HR.TB_NHANVIEN WHERE MANV = :p0 AND ROWNUM = 1",
                                    new OracleParameter("p0", freshManv.Value)
                                ).FirstOrDefault();
                            }
                        }
                        catch { }
                    }

                    return Ok(new
                    {
                        user = new
                        {
                            IdUser = user.IDUSER,
                            id = user.IDUSER,
                            Username = user.USERNAME,
                            username = user.USERNAME,
                            FullName = user.FULLNAME ?? user.USERNAME,
                            fullName = user.FULLNAME ?? user.USERNAME,
                            IsAdmin = isAdmin,
                            isAdmin = isAdmin,
                            Rights = freshRights,
                            rights = freshRights,
                            DetailedRights = freshDetailedRights,
                            detailedRights = freshDetailedRights,
                            manv = freshManv,
                            Manv = freshManv,
                            employeeCode = freshEmpCode,
                            EmployeeCode = freshEmpCode,
                            isMobileEnabled = freshMobileEnabled,
                            IsMobileEnabled = freshMobileEnabled,
                            clientType = user.CLIENT_TYPE ?? "ALL",
                            ClientType = user.CLIENT_TYPE ?? "ALL"
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi GetCurrentUser: " + ex.Message);
                return Ok(new
                {
                    user = new
                    {
                        IdUser = currentUserId,
                        id = currentUserId,
                        Username = jwtClaims.Username,
                        username = jwtClaims.Username,
                        FullName = jwtClaims.FullName,
                        fullName = jwtClaims.FullName,
                        IsAdmin = jwtClaims.IsAdmin,
                        isAdmin = jwtClaims.IsAdmin,
                        Rights = jwtClaims.Rights,
                        rights = jwtClaims.Rights
                    }
                });
            }
        }

        /// <summary>
        /// POST: api/auth/change-password
        /// Đổi mật khẩu: BCrypt hash, tăng TOKEN_VERSION, thu hồi toàn bộ session đang mở
        /// </summary>
        [HttpPost]
        [Route("change-password")]
        [JwtAuthorize]
        public async Task<IHttpActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.OldPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
            {
                return BadRequest("Vui lòng nhập mật khẩu hiện tại và mật khẩu mới.");
            }

            var claims = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (claims == null || !decimal.TryParse(claims.UserId, out decimal userId))
            {
                return Unauthorized();
            }

            string clientIp = GetClientIpAddress();
            string userAgent = Request.Headers.UserAgent?.ToString() ?? "Unknown";
            string correlationId = GetHeaderValue("X-Correlation-Id") ?? Guid.NewGuid().ToString("N");

            var result = await _authSecurityService.ChangePasswordWithRevocationAsync(
                userId,
                req.OldPassword,
                req.NewPassword,
                clientIp,
                userAgent,
                correlationId
            );

            if (!result.Success)
            {
                return Content(HttpStatusCode.BadRequest, new { success = false, message = result.Message });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                sessionsRevoked = result.SessionsRevokedCount
            });
        }

        /// <summary>
        /// POST: api/auth/logout
        /// Đăng xuất khỏi thiết bị hiện tại (thu hồi session cụ thể trong DB)
        /// </summary>
        [HttpPost]
        [Route("logout")]
        public async Task<IHttpActionResult> Logout()
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && !string.IsNullOrWhiteSpace(authHeader.Parameter))
            {
                string token = authHeader.Parameter;
                if (JwtService.ValidateToken(token, out var jwtClaims, out _))
                {
                    if (decimal.TryParse(jwtClaims.UserId, out decimal userId))
                    {
                        string correlationId = GetHeaderValue("X-Correlation-Id") ?? Guid.NewGuid().ToString("N");
                        await _authSecurityService.RevokeSessionAsync(jwtClaims.Jti, AuthRevokeReasons.UserLogout, userId, correlationId);
                    }
                }
            }
            return Ok(new { success = true, message = "Đã đăng xuất thành công." });
        }

        /// <summary>
        /// POST: api/auth/logout-all
        /// Đăng xuất khỏi toàn bộ thiết bị (thu hồi tất cả sessions + tăng TOKEN_VERSION)
        /// </summary>
        [HttpPost]
        [Route("logout-all")]
        [JwtAuthorize]
        public async Task<IHttpActionResult> LogoutAll()
        {
            var claims = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (claims == null || !decimal.TryParse(claims.UserId, out decimal userId))
            {
                return Unauthorized();
            }

            string correlationId = GetHeaderValue("X-Correlation-Id") ?? Guid.NewGuid().ToString("N");
            int count = await _authSecurityService.RevokeAllSessionsAsync(userId, AuthRevokeReasons.UserLogoutAll, true, userId, correlationId);

            return Ok(new
            {
                success = true,
                revokedSessionsCount = count,
                message = "Đã đăng xuất toàn bộ thiết bị thành công."
            });
        }

        /// <summary>
        /// GET: api/auth/sessions
        /// Danh sách phiên đăng nhập của người dùng hiện tại
        /// </summary>
        [HttpGet]
        [Route("sessions")]
        [JwtAuthorize]
        public async Task<IHttpActionResult> GetSessions()
        {
            var claims = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (claims == null || !decimal.TryParse(claims.UserId, out decimal userId))
            {
                return Unauthorized();
            }

            var sessions = await _authSecurityService.GetUserSessionsAsync(userId, claims.Jti);
            return Ok(new { success = true, sessions = sessions });
        }

        /// <summary>
        /// DELETE: api/auth/sessions/{sessionId}
        /// Thu hồi một phiên đăng nhập cụ thể
        /// </summary>
        [HttpDelete]
        [Route("sessions/{sessionId}")]
        [JwtAuthorize]
        public async Task<IHttpActionResult> RevokeSession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return BadRequest("Mã phiên đăng nhập không hợp lệ.");
            }

            var claims = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (claims == null || !decimal.TryParse(claims.UserId, out decimal userId))
            {
                return Unauthorized();
            }

            string correlationId = GetHeaderValue("X-Correlation-Id") ?? Guid.NewGuid().ToString("N");
            bool ok = await _authSecurityService.RevokeSessionAsync(sessionId, AuthRevokeReasons.UserRevoked, userId, correlationId);

            if (!ok)
            {
                return BadRequest("Không thể thu hồi phiên đăng nhập này hoặc phiên đã hết hạn.");
            }

            return Ok(new { success = true, message = "Đã thu hồi phiên đăng nhập thành công." });
        }

        /// <summary>
        /// GET: api/auth/admin/users/{userId}/sessions
        /// [ADMIN] Xem danh sách phiên đăng nhập của bất kỳ user nào
        /// </summary>
        [HttpGet]
        [Route("admin/users/{userId:decimal}/sessions")]
        [JwtAuthorize(RequireAdmin = true)]
        public async Task<IHttpActionResult> GetAdminUserSessions(decimal userId)
        {
            var sessions = await _authSecurityService.GetUserSessionsAsync(userId, null);
            return Ok(new { success = true, sessions = sessions });
        }

        /// <summary>
        /// POST: api/auth/admin/users/{userId}/force-logout
        /// [ADMIN] Cưỡng chế đăng xuất toàn bộ phiên của người dùng
        /// </summary>
        [HttpPost]
        [Route("admin/users/{userId:decimal}/force-logout")]
        [JwtAuthorize(RequireAdmin = true)]
        public async Task<IHttpActionResult> AdminForceLogout(decimal userId)
        {
            var claims = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            decimal adminId = decimal.TryParse(claims?.UserId, out var aId) ? aId : 1;
            string correlationId = GetHeaderValue("X-Correlation-Id") ?? Guid.NewGuid().ToString("N");

            int count = await _authSecurityService.RevokeAllSessionsAsync(userId, AuthRevokeReasons.AdminForceLogout, true, adminId, correlationId);
            return Ok(new
            {
                success = true,
                revokedCount = count,
                message = $"Đã thu hồi toàn bộ {count} phiên đăng nhập của người dùng."
            });
        }

        /// <summary>
        /// POST: api/auth/admin/users/{userId}/unlock
        /// [ADMIN] Mở khóa tài khoản bị khóa do nhập sai nhiều lần
        /// </summary>
        [HttpPost]
        [Route("admin/users/{userId:decimal}/unlock")]
        [JwtAuthorize(RequireAdmin = true)]
        public async Task<IHttpActionResult> AdminUnlockUser(decimal userId)
        {
            var claims = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            decimal adminId = decimal.TryParse(claims?.UserId, out var aId) ? aId : 1;
            string correlationId = GetHeaderValue("X-Correlation-Id") ?? Guid.NewGuid().ToString("N");

            bool ok = await _authSecurityService.UnlockUserAsync(userId, adminId, correlationId);
            if (!ok)
            {
                return BadRequest("Không thể mở khóa tài khoản hoặc tài khoản không tồn tại.");
            }

            return Ok(new { success = true, message = "Đã mở khóa tài khoản thành công." });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientType { get; set; }
        public string Platform { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class SessionInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
        public List<string> Rights { get; set; }
        public DateTime LoginTime { get; set; }
        public decimal? Manv { get; set; }
        public string ClientType { get; set; }
    }

    public class MappingRow
    {
        public decimal? USER_ID { get; set; }
        public decimal? EMPLOYEE_ID { get; set; }
        public decimal? IS_MOBILE_ENABLED { get; set; }
    }

    public class EmpRow
    {
        public decimal MANV { get; set; }
        public string EMPLOYEE_CODE { get; set; }
        public decimal? DATHOIVIEC { get; set; }
        public string HOTEN { get; set; }
    }
}
