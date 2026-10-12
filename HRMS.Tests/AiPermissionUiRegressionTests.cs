using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Bu.CLASS_SECURITY;
using Bu.CLASS_SYSTEM;
using Bu.DTO;
using Bu.Services.AI_Services.Security;
using DA;
using HRMS_API.Controllers;
using HRMS_API.Filters;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture, NonParallelizable]
    public class AiPermissionUiRegressionTests
    {
        [SetUp] public void Setup() { UserSession.Clear(); }
        [TearDown] public void Cleanup() { UserSession.Clear(); }
        [TestCase("ADMIN", false, true, false)]
        [TestCase("hr.manager", true, true, true)]
        [TestCase("ADMIN", true, false, false)]
        [TestCase("hr.manager", false, true, false)]
        public void PermissionAdministrationUsesRealEditGrant(string username, bool edit, bool parent, bool allowed)
        {
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 80, USERNAME = username, DISABLED = 0 };
            UserSession.CurrentSessionId = "test-session"; UserSession.CurrentJti = "test-jti";
            UserSession.CurrentChannel = "DESKTOP";
            UserSession.DetailedRights = new Dictionary<string, UserRightDetail>(StringComparer.OrdinalIgnoreCase) {
                { "F_LOGIN_DESKTOP", new UserRightDetail { CAN_VIEW = parent } },
                { "F_SYSTEM_PQ_CHUCNANG", new UserRightDetail { CAN_VIEW = true, CAN_EDIT = edit } }
            };
            Assert.That(new DesktopAdminSecurityContext().IsAdmin(), Is.EqualTo(allowed));
        }
        [Test]
        public void AiApiUsesPermissionGuardInsteadOfNameBasedAdminGuard()
        {
            var guard = typeof(AiScopeAdminController).GetCustomAttributes(typeof(JwtAuthorizeAttribute), true).Cast<JwtAuthorizeAttribute>().Single();
            Assert.That(guard.RequireAdmin, Is.False);
            Assert.That(guard.Right, Is.EqualTo("F_SYSTEM_PQ_CHUCNANG"));
            Assert.That(guard.Action, Is.EqualTo(ChannelAction.Edit));
            var accountGuard = typeof(UserController).GetCustomAttributes(typeof(JwtAuthorizeAttribute), true).Cast<JwtAuthorizeAttribute>().Single();
            Assert.That(accountGuard.RequireAdmin, Is.False);
            foreach (string method in new[] { "SaveChannelPermissions", "SaveBatchChannelPermissions", "SavePlatformAccess" }) {
                var info = typeof(UserController).GetMethods().FirstOrDefault(m => m.Name == method);
                Assert.That(info, Is.Not.Null, method);
                var actionGuard = info.GetCustomAttributes(typeof(JwtAuthorizeAttribute), true).Cast<JwtAuthorizeAttribute>().Single();
                Assert.That(actionGuard.Right, Is.EqualTo("F_SYSTEM_PQ_CHUCNANG"));
                Assert.That(actionGuard.Action, Is.EqualTo(ChannelAction.Edit));
            }
        }
        [Test]
        public void SavingAvailableCapabilitiesRetainsDisabledCapabilityGrants()
        {
            var repo = new InMemoryAiScopeGrantRepository();
            repo.AddUser(101,"ADMIN","Test",manv:1,rights:new[]{"F_SYSTEM_AI","F_DM_NHANVIEN","F_CC_BANGLUONG"});
            var stored = new AiScopeGrantRecord { CapabilityCode="PAYROLL_VIEW", ScopeType="ALL", Effect="ALLOW", IsEnabled=true };
            repo.GrantsStore["USER:101"] = new List<AiScopeGrantRecord> { stored };
            Assert.That(AiCapabilityCatalog.Get("PAYROLL_VIEW").IsEnabled, Is.False);
            var svc = new AiScopeGrantManagementService(repo,new ApiAdminSecurityContext(80,"manager",true));
            var result = svc.SaveSubjectScopeGrants(new SaveAiSubjectScopeGrantsRequest {
                SubjectId=101, SubjectType="USER", BaseRevision=repo.Revision,
                Grants=new List<SaveAiSubjectScopeGrantItemDto> { new SaveAiSubjectScopeGrantItemDto {CapabilityCode="EMPLOYEE_LOOKUP",Effect="ALLOW",ScopeType="ALL"} }
            });
            Assert.That(result.Success,Is.True,result.Message);
            Assert.That(repo.GetGrants("USER",101).Select(g=>g.CapabilityCode),Is.EquivalentTo(new[]{"EMPLOYEE_LOOKUP","PAYROLL_VIEW"}));
        }
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static Type FormType(string name) => typeof(QLyNSu.Functions.AiTypingTimeline).Assembly.GetType("QLyNSu.FORM_SYSTEM." + name, true);
        private static object Get(object obj, string field) => obj.GetType().GetField(field, Private).GetValue(obj);
        private static object Call(object obj, string name, params object[] args) => obj.GetType().GetMethod(name, Private).Invoke(obj, args);

        [Test, Apartment(ApartmentState.STA)]
        public void DesktopEditPreservesAdditionalDepartmentScopesAndDeny()
        {
            var type = FormType("FrmAiScopeGrantDetail");
            using (var form = (Form)Activator.CreateInstance(type, new object[] { 101, "ADMIN", "Test", false })) {
                var rowType = type.GetNestedType("AiScopeRowViewModel"); var row = Activator.CreateInstance(rowType);
                Action<string, object> set = (key, val) => rowType.GetProperty(key).SetValue(row, val);
                set("CapabilityCode", "EMPLOYEE_LOOKUP"); set("DirectEffect", "ALLOW"); set("DirectScopeType", "DEPARTMENT"); set("DirectScopeKey", "4");
                set("DirectGrants", new List<AiScopeGrantItemDto> {
                    new AiScopeGrantItemDto { Effect="ALLOW", ScopeType="DEPARTMENT", ScopeKey="2" },
                    new AiScopeGrantItemDto { Effect="ALLOW", ScopeType="DEPARTMENT", ScopeKey="3" },
                    new AiScopeGrantItemDto { Effect="DENY", ScopeType="ALL" }
                });
                var saved = ((IEnumerable)Call(form, "BuildDirectGrants", row)).Cast<AiScopeGrantRecord>().ToList();
                Assert.That(saved.Count, Is.EqualTo(3)); Assert.That(saved[0].ScopeKey, Is.EqualTo("4"));
                Assert.That(saved[1].ScopeKey, Is.EqualTo("3")); Assert.That(saved[2].Effect, Is.EqualTo("DENY"));
                set("DirectEffect", "NONE"); Assert.That(((IEnumerable)Call(form, "BuildDirectGrants", row)).Cast<object>(), Is.Empty);
            }
        }
        [TestCase("retains-scopes"), TestCase("new-self"), TestCase("group-deny"), TestCase("expired-self"), Apartment(ApartmentState.STA)]
        public void DesktopBinaryPermissionKeepsExistingScopesAndCannotOverrideGroupDeny(string scenario)
        {
            var type=FormType("FrmAiScopeGrantDetail");var rowType=type.GetNestedType("AiScopeRowViewModel");
            var row=Activator.CreateInstance(rowType);
            rowType.GetProperty("CapabilityCode").SetValue(row,"EMPLOYEE_LOOKUP");rowType.GetProperty("IsCapabilityEnabled").SetValue(row,true);
            var direct=new List<AiScopeGrantItemDto>();
            if(scenario=="retains-scopes") {
                direct.Add(new AiScopeGrantItemDto {Effect="ALLOW",ScopeType="DEPARTMENT",ScopeKey="2",ValidTo=new DateTime(2030,1,1),IsActive=true});
                direct.Add(new AiScopeGrantItemDto {Effect="ALLOW",ScopeType="DEPARTMENT",ScopeKey="3",IsActive=true});
            }
            if(scenario=="expired-self")direct.Add(new AiScopeGrantItemDto {Effect="ALLOW",ScopeType="SELF",ValidTo=new DateTime(2000,1,1),IsActive=false});
            rowType.GetProperty("DirectGrants").SetValue(row,direct);
            if(scenario=="group-deny")rowType.GetProperty("InheritedGrants").SetValue(row,new List<AiInheritedScopeGrantDto> {new AiInheritedScopeGrantDto {Effect="DENY",ScopeType="ALL",IsActive=true}});
            var method=type.GetMethod("SetSimplePermission",BindingFlags.NonPublic|BindingFlags.Static);
            if(scenario=="retains-scopes") {
                method.Invoke(null,new[]{row,(object)false});
                Assert.That((bool)rowType.GetProperty("IsAiAllowed").GetValue(row),Is.False);
            }
            method.Invoke(null,new[]{row,(object)true});
            var saved=(List<AiScopeGrantItemDto>)rowType.GetProperty("DirectGrants").GetValue(row);
            if(scenario=="group-deny") {Assert.That(saved,Is.Empty);Assert.That((bool)rowType.GetProperty("IsAiAllowed").GetValue(row),Is.False);}
            else if(scenario=="retains-scopes") {Assert.That(saved.Select(g=>g.ScopeKey),Is.EqualTo(new[]{"2","3"}));Assert.That(saved[0].ValidTo,Is.EqualTo(new DateTime(2030,1,1)));}
            else {Assert.That(saved.Count,Is.EqualTo(1));Assert.That(saved[0].ScopeType,Is.EqualTo("SELF"));Assert.That(saved[0].ValidTo,Is.Null);}
        }
        private static void PopulatePreviewGrants(Form form)
        {
            var type=form.GetType(); var rowType=type.GetNestedType("AiScopeRowViewModel");
            var rows=(IList)Activator.CreateInstance(typeof(System.ComponentModel.BindingList<>).MakeGenericType(rowType));
            foreach(var code in new[]{"EMPLOYEE_LOOKUP","POLICY_LOOKUP"}) {
                var row=Activator.CreateInstance(rowType);
                var values=new Dictionary<string,object> {
                    {"CapabilityCode",code},{"Description",code=="EMPLOYEE_LOOKUP"?"Tra cứu thông tin nhân viên":"Tra cứu nội quy, quy chế công ty"},
                    {"IsCapabilityEnabled",true},{"AvailabilityStatus","READY"},{"FunctionRightDisplay","F_SYSTEM_AI — ĐÃ CÓ"},
                    {"HasRequiredFunctionRight",true},{"DirectEffect","ALLOW"},{"DirectScopeType","ALL"},
                    {"InheritedSummary","Nhóm Nhân sự: ALLOW SELF"},{"EffectiveEffect","ALLOW"},
                    {"IsAiAllowed",true},{"DirectGrants",new List<AiScopeGrantItemDto> {new AiScopeGrantItemDto {CapabilityCode=code,Effect="ALLOW",ScopeType="ALL",IsActive=true}}},{"EffectiveScopeSummary","Toàn bộ dữ liệu"},{"ExplanationNotes","Cần quyền nghiệp vụ nền và phạm vi AI."}
                };
                foreach(var entry in values)rowType.GetProperty(entry.Key).SetValue(row,entry.Value);
                rows.Add(row);
            }
            type.GetField("_bindingList",Private).SetValue(form,rows);
            var grid=Get(form,"gridControlGrants");grid.GetType().GetProperty("DataSource").SetValue(grid,rows);
            ((Control)Get(form,"lblSubjectTitle")).Text="Tài khoản: ADMIN — Phân quyền AI";
            ((Control)Get(form,"lblAiRightStatus")).Text="Quyền giao diện AI (F_SYSTEM_AI): ĐÃ CẤP";
        }
        [TestCase(960, 640), TestCase(1280, 800), Apartment(ApartmentState.STA)]
        public void EmbeddedAiTabFitsBesideAccountListAndBelowPlatformHeader(int width,int height)
        {
            var type=FormType("FrmPhanQuyenChucNang");var childType=FormType("FrmAiScopeGrantDetail");
            using(var parent=(Form)Activator.CreateInstance(type))
            using(var child=(Form)Activator.CreateInstance(childType,new object[]{80,"ADMIN","Kiểm thử",false})) {
                parent.Load-=(EventHandler)Delegate.CreateDelegate(typeof(EventHandler),parent,type.GetMethod("FrmPhanQuyenChucNang_Load",Private));
                child.Load-=(EventHandler)Delegate.CreateDelegate(typeof(EventHandler),child,childType.GetMethod("FrmAiScopeGrantDetail_Load",Private));
                parent.MinimumSize=new Size(960,640);parent.ClientSize=new Size(width,height);
                Call(parent,"SetupTogglePanel");Call(parent,"SetupChannelHeaderPanel");Call(parent,"SetupActionButtons");
                type.GetField("_switchingPermissionPage",Private).SetValue(parent,true);
                ((RadioButton)Get(parent,"rdoAiPermissions")).Checked=true;
                foreach(string field in new[]{"btnSua","btnLuu","btnHuy","btnLamMoi","btnChonTatCa","btnBoChonTatCa","gcRight","chkParentAccess","lblParentInherited","lblReadinessStatus"}) ((Control)Get(parent,field)).Visible=false;
                Call(child,"ConfigureResponsiveLayout");PopulatePreviewGrants(child);
                childType.GetMethod("ConfigureEmbedded").Invoke(child,null);
                type.GetField("_aiPermissionsForm",Private).SetValue(parent,child);
                dynamic split=Get(parent,"splitContainerControl1");
                Control panel=split.Panel2;panel.Controls.Add(child);child.BringToFront();
                parent.ShowInTaskbar=false;parent.Opacity=0;parent.StartPosition=FormStartPosition.Manual;parent.Location=new Point(-30000,-30000);
                parent.Show();child.Show();Application.DoEvents();parent.PerformLayout();panel.PerformLayout();
                var header=(Control)Get(parent,"pnlChannelHeader");
                foreach (string field in new[] { "rdoAllSubjects", "rdoGroup", "rdoUser" }) {
                    var option = (Control)Get(parent, field);
                    Assert.That(option.Bottom, Is.LessThanOrEqualTo(option.Parent.ClientSize.Height), "Subject filter is clipped");
                    Assert.That(option.Parent.Bottom, Is.LessThanOrEqualTo(option.Parent.Parent.ClientSize.Height), "Subject filter container is clipped");
                }
                Assert.That(header.Bounds.IntersectsWith(child.Bounds),Is.False,"AI child covers platform tabs");
                Assert.That(child.Top,Is.GreaterThanOrEqualTo(header.Bottom));Assert.That(child.Right,Is.LessThanOrEqualTo(panel.Width));
                Assert.That(child.Height,Is.GreaterThan(250));
                var childView=Get(child,"gridViewGrants");
                var columns=(IEnumerable)childView.GetType().GetProperty("Columns").GetValue(childView);
                Assert.That(columns.Cast<object>().Where(c=>(bool)c.GetType().GetProperty("Visible").GetValue(c)).Select(c=>(string)c.GetType().GetProperty("FieldName").GetValue(c)),Is.EquivalentTo(new[]{"Description","IsAiAllowed"}),"Only the function and Yes/No control should be visible");
                var childTop=(Control)Get(child,"pnlTop");
                var table=(TableLayoutPanel)childTop.Controls[0];table.PerformLayout();
                Assert.That(table.HorizontalScroll.Visible,Is.False,"Header must wrap instead of scrolling horizontally");
                foreach(Control label in table.Controls) {
                    Assert.That(label.Right,Is.LessThanOrEqualTo(table.ClientSize.Width),"Header label overflows narrow AI pane");
                    Assert.That(label.Bottom,Is.LessThanOrEqualTo(table.ClientSize.Height),"Header text is clipped vertically");
                }
                string path=Environment.GetEnvironmentVariable("HRMS_UI_ARTIFACT_DIR");
                if(!string.IsNullOrEmpty(path))using(var bitmap=new Bitmap(parent.Width,parent.Height)){parent.DrawToBitmap(bitmap,new Rectangle(Point.Empty,parent.Size));bitmap.Save(System.IO.Path.Combine(path,"desktop-permissions-"+width+".png"));}
            }
        }
        [TestCase(960, 640), TestCase(1280, 800), Apartment(ApartmentState.STA)]
        public void DesktopLayoutKeepsActionsAndTitleWithinTheirPanels(int width, int height)
        {
            var type = FormType("FrmAiScopeGrantDetail");
            using (var form = (Form)Activator.CreateInstance(type, new object[] { 101, "ADMIN", "Test", false })) {
                Call(form, "ConfigureResponsiveLayout"); form.ClientSize = new Size(width, height); form.CreateControl(); form.PerformLayout();
                var top = (Control)Get(form, "pnlTop"); var bottom = (Control)Get(form, "pnlBottom");
                var title = (Control)Get(form, "lblSubjectTitle"); var grid = (Control)Get(form, "gridControlGrants");
                PopulatePreviewGrants(form);
                title.Text="Tài khoản: ADMIN — Phân quyền AI";
                // Realize native controls without invoking the Oracle-backed Load handler.
                form.Load -= (EventHandler)Delegate.CreateDelegate(typeof(EventHandler),form,type.GetMethod("FrmAiScopeGrantDetail_Load",Private));
                form.ShowInTaskbar=false; form.Opacity=0; form.StartPosition=FormStartPosition.Manual;
                form.Location=new Point(-30000,-30000); form.Show(); Application.DoEvents();
                top.PerformLayout(); bottom.PerformLayout();

                Assert.That(top.Bounds.IntersectsWith(grid.Bounds), Is.False, "Header and grid overlap");
                Assert.That(bottom.Bounds.IntersectsWith(grid.Bounds), Is.False, "Footer and grid overlap");
                Assert.That(title.Width, Is.GreaterThan(width - 50));
                foreach(Control button in bottom.Controls[0].Controls) {
                    Assert.That(button.Left, Is.GreaterThanOrEqualTo(0));
                    Assert.That(button.Right, Is.LessThanOrEqualTo(bottom.ClientSize.Width));
                    Assert.That(button.Bottom, Is.LessThanOrEqualTo(bottom.ClientSize.Height));
                }
                string path = Environment.GetEnvironmentVariable("HRMS_UI_ARTIFACT_DIR");
                if(!string.IsNullOrEmpty(path)) using(var bitmap=new Bitmap(form.Width,form.Height)) { form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height)); bitmap.Save(System.IO.Path.Combine(path,"desktop-ai-"+width+".png")); }
            }
        }
    }
}
