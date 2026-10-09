using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.CLASS_SECURITY
{
    public interface IPlatformAccessGuard
    {
        bool CanExecute(decimal userId, string channel, string functionCode, ChannelAction action, string username = null);
        bool CanExecute(MyEntities db, decimal userId, string channel, string functionCode, ChannelAction action, string username = null);
        bool ResolveEndpointAction(string rightAlias, out string functionCode, out ChannelAction action);
    }

    /// <summary>
    /// Guard trung tâm kiểm soát thẩm quyền theo nền tảng (Desktop, Web, Mobile) và quyền cha - con
    /// theo đặc tả mục 13 và 14 trong ANTIGRAVITY_PLATFORM_ACCESS_LOGIC_AND_PLAN.md.
    /// Toàn bộ Desktop UserSession, Web API filters và Business Entry Points đều đi qua Guard này.
    /// </summary>
    public class PlatformAccessGuard : IPlatformAccessGuard
    {
        private static readonly IPlatformAccessGuard _instance = new PlatformAccessGuard();
        public static IPlatformAccessGuard Current => _instance;

        private readonly IPlatformAccessResolver _platformResolver;

        public PlatformAccessGuard(IPlatformAccessResolver platformResolver = null)
        {
            _platformResolver = platformResolver ?? new PlatformAccessResolver();
        }

        /// <summary>
        /// Bảng ánh xạ tường minh các mã Alias trên các Controller Web API sang Cặp (FunctionCode, ChannelAction).
        /// Loại bỏ hoàn toàn việc suy luận chuỗi bừa bãi hoặc bỏ qua kiểm tra khi không khớp.
        /// </summary>
        private static readonly Dictionary<string, (string FunctionCode, ChannelAction Action)> _endpointAliasMap
            = new Dictionary<string, (string FunctionCode, ChannelAction Action)>(StringComparer.OrdinalIgnoreCase)
        {
            // Hợp đồng lao động
            { "F_HOPDONG_VIEW", ("F_NV_HOPDONG", ChannelAction.View) },
            { "F_HOPDONG_ADD", ("F_NV_HOPDONG", ChannelAction.Add) },
            { "F_HOPDONG_EDIT", ("F_NV_HOPDONG", ChannelAction.Edit) },
            { "F_HOPDONG_DELETE", ("F_NV_HOPDONG", ChannelAction.Delete) },
            { "F_HOPDONG_PRINT", ("F_NV_HOPDONG", ChannelAction.Print) },

            // Hồ sơ nhân sự
            { "F_NHANSU_VIEW", ("F_DM_NHANVIEN", ChannelAction.View) },
            { "F_NHANSU_ADD", ("F_DM_NHANVIEN", ChannelAction.Add) },
            { "F_NHANSU_EDIT", ("F_DM_NHANVIEN", ChannelAction.Edit) },
            { "F_NHANSU_DELETE", ("F_DM_NHANVIEN", ChannelAction.Delete) },
            { "F_NHANSU_PRINT", ("F_DM_NHANVIEN", ChannelAction.Print) },

            // Tạm ứng lương
            { "F_UNGLUONG_VIEW", ("F_CC_UNGLUONG", ChannelAction.View) },
            { "F_UNGLUONG_ADD", ("F_CC_UNGLUONG", ChannelAction.Add) },
            { "F_UNGLUONG_EDIT", ("F_CC_UNGLUONG", ChannelAction.Edit) },
            { "F_UNGLUONG_DELETE", ("F_CC_UNGLUONG", ChannelAction.Delete) },

            // Tăng ca
            { "F_TANGCA_VIEW", ("F_CC_TANGCA", ChannelAction.View) },
            { "F_TANGCA_ADD", ("F_CC_TANGCA", ChannelAction.Add) },
            { "F_TANGCA_EDIT", ("F_CC_TANGCA", ChannelAction.Edit) },
            { "F_TANGCA_DELETE", ("F_CC_TANGCA", ChannelAction.Delete) },

            // Nâng lương
            { "F_NANGLUONG_VIEW", ("F_NV_NANGLUONG", ChannelAction.View) },
            { "F_NANGLUONG_ADD", ("F_NV_NANGLUONG", ChannelAction.Add) },
            { "F_NANGLUONG_EDIT", ("F_NV_NANGLUONG", ChannelAction.Edit) },
            { "F_NANGLUONG_DELETE", ("F_NV_NANGLUONG", ChannelAction.Delete) },

            // Điều chuyển
            { "F_DIEUCHUYEN_VIEW", ("F_NV_DIEUCHUYEN", ChannelAction.View) },
            { "F_DIEUCHUYEN_ADD", ("F_NV_DIEUCHUYEN", ChannelAction.Add) },
            { "F_DIEUCHUYEN_EDIT", ("F_NV_DIEUCHUYEN", ChannelAction.Edit) },
            { "F_DIEUCHUYEN_DELETE", ("F_NV_DIEUCHUYEN", ChannelAction.Delete) },

            // Khen thưởng
            { "F_KHENTHUONG_VIEW", ("F_NV_KHENTHUONG", ChannelAction.View) },
            { "F_KHENTHUONG_ADD", ("F_NV_KHENTHUONG", ChannelAction.Add) },
            { "F_KHENTHUONG_EDIT", ("F_NV_KHENTHUONG", ChannelAction.Edit) },
            { "F_KHENTHUONG_DELETE", ("F_NV_KHENTHUONG", ChannelAction.Delete) },

            // Bảng lương
            { "F_TIENLUONG", ("F_CC_BANGLUONG", ChannelAction.View) },
            { "F_BANGLUONG_VIEW", ("F_CC_BANGLUONG", ChannelAction.View) },
            { "F_BANGLUONG_ADD", ("F_CC_BANGLUONG", ChannelAction.Add) },
            { "F_BANGLUONG_EDIT", ("F_CC_BANGLUONG", ChannelAction.Edit) },
            { "F_BANGLUONG_DELETE", ("F_CC_BANGLUONG", ChannelAction.Delete) },
            { "F_BANGLUONG_PRINT", ("F_CC_BANGLUONG", ChannelAction.Print) },

            // Bảng công
            { "F_BANGCONG", ("F_CC_BANGCONG", ChannelAction.View) },
            { "F_BANGCONG_VIEW", ("F_CC_BANGCONG", ChannelAction.View) },
            { "F_BANGCONG_ADD", ("F_CC_BANGCONG", ChannelAction.Add) },
            { "F_BANGCONG_EDIT", ("F_CC_BANGCONG", ChannelAction.Edit) },
            { "F_BANGCONG_DELETE", ("F_CC_BANGCONG", ChannelAction.Delete) }
        };

        public bool ResolveEndpointAction(string rightAlias, out string functionCode, out ChannelAction action)
        {
            functionCode = null;
            action = ChannelAction.View;
            if (string.IsNullOrWhiteSpace(rightAlias)) return false;

            string key = rightAlias.Trim();
            if (_endpointAliasMap.TryGetValue(key, out var mapping))
            {
                functionCode = mapping.FunctionCode;
                action = mapping.Action;
                return true;
            }

            // Nếu trực tiếp là mã chức năng trong hệ thống
            functionCode = key.ToUpperInvariant();
            action = ChannelAction.View;
            return true;
        }

        public bool CanExecute(decimal userId, string channel, string functionCode, ChannelAction action, string username = null)
        {
            using (var db = new MyEntities())
            {
                return CanExecute(db, userId, channel, functionCode, action, username);
            }
        }

        public bool CanExecute(MyEntities db, decimal userId, string channel, string functionCode, ChannelAction action, string username = null)
        {
            if (db == null || userId <= 0 || string.IsNullOrWhiteSpace(channel) || string.IsNullOrWhiteSpace(functionCode))
            {
                return false;
            }

            string normChannel = AppChannels.Normalize(channel);
            if (normChannel == null) return false;

            // 1. Kiểm tra tài khoản người dùng
            var userRow = db.Database.SqlQuery<UserCheckRow>(@"
                SELECT IDUSER, USERNAME, DISABLED
                FROM HR.TB_SYS_USER
                WHERE IDUSER = :p0",
                new OracleParameter("p0", userId)
            ).FirstOrDefault();

            if (userRow == null || (userRow.DISABLED ?? 0) == 1)
            {
                return false; // Tài khoản không tồn tại hoặc bị vô hiệu hóa
            }

            string effUsername = username ?? userRow.USERNAME;
            bool isRootAdmin = effUsername != null && effUsername.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase);

            // 2. Quyền cha F_LOGIN_* theo kênh: Kể cả Root Admin cũng phải tuân thủ quyền cha và chính sách kênh!
            var parentRes = _platformResolver.ResolveChannel(db, userId, normChannel, effUsername);
            if (!parentRes.IsGranted || parentRes.ReadinessCode != "READY")
            {
                // Quyền cha bị TẮT hoặc kênh chưa sẵn sàng -> Toàn bộ quyền con của kênh bị VÔ HIỆU HÓA
                return false;
            }

            // 3. Nếu là chính mã đăng nhập nền tảng F_LOGIN_*
            if (PlatformFunctionCodes.IsPlatformFunction(functionCode))
            {
                string expectedParent = PlatformFunctionCodes.GetFunctionCodeForChannel(normChannel);
                if (!string.Equals(functionCode.Trim(), expectedParent, StringComparison.OrdinalIgnoreCase))
                {
                    return false; // Mã cha không khớp với kênh hiện tại
                }
                return action == ChannelAction.View;
            }

            // 4. Kiểm tra khả năng hỗ trợ của kênh (Capability Boundary)
            // Zero Trust: Nếu kênh không hỗ trợ action này (ví dụ Web Payroll tính lương, Mobile quản trị),
            // thì KỂ CẢ ADMIN HOẶC WILDCARD CŨNG BỊ CẤM TUYỆT ĐỐI!
            if (!ChannelCapabilityRegistry.IsActionSupported(normChannel, functionCode, action))
            {
                return false;
            }

            // 5. Kiểm tra phân quyền con trong TB_SYS_RIGHT_CHANNEL (kể cả ADMIN - ADMIN child off denied)
            return CheckChannelActionGrant(db, userId, normChannel, functionCode, action);
        }

        private bool CheckChannelActionGrant(MyEntities db, decimal userId, string channel, string functionCode, ChannelAction action)
        {
            string col = GetColumnName(action);

            // 1. Direct grant trong TB_SYS_RIGHT_CHANNEL
            string directSql = $@"
                SELECT NVL({col}, 0)
                FROM HR.TB_SYS_RIGHT_CHANNEL
                WHERE IDUSER = :p0 AND CLIENT_TYPE = :p1 AND FUNCTION_CODE = :p2";

            var directVal = db.Database.SqlQuery<decimal?>(directSql,
                new OracleParameter("p0", userId),
                new OracleParameter("p1", channel),
                new OracleParameter("p2", functionCode)
            ).FirstOrDefault();

            if (directVal.HasValue && directVal.Value == 1)
            {
                return true;
            }

            // 2. Group grant trong TB_SYS_RIGHT_CHANNEL (kế thừa từ active group)
            string groupSql = $@"
                SELECT NVL(rc.{col}, 0)
                FROM HR.TB_SYS_RIGHT_CHANNEL rc
                JOIN HR.TB_SYS_GROUP g ON g.ID_GROUP = rc.IDUSER
                JOIN HR.TB_SYS_USER u ON u.IDUSER = g.ID_GROUP
                WHERE g.MEMBER = :p0 AND rc.CLIENT_TYPE = :p1 AND rc.FUNCTION_CODE = :p2
                  AND NVL(u.ISGROUP, 0) = 1 AND NVL(u.DISABLED, 0) = 0";

            var groupVals = db.Database.SqlQuery<decimal?>(groupSql,
                new OracleParameter("p0", userId),
                new OracleParameter("p1", channel),
                new OracleParameter("p2", functionCode)
            ).ToList();

            return groupVals.Any(v => v.HasValue && v.Value == 1);
        }

        private static string GetColumnName(ChannelAction action)
        {
            switch (action)
            {
                case ChannelAction.View: return "CAN_VIEW";
                case ChannelAction.Add: return "CAN_ADD";
                case ChannelAction.Edit: return "CAN_EDIT";
                case ChannelAction.Delete: return "CAN_DELETE";
                case ChannelAction.Print: return "CAN_PRINT";
                default: return "CAN_VIEW";
            }
        }

        private class UserCheckRow
        {
            public decimal IDUSER { get; set; }
            public string USERNAME { get; set; }
            public decimal? DISABLED { get; set; }
        }
    }
}
