using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;

namespace Bu.CLASS_SYSTEM
{
    public static class UserSession
    {
        public static TB_SYS_USER CurrentUser { get; set; }
        public static List<string> UserRights { get; set; } = new List<string>();
        public static Dictionary<string, UserRightDetail> DetailedRights { get; set; } = new Dictionary<string, UserRightDetail>(StringComparer.OrdinalIgnoreCase);
        public static decimal CurrentLoginId { get; set; }

        public static string CurrentSessionId { get; set; }
        public static string CurrentJti { get; set; }
        public static long CurrentTokenVersion { get; set; }
        public static string CurrentChannel { get; set; } = "DESKTOP";
        public static string CurrentToken { get; set; }
        public static event Action SessionCleared;

        public static bool IsLoggedIn => CurrentUser != null && !string.IsNullOrWhiteSpace(CurrentSessionId) && !string.IsNullOrWhiteSpace(CurrentJti);

        public static bool ParentDesktopOn
        {
            get
            {
                if (DetailedRights != null && DetailedRights.TryGetValue("F_LOGIN_DESKTOP", out var p))
                {
                    return p.CAN_VIEW;
                }
                return UserRights != null && UserRights.Contains("F_LOGIN_DESKTOP");
            }
            set
            {
                if (DetailedRights == null) DetailedRights = new Dictionary<string, UserRightDetail>(StringComparer.OrdinalIgnoreCase);
                if (DetailedRights.ContainsKey("F_LOGIN_DESKTOP"))
                {
                    DetailedRights["F_LOGIN_DESKTOP"].CAN_VIEW = value;
                }
                else
                {
                    DetailedRights["F_LOGIN_DESKTOP"] = new UserRightDetail { CAN_VIEW = value };
                }
                if (value)
                {
                    if (UserRights == null) UserRights = new List<string>();
                    if (!UserRights.Contains("F_LOGIN_DESKTOP")) UserRights.Add("F_LOGIN_DESKTOP");
                }
                else
                {
                    if (UserRights != null) UserRights.Remove("F_LOGIN_DESKTOP");
                }
            }
        }

        public static bool IsAdmin => CurrentUser != null &&
            CurrentUser.USERNAME != null &&
            CurrentUser.USERNAME.Equals("admin", StringComparison.OrdinalIgnoreCase);

        public static string NormalizeFunctionCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return code;
            if (code.Equals("F_TIENLUONG", StringComparison.OrdinalIgnoreCase)) return "F_CC_BANGLUONG";
            if (code.Equals("F_BANGCONG", StringComparison.OrdinalIgnoreCase)) return "F_CC_BANGCONG";
            if (code.Equals("F_SYSTEM_PURGE", StringComparison.OrdinalIgnoreCase)) return "F_SYSTEM_PHUCHOI";
            return code;
        }

