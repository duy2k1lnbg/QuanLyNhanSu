using DA;
using System;
using System.Collections.Generic;

namespace Bu.CLASS_SYSTEM
{
    public static class UserSession
    {
        public static TB_SYS_USER CurrentUser { get; set; }
        public static List<string> UserRights { get; set; } = new List<string>();
        public static decimal CurrentLoginId { get; set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static bool HasRight(string functionCode)
        {
            if (CurrentUser == null) return false;
            
            // System administrator has full rights
            if (CurrentUser.USERNAME.Equals("admin", StringComparison.OrdinalIgnoreCase))
                return true;

            return UserRights.Contains(functionCode);
        }

        public static void Clear()
        {
            CurrentUser = null;
            UserRights.Clear();
        }
    }
}
