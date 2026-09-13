using Bu.CLASS_SYSTEM;
using DA;
using HRMS_API.Filters;
using HRMS_API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly SYS_USER _userBus = new SYS_USER();

        private class LoginAttemptTracker
        {
            public int FailCount { get; set; }
            public DateTime LastAttempt { get; set; }
            public DateTime? LockoutUntil { get; set; }
        }

        private static readonly Dictionary<string, SessionInfo> _activeSessions = new Dictionary<string, SessionInfo>();
        private static readonly Dictionary<string, LoginAttemptTracker> _rateLimits = new Dictionary<string, LoginAttemptTracker>();
        private static readonly object _rateLimitLock = new object();
        private const int MAX_FAILED_ATTEMPTS = 5;
        private static readonly TimeSpan LOCKOUT_PERIOD = TimeSpan.FromMinutes(15);

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

        private void RecordFailedLogin(string rateKey)
        {
            lock (_rateLimitLock)
            {
                if (!_rateLimits.TryGetValue(rateKey, out var attempt))
                {
                    attempt = new LoginAttemptTracker { FailCount = 0, LastAttempt = DateTime.Now };
                    _rateLimits[rateKey] = attempt;
                }

                attempt.FailCount++;
                attempt.LastAttempt = DateTime.Now;

                if (attempt.FailCount >= MAX_FAILED_ATTEMPTS)
                {
                    attempt.LockoutUntil = DateTime.Now.Add(LOCKOUT_PERIOD);
                }
            }
        }

        private void ResetFailedLogin(string rateKey)
        {
            lock (_rateLimitLock)
            {
                if (_rateLimits.ContainsKey(rateKey))
                {
                    _rateLimits.Remove(rateKey);
                }
            }
        }

        /// <summary>
        /// POST: api/auth/login
        /// Xác thực đăng nhập an toàn bằng BCrypt từ Oracle Database
        /// </summary>
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest("Vui lòng nhập tên đăng nhập và mật khẩu.");
            }

            string clientIp = GetClientIpAddress();
            string rateKey = $"{clientIp}_{req.Username.Trim().ToLower()}";

            // Kiểm tra Rate Limiting
            lock (_rateLimitLock)
            {
                if (_rateLimits.TryGetValue(rateKey, out var attempt))
                {
                    if (attempt.LockoutUntil.HasValue && attempt.LockoutUntil.Value > DateTime.Now)
                    {
                        var remaining = (int)Math.Ceiling((attempt.LockoutUntil.Value - DateTime.Now).TotalMinutes);
                        return Content(System.Net.HttpStatusCode.BadRequest, new
                        {
                            success = false,
                            message = $"Tài khoản hoặc thiết bị này tạm thời bị khóa do nhập sai quá {MAX_FAILED_ATTEMPTS} lần. Vui lòng thử lại sau {remaining} phút."
                        });
                    }
                    if (attempt.LockoutUntil.HasValue && attempt.LockoutUntil.Value <= DateTime.Now)
                    {
                        _rateLimits.Remove(rateKey);
                    }
                }
            }

            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    string uName = req.Username.Trim().ToLower();
                    var user = db.TB_SYS_USER.FirstOrDefault(x => x.USERNAME.Trim().ToLower() == uName && (x.ISGROUP ?? 0) == 0);
                    if (user == null)
                    {
                        user = db.TB_SYS_USER.FirstOrDefault(x => x.USERNAME.Trim().ToLower() == uName);
                    }

                    // Chống Username Enumeration: Không phân biệt tài khoản không tồn tại hay sai mật khẩu
                    if (user == null)
                    {
                        RecordFailedLogin(rateKey);
                        return BadRequest("Tên đăng nhập hoặc mật khẩu không chính xác.");
                    }

                    if ((user.DISABLED ?? 0) == 1)
                    {
                        return BadRequest("Tài khoản này đang bị vô hiệu hóa hoặc tạm khóa. Vui lòng liên hệ Quản trị viên.");
                    }

                    // Xác thực mật khẩu qua BCrypt
                    bool isPasswordValid = PasswordHasher.VerifyPassword(req.Password, user.PASSWORD);

                    // Cơ chế chuyển đổi an toàn (Auto-Migration): Hỗ trợ tài khoản CSDL chưa kịp mã hóa BCrypt
                    if (!isPasswordValid)
                    {
                        string stored = (user.PASSWORD ?? "").Trim();
                        string entered = (req.Password ?? "").Trim();

                        if (!string.IsNullOrEmpty(stored) && stored.Equals(entered, StringComparison.Ordinal))
                        {
                            isPasswordValid = true;
                            // Tự động băm BCrypt và lưu vào CSDL cho các lần đăng nhập tiếp theo
                            try
                            {
                                user.PASSWORD = PasswordHasher.HashPassword(entered);
                                db.SaveChanges();
                            }
                            catch { }
                        }
                    }

                    if (!isPasswordValid)
                    {
                        RecordFailedLogin(rateKey);
                        return BadRequest("Tên đăng nhập hoặc mật khẩu không chính xác.");
                    }

                    // Đăng nhập thành công -> Reset bộ đếm thất bại
                    ResetFailedLogin(rateKey);

                    // Phân quyền hạn chức năng
                    List<string> rights = new List<string>();
                    bool isAdmin = user.USERNAME.Trim().ToUpper() == "ADMIN" || (user.ISGROUP.HasValue && user.ISGROUP == 1);

                    if (isAdmin)
                    {
                        rights.Add("*"); // Full quyền Super Admin
                        rights.AddRange(db.TB_SYS_FUNCTION.Select(f => f.FUNCTION_CODE).ToList());
                    }
                    else
                    {
                        rights = _userBus.GetRights(user.IDUSER);
                        if (!rights.Contains("F_DB_NHANSU")) rights.Add("F_DB_NHANSU");
                        if (!rights.Contains("F_SYSTEM_AI")) rights.Add("F_SYSTEM_AI");
                    }

                    rights = rights.Distinct().ToList();

                    // Tạo Token phiên làm việc chuẩn JSON Web Token (HMAC-SHA256)
                    string token = JwtService.GenerateToken((int)user.IDUSER, user.USERNAME, user.FULLNAME ?? user.USERNAME, isAdmin, rights, user.MACTY, user.MADVI);
                    var session = new SessionInfo
                    {
                        UserId = (int)user.IDUSER,
                        Username = user.USERNAME,
                        FullName = user.FULLNAME ?? user.USERNAME,
                        IsAdmin = isAdmin,
                        Rights = rights,
                        LoginTime = DateTime.Now
                    };

                    lock (_activeSessions)
                    {
                        _activeSessions[token] = session;
                    }

                    // Ghi nhận lịch sử đăng nhập nếu có
                    try
                    {
                        var history = new TB_SYS_LOGIN_HISTORY
                        {
                            ID_USER = user.IDUSER,
                            THOIGIAN = DateTime.Now,
                            IP_ADDRESS = "127.0.0.1",
                            TRANGTHAI = "SUCCESS"
                        };
                        db.TB_SYS_LOGIN_HISTORY.Add(history);
                        db.SaveChanges();
                    }
                    catch { }

                    // Lấy chi tiết 5 quyền (Xem, Thêm, Sửa, Xóa, In)
                    var detailedRights = _userBus.GetDetailedRights(user.IDUSER);
                    if (isAdmin)
                    {
                        foreach (var key in detailedRights.Keys.ToList())
                        {
                            detailedRights[key].CAN_VIEW = true;
                            detailedRights[key].CAN_ADD = true;
                            detailedRights[key].CAN_EDIT = true;
                            detailedRights[key].CAN_DELETE = true;
                            detailedRights[key].CAN_PRINT = true;
                        }
                    }

                    return Ok(new
                    {
                        success = true,
                        Success = true,
                        token = token,
                        Token = token,
                        user = new
                        {
                            IdUser = (int)user.IDUSER,
                            id = (int)user.IDUSER,
                            Username = user.USERNAME,
                            username = user.USERNAME,
                            FullName = user.FULLNAME ?? user.USERNAME,
                            fullName = user.FULLNAME ?? user.USERNAME,
                            IsAdmin = isAdmin,
                            isAdmin = isAdmin,
                            Rights = rights,
                            rights = rights,
                            DetailedRights = detailedRights,
                            detailedRights = detailedRights
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi hệ thống khi đăng nhập: " + ex.ToString());
                string detail = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, new { 
                    success = false, 
                    message = "Lỗi kết nối CSDL hoặc máy chủ: " + detail,
                    detail = detail
                });
            }
        }

        /// <summary>
        /// GET: api/auth/me
        /// Kiểm tra phiên làm việc và lấy thông tin người dùng hiện tại (làm mới quyền hạn từ CSDL)
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
            int currentUserId = 0;
            string currentUsername = "";
            string currentFullName = "";
            bool currentIsAdmin = false;
            List<string> currentRights = new List<string>();

            // 1. Thử xác thực giải mã chuẩn JWT
            if (JwtService.ValidateToken(token, out var jwtClaims, out _))
            {
                int.TryParse(jwtClaims.UserId, out currentUserId);
                currentUsername = jwtClaims.Username;
                currentFullName = jwtClaims.FullName;
                currentIsAdmin = jwtClaims.IsAdmin;
                currentRights = jwtClaims.Rights ?? new List<string>();
            }
            else
            {
                // Fallback nếu dùng token cũ trong activeSessions
                lock (_activeSessions)
                {
                    if (_activeSessions.TryGetValue(token, out var session))
                    {
                        currentUserId = session.UserId;
                        currentUsername = session.Username;
                        currentFullName = session.FullName;
                        currentIsAdmin = session.IsAdmin;
                        currentRights = session.Rights ?? new List<string>();
                    }
                }
            }

            if (currentUserId == 0)
            {
                return Unauthorized();
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
                            detailedRights = freshDetailedRights
                        }
                    });
                }
            }
            catch
            {
                // Fallback nếu CSDL bận
                return Ok(new
                {
                    user = new
                    {
                        IdUser = currentUserId,
                        id = currentUserId,
                        Username = currentUsername,
                        username = currentUsername,
                        FullName = currentFullName,
                        fullName = currentFullName,
                        IsAdmin = currentIsAdmin,
                        isAdmin = currentIsAdmin,
                        Rights = currentRights,
                        rights = currentRights
                    }
                });
            }
        }

        /// <summary>
        /// POST: api/auth/change-password
        /// Đổi mật khẩu cho người dùng đang đăng nhập (Tương ứng chức năng DOIMATKHAU)
        /// </summary>
        [HttpPost]
        [Route("change-password")]
        public IHttpActionResult ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader == null || string.IsNullOrWhiteSpace(authHeader.Parameter))
            {
                return Unauthorized();
            }

            if (req == null || string.IsNullOrWhiteSpace(req.OldPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
            {
                return BadRequest("Vui lòng nhập mật khẩu hiện tại và mật khẩu mới.");
            }

            string token = authHeader.Parameter;
            int currentUserId = 0;

            if (JwtService.ValidateToken(token, out var jwtClaims, out _))
            {
                int.TryParse(jwtClaims.UserId, out currentUserId);
            }
            else
            {
                lock (_activeSessions)
                {
                    if (_activeSessions.TryGetValue(token, out var session))
                    {
                        currentUserId = session.UserId;
                    }
                }
            }

            if (currentUserId == 0)
            {
                return Unauthorized();
            }

            try
            {
                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == currentUserId);
                    if (user == null) return NotFound();

                    if (!PasswordHasher.VerifyPassword(req.OldPassword, user.PASSWORD))
                    {
                        return BadRequest("Mật khẩu hiện tại không chính xác.");
                    }

                    user.PASSWORD = PasswordHasher.HashPassword(req.NewPassword);
                    db.SaveChanges();

                    return Ok(new { success = true, message = "Đổi mật khẩu thành công!" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi khi đổi mật khẩu: " + ex.ToString());
                return Content(System.Net.HttpStatusCode.InternalServerError, new { success = false, message = "Đã xảy ra lỗi khi thực hiện đổi mật khẩu. Vui lòng thử lại sau." });
            }
        }

        /// <summary>
        /// POST: api/auth/logout
        /// Đăng xuất khỏi hệ thống
        /// </summary>
        [HttpPost]
        [Route("logout")]
        public IHttpActionResult Logout()
        {
            var authHeader = Request.Headers.Authorization;
            if (authHeader != null && !string.IsNullOrWhiteSpace(authHeader.Parameter))
            {
                lock (_activeSessions)
                {
                    _activeSessions.Remove(authHeader.Parameter);
                }
            }
            return Ok(new { success = true, message = "Đã đăng xuất thành công." });
        }

        private static string GenerateToken(string username)
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            byte[] userBytes = Encoding.UTF8.GetBytes(username);
            byte[] combined = time.Concat(key).Concat(userBytes).ToArray();
            return Convert.ToBase64String(combined).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
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
    }
}
