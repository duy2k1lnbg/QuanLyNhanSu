using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Bu.CLASS_SECURITY
{
    public interface IChannelPermissionResolver
    {
        bool CanExecuteAction(MyEntities db, decimal userId, string channel, string functionCode, ChannelAction action, string username = null);
        PlatformChannelTreeDto ResolveChannelTree(MyEntities db, decimal userId, string channel, string username = null);
        UserFullChannelPermissionsDto ResolveUserFullPermissions(MyEntities db, decimal userId, string username = null);
        Task<SaveChannelRightsResult> SaveChannelRightsAsync(SaveChannelRightsRequest req);
        Task<BatchSaveChannelRightsResult> SaveBatchChannelRightsAsync(BatchSaveChannelRightsRequest req);
        void ProjectEffectiveRights(PlatformChannelTreeDto channelTree, out Dictionary<string, Bu.DTO.UserRightDetail> detailedRights, out List<string> viewableRights);
    }

    /// <summary>
    /// Resolver phân quyền cha - con theo kênh theo đặc tả mục 13 và 14
    /// </summary>
    public class ChannelPermissionResolver : IChannelPermissionResolver
    {
        private readonly IPlatformAccessResolver _platformResolver;
        private readonly IAuthAuditService _auditService;

        public ChannelPermissionResolver(IPlatformAccessResolver platformResolver = null, IAuthAuditService auditService = null)
        {
            _platformResolver = platformResolver ?? new PlatformAccessResolver();
            _auditService = auditService ?? new AuthAuditService();
        }

        private class RawChannelRightRow
        {
            public decimal IDUSER { get; set; }
            public string CLIENT_TYPE { get; set; }
            public string FUNCTION_CODE { get; set; }
            public decimal? CAN_VIEW { get; set; }
            public decimal? CAN_ADD { get; set; }
            public decimal? CAN_EDIT { get; set; }
            public decimal? CAN_DELETE { get; set; }
            public decimal? CAN_PRINT { get; set; }
        }

        private class FunctionMetaRow
        {
            public string FUNCTION_CODE { get; set; }
            public string DESCRIPTION { get; set; }
            public string PARENT { get; set; }
            public string RIGHT_TYPE { get; set; }
            public decimal SORT { get; set; }
            public decimal? ISGROUP { get; set; }
        }

        private class GroupRow
        {
            public decimal ID_GROUP { get; set; }
            public string GROUP_NAME { get; set; }
        }

        private class TargetUserRow
        {
            public decimal IDUSER { get; set; }
            public string USERNAME { get; set; }
            public string FULLNAME { get; set; }
            public decimal? DISABLED { get; set; }
            public decimal? ISGROUP { get; set; }
            public decimal? MANV { get; set; }
            public DateTime? LOCKOUT_END { get; set; }
            public decimal? TOKEN_VERSION { get; set; }
        }

        /// <summary>
        /// Kiểm tra quyền thực thi một hành động trên kênh cụ thể.
        /// Công thức:
        /// Effective = AccountUsable
        ///             AND PlatformGranted(user, channel) (Quyền cha)
        ///             AND ChannelSupports(channel, func, action) (Khả năng kênh)
        ///             AND ChannelActionGranted(user, channel, func, action)
        /// </summary>
        public bool CanExecuteAction(MyEntities db, decimal userId, string channel, string functionCode, ChannelAction action, string username = null)
        {
            string normChannel = AppChannels.Normalize(channel);
            if (normChannel == null || string.IsNullOrWhiteSpace(functionCode)) return false;

            // 1. Kiểm tra quyền cha (Platform Access)
            var parentRes = _platformResolver.ResolveChannel(db, userId, normChannel, username);
            if (!parentRes.IsGranted)
            {
                // Quyền cha tắt -> Toàn bộ quyền con không có hiệu lực
                return false;
            }

            // 2. Quyền cha F_LOGIN_* chỉ có action View (tương đương cấp phép)
            if (PlatformFunctionCodes.IsPlatformFunction(functionCode))
            {
                return action == ChannelAction.View;
            }

            // 3. Kiểm tra khả năng hỗ trợ của kênh
            if (!ChannelCapabilityRegistry.IsActionSupported(normChannel, functionCode, action))
            {
                // Kênh không hỗ trợ thao tác này (ví dụ: Web Payroll tính lương)
                return false;
            }

            // 4. Kiểm tra bảng TB_SYS_RIGHT_CHANNEL (Default Deny - không fallback sang TB_SYS_RIGHT)
            bool isChannelTableReady = CheckIfChannelTableExists(db);
            if (!isChannelTableReady)
            {
                // Sau cutover, thiếu bảng hoặc lỗi DB phải trả trạng thái chưa sẵn sàng và chặn
                return false;
            }

            // Cả Admin và người dùng thường đều phải được cấp quyền con trên kênh (ADMIN child off denied)
            return CheckActionInRightChannel(db, userId, normChannel, functionCode, action);
        }

        private bool CheckActionInRightChannel(MyEntities db, decimal userId, string channel, string functionCode, ChannelAction action)
        {
            // Kiểm tra direct grant
            var direct = db.Database.SqlQuery<RawChannelRightRow>(@"
                SELECT IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT
                FROM HR.TB_SYS_RIGHT_CHANNEL
                WHERE IDUSER = :p0 AND CLIENT_TYPE = :p1 AND FUNCTION_CODE = :p2",
                new OracleParameter("p0", userId),
                new OracleParameter("p1", channel),
                new OracleParameter("p2", functionCode)
            ).FirstOrDefault();

            if (direct != null && IsActionSet(direct, action))
            {
                return true;
            }

            // Kiểm tra inherited grant từ nhóm đang hoạt động
            var groupRights = db.Database.SqlQuery<RawChannelRightRow>(@"
                SELECT r.IDUSER, r.CLIENT_TYPE, r.FUNCTION_CODE, r.CAN_VIEW, r.CAN_ADD, r.CAN_EDIT, r.CAN_DELETE, r.CAN_PRINT
                FROM HR.TB_SYS_RIGHT_CHANNEL r
                JOIN HR.TB_SYS_GROUP g ON g.ID_GROUP = r.IDUSER
                JOIN HR.TB_SYS_USER u ON u.IDUSER = g.ID_GROUP
                WHERE g.MEMBER = :p0 AND r.CLIENT_TYPE = :p1 AND r.FUNCTION_CODE = :p2
                  AND NVL(u.ISGROUP, 0) = 1 AND NVL(u.DISABLED, 0) = 0",
                new OracleParameter("p0", userId),
                new OracleParameter("p1", channel),
                new OracleParameter("p2", functionCode)
            ).ToList();

            return groupRights.Any(r => IsActionSet(r, action));
        }

        private bool CheckActionInLegacyRight(MyEntities db, decimal userId, string functionCode, ChannelAction action)
        {
            // Fallback đọc từ TB_SYS_RIGHT cho môi trường chưa cutover
            string col = GetColumnNameForAction(action);
            string sql = $@"
                SELECT NVL({col}, 0)
                FROM HR.TB_SYS_RIGHT
                WHERE IDUSER = :p0 AND FUNCTION_CODE = :p1";

            decimal? directVal = db.Database.SqlQuery<decimal?>(sql,
                new OracleParameter("p0", userId),
                new OracleParameter("p1", functionCode)
            ).FirstOrDefault();

            if (directVal.HasValue && directVal.Value == 1) return true;

            string groupSql = $@"
                SELECT NVL(r.{col}, 0)
                FROM HR.TB_SYS_RIGHT r
                JOIN HR.TB_SYS_GROUP g ON g.ID_GROUP = r.IDUSER
                JOIN HR.TB_SYS_USER u ON u.IDUSER = g.ID_GROUP
                WHERE g.MEMBER = :p0 AND r.FUNCTION_CODE = :p1
                  AND NVL(u.ISGROUP, 0) = 1 AND NVL(u.DISABLED, 0) = 0";

            var groupVals = db.Database.SqlQuery<decimal?>(groupSql,
                new OracleParameter("p0", userId),
                new OracleParameter("p1", functionCode)
            ).ToList();

            return groupVals.Any(v => v.HasValue && v.Value == 1);
        }

        private static bool IsActionSet(RawChannelRightRow r, ChannelAction action)
        {
            if (r == null) return false;
            switch (action)
            {
                case ChannelAction.View: return (r.CAN_VIEW ?? 0) == 1;
                case ChannelAction.Add: return (r.CAN_ADD ?? 0) == 1;
                case ChannelAction.Edit: return (r.CAN_EDIT ?? 0) == 1;
                case ChannelAction.Delete: return (r.CAN_DELETE ?? 0) == 1;
                case ChannelAction.Print: return (r.CAN_PRINT ?? 0) == 1;
                default: return false;
            }
        }

        private static string GetColumnNameForAction(ChannelAction action)
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

        private bool CheckIfChannelTableExists(MyEntities db)
        {
            try
            {
                int cnt = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM ALL_TABLES WHERE OWNER = 'HR' AND TABLE_NAME = 'TB_SYS_RIGHT_CHANNEL'"
                ).FirstOrDefault();
                return cnt > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Phân giải cây quyền cho 1 kênh cụ thể
        /// </summary>
        public PlatformChannelTreeDto ResolveChannelTree(MyEntities db, decimal userId, string channel, string username = null)
        {
            string normChannel = AppChannels.Normalize(channel);
            var parentRes = _platformResolver.ResolveChannel(db, userId, normChannel, username);

            var tree = new PlatformChannelTreeDto
            {
                Channel = normChannel,
                ChannelLabel = AppChannels.GetLabel(normChannel),
                ParentFunctionCode = parentRes.FunctionCode,
                ParentFunctionName = AppChannels.GetLabel(normChannel),
                ParentIsEffective = parentRes.IsGranted,
                ParentDirectGrant = parentRes.DirectGrant,
                ParentInheritedGrant = parentRes.InheritedGrant,
                ParentInheritedGroupNames = parentRes.InheritedFromGroupNames,
                ReadinessCode = parentRes.ReadinessCode,
                ReadinessMessage = parentRes.ReadinessMessage
            };

            // Lấy danh mục chức năng
            var functions = db.Database.SqlQuery<FunctionMetaRow>(@"
                SELECT FUNCTION_CODE, DESCRIPTION, PARENT, NVL(RIGHT_TYPE, 'FUNCTION') AS RIGHT_TYPE, SORT, ISGROUP
                FROM HR.TB_SYS_FUNCTION
                WHERE FUNCTION_CODE NOT IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE')
                ORDER BY SORT, FUNCTION_CODE"
            ).ToList();

            // Lấy direct grants và group grants
            bool hasChannelTable = CheckIfChannelTableExists(db);
            Dictionary<string, RawChannelRightRow> directMap = new Dictionary<string, RawChannelRightRow>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, List<RawChannelRightRow>> groupMap = new Dictionary<string, List<RawChannelRightRow>>(StringComparer.OrdinalIgnoreCase);

            if (hasChannelTable)
            {
                var directs = db.Database.SqlQuery<RawChannelRightRow>(@"
                    SELECT IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT
                    FROM HR.TB_SYS_RIGHT_CHANNEL
                    WHERE IDUSER = :p0 AND CLIENT_TYPE = :p1",
                    new OracleParameter("p0", userId),
                    new OracleParameter("p1", normChannel)
                ).ToList();
                foreach (var d in directs) directMap[d.FUNCTION_CODE] = d;

                var groups = db.Database.SqlQuery<RawChannelRightRow>(@"
                    SELECT r.IDUSER, r.CLIENT_TYPE, r.FUNCTION_CODE, r.CAN_VIEW, r.CAN_ADD, r.CAN_EDIT, r.CAN_DELETE, r.CAN_PRINT
                    FROM HR.TB_SYS_RIGHT_CHANNEL r
                    JOIN HR.TB_SYS_GROUP g ON g.ID_GROUP = r.IDUSER
                    JOIN HR.TB_SYS_USER u ON u.IDUSER = g.ID_GROUP
                    WHERE g.MEMBER = :p0 AND r.CLIENT_TYPE = :p1
                      AND NVL(u.ISGROUP, 0) = 1 AND NVL(u.DISABLED, 0) = 0",
                    new OracleParameter("p0", userId),
                    new OracleParameter("p1", normChannel)
                ).ToList();
                foreach (var g in groups)
                {
                    if (!groupMap.ContainsKey(g.FUNCTION_CODE)) groupMap[g.FUNCTION_CODE] = new List<RawChannelRightRow>();
                    groupMap[g.FUNCTION_CODE].Add(g);
                }
            }
            // Neu khong co bang TB_SYS_RIGHT_CHANNEL, directMap va groupMap de rong (Default Deny)

            bool parentOn = parentRes.IsGranted;

            foreach (var fn in functions)
            {
                var cap = ChannelCapabilityRegistry.GetCapability(normChannel, fn.FUNCTION_CODE);
                var direct = directMap.ContainsKey(fn.FUNCTION_CODE) ? directMap[fn.FUNCTION_CODE] : null;
                var groups = groupMap.ContainsKey(fn.FUNCTION_CODE) ? groupMap[fn.FUNCTION_CODE] : new List<RawChannelRightRow>();

                var item = new ChannelFunctionRightItemDto
                {
                    FunctionCode = fn.FUNCTION_CODE,
                    FunctionName = fn.DESCRIPTION,
                    ParentCode = fn.PARENT,
                    RightType = fn.RIGHT_TYPE,
                    Sort = fn.SORT,
                    RestrictionNote = cap?.RestrictionNote,
                    IsDisabledByParent = !parentOn,
                    SupportedCapabilities = new FunctionActionGrantDto
                    {
                        CanView = cap?.CanView ?? false,
                        CanAdd = cap?.CanAdd ?? false,
                        CanEdit = cap?.CanEdit ?? false,
                        CanDelete = cap?.CanDelete ?? false,
                        CanPrint = cap?.CanPrint ?? false
                    },
                    DirectGrant = new FunctionActionGrantDto
                    {
                        CanView = direct != null && (direct.CAN_VIEW ?? 0) == 1,
                        CanAdd = direct != null && (direct.CAN_ADD ?? 0) == 1,
                        CanEdit = direct != null && (direct.CAN_EDIT ?? 0) == 1,
                        CanDelete = direct != null && (direct.CAN_DELETE ?? 0) == 1,
                        CanPrint = direct != null && (direct.CAN_PRINT ?? 0) == 1
                    },
                    InheritedGrant = new FunctionActionGrantDto
                    {
                        CanView = groups.Any(g => (g.CAN_VIEW ?? 0) == 1),
                        CanAdd = groups.Any(g => (g.CAN_ADD ?? 0) == 1),
                        CanEdit = groups.Any(g => (g.CAN_EDIT ?? 0) == 1),
                        CanDelete = groups.Any(g => (g.CAN_DELETE ?? 0) == 1),
                        CanPrint = groups.Any(g => (g.CAN_PRINT ?? 0) == 1)
                    }
                };

                // Quyền hiệu lực: Chỉ có khi quyền cha đang bật VÀ kênh hỗ trợ VÀ (direct OR inherited)
                if (parentOn)
                {
                    item.EffectiveGrant = new FunctionActionGrantDto
                    {
                        CanView = item.SupportedCapabilities.CanView && (item.DirectGrant.CanView || item.InheritedGrant.CanView),
                        CanAdd = item.SupportedCapabilities.CanAdd && (item.DirectGrant.CanAdd || item.InheritedGrant.CanAdd),
                        CanEdit = item.SupportedCapabilities.CanEdit && (item.DirectGrant.CanEdit || item.InheritedGrant.CanEdit),
                        CanDelete = item.SupportedCapabilities.CanDelete && (item.DirectGrant.CanDelete || item.InheritedGrant.CanDelete),
                        CanPrint = item.SupportedCapabilities.CanPrint && (item.DirectGrant.CanPrint || item.InheritedGrant.CanPrint)
                    };
                }
                else
                {
                    // Cha tắt -> toàn bộ quyền con hiệu lực = false
                    item.EffectiveGrant = new FunctionActionGrantDto();
                }

                tree.Functions.Add(item);
            }

            return tree;
        }

        public UserFullChannelPermissionsDto ResolveUserFullPermissions(MyEntities db, decimal userId, string username = null)
        {
            var user = db.Database.SqlQuery<TargetUserRow>(@"
                SELECT IDUSER, USERNAME, FULLNAME, DISABLED, ISGROUP, MANV, LOCKOUT_END, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION
                FROM HR.TB_SYS_USER WHERE IDUSER = :p0",
                new OracleParameter("p0", userId)
            ).FirstOrDefault();

            if (user == null) return null;

            bool isAdmin = user.USERNAME != null && user.USERNAME.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase);

            var result = new UserFullChannelPermissionsDto
            {
                UserId = user.IDUSER,
                Username = user.USERNAME,
                FullName = user.FULLNAME,
                IsGroup = (user.ISGROUP ?? 0) == 1,
                IsAdmin = isAdmin,
                IsDisabled = (user.DISABLED ?? 0) == 1,
                SecurityVersion = (long)(user.TOKEN_VERSION ?? 1)
            };

            result.Channels.Add(ResolveChannelTree(db, userId, AppChannels.Desktop, user.USERNAME));
            result.Channels.Add(ResolveChannelTree(db, userId, AppChannels.Web, user.USERNAME));
            result.Channels.Add(ResolveChannelTree(db, userId, AppChannels.Mobile, user.USERNAME));

            return result;
        }

        /// <summary>
        /// Lưu cấu hình quyền cha và con cho một kênh.
        /// Quy tắc bảo vệ:
        /// 1. Nếu tắt cha: giữ nguyên cấu hình con cũ đã lưu, không cấp mới con khi cha tắt.
        /// 2. Cấp mới con: bắt buộc cha phải đang bật hoặc được bật cùng lúc.
        /// 3. Khóa các action không được kênh hỗ trợ (ví dụ Web lương).
        /// 4. Admin cấm Mobile.
        /// 5. Transaction an toàn, tăng version/thu hồi phiên khi giảm quyền.
        /// </summary>
        public async Task<SaveChannelRightsResult> SaveChannelRightsAsync(SaveChannelRightsRequest req)
        {
            if (req == null || req.TargetUserId <= 0 || string.IsNullOrWhiteSpace(req.Channel))
            {
                return new SaveChannelRightsResult { Success = false, Message = "Dữ liệu yêu cầu không hợp lệ." };
            }

            string normChannel = AppChannels.Normalize(req.Channel);
            if (normChannel == null)
            {
                return new SaveChannelRightsResult { Success = false, Message = "Kênh truy cập không hợp lệ." };
            }

            string correlationId = req.CorrelationId ?? Guid.NewGuid().ToString("N");

            using (var db = new MyEntities())
            {
                using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // 0. Xác thực Actor (Người thực hiện phải là Quản trị viên hệ thống)
                        decimal actorId = req.ActorUserId;
                        if (actorId <= 0 && MyEntities.CurrentAuditUserId.HasValue && MyEntities.CurrentAuditUserId.Value > 0)
                        {
                            actorId = (decimal)MyEntities.CurrentAuditUserId.Value;
                        }

                        if (actorId > 0)
                        {
                            var actor = db.Database.SqlQuery<TargetUserRow>(@"
                                SELECT IDUSER, USERNAME, FULLNAME, DISABLED, ISGROUP, MANV, LOCKOUT_END, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION
                                FROM HR.TB_SYS_USER
                                WHERE IDUSER = :p0",
                                new OracleParameter("p0", actorId)
                            ).FirstOrDefault();

                            bool isActorAdmin = actor != null && actor.USERNAME != null && actor.USERNAME.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
                            if (!isActorAdmin)
                            {
                                transaction.Rollback();
                                return new SaveChannelRightsResult
                                {
                                    Success = false,
                                    Message = "Từ chối truy cập: Thao tác phân quyền chỉ dành riêng cho Quản trị viên hệ thống (Admin)."
                                };
                            }
                        }

                        // 1. Lock Target User
                        var user = db.Database.SqlQuery<TargetUserRow>(@"
                            SELECT IDUSER, USERNAME, FULLNAME, DISABLED, ISGROUP, MANV, LOCKOUT_END, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION
                            FROM HR.TB_SYS_USER
                            WHERE IDUSER = :p0 FOR UPDATE",
                            new OracleParameter("p0", req.TargetUserId)
                        ).FirstOrDefault();

                        if (user == null)
                        {
                            transaction.Rollback();
                            return new SaveChannelRightsResult { Success = false, Message = "Người dùng không tồn tại." };
                        }

                        // Concurrency Check
                        if (req.ExpectedSecurityVersion.HasValue && (user.TOKEN_VERSION ?? 1) != req.ExpectedSecurityVersion.Value)
                        {
                            transaction.Rollback();
                            return new SaveChannelRightsResult
                            {
                                Success = false,
                                IsConflict = true,
                                Message = "Xung đột phiên bản: Dữ liệu phân quyền đã được quản trị viên khác cập nhật. Vui lòng tải lại dữ liệu mới nhất."
                            };
                        }

                        bool isTargetAdmin = user.USERNAME != null && user.USERNAME.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase);

                        // Quy tắc bảo mật: Admin cấm tuyệt đối Mobile
                        if (isTargetAdmin && normChannel == AppChannels.Mobile && req.ParentDirectGrant == true)
                        {
                            transaction.Rollback();
                            return new SaveChannelRightsResult
                            {
                                Success = false,
                                Message = "Tài khoản Quản trị tối cao (ADMIN) bị cấm sử dụng ứng dụng di động Mobile."
                            };
                        }

                        // 2. Cập nhật quyền cha nếu có truyền
                        string parentFunc = PlatformFunctionCodes.GetFunctionCodeForChannel(normChannel);
                        if (req.ParentDirectGrant.HasValue)
                        {
                            int val = req.ParentDirectGrant.Value ? 1 : 0;
                            db.Database.ExecuteSqlCommand(@"
                                MERGE INTO HR.TB_SYS_RIGHT tgt
                                USING (SELECT :p0 AS IDUSER, :p1 AS FUNCTION_CODE FROM DUAL) src
                                ON (tgt.IDUSER = src.IDUSER AND tgt.FUNCTION_CODE = src.FUNCTION_CODE)
                                WHEN MATCHED THEN
                                    UPDATE SET tgt.CAN_VIEW = :p2, tgt.USER_RIGHT = :p3,
                                               tgt.CAN_ADD = 0, tgt.CAN_EDIT = 0, tgt.CAN_DELETE = 0, tgt.CAN_PRINT = 0
                                WHEN NOT MATCHED THEN
                                    INSERT (IDUSER, FUNCTION_CODE, CAN_VIEW, USER_RIGHT, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
                                    VALUES (src.IDUSER, src.FUNCTION_CODE, :p4, :p5, 0, 0, 0, 0)",
                                new OracleParameter("p0", req.TargetUserId),
                                new OracleParameter("p1", parentFunc),
                                new OracleParameter("p2", val),
                                new OracleParameter("p3", val),
                                new OracleParameter("p4", val),
                                new OracleParameter("p5", val)
                            );
                        }

                        // Kiểm tra trạng thái quyền cha sau cập nhật
                        var parentRes = _platformResolver.ResolveChannel(db, req.TargetUserId, normChannel, user.USERNAME);
                        bool isParentEffective = parentRes.IsGranted;

                        // 3. Cập nhật quyền con
                        bool hasChannelTable = CheckIfChannelTableExists(db);
                        if (hasChannelTable && req.Functions != null && req.Functions.Any())
                        {
                            // Nếu cha tắt: không cho phép cấp thêm quyền con mới
                            if (!isParentEffective)
                            {
                                // Cấu hình quyền con cũ trong DB vẫn được giữ nguyên để rà soát
                            }
                            else
                            {
                                foreach (var f in req.Functions)
                                {
                                    if (PlatformFunctionCodes.IsPlatformFunction(f.FunctionCode)) continue;

                                    var cap = ChannelCapabilityRegistry.GetCapability(normChannel, f.FunctionCode);
                                    if (cap == null) continue;

                                    // Khóa các action không được kênh hỗ trợ
                                    int view = (cap.CanView && f.CanView) ? 1 : 0;
                                    int add = (cap.CanAdd && f.CanAdd) ? 1 : 0;
                                    int edit = (cap.CanEdit && f.CanEdit) ? 1 : 0;
                                    int del = (cap.CanDelete && f.CanDelete) ? 1 : 0;
                                    int print = (cap.CanPrint && f.CanPrint) ? 1 : 0;

                                    db.Database.ExecuteSqlCommand(@"
                                        MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
                                        USING (SELECT :p0 AS IDUSER, :p1 AS CLIENT_TYPE, :p2 AS FUNCTION_CODE FROM DUAL) src
                                        ON (tgt.IDUSER = src.IDUSER AND tgt.CLIENT_TYPE = src.CLIENT_TYPE AND tgt.FUNCTION_CODE = src.FUNCTION_CODE)
                                        WHEN MATCHED THEN
                                            UPDATE SET tgt.CAN_VIEW = :p3, tgt.CAN_ADD = :p4, tgt.CAN_EDIT = :p5,
                                                       tgt.CAN_DELETE = :p6, tgt.CAN_PRINT = :p7, tgt.UPDATED_AT = SYSTIMESTAMP
                                        WHEN NOT MATCHED THEN
                                            INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
                                            VALUES (src.IDUSER, src.CLIENT_TYPE, src.FUNCTION_CODE, :p8, :p9, :p10, :p11, :p12)",
                                        new OracleParameter("p0", req.TargetUserId),
                                        new OracleParameter("p1", normChannel),
                                        new OracleParameter("p2", f.FunctionCode),
                                        new OracleParameter("p3", view),
                                        new OracleParameter("p4", add),
                                        new OracleParameter("p5", edit),
                                        new OracleParameter("p6", del),
                                        new OracleParameter("p7", print),
                                        new OracleParameter("p8", view),
                                        new OracleParameter("p9", add),
                                        new OracleParameter("p10", edit),
                                        new OracleParameter("p11", del),
                                        new OracleParameter("p12", print)
                                    );
                                }
                            }
                        }

                        // 4. Thu hồi phiên và làm mới Token Version
                        int revokedSessions = 0;
                        if (!isParentEffective)
                        {
                            revokedSessions = RevokeSessionsForChannel(db, req.TargetUserId, normChannel, "PARENT_PLATFORM_DISABLED");
                        }

                        // Nếu đối tượng là Nhóm: Cập nhật TokenVersion và thu hồi phiên của mọi thành viên bị ảnh hưởng
                        bool isGroup = (user.ISGROUP ?? 0) == 1;
                        if (isGroup)
                        {
                            var memberIds = db.TB_SYS_GROUP.Where(g => g.ID_GROUP == req.TargetUserId).Select(g => g.MEMBER).ToList();
                            foreach (var memberId in memberIds)
                            {
                                if (!isParentEffective)
                                {
                                    revokedSessions += RevokeSessionsForChannel(db, memberId, normChannel, "GROUP_PLATFORM_DISABLED");
                                }
                                db.Database.ExecuteSqlCommand(
                                    "UPDATE HR.TB_SYS_USER SET TOKEN_VERSION = NVL(TOKEN_VERSION, 1) + 1 WHERE IDUSER = :p0",
                                    new OracleParameter("p0", memberId)
                                );
                            }
                        }
                        else
                        {
                            db.Database.ExecuteSqlCommand(
                                "UPDATE HR.TB_SYS_USER SET TOKEN_VERSION = NVL(TOKEN_VERSION, 1) + 1 WHERE IDUSER = :p0",
                                new OracleParameter("p0", req.TargetUserId)
                            );
                        }

                        bool actorAffected = actorId > 0 && (
                            actorId == req.TargetUserId ||
                            (isGroup && db.TB_SYS_GROUP.Any(g => g.ID_GROUP == req.TargetUserId && g.MEMBER == actorId))
                        );

                        // 5. Ghi Audit log với context chính xác
                        string actorName = MyEntities.CurrentAuditUsername ?? "System Administrator";
                        await _auditService.LogEventAsync(
                            req.TargetUserId, actorId > 0 ? actorId : (decimal?)null, actorName,
                            "SAVE_CHANNEL_RIGHTS", "SUCCESS",
                            $"Updated rights for channel {normChannel}. ParentEffective={isParentEffective}, RevokedSessions={revokedSessions}",
                            normChannel, null, "127.0.0.1", "Web Admin", correlationId
                        );

                        // 6. Commit transaction trước khi reload dữ liệu
                        transaction.Commit();

                        long newSecurityVersion = (long)(user.TOKEN_VERSION ?? 1) + 1;

                        // 7. Nạp lại cây quyền độc lập sau commit (không làm hỏng giao dịch đã hoàn tất)
                        PlatformChannelTreeDto updatedTree = null;
                        try
                        {
                            updatedTree = ResolveChannelTree(db, req.TargetUserId, normChannel, user.USERNAME);
                        }
                        catch (Exception reloadEx)
                        {
                            System.Diagnostics.Trace.TraceWarning("[SaveChannelRightsAsync] Reload error after commit: " + reloadEx.Message);
                        }

                        return new SaveChannelRightsResult
                        {
                            Success = true,
                            Message = "Cập nhật quyền nền tảng thành công.",
                            SessionsRevokedCount = revokedSessions,
                            UpdatedChannelTree = updatedTree,
                            RequiresReLogin = actorAffected,
                            NewSecurityVersion = newSecurityVersion
                        };
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { }
                        return new SaveChannelRightsResult
                        {
                            Success = false,
                            Message = "Lỗi khi lưu quyền nền tảng: " + ex.Message
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Lưu cấu hình quyền đồng thời cho nhiều kênh (Desktop, Web, Mobile) trong MỘT TRANSACTION duy nhất.
        /// Bảo đảm tính nguyên tử (atomic), chống ghi đè bằng ExpectedSecurityVersion,
        /// tăng TokenVersion đúng 1 lần và cảnh báo nếu admin tự sửa quyền của chính mình/nhóm chứa mình.
        /// </summary>
        public async Task<BatchSaveChannelRightsResult> SaveBatchChannelRightsAsync(BatchSaveChannelRightsRequest req)
        {
            if (req == null || req.TargetUserId <= 0 || req.Channels == null || !req.Channels.Any())
            {
                return new BatchSaveChannelRightsResult { Success = false, Message = "Dữ liệu yêu cầu không hợp lệ." };
            }

            string correlationId = req.CorrelationId ?? Guid.NewGuid().ToString("N");

            using (var db = new MyEntities())
            {
                using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // 0. Xác thực Actor (Người thực hiện phải là Quản trị viên hệ thống)
                        decimal actorId = req.ActorUserId;
                        if (actorId <= 0 && MyEntities.CurrentAuditUserId.HasValue && MyEntities.CurrentAuditUserId.Value > 0)
                        {
                            actorId = (decimal)MyEntities.CurrentAuditUserId.Value;
                        }

                        if (actorId > 0)
                        {
                            var actor = db.Database.SqlQuery<TargetUserRow>(@"
                                SELECT IDUSER, USERNAME, FULLNAME, DISABLED, ISGROUP, MANV, LOCKOUT_END, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION
                                FROM HR.TB_SYS_USER
                                WHERE IDUSER = :p0",
                                new OracleParameter("p0", actorId)
                            ).FirstOrDefault();

                            bool isActorAdmin = actor != null && actor.USERNAME != null && actor.USERNAME.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
                            if (!isActorAdmin)
                            {
                                transaction.Rollback();
                                return new BatchSaveChannelRightsResult
                                {
                                    Success = false,
                                    Message = "Từ chối truy cập: Thao tác phân quyền chỉ dành riêng cho Quản trị viên hệ thống (Admin)."
                                };
                            }
                        }

                        // 1. Lock Target User
                        var user = db.Database.SqlQuery<TargetUserRow>(@"
                            SELECT IDUSER, USERNAME, FULLNAME, DISABLED, ISGROUP, MANV, LOCKOUT_END, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION
                            FROM HR.TB_SYS_USER
                            WHERE IDUSER = :p0 FOR UPDATE",
                            new OracleParameter("p0", req.TargetUserId)
                        ).FirstOrDefault();

                        if (user == null)
                        {
                            transaction.Rollback();
                            return new BatchSaveChannelRightsResult { Success = false, Message = "Người dùng không tồn tại." };
                        }

                        // Kiểm tra phiên bản đồng thời (Concurrency check)
                        if (req.ExpectedSecurityVersion.HasValue && (user.TOKEN_VERSION ?? 1) != req.ExpectedSecurityVersion.Value)
                        {
                            transaction.Rollback();
                            return new BatchSaveChannelRightsResult
                            {
                                Success = false,
                                IsConflict = true,
                                Message = "Xung đột phiên bản: Dữ liệu phân quyền đã được quản trị viên khác cập nhật. Vui lòng tải lại dữ liệu mới nhất."
                            };
                        }

                        bool isTargetAdmin = user.USERNAME != null && user.USERNAME.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase);

                        // Kiểm tra tính hợp lệ trước khi thực hiện bất kỳ lệnh ghi nào
                        foreach (var chReq in req.Channels)
                        {
                            string norm = AppChannels.Normalize(chReq.Channel);
                            if (norm == null)
                            {
                                transaction.Rollback();
                                return new BatchSaveChannelRightsResult { Success = false, Message = $"Kênh truy cập '{chReq.Channel}' không hợp lệ." };
                            }
                            if (isTargetAdmin && norm == AppChannels.Mobile && chReq.ParentDirectGrant == true)
                            {
                                transaction.Rollback();
                                return new BatchSaveChannelRightsResult
                                {
                                    Success = false,
                                    Message = "Tài khoản Quản trị tối cao (ADMIN) bị cấm sử dụng ứng dụng di động Mobile."
                                };
                            }
                        }

                        bool hasChannelTable = CheckIfChannelTableExists(db);
                        int totalRevokedSessions = 0;
                        var affectedChannelsList = new List<string>();

                        // 2. Thực hiện cập nhật từng kênh trong cùng 1 transaction
                        foreach (var chReq in req.Channels)
                        {
                            string normChannel = AppChannels.Normalize(chReq.Channel);
                            affectedChannelsList.Add(normChannel);

                            // 2.1 Cập nhật quyền cha
                            string parentFunc = PlatformFunctionCodes.GetFunctionCodeForChannel(normChannel);
                            if (chReq.ParentDirectGrant.HasValue)
                            {
                                int val = chReq.ParentDirectGrant.Value ? 1 : 0;
                                db.Database.ExecuteSqlCommand(@"
                                    MERGE INTO HR.TB_SYS_RIGHT tgt
                                    USING (SELECT :p0 AS IDUSER, :p1 AS FUNCTION_CODE FROM DUAL) src
                                    ON (tgt.IDUSER = src.IDUSER AND tgt.FUNCTION_CODE = src.FUNCTION_CODE)
                                    WHEN MATCHED THEN
                                        UPDATE SET tgt.CAN_VIEW = :p2, tgt.USER_RIGHT = :p3,
                                                   tgt.CAN_ADD = 0, tgt.CAN_EDIT = 0, tgt.CAN_DELETE = 0, tgt.CAN_PRINT = 0
                                    WHEN NOT MATCHED THEN
                                        INSERT (IDUSER, FUNCTION_CODE, CAN_VIEW, USER_RIGHT, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
                                        VALUES (src.IDUSER, src.FUNCTION_CODE, :p4, :p5, 0, 0, 0, 0)",
                                    new OracleParameter("p0", req.TargetUserId),
                                    new OracleParameter("p1", parentFunc),
                                    new OracleParameter("p2", val),
                                    new OracleParameter("p3", val),
                                    new OracleParameter("p4", val),
                                    new OracleParameter("p5", val)
                                );
                            }

                            // Kiểm tra trạng thái cha sau khi merge
                            var parentRes = _platformResolver.ResolveChannel(db, req.TargetUserId, normChannel, user.USERNAME);
                            bool isParentEffective = parentRes.IsGranted;

                            // 2.2 Cập nhật quyền con
                            if (hasChannelTable && chReq.Functions != null && chReq.Functions.Any())
                            {
                                if (isParentEffective)
                                {
                                    foreach (var f in chReq.Functions)
                                    {
                                        if (PlatformFunctionCodes.IsPlatformFunction(f.FunctionCode)) continue;

                                        var cap = ChannelCapabilityRegistry.GetCapability(normChannel, f.FunctionCode);
                                        if (cap == null) continue;

                                        int view = (cap.CanView && f.CanView) ? 1 : 0;
                                        int add = (cap.CanAdd && f.CanAdd) ? 1 : 0;
                                        int edit = (cap.CanEdit && f.CanEdit) ? 1 : 0;
                                        int del = (cap.CanDelete && f.CanDelete) ? 1 : 0;
                                        int print = (cap.CanPrint && f.CanPrint) ? 1 : 0;

                                        db.Database.ExecuteSqlCommand(@"
                                            MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
                                            USING (SELECT :p0 AS IDUSER, :p1 AS CLIENT_TYPE, :p2 AS FUNCTION_CODE FROM DUAL) src
                                            ON (tgt.IDUSER = src.IDUSER AND tgt.CLIENT_TYPE = src.CLIENT_TYPE AND tgt.FUNCTION_CODE = src.FUNCTION_CODE)
                                            WHEN MATCHED THEN
                                                UPDATE SET tgt.CAN_VIEW = :p3, tgt.CAN_ADD = :p4, tgt.CAN_EDIT = :p5,
                                                           tgt.CAN_DELETE = :p6, tgt.CAN_PRINT = :p7, tgt.UPDATED_AT = SYSTIMESTAMP
                                            WHEN NOT MATCHED THEN
                                                INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
                                                VALUES (src.IDUSER, src.CLIENT_TYPE, src.FUNCTION_CODE, :p8, :p9, :p10, :p11, :p12)",
                                            new OracleParameter("p0", req.TargetUserId),
                                            new OracleParameter("p1", normChannel),
                                            new OracleParameter("p2", f.FunctionCode),
                                            new OracleParameter("p3", view),
                                            new OracleParameter("p4", add),
                                            new OracleParameter("p5", edit),
                                            new OracleParameter("p6", del),
                                            new OracleParameter("p7", print),
                                            new OracleParameter("p8", view),
                                            new OracleParameter("p9", add),
                                            new OracleParameter("p10", edit),
                                            new OracleParameter("p11", del),
                                            new OracleParameter("p12", print)
                                        );
                                    }
                                }
                            }

                            // 2.3 Thu hồi phiên cho kênh nếu cha bị tắt
                            if (!isParentEffective)
                            {
                                totalRevokedSessions += RevokeSessionsForChannel(db, req.TargetUserId, normChannel, "PARENT_PLATFORM_DISABLED");
                            }
                        }

                        // 3. Tăng TokenVersion DUY NHẤT 1 LẦN cho đối tượng
                        bool isGroup = (user.ISGROUP ?? 0) == 1;
                        if (isGroup)
                        {
                            var memberIds = db.TB_SYS_GROUP.Where(g => g.ID_GROUP == req.TargetUserId).Select(g => g.MEMBER).ToList();
                            foreach (var memberId in memberIds)
                            {
                                foreach (var chReq in req.Channels)
                                {
                                    string norm = AppChannels.Normalize(chReq.Channel);
                                    var parentRes = _platformResolver.ResolveChannel(db, req.TargetUserId, norm, user.USERNAME);
                                    if (!parentRes.IsGranted)
                                    {
                                        totalRevokedSessions += RevokeSessionsForChannel(db, memberId, norm, "GROUP_PLATFORM_DISABLED");
                                    }
                                }
                                db.Database.ExecuteSqlCommand(
                                    "UPDATE HR.TB_SYS_USER SET TOKEN_VERSION = NVL(TOKEN_VERSION, 1) + 1 WHERE IDUSER = :p0",
                                    new OracleParameter("p0", memberId)
                                );
                            }
                        }
                        else
                        {
                            db.Database.ExecuteSqlCommand(
                                "UPDATE HR.TB_SYS_USER SET TOKEN_VERSION = NVL(TOKEN_VERSION, 1) + 1 WHERE IDUSER = :p0",
                                new OracleParameter("p0", req.TargetUserId)
                            );
                        }

                        // Kiểm tra xem Actor có nằm trong đối tượng bị tác động không
                        bool actorAffected = actorId > 0 && (
                            actorId == req.TargetUserId ||
                            (isGroup && db.TB_SYS_GROUP.Any(g => g.ID_GROUP == req.TargetUserId && g.MEMBER == actorId))
                        );

                        // 4. Ghi Audit Log cho đợt batch
                        string actorName = MyEntities.CurrentAuditUsername ?? "System Administrator";
                        string summaryChannels = string.Join(", ", affectedChannelsList);
                        await _auditService.LogEventAsync(
                            req.TargetUserId, actorId > 0 ? actorId : (decimal?)null, actorName,
                            "SAVE_BATCH_CHANNEL_RIGHTS", "SUCCESS",
                            $"Batch updated rights for channels [{summaryChannels}]. RevokedSessions={totalRevokedSessions}, ActorAffected={actorAffected}",
                            summaryChannels, null, "127.0.0.1", "Web Admin", correlationId
                        );

                        // 5. Commit Transaction
                        transaction.Commit();

                        long newSecVersion = (long)(user.TOKEN_VERSION ?? 1) + 1;

                        // 6. Nạp lại cây quyền độc lập sau commit
                        var updatedTrees = new List<PlatformChannelTreeDto>();
                        foreach (var chReq in req.Channels)
                        {
                            string norm = AppChannels.Normalize(chReq.Channel);
                            if (norm != null)
                            {
                                try
                                {
                                    updatedTrees.Add(ResolveChannelTree(db, req.TargetUserId, norm, user.USERNAME));
                                }
                                catch (Exception reloadEx)
                                {
                                    System.Diagnostics.Trace.TraceWarning("[SaveBatchChannelRightsAsync] Reload error for channel " + norm + ": " + reloadEx.Message);
                                }
                            }
                        }

                        return new BatchSaveChannelRightsResult
                        {
                            Success = true,
                            Message = "Cập nhật quyền tất cả các kênh thành công.",
                            NewSecurityVersion = newSecVersion,
                            RequiresReLogin = actorAffected,
                            SessionsRevokedCount = totalRevokedSessions,
                            UpdatedChannelTrees = updatedTrees
                        };
                    }
                    catch (Exception ex)
                    {
                        try { transaction.Rollback(); } catch { }
                        return new BatchSaveChannelRightsResult
                        {
                            Success = false,
                            Message = "Lỗi khi lưu quyền theo kênh (batch): " + ex.Message
                        };
                    }
                }
            }
        }

        private int RevokeSessionsForChannel(MyEntities db, decimal userId, string channel, string reason)
        {
            return db.Database.ExecuteSqlCommand(@"
                UPDATE HR.TB_AUTH_SESSION
                SET REVOKED_AT = CURRENT_TIMESTAMP,
                    REVOKE_REASON = :p0
                WHERE USER_ID = :p1 
                  AND UPPER(TRIM(CLIENT_TYPE)) = :p2 
                  AND REVOKED_AT IS NULL 
                  AND EXPIRES_AT > CURRENT_TIMESTAMP",
                new OracleParameter("p0", reason),
                new OracleParameter("p1", userId),
                new OracleParameter("p2", channel.ToUpperInvariant())
            );
        }

        public void ProjectEffectiveRights(
            PlatformChannelTreeDto channelTree,
            out Dictionary<string, Bu.DTO.UserRightDetail> detailedRights,
            out List<string> viewableRights)
        {
            detailedRights = new Dictionary<string, Bu.DTO.UserRightDetail>(StringComparer.OrdinalIgnoreCase);
            viewableRights = new List<string>();

            if (channelTree == null) return;

            // 1. Chieu quyen cha kenh vao DetailedRights va viewableRights
            if (!string.IsNullOrEmpty(channelTree.ParentFunctionCode))
            {
                bool parentEffective = channelTree.ParentIsEffective;
                detailedRights[channelTree.ParentFunctionCode] = new Bu.DTO.UserRightDetail
                {
                    FUNCTION_CODE = channelTree.ParentFunctionCode,
                    DESCRIPTION = channelTree.ParentFunctionName ?? channelTree.ParentFunctionCode,
                    CAN_VIEW = parentEffective,
                    CAN_ADD = false,
                    CAN_EDIT = false,
                    CAN_DELETE = false,
                    CAN_PRINT = false
                };

                if (parentEffective)
                {
                    viewableRights.Add(channelTree.ParentFunctionCode);
                }
            }

            // 2. Chieu danh sach chuc nang con
            if (channelTree.Functions != null)
            {
                foreach (var item in channelTree.Functions)
                {
                    if (item == null || string.IsNullOrEmpty(item.FunctionCode)) continue;

                    bool canView = item.EffectiveGrant != null && item.EffectiveGrant.CanView;
                    bool canAdd = item.EffectiveGrant != null && item.EffectiveGrant.CanAdd;
                    bool canEdit = item.EffectiveGrant != null && item.EffectiveGrant.CanEdit;
                    bool canDelete = item.EffectiveGrant != null && item.EffectiveGrant.CanDelete;
                    bool canPrint = item.EffectiveGrant != null && item.EffectiveGrant.CanPrint;

                    detailedRights[item.FunctionCode] = new Bu.DTO.UserRightDetail
                    {
                        FUNCTION_CODE = item.FunctionCode,
                        DESCRIPTION = item.FunctionName,
                        CAN_VIEW = canView,
                        CAN_ADD = canAdd,
                        CAN_EDIT = canEdit,
                        CAN_DELETE = canDelete,
                        CAN_PRINT = canPrint
                    };

                    if (canView && !viewableRights.Contains(item.FunctionCode))
                    {
                        viewableRights.Add(item.FunctionCode);
                    }
                }
            }
        }
    }
}
