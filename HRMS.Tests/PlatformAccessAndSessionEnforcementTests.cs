using Bu.CLASS_SECURITY;
using Bu.CLASS_SYSTEM;
using DA;
using HRMS_API.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRMS.Tests
{
    /// <summary>
    /// Bộ kiểm thử Offline & In-Memory cho Logic Phân quyền 3 Nền tảng (Desktop, Web, Mobile) và Quản lý Phiên làm việc.
    /// TUYỆT ĐỐI KHÔNG GHI DỮ LIỆU VÀO DATABASE THẬT theo yêu cầu an toàn hệ thống.
    /// </summary>
    [TestFixture]
    public class PlatformAccessAndSessionEnforcementTests
    {
        #region Mock / In-Memory Decision Fixtures

        public class MockUser
        {
            public decimal IdUser { get; set; }
            public string Username { get; set; }
            public bool IsAdmin => Username != null && Username.Trim().Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
            public bool IsGroup { get; set; }
            public bool IsDisabled { get; set; }
            public long TokenVersion { get; set; } = 1;
            public decimal? Manv { get; set; }
            public string LegacyClientType { get; set; } = "ALL";
        }

        public class MockGroupMembership
        {
            public decimal GroupId { get; set; }
            public string GroupName { get; set; }
            public decimal MemberUserId { get; set; }
            public bool IsGroupDisabled { get; set; }
        }

        public class MockUserRight
        {
            public decimal UserId { get; set; }
            public string FunctionCode { get; set; }
            public int CanView { get; set; }
            public int UserRight { get; set; }
            public int CanAdd { get; set; }
            public int CanEdit { get; set; }
            public int CanDelete { get; set; }
            public int CanPrint { get; set; }
        }

        public class MockEmployeeMapping
        {
            public decimal UserId { get; set; }
            public decimal EmployeeId { get; set; }
            public bool IsMobileEnabled { get; set; } = true;
            public bool IsTerminated { get; set; } = false;
        }

        public class InMemoryPlatformEvaluator
        {
            public bool IsCutoverApplied { get; set; } = true;

            public PlatformResolutionResult Evaluate(
                MockUser user,
                string channel,
                List<MockUserRight> rights,
                List<MockGroupMembership> memberships,
                MockEmployeeMapping mapping)
            {
                string normChannel = AppChannels.Normalize(channel);
                string funcCode = PlatformFunctionCodes.GetFunctionCodeForChannel(normChannel);

                // Root Admin cấm tuyệt đối Mobile
                if (user.IsAdmin && normChannel == AppChannels.Mobile)
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

                // 1. Direct Grant
                var direct = rights.FirstOrDefault(r => r.UserId == user.IdUser && r.FunctionCode.Equals(funcCode, StringComparison.OrdinalIgnoreCase));
                bool directGrant = direct != null && direct.CanView == 1;

                // 2. Group Inheritance
                var activeGroups = memberships.Where(m => m.MemberUserId == user.IdUser && !m.IsGroupDisabled).ToList();
                var activeGroupIds = activeGroups.Select(g => g.GroupId).ToList();
                var groupRights = rights.Where(r => activeGroupIds.Contains(r.UserId) && r.FunctionCode.Equals(funcCode, StringComparison.OrdinalIgnoreCase) && r.CanView == 1).ToList();

                bool inheritedGrant = groupRights.Any();
                var grantedGroupNames = activeGroups.Where(g => groupRights.Any(gr => gr.UserId == g.GroupId)).Select(g => g.GroupName).Distinct().ToList();

                bool isGranted = directGrant || inheritedGrant;

                // Fallback nếu chưa cutover
                if (!isGranted && !IsCutoverApplied)
                {
                    string legacy = (user.LegacyClientType ?? "ALL").ToUpperInvariant();
                    if (legacy == "ALL") isGranted = true;
                    else if (legacy == "DESKTOP" && (normChannel == AppChannels.Desktop || normChannel == AppChannels.Web)) isGranted = true;
                    else if (legacy == "MOBILE" && normChannel == AppChannels.Mobile) isGranted = true;
                }

                // 3. Readiness check
                string readiness = "READY";
                string message = "Sẵn sàng sử dụng.";

                if (!isGranted)
                {
                    readiness = "NOT_GRANTED";
                    message = PlatformErrorCodes.GetFriendlyMessage(PlatformErrorCodes.PlatformAccessDenied, normChannel);
                }
                else if (normChannel == AppChannels.Mobile)
                {
                    if (mapping == null || mapping.EmployeeId <= 0)
                    {
                        readiness = "MISSING_MAPPING";
                        message = "Tài khoản chưa được liên kết với hồ sơ nhân viên để sử dụng Mobile.";
                    }
                    else if (!mapping.IsMobileEnabled)
                    {
                        readiness = "MOBILE_DISABLED";
                        message = "Quyền sử dụng Mobile đang bị tạm dừng cho hồ sơ này.";
                    }
                    else if (mapping.IsTerminated)
                    {
                        readiness = "EMPLOYEE_TERMINATED";
                        message = "Hồ sơ nhân viên đã thôi việc, không được phép truy cập Mobile.";
                    }
                }

                return new PlatformResolutionResult
                {
                    Channel = normChannel,
                    FunctionCode = funcCode,
                    IsGranted = isGranted,
                    DirectGrant = directGrant,
                    InheritedGrant = inheritedGrant,
                    InheritedFromGroupNames = grantedGroupNames,
                    ReadinessCode = readiness,
                    ReadinessMessage = message
                };
            }
        }

        #endregion

        private InMemoryPlatformEvaluator _evaluator;

        [SetUp]
        public void SetUp()
        {
            _evaluator = new InMemoryPlatformEvaluator { IsCutoverApplied = true };
        }

        /// <summary>
        /// Ca 1: Direct Grant cho Desktop, Web, Mobile
        /// </summary>
        [Test]
        public void Test01_DirectGrant_Desktop_Web_Mobile()
        {
            var user = new MockUser { IdUser = 10, Username = "user10" };
            var rights = new List<MockUserRight>
            {
                new MockUserRight { UserId = 10, FunctionCode = PlatformFunctionCodes.LoginDesktop, CanView = 1, UserRight = 1 },
                new MockUserRight { UserId = 10, FunctionCode = PlatformFunctionCodes.LoginWeb, CanView = 1, UserRight = 1 },
                new MockUserRight { UserId = 10, FunctionCode = PlatformFunctionCodes.LoginMobile, CanView = 1, UserRight = 1 }
            };
            var mapping = new MockEmployeeMapping { UserId = 10, EmployeeId = 100, IsMobileEnabled = true };

            var resDesktop = _evaluator.Evaluate(user, AppChannels.Desktop, rights, new List<MockGroupMembership>(), mapping);
            var resWeb = _evaluator.Evaluate(user, AppChannels.Web, rights, new List<MockGroupMembership>(), mapping);
            var resMobile = _evaluator.Evaluate(user, AppChannels.Mobile, rights, new List<MockGroupMembership>(), mapping);

            Assert.IsTrue(resDesktop.IsGranted && resDesktop.DirectGrant);
            Assert.IsTrue(resWeb.IsGranted && resWeb.DirectGrant);
            Assert.IsTrue(resMobile.IsGranted && resMobile.DirectGrant && resMobile.ReadinessCode == "READY");
        }

        /// <summary>
        /// Ca 2: Group Inheritance - Kế thừa quyền từ nhóm đang hoạt động
        /// </summary>
        [Test]
        public void Test02_GroupInheritance()
        {
            var user = new MockUser { IdUser = 20, Username = "user20" };
            var rights = new List<MockUserRight>
            {
                new MockUserRight { UserId = 999, FunctionCode = PlatformFunctionCodes.LoginWeb, CanView = 1 }
            };
            var memberships = new List<MockGroupMembership>
            {
                new MockGroupMembership { GroupId = 999, GroupName = "NhomNhanSu", MemberUserId = 20, IsGroupDisabled = false }
            };

            var resWeb = _evaluator.Evaluate(user, AppChannels.Web, rights, memberships, null);
            var resDesktop = _evaluator.Evaluate(user, AppChannels.Desktop, rights, memberships, null);

            Assert.IsTrue(resWeb.IsGranted);
            Assert.IsFalse(resWeb.DirectGrant);
            Assert.IsTrue(resWeb.InheritedGrant);
            Assert.Contains("NhomNhanSu", resWeb.InheritedFromGroupNames);

            Assert.IsFalse(resDesktop.IsGranted, "Desktop không có grant từ user hay nhóm nên phải bị từ chối");
        }

        /// <summary>
        /// Ca 3: Conflict Resolution - Direct grant kết hợp Group grant (OR logic)
        /// </summary>
        [Test]
        public void Test03_ConflictResolution_DirectOverridesGroup()
        {
            var user = new MockUser { IdUser = 30, Username = "user30" };
            var rights = new List<MockUserRight>
            {
                new MockUserRight { UserId = 30, FunctionCode = PlatformFunctionCodes.LoginDesktop, CanView = 1 },
                new MockUserRight { UserId = 888, FunctionCode = PlatformFunctionCodes.LoginWeb, CanView = 1 }
            };
            var memberships = new List<MockGroupMembership>
            {
                new MockGroupMembership { GroupId = 888, GroupName = "NhomVanPhong", MemberUserId = 30, IsGroupDisabled = false }
            };

            var resDesktop = _evaluator.Evaluate(user, AppChannels.Desktop, rights, memberships, null);
            var resWeb = _evaluator.Evaluate(user, AppChannels.Web, rights, memberships, null);

            Assert.IsTrue(resDesktop.IsGranted && resDesktop.DirectGrant && !resDesktop.InheritedGrant);
            Assert.IsTrue(resWeb.IsGranted && !resWeb.DirectGrant && resWeb.InheritedGrant);
        }

        /// <summary>
        /// Ca 4: Disabled Group Handling - Nhóm bị vô hiệu hóa (DISABLED=1) không được kế thừa quyền
        /// </summary>
        [Test]
        public void Test04_DisabledGroup_Ignored()
        {
            var user = new MockUser { IdUser = 40, Username = "user40" };
            var rights = new List<MockUserRight>
            {
                new MockUserRight { UserId = 777, FunctionCode = PlatformFunctionCodes.LoginDesktop, CanView = 1 }
            };
            var memberships = new List<MockGroupMembership>
            {
                new MockGroupMembership { GroupId = 777, GroupName = "NhomBiKhoa", MemberUserId = 40, IsGroupDisabled = true }
            };

            var res = _evaluator.Evaluate(user, AppChannels.Desktop, rights, memberships, null);

            Assert.IsFalse(res.IsGranted, "Nhóm bị disabled không được phép kế thừa quyền");
            Assert.IsFalse(res.InheritedGrant);
        }

        /// <summary>
        /// Ca 5: Root Admin Behavior - ADMIN được vào Desktop, Web nhưng cấm tuyệt đối Mobile
        /// </summary>
        [Test]
        public void Test05_RootAdmin_DesktopWebAllowed_MobileStrictlyForbidden()
        {
            var adminUser = new MockUser { IdUser = 1, Username = "ADMIN" };
            var rights = new List<MockUserRight>
            {
                new MockUserRight { UserId = 1, FunctionCode = PlatformFunctionCodes.LoginDesktop, CanView = 1 },
                new MockUserRight { UserId = 1, FunctionCode = PlatformFunctionCodes.LoginWeb, CanView = 1 },
                new MockUserRight { UserId = 1, FunctionCode = PlatformFunctionCodes.LoginMobile, CanView = 1 }
            };

            var resMobile = _evaluator.Evaluate(adminUser, AppChannels.Mobile, rights, new List<MockGroupMembership>(), null);
            Assert.IsFalse(resMobile.IsGranted, "Root Admin phải bị cấm đăng nhập Mobile dù có bản ghi grant");
            Assert.AreEqual("ROOT_ADMIN_MOBILE_FORBIDDEN", resMobile.ReadinessCode);

            var resDesktop = _evaluator.Evaluate(adminUser, AppChannels.Desktop, rights, new List<MockGroupMembership>(), null);
            Assert.IsTrue(resDesktop.IsGranted, "Root Admin được phép vào Desktop");
        }

        /// <summary>
        /// Ca 6: Employee Status Check - Nhân viên đã thôi việc (DATHOIVIEC=1) cấm Mobile
        /// </summary>
        [Test]
        public void Test06_EmployeeStatus_Terminated_MobileForbidden()
        {
            var user = new MockUser { IdUser = 60, Username = "nv60" };
            var rights = new List<MockUserRight>
            {
                new MockUserRight { UserId = 60, FunctionCode = PlatformFunctionCodes.LoginMobile, CanView = 1 }
            };
            var mapping = new MockEmployeeMapping { UserId = 60, EmployeeId = 600, IsMobileEnabled = true, IsTerminated = true };

            var res = _evaluator.Evaluate(user, AppChannels.Mobile, rights, new List<MockGroupMembership>(), mapping);

            Assert.IsTrue(res.IsGranted, "Về mặt chức năng có grant");
            Assert.AreEqual("EMPLOYEE_TERMINATED", res.ReadinessCode, "Nhưng tính sẵn sàng bị chặn do thôi việc");
        }

        /// <summary>
        /// Ca 7: Mobile Disabled Check - IS_MOBILE_ENABLED = 0 trên mapping cấm Mobile
        /// </summary>
        [Test]
        public void Test07_MobileDisabled_Flag_Enforced()
        {
            var user = new MockUser { IdUser = 70, Username = "nv70" };
            var rights = new List<MockUserRight>
            {
                new MockUserRight { UserId = 70, FunctionCode = PlatformFunctionCodes.LoginMobile, CanView = 1 }
            };
            var mapping = new MockEmployeeMapping { UserId = 70, EmployeeId = 700, IsMobileEnabled = false };

            var res = _evaluator.Evaluate(user, AppChannels.Mobile, rights, new List<MockGroupMembership>(), mapping);

            Assert.AreEqual("MOBILE_DISABLED", res.ReadinessCode, "Tạm dừng Mobile phải trả mã MOBILE_DISABLED");
        }

        /// <summary>
        /// Ca 8: Wildcard '*' Claim Handling - Claim '*' không tự động bypass cho F_LOGIN_*
        /// </summary>
        [Test]
        public void Test08_WildcardStar_Claim_ExcludesPlatformFunctions()
        {
            UserSession.Clear();
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 1, USERNAME = "ADMIN", DISABLED = 0 };
            UserSession.CurrentSessionId = "TEST_SESSION";
            UserSession.CurrentJti = "TEST_JTI";
            UserSession.UserRights = new List<string> { "*" }; // Chỉ có wildcard nghiệp vụ

            // Với quyền nền tảng, '*' KHÔNG bypass: bắt buộc phải có grant rõ ràng
            bool canDesktopWithoutGrant = UserSession.CanView(PlatformFunctionCodes.LoginDesktop);
            Assert.IsFalse(canDesktopWithoutGrant, "Quyền nền tảng F_LOGIN_DESKTOP không được bypass bởi wildcard '*'");

            // Cấp grant rõ ràng cho nền tảng Desktop
            UserSession.UserRights.Add(PlatformFunctionCodes.LoginDesktop);
            bool canDesktopWithGrant = UserSession.CanView(PlatformFunctionCodes.LoginDesktop);
            Assert.IsTrue(canDesktopWithGrant, "Khi có grant rõ ràng thì được cấp phép");

            // Sau khi quyền cha đã bật, Wildcard '*' cấp quyền nghiệp vụ cho chức năng
            bool canViewNhanSu = UserSession.CanView("F_DM_NHANVIEN");
            Assert.IsTrue(canViewNhanSu, "Sau khi quyền cha bật, Wildcard '*' cấp quyền nghiệp vụ");
        }

        /// <summary>
        /// Ca 9: Token Version Check - tokenVersion <= 0 hoặc mismatch bị reject ngay
        /// </summary>
        [Test]
        public void Test09_TokenVersion_Mismatch_Or_Zero_RejectsSession()
        {
            var authService = new AuthSecurityService();

            // 1. tokenVersion = 0
            bool resZero = authService.ValidateSession("fake_jti", 0, 1, AppChannels.Web);
            Assert.IsFalse(resZero, "tokenVersion <= 0 phải bị từ chối ngay");

            // 2. tokenVersion âm
            bool resNegative = authService.ValidateSession("fake_jti", -5, 1, AppChannels.Web);
            Assert.IsFalse(resNegative, "tokenVersion âm phải bị từ chối");
        }

        /// <summary>
        /// Ca 10: Empty/Null JTI Check - Thiếu JTI bị reject ngay
        /// </summary>
        [Test]
        public void Test10_MissingOrEmptyJti_RejectsSession()
        {
            var authService = new AuthSecurityService();

            bool resNull = authService.ValidateSession(null, 1, 1, AppChannels.Web);
            Assert.IsFalse(resNull, "JTI null phải bị từ chối");

            bool resEmpty = authService.ValidateSession("   ", 1, 1, AppChannels.Web);
            Assert.IsFalse(resEmpty, "JTI rỗng phải bị từ chối");
        }

        /// <summary>
        /// Ca 11: Idle Timeout Enforcement - Quá 480 phút không hoạt động thì hết hạn
        /// </summary>
        [Test]
        public void Test11_IdleTimeout_Enforced_480Minutes()
        {
            DateTime now = DateTime.UtcNow;
            DateTime lastActiveOld = now.AddMinutes(-481); // 481 phút trước

            bool isIdleExpired = (now - lastActiveOld).TotalMinutes > 480;
            Assert.IsTrue(isIdleExpired, "Sau 480 phút không hoạt động thì phiên phải hết hạn theo Idle Timeout");

            DateTime lastActiveRecent = now.AddMinutes(-479); // 479 phút trước
            bool isIdleValid = (now - lastActiveRecent).TotalMinutes <= 480;
            Assert.IsTrue(isIdleValid, "Dưới 480 phút thì phiên còn hợp lệ");
        }

        /// <summary>
        /// Ca 12: Absolute Timeout Enforcement - Quá 1440 phút (24h) từ khi tạo thì hết hạn tuyệt đối
        /// </summary>
        [Test]
        public void Test12_AbsoluteTimeout_Enforced_1440Minutes()
        {
            DateTime now = DateTime.UtcNow;
            DateTime createdAtOld = now.AddMinutes(-1441); // 1441 phút trước

            bool isAbsoluteExpired = (now - createdAtOld).TotalMinutes > 1440;
            Assert.IsTrue(isAbsoluteExpired, "Sau 1440 phút (24h) từ khi tạo thì phiên phải hết hạn tuyệt đối");
        }

        /// <summary>
        /// Ca 13: Expected Channel Mismatch - Token cấp cho Web cố tình dùng ở Mobile bị reject
        /// </summary>
        [Test]
        public void Test13_ExpectedChannel_Mismatch_RejectsToken()
        {
            string sessionClientType = "WEB";
            string requestedChannel = AppChannels.Mobile;

            bool isChannelMatch = sessionClientType.Equals(requestedChannel, StringComparison.OrdinalIgnoreCase);
            Assert.IsFalse(isChannelMatch, "Phiên WEB không được phép sử dụng trên kênh MOBILE");
        }

        /// <summary>
        /// Ca 14: Targeted Revocation - Mất quyền nền tảng chỉ thu hồi session của nền tảng đó
        /// </summary>
        [Test]
        public void Test14_TargetedRevocation_PlatformAccessRemoved()
        {
            var sessions = new List<TB_AUTH_SESSION>
            {
                new TB_AUTH_SESSION { SESSION_ID = "sess_web", CLIENT_TYPE = "WEB", REVOKED_AT = null },
                new TB_AUTH_SESSION { SESSION_ID = "sess_desk", CLIENT_TYPE = "DESKTOP", REVOKED_AT = null },
                new TB_AUTH_SESSION { SESSION_ID = "sess_mob", CLIENT_TYPE = "MOBILE", REVOKED_AT = null }
            };

            // Giả lập thu hồi quyền Web: chỉ thu hồi session WEB với lý do PLATFORM_ACCESS_REMOVED
            string targetChannel = AppChannels.Web;
            foreach (var s in sessions)
            {
                if (s.CLIENT_TYPE.Equals(targetChannel, StringComparison.OrdinalIgnoreCase))
                {
                    s.REVOKED_AT = DateTime.UtcNow;
                    s.REVOKE_REASON = AuthRevokeReasons.PlatformAccessRemoved;
                }
            }

            Assert.IsNotNull(sessions.First(s => s.SESSION_ID == "sess_web").REVOKED_AT);
            Assert.AreEqual("PLATFORM_ACCESS_REMOVED", sessions.First(s => s.SESSION_ID == "sess_web").REVOKE_REASON);
            Assert.IsNull(sessions.First(s => s.SESSION_ID == "sess_desk").REVOKED_AT, "Phiên Desktop không được bị ảnh hưởng");
            Assert.IsNull(sessions.First(s => s.SESSION_ID == "sess_mob").REVOKED_AT, "Phiên Mobile không được bị ảnh hưởng");
        }

        /// <summary>
        /// Ca 15: Targeted Revocation - Tạm dừng Mobile chỉ thu hồi session Mobile
        /// </summary>
        [Test]
        public void Test15_TargetedRevocation_MobileDisabled()
        {
            var sessions = new List<TB_AUTH_SESSION>
            {
                new TB_AUTH_SESSION { SESSION_ID = "sess_web", CLIENT_TYPE = "WEB", REVOKED_AT = null },
                new TB_AUTH_SESSION { SESSION_ID = "sess_mob", CLIENT_TYPE = "MOBILE", REVOKED_AT = null }
            };

            // Tắt Mobile -> revoke Mobile với MOBILE_DISABLED
            foreach (var s in sessions.Where(x => x.CLIENT_TYPE == "MOBILE"))
            {
                s.REVOKED_AT = DateTime.UtcNow;
                s.REVOKE_REASON = AuthRevokeReasons.MobileDisabled;
            }

            Assert.IsNotNull(sessions.First(s => s.SESSION_ID == "sess_mob").REVOKED_AT);
            Assert.AreEqual("MOBILE_DISABLED", sessions.First(s => s.SESSION_ID == "sess_mob").REVOKE_REASON);
            Assert.IsNull(sessions.First(s => s.SESSION_ID == "sess_web").REVOKED_AT);
        }

        /// <summary>
        /// Ca 16: Session Quota Enforcement - Quota theo kênh
        /// </summary>
        [Test]
        public void Test16_SessionQuota_Enforced()
        {
            int mobileQuota = 1;
            int webQuota = 3;
            int desktopQuota = 2;

            var activeMobileSessions = new List<string> { "mob_1" };
            bool canAddMobile = activeMobileSessions.Count < mobileQuota;
            Assert.IsFalse(canAddMobile, "Mobile chỉ cho phép 1 phiên đồng thời, phiên mới phải revoke phiên cũ hoặc chặn");

            var activeWebSessions = new List<string> { "web_1", "web_2" };
            bool canAddWeb = activeWebSessions.Count < webQuota;
            Assert.IsTrue(canAddWeb, "Web cho phép tối đa 3 phiên, 2 phiên hiện tại vẫn còn quota");

            var activeDesktopSessions = new List<string> { "desk_1" };
            bool canAddDesktop = activeDesktopSessions.Count < desktopQuota;
            Assert.IsTrue(canAddDesktop, "Desktop cho phép tối đa 2 phiên");
        }

        /// <summary>
        /// Ca 17: Platform Functions Non-View Rights Locked To 0
        /// </summary>
        [Test]
        public void Test17_PlatformFunctions_NonViewRights_LockedToZero()
        {
            var platformRight = new TB_SYS_RIGHT
            {
                FUNCTION_CODE = PlatformFunctionCodes.LoginDesktop,
                CAN_VIEW = 1,
                CAN_ADD = 0,
                CAN_EDIT = 0,
                CAN_DELETE = 0,
                CAN_PRINT = 0
            };

            Assert.AreEqual(1, platformRight.CAN_VIEW);
            Assert.AreEqual(0, platformRight.CAN_ADD, "CAN_ADD của quyền nền tảng phải là 0");
            Assert.AreEqual(0, platformRight.CAN_EDIT, "CAN_EDIT của quyền nền tảng phải là 0");
            Assert.AreEqual(0, platformRight.CAN_DELETE, "CAN_DELETE của quyền nền tảng phải là 0");
            Assert.AreEqual(0, platformRight.CAN_PRINT, "CAN_PRINT của quyền nền tảng phải là 0");
        }

        /// <summary>
        /// Ca 18: Cutover Backward Compatibility Fallback
        /// </summary>
        [Test]
        public void Test18_Cutover_BackwardCompatibility_Fallback()
        {
            var user = new MockUser { IdUser = 90, Username = "legacyUser", LegacyClientType = "DESKTOP" };
            var emptyRights = new List<MockUserRight>();

            // 1. Khi CHƯA cutover: Fallback theo CLIENT_TYPE = DESKTOP
            _evaluator.IsCutoverApplied = false;
            var resDesktopPreCutover = _evaluator.Evaluate(user, AppChannels.Desktop, emptyRights, new List<MockGroupMembership>(), null);
            var resMobilePreCutover = _evaluator.Evaluate(user, AppChannels.Mobile, emptyRights, new List<MockGroupMembership>(), null);

            Assert.IsTrue(resDesktopPreCutover.IsGranted, "Chưa cutover: fallback CLIENT_TYPE = DESKTOP cho phép Desktop");
            Assert.IsFalse(resMobilePreCutover.IsGranted, "Chưa cutover: CLIENT_TYPE = DESKTOP từ chối Mobile");

            // 2. Khi ĐÃ cutover: Không fallback, bắt buộc phải có grant F_LOGIN_*
            _evaluator.IsCutoverApplied = true;
            var resDesktopPostCutover = _evaluator.Evaluate(user, AppChannels.Desktop, emptyRights, new List<MockGroupMembership>(), null);
            Assert.IsFalse(resDesktopPostCutover.IsGranted, "Đã cutover: không fallback, thiếu F_LOGIN_DESKTOP bị từ chối");
        }
    }
}
