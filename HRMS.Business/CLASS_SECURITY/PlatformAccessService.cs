using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Bu.CLASS_SECURITY
{
    public interface IPlatformAccessService
    {
        Task<UserPlatformSummaryDto> GetUserPlatformSummaryAsync(decimal targetUserId);
        Task<SavePlatformRightsResult> SavePlatformAccessAsync(SavePlatformRightsRequest req);
        Task<bool> ToggleMobileAccessAsync(decimal targetUserId, bool isEnabled, decimal actorUserId, string correlationId);
    }

    public class PlatformAccessService : IPlatformAccessService
    {
        private readonly IPlatformAccessResolver _resolver;
        private readonly IAuthAuditService _auditService;

        public PlatformAccessService(IPlatformAccessResolver resolver = null, IAuthAuditService auditService = null)
        {
            _resolver = resolver ?? new PlatformAccessResolver();
            _auditService = auditService ?? new AuthAuditService();
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

        private class EmpInfoRow
        {
            public decimal MANV { get; set; }
            public string EMPLOYEE_CODE { get; set; }
            public string HOTEN { get; set; }
        }

        public async Task<UserPlatformSummaryDto> GetUserPlatformSummaryAsync(decimal targetUserId)
        {
            using (var db = new MyEntities())
            {
                var user = db.Database.SqlQuery<TargetUserRow>(@"
                    SELECT IDUSER, USERNAME, FULLNAME, DISABLED, ISGROUP, MANV, LOCKOUT_END, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION
                    FROM HR.TB_SYS_USER
                    WHERE IDUSER = :p0",
                    new OracleParameter("p0", targetUserId)
                ).FirstOrDefault();

                if (user == null) return null;

                bool isGroup = (user.ISGROUP ?? 0) == 1;
                bool isAdmin = user.USERNAME != null && user.USERNAME.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
                bool isDisabled = (user.DISABLED ?? 0) == 1;
                bool isLockedOut = user.LOCKOUT_END.HasValue && user.LOCKOUT_END.Value > DateTime.Now;

                // Mapping info
                var mapRow = db.Database.SqlQuery<MappingRow>(@"
                    SELECT USER_ID, EMPLOYEE_ID, IS_MOBILE_ENABLED
                    FROM HR.TB_USER_EMPLOYEE_MAPPING
                    WHERE USER_ID = :p0 AND ROWNUM = 1",
                    new OracleParameter("p0", targetUserId)
                ).FirstOrDefault();

                decimal? manv = null;
                string empCode = null;
                string empName = null;
                bool isMobileEnabled = true;

                if (!isAdmin && mapRow != null && mapRow.EMPLOYEE_ID.HasValue && mapRow.EMPLOYEE_ID.Value > 0)
                {
                    manv = mapRow.EMPLOYEE_ID.Value;
                    isMobileEnabled = (mapRow.IS_MOBILE_ENABLED ?? 1) == 1;
                    var emp = db.Database.SqlQuery<EmpInfoRow>(@"
                        SELECT MANV, EMPLOYEE_CODE, HOTEN
                        FROM HR.TB_NHANVIEN
                        WHERE MANV = :p0 AND ROWNUM = 1",
                        new OracleParameter("p0", manv.Value)
                    ).FirstOrDefault();

                    if (emp != null)
                    {
                        empCode = emp.EMPLOYEE_CODE;
                        empName = emp.HOTEN;
                    }
                }
                else if (!isAdmin && user.MANV.HasValue && user.MANV.Value > 0)
                {
                    manv = user.MANV.Value;
                    var emp = db.Database.SqlQuery<EmpInfoRow>(@"
                        SELECT MANV, EMPLOYEE_CODE, HOTEN
                        FROM HR.TB_NHANVIEN
                        WHERE MANV = :p0 AND ROWNUM = 1",
                        new OracleParameter("p0", manv.Value)
                    ).FirstOrDefault();

                    if (emp != null)
                    {
                        empCode = emp.EMPLOYEE_CODE;
                        empName = emp.HOTEN;
                    }
                }

                // Resolve platforms
                var resolutions = _resolver.ResolveAllChannels(db, targetUserId, user.USERNAME);
                var platformItems = new List<PlatformAccessItemDto>();

                foreach (var r in resolutions)
                {
                    string label = r.Channel == AppChannels.Desktop ? "Desktop WinForms" :
                                   (r.Channel == AppChannels.Web ? "Web Portal" : "Mobile App");

                    platformItems.Add(new PlatformAccessItemDto
                    {
                        Channel = r.Channel,
                        FunctionCode = r.FunctionCode,
                        ChannelLabel = label,
                        DirectGrant = r.DirectGrant,
                        InheritedGrant = r.InheritedGrant,
                        InheritedFromGroups = r.InheritedFromGroupNames,
                        IsEffective = r.IsGranted,
                        ReadinessCode = r.ReadinessCode,
                        ReadinessMessage = r.ReadinessMessage
                    });
                }

                return new UserPlatformSummaryDto
                {
                    UserId = user.IDUSER,
                    Username = user.USERNAME,
                    FullName = user.FULLNAME ?? user.USERNAME,
                    IsGroup = isGroup,
                    IsAdmin = isAdmin,
                    IsDisabled = isDisabled,
                    IsLockedOut = isLockedOut,
                    Manv = manv,
                    EmployeeCode = empCode,
                    EmployeeName = empName,
                    IsMobileEnabled = isMobileEnabled,
                    Platforms = platformItems
                };
            }
        }

        public async Task<SavePlatformRightsResult> SavePlatformAccessAsync(SavePlatformRightsRequest req)
        {
            if (req == null || req.TargetUserId <= 0)
            {
                return new SavePlatformRightsResult { Success = false, Message = "Dữ liệu yêu cầu không hợp lệ." };
            }

            string correlationId = req.CorrelationId ?? Guid.NewGuid().ToString("N");

            using (var db = new MyEntities())
            {
                using (var transaction = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // 1. Lock Target User
                        var user = db.Database.SqlQuery<TargetUserRow>(@"
                            SELECT IDUSER, USERNAME, FULLNAME, DISABLED, ISGROUP, MANV, LOCKOUT_END, NVL(TOKEN_VERSION, 1) AS TOKEN_VERSION
                            FROM HR.TB_SYS_USER
                            WHERE IDUSER = :p0
                            FOR UPDATE",
                            new OracleParameter("p0", req.TargetUserId)
                        ).FirstOrDefault();

                        if (user == null)
                        {
                            transaction.Rollback();
                            return new SavePlatformRightsResult { Success = false, Message = "Người dùng không tồn tại." };
                        }

                        bool isTargetAdmin = user.USERNAME != null && user.USERNAME.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase);

                        // Cấm cấp Mobile cho ADMIN
                        if (isTargetAdmin && req.MobileDirectGrant)
                        {
                            transaction.Rollback();
                            return new SavePlatformRightsResult
                            {
                                Success = false,
                                Message = "Tài khoản Quản trị tối cao (ADMIN) bị cấm sử dụng ứng dụng di động Mobile."
                            };
                        }

                        // 2. Lấy trạng thái quyền hiệu lực TRƯỚC khi cập nhật
                        var beforeResolutions = _resolver.ResolveAllChannels(db, req.TargetUserId, user.USERNAME);
                        bool beforeDesktopEffective = beforeResolutions.First(x => x.Channel == AppChannels.Desktop).IsGranted;
                        bool beforeWebEffective = beforeResolutions.First(x => x.Channel == AppChannels.Web).IsGranted;
                        bool beforeMobileEffective = beforeResolutions.First(x => x.Channel == AppChannels.Mobile).IsGranted;

                        // 3. Cập nhật direct grants cho 3 mã F_LOGIN_* trong TB_SYS_RIGHT
                        UpdateSinglePlatformRight(db, req.TargetUserId, PlatformFunctionCodes.LoginDesktop, req.DesktopDirectGrant);
                        UpdateSinglePlatformRight(db, req.TargetUserId, PlatformFunctionCodes.LoginWeb, req.WebDirectGrant);
                        UpdateSinglePlatformRight(db, req.TargetUserId, PlatformFunctionCodes.LoginMobile, isTargetAdmin ? false : req.MobileDirectGrant);

                        // 4. Cập nhật cờ IS_MOBILE_ENABLED nếu có truyền
                        bool mobileFlagChanged = false;
                        if (req.IsMobileEnabled.HasValue && !isTargetAdmin)
                        {
                            int targetFlag = req.IsMobileEnabled.Value ? 1 : 0;
                            int mapCount = db.Database.SqlQuery<int>(
                                "SELECT COUNT(*) FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                                new OracleParameter("p0", req.TargetUserId)
                            ).FirstOrDefault();

                            if (mapCount > 0)
                            {
                                int currentFlag = db.Database.SqlQuery<int>(
                                    "SELECT NVL(IS_MOBILE_ENABLED, 1) FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                                    new OracleParameter("p0", req.TargetUserId)
                                ).FirstOrDefault();

                                if (currentFlag != targetFlag)
                                {
                                    db.Database.ExecuteSqlCommand(@"
                                        UPDATE HR.TB_USER_EMPLOYEE_MAPPING 
                                        SET IS_MOBILE_ENABLED = :p0, UPDATED_AT = SYSDATE 
                                        WHERE USER_ID = :p1",
                                        new OracleParameter("p0", targetFlag),
                                        new OracleParameter("p1", req.TargetUserId)
                                    );
                                    mobileFlagChanged = true;
                                }
                            }
                        }

                        // 5. Tính lại quyền hiệu lực SAU khi cập nhật
                        var afterResolutions = _resolver.ResolveAllChannels(db, req.TargetUserId, user.USERNAME);
                        bool afterDesktopEffective = afterResolutions.First(x => x.Channel == AppChannels.Desktop).IsGranted;
                        bool afterWebEffective = afterResolutions.First(x => x.Channel == AppChannels.Web).IsGranted;
                        bool afterMobileEffective = afterResolutions.First(x => x.Channel == AppChannels.Mobile).IsGranted;

                        int revokedSessionsCount = 0;

                        // 6. Xử lý thu hồi phiên làm việc mục tiêu (Targeted Revocation):
                        // Kênh Desktop mất quyền -> thu hồi các phiên DESKTOP
                        if (beforeDesktopEffective && !afterDesktopEffective)
                        {
                            revokedSessionsCount += RevokeSessionsForChannel(db, req.TargetUserId, AppChannels.Desktop, AuthRevokeReasons.PlatformAccessRemoved);
                        }

                        // Kênh Web mất quyền -> thu hồi các phiên WEB
                        if (beforeWebEffective && !afterWebEffective)
                        {
                            revokedSessionsCount += RevokeSessionsForChannel(db, req.TargetUserId, AppChannels.Web, AuthRevokeReasons.PlatformAccessRemoved);
                        }

                        // Kênh Mobile mất quyền HOẶC công tắc mobile bị tắt -> thu hồi các phiên MOBILE
                        if ((beforeMobileEffective && !afterMobileEffective) || (mobileFlagChanged && req.IsMobileEnabled == false))
                        {
                            string reason = (beforeMobileEffective && !afterMobileEffective) 
                                ? AuthRevokeReasons.PlatformAccessRemoved 
                                : AuthRevokeReasons.MobileDisabled;
                            revokedSessionsCount += RevokeSessionsForChannel(db, req.TargetUserId, AppChannels.Mobile, reason);
                        }

                        // 7. Ghi Audit Log vào TB_AUTH_AUDIT cùng transaction
                        await _auditService.LogEventAsync(
                            req.ActorUserId,
                            req.TargetUserId,
                            null,
                            AuthAuditEvents.PlatformAccessGranted,
                            "SUCCESS",
                            $"Updated platform rights. Desktop={afterDesktopEffective}, Web={afterWebEffective}, Mobile={afterMobileEffective}. Sessions revoked: {revokedSessionsCount}.",
                            "SYSTEM",
                            null,
                            "127.0.0.1",
                            "PlatformAccessService",
                            correlationId,
                            new
                            {
                                TargetUserId = req.TargetUserId,
                                Desktop = req.DesktopDirectGrant,
                                Web = req.WebDirectGrant,
                                Mobile = req.MobileDirectGrant,
                                IsMobileEnabled = req.IsMobileEnabled,
                                RevokedSessions = revokedSessionsCount
                            }
                        );

                        transaction.Commit();

                        var summary = await GetUserPlatformSummaryAsync(req.TargetUserId);

                        return new SavePlatformRightsResult
                        {
                            Success = true,
                            Message = "Cập nhật quyền đăng nhập nền tảng thành công.",
                            SessionsRevokedCount = revokedSessionsCount,
                            UpdatedSummary = summary
                        };
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Trace.TraceError("[PlatformAccessService] SavePlatformAccessAsync error: " + ex);
                        return new SavePlatformRightsResult
                        {
                            Success = false,
                            Message = "Lỗi hệ thống khi lưu quyền nền tảng: " + ex.Message
                        };
                    }
                }
            }
        }

        public async Task<bool> ToggleMobileAccessAsync(decimal targetUserId, bool isEnabled, decimal actorUserId, string correlationId)
        {
            var req = new SavePlatformRightsRequest
            {
                TargetUserId = targetUserId,
                ActorUserId = actorUserId,
                CorrelationId = correlationId,
                IsMobileEnabled = isEnabled
            };

            using (var db = new MyEntities())
            {
                // Giữ nguyên các direct grant hiện tại
                var directRights = db.TB_SYS_RIGHT.Where(r => r.IDUSER == targetUserId && (r.CAN_VIEW == 1 || r.USER_RIGHT == 1)).ToList();
                req.DesktopDirectGrant = directRights.Any(r => r.FUNCTION_CODE == PlatformFunctionCodes.LoginDesktop);
                req.WebDirectGrant = directRights.Any(r => r.FUNCTION_CODE == PlatformFunctionCodes.LoginWeb);
                req.MobileDirectGrant = directRights.Any(r => r.FUNCTION_CODE == PlatformFunctionCodes.LoginMobile);
            }

            var result = await SavePlatformAccessAsync(req);
            return result.Success;
        }

        private void UpdateSinglePlatformRight(MyEntities db, decimal userId, string funcCode, bool isGranted)
        {
            int canView = isGranted ? 1 : 0;
            var existing = db.TB_SYS_RIGHT.FirstOrDefault(r => r.IDUSER == userId && r.FUNCTION_CODE == funcCode);

            if (existing == null)
            {
                db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT
                {
                    IDUSER = userId,
                    FUNCTION_CODE = funcCode,
                    CAN_VIEW = canView,
                    USER_RIGHT = canView,
                    CAN_ADD = 0,
                    CAN_EDIT = 0,
                    CAN_DELETE = 0,
                    CAN_PRINT = 0
                });
            }
            else
            {
                existing.CAN_VIEW = canView;
                existing.USER_RIGHT = canView;
                existing.CAN_ADD = 0;
                existing.CAN_EDIT = 0;
                existing.CAN_DELETE = 0;
                existing.CAN_PRINT = 0;
            }

            db.SaveChanges();
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
    }
}
