using DA;
using Bu.CLASS_SYSTEM;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_SECURITY
{
    public interface IAuthSecurityService
    {
        Task<LoginResultDto> AuthenticateAsync(
            string usernameOrEmpCode,
            string password,
            string clientType,
            string platform,
            string deviceId,
            string deviceName,
            string clientIp,
            string userAgent,
            string correlationId
        );

        Task<bool> ValidateSessionAsync(string jti, long tokenVersion, decimal userId);
        Task<bool> ValidateSessionAsync(string jti, long tokenVersion, decimal userId, string expectedChannel);
        bool ValidateSession(string jti, long tokenVersion, decimal userId);
        bool ValidateSession(string jti, long tokenVersion, decimal userId, string expectedChannel);

        Task<bool> RevokeSessionAsync(string sessionIdOrJti, string reason, decimal actorUserId, string correlationId);

        Task<int> RevokeAllSessionsAsync(decimal userId, string reason, bool incrementSecurityVersion, decimal actorUserId, string correlationId);
        Task<int> RevokePlatformSessionsAsync(decimal userId, string channel, string reason, decimal actorUserId, string correlationId);

        Task<ChangePasswordResultDto> ChangePasswordWithRevocationAsync(
            decimal userId,
            string oldPassword,
            string newPassword,
            string clientIp,
            string userAgent,
            string correlationId
        );

        Task<List<SessionInfoDto>> GetUserSessionsAsync(decimal userId, string currentJti);

        Task<bool> UnlockUserAsync(decimal targetUserId, decimal actorUserId, string correlationId);
    }

    public class AuthSecurityService : IAuthSecurityService
    {
        private readonly ISessionPolicyService _policyService;
        private readonly IAuthAuditService _auditService;
        private readonly IPlatformAccessResolver _platformResolver;
        private readonly IChannelPermissionResolver _channelResolver;

        public AuthSecurityService(
            ISessionPolicyService policyService = null, 
            IAuthAuditService auditService = null,
            IPlatformAccessResolver platformResolver = null,
            IChannelPermissionResolver channelResolver = null)
        {
            _policyService = policyService ?? new SessionPolicyService();
            _auditService = auditService ?? new AuthAuditService();
            _platformResolver = platformResolver ?? new PlatformAccessResolver();
            _channelResolver = channelResolver ?? new ChannelPermissionResolver(_platformResolver, _auditService);
        }

        private static string HashDeviceId(string rawDeviceId)
        {
            if (string.IsNullOrWhiteSpace(rawDeviceId)) return null;
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawDeviceId.Trim()));
                var sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static string GenerateCryptoHex(int byteCount = 32)
        {
            byte[] bytes = new byte[byteCount];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var sb = new StringBuilder();
            foreach (byte b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public async Task<LoginResultDto> AuthenticateAsync(
            string usernameOrEmpCode,
            string password,
            string clientType,
            string platform,
            string deviceId,
            string deviceName,
            string clientIp,
            string userAgent,
            string correlationId)
        {
            var result = new LoginResultDto();
            string deviceIdHash = HashDeviceId(deviceId);

            if (string.IsNullOrWhiteSpace(usernameOrEmpCode) || string.IsNullOrWhiteSpace(password))
            {
                result.Success = false;
                result.ErrorMessage = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return result;
            }

            string input = usernameOrEmpCode.Trim();
            string inputLower = input.ToLowerInvariant();
            string normChannel = AppChannels.Normalize(clientType);
            if (normChannel == null)
            {
                result.Success = false;
                result.FailureReason = "INVALID_CHANNEL";
                result.ErrorMessage = "Kênh đăng nhập không hợp lệ. Chỉ chấp nhận DESKTOP, WEB hoặc MOBILE.";
                return result;
            }
            clientType = normChannel;
            platform = (platform ?? (clientType == AppChannels.Desktop ? "WINDOWS" : "WEB")).Trim().ToUpperInvariant();

            using (var db = new MyEntities())
            {
                // Sử dụng Transaction với ReadCommitted để serialize row-lock trên user
                using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // 1. Phân giải tài khoản (Username hoặc Employee Code)
                        decimal? resolvedUserId = null;

                        // Tìm theo Username
                        var userBasic = db.Database.SqlQuery<UserLookupRow>(@"
                            SELECT IDUSER, USERNAME, PASSWORD, DISABLED, ISGROUP, MACTY, MADVI, MANV, CLIENT_TYPE,
                                   FAILED_LOGIN_COUNT, LOCKOUT_END, TOKEN_VERSION
                            FROM HR.TB_SYS_USER
                            WHERE LOWER(TRIM(USERNAME)) = :p0 AND ROWNUM = 1",
                            new OracleParameter("p0", inputLower)
                        ).FirstOrDefault();

                        if (userBasic != null)
                        {
                            bool isRootAdminBasic = userBasic.USERNAME != null && userBasic.USERNAME.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
                            if ((userBasic.ISGROUP ?? 0) == 1 && !isRootAdminBasic)
                            {
                                await _auditService.RecordLoginAttemptAsync(
                                    userBasic.IDUSER, input, clientType, deviceIdHash, clientIp, userAgent,
                                    false, "GROUP_CANNOT_LOGIN", correlationId
                                );
                                result.Success = false;
                                result.ErrorMessage = "Nhóm người dùng không thể dùng để đăng nhập.";
                                transaction.Commit();
                                return result;
                            }
                            resolvedUserId = userBasic.IDUSER;
                        }
                        else
                        {
                            // Tìm theo Mã Nhân Viên nguyên bản
                            var emp = db.Database.SqlQuery<EmpLookupRow>(@"
                                SELECT MANV, EMPLOYEE_CODE, DATHOIVIEC, HOTEN
                                FROM HR.TB_NHANVIEN
                                WHERE LOWER(TRIM(EMPLOYEE_CODE)) = :p0 AND ROWNUM = 1",
                                new OracleParameter("p0", inputLower)
                            ).FirstOrDefault();

                            if (emp != null)
                            {
                                // Tìm qua bảng liên kết mapping 1-1
                                var mapped = db.Database.SqlQuery<decimal?>(@"
                                    SELECT USER_ID FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE EMPLOYEE_ID = :p0 AND ROWNUM = 1",
                                    new OracleParameter("p0", emp.MANV)
                                ).FirstOrDefault();

                                if (mapped.HasValue && mapped.Value > 0)
                                {
                                    resolvedUserId = mapped.Value;
                                }
                                else
                                {
                                    // Fallback sang TB_SYS_USER.MANV
                                    var userByManv = db.Database.SqlQuery<decimal?>(@"
                                        SELECT IDUSER FROM HR.TB_SYS_USER WHERE MANV = :p0 AND (ISGROUP = 0 OR ISGROUP IS NULL) AND ROWNUM = 1",
                                        new OracleParameter("p0", emp.MANV)
                                    ).FirstOrDefault();

                                    if (userByManv.HasValue) resolvedUserId = userByManv.Value;
                                }
                            }
                        }

                        // Nếu không tìm thấy người dùng
                        if (!resolvedUserId.HasValue)
                        {
                            await _auditService.RecordLoginAttemptAsync(
                                null, input, clientType, deviceIdHash, clientIp, userAgent,
                                false, "USER_NOT_FOUND", correlationId
                            );
                            await _auditService.LogEventAsync(
                                null, null, null, AuthAuditEvents.LoginFailed, "FAILURE",
                                "USER_NOT_FOUND", clientType, deviceIdHash, clientIp, userAgent, correlationId,
                                new { identifier = input }
                            );

                            result.Success = false;
                            result.ErrorMessage = "Mã đăng nhập hoặc mật khẩu không chính xác.";
                            transaction.Commit();
                            return result;
                        }

                        // 2. Row Lock trên bản ghi người dùng để tuần tự hóa (prevent race conditions)
                        var lockedUser = db.Database.SqlQuery<UserLockedRow>(@"
                            SELECT IDUSER, USERNAME, FULLNAME, PASSWORD, DISABLED, ISGROUP, MACTY, MADVI, MANV, CLIENT_TYPE,
                                   NVL(FAILED_LOGIN_COUNT, 0) AS FAILED_LOGIN_COUNT, LOCKOUT_END, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION
                            FROM HR.TB_SYS_USER
                            WHERE IDUSER = :p0
                            FOR UPDATE",
                            new OracleParameter("p0", resolvedUserId.Value)
                        ).FirstOrDefault();

                        if (lockedUser == null)
                        {
                            result.Success = false;
                            result.ErrorMessage = "Mã đăng nhập hoặc mật khẩu không chính xác.";
                            transaction.Commit();
                            return result;
                        }

                        bool isRootAdmin = lockedUser.USERNAME != null && lockedUser.USERNAME.Trim().ToUpperInvariant() == "ADMIN";
                        var policy = await _policyService.GetEffectivePolicyAsync(lockedUser.IDUSER, lockedUser.USERNAME, isRootAdmin);

                        // 3. Kiểm tra tài khoản bị vô hiệu hóa
                        if (lockedUser.DISABLED.HasValue && lockedUser.DISABLED.Value == 1)
                        {
                            await _auditService.RecordLoginAttemptAsync(
                                lockedUser.IDUSER, input, clientType, deviceIdHash, clientIp, userAgent,
                                false, "ACCOUNT_DISABLED", correlationId
                            );
                            await _auditService.LogEventAsync(
                                lockedUser.IDUSER, null, null, AuthAuditEvents.LoginFailed, "DENIED",
                                "ACCOUNT_DISABLED", clientType, deviceIdHash, clientIp, userAgent, correlationId
                            );

                            result.Success = false;
                            result.ErrorMessage = "Tài khoản này đang bị vô hiệu hóa. Vui lòng liên hệ Quản trị viên.";
                            transaction.Commit();
                            return result;
                        }

                        // 4. Kiểm tra tài khoản bị khóa tạm thời (Lockout)
                        DateTime now = DateTime.Now;
                        if (lockedUser.LOCKOUT_END.HasValue && lockedUser.LOCKOUT_END.Value > now)
                        {
                            int remainingMinutes = (int)Math.Ceiling((lockedUser.LOCKOUT_END.Value - now).TotalMinutes);
                            await _auditService.RecordLoginAttemptAsync(
                                lockedUser.IDUSER, input, clientType, deviceIdHash, clientIp, userAgent,
                                false, "ACCOUNT_LOCKED", correlationId
                            );

                            result.Success = false;
                            result.IsLocked = true;
                            result.IsLockedOut = true;
                            result.LockoutMinutes = remainingMinutes;
                            result.LockoutRemainingMinutes = remainingMinutes;
                            result.ErrorMessage = $"Tài khoản tạm thời bị khóa do nhập sai nhiều lần. Vui lòng thử lại sau {remainingMinutes} phút.";
                            transaction.Commit();
                            return result;
                        }

                        // 5. Xác thực Mật khẩu (Chỉ dùng BCrypt an toàn, không backdoor)
                        bool isPasswordValid = PasswordHasher.VerifyPassword(password, lockedUser.PASSWORD);
                        if (!isPasswordValid)
                        {
                            // Tự động kiểm tra nếu mật khẩu cũ trong DB chưa hash BCrypt (legacy migration)
                            string stored = (lockedUser.PASSWORD ?? "").Trim();
                            if (!string.IsNullOrEmpty(stored) && stored.Equals(password.Trim(), StringComparison.Ordinal))
                            {
                                isPasswordValid = true;
                                string newHash = PasswordHasher.HashPassword(password.Trim());
                                db.Database.ExecuteSqlCommand(
                                    "UPDATE HR.TB_SYS_USER SET PASSWORD = :p0 WHERE IDUSER = :p1",
                                    new OracleParameter("p0", newHash),
                                    new OracleParameter("p1", lockedUser.IDUSER)
                                );
                            }
                        }

                        if (!isPasswordValid)
                        {
                            int newFailedCount = (int)lockedUser.FAILED_LOGIN_COUNT + 1;
                            DateTime? newLockout = null;
                            string lockReason = null;

                            if (newFailedCount >= policy.MAX_FAILED_LOGIN_ATTEMPTS)
                            {
                                newLockout = now.AddMinutes(policy.LOCKOUT_DURATION_MINUTES);
                                lockReason = "BRUTE_FORCE_EXCEEDED";

                                await _auditService.LogEventAsync(
                                    lockedUser.IDUSER, null, null, AuthAuditEvents.AccountLocked, "FAILURE",
                                    $"Locked for {policy.LOCKOUT_DURATION_MINUTES} mins after {newFailedCount} failed attempts",
                                    clientType, deviceIdHash, clientIp, userAgent, correlationId
                                );
                            }

                            db.Database.ExecuteSqlCommand(@"
                                UPDATE HR.TB_SYS_USER
                                SET FAILED_LOGIN_COUNT = :p0,
                                    LAST_FAILED_LOGIN_AT = CURRENT_TIMESTAMP,
                                    FIRST_FAILED_LOGIN_AT = NVL(FIRST_FAILED_LOGIN_AT, CURRENT_TIMESTAMP),
                                    LOCKOUT_END = :p1,
                                    LOCK_REASON = :p2
                                WHERE IDUSER = :p3",
                                new OracleParameter("p0", newFailedCount),
                                newLockout.HasValue ? (object)new OracleParameter("p1", newLockout.Value) : new OracleParameter("p1", DBNull.Value),
                                (object)new OracleParameter("p2", lockReason) ?? new OracleParameter("p2", DBNull.Value),
                                new OracleParameter("p3", lockedUser.IDUSER)
                            );

                            await _auditService.RecordLoginAttemptAsync(
                                lockedUser.IDUSER, input, clientType, deviceIdHash, clientIp, userAgent,
                                false, "INVALID_PASSWORD", correlationId
                            );
                            await _auditService.LogEventAsync(
                                lockedUser.IDUSER, null, null, AuthAuditEvents.LoginFailed, "FAILURE",
                                "INVALID_PASSWORD", clientType, deviceIdHash, clientIp, userAgent, correlationId,
                                new { failedCount = newFailedCount }
                            );

                            result.Success = false;
                            if (newLockout.HasValue)
                            {
                                result.IsLocked = true;
                                result.IsLockedOut = true;
                                result.LockoutMinutes = policy.LOCKOUT_DURATION_MINUTES;
                                result.LockoutRemainingMinutes = policy.LOCKOUT_DURATION_MINUTES;
                                result.ErrorMessage = $"Tài khoản tạm thời bị khóa do nhập sai quá {policy.MAX_FAILED_LOGIN_ATTEMPTS} lần. Vui lòng thử lại sau {policy.LOCKOUT_DURATION_MINUTES} phút.";
                            }
                            else
                            {
                                result.ErrorMessage = "Mã đăng nhập hoặc mật khẩu không chính xác.";
                            }
                            transaction.Commit();
                            return result;
                        }

                        // 6. Kiểm tra quyền nền tảng qua PlatformAccessResolver (Zero-Trust)
                        if (!_platformResolver.CanLoginChannel(db, lockedUser.IDUSER, clientType, out string platformErrCode, out string platformErrMsg, lockedUser.USERNAME))
                        {
                            await _auditService.RecordLoginAttemptAsync(
                                lockedUser.IDUSER, input, clientType, deviceIdHash, clientIp, userAgent,
                                false, platformErrCode, correlationId
                            );
                            await _auditService.LogEventAsync(
                                lockedUser.IDUSER, null, null, AuthAuditEvents.LoginFailed, "DENIED",
                                platformErrCode, clientType, deviceIdHash, clientIp, userAgent, correlationId,
                                new { channel = clientType, reason = platformErrMsg }
                            );

                            result.Success = false;
                            result.FailureReason = platformErrCode;
                            result.ErrorMessage = platformErrMsg;
                            transaction.Commit();
                            return result;
                        }

                        // Kiểm tra IP Whitelist nếu tài khoản có cấu hình
                        var allowedIpsStr = db.Database.SqlQuery<string>(
                            "SELECT ALLOWED_IPS FROM HR.TB_SYS_USER WHERE IDUSER = :id",
                            new OracleParameter("id", lockedUser.IDUSER)
                        ).FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(allowedIpsStr))
                        {
                            var allowedList = allowedIpsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                            if (!allowedList.Contains(clientIp))
                            {
                                await _auditService.RecordLoginAttemptAsync(
                                    lockedUser.IDUSER, input, clientType, deviceIdHash, clientIp, userAgent,
                                    false, "IP_NOT_ALLOWED", correlationId
                                );
                                result.Success = false;
                                result.FailureReason = "IP_NOT_ALLOWED";
                                result.ErrorMessage = "Địa chỉ IP hiện tại không được phép đăng nhập vào tài khoản này.";
                                transaction.Commit();
                                return result;
                            }
                        }

                        // 7. Mật khẩu đúng -> Reset Failed Login Counter
                        db.Database.ExecuteSqlCommand(@"
                            UPDATE HR.TB_SYS_USER
                            SET FAILED_LOGIN_COUNT = 0,
                                LOCKOUT_END = NULL,
                                FIRST_FAILED_LOGIN_AT = NULL,
                                LOCK_REASON = NULL,
                                LAST_SUCCESS_LOGIN_AT = CURRENT_TIMESTAMP
                            WHERE IDUSER = :p0",
                            new OracleParameter("p0", lockedUser.IDUSER)
                        );

                        // 8. Quản Lý Session Quota: Kiểm tra số session đang hoạt động
                        var activeSessions = db.Database.SqlQuery<ActiveSessionRow>(@"
                            SELECT SESSION_ID, JTI, LAST_USED_AT, CREATED_AT
                            FROM HR.TB_AUTH_SESSION
                            WHERE USER_ID = :p0 AND REVOKED_AT IS NULL AND EXPIRES_AT > CURRENT_TIMESTAMP
                            ORDER BY LAST_USED_AT ASC, CREATED_AT ASC",
                            new OracleParameter("p0", lockedUser.IDUSER)
                        ).ToList();

                        if (activeSessions.Count >= policy.MAX_ACTIVE_SESSIONS)
                        {
                            if (policy.SESSION_LIMIT_STRATEGY == SessionLimitStrategies.RevokeOldest)
                            {
                                int needToRevoke = activeSessions.Count - policy.MAX_ACTIVE_SESSIONS + 1;
                                var sessionsToRevoke = activeSessions.Take(needToRevoke).ToList();

                                foreach (var oldSess in sessionsToRevoke)
                                {
                                    db.Database.ExecuteSqlCommand(@"
                                        UPDATE HR.TB_AUTH_SESSION
                                        SET REVOKED_AT = CURRENT_TIMESTAMP,
                                            REVOKE_REASON = :p0
                                        WHERE SESSION_ID = :p1",
                                        new OracleParameter("p0", AuthRevokeReasons.SessionLimit),
                                        new OracleParameter("p1", oldSess.SESSION_ID)
                                    );

                                    await _auditService.LogEventAsync(
                                        lockedUser.IDUSER, lockedUser.IDUSER, oldSess.SESSION_ID,
                                        AuthAuditEvents.SessionRevoked, "SUCCESS",
                                        $"Revoked due to session quota limit ({policy.MAX_ACTIVE_SESSIONS})",
                                        clientType, deviceIdHash, clientIp, userAgent, correlationId
                                    );
                                }

                                await _auditService.LogEventAsync(
                                    lockedUser.IDUSER, lockedUser.IDUSER, null,
                                    AuthAuditEvents.SessionLimitReached, "SUCCESS",
                                    $"Session limit ({policy.MAX_ACTIVE_SESSIONS}) reached. Oldest session revoked.",
                                    clientType, deviceIdHash, clientIp, userAgent, correlationId
                                );
                            }
                            else
                            {
                                // REJECT_NEW
                                await _auditService.LogEventAsync(
                                    lockedUser.IDUSER, lockedUser.IDUSER, null,
                                    AuthAuditEvents.SessionLimitReached, "DENIED",
                                    $"Session limit ({policy.MAX_ACTIVE_SESSIONS}) reached. New login rejected.",
                                    clientType, deviceIdHash, clientIp, userAgent, correlationId
                                );

                                result.Success = false;
                                result.ErrorMessage = $"Tài khoản đã đạt giới hạn tối đa {policy.MAX_ACTIVE_SESSIONS} phiên đăng nhập đồng thời. Vui lòng đăng xuất ở thiết bị khác trước.";
                                transaction.Commit();
                                return result;
                            }
                        }

                        // 9. Tạo Session Mới
                        string sessionId = Guid.NewGuid().ToString("N");
                        string jti = Guid.NewGuid().ToString("N");
                        DateTime sessionExpiresAt = now.AddMinutes(policy.ACCESS_TOKEN_MINUTES);

                        db.Database.ExecuteSqlCommand(@"
                            INSERT INTO HR.TB_AUTH_SESSION (
                                SESSION_ID, USER_ID, JTI, CLIENT_TYPE, PLATFORM,
                                DEVICE_ID_HASH, DEVICE_NAME, IP_ADDRESS, USER_AGENT,
                                CREATED_AT, LAST_USED_AT, EXPIRES_AT, REVOKED_AT, REVOKE_REASON
                            ) VALUES (
                                :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8,
                                CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, :p9, NULL, NULL
                            )",
                            new OracleParameter("p0", sessionId),
                            new OracleParameter("p1", lockedUser.IDUSER),
                            new OracleParameter("p2", jti),
                            new OracleParameter("p3", clientType),
                            new OracleParameter("p4", platform),
                            new OracleParameter("p5", (object)deviceIdHash ?? DBNull.Value),
                            new OracleParameter("p6", (object)deviceName ?? DBNull.Value),
                            new OracleParameter("p7", (object)clientIp ?? DBNull.Value),
                            new OracleParameter("p8", (object)userAgent ?? DBNull.Value),
                            new OracleParameter("p9", sessionExpiresAt)
                        );

                        // 10. Ghi Audit Đăng Nhập Thành Công
                        await _auditService.RecordLoginAttemptAsync(
                            lockedUser.IDUSER, input, clientType, deviceIdHash, clientIp, userAgent,
                            true, null, correlationId
                        );

                        await _auditService.LogEventAsync(
                            lockedUser.IDUSER, lockedUser.IDUSER, sessionId,
                            AuthAuditEvents.SessionCreated, "SUCCESS",
                            $"Session created successfully (Expires: {sessionExpiresAt:yyyy-MM-dd HH:mm:ss})",
                            clientType, deviceIdHash, clientIp, userAgent, correlationId
                        );

                        await _auditService.LogEventAsync(
                            lockedUser.IDUSER, lockedUser.IDUSER, sessionId,
                            AuthAuditEvents.LoginSuccess, "SUCCESS",
                            "Login completed successfully",
                            clientType, deviceIdHash, clientIp, userAgent, correlationId
                        );

                        // Đồng bộ lịch sử đăng nhập truyền thống TB_SYS_LOGIN_HISTORY
                        try
                        {
                            db.Database.ExecuteSqlCommand(@"
                                INSERT INTO HR.TB_SYS_LOGIN_HISTORY (ID_USER, THOIGIAN, IP_ADDRESS, TRANGTHAI)
                                VALUES (:p0, CURRENT_TIMESTAMP, :p1, :p2)",
                                new OracleParameter("p0", lockedUser.IDUSER),
                                new OracleParameter("p1", (object)clientIp ?? "127.0.0.1"),
                                new OracleParameter("p2", "SUCCESS")
                            );
                        }
                        catch { }

                        // Detailed Rights theo kênh và bảng phân quyền thực tế (Zero-Trust, loại bỏ giả lập quyền ảo cho ADMIN)
                        Dictionary<string, Bu.DTO.UserRightDetail> detailedRights = null;
                        List<string> projectedViewRights = null;
                        if (!string.IsNullOrEmpty(normChannel))
                        {
                            try
                            {
                                var channelTree = _channelResolver.ResolveChannelTree(db, lockedUser.IDUSER, normChannel, lockedUser.USERNAME);
                                if (channelTree != null)
                                {
                                    _channelResolver.ProjectEffectiveRights(channelTree, out detailedRights, out projectedViewRights);
                                }
                            }
                            catch (Exception chEx)
                            {
                                System.Diagnostics.Trace.TraceWarning("[AuthSecurityService.LoginAsync] ResolveChannelTree error: " + chEx.Message);
                                detailedRights = null;
                                projectedViewRights = null;
                            }
                        }

                        // Chỉ fallback sang legacy table khi không chỉ định kênh hoặc resolver gặp lỗi (chưa có schema channel)
                        if (detailedRights == null)
                        {
                            var userBus = new SYS_USER();
                            detailedRights = userBus.GetDetailedRights(lockedUser.IDUSER);
                        }

                        // Load Rights
                        var freshRights = new List<string>();
                        if (isRootAdmin)
                        {
                            freshRights.Add("*");
                            if (detailedRights != null && detailedRights.Any())
                            {
                                freshRights.AddRange(detailedRights.Where(kv => kv.Value.CAN_VIEW).Select(kv => kv.Key));
                            }
                            else
                            {
                                var funcCodes = db.TB_SYS_FUNCTION.Select(f => f.FUNCTION_CODE).ToList();
                                freshRights.AddRange(funcCodes);
                            }
                        }
                        else
                        {
                            if (detailedRights != null && detailedRights.Any())
                            {
                                freshRights.AddRange(detailedRights.Where(kv => kv.Value.CAN_VIEW).Select(kv => kv.Key));
                            }
                            else
                            {
                                var direct = db.TB_SYS_RIGHT.Where(r => r.IDUSER == lockedUser.IDUSER && (r.CAN_VIEW == 1 || r.USER_RIGHT == 1)).Select(r => r.FUNCTION_CODE).ToList();
                                freshRights.AddRange(direct);
                                var groupIds = db.TB_SYS_GROUP.Where(g => g.MEMBER == lockedUser.IDUSER).Select(g => g.ID_GROUP).ToList();
                                if (groupIds.Any())
                                {
                                    var groupRights = db.TB_SYS_RIGHT.Where(r => groupIds.Contains(r.IDUSER) && (r.CAN_VIEW == 1 || r.USER_RIGHT == 1)).Select(r => r.FUNCTION_CODE).ToList();
                                    freshRights.AddRange(groupRights);
                                }
                            }
                        }
                        var distinctRights = freshRights.Distinct().ToList();

                        // Employee mapping
                        decimal? freshManv = isRootAdmin ? null : lockedUser.MANV;
                        string freshEmpCode = null;
                        bool freshMobileEnabled = !isRootAdmin;

                        if (!isRootAdmin)
                        {
                            try
                            {
                                var mapping = db.Database.SqlQuery<MappingRow>(
                                    "SELECT USER_ID, EMPLOYEE_ID, IS_MOBILE_ENABLED FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0 AND ROWNUM = 1",
                                    new OracleParameter("p0", lockedUser.IDUSER)
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

                        transaction.Commit();

                        result.Success = true;
                        result.SessionId = sessionId;
                        result.Jti = jti;
                        result.UserId = lockedUser.IDUSER;
                        result.Username = lockedUser.USERNAME;
                        result.FullName = lockedUser.FULLNAME ?? lockedUser.USERNAME;
                        result.IsAdmin = isRootAdmin;
                        result.Rights = distinctRights;
                        result.DetailedRights = detailedRights;
                        result.MaCty = lockedUser.MACTY;
                        result.MaDvi = lockedUser.MADVI;
                        result.Manv = freshManv;
                        result.EmployeeCode = freshEmpCode;
                        result.IsMobileEnabled = freshMobileEnabled;
                        result.ClientType = clientType;
                        result.TokenVersion = (long)lockedUser.TOKEN_VERSION;
                        result.ExpiresAt = sessionExpiresAt;

                        // Chuẩn bị thông tin trả về
                        result.User = new
                        {
                            IdUser = (int)lockedUser.IDUSER,
                            id = (int)lockedUser.IDUSER,
                            Username = lockedUser.USERNAME,
                            username = lockedUser.USERNAME,
                            FullName = lockedUser.FULLNAME ?? lockedUser.USERNAME,
                            fullName = lockedUser.FULLNAME ?? lockedUser.USERNAME,
                            IsAdmin = isRootAdmin,
                            isAdmin = isRootAdmin,
                            Rights = distinctRights,
                            rights = distinctRights,
                            DetailedRights = detailedRights,
                            detailedRights = detailedRights,
                            manv = freshManv,
                            Manv = freshManv,
                            employeeCode = freshEmpCode,
                            EmployeeCode = freshEmpCode,
                            isMobileEnabled = freshMobileEnabled,
                            IsMobileEnabled = freshMobileEnabled,
                            clientType = clientType,
                            ClientType = clientType,
                            MaCty = lockedUser.MACTY,
                            MaDvi = lockedUser.MADVI,
                            TokenVersion = (long)lockedUser.TOKEN_VERSION
                        };

                        return result;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Trace.TraceError("[AuthSecurityService] AuthenticateAsync error: " + ex);
                        result.Success = false;
                        result.ErrorMessage = "Lỗi hệ thống khi xác thực đăng nhập: " + ex.Message;
                        return result;
                    }
                }
            }
        }

        public async Task<bool> ValidateSessionAsync(string jti, long tokenVersion, decimal userId)
        {
            return await ValidateSessionAsync(jti, tokenVersion, userId, null);
        }

        public async Task<bool> ValidateSessionAsync(string jti, long tokenVersion, decimal userId, string expectedChannel)
        {
            if (string.IsNullOrWhiteSpace(jti) || userId <= 0 || tokenVersion <= 0) return false;

            try
            {
                using (var db = new MyEntities())
                {
                    // 1. Kiểm tra Security Version và trạng thái User
                    var userState = await db.Database.SqlQuery<UserStateRow>(@"
                        SELECT IDUSER, DISABLED, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION, LOCKOUT_END
                        FROM HR.TB_SYS_USER
                        WHERE IDUSER = :p0",
                        new OracleParameter("p0", userId)
                    ).FirstOrDefaultAsync();

                    if (userState == null) return false;
                    if (userState.DISABLED.HasValue && userState.DISABLED.Value == 1) return false;
                    if (userState.LOCKOUT_END.HasValue && userState.LOCKOUT_END.Value > DateTime.Now) return false;
                    if (userState.TOKEN_VERSION != tokenVersion) return false; // Immediate Global Revocation

                    // 2. Kiểm tra Session State trong TB_AUTH_SESSION
                    var sessionState = await db.Database.SqlQuery<SessionStateRow>(@"
                        SELECT SESSION_ID, USER_ID, JTI, CLIENT_TYPE, EXPIRES_AT, REVOKED_AT, LAST_USED_AT, CREATED_AT
                        FROM HR.TB_AUTH_SESSION
                        WHERE JTI = :p0 AND USER_ID = :p1",
                        new OracleParameter("p0", jti.Trim()),
                        new OracleParameter("p1", userId)
                    ).FirstOrDefaultAsync();

                    if (sessionState == null) return false;
                    if (sessionState.REVOKED_AT.HasValue) return false; // Immediate Targeted Revocation
                    if (sessionState.EXPIRES_AT <= DateTime.Now) return false; // Expired

                    // Kiểm tra kênh mong đợi nếu có
                    if (!string.IsNullOrWhiteSpace(expectedChannel))
                    {
                        string normExpected = AppChannels.Normalize(expectedChannel);
                        if (normExpected != null && !string.Equals(sessionState.CLIENT_TYPE?.Trim(), normExpected, StringComparison.OrdinalIgnoreCase))
                        {
                            return false; // Kênh của session không khớp với kênh request
                        }
                    }

                    // 3. Kiểm tra Idle Timeout (480 phút) và Absolute Timeout (1440 phút)
                    DateTime now = DateTime.Now;
                    if ((now - sessionState.LAST_USED_AT).TotalMinutes > 480.0)
                    {
                        try
                        {
                            await db.Database.ExecuteSqlCommandAsync(@"
                                UPDATE HR.TB_AUTH_SESSION
                                SET REVOKED_AT = CURRENT_TIMESTAMP, REVOKE_REASON = 'IDLE_TIMEOUT'
                                WHERE JTI = :p0",
                                new OracleParameter("p0", jti.Trim())
                            );
                        }
                        catch { }
                        return false;
                    }

                    if ((now - sessionState.CREATED_AT).TotalMinutes > 1440.0)
                    {
                        try
                        {
                            await db.Database.ExecuteSqlCommandAsync(@"
                                UPDATE HR.TB_AUTH_SESSION
                                SET REVOKED_AT = CURRENT_TIMESTAMP, REVOKE_REASON = 'ABSOLUTE_TIMEOUT'
                                WHERE JTI = :p0",
                                new OracleParameter("p0", jti.Trim())
                            );
                        }
                        catch { }
                        return false;
                    }

                    // 4. Cập nhật LAST_USED_AT có throttling (chỉ update nếu đã qua hơn 1 phút)
                    if ((now - sessionState.LAST_USED_AT).TotalMinutes >= 1.0)
                    {
                        try
                        {
                            await db.Database.ExecuteSqlCommandAsync(@"
                                UPDATE HR.TB_AUTH_SESSION
                                SET LAST_USED_AT = CURRENT_TIMESTAMP
                                WHERE JTI = :p0",
                                new OracleParameter("p0", jti.Trim())
                            );
                        }
                        catch { }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[AuthSecurityService] ValidateSessionAsync error: " + ex.Message);
                return false;
            }
        }

        public bool ValidateSession(string jti, long tokenVersion, decimal userId)
        {
            return ValidateSession(jti, tokenVersion, userId, null);
        }

        public bool ValidateSession(string jti, long tokenVersion, decimal userId, string expectedChannel)
        {
            if (string.IsNullOrWhiteSpace(jti) || userId <= 0 || tokenVersion <= 0) return false;

            try
            {
                using (var db = new MyEntities())
                {
                    // 1. Kiểm tra Security Version và trạng thái User
                    var userState = db.Database.SqlQuery<UserStateRow>(@"
                        SELECT IDUSER, DISABLED, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION, LOCKOUT_END
                        FROM HR.TB_SYS_USER
                        WHERE IDUSER = :p0",
                        new OracleParameter("p0", userId)
                    ).FirstOrDefault();

                    if (userState == null) return false;
                    if (userState.DISABLED.HasValue && userState.DISABLED.Value == 1) return false;
                    if (userState.LOCKOUT_END.HasValue && userState.LOCKOUT_END.Value > DateTime.Now) return false;
                    if (userState.TOKEN_VERSION != tokenVersion) return false;

                    // 2. Kiểm tra Session State trong TB_AUTH_SESSION
                    var sessionState = db.Database.SqlQuery<SessionStateRow>(@"
                        SELECT SESSION_ID, USER_ID, JTI, CLIENT_TYPE, EXPIRES_AT, REVOKED_AT, LAST_USED_AT, CREATED_AT
                        FROM HR.TB_AUTH_SESSION
                        WHERE JTI = :p0 AND USER_ID = :p1",
                        new OracleParameter("p0", jti.Trim()),
                        new OracleParameter("p1", userId)
                    ).FirstOrDefault();

                    if (sessionState == null) return false;
                    if (sessionState.REVOKED_AT.HasValue) return false; // Immediate Targeted Revocation
                    if (sessionState.EXPIRES_AT <= DateTime.Now) return false; // Expired

                    // Kiểm tra kênh mong đợi nếu có
                    if (!string.IsNullOrWhiteSpace(expectedChannel))
                    {
                        string normExpected = AppChannels.Normalize(expectedChannel);
                        if (normExpected != null && !string.Equals(sessionState.CLIENT_TYPE?.Trim(), normExpected, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }

                    // 3. Kiểm tra Idle Timeout và Absolute Timeout
                    DateTime now = DateTime.Now;
                    if ((now - sessionState.LAST_USED_AT).TotalMinutes > 480.0)
                    {
                        try
                        {
                            db.Database.ExecuteSqlCommand(@"
                                UPDATE HR.TB_AUTH_SESSION
                                SET REVOKED_AT = CURRENT_TIMESTAMP, REVOKE_REASON = 'IDLE_TIMEOUT'
                                WHERE JTI = :p0",
                                new OracleParameter("p0", jti.Trim())
                            );
                        }
                        catch { }
                        return false;
                    }

                    if ((now - sessionState.CREATED_AT).TotalMinutes > 1440.0)
                    {
                        try
                        {
                            db.Database.ExecuteSqlCommand(@"
                                UPDATE HR.TB_AUTH_SESSION
                                SET REVOKED_AT = CURRENT_TIMESTAMP, REVOKE_REASON = 'ABSOLUTE_TIMEOUT'
                                WHERE JTI = :p0",
                                new OracleParameter("p0", jti.Trim())
                            );
                        }
                        catch { }
                        return false;
                    }

                    // 4. Cập nhật LAST_USED_AT có throttling
                    if ((now - sessionState.LAST_USED_AT).TotalMinutes >= 1.0)
                    {
                        try
                        {
                            db.Database.ExecuteSqlCommand(@"
                                UPDATE HR.TB_AUTH_SESSION
                                SET LAST_USED_AT = CURRENT_TIMESTAMP
                                WHERE JTI = :p0",
                                new OracleParameter("p0", jti.Trim())
                            );
                        }
                        catch { }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[AuthSecurityService] ValidateSession error: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> RevokeSessionAsync(string sessionIdOrJti, string reason, decimal actorUserId, string correlationId)
        {
            if (string.IsNullOrWhiteSpace(sessionIdOrJti)) return false;

            try
            {
                using (var db = new MyEntities())
                {
                    string trimmed = sessionIdOrJti.Trim();
                    var session = await db.Database.SqlQuery<SessionStateRow>(@"
                        SELECT SESSION_ID, USER_ID, JTI, EXPIRES_AT, REVOKED_AT, LAST_USED_AT
                        FROM HR.TB_AUTH_SESSION
                        WHERE (SESSION_ID = :p0 OR JTI = :p1) AND REVOKED_AT IS NULL",
                        new OracleParameter("p0", trimmed),
                        new OracleParameter("p1", trimmed)
                    ).FirstOrDefaultAsync();

                    if (session == null) return true; // Idempotent

                    await db.Database.ExecuteSqlCommandAsync(@"
                        UPDATE HR.TB_AUTH_SESSION
                        SET REVOKED_AT = CURRENT_TIMESTAMP,
                            REVOKE_REASON = :p0
                        WHERE SESSION_ID = :p1",
                        new OracleParameter("p0", reason ?? AuthRevokeReasons.Logout),
                        new OracleParameter("p1", session.SESSION_ID)
                    );

                    await _auditService.LogEventAsync(
                        session.USER_ID, actorUserId > 0 ? (decimal?)actorUserId : session.USER_ID,
                        session.SESSION_ID,
                        reason == AuthRevokeReasons.Logout ? AuthAuditEvents.Logout : AuthAuditEvents.SessionRevoked,
                        "SUCCESS", reason, null, null, null, null, correlationId
                    );

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AuthSecurityService] RevokeSessionAsync error: " + ex);
                return false;
            }
        }

        public async Task<int> RevokeAllSessionsAsync(decimal userId, string reason, bool incrementSecurityVersion, decimal actorUserId, string correlationId)
        {
            if (userId <= 0) return 0;

            try
            {
                using (var db = new MyEntities())
                {
                    using (var tx = db.Database.BeginTransaction())
                    {
                        int revokedCount = await db.Database.ExecuteSqlCommandAsync(@"
                            UPDATE HR.TB_AUTH_SESSION
                            SET REVOKED_AT = CURRENT_TIMESTAMP,
                                REVOKE_REASON = :p0
                            WHERE USER_ID = :p1 AND REVOKED_AT IS NULL",
                            new OracleParameter("p0", reason ?? AuthRevokeReasons.LogoutAll),
                            new OracleParameter("p1", userId)
                        );

                        if (incrementSecurityVersion)
                        {
                            await db.Database.ExecuteSqlCommandAsync(@"
                                UPDATE HR.TB_SYS_USER
                                SET TOKEN_VERSION = NVL(TOKEN_VERSION, 1) + 1
                                WHERE IDUSER = :p0",
                                new OracleParameter("p0", userId)
                            );

                            await _auditService.LogEventAsync(
                                userId, actorUserId > 0 ? (decimal?)actorUserId : userId,
                                null, AuthAuditEvents.SecurityVersionChanged, "SUCCESS",
                                "Token version incremented to invalidate all active JWTs",
                                null, null, null, null, correlationId
                            );
                        }

                        await _auditService.LogEventAsync(
                            userId, actorUserId > 0 ? (decimal?)actorUserId : userId,
                            null, AuthAuditEvents.LogoutAll, "SUCCESS",
                            $"Revoked {revokedCount} active sessions ({reason})",
                            null, null, null, null, correlationId
                        );

                        tx.Commit();
                        return revokedCount;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AuthSecurityService] RevokeAllSessionsAsync error: " + ex);
                return 0;
            }
        }

        public async Task<int> RevokePlatformSessionsAsync(decimal userId, string channel, string reason, decimal actorUserId, string correlationId)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(channel)) return 0;

            try
            {
                using (var db = new MyEntities())
                {
                    string normChannel = AppChannels.Normalize(channel);
                    if (normChannel == null) return 0;

                    int revokedCount = await db.Database.ExecuteSqlCommandAsync(@"
                        UPDATE HR.TB_AUTH_SESSION
                        SET REVOKED_AT = CURRENT_TIMESTAMP,
                            REVOKE_REASON = :p0
                        WHERE USER_ID = :p1 
                          AND UPPER(TRIM(CLIENT_TYPE)) = :p2 
                          AND REVOKED_AT IS NULL 
                          AND EXPIRES_AT > CURRENT_TIMESTAMP",
                        new OracleParameter("p0", reason ?? AuthRevokeReasons.PlatformAccessRemoved),
                        new OracleParameter("p1", userId),
                        new OracleParameter("p2", normChannel)
                    );

                    await _auditService.LogEventAsync(
                        userId, actorUserId > 0 ? (decimal?)actorUserId : userId,
                        null, AuthAuditEvents.SessionRevoked, "SUCCESS",
                        $"Revoked {revokedCount} active sessions on channel {normChannel} ({reason})",
                        normChannel, null, null, null, correlationId
                    );

                    return revokedCount;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AuthSecurityService] RevokePlatformSessionsAsync error: " + ex);
                return 0;
            }
        }

        public async Task<ChangePasswordResultDto> ChangePasswordWithRevocationAsync(
            decimal userId,
            string oldPassword,
            string newPassword,
            string clientIp,
            string userAgent,
            string correlationId)
        {
            var res = new ChangePasswordResultDto();
            if (userId <= 0 || string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                res.Success = false;
                res.Message = "Vui lòng nhập mật khẩu hiện tại và mật khẩu mới.";
                return res;
            }

            using (var db = new MyEntities())
            {
                using (var tx = db.Database.BeginTransaction())
                {
                    try
                    {
                        var user = db.Database.SqlQuery<UserPasswordRow>(@"
                            SELECT IDUSER, PASSWORD, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION
                            FROM HR.TB_SYS_USER
                            WHERE IDUSER = :p0
                            FOR UPDATE",
                            new OracleParameter("p0", userId)
                        ).FirstOrDefault();

                        if (user == null)
                        {
                            res.Success = false;
                            res.Message = "Không tìm thấy thông tin tài khoản.";
                            tx.Rollback();
                            return res;
                        }

                        if (!PasswordHasher.VerifyPassword(oldPassword, user.PASSWORD))
                        {
                            res.Success = false;
                            res.Message = "Mật khẩu hiện tại không chính xác.";
                            tx.Rollback();
                            return res;
                        }

                        // 1. Hash mật khẩu mới bằng BCrypt
                        string hashedNewPassword = PasswordHasher.HashPassword(newPassword.Trim());
                        long newTokenVersion = (long)user.TOKEN_VERSION + 1;

                        // 2. Cập nhật mật khẩu và tăng TOKEN_VERSION
                        db.Database.ExecuteSqlCommand(@"
                            UPDATE HR.TB_SYS_USER
                            SET PASSWORD = :p0,
                                TOKEN_VERSION = :p1,
                                LAST_PWD_CHANGED = CURRENT_TIMESTAMP
                            WHERE IDUSER = :p2",
                            new OracleParameter("p0", hashedNewPassword),
                            new OracleParameter("p1", newTokenVersion),
                            new OracleParameter("p2", userId)
                        );

                        // 3. Thu hồi TOÀN BỘ phiên làm việc
                        int revokedCount = db.Database.ExecuteSqlCommand(@"
                            UPDATE HR.TB_AUTH_SESSION
                            SET REVOKED_AT = CURRENT_TIMESTAMP,
                                REVOKE_REASON = :p0
                            WHERE USER_ID = :p1 AND REVOKED_AT IS NULL",
                            new OracleParameter("p0", AuthRevokeReasons.PasswordChanged),
                            new OracleParameter("p1", userId)
                        );

                        // 4. Ghi Audit
                        await _auditService.LogEventAsync(
                            userId, userId, null, AuthAuditEvents.PasswordChanged, "SUCCESS",
                            $"Password updated. Token version bumped to {newTokenVersion}. Revoked {revokedCount} active sessions.",
                            null, null, clientIp, userAgent, correlationId
                        );

                        tx.Commit();

                        res.Success = true;
                        res.Message = "Đổi mật khẩu thành công! Toàn bộ phiên đăng nhập cũ đã được thu hồi.";
                        res.SessionsRevokedCount = revokedCount;
                        return res;
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        System.Diagnostics.Trace.TraceError("[AuthSecurityService] ChangePassword error: " + ex);
                        res.Success = false;
                        res.Message = "Đã xảy ra lỗi khi thực hiện đổi mật khẩu: " + ex.Message;
                        return res;
                    }
                }
            }
        }

        public async Task<List<SessionInfoDto>> GetUserSessionsAsync(decimal userId, string currentJti)
        {
            if (userId <= 0) return new List<SessionInfoDto>();

            try
            {
                using (var db = new MyEntities())
                {
                    var rows = await db.Database.SqlQuery<AuthSessionEntity>(@"
                        SELECT SESSION_ID, USER_ID, JTI, CLIENT_TYPE, PLATFORM,
                               DEVICE_ID_HASH, DEVICE_NAME, IP_ADDRESS, USER_AGENT,
                               CREATED_AT, LAST_USED_AT, EXPIRES_AT, REVOKED_AT, REVOKE_REASON
                        FROM HR.TB_AUTH_SESSION
                        WHERE USER_ID = :p0 AND (REVOKED_AT IS NULL OR REVOKED_AT > CURRENT_TIMESTAMP - INTERVAL '7' DAY)
                        ORDER BY LAST_USED_AT DESC",
                        new OracleParameter("p0", userId)
                    ).ToListAsync();

                    return rows.Select(r => new SessionInfoDto
                    {
                        SessionId = r.SESSION_ID,
                        Jti = r.JTI,
                        ClientType = r.CLIENT_TYPE,
                        Platform = r.PLATFORM,
                        DeviceName = !string.IsNullOrWhiteSpace(r.DEVICE_NAME) ? r.DEVICE_NAME : $"{r.CLIENT_TYPE} ({r.PLATFORM})",
                        IpAddress = r.IP_ADDRESS,
                        UserAgent = r.USER_AGENT,
                        CreatedAt = r.CREATED_AT,
                        LastUsedAt = r.LAST_USED_AT,
                        ExpiresAt = r.EXPIRES_AT,
                        IsCurrent = (!string.IsNullOrWhiteSpace(currentJti) && r.JTI == currentJti),
                        IsRevoked = r.REVOKED_AT.HasValue,
                        RevokeReason = r.REVOKE_REASON
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AuthSecurityService] GetUserSessionsAsync error: " + ex);
                return new List<SessionInfoDto>();
            }
        }

        public async Task<bool> UnlockUserAsync(decimal targetUserId, decimal actorUserId, string correlationId)
        {
            if (targetUserId <= 0) return false;

            try
            {
                using (var db = new MyEntities())
                {
                    db.Database.ExecuteSqlCommand(@"
                        UPDATE HR.TB_SYS_USER
                        SET FAILED_LOGIN_COUNT = 0,
                            LOCKOUT_END = NULL,
                            LOCK_REASON = NULL
                        WHERE IDUSER = :p0",
                        new OracleParameter("p0", targetUserId)
                    );

                    await _auditService.LogEventAsync(
                        targetUserId, actorUserId, null, AuthAuditEvents.AccountUnlocked, "SUCCESS",
                        "User unlocked by admin", null, null, null, null, correlationId
                    );

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AuthSecurityService] UnlockUserAsync error: " + ex);
                return false;
            }
        }

        // Helper private POCOs for internal SQL queries
        private class UserLookupRow
        {
            public decimal IDUSER { get; set; }
            public string USERNAME { get; set; }
            public string PASSWORD { get; set; }
            public decimal? DISABLED { get; set; }
            public decimal? ISGROUP { get; set; }
            public string MACTY { get; set; }
            public string MADVI { get; set; }
            public decimal? MANV { get; set; }
            public string CLIENT_TYPE { get; set; }
            public decimal? FAILED_LOGIN_COUNT { get; set; }
            public DateTime? LOCKOUT_END { get; set; }
            public decimal? TOKEN_VERSION { get; set; }
        }

        private class EmpLookupRow
        {
            public decimal MANV { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public decimal? DATHOIVIEC { get; set; }
            public string HOTEN { get; set; }
        }

        private class UserLockedRow
        {
            public decimal IDUSER { get; set; }
            public string USERNAME { get; set; }
            public string FULLNAME { get; set; }
            public string PASSWORD { get; set; }
            public decimal? DISABLED { get; set; }
            public decimal? ISGROUP { get; set; }
            public string MACTY { get; set; }
            public string MADVI { get; set; }
            public decimal? MANV { get; set; }
            public string CLIENT_TYPE { get; set; }
            public decimal FAILED_LOGIN_COUNT { get; set; }
            public DateTime? LOCKOUT_END { get; set; }
            public decimal TOKEN_VERSION { get; set; }
        }

        private class ActiveSessionRow
        {
            public string SESSION_ID { get; set; }
            public string JTI { get; set; }
            public DateTime LAST_USED_AT { get; set; }
            public DateTime CREATED_AT { get; set; }
        }

        private class UserStateRow
        {
            public decimal IDUSER { get; set; }
            public decimal? DISABLED { get; set; }
            public decimal TOKEN_VERSION { get; set; }
            public DateTime? LOCKOUT_END { get; set; }
        }

        private class SessionStateRow
        {
            public string SESSION_ID { get; set; }
            public decimal USER_ID { get; set; }
            public string JTI { get; set; }
            public string CLIENT_TYPE { get; set; }
            public DateTime EXPIRES_AT { get; set; }
            public DateTime? REVOKED_AT { get; set; }
            public DateTime LAST_USED_AT { get; set; }
            public DateTime CREATED_AT { get; set; }
        }

        private class UserPasswordRow
        {
            public decimal IDUSER { get; set; }
            public string PASSWORD { get; set; }
            public decimal TOKEN_VERSION { get; set; }
        }
    }
}
