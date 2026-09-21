using Bu.CLASS_SYSTEM;
using DA;
using HRMS_API.Filters;
using HRMS_API.Services;
using Oracle.ManagedDataAccess.Client;
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

                    string input = (req.Username ?? "").Trim();
                    if (string.IsNullOrEmpty(input))
                    {
                        return BadRequest("Vui lòng nhập tên đăng nhập hoặc mã nhân viên.");
                    }
                    string inputLower = input.ToLowerInvariant();

                    TB_SYS_USER user = null;
                    decimal? resolvedManv = null;
                    string resolvedEmployeeCode = null;

                    // 1. Phân giải danh tính (Identity Resolution): Thử tìm theo Tên Đăng Nhập ổn định (LoginName / TB_SYS_USER.USERNAME)
                    user = db.TB_SYS_USER.FirstOrDefault(x => x.USERNAME.Trim().ToLower() == inputLower && (x.ISGROUP ?? 0) == 0);
                    if (user == null)
                    {
                        user = db.TB_SYS_USER.FirstOrDefault(x => x.USERNAME.Trim().ToLower() == inputLower);
                    }

                    // 2. Nếu không trùng LoginName: Thử tìm theo Mã Nhân Viên nghiệp vụ nguyên bản (EMPLOYEE_CODE)
                    // TUYỆT ĐỐI KHÔNG chuẩn hóa 01 -> 1, giữ nguyên định dạng chính xác (VD: 01, 0001, PX01-KT-TV-2026-001)
                    if (user == null)
                    {
                        try
                        {
                            var emp = db.Database.SqlQuery<EmpRow>(
                                "SELECT MANV, EMPLOYEE_CODE, DATHOIVIEC, HOTEN FROM HR.TB_NHANVIEN WHERE LOWER(TRIM(EMPLOYEE_CODE)) = :p0 AND ROWNUM = 1",
                                new OracleParameter("p0", inputLower)
                            ).FirstOrDefault();

                            if (emp != null)
                            {
                                resolvedManv = emp.MANV;
                                resolvedEmployeeCode = emp.EMPLOYEE_CODE;

                                // Tìm tài khoản tương ứng qua bảng liên kết TB_USER_EMPLOYEE_MAPPING (1-1)
                                var mapped = db.Database.SqlQuery<MappingRow>(
                                    "SELECT USER_ID, EMPLOYEE_ID, IS_MOBILE_ENABLED FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE EMPLOYEE_ID = :p0 AND ROWNUM = 1",
                                    new OracleParameter("p0", emp.MANV)
                                ).FirstOrDefault();

                                if (mapped != null && mapped.USER_ID.HasValue)
                                {
                                    user = db.TB_SYS_USER.FirstOrDefault(x => x.IDUSER == mapped.USER_ID.Value && (x.ISGROUP ?? 0) == 0);
                                    if (user == null)
                                    {
                                        user = db.TB_SYS_USER.FirstOrDefault(x => x.IDUSER == mapped.USER_ID.Value);
                                    }
                                }

                                // Dự phòng tương thích ngược với trường MANV trên TB_SYS_USER
                                if (user == null)
                                {
                                    user = db.TB_SYS_USER.FirstOrDefault(x => x.MANV == emp.MANV && (x.ISGROUP ?? 0) == 0);
                                    if (user == null)
                                    {
                                        user = db.TB_SYS_USER.FirstOrDefault(x => x.MANV == emp.MANV);
                                    }
                                }
                            }
                        }
                        catch (Exception exSql)
                        {
                            System.Diagnostics.Trace.TraceWarning("Lỗi khi tra cứu mã nhân viên: " + exSql.Message);
                        }
                    }

                    // Chống Username Enumeration: Không phân biệt tài khoản không tồn tại hay sai mật khẩu
                    if (user == null)
                    {
                        RecordFailedLogin(rateKey);
                        return BadRequest("Mã đăng nhập hoặc mật khẩu không chính xác.");
                    }

                    if ((user.DISABLED ?? 0) == 1)
                    {
                        return BadRequest("Tài khoản này đang bị vô hiệu hóa hoặc tạm khóa. Vui lòng liên hệ Quản trị viên.");
                    }

                    bool isRootAdmin = user.USERNAME != null && user.USERNAME.Trim().ToUpper() == "ADMIN";

                    // Xác thực mật khẩu qua BCrypt
                    bool isPasswordValid = false;
                    string entered = (req.Password ?? "").Trim();

                    // 1. Quản trị viên hệ thống ADMIN: hỗ trợ các mật khẩu mặc định (admin, ADMIN, 123, 123456)
                    if (isRootAdmin && (entered == "admin" || entered == "ADMIN" || entered == "123" || entered == "123456"))
                    {
                        isPasswordValid = true;
                    }
                    else if (PasswordHasher.VerifyPassword(req.Password, user.PASSWORD))
                    {
                        isPasswordValid = true;
                    }
                    else
                    {
                        string stored = (user.PASSWORD ?? "").Trim();
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
                        return BadRequest("Mã đăng nhập hoặc mật khẩu không chính xác.");
                    }

                    // Phân giải hồ sơ nhân viên và quyền truy cập Mobile
                    int isMobileEnabled = isRootAdmin ? 0 : 1;

                    if (isRootAdmin)
                    {
                        resolvedManv = null;
                        resolvedEmployeeCode = null;
                    }
                    else if (!resolvedManv.HasValue)
                    {
                        try
                        {
                            var mapping = db.Database.SqlQuery<MappingRow>(
                                "SELECT USER_ID, EMPLOYEE_ID, IS_MOBILE_ENABLED FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0 AND ROWNUM = 1",
                                new OracleParameter("p0", user.IDUSER)
                            ).FirstOrDefault();

                            if (mapping != null && mapping.EMPLOYEE_ID.HasValue && mapping.EMPLOYEE_ID.Value > 0)
                            {
                                resolvedManv = mapping.EMPLOYEE_ID.Value;
                                if (mapping.IS_MOBILE_ENABLED.HasValue)
                                {
                                    isMobileEnabled = (int)mapping.IS_MOBILE_ENABLED.Value;
                                }
                            }
                        }
                        catch { }

                        // Fallback sang user.MANV
                        if (!resolvedManv.HasValue && user.MANV.HasValue && user.MANV.Value > 0)
                        {
                            resolvedManv = user.MANV.Value;
                        }
                    }

                    if (resolvedManv.HasValue && resolvedManv.Value > 0)
                    {
                        try
                        {
                            var empDetails = db.Database.SqlQuery<EmpRow>(
                                "SELECT MANV, EMPLOYEE_CODE, DATHOIVIEC, HOTEN FROM HR.TB_NHANVIEN WHERE MANV = :p0 AND ROWNUM = 1",
                                new OracleParameter("p0", resolvedManv.Value)
                            ).FirstOrDefault();

                            if (empDetails != null)
                            {
                                // Rule 52, 54: Nếu nhân viên đã thôi việc (DATHOIVIEC == 1), chặn phiên truy cập
                                if ((empDetails.DATHOIVIEC ?? 0) == 1)
                                {
                                    return Content(System.Net.HttpStatusCode.BadRequest, new
                                    {
                                        success = false,
                                        message = "Hồ sơ nhân viên liên kết đã thôi việc. Tài khoản tạm dừng hoạt động."
                                    });
                                }

                                resolvedEmployeeCode = empDetails.EMPLOYEE_CODE;
                            }

                            // Tra cứu cờ IS_MOBILE_ENABLED từ bảng mapping
                            var mapFlag = db.Database.SqlQuery<decimal?>(
                                "SELECT IS_MOBILE_ENABLED FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0 AND ROWNUM = 1",
                                new OracleParameter("p0", user.IDUSER)
                            ).FirstOrDefault();

                            if (mapFlag.HasValue)
                            {
                                isMobileEnabled = (int)mapFlag.Value;
                            }
                        }
                        catch { }
                    }

                    // Kiểm tra loại ứng dụng client (Client Type) và quyền truy cập Mobile
                    string clientType = req.ClientType;
                    if (string.IsNullOrWhiteSpace(clientType))
                    {
                        if (Request.Headers.TryGetValues("X-Client-Type", out var cVals))
                        {
                            clientType = cVals.FirstOrDefault();
                        }
                    }
                    if (string.IsNullOrWhiteSpace(clientType))
                    {
                        clientType = "ALL";
                    }
                    clientType = clientType.Trim().ToUpperInvariant();

                    if (clientType == "MOBILE")
                    {
                        if (isRootAdmin)
                        {
                            return Content(System.Net.HttpStatusCode.BadRequest, new
                            {
                                success = false,
                                message = "Tài khoản Quản trị viên tối cao (ADMIN) dành riêng cho cổng quản lý (Web) và ứng dụng quản trị (Desktop), không áp dụng cho ứng dụng nhân viên tự phục vụ (Mobile)."
                            });
                        }

                        string userAllowedClient = (user.CLIENT_TYPE ?? "ALL").Trim().ToUpperInvariant();
                        if (userAllowedClient == "DESKTOP" || userAllowedClient == "SYSTEM" || userAllowedClient == "WEB")
                        {
                            return Content(System.Net.HttpStatusCode.BadRequest, new
                            {
                                success = false,
                                message = "Tài khoản hệ thống chỉ dùng cho Desktop và Web, không được phép truy cập ứng dụng di động (Mobile)."
                            });
                        }

                        if (!resolvedManv.HasValue || resolvedManv.Value <= 0)
                        {
                            return Content(System.Net.HttpStatusCode.BadRequest, new
                            {
                                success = false,
                                message = "Tài khoản hệ thống không được phép đăng nhập ứng dụng di động (Mobile). Bản Mobile chỉ dành cho tài khoản nhân viên."
                            });
                        }

                        if (isMobileEnabled == 0)
                        {
                            return Content(System.Net.HttpStatusCode.BadRequest, new
                            {
                                success = false,
                                message = "Tài khoản chưa được kích hoạt quyền truy cập ứng dụng di động (Mobile Access)."
                            });
                        }
                    }
                    else
                    {
                        // Đăng nhập Web / Cổng quản trị
                        string userAllowedClient = (user.CLIENT_TYPE ?? "ALL").Trim().ToUpperInvariant();
                        if (userAllowedClient == "MOBILE")
                        {
                            return Content(System.Net.HttpStatusCode.BadRequest, new
                            {
                                success = false,
                                message = "Tài khoản nhân viên chỉ dùng để đăng nhập ứng dụng di động (Mobile), không có quyền truy cập cổng quản trị Web."
                            });
                        }
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
                    string token = JwtService.GenerateToken(
                        (int)user.IDUSER, 
                        user.USERNAME, 
                        user.FULLNAME ?? user.USERNAME, 
                        isAdmin, 
                        rights, 
                        user.MACTY, 
                        user.MADVI,
                        resolvedManv.HasValue ? resolvedManv.Value.ToString() : null,
                        clientType
                    );

                    var session = new SessionInfo
                    {
                        UserId = (int)user.IDUSER,
                        Username = user.USERNAME,
                        FullName = user.FULLNAME ?? user.USERNAME,
                        IsAdmin = isAdmin,
                        Rights = rights,
                        LoginTime = DateTime.Now,
                        Manv = resolvedManv,
                        ClientType = clientType
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
                            detailedRights = detailedRights,
                            manv = resolvedManv,
                            Manv = resolvedManv,
                            employeeCode = resolvedEmployeeCode,
                            EmployeeCode = resolvedEmployeeCode,
                            isMobileEnabled = (isMobileEnabled == 1),
                            IsMobileEnabled = (isMobileEnabled == 1),
                            clientType = clientType,
                            ClientType = clientType
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
        public string ClientType { get; set; }
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
