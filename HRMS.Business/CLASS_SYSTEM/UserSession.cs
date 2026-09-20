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

        public static bool IsLoggedIn => CurrentUser != null;

        public static bool IsAdmin => CurrentUser != null &&
            CurrentUser.USERNAME != null &&
            CurrentUser.USERNAME.Equals("admin", StringComparison.OrdinalIgnoreCase);

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
            if (CurrentUser == null || string.IsNullOrEmpty(functionCode)) return false;
            if (IsAdmin) return true;

            if (DetailedRights.TryGetValue(functionCode, out var detail))
            {
                return detail.CAN_VIEW;
            }

            return UserRights.Contains(functionCode);
        }

        /// <summary>
        /// Quyền Thêm (ADD): Cho phép bấm nút Thêm và thực hiện thao tác INSERT.
        /// </summary>
        public static bool CanAdd(string functionCode)
        {
            if (CurrentUser == null || string.IsNullOrEmpty(functionCode)) return false;
            if (IsAdmin) return true;

            if (DetailedRights.TryGetValue(functionCode, out var detail))
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
            if (CurrentUser == null || string.IsNullOrEmpty(functionCode)) return false;
            if (IsAdmin) return true;

            if (DetailedRights.TryGetValue(functionCode, out var detail))
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
            if (CurrentUser == null || string.IsNullOrEmpty(functionCode)) return false;
            if (IsAdmin) return true;

            if (DetailedRights.TryGetValue(functionCode, out var detail))
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
            if (CurrentUser == null || string.IsNullOrEmpty(functionCode)) return false;
            if (IsAdmin) return true;

            if (DetailedRights.TryGetValue(functionCode, out var detail))
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
            CurrentUser = null;
            UserRights?.Clear();
            DetailedRights?.Clear();
            CurrentLoginId = 0;
        }
    }
}
