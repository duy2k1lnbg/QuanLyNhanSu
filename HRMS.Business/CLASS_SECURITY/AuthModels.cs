using System;
using System.Collections.Generic;

namespace Bu.CLASS_SECURITY
{
    public static class AppChannels
    {
        public const string Desktop = "DESKTOP";
        public const string Web = "WEB";
        public const string Mobile = "MOBILE";

        public static bool IsValid(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel)) return false;
            string upper = channel.Trim().ToUpperInvariant();
            return upper == Desktop || upper == Web || upper == Mobile;
        }

        public static string Normalize(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel)) return null;
            string upper = channel.Trim().ToUpperInvariant();
            if (upper == Desktop) return Desktop;
            if (upper == Web) return Web;
            if (upper == Mobile) return Mobile;
            return null;
        }

        public static string GetLabel(string channel)
        {
            string norm = Normalize(channel);
            if (norm == Desktop) return "Desktop WinForms";
            if (norm == Web) return "Web Portal";
            if (norm == Mobile) return "Mobile App";
            return channel;
        }
    }

    public static class PlatformFunctionCodes
    {
        public const string LoginDesktop = "F_LOGIN_DESKTOP";
        public const string LoginWeb = "F_LOGIN_WEB";
        public const string LoginMobile = "F_LOGIN_MOBILE";

        public static bool IsPlatformCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            string upper = code.Trim().ToUpperInvariant();
            return upper == LoginDesktop || upper == LoginWeb || upper == LoginMobile;
        }

        public static bool IsPlatformFunction(string code) => IsPlatformCode(code);

        public static string GetFunctionCodeForChannel(string channel)
        {
            string norm = AppChannels.Normalize(channel);
            if (norm == AppChannels.Desktop) return LoginDesktop;
            if (norm == AppChannels.Web) return LoginWeb;
            if (norm == AppChannels.Mobile) return LoginMobile;
            return null;
        }
    }

    public static class PlatformErrorCodes
    {
        public const string PlatformAccessDenied = "PLATFORM_ACCESS_DENIED";
        public const string EmployeeLinkRequired = "EMPLOYEE_LINK_REQUIRED";
        public const string EmployeeLinkConflict = "EMPLOYEE_LINK_CONFLICT";
        public const string MobileDisabled = "MOBILE_DISABLED";
        public const string SessionRevoked = "SESSION_REVOKED";
        public const string AuthServiceUnavailable = "AUTH_SERVICE_UNAVAILABLE";
        public const string ConcurrentAccessChange = "CONCURRENT_ACCESS_CHANGE";

        public static string GetFriendlyMessage(string errorCode, string channel = null)
        {
            switch (errorCode)
            {
                case PlatformAccessDenied:
                    string chName = channel == AppChannels.Desktop ? "Desktop" : (channel == AppChannels.Web ? "Website" : (channel == AppChannels.Mobile ? "Mobile" : "hệ thống"));
                    return $"Tài khoản chưa được cấp quyền đăng nhập {chName}. Vui lòng liên hệ quản trị viên.";
                case EmployeeLinkRequired:
                    return "Tài khoản chưa được liên kết với hồ sơ nhân viên.";
                case EmployeeLinkConflict:
                    return "Liên kết hồ sơ nhân viên chưa nhất quán. Vui lòng liên hệ quản trị viên.";
                case MobileDisabled:
                    return "Quyền sử dụng Mobile đang được tạm dừng.";
                case SessionRevoked:
                    return "Phiên đăng nhập đã bị thu hồi. Vui lòng đăng nhập lại.";
                case AuthServiceUnavailable:
                    return "Chưa kiểm tra được quyền truy cập. Vui lòng thử lại sau.";
                case ConcurrentAccessChange:
                    return "Thông tin quyền đã được thay đổi. Vui lòng tải lại trước khi lưu.";
                default:
                    return "Từ chối truy cập quyền nền tảng.";
            }
        }
    }

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
        public const string PlatformAccessRemoved = "PLATFORM_ACCESS_REMOVED";
        public const string MobileDisabled = "MOBILE_DISABLED";
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
        public const string PlatformAccessGranted = "PLATFORM_ACCESS_GRANTED";
        public const string PlatformAccessRevoked = "PLATFORM_ACCESS_REVOKED";
        public const string MobileToggled = "MOBILE_TOGGLED";
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

    public class PlatformAccessItemDto
    {
        public string Channel { get; set; }
        public string FunctionCode { get; set; }
        public string ChannelLabel { get; set; }
        public bool DirectGrant { get; set; }
        public bool InheritedGrant { get; set; }
        public List<string> InheritedFromGroups { get; set; } = new List<string>();
        public bool IsEffective { get; set; }
        public string ReadinessCode { get; set; } // READY, MISSING_MAPPING, MOBILE_DISABLED, ACCOUNT_DISABLED, LOCKED_OUT
        public string ReadinessMessage { get; set; }
    }

    public class UserPlatformSummaryDto
    {
        public decimal UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public bool IsGroup { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsLockedOut { get; set; }
        public decimal? Manv { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public bool IsMobileEnabled { get; set; }
        public List<PlatformAccessItemDto> Platforms { get; set; } = new List<PlatformAccessItemDto>();
    }

    public class SavePlatformRightsRequest
    {
        public decimal TargetUserId { get; set; }
        public bool DesktopDirectGrant { get; set; }
        public bool WebDirectGrant { get; set; }
        public bool MobileDirectGrant { get; set; }
        public bool? IsMobileEnabled { get; set; }
        public decimal ActorUserId { get; set; }
        public string CorrelationId { get; set; }
    }

    public class SavePlatformRightsResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int SessionsRevokedCount { get; set; }
        public UserPlatformSummaryDto UpdatedSummary { get; set; }
    }

    // =========================================================================
    // DTOs MỤC 13 - 14: QUYỀN CHA - CON VÀ PHÂN BIỆT THEO KÊNH (DESKTOP, WEB, MOBILE)
    // =========================================================================

    public class FunctionActionGrantDto
    {
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPrint { get; set; }
    }

    public class ChannelFunctionRightItemDto
    {
        public string FunctionCode { get; set; }
        public string FunctionName { get; set; }
        public string ParentCode { get; set; }
        public string RightType { get; set; } // LOGIN, FUNCTION, CATEGORY
        public decimal Sort { get; set; }

        // Khả năng hỗ trợ của kênh từ ChannelCapabilityRegistry
        public FunctionActionGrantDto SupportedCapabilities { get; set; } = new FunctionActionGrantDto();
        public string RestrictionNote { get; set; }

        // Quyền trực tiếp
        public FunctionActionGrantDto DirectGrant { get; set; } = new FunctionActionGrantDto();

        // Quyền kế thừa từ nhóm
        public FunctionActionGrantDto InheritedGrant { get; set; } = new FunctionActionGrantDto();
        public List<string> InheritedFromGroupNames { get; set; } = new List<string>();

        // Quyền hiệu lực thực tế (chỉ có giá trị khi quyền cha đang bật)
        public FunctionActionGrantDto EffectiveGrant { get; set; } = new FunctionActionGrantDto();

        // Trạng thái khóa do quyền cha tắt
        public bool IsDisabledByParent { get; set; }
    }

    public class PlatformChannelTreeDto
    {
        public string Channel { get; set; }
        public string ChannelLabel { get; set; }
        public string ParentFunctionCode { get; set; }
        public string ParentFunctionName { get; set; }
        public bool ParentIsEffective { get; set; }
        public bool ParentDirectGrant { get; set; }
        public bool ParentInheritedGrant { get; set; }
        public List<string> ParentInheritedGroupNames { get; set; } = new List<string>();
        public string ReadinessCode { get; set; }
        public string ReadinessMessage { get; set; }

        // Danh sách quyền con thuộc kênh này
        public List<ChannelFunctionRightItemDto> Functions { get; set; } = new List<ChannelFunctionRightItemDto>();
    }

    public class UserFullChannelPermissionsDto
    {
        public decimal UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public bool IsGroup { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDisabled { get; set; }
        public long SecurityVersion { get; set; } = 1;
        public List<PlatformChannelTreeDto> Channels { get; set; } = new List<PlatformChannelTreeDto>();
    }

    public class SaveChannelFunctionItemRequest
    {
        public string FunctionCode { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanPrint { get; set; }
    }

    public class SaveChannelRightsRequest
    {
        public decimal TargetUserId { get; set; }
        public string Channel { get; set; } // DESKTOP, WEB, MOBILE
        public bool? ParentDirectGrant { get; set; } // Quyền cha F_LOGIN_*
        public List<SaveChannelFunctionItemRequest> Functions { get; set; } = new List<SaveChannelFunctionItemRequest>();
        public decimal ActorUserId { get; set; }
        public string CorrelationId { get; set; }
        public long? ExpectedSecurityVersion { get; set; }
    }

    public class SaveChannelRightsResult
    {
        public bool Success { get; set; }
        public bool IsConflict { get; set; }
        public bool RequiresReLogin { get; set; }
        public string Message { get; set; }
        public long NewSecurityVersion { get; set; }
        public int SessionsRevokedCount { get; set; }
        public PlatformChannelTreeDto UpdatedChannelTree { get; set; }
    }

    public class BatchSaveChannelRightsRequest
    {
        public decimal TargetUserId { get; set; }
        public long? ExpectedSecurityVersion { get; set; }
        public List<SaveChannelRightsRequest> Channels { get; set; } = new List<SaveChannelRightsRequest>();
        public decimal ActorUserId { get; set; }
        public string CorrelationId { get; set; }
    }

    public class BatchSaveChannelRightsResult
    {
        public bool Success { get; set; }
        public bool IsConflict { get; set; }
        public bool RequiresReLogin { get; set; }
        public string Message { get; set; }
        public long NewSecurityVersion { get; set; }
        public int SessionsRevokedCount { get; set; }
        public List<PlatformChannelTreeDto> UpdatedChannelTrees { get; set; } = new List<PlatformChannelTreeDto>();
    }
}

