using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bu.CLASS_CHAMCONG;
using Bu.CLASS_SYSTEM;
using Bu.DTO;
using DA;
using NUnit.Framework;
namespace HRMS.Tests
{
    [TestFixture, NonParallelizable]
    public class DesktopLayoutRepairTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static object Field(object target, string name) => target.GetType().GetField(name, Private).GetValue(target);
        private static object Prop(object target, string name) => target.GetType().GetProperties().First(p => p.Name == name && p.GetIndexParameters().Length == 0).GetValue(target);
        private static void Login(bool edit) {
            UserSession.Clear(); UserSession.CurrentUser = new TB_SYS_USER { IDUSER=80, USERNAME="ADMIN",DISABLED=0 };
            UserSession.CurrentSessionId="test";UserSession.CurrentJti="test";UserSession.CurrentChannel="DESKTOP";
            UserSession.DetailedRights=new Dictionary<string,UserRightDetail> {
                {"F_LOGIN_DESKTOP",new UserRightDetail {CAN_VIEW=true}},
                {"F_CC_BANGCONG",new UserRightDetail {CAN_VIEW=true,CAN_EDIT=edit}}
            };
        }
        [TearDown] public void Clear() => UserSession.Clear();
        [TestCase(false)][TestCase(true)]
        public async Task DeniedLockReturnsAResultFromWorkerWithoutTouchingDatabase(bool unlock) {
            Login(false); bool accessed=false;
            var service=new KYCONG(()=>{accessed=true;throw new Exception("Unexpected DB access");});
            var result=await Task.Run(()=>unlock?service.TryUnlockKyCong(202610,80,"test"):service.TryLockKyCong(202610,80));
            Assert.That(result.Success,Is.False);Assert.That(result.ErrorCode,Is.EqualTo("PERMISSION_DENIED"));
            Assert.That(result.Period,Is.EqualTo(202610));Assert.That(accessed,Is.False);
        }
        [TestCase(0)][TestCase(202600)][TestCase(202613)]
        public void InvalidPeriodNeverOpensAConnection(int period) {
            Login(true);var service=new KYCONG(()=>{throw new AssertionException("Must not open DB");});
            Assert.That(service.TryLockKyCong(period,80).ErrorCode,Is.EqualTo("INVALID_PERIOD"));
            Assert.That(service.TryUnlockKyCong(period,80,"test").ErrorCode,Is.EqualTo("INVALID_PERIOD"));
        }
        [Test] public void UnlockStillRequiresAReason() {
            Login(true);var service=new KYCONG(()=>{throw new AssertionException("Must not open DB");});
            Assert.That(service.TryUnlockKyCong(202610,80," ").ErrorCode,Is.EqualTo("MISSING_UNLOCK_REASON"));
        }
        [Test] public void UnauthenticatedLockDoesNotOpenDatabase() {
            var service=new KYCONG(()=>{throw new AssertionException("Must not open DB");});
            Assert.That(service.TryLockKyCong(202610,0).ErrorCode,Is.EqualTo("UNAUTHENTICATED"));
        }
        [Test] public void ConnectionFailureIsDistinctFromAttendanceNotReady() {
            Login(true);var service=new KYCONG(()=>{throw new InvalidOperationException("Unavailable test connection");});
            Assert.That(service.TryLockKyCong(202610,80).ErrorCode,Is.EqualTo("DATABASE_ERROR"));
        }
        [Test] public void AttendanceFailureNamesTheExactPeriodAndBlockedCount() {
            var result=KYCONG.AssessLockAttendance(202609,6030,394);
            Assert.That(result.Success,Is.False);Assert.That(result.BlockedDays,Is.EqualTo(394));
            Assert.That(result.Message,Does.Contain("202609").And.Contain("394"));
            Assert.That(KYCONG.AssessLockAttendance(202610,6076,0).Success,Is.True);
        }
        [Test,Apartment(ApartmentState.STA)] public void PeriodSelectionFollowsSelectedRowWithoutEditableTextOrDuplicateYears() {
            Login(true);var type=typeof(QLyNSu.Functions.TranslationManager).Assembly.GetType("QLyNSu.FORM_CHAMCONG.FrmBangCong",true);
            using(var form=(Form)Activator.CreateInstance(type)) {
                type.GetMethod("showHide",Private).Invoke(form,new object[]{true});
                var grid=Field(form,"gcDanhSach");var view=Field(form,"gvDanhSach");
                var handle=form.Handle;
                grid.GetType().GetProperty("DataSource").SetValue(grid,new[]{new TB_KYCONG{MAKYCONG=202512,NAM=2025,THANG=12,TRANGTHAI=1,KHOA=0}});
                view.GetType().GetProperty("FocusedRowHandle").SetValue(view,0);
                type.GetMethod("SyncFocusedRow",Private).Invoke(form,null);
                foreach(var pair in new[]{new[]{"cboNam","2025"},new[]{"cboThang","12"}}) {
                    var control=(Control)Field(form,pair[0]);var properties=Prop(control,"Properties");
                    Assert.That(control.Text,Is.EqualTo(pair[1]));Assert.That(Prop(properties,"ReadOnly"),Is.True);
                    Assert.That(Prop(properties,"AllowFocused"),Is.False);
                    Assert.That(Prop(properties,"TextEditStyle").ToString(),Is.EqualTo("DisableTextEditor"));
                    var values=((IEnumerable)Prop(properties,"Items")).Cast<object>().Select(x=>x.ToString()).ToList();
                    Assert.That(values.Count,Is.EqualTo(values.Distinct().Count()));
                }
                type.GetMethod("showHide",Private).Invoke(form,new object[]{false});
                Assert.That(Prop(Prop(Field(form,"cboNam"),"Properties"),"ReadOnly"),Is.False);
                Assert.That(Prop(Prop(Field(form,"cboThang"),"Properties"),"AllowFocused"),Is.True);
            }
        }
        [Test,Apartment(ApartmentState.STA)] public void RibbonKeepsEveryCommandOnceAndCanCollapseWholeGroups() {
            var type=typeof(QLyNSu.Functions.TranslationManager).Assembly.GetType("QLyNSu.MainForm",true);
            using(var form=(Form)Activator.CreateInstance(type)) {
                var ribbon=Field(form,"ribbonControl1");var pages=((IEnumerable)Prop(ribbon,"Pages")).Cast<object>().ToList();
                foreach(var index in new[]{0,2}) {
                    var links=new List<object>();
                    foreach(var group in (IEnumerable)Prop(pages[index],"Groups")) {
                        Assert.That(Prop(group,"AllowMinimize"),Is.True);Assert.That(Prop(group,"State").ToString(),Is.EqualTo("Auto"));
                        foreach(var link in (IEnumerable)Prop(group,"ItemLinks")) {Assert.That(Prop(link,"BeginGroup"),Is.False);links.Add(Prop(link,"Item"));}
                    }
                    Assert.That(links.Count,Is.EqualTo(index==0?16:12));Assert.That(links.Distinct().Count(),Is.EqualTo(links.Count));
                    foreach(var item in links) {
                        Assert.That(Prop(item,"LargeWidth"),Is.EqualTo(0));
                        Assert.That(Prop(item,"RibbonStyle").ToString(),Does.Not.Contain("SmallWithoutText"));
                    }
                }
                var appearance=Prop(Prop(ribbon,"Controller"),"AppearancesRibbon");
                Assert.That(((System.Drawing.Font)Prop(Prop(appearance,"Item"),"Font")).Size,Is.EqualTo(9.5f));
            }
        }
    }
}
