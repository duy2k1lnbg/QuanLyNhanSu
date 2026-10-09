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
using System.Net.Http;
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
        private readonly Bu.CLASS_SECURITY.IChannelPermissionResolver _channelResolver = new Bu.CLASS_SECURITY.ChannelPermissionResolver();

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
        [RateLimit(Policy = RateLimitPolicy.Login)]
        public async Task<IHttpActionResult> Login([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest("Vui lòng nhập tên đăng nhập và mật khẩu.");
            }

            string correlationId = GetHeaderValue("X-Correlation-Id") ?? Guid.NewGuid().ToString();

            // Kiểm tra rate limit theo username chuẩn hóa (5 yêu cầu/phút)
            if (!RateLimiterService.Instance.CheckLoginUsernameRate(req.Username, 5, TimeSpan.FromMinutes(1), out int retryUserSec))
            {
                var resp = Request.CreateResponse((HttpStatusCode)429, new
                {
                    success = false,
                    code = "RATE_LIMITED",
                    message = $"Tài khoản '{req.Username}' đã nhận quá nhiều lần thử đăng nhập. Vui lòng thử lại sau {retryUserSec} giây.",
                    retryAfterSeconds = retryUserSec,
                    correlationId = correlationId
                });
                resp.Headers.Add("Retry-After", retryUserSec.ToString());
                resp.Headers.Add("X-Correlation-Id", correlationId);
                return ResponseMessage(resp);
            }

            string clientIp = GetClientIpAddress();
            string userAgent = Request.Headers.UserAgent?.ToString() ?? "Unknown";
            string rawChannel = !string.IsNullOrWhiteSpace(req.ClientType) ? req.ClientType.Trim().ToUpperInvariant() : (GetHeaderValue("X-Client-Type")?.Trim().ToUpperInvariant());
            if (rawChannel != Bu.CLASS_SECURITY.AppChannels.Web && rawChannel != Bu.CLASS_SECURITY.AppChannels.Mobile)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    success = false,
                    code = "INVALID_CHANNEL",
                    message = "Kênh đăng nhập API không hợp lệ. Cổng dịch vụ trực tuyến chỉ chấp nhận kênh WEB hoặc MOBILE."
                });
            }

            string clientType = rawChannel;
            string platform = !string.IsNullOrWhiteSpace(req.Platform) ? req.Platform : (GetHeaderValue("X-Platform") ?? (clientType == Bu.CLASS_SECURITY.AppChannels.Mobile ? "MOBILE_APP" : "BROWSER"));
            string deviceId = !string.IsNullOrWhiteSpace(req.DeviceId) ? req.DeviceId : GetHeaderValue("X-Device-Id");
            string deviceName = !string.IsNullOrWhiteSpace(req.DeviceName) ? req.DeviceName : GetHeaderValue("X-Device-Name");

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
                        code = loginResult.FailureReason,
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
        /// POST: api/auth/desktop-token
        /// Trao đổi phiên Desktop đã xác thực thành công qua DB lấy JWT Bearer Token để sử dụng các dịch vụ AI / API.
        /// Yêu cầu cung cấp thông tin xác thực và phiên Desktop còn hiệu lực trong TB_AUTH_SESSION.
        /// Đảm bảo không tạo 2 phiên trùng lặp và không cấp token cho client nếu thiếu quyền F_LOGIN_DESKTOP.
        /// </summary>
        [HttpPost]
        [Route("desktop-token")]
        [AllowAnonymous]
        [RateLimit(Policy = RateLimitPolicy.DesktopToken)]
        public async Task<IHttpActionResult> ExchangeDesktopToken([FromBody] DesktopTokenExchangeRequest req)
        {
            await Task.Yield();
            if (req == null || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password)
                || string.IsNullOrWhiteSpace(req.SessionId) || string.IsNullOrWhiteSpace(req.Jti))
            {
                return BadRequest("Vui lòng cung cấp đầy đủ thông tin xác thực và định danh phiên Desktop.");
            }

            try
            {
                using (var db = new MyEntities())
                {
                    // 1. Phân giải người dùng theo username hoặc mã nhân viên
                    string input = req.Username.Trim().ToLowerInvariant();
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.USERNAME.ToLower() == input && u.ISGROUP != 1);
                    if (user == null)
                    {
                        var emp = db.TB_NHANVIEN.FirstOrDefault(e => e.EMPLOYEE_CODE.ToLower() == input);
                        if (emp != null)
                        {
                            user = db.TB_SYS_USER.FirstOrDefault(u => u.MANV == emp.MANV && u.ISGROUP != 1);
                        }
                    }

                    if (user == null || !PasswordHasher.VerifyPassword(req.Password, user.PASSWORD))
                    {
                        return Content(HttpStatusCode.Unauthorized, new
                        {
                            success = false,
                            code = "INVALID_CREDENTIALS",
                            message = "Tên đăng nhập hoặc mật khẩu không chính xác."
                        });
                    }

                    if (user.DISABLED == 1)
                    {
                        return Content(HttpStatusCode.Forbidden, new
                        {
                            success = false,
                            code = "ACCOUNT_DISABLED",
                            message = "Tài khoản của bạn đã bị khóa hoặc vô hiệu hóa."
                        });
                    }

                    // 2. Xác thực phiên Desktop trong TB_AUTH_SESSION
                    var sessionRow = db.Database.SqlQuery<SessionStateRow>(@"
                        SELECT SESSION_ID, USER_ID, JTI, CLIENT_TYPE, EXPIRES_AT, REVOKED_AT
                        FROM HR.TB_AUTH_SESSION
                        WHERE SESSION_ID = :p0 AND JTI = :p1 AND USER_ID = :p2",
                        new OracleParameter("p0", req.SessionId.Trim()),
                        new OracleParameter("p1", req.Jti.Trim()),
                        new OracleParameter("p2", user.IDUSER)
                    ).FirstOrDefault();

                    if (sessionRow == null || sessionRow.REVOKED_AT.HasValue || sessionRow.EXPIRES_AT <= DateTime.UtcNow)
                    {
                        return Content(HttpStatusCode.Unauthorized, new
                        {
                            success = false,
                            code = "SESSION_INVALID_OR_EXPIRED",
                            message = "Phiên làm việc Desktop không tồn tại, đã bị thu hồi hoặc đã hết hạn."
                        });
                    }

                    string sessionChannel = Bu.CLASS_SECURITY.AppChannels.Normalize(sessionRow.CLIENT_TYPE);
                    if (sessionChannel != Bu.CLASS_SECURITY.AppChannels.Desktop)
                    {
                        return Content(HttpStatusCode.Forbidden, new
                        {
                            success = false,
                            code = "CHANNEL_MISMATCH",
                            message = "Phiên đăng nhập này không thuộc kênh DESKTOP."
                        });
                    }

                    // 3. Kiểm tra quyền F_LOGIN_DESKTOP qua PlatformAccessGuard (Zero-Trust)
                    bool canDesktop = Bu.CLASS_SECURITY.PlatformAccessGuard.Current.CanExecute(
                        user.IDUSER, Bu.CLASS_SECURITY.AppChannels.Desktop, Bu.CLASS_SECURITY.PlatformFunctionCodes.LoginDesktop, Bu.CLASS_SECURITY.ChannelAction.View, user.USERNAME
                    );
                    if (!canDesktop)
                    {
                        return Content(HttpStatusCode.Forbidden, new
                        {
                            success = false,
                            code = "PLATFORM_ACCESS_DENIED",
                            message = "Từ chối truy cập: Quyền sử dụng kênh DESKTOP đã bị tắt hoặc chưa được cấp."
                        });
                    }

                    // 4. Lấy danh sách quyền và phát hành JWT Token với kênh DESKTOP
                    bool isRootAdmin = user.USERNAME.Equals("admin", StringComparison.OrdinalIgnoreCase);
                    var rights = new List<string>();
                    if (isRootAdmin)
                    {
                        rights.Add("*");
                        var allFuncs = db.TB_SYS_FUNCTION.Select(f => f.FUNCTION_CODE).ToList();
                        rights.AddRange(allFuncs);
                    }
                    else
                    {
                        var direct = db.TB_SYS_RIGHT.Where(r => r.IDUSER == user.IDUSER && (r.CAN_VIEW == 1 || r.USER_RIGHT == 1)).Select(r => r.FUNCTION_CODE).ToList();
                        rights.AddRange(direct);
                        var groupIds = db.TB_SYS_GROUP.Where(g => g.MEMBER == user.IDUSER).Select(g => g.ID_GROUP).ToList();
                        if (groupIds.Any())
                        {
                            var grpRights = db.TB_SYS_RIGHT.Where(r => groupIds.Contains(r.IDUSER) && (r.CAN_VIEW == 1 || r.USER_RIGHT == 1)).Select(r => r.FUNCTION_CODE).ToList();
                            rights.AddRange(grpRights);
                        }
                    }

                    long tokenVer = Convert.ToInt64(user.TOKEN_VERSION);
                    string token = JwtService.GenerateToken(
                        (int)user.IDUSER,
                        user.USERNAME,
                        user.FULLNAME ?? user.USERNAME,
                        isRootAdmin,
                        rights.Distinct().ToList(),
                        user.MACTY,
                        user.MADVI,
                        user.MANV.HasValue ? user.MANV.Value.ToString() : null,
                        Bu.CLASS_SECURITY.AppChannels.Desktop,
                        sessionRow.JTI,
                        tokenVer
                    );

                    return Ok(new
                    {
                        success = true,
                        token = token,
                        Token = token,
                        sessionId = sessionRow.SESSION_ID,
                        jti = sessionRow.JTI,
                        clientType = Bu.CLASS_SECURITY.AppChannels.Desktop,
                        expiresAt = sessionRow.EXPIRES_AT
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi trong ExchangeDesktopToken: " + ex);
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Đã xảy ra sự cố nội bộ trong quá trình trao đổi phiên Desktop."
                });
            }
        }

        /// <summary>
        /// POST: api/auth/desktop-login
        /// Đăng nhập trực tiếp kênh Desktop tạo phiên TB_AUTH_SESSION và cấp JWT Bearer Token.
        /// </summary>
        [HttpPost]
        [Route("desktop-login")]
        [AllowAnonymous]
        [RateLimit(Policy = RateLimitPolicy.Login)]
        public async Task<IHttpActionResult> DesktopLogin([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest("Vui lòng nhập tên đăng nhập và mật khẩu.");
            }

            string correlationId = GetHeaderValue("X-Correlation-Id") ?? Guid.NewGuid().ToString();

            // Kiểm tra rate limit theo username chuẩn hóa (5 yêu cầu/phút)
            if (!RateLimiterService.Instance.CheckLoginUsernameRate(req.Username, 5, TimeSpan.FromMinutes(1), out int retryUserSec))
            {
                var resp = Request.CreateResponse((HttpStatusCode)429, new
                {
                    success = false,
                    code = "RATE_LIMITED",
                    message = $"Tài khoản '{req.Username}' đã nhận quá nhiều lần thử đăng nhập. Vui lòng thử lại sau {retryUserSec} giây.",
                    retryAfterSeconds = retryUserSec,
                    correlationId = correlationId
                });
                resp.Headers.Add("Retry-After", retryUserSec.ToString());
                resp.Headers.Add("X-Correlation-Id", correlationId);
                return ResponseMessage(resp);
            }

            string clientIp = GetClientIpAddress();
            string userAgent = Request.Headers.UserAgent?.ToString() ?? "HRMS-Desktop/1.0";
            string deviceId = !string.IsNullOrWhiteSpace(req.DeviceId) ? req.DeviceId : GetHeaderValue("X-Device-Id");
            string deviceName = !string.IsNullOrWhiteSpace(req.DeviceName) ? req.DeviceName : (GetHeaderValue("X-Device-Name") ?? "Desktop-Client");

            try
            {
                var loginResult = await _authSecurityService.AuthenticateAsync(
                    req.Username,
                    req.Password,
                    Bu.CLASS_SECURITY.AppChannels.Desktop,
                    "WINDOWS",
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
                        code = loginResult.FailureReason,
                        message = loginResult.ErrorMessage,
                        isLockedOut = loginResult.IsLockedOut,
                        lockoutMinutes = loginResult.LockoutRemainingMinutes
                    });
                }

                string token = JwtService.GenerateToken(
                    (int)loginResult.UserId,
                    loginResult.Username,
                    loginResult.FullName,
                    loginResult.IsAdmin,
                    loginResult.Rights,
                    loginResult.MaCty,
                    loginResult.MaDvi,
                    loginResult.Manv.HasValue ? loginResult.Manv.Value.ToString() : null,
                    Bu.CLASS_SECURITY.AppChannels.Desktop,
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
                    jti = loginResult.Jti,
                    clientType = Bu.CLASS_SECURITY.AppChannels.Desktop,
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
                        ClientType = Bu.CLASS_SECURITY.AppChannels.Desktop,
                        clientType = Bu.CLASS_SECURITY.AppChannels.Desktop
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi trong DesktopLogin: " + ex);
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Đã xảy ra sự cố nội bộ trong quá trình đăng nhập Desktop."
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

                    // Lấy chi tiết 5 quyền theo kênh và bảng phân quyền thực tế (Zero-Trust, loại bỏ giả lập quyền ảo cho ADMIN)
                    Dictionary<string, Bu.DTO.UserRightDetail> freshDetailedRights = null;
                    List<string> projectedRefreshViewRights = null;
                    string refreshChannel = jwtClaims != null ? Bu.CLASS_SECURITY.AppChannels.Normalize(jwtClaims.ClientType) : null;
                    if (!string.IsNullOrEmpty(refreshChannel))
                    {
                        try
                        {
                            var channelTree = _channelResolver.ResolveChannelTree(db, user.IDUSER, refreshChannel, user.USERNAME);
                            if (channelTree != null)
                            {
                                _channelResolver.ProjectEffectiveRights(channelTree, out freshDetailedRights, out projectedRefreshViewRights);
                            }
                        }
                        catch (Exception chEx)
                        {
                            System.Diagnostics.Trace.TraceWarning("[AuthController.RefreshSession] ResolveChannelTree error: " + chEx.Message);
                            freshDetailedRights = null;
                            projectedRefreshViewRights = null;
                        }
                    }

                    // Chỉ fallback sang legacy table khi không có kênh hoặc resolver lỗi (chưa có schema channel)
                    if (freshDetailedRights == null)
                    {
                        freshDetailedRights = _userBus.GetDetailedRights(user.IDUSER);
                    }

                    if (isAdmin)
                    {
                        freshRights.Add("*");
                        if (freshDetailedRights != null && freshDetailedRights.Any())
                        {
                            freshRights.AddRange(freshDetailedRights.Where(kv => kv.Value.CAN_VIEW).Select(kv => kv.Key));
                        }
                        else
                        {
                            freshRights.AddRange(db.TB_SYS_FUNCTION.Select(f => f.FUNCTION_CODE).ToList());
                        }
                    }
                    else
                    {
                        if (freshDetailedRights != null && freshDetailedRights.Any())
                        {
                            freshRights.AddRange(freshDetailedRights.Where(kv => kv.Value.CAN_VIEW).Select(kv => kv.Key));
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
                        }
                    }

                    freshRights = freshRights.Distinct().ToList();

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

    public class DesktopTokenExchangeRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string SessionId { get; set; }
        public string Jti { get; set; }
    }

    public class SessionStateRow
    {
        public string SESSION_ID { get; set; }
        public decimal USER_ID { get; set; }
        public string JTI { get; set; }
        public string CLIENT_TYPE { get; set; }
        public DateTime EXPIRES_AT { get; set; }
        public DateTime? REVOKED_AT { get; set; }
    }
}
