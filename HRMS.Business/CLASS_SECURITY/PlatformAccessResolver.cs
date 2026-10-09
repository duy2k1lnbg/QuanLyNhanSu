using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Bu.CLASS_SECURITY
{
    public class PlatformResolutionResult
    {
        public string Channel { get; set; }
        public string FunctionCode { get; set; }
        public bool IsGranted { get; set; }
        public bool DirectGrant { get; set; }
        public bool InheritedGrant { get; set; }
        public List<string> InheritedFromGroupNames { get; set; } = new List<string>();
        public bool HasMismatchWarning { get; set; }
        public string ReadinessCode { get; set; }
        public string ReadinessMessage { get; set; }
    }

    public interface IPlatformAccessResolver
    {
        PlatformResolutionResult ResolveChannel(MyEntities db, decimal userId, string channel, string username = null);
        Task<PlatformResolutionResult> ResolveChannelAsync(MyEntities db, decimal userId, string channel, string username = null);
        List<PlatformResolutionResult> ResolveAllChannels(MyEntities db, decimal userId, string username = null);
        Task<List<PlatformResolutionResult>> ResolveAllChannelsAsync(MyEntities db, decimal userId, string username = null);
        bool CanLoginChannel(MyEntities db, decimal userId, string channel, out string errorCode, out string errorMessage, string username = null);
        bool CanLoginChannel(decimal userId, string channel, out string errorCode, out string errorMessage, string username = null);
        bool CanLoginChannel(decimal userId, string channel);
    }

    public class PlatformAccessResolver : IPlatformAccessResolver
    {
        private class RightCheckRow
        {
            public decimal IDUSER { get; set; }
            public string FUNCTION_CODE { get; set; }
            public decimal? CAN_VIEW { get; set; }
            public decimal? USER_RIGHT { get; set; }
        }

        private class GroupInfoRow
        {
            public decimal ID_GROUP { get; set; }
            public string GROUP_NAME { get; set; }
            public decimal? DISABLED { get; set; }
            public decimal? ISGROUP { get; set; }
        }

        private class EmpStatusRow
        {
            public decimal MANV { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public string HOTEN { get; set; }
            public decimal? DATHOIVIEC { get; set; }
        }

        public PlatformResolutionResult ResolveChannel(MyEntities db, decimal userId, string channel, string username = null)
        {
            string normChannel = AppChannels.Normalize(channel);
            if (normChannel == null)
            {
                return new PlatformResolutionResult
                {
                    Channel = channel,
                    IsGranted = false,
                    ReadinessCode = "INVALID_CHANNEL",
                    ReadinessMessage = "Kênh truy cập không hợp lệ."
                };
            }

            string funcCode = PlatformFunctionCodes.GetFunctionCodeForChannel(normChannel);

            // 1. Kiểm tra tài khoản Root Admin
            bool isRootAdmin = false;
            if (!string.IsNullOrEmpty(username))
            {
                isRootAdmin = username.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                var userRow = db.Database.SqlQuery<string>(
                    "SELECT USERNAME FROM HR.TB_SYS_USER WHERE IDUSER = :p0",
                    new OracleParameter("p0", userId)
                ).FirstOrDefault();
                isRootAdmin = (userRow != null && userRow.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase));
            }

            // Quy tắc: ADMIN cấm tuyệt đối MOBILE
            if (isRootAdmin && normChannel == AppChannels.Mobile)
            {
                return new PlatformResolutionResult
                {
                    Channel = normChannel,
                    FunctionCode = funcCode,
                    IsGranted = false,
                    DirectGrant = false,
                    InheritedGrant = false,
                    ReadinessCode = "ROOT_ADMIN_MOBILE_FORBIDDEN",
                    ReadinessMessage = "Tài khoản Quản trị tối cao (ADMIN) không áp dụng cho ứng dụng di động Mobile."
                };
            }

            // 2. Direct grant
            var directRight = db.Database.SqlQuery<RightCheckRow>(@"
                SELECT IDUSER, FUNCTION_CODE, CAN_VIEW, USER_RIGHT
                FROM HR.TB_SYS_RIGHT
                WHERE IDUSER = :p0 AND FUNCTION_CODE = :p1",
                new OracleParameter("p0", userId),
                new OracleParameter("p1", funcCode)
            ).FirstOrDefault();

            bool directGrant = false;
            bool mismatchWarning = false;

            if (directRight != null)
            {
                decimal canView = directRight.CAN_VIEW ?? 0;
                decimal userRight = directRight.USER_RIGHT ?? 0;
                if (canView != userRight)
                {
                    mismatchWarning = true;
                }
                // Zero-Trust: Chỉ đọc CAN_VIEW, không OR với USER_RIGHT để tránh mở quyền ngoài ý muốn
                directGrant = (canView == 1);
            }

            // 3. Group grants (chỉ lấy nhóm hợp lệ: ISGROUP=1, DISABLED=0)
            var activeGroups = db.Database.SqlQuery<GroupInfoRow>(@"
                SELECT g.ID_GROUP, u.USERNAME AS GROUP_NAME, u.DISABLED, u.ISGROUP
                FROM HR.TB_SYS_GROUP g
                JOIN HR.TB_SYS_USER u ON g.ID_GROUP = u.IDUSER
                WHERE g.MEMBER = :p0 AND NVL(u.ISGROUP, 0) = 1 AND NVL(u.DISABLED, 0) = 0",
                new OracleParameter("p0", userId)
            ).ToList();

            bool inheritedGrant = false;
            var grantedGroups = new List<string>();

            if (activeGroups.Any())
            {
                var groupIds = activeGroups.Select(g => g.ID_GROUP).ToList();
                // Query group rights
                string inClause = string.Join(",", groupIds.Select((id, idx) => $":g{idx}"));
                var parameters = new List<OracleParameter>();
                for (int i = 0; i < groupIds.Count; i++)
                {
                    parameters.Add(new OracleParameter($"g{i}", groupIds[i]));
                }
                parameters.Add(new OracleParameter("fcode", funcCode));

                string sql = $@"
                    SELECT r.IDUSER, r.FUNCTION_CODE, r.CAN_VIEW, r.USER_RIGHT
                    FROM HR.TB_SYS_RIGHT r
                    WHERE r.IDUSER IN ({inClause}) AND r.FUNCTION_CODE = :fcode";

                var groupRights = db.Database.SqlQuery<RightCheckRow>(sql, parameters.ToArray()).ToList();

                foreach (var gr in groupRights)
                {
                    if ((gr.CAN_VIEW ?? 0) == 1)
                    {
                        inheritedGrant = true;
                        var grpObj = activeGroups.FirstOrDefault(x => x.ID_GROUP == gr.IDUSER);
                        if (grpObj != null && !grantedGroups.Contains(grpObj.GROUP_NAME))
                        {
                            grantedGroups.Add(grpObj.GROUP_NAME);
                        }
                    }
                }
            }

            bool isGranted = directGrant || inheritedGrant;

            // 4. Đánh giá tính sẵn sàng sử dụng (Readiness)
            string readinessCode = "READY";
            string readinessMessage = "Sẵn sàng sử dụng.";

            if (!isGranted)
            {
                readinessCode = "NOT_GRANTED";
                readinessMessage = PlatformErrorCodes.GetFriendlyMessage(PlatformErrorCodes.PlatformAccessDenied, normChannel);
            }
            else
            {
                // Nếu được cấp quyền, kiểm tra điều kiện hồ sơ bổ sung
                if (normChannel == AppChannels.Mobile)
                {
                    var mapRow = db.Database.SqlQuery<MappingRow>(@"
                        SELECT USER_ID, EMPLOYEE_ID, IS_MOBILE_ENABLED
                        FROM HR.TB_USER_EMPLOYEE_MAPPING
                        WHERE USER_ID = :p0 AND ROWNUM = 1",
                        new OracleParameter("p0", userId)
                    ).FirstOrDefault();

                    if (mapRow == null || !mapRow.EMPLOYEE_ID.HasValue || mapRow.EMPLOYEE_ID.Value <= 0)
                    {
                        readinessCode = "MISSING_MAPPING";
                        readinessMessage = "Tài khoản chưa được liên kết với hồ sơ nhân viên để sử dụng Mobile.";
                    }
                    else if ((mapRow.IS_MOBILE_ENABLED ?? 1) == 0)
                    {
                        readinessCode = "MOBILE_DISABLED";
                        readinessMessage = "Quyền sử dụng Mobile đang bị tạm dừng cho hồ sơ này.";
                    }
                    else
                    {
                        // Kiểm tra nhân viên tồn tại và chưa thôi việc
                        var empRow = db.Database.SqlQuery<EmpStatusRow>(@"
                            SELECT MANV, EMPLOYEE_CODE, HOTEN, DATHOIVIEC
                            FROM HR.TB_NHANVIEN
                            WHERE MANV = :p0 AND ROWNUM = 1",
                            new OracleParameter("p0", mapRow.EMPLOYEE_ID.Value)
                        ).FirstOrDefault();

                        if (empRow == null)
                        {
                            readinessCode = "EMPLOYEE_NOT_FOUND";
                            readinessMessage = "Hồ sơ nhân viên liên kết không tồn tại trong hệ thống.";
                        }
                        else if (empRow.DATHOIVIEC.HasValue && empRow.DATHOIVIEC.Value == 1)
                        {
                            readinessCode = "EMPLOYEE_TERMINATED";
                            readinessMessage = "Hồ sơ nhân viên đã thôi việc, không được phép truy cập Mobile.";
                        }
                    }
                }
            }

            return new PlatformResolutionResult
            {
                Channel = normChannel,
                FunctionCode = funcCode,
                IsGranted = isGranted,
                DirectGrant = directGrant,
                InheritedGrant = inheritedGrant,
                InheritedFromGroupNames = grantedGroups,
                HasMismatchWarning = mismatchWarning,
                ReadinessCode = readinessCode,
                ReadinessMessage = readinessMessage
            };
        }

        private static bool? _isCutoverAppliedCache;
        private static DateTime _cutoverCacheTime = DateTime.MinValue;

        private bool IsCutoverApplied(MyEntities db)
        {
            if (_isCutoverAppliedCache.HasValue && (DateTime.UtcNow - _cutoverCacheTime).TotalSeconds < 30)
            {
                return _isCutoverAppliedCache.Value;
            }

            try
            {
                var configVal = db.Database.SqlQuery<string>(
                    "SELECT CONFIG_VALUE FROM HR.TB_SYS_CONFIG WHERE CONFIG_KEY = 'PLATFORM_ACCESS_CUTOVER_APPLIED' AND ROWNUM = 1"
                ).FirstOrDefault();

                bool applied = configVal != null && configVal.Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                _isCutoverAppliedCache = applied;
                _cutoverCacheTime = DateTime.UtcNow;
                return applied;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PlatformResolutionResult> ResolveChannelAsync(MyEntities db, decimal userId, string channel, string username = null)
        {
            return await Task.Run(() => ResolveChannel(db, userId, channel, username));
        }

        public List<PlatformResolutionResult> ResolveAllChannels(MyEntities db, decimal userId, string username = null)
        {
            var results = new List<PlatformResolutionResult>();
            results.Add(ResolveChannel(db, userId, AppChannels.Desktop, username));
            results.Add(ResolveChannel(db, userId, AppChannels.Web, username));
            results.Add(ResolveChannel(db, userId, AppChannels.Mobile, username));
            return results;
        }

        public async Task<List<PlatformResolutionResult>> ResolveAllChannelsAsync(MyEntities db, decimal userId, string username = null)
        {
            return await Task.Run(() => ResolveAllChannels(db, userId, username));
        }

        public bool CanLoginChannel(MyEntities db, decimal userId, string channel, out string errorCode, out string errorMessage, string username = null)
        {
            errorCode = null;
            errorMessage = null;

            var resolution = ResolveChannel(db, userId, channel, username);
            if (!resolution.IsGranted)
            {
                errorCode = PlatformErrorCodes.PlatformAccessDenied;
                errorMessage = PlatformErrorCodes.GetFriendlyMessage(PlatformErrorCodes.PlatformAccessDenied, channel);
                return false;
            }

            if (resolution.ReadinessCode != "READY")
            {
                if (resolution.ReadinessCode == "MISSING_MAPPING")
                {
                    errorCode = PlatformErrorCodes.EmployeeLinkRequired;
                    errorMessage = PlatformErrorCodes.GetFriendlyMessage(PlatformErrorCodes.EmployeeLinkRequired);
                }
                else if (resolution.ReadinessCode == "MOBILE_DISABLED")
                {
                    errorCode = PlatformErrorCodes.MobileDisabled;
                    errorMessage = PlatformErrorCodes.GetFriendlyMessage(PlatformErrorCodes.MobileDisabled);
                }
                else
                {
                    errorCode = resolution.ReadinessCode;
                    errorMessage = resolution.ReadinessMessage;
                }
                return false;
            }

            return true;
        }

        public bool CanLoginChannel(decimal userId, string channel, out string errorCode, out string errorMessage, string username = null)
        {
            using (var db = new MyEntities())
            {
                return CanLoginChannel(db, userId, channel, out errorCode, out errorMessage, username);
            }
        }

        public bool CanLoginChannel(decimal userId, string channel)
        {
            string errCode, errMsg;
            return CanLoginChannel(userId, channel, out errCode, out errMsg);
        }
    }
}