        /// <summary>
        /// Kiểm tra điều kiện tiên quyết của phiên làm việc Desktop:
        /// - Tài khoản tồn tại và không bị disabled
        /// - Phiên làm việc hợp lệ (có SessionId và JTI thực tế từ Server)
        /// - Quyền cha của kênh đang bật (F_LOGIN_DESKTOP)
        /// </summary>
        private static bool CheckSessionAndParentChannel(string functionCode, Bu.CLASS_SECURITY.ChannelAction action, out string normalizedCode)
        {
            normalizedCode = NormalizeFunctionCode(functionCode);
            if (CurrentUser == null || string.IsNullOrWhiteSpace(functionCode)) return false;

            // 1. Phải có định danh phiên làm việc thực tế (Zero-Trust)
            if (string.IsNullOrWhiteSpace(CurrentSessionId) || string.IsNullOrWhiteSpace(CurrentJti))
            {
                return false;
            }

            // 2. Tài khoản không bị vô hiệu hóa
            if ((CurrentUser.DISABLED ?? 0) == 1)
            {
                return false;
            }

            string channel = string.IsNullOrWhiteSpace(CurrentChannel) ? "DESKTOP" : Bu.CLASS_SECURITY.AppChannels.Normalize(CurrentChannel);

            // 3. Nếu là chính mã đăng nhập nền tảng (F_LOGIN_*)
            if (Bu.CLASS_SECURITY.PlatformFunctionCodes.IsPlatformCode(functionCode))
            {
                if (action != Bu.CLASS_SECURITY.ChannelAction.View)
                {
                    return false; // Quyền nền tảng chỉ có action View/Grant
                }

                if (DetailedRights != null && DetailedRights.TryGetValue(functionCode, out var pDetail))
                {
                    return pDetail.CAN_VIEW;
                }
                return UserRights != null && UserRights.Contains(functionCode);
            }

            // 4. Kiểm tra quyền cha của kênh (F_LOGIN_DESKTOP / F_LOGIN_WEB / F_LOGIN_MOBILE)
            string parentCode = Bu.CLASS_SECURITY.PlatformFunctionCodes.GetFunctionCodeForChannel(channel);
            bool isParentGranted = false;
            if (DetailedRights != null && DetailedRights.TryGetValue(parentCode, out var parentDetail))
            {
                isParentGranted = parentDetail.CAN_VIEW;
            }
            else if (UserRights != null)
            {
                isParentGranted = UserRights.Contains(parentCode);
            }

            if (!isParentGranted)
            {
                // Quyền cha TẮT -> Toàn bộ quyền con của kênh bị VÔ HIỆU HÓA (kể cả ADMIN!)
                return false;
            }

            // 5. Kiểm tra khả năng hỗ trợ của kênh (Capability Registry Boundary)
            // Kể cả ADMIN hoặc Wildcard cũng không được vượt qua giới hạn của kênh!
            if (!Bu.CLASS_SECURITY.ChannelCapabilityRegistry.IsActionSupported(channel, normalizedCode, action))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tương thích ngược: HasRight tương đương với CanView.
        /// </summary>
        public static bool HasRight(string functionCode)
        {
            return CanView(functionCode);
        }

        /// <summary>
        /// Quyền Xem (VIEW): Cho phép truy cập màn hình, xem danh sách dữ liệu.
        /// </summary>
        public static bool CanView(string functionCode)
        {
            if (!CheckSessionAndParentChannel(functionCode, Bu.CLASS_SECURITY.ChannelAction.View, out string normalized))
            {
                return false;
            }

            if (DetailedRights != null && (DetailedRights.TryGetValue(functionCode, out var detail) || DetailedRights.TryGetValue(normalized, out detail)))
            {
                return detail.CAN_VIEW;
            }

            if (UserRights != null)
            {
                if (UserRights.Contains(functionCode) || UserRights.Contains(normalized)) return true;
                if (!Bu.CLASS_SECURITY.PlatformFunctionCodes.IsPlatformCode(normalized) && UserRights.Contains("*")) return true;
            }

            return false;
        }

        /// <summary>
        /// Quyền Thêm (ADD): Cho phép bấm nút Thêm và thực hiện thao tác INSERT.
        /// </summary>
        public static bool CanAdd(string functionCode)
        {
            if (!CheckSessionAndParentChannel(functionCode, Bu.CLASS_SECURITY.ChannelAction.Add, out string normalized))
            {
                return false;
            }

            if (DetailedRights != null && (DetailedRights.TryGetValue(functionCode, out var detail) || DetailedRights.TryGetValue(normalized, out detail)))
            {
                return detail.CAN_ADD;
            }

            return false;
        }

        /// <summary>
        /// Quyền Sửa (EDIT): Cho phép bấm nút Sửa và thực hiện thao tác UPDATE.
        /// </summary>
        public static bool CanEdit(string functionCode)
        {
            if (!CheckSessionAndParentChannel(functionCode, Bu.CLASS_SECURITY.ChannelAction.Edit, out string normalized))
            {
                return false;
            }

            if (DetailedRights != null && (DetailedRights.TryGetValue(functionCode, out var detail) || DetailedRights.TryGetValue(normalized, out detail)))
            {
                return detail.CAN_EDIT;
            }

            return false;
        }

        /// <summary>
        /// Quyền Xóa (DELETE): Cho phép bấm nút Xóa và thực hiện thao tác DELETE.
        /// </summary>
        public static bool CanDelete(string functionCode)
        {
            if (!CheckSessionAndParentChannel(functionCode, Bu.CLASS_SECURITY.ChannelAction.Delete, out string normalized))
            {
                return false;
            }

            if (DetailedRights != null && (DetailedRights.TryGetValue(functionCode, out var detail) || DetailedRights.TryGetValue(normalized, out detail)))
            {
                return detail.CAN_DELETE;
            }

            return false;
        }

        /// <summary>
        /// Quyền In (PRINT): Cho phép bấm nút In và xuất báo cáo.
        /// </summary>
        public static bool CanPrint(string functionCode)
        {
            if (!CheckSessionAndParentChannel(functionCode, Bu.CLASS_SECURITY.ChannelAction.Print, out string normalized))
            {
                return false;
            }

            if (DetailedRights != null && (DetailedRights.TryGetValue(functionCode, out var detail) || DetailedRights.TryGetValue(normalized, out detail)))
            {
                return detail.CAN_PRINT;
            }

            return false;
        }

        public static bool CheckPermission(string functionCode, PermissionAction action)
        {
            switch (action)
            {
                case PermissionAction.View: return CanView(functionCode);
                case PermissionAction.Add: return CanAdd(functionCode);
                case PermissionAction.Edit: return CanEdit(functionCode);
                case PermissionAction.Delete: return CanDelete(functionCode);
                case PermissionAction.Print: return CanPrint(functionCode);
                default: return false;
            }
        }

        public static void Clear()
        {
            if (UserRights == null) UserRights = new List<string>();
            else UserRights.Clear();
            if (DetailedRights == null) DetailedRights = new Dictionary<string, UserRightDetail>(StringComparer.OrdinalIgnoreCase);
            else DetailedRights.Clear();
            CurrentLoginId = 0;
            CurrentSessionId = null;
            CurrentJti = null;
            CurrentTokenVersion = 0;
            CurrentChannel = "DESKTOP";
            CurrentToken = null;
            SessionCleared?.Invoke();
        }
    }
}
