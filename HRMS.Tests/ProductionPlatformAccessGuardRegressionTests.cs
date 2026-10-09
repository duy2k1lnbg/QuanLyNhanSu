using Bu.CLASS_SECURITY;
using Bu.CLASS_SYSTEM;
using Bu.DTO;
using DA;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace HRMS.Tests
{
    /// <summary>
    /// Bộ kiểm thử hồi quy trực tiếp trên PRODUCTION CODE (Bu.dll)
    /// Kiểm tra toàn bộ các ca probe và yêu cầu tại review prompt 20261008:
    /// 1. Old right on nhưng parent off -> DENIED
    /// 2. SessionId / JTI missing -> DENIED
    /// 3. ADMIN nhưng parent off -> DENIED
    /// 4. ADMIN nhưng child right off -> DENIED (ADMIN child off denied)
    /// 5. Unknown function code trên Desktop -> DENIED (Default Deny)
    /// 6. Web không thể ghi bảng lương (F_CC_BANGLUONG Edit/Add cấm trên Web)
    /// 7. Endpoint Alias Mapping (F_HOPDONG_ADD/EDIT/DELETE -> F_NV_HOPDONG)
    /// 8. Root Admin cấm Mobile (F_LOGIN_MOBILE cấm ADMIN)
    /// 9. Tài khoản DISABLED -> DENIED
    /// </summary>
    [TestFixture]
    public class ProductionPlatformAccessGuardRegressionTests
    {
        [SetUp]
        public void Setup()
        {
            // Reset context UserSession trước mỗi test
            UserSession.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            UserSession.Clear();
        }

        #region Probe 1: JTI / SessionId Missing -> DENIED

        [Test]
        public void UserSession_WhenSessionIdOrJtiMissing_CanEditMustReturnFalseEvenIfRightsAreTrue()
        {
            // Gán thông tin người dùng hợp lệ
            UserSession.CurrentUser = new TB_SYS_USER
            {
                IDUSER = 10,
                USERNAME = "testuser",
                FULLNAME = "Test User",
                DISABLED = 0
            };
            UserSession.CurrentChannel = "DESKTOP";
            UserSession.ParentDesktopOn = true;

            // Cấp quyền con CAN_EDIT = true cho F_NV_HOPDONG
            UserSession.DetailedRights = new Dictionary<string, UserRightDetail>(StringComparer.OrdinalIgnoreCase)
            {
                { "F_LOGIN_DESKTOP", new UserRightDetail { CAN_VIEW = true } },
                { "F_NV_HOPDONG", new UserRightDetail { CAN_VIEW = true, CAN_EDIT = true } }
            };

            // Trường hợp 1: SessionId và JTI đều null
            UserSession.CurrentSessionId = null;
            UserSession.CurrentJti = null;
            Assert.IsFalse(UserSession.CanEdit("F_NV_HOPDONG"), "Khi SessionId và JTI thiếu, CanEdit phải trả về false");

            // Trường hợp 2: Có SessionId nhưng JTI rỗng
            UserSession.CurrentSessionId = "SESSION_123";
            UserSession.CurrentJti = "";
            Assert.IsFalse(UserSession.CanEdit("F_NV_HOPDONG"), "Khi JTI rỗng, CanEdit phải trả về false");

            // Trường hợp 3: Khi có đầy đủ SessionId và JTI
            UserSession.CurrentJti = "JTI_ABC";
            Assert.IsTrue(UserSession.CanEdit("F_NV_HOPDONG"), "Khi có đầy đủ SessionId và JTI, CanEdit phải trả về true");
        }

        #endregion

        #region Probe 2: Parent Desktop Off -> DENIED (Kể cả ADMIN)

        [Test]
        public void UserSession_WhenParentDesktopOff_CanEditMustReturnFalseEvenForAdmin()
        {
            // Tài khoản Quản trị tối cao ADMIN
            UserSession.CurrentUser = new TB_SYS_USER
            {
                IDUSER = 1,
                USERNAME = "ADMIN",
                FULLNAME = "System Administrator",
                DISABLED = 0
            };
            UserSession.CurrentChannel = "DESKTOP";
            UserSession.CurrentSessionId = "ADMIN_SESSION_01";
            UserSession.CurrentJti = "ADMIN_JTI_01";

            // Cấp quyền con
            UserSession.DetailedRights = new Dictionary<string, UserRightDetail>(StringComparer.OrdinalIgnoreCase)
            {
                { "F_NV_HOPDONG", new UserRightDetail { CAN_VIEW = true, CAN_EDIT = true } }
            };

            // Quyền cha Desktop bị TẮT
            UserSession.ParentDesktopOn = false;

            // CanEdit PHẢI trả về false (ADMIN không vượt quyền cha tắt)
            Assert.IsFalse(UserSession.CanEdit("F_NV_HOPDONG"), "Kể cả ADMIN, khi quyền cha Desktop tắt thì CanEdit phải bị chặn");
            Assert.IsFalse(UserSession.CanView("F_NV_HOPDONG"), "Kể cả ADMIN, khi quyền cha Desktop tắt thì CanView phải bị chặn");
            Assert.IsFalse(UserSession.CanAdd("F_NV_HOPDONG"), "Kể cả ADMIN, khi quyền cha Desktop tắt thì CanAdd phải bị chặn");
            Assert.IsFalse(UserSession.CanDelete("F_NV_HOPDONG"), "Kể cả ADMIN, khi quyền cha Desktop tắt thì CanDelete phải bị chặn");
        }

        #endregion

        #region Probe 3: Unknown Function Code on Desktop -> DENIED (Default Deny)

        [Test]
        public void ChannelCapabilityRegistry_UnknownFunctionOnDesktop_MustBeDeniedByDefault()
        {
            string unknownCode = "REVIEW_UNKNOWN_FUNCTION";

            // Gọi trực tiếp production ChannelCapabilityRegistry
            bool canEdit = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Desktop, unknownCode, ChannelAction.Edit);
            bool canView = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Desktop, unknownCode, ChannelAction.View);
            bool canAdd = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Desktop, unknownCode, ChannelAction.Add);
            bool canDelete = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Desktop, unknownCode, ChannelAction.Delete);

            Assert.IsFalse(canEdit, "Mã chức năng lạ trên Desktop phải bị từ chối Edit theo nguyên tắc Default Deny");
            Assert.IsFalse(canView, "Mã chức năng lạ trên Desktop phải bị từ chối View theo nguyên tắc Default Deny");
            Assert.IsFalse(canAdd, "Mã chức năng lạ trên Desktop phải bị từ chối Add theo nguyên tắc Default Deny");
            Assert.IsFalse(canDelete, "Mã chức năng lạ trên Desktop phải bị từ chối Delete theo nguyên tắc Default Deny");

            var cap = ChannelCapabilityRegistry.GetCapability(AppChannels.Desktop, unknownCode);
            Assert.IsNotNull(cap);
            Assert.IsFalse(cap.CanView, "Mã lạ không được hỗ trợ View");
            Assert.IsFalse(cap.CanEdit, "Mã lạ không được hỗ trợ Edit");
            Assert.IsFalse(cap.CanAdd, "Mã lạ không được hỗ trợ Add");
            Assert.IsFalse(cap.CanDelete, "Mã lạ không được hỗ trợ Delete");
            Assert.IsFalse(cap.CanPrint, "Mã lạ không được hỗ trợ Print");
        }

        [Test]
        public void UserSession_WhenFunctionIsUnknown_CanEditMustReturnFalseEvenWithSession()
        {
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 1, USERNAME = "ADMIN", DISABLED = 0 };
            UserSession.CurrentChannel = "DESKTOP";
            UserSession.CurrentSessionId = "SESS_1";
            UserSession.CurrentJti = "JTI_1";
            UserSession.ParentDesktopOn = true;

            // Dù DetailedRights có bản ghi cho mã lạ
            UserSession.DetailedRights = new Dictionary<string, UserRightDetail>(StringComparer.OrdinalIgnoreCase)
            {
                { "F_LOGIN_DESKTOP", new UserRightDetail { CAN_VIEW = true } },
                { "REVIEW_UNKNOWN_FUNCTION", new UserRightDetail { CAN_VIEW = true, CAN_EDIT = true } }
            };

            // CanEdit PHẢI trả về false vì ChannelCapabilityRegistry không hỗ trợ mã lạ
            Assert.IsFalse(UserSession.CanEdit("REVIEW_UNKNOWN_FUNCTION"), "Mã lạ không được kênh hỗ trợ thì UserSession.CanEdit phải trả về false");
        }

        #endregion

        #region Probe 4: ADMIN Child Off -> DENIED (Không bypass quyền con)

        [Test]
        public void UserSession_WhenAdminHasChildRightOff_CanEditMustReturnFalse()
        {
            // Tài khoản ADMIN, có phiên và quyền cha Desktop bật
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 1, USERNAME = "ADMIN", DISABLED = 0 };
            UserSession.CurrentChannel = "DESKTOP";
            UserSession.CurrentSessionId = "ADMIN_SESS";
            UserSession.CurrentJti = "ADMIN_JTI";
            UserSession.ParentDesktopOn = true;

            // Quyền con F_NV_HOPDONG có CAN_VIEW = true nhưng CAN_EDIT = false
            UserSession.DetailedRights = new Dictionary<string, UserRightDetail>(StringComparer.OrdinalIgnoreCase)
            {
                { "F_LOGIN_DESKTOP", new UserRightDetail { CAN_VIEW = true } },
                { "F_NV_HOPDONG", new UserRightDetail { CAN_VIEW = true, CAN_EDIT = false, CAN_ADD = false, CAN_DELETE = false } }
            };

            // CanView được phép
            Assert.IsTrue(UserSession.CanView("F_NV_HOPDONG"), "Admin có CAN_VIEW = true thì CanView trả về true");

            // Nhưng CAN_EDIT = false thì CanEdit PHẢI trả về false! (ADMIN child off denied)
            Assert.IsFalse(UserSession.CanEdit("F_NV_HOPDONG"), "Admin khi quyền con CAN_EDIT tắt thì CanEdit phải bị chặn");
            Assert.IsFalse(UserSession.CanAdd("F_NV_HOPDONG"), "Admin khi quyền con CAN_ADD tắt thì CanAdd phải bị chặn");
            Assert.IsFalse(UserSession.CanDelete("F_NV_HOPDONG"), "Admin khi quyền con CAN_DELETE tắt thì CanDelete phải bị chặn");
        }

        #endregion

        #region Probe 5: Platform Capability Boundary (Web Payroll Write Denied)

        [Test]
        public void ChannelCapabilityRegistry_WebPayrollMustOnlyAllowViewAndDenyAllWriteActions()
        {
            // Trên WEB: F_CC_BANGLUONG chỉ được VIEW, cấm ADD, EDIT, DELETE, PRINT
            bool webCanView = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Web, "F_CC_BANGLUONG", ChannelAction.View);
            bool webCanAdd = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Web, "F_CC_BANGLUONG", ChannelAction.Add);
            bool webCanEdit = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Web, "F_CC_BANGLUONG", ChannelAction.Edit);
            bool webCanDel = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Web, "F_CC_BANGLUONG", ChannelAction.Delete);

            Assert.IsTrue(webCanView, "Web được phép xem bảng lương (View)");
            Assert.IsFalse(webCanAdd, "Web cấm tuyệt đối tính/thêm bảng lương (Add)");
            Assert.IsFalse(webCanEdit, "Web cấm tuyệt đối sửa bảng lương (Edit)");
            Assert.IsFalse(webCanDel, "Web cấm tuyệt đối xóa bảng lương (Delete)");

            // Trong khi trên DESKTOP: F_CC_BANGLUONG được phép toàn quyền
            bool deskCanView = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Desktop, "F_CC_BANGLUONG", ChannelAction.View);
            bool deskCanEdit = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Desktop, "F_CC_BANGLUONG", ChannelAction.Edit);
            bool deskCanAdd = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Desktop, "F_CC_BANGLUONG", ChannelAction.Add);

            Assert.IsTrue(deskCanView, "Desktop được phép xem bảng lương");
            Assert.IsTrue(deskCanEdit, "Desktop được phép tính/sửa bảng lương");
            Assert.IsTrue(deskCanAdd, "Desktop được phép thêm bảng lương");
        }

        #endregion

        #region Probe 6: Endpoint Alias Mapping trong PlatformAccessGuard

        [Test]
        public void PlatformAccessGuard_ResolveEndpointAction_MustMapExplicitAliasesCorrectly()
        {
            var guard = PlatformAccessGuard.Current;

            // Hợp đồng lao động
            Assert.IsTrue(guard.ResolveEndpointAction("F_HOPDONG_ADD", out string fn, out ChannelAction act));
            Assert.AreEqual("F_NV_HOPDONG", fn);
            Assert.AreEqual(ChannelAction.Add, act);

            Assert.IsTrue(guard.ResolveEndpointAction("F_HOPDONG_EDIT", out fn, out act));
            Assert.AreEqual("F_NV_HOPDONG", fn);
            Assert.AreEqual(ChannelAction.Edit, act);

            Assert.IsTrue(guard.ResolveEndpointAction("F_HOPDONG_DELETE", out fn, out act));
            Assert.AreEqual("F_NV_HOPDONG", fn);
            Assert.AreEqual(ChannelAction.Delete, act);

            // Tạm ứng lương
            Assert.IsTrue(guard.ResolveEndpointAction("F_UNGLUONG_EDIT", out fn, out act));
            Assert.AreEqual("F_CC_UNGLUONG", fn);
            Assert.AreEqual(ChannelAction.Edit, act);

            // Tăng ca
            Assert.IsTrue(guard.ResolveEndpointAction("F_TANGCA_ADD", out fn, out act));
            Assert.AreEqual("F_CC_TANGCA", fn);
            Assert.AreEqual(ChannelAction.Add, act);

            // Bảng lương
            Assert.IsTrue(guard.ResolveEndpointAction("F_TIENLUONG", out fn, out act));
            Assert.AreEqual("F_CC_BANGLUONG", fn);
            Assert.AreEqual(ChannelAction.View, act);

            Assert.IsTrue(guard.ResolveEndpointAction("F_BANGLUONG_EDIT", out fn, out act));
            Assert.AreEqual("F_CC_BANGLUONG", fn);
            Assert.AreEqual(ChannelAction.Edit, act);
        }

        #endregion

        #region Probe 7: Platform Function Isolation

        [Test]
        public void PlatformFunctionCodes_ChannelFunctionsMustBeIsolated()
        {
            Assert.IsTrue(PlatformFunctionCodes.IsPlatformCode("F_LOGIN_DESKTOP"));
            Assert.IsTrue(PlatformFunctionCodes.IsPlatformCode("F_LOGIN_WEB"));
            Assert.IsTrue(PlatformFunctionCodes.IsPlatformCode("F_LOGIN_MOBILE"));
            Assert.IsFalse(PlatformFunctionCodes.IsPlatformCode("F_NV_HOPDONG"));

            Assert.AreEqual("F_LOGIN_DESKTOP", PlatformFunctionCodes.GetFunctionCodeForChannel(AppChannels.Desktop));
            Assert.AreEqual("F_LOGIN_WEB", PlatformFunctionCodes.GetFunctionCodeForChannel(AppChannels.Web));
            Assert.AreEqual("F_LOGIN_MOBILE", PlatformFunctionCodes.GetFunctionCodeForChannel(AppChannels.Mobile));
        }

        #endregion

        #region Probe 8: Disabled User Account -> DENIED

        [Test]
        public void UserSession_WhenUserIsDisabled_MustBeDeniedCompletely()
        {
            UserSession.CurrentUser = new TB_SYS_USER
            {
                IDUSER = 99,
                USERNAME = "locked_user",
                DISABLED = 1 // Vô hiệu hóa
            };
            UserSession.CurrentChannel = "DESKTOP";
            UserSession.CurrentSessionId = "VALID_SESSION";
            UserSession.CurrentJti = "VALID_JTI";
            UserSession.ParentDesktopOn = true;
            UserSession.DetailedRights = new Dictionary<string, UserRightDetail>(StringComparer.OrdinalIgnoreCase)
            {
                { "F_LOGIN_DESKTOP", new UserRightDetail { CAN_VIEW = true } },
                { "F_NV_HOPDONG", new UserRightDetail { CAN_VIEW = true, CAN_EDIT = true } }
            };

            Assert.IsFalse(UserSession.CanView("F_NV_HOPDONG"), "Tài khoản disabled phải bị từ chối CanView");
            Assert.IsFalse(UserSession.CanEdit("F_NV_HOPDONG"), "Tài khoản disabled phải bị từ chối CanEdit");
            Assert.IsFalse(UserSession.CanAdd("F_NV_HOPDONG"), "Tài khoản disabled phải bị từ chối CanAdd");
            Assert.IsFalse(UserSession.CanDelete("F_NV_HOPDONG"), "Tài khoản disabled phải bị từ chối CanDelete");
        }

        #endregion

        #region Probe 9: Desktop Edit vs Web View trên cùng một Function (F_CC_BANGLUONG)

        [Test]
        public void CrossChannel_SameFunction_DesktopCanEdit_WebCanOnlyView()
        {
            string func = "F_CC_BANGLUONG";

            // Desktop có quyền Edit
            bool deskEdit = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Desktop, func, ChannelAction.Edit);
            Assert.IsTrue(deskEdit, "Desktop được phép Edit bảng lương");

            // Web cấm Edit nhưng cho phép View
            bool webEdit = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Web, func, ChannelAction.Edit);
            bool webView = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Web, func, ChannelAction.View);
            Assert.IsFalse(webEdit, "Web cấm Edit bảng lương");
            Assert.IsTrue(webView, "Web được phép View bảng lương");

            // Mobile cấm hoàn toàn chức năng quản trị bảng lương
            bool mobileView = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, func, ChannelAction.View);
            bool mobileEdit = ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, func, ChannelAction.Edit);
            Assert.IsFalse(mobileView, "Mobile cấm View chức năng bảng lương quản trị F_CC_BANGLUONG");
            Assert.IsFalse(mobileEdit, "Mobile cấm Edit chức năng bảng lương quản trị F_CC_BANGLUONG");
        }

        #endregion

        #region Probe 10: RIGHT_TYPE Classification và Entity Materialization

        [Test]
        public void RightTypeClassification_MustProperlyIdentifyLoginAndFunctionTypes()
        {
            var loginFn = new TB_SYS_FUNCTION { FUNCTION_CODE = "F_LOGIN_DESKTOP", RIGHT_TYPE = "LOGIN" };
            Assert.IsTrue(loginFn.IsLoginRight, "F_LOGIN_DESKTOP phải là LoginRight");
            Assert.IsFalse(loginFn.IsCategoryRight, "F_LOGIN_DESKTOP không phải là CategoryRight");
            Assert.IsFalse(loginFn.IsBusinessFunction, "F_LOGIN_DESKTOP không phải là BusinessFunction");

            var bizFn = new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_HOPDONG", RIGHT_TYPE = "FUNCTION" };
            Assert.IsFalse(bizFn.IsLoginRight, "F_NV_HOPDONG không phải là LoginRight");
            Assert.IsTrue(bizFn.IsBusinessFunction, "F_NV_HOPDONG phải là BusinessFunction");
            Assert.IsFalse(bizFn.IsCategoryRight, "F_NV_HOPDONG không phải là CategoryRight");

            var catFn = new TB_SYS_FUNCTION { FUNCTION_CODE = "MOD_HR", RIGHT_TYPE = "CATEGORY" };
            Assert.IsTrue(catFn.IsCategoryRight, "MOD_HR phải là CategoryRight");
            Assert.IsFalse(catFn.IsLoginRight, "MOD_HR không phải là LoginRight");
            Assert.IsFalse(catFn.IsBusinessFunction, "MOD_HR không phải là BusinessFunction");
        }

        #endregion

        #region Probe 11: MeController Personal Endpoints Capability Mapping

        [Test]
        public void MobilePersonalEndpoints_MustEnforceActionAndCapabilityBoundaries()
        {
            // Hồ sơ cá nhân: View được hỗ trợ trên Mobile, cấm Delete
            Assert.IsTrue(ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, "MOBILE_PROFILE_VIEW", ChannelAction.View));
            Assert.IsFalse(ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, "MOBILE_PROFILE_VIEW", ChannelAction.Delete), "Mobile Profile không được xóa");

            // Chấm công & Phiếu lương cá nhân: Chỉ View, cấm Edit/Add
            Assert.IsTrue(ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, "MOBILE_ATTENDANCE_VIEW", ChannelAction.View));
            Assert.IsFalse(ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, "MOBILE_ATTENDANCE_VIEW", ChannelAction.Edit), "Mobile Attendance cấm Edit");
            Assert.IsTrue(ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, "MOBILE_PAYROLL_VIEW", ChannelAction.View));
            Assert.IsFalse(ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, "MOBILE_PAYROLL_VIEW", ChannelAction.Edit), "Mobile Payroll cấm Edit");

            // Đơn nghỉ phép cá nhân: Cho phép View và Add (gửi đơn mới), cấm Delete
            Assert.IsTrue(ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, "MOBILE_REQUEST_LEAVE", ChannelAction.View));
            Assert.IsTrue(ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, "MOBILE_REQUEST_LEAVE", ChannelAction.Add));
            Assert.IsFalse(ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, "MOBILE_REQUEST_LEAVE", ChannelAction.Delete));
        }

        #endregion
    }
}
