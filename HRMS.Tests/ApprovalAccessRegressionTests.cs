using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Bu.CLASS_SECURITY;
using Bu.CLASS_SYSTEM;
using Bu.CLASS_PAYROLL;
using Bu.DTO;
using DA;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture, NonParallelizable]
    public class ApprovalAccessRegressionTests
    {
        [SetUp] public void Setup() { UserSession.Clear(); }
        [TearDown] public void Cleanup() { UserSession.Clear(); }
        private const string Function = "F_NV_PHEDUYET";
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static Type DesktopType(string name) => typeof(QLyNSu.Functions.TranslationManager).Assembly.GetType(name, true);
        private static void Login(string username, bool parent, bool view, bool edit, bool print)
        {
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 80, USERNAME = username, DISABLED = 0 };
            UserSession.CurrentSessionId = "test-session";
            UserSession.CurrentJti = "test-jti";
            UserSession.CurrentChannel = "DESKTOP";
            UserSession.DetailedRights = new Dictionary<string, UserRightDetail>(StringComparer.OrdinalIgnoreCase) {
                { "F_LOGIN_DESKTOP", new UserRightDetail { CAN_VIEW = parent } },
                { Function, new UserRightDetail { CAN_VIEW = view, CAN_EDIT = edit, CAN_PRINT = print } }
            };
        }
        [TestCase(ChannelAction.View, true)]
        [TestCase(ChannelAction.Edit, true)]
        [TestCase(ChannelAction.Print, true)]
        [TestCase(ChannelAction.Add, true)]
        [TestCase(ChannelAction.Delete, false)]
        public void DesktopApprovalSupportsOnlyImplementedActions(ChannelAction action, bool supported)
        {
            Assert.That(ChannelCapabilityRegistry.IsActionSupported(AppChannels.Desktop, Function, action), Is.EqualTo(supported));
            Assert.That(ChannelCapabilityRegistry.IsActionSupported(AppChannels.Mobile, Function, action), Is.False);
        }
        [TestCase("ADMIN", true, true, true)]
        [TestCase("hr.manager", true, true, true)]
        [TestCase("ADMIN", true, false, false)]
        [TestCase("ADMIN", false, true, false)]
        public void ApprovalViewRequiresActualGrantAndPlatform(string username, bool parent, bool view, bool allowed)
        {
            Login(username, parent, view, true, true);
            Assert.That(UserSession.CanView(Function), Is.EqualTo(allowed));
        }
        [Test]
        public void ExpiredSessionDoesNotEnableApproval()
        {
            Login("ADMIN", true, true, true, true);
            UserSession.CurrentJti = null;
            Assert.That(UserSession.CanView(Function), Is.False);
        }
        [TestCase(true, false, false, true, false, false)]
        [TestCase(true, true, true, true, true, true)]
        [TestCase(false, true, true, false, false, false)]
        [TestCase(true, false, true, true, false, true)]
        [Apartment(ApartmentState.STA)]
        public void RealApprovalFormSeparatesViewEditAndPrint(bool view, bool edit, bool print, bool refresh, bool approve, bool export)
        {
            Login("ADMIN", true, view, edit, print);
            var type = DesktopType("QLyNSu.FORM_NHANSU.FrmPheDuyetYeuCau");
            using (var form = (Form)Activator.CreateInstance(type)) {
                type.GetMethod("ApplyApprovalPermissions", Private).Invoke(form, null);
                Assert.That(((Control)type.GetField("btnRefreshAll", Private).GetValue(form)).Enabled, Is.EqualTo(refresh));
                foreach (var name in new[] { "btnApproveLeave", "btnRejectLeave", "btnApproveAtt", "btnRejectAtt", "btnApproveOt", "btnRejectOt" })
                    Assert.That(((Control)type.GetField(name, Private).GetValue(form)).Enabled, Is.EqualTo(approve), name);
                foreach (var name in new[] { "btnExportLeave", "btnExportAtt", "btnExportOt" })
                    Assert.That(((Control)type.GetField(name, Private).GetValue(form)).Enabled, Is.EqualTo(export), name);
            }
        }
        private static string Readiness(PolicyGroupStatus status)
        {
            var type = DesktopType("QLyNSu.FORM_CHAMCONG.FrmBangLuong");
            return (string)type.GetMethod("BuildPolicyReadinessMessage", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { new[] { new PolicyGroupInspectionResult { GroupKey = "SALARY", Status = status, CorrelationId = "test-ref" } }, 202610 });
        }
        [Test]
        public void MissingTableMessageIncludesSchemaAndRightsInsteadOfDemandingMigration()
        {
            var message = Readiness(PolicyGroupStatus.ObjectNotFound);
            Assert.That(message, Does.Contain("ORA-00942"));
            Assert.That(message, Does.Contain("schema"));
            Assert.That(message, Does.Contain("SELECT"));
            Assert.That(message, Does.Not.Contain("V1_16"));
            Assert.That(message.Length, Is.LessThan(1000));
        }
        [TestCase(PolicyGroupStatus.ConnectionFailed, "kết nối")]
        [TestCase(PolicyGroupStatus.NoEffectivePolicy, "cuối kỳ")]
        public void ReadinessDistinguishesConnectionFromEffectiveDate(PolicyGroupStatus status, string expected)
        {
            var message = Readiness(status);
            Assert.That(message.ToLowerInvariant(), Does.Contain(expected));
            Assert.That(message, Does.Not.Contain("ORA-00942"));
            Assert.That(message, Does.Not.Contain("V1_16"));
        }
    }
}
