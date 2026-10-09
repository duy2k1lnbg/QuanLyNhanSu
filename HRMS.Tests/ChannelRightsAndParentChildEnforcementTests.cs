using Bu.CLASS_SECURITY;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRMS.Tests
{
    /// <summary>
    /// Bộ kiểm thử Offline & In-Memory cho Logic Quyền Cha - Con (Mục 13) và Phân biệt Kênh/Loại quyền (Mục 14).
    /// TUYỆT ĐỐI KHÔNG GHI DỮ LIỆU VÀO DATABASE THẬT theo yêu cầu an toàn hệ thống.
    /// </summary>
    [TestFixture]
    public class ChannelRightsAndParentChildEnforcementTests
    {
        #region In-Memory Decision Fixtures

        public class MockChannelRight
        {
            public decimal UserId { get; set; }
            public string Channel { get; set; } // DESKTOP, WEB, MOBILE
            public string FunctionCode { get; set; }
            public bool CanView { get; set; }
            public bool CanAdd { get; set; }
            public bool CanEdit { get; set; }
            public bool CanDelete { get; set; }
            public bool CanPrint { get; set; }
        }

        public class MockParentPlatformRight
        {
            public decimal UserId { get; set; }
            public string Channel { get; set; }
            public bool IsGranted { get; set; }
        }

        public class InMemoryChannelPermissionEvaluator
        {
            private readonly List<MockParentPlatformRight> _parentGrants = new List<MockParentPlatformRight>();
            private readonly List<MockChannelRight> _channelRights = new List<MockChannelRight>();
            private readonly List<Tuple<decimal, decimal>> _groupMemberships = new List<Tuple<decimal, decimal>>(); // GroupId, UserId

            public void SetParentGrant(decimal userId, string channel, bool isGranted)
            {
                var existing = _parentGrants.FirstOrDefault(p => p.UserId == userId && p.Channel == channel);
                if (existing != null) existing.IsGranted = isGranted;
                else _parentGrants.Add(new MockParentPlatformRight { UserId = userId, Channel = channel, IsGranted = isGranted });
            }

            public void SetChannelRight(decimal userId, string channel, string funcCode, bool v, bool a, bool e, bool d, bool p)
            {
                var existing = _channelRights.FirstOrDefault(r => r.UserId == userId && r.Channel == channel && r.FunctionCode == funcCode);
                if (existing != null)
                {
                    existing.CanView = v; existing.CanAdd = a; existing.CanEdit = e; existing.CanDelete = d; existing.CanPrint = p;
                }
                else
                {
                    _channelRights.Add(new MockChannelRight
                    {
                        UserId = userId, Channel = channel, FunctionCode = funcCode,
                        CanView = v, CanAdd = a, CanEdit = e, CanDelete = d, CanPrint = p
                    });
                }
            }

            public void AddGroupMember(decimal groupId, decimal memberUserId)
            {
                _groupMemberships.Add(Tuple.Create(groupId, memberUserId));
            }

            /// <summary>
            /// Đánh giá quyền hiệu lực theo công thức Mục 13:
            /// Effective = ParentGranted
            ///             AND ChannelSupports(channel, func, action)
            ///             AND (DirectGrant OR GroupGrant)
            /// </summary>
            public bool EvaluateAction(decimal userId, string channel, string funcCode, ChannelAction action, bool isRootAdmin = false)
            {
                string normChannel = AppChannels.Normalize(channel);
                if (normChannel == null) return false;

                // 1. Kiểm tra Admin đối với kênh Mobile: cấm tuyệt đối
                if (isRootAdmin && normChannel == AppChannels.Mobile) return false;

                // 2. Quyền cha
                var parent = _parentGrants.FirstOrDefault(p => p.UserId == userId && p.Channel == normChannel);
                bool parentGranted = parent != null && parent.IsGranted;
                if (!parentGranted)
                {
                    // Cha tắt -> toan bo quyen con vo hieu hoa!
                    return false;
                }

                // 3. Khả năng hỗ trợ của kênh từ ChannelCapabilityRegistry
                if (!ChannelCapabilityRegistry.IsActionSupported(normChannel, funcCode, action))
                {
                    return false;
                }

                // 4. Nếu là Admin và cha đang bật và kênh hỗ trợ -> Admin có toàn quyền nghiệp vụ
                if (isRootAdmin)
                {
                    return true;
                }

                // 5. Kiểm tra Direct grant
                var direct = _channelRights.FirstOrDefault(r => r.UserId == userId && r.Channel == normChannel && r.FunctionCode == funcCode);
                bool directAction = direct != null && CheckAction(direct, action);

                // 6. Kiểm tra Group grant
                var userGroups = _groupMemberships.Where(m => m.Item2 == userId).Select(m => m.Item1).ToList();
                var groupRights = _channelRights.Where(r => userGroups.Contains(r.UserId) && r.Channel == normChannel && r.FunctionCode == funcCode).ToList();
                bool groupAction = groupRights.Any(r => CheckAction(r, action));

                return directAction || groupAction;
            }

            private static bool CheckAction(MockChannelRight r, ChannelAction action)
            {
                switch (action)
                {
                    case ChannelAction.View: return r.CanView;
                    case ChannelAction.Add: return r.CanAdd;
                    case ChannelAction.Edit: return r.CanEdit;
                    case ChannelAction.Delete: return r.CanDelete;
                    case ChannelAction.Print: return r.CanPrint;
                    default: return false;
                }
            }
        }

        #endregion

        #region Unit Tests Mục 13: Quyền Cha - Con & Khả Năng Kênh

        [Test]
        public void Test01_ParentOff_DisablesAllChildActions_EvenIfDirectlyGranted()
        {
            // Arrange
            var eval = new InMemoryChannelPermissionEvaluator();
            decimal userId = 100;
            eval.SetParentGrant(userId, AppChannels.Desktop, false); // Cha TẮT
            eval.SetChannelRight(userId, AppChannels.Desktop, "F_NV_HOPDONG", true, true, true, true, true); // Con được cấp full trong DB

            // Act & Assert
            // Vì cha tắt, toàn bộ quyền con phải bị từ chối
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_HOPDONG", ChannelAction.View), "Cha tắt phải từ chối View.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_HOPDONG", ChannelAction.Edit), "Cha tắt phải từ chối Edit.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_HOPDONG", ChannelAction.Delete), "Cha tắt phải từ chối Delete.");
        }

        [Test]
        public void Test02_ParentOn_EnablesGrantedChildActions()
        {
            // Arrange
            var eval = new InMemoryChannelPermissionEvaluator();
            decimal userId = 100;
            eval.SetParentGrant(userId, AppChannels.Desktop, true); // Cha BẬT
            eval.SetChannelRight(userId, AppChannels.Desktop, "F_NV_HOPDONG", true, false, true, false, false); // View=1, Edit=1

            // Act & Assert
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_HOPDONG", ChannelAction.View), "Cha bật và được cấp View phải cho phép.");
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_HOPDONG", ChannelAction.Edit), "Cha bật và được cấp Edit phải cho phép.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_HOPDONG", ChannelAction.Add), "Không được cấp Add phải từ chối.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_HOPDONG", ChannelAction.Delete), "Không được cấp Delete phải từ chối.");
        }

        [Test]
        public void Test03_WebChannel_PayrollModule_OnlyAllowsView_RejectsCalculationsAndModifications()
        {
            // Arrange
            var eval = new InMemoryChannelPermissionEvaluator();
            decimal userId = 200;
            eval.SetParentGrant(userId, AppChannels.Web, true); // Web cha BẬT
            // Giả lập DB cố tình cấp full quyền bảng lương trên Web
            eval.SetChannelRight(userId, AppChannels.Web, "F_CC_BANGLUONG", true, true, true, true, true);

            // Act & Assert
            // Theo đặc tả Mục 13: Bảng lương trên Web chỉ cho phép View, khóa mọi action tính/sửa/xóa/in lương!
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Web, "F_CC_BANGLUONG", ChannelAction.View), "Web Bảng lương cho phép Xem.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Web, "F_CC_BANGLUONG", ChannelAction.Add), "Web Bảng lương cấm Thêm/Tính lương.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Web, "F_CC_BANGLUONG", ChannelAction.Edit), "Web Bảng lương cấm Sửa.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Web, "F_CC_BANGLUONG", ChannelAction.Delete), "Web Bảng lương cấm Xóa.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Web, "F_CC_BANGLUONG", ChannelAction.Print), "Web Bảng lương cấm In.");
        }

        [Test]
        public void Test04_DesktopChannel_PayrollModule_AllowsFullActionsWhenGranted()
        {
            // Arrange
            var eval = new InMemoryChannelPermissionEvaluator();
            decimal userId = 200;
            eval.SetParentGrant(userId, AppChannels.Desktop, true);
            eval.SetChannelRight(userId, AppChannels.Desktop, "F_CC_BANGLUONG", true, true, true, true, true);

            // Act & Assert: Desktop WinForms hỗ trợ đầy đủ quy trình tính và quản lý bảng lương
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Desktop, "F_CC_BANGLUONG", ChannelAction.View));
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Desktop, "F_CC_BANGLUONG", ChannelAction.Add));
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Desktop, "F_CC_BANGLUONG", ChannelAction.Edit));
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Desktop, "F_CC_BANGLUONG", ChannelAction.Delete));
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Desktop, "F_CC_BANGLUONG", ChannelAction.Print));
        }

        [Test]
        public void Test05_MobileChannel_OnlyAllowsMobileCapabilities_RejectsDesktopAdminModules()
        {
            // Arrange
            var eval = new InMemoryChannelPermissionEvaluator();
            decimal userId = 300;
            eval.SetParentGrant(userId, AppChannels.Mobile, true);
            // Cấp quyền chức năng cá nhân Mobile và chức năng quản lý Desktop
            eval.SetChannelRight(userId, AppChannels.Mobile, "MOBILE_PROFILE_VIEW", true, false, false, false, false);
            eval.SetChannelRight(userId, AppChannels.Mobile, "MOBILE_REQUEST_LEAVE", true, true, false, false, false);
            eval.SetChannelRight(userId, AppChannels.Mobile, "F_SYSTEM_DB_CONFIG", true, true, true, true, true);

            // Act & Assert
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Mobile, "MOBILE_PROFILE_VIEW", ChannelAction.View), "Mobile xem hồ sơ hợp lệ.");
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Mobile, "MOBILE_REQUEST_LEAVE", ChannelAction.Add), "Mobile gửi đơn nghỉ phép hợp lệ.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Mobile, "F_SYSTEM_DB_CONFIG", ChannelAction.View), "Mobile cấm tuyệt đối chức năng cấu hình DB hệ thống.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Mobile, "F_SYSTEM_DB_CONFIG", ChannelAction.Edit), "Mobile cấm sửa DB.");
        }

        [Test]
        public void Test06_Admin_StrictProhibitionOnMobile_AndCannotBypassDisabledWeb()
        {
            // Arrange
            var eval = new InMemoryChannelPermissionEvaluator();
            decimal adminId = 80;

            // 1. Admin trên Mobile luôn bị từ chối
            eval.SetParentGrant(adminId, AppChannels.Mobile, true); // Giả lập bật grant cha
            Assert.IsFalse(eval.EvaluateAction(adminId, AppChannels.Mobile, "MOBILE_PROFILE_VIEW", ChannelAction.View, isRootAdmin: true),
                "Root Admin cấm tuyệt đối sử dụng kênh Mobile.");

            // 2. Admin trên Web: Nếu cha TẮT -> Admin cũng bị chặn
            eval.SetParentGrant(adminId, AppChannels.Web, false);
            Assert.IsFalse(eval.EvaluateAction(adminId, AppChannels.Web, "F_NV_HOPDONG", ChannelAction.View, isRootAdmin: true),
                "Admin không được bypass khi quyền nền tảng cha bị tắt.");

            // 3. Admin trên Web: Nếu cha BẬT -> Admin không bypass giới hạn kênh (Web bảng lương không tính được)
            eval.SetParentGrant(adminId, AppChannels.Web, true);
            Assert.IsTrue(eval.EvaluateAction(adminId, AppChannels.Web, "F_NV_HOPDONG", ChannelAction.View, isRootAdmin: true));
            Assert.IsFalse(eval.EvaluateAction(adminId, AppChannels.Web, "F_CC_BANGLUONG", ChannelAction.Edit, isRootAdmin: true),
                "Admin không bypass giới hạn khả năng kênh: Web không hỗ trợ tính/sửa lương.");
        }

        [Test]
        public void Test07_ChildPermissions_InheritedFromActiveGroup_WhenDirectGrantIsZero()
        {
            // Arrange
            var eval = new InMemoryChannelPermissionEvaluator();
            decimal userId = 101;
            decimal groupId = 50;

            eval.SetParentGrant(userId, AppChannels.Desktop, true);
            eval.AddGroupMember(groupId, userId);

            // Direct grant = 0, Group grant = 1
            eval.SetChannelRight(userId, AppChannels.Desktop, "F_NV_DIEUCHUYEN", false, false, false, false, false);
            eval.SetChannelRight(groupId, AppChannels.Desktop, "F_NV_DIEUCHUYEN", true, true, false, false, false);

            // Act & Assert: Kế thừa quyền từ nhóm thành công
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_DIEUCHUYEN", ChannelAction.View), "Kế thừa View từ nhóm.");
            Assert.IsTrue(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_DIEUCHUYEN", ChannelAction.Add), "Kế thừa Add từ nhóm.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_DIEUCHUYEN", ChannelAction.Edit), "Nhóm không cấp Edit nên bị từ chối.");
        }

        [Test]
        public void Test08_ParentOff_OverrulesGroupInheritance_ForChildActions()
        {
            // Arrange
            var eval = new InMemoryChannelPermissionEvaluator();
            decimal userId = 101;
            decimal groupId = 50;

            eval.SetParentGrant(userId, AppChannels.Desktop, false); // Cha TẮT
            eval.AddGroupMember(groupId, userId);
            eval.SetChannelRight(groupId, AppChannels.Desktop, "F_NV_DIEUCHUYEN", true, true, true, true, true); // Nhóm cấp full

            // Act & Assert: Cha tắt đè hoàn toàn quyền nhóm
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_DIEUCHUYEN", ChannelAction.View), "Cha tắt phải chặn quyền nhóm.");
            Assert.IsFalse(eval.EvaluateAction(userId, AppChannels.Desktop, "F_NV_DIEUCHUYEN", ChannelAction.Add));
        }

        [Test]
        public void Test09_ChannelCapabilityRegistry_ReturnsExactRegisteredCapabilities()
        {
            // Web Bảng lương
            var webPayroll = ChannelCapabilityRegistry.GetCapability(AppChannels.Web, "F_CC_BANGLUONG");
            Assert.IsNotNull(webPayroll);
            Assert.IsTrue(webPayroll.CanView);
            Assert.IsFalse(webPayroll.CanAdd);
            Assert.IsFalse(webPayroll.CanEdit);
            Assert.IsFalse(webPayroll.CanDelete);
            Assert.IsFalse(webPayroll.CanPrint);

            // Desktop Bảng lương
            var dtPayroll = ChannelCapabilityRegistry.GetCapability(AppChannels.Desktop, "F_CC_BANGLUONG");
            Assert.IsNotNull(dtPayroll);
            Assert.IsTrue(dtPayroll.CanView);
            Assert.IsTrue(dtPayroll.CanAdd);
            Assert.IsTrue(dtPayroll.CanEdit);

            // Mobile Chức năng cá nhân
            var mbLeave = ChannelCapabilityRegistry.GetCapability(AppChannels.Mobile, "MOBILE_REQUEST_LEAVE");
            Assert.IsNotNull(mbLeave);
            Assert.IsTrue(mbLeave.CanView);
            Assert.IsTrue(mbLeave.CanAdd); // Gửi đơn
            Assert.IsFalse(mbLeave.CanDelete);
        }

        [Test]
        public void Test10_RightType_MetadataDistinguishesPlatformLogin_FromBusinessFunctions()
        {
            // 3 mã đăng nhập nền tảng
            Assert.IsTrue(PlatformFunctionCodes.IsPlatformCode("F_LOGIN_DESKTOP"));
            Assert.IsTrue(PlatformFunctionCodes.IsPlatformCode("F_LOGIN_WEB"));
            Assert.IsTrue(PlatformFunctionCodes.IsPlatformCode("F_LOGIN_MOBILE"));

            // Các mã nghiệp vụ không phải mã đăng nhập nền tảng
            Assert.IsFalse(PlatformFunctionCodes.IsPlatformCode("F_CC_BANGLUONG"));
            Assert.IsFalse(PlatformFunctionCodes.IsPlatformCode("F_NV_HOPDONG"));
            Assert.IsFalse(PlatformFunctionCodes.IsPlatformCode("MOBILE_PROFILE_VIEW"));
        }

        #endregion
    }
}
