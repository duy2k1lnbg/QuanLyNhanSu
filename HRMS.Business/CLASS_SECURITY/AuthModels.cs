using System;
using System.Collections.Generic;

namespace Bu.CLASS_SECURITY
{
    public static class AuthRevokeReasons
    {
        public const string Logout = "LOGOUT";
        public const string LogoutAll = "LOGOUT_ALL";
        public const string UserLogout = "LOGOUT";
        public const string UserLogoutAll = "LOGOUT_ALL";
        public const string UserRevoked = "USER_REVOKED";
        public const string PasswordChanged = "PASSWORD_CHANGED";
        public const string SessionLimit = "SESSION_LIMIT";
        public const string AdminRevoke = "ADMIN_REVOKE";
        public const string ForceLogout = "FORCE_LOGOUT";
        public const string AdminForceLogout = "FORCE_LOGOUT";
        public const string SecurityReset = "SECURITY_RESET";
        public const string AccountLocked = "ACCOUNT_LOCKED";
    }

    public static class AuthAuditEvents
    {
        public const string LoginSuccess = "LOGIN_SUCCESS";
        public const string LoginFailed = "LOGIN_FAILED";
        public const string AccountLocked = "ACCOUNT_LOCKED";
        public const string AccountUnlocked = "ACCOUNT_UNLOCKED";
        public const string SessionCreated = "SESSION_CREATED";
        public const string SessionRevoked = "SESSION_REVOKED";
        public const string SessionExpired = "SESSION_EXPIRED";
        public const string Logout = "LOGOUT";
        public const string LogoutAll = "LOGOUT_ALL";
        public const string PasswordChanged = "PASSWORD_CHANGED";
        public const string PasswordReset = "PASSWORD_RESET";
        public const string ForceLogout = "FORCE_LOGOUT";
        public const string SessionRevokedByAdmin = "SESSION_REVOKED_BY_ADMIN";
        public const string TokenRejected = "TOKEN_REJECTED";
        public const string SecurityVersionChanged = "SECURITY_VERSION_CHANGED";
        public const string SessionLimitReached = "SESSION_LIMIT_REACHED";
    }

    public static class SessionLimitStrategies
    {
        public const string RevokeOldest = "REVOKE_OLDEST";
        public const string RejectNew = "REJECT_NEW";
    }

    public class AuthSessionEntity
    {
        public string SESSION_ID { get; set; }
        public decimal USER_ID { get; set; }
        public string JTI { get; set; }
        public string CLIENT_TYPE { get; set; }
        public string PLATFORM { get; set; }
        public string DEVICE_ID_HASH { get; set; }
        public string DEVICE_NAME { get; set; }
        public string IP_ADDRESS { get; set; }
        public string USER_AGENT { get; set; }
        public DateTime CREATED_AT { get; set; }
        public DateTime LAST_USED_AT { get; set; }
        public DateTime EXPIRES_AT { get; set; }
        public DateTime? REVOKED_AT { get; set; }
        public string REVOKE_REASON { get; set; }
    }

    public class AuthPolicyEntity
    {
        public decimal POLICY_ID { get; set; }
        public string SCOPE_TYPE { get; set; }
        public string SCOPE_ID { get; set; }
        public int MAX_ACTIVE_SESSIONS { get; set; } = 2;
        public string SESSION_LIMIT_STRATEGY { get; set; } = SessionLimitStrategies.RevokeOldest;
        public int ACCESS_TOKEN_MINUTES { get; set; } = 60;
        public int IDLE_TIMEOUT_MINUTES { get; set; } = 480;
        public int ABSOLUTE_TIMEOUT_MINUTES { get; set; } = 1440;
        public int MAX_FAILED_LOGIN_ATTEMPTS { get; set; } = 5;
        public int FAILED_ATTEMPT_WINDOW_MINUTES { get; set; } = 15;
        public int LOCKOUT_DURATION_MINUTES { get; set; } = 15;
        public int ENABLED { get; set; } = 1;
        public DateTime CREATED_AT { get; set; }
        public DateTime UPDATED_AT { get; set; }
    }

    public class SessionInfoDto
    {
        public string SessionId { get; set; }
        public string Jti { get; set; }
        public string ClientType { get; set; }
        public string Platform { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsRevoked { get; set; }
        public string RevokeReason { get; set; }
    }

    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string Token { get; set; }
        public object User { get; set; }
        public bool IsLocked { get; set; }
        public bool IsLockedOut { get; set; }
        public int LockoutMinutes { get; set; }
        public int LockoutRemainingMinutes { get; set; }
        public string FailureReason { get; set; }
        public string SessionId { get; set; }
        public string Jti { get; set; }

        public decimal UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
        public List<string> Rights { get; set; } = new List<string>();
        public Dictionary<string, Bu.DTO.UserRightDetail> DetailedRights { get; set; }
        public string MaCty { get; set; }
        public string MaDvi { get; set; }
        public decimal? Manv { get; set; }
        public string EmployeeCode { get; set; }
        public bool IsMobileEnabled { get; set; }
        public string ClientType { get; set; }
        public long TokenVersion { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class ChangePasswordResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int SessionsRevokedCount { get; set; }
    }

    public class MappingRow
    {
        public decimal? USER_ID { get; set; }
        public decimal? EMPLOYEE_ID { get; set; }
        public decimal? IS_MOBILE_ENABLED { get; set; }
    }
}
