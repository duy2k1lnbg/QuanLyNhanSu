using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Bu.CLASS_CHAMCONG;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using Bu.DTO;
using DA;
using NUnit.Framework;
namespace HRMS.Tests
{
    [TestFixture, NonParallelizable]
    public class PayrollFollowupUiTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static Type Desktop(string name) => typeof(QLyNSu.Functions.TranslationManager).Assembly.GetType("QLyNSu.FORM_CHAMCONG." + name, true);
        private static object Field(object value,string name) => value.GetType().GetField(name,Private).GetValue(value);
        private static object Property(object value,string name) => value.GetType().GetProperties().First(p => p.Name == name && p.GetIndexParameters().Length == 0).GetValue(value);
        private static void Login(bool edit)
        {
            UserSession.Clear(); UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 80, USERNAME = "ADMIN", DISABLED = 0 };
            UserSession.CurrentSessionId = "test"; UserSession.CurrentJti = "test"; UserSession.CurrentChannel = "DESKTOP";
            UserSession.DetailedRights = new Dictionary<string,UserRightDetail> {
                {"F_LOGIN_DESKTOP",new UserRightDetail {CAN_VIEW=true}},
                {"F_CC_BANGCONG",new UserRightDetail {CAN_VIEW=true,CAN_EDIT=edit}},
                {"F_CC_BANGLUONG",new UserRightDetail {CAN_VIEW=true,CAN_EDIT=edit}},
                {"F_CC_PHUCAP",new UserRightDetail {CAN_VIEW=edit}},
                {"F_CC_UNGLUONG",new UserRightDetail {CAN_VIEW=edit}}
            };
        }
        [TearDown] public void Cleanup() => UserSession.Clear();
        [TestCase(false)][TestCase(true)]
        public void AdminNameDoesNotBypassPeriodLockPermission(bool unlock)
        {
            Login(false); bool reached = false;
            var service = new KYCONG(() => {reached=true; throw new Exception("Must not access DB");});
            Assert.Throws<BusinessException>(() => { if(unlock) service.UnlockKyCong(202610,80,"test"); else service.LockKyCong(202610,80); });
            Assert.That(reached,Is.False);
        }
        [TestCase(0,0,false)][TestCase(100,1,false)][TestCase(100,0,true)]
        public void OnlyPublishedCompleteAttendanceCanBeLocked(int total,int blocked,bool valid)
        {
            var result = KYCONG.AssessLockAttendance(202610, total, blocked);
            Assert.That(result.Success, Is.EqualTo(valid));
            Assert.That(result.Period, Is.EqualTo(202610));
            if (!valid) Assert.That(result.ErrorCode, Is.EqualTo("LOCK_NOT_READY"));
        }
        [Test,Apartment(ApartmentState.STA)]
        public void LockActionIsVisibleAndFollowsSelectionEditingAndPermission()
        {
            Login(true);var type=Desktop("FrmBangCong");using(var form=(Form)Activator.CreateInstance(type)) {
                type.GetField("_MAKYCONG",Private).SetValue(form,202610);
                var save=Field(form,"btnLuu"); save.GetType().GetProperty("Enabled").SetValue(save,false);
                var action=Field(form,"_periodLock");var update=type.GetMethod("UpdateLockAction",Private);
                update.Invoke(form,null); Assert.That(Property(action,"Enabled"),Is.True);
                ((CheckBox)Field(form,"chkKhoa")).Checked=true;update.Invoke(form,null);
                Assert.That(Property(action,"Caption"),Is.EqualTo("Mở khóa kỳ công"));
                save.GetType().GetProperty("Enabled").SetValue(save,true);update.Invoke(form,null);Assert.That(Property(action,"Enabled"),Is.False);
                save.GetType().GetProperty("Enabled").SetValue(save,false);Login(false);update.Invoke(form,null);Assert.That(Property(action,"Enabled"),Is.False);
            }
        }
        [TestCase("Khoản phát sinh nhập riêng","PHATSINH",1)]
        [TestCase("Phụ cấp hợp đồng","PHUCAP",1)]
        [TestCase("Ứng lương","UNGLUONG",1)]
        [TestCase("Tất cả nguồn",null,3)]
        public void SourceFilterKeepsReferencesSeparateFromEditableOccurrences(string filter,string expected,int count)
        {
            var items=new[]{new PayrollOccurrenceDto{SourceType="PHATSINH"},new PayrollOccurrenceDto{SourceType="PHUCAP"},new PayrollOccurrenceDto{SourceType="UNGLUONG"}};
            var method=Desktop("FrmPhatSinhLuong").GetMethod("FilterBySource",BindingFlags.Static|BindingFlags.NonPublic);
            var result=((IEnumerable<PayrollOccurrenceDto>)method.Invoke(null,new object[]{items,filter})).ToList();Assert.That(result.Count,Is.EqualTo(count));
            if(expected!=null)Assert.That(result.All(item=>item.SourceType==expected),Is.True);
        }
        [TestCase("PHUCAP")][TestCase("UNGLUONG")]
        [Apartment(ApartmentState.STA)]
        public void ReferenceRowsHaveManagementNavigationWithoutOccurrenceApproval(string source)
        {
            Login(true);var type=Desktop("FrmPhatSinhLuong");using(var form=(Form)Activator.CreateInstance(type)) {
                var update=type.GetMethod("UpdateOccurrenceActions",Private);update.Invoke(form,new object[]{new PayrollOccurrenceDto {SourceType=source,CanApprove=true,CanRevoke=true,CanDelete=true}});
                Assert.That(Property(Field(form,"btnDuyet"),"Enabled"),Is.False);
                Assert.That(Property(Field(form,"_manageSource"),"Enabled"),Is.True);
                Login(false);update.Invoke(form,new object[]{new PayrollOccurrenceDto {SourceType=source}});Assert.That(Property(Field(form,"_manageSource"),"Enabled"),Is.False);
            }
        }
        [Test,Apartment(ApartmentState.STA)]
        public void PayrollToolbarsUseSameFontForNormalAndDisabledStates()
        {
            foreach(var name in new[]{"FrmBangCong","FrmBangLuong","FrmPhatSinhLuong"}) using(var form=(Form)Activator.CreateInstance(Desktop(name))) {
                foreach(var item in (IEnumerable)Property(Field(form,"barManager1"),"Items")) {
                    var appearance=Property(item,"ItemAppearance");var normal=(System.Drawing.Font)Property(Property(appearance,"Normal"),"Font");var disabled=(System.Drawing.Font)Property(Property(appearance,"Disabled"),"Font");
                    Assert.That(normal.Size,Is.EqualTo(9.5f),name);Assert.That(disabled.Size,Is.EqualTo(normal.Size),name);Assert.That(normal.Name,Is.EqualTo("Segoe UI"));
                }
            }
        }
        [TestCase("LockKyCong")][TestCase("UnlockKyCong")]
        public void WebPeriodLockRequiresExplicitEditGrant(string methodName)
        {
            var method = typeof(HRMS_API.Controllers.ChamCongController).GetMethod(methodName);
            var attribute = method.GetCustomAttributes(false).First(a => a.GetType().Name == "JwtAuthorizeAttribute");
            Assert.That(Property(attribute,"Right"),Is.EqualTo("F_CC_BANGCONG"));
            Assert.That(Property(attribute,"Action").ToString(),Is.EqualTo("Edit"));
        }
        [Test,Apartment(ApartmentState.STA)]
        public void WorkflowHasFourFooterActionsAndNoEditableFilterRow()
        {
            Login(true);using(var form=(Form)Activator.CreateInstance(Desktop("FrmPayrollWorkflow"),new object[]{202610})) {
                var footer=form.Controls.OfType<FlowLayoutPanel>().Single();Assert.That(footer.Controls.Count,Is.EqualTo(4));
                var options=Property(Field(form,"_issueView"),"OptionsView");Assert.That(Property(options,"ShowAutoFilterRow"),Is.False);
                var report=new PayrollReadinessReport {Period=202610,CheckedAt=DateTime.Now};report.Steps.Add(new PayrollPreparationStep {Title="Hồ sơ thuế",Guidance="Kiểm tra hồ sơ",Status="Đã kiểm tra"});
                form.GetType().GetMethod("ApplyReport").Invoke(form,new object[]{report});Assert.That(((Control)Field(form,"_calculate")).Enabled,Is.True);
            }
        }
    }
}
