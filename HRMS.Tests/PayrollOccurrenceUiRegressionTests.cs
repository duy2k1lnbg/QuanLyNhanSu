using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Bu.CLASS_SYSTEM;
using Bu.CLASS_PAYROLL;
using Bu.DTO;
using DA;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture, NonParallelizable]
    public class PayrollOccurrenceUiRegressionTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [SetUp] public void Setup() { UserSession.Clear(); }
        [TearDown] public void Cleanup() { UserSession.Clear(); }
        private static void Login(bool edit)
        {
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER=80, USERNAME="ADMIN", DISABLED=0 };
            UserSession.CurrentSessionId="test-session"; UserSession.CurrentJti="test-jti";
            UserSession.CurrentChannel="DESKTOP";
            UserSession.DetailedRights = new Dictionary<string,UserRightDetail> {
                {"F_LOGIN_DESKTOP",new UserRightDetail{CAN_VIEW=true}},
                {"F_CC_BANGLUONG",new UserRightDetail{CAN_VIEW=true,CAN_EDIT=edit,CAN_DELETE=edit,CAN_ADD=edit}}
            };
        }
        [TestCase("PHUCAP",true,true,true,false,false,false)]
        [TestCase("UNGLUONG",true,true,true,false,false,false)]
        [TestCase("PHATSINH",true,false,true,true,false,true)]
        [TestCase("PHATSINH",false,true,false,false,true,false)]
        [TestCase("PHATSINH",false,false,true,false,false,true)]
        [TestCase(null,false,false,false,false,false,false)]
        [Apartment(ApartmentState.STA)]
        public void SelectedSourceAndStateControlRealToolbar(string source,bool canApprove,bool canRevoke,bool canDelete,bool approve,bool revoke,bool delete)
        {
            Login(true);
            Verify(source == null ? null : new PayrollOccurrenceDto {SourceType=source,CanApprove=canApprove,CanRevoke=canRevoke,CanDelete=canDelete},approve,revoke,delete);
        }
        [Test,Apartment(ApartmentState.STA)]
        public void AdminNameDoesNotEnableOccurrenceButtonsWithoutEditGrant()
        {
            Login(false);
            Verify(new PayrollOccurrenceDto {SourceType="PHATSINH",CanApprove=true,CanRevoke=true,CanDelete=true},false,false,false);
        }
        private static void Verify(PayrollOccurrenceDto item,bool approve,bool revoke,bool delete)
        {
            var type=typeof(QLyNSu.Functions.TranslationManager).Assembly.GetType("QLyNSu.FORM_CHAMCONG.FrmPhatSinhLuong",true);
            using(var form=(Form)Activator.CreateInstance(type)) {
                type.GetMethod("UpdateOccurrenceActions",Private).Invoke(form,new object[]{item});
                foreach(var pair in new[]{Tuple.Create("btnDuyet",approve),Tuple.Create("btnThuHoi",revoke),Tuple.Create("btnXoaMem",delete)}) {
                    var button=type.GetField(pair.Item1,Private).GetValue(form);
                    Assert.That(button.GetType().GetProperty("Enabled").GetValue(button),Is.EqualTo(pair.Item2),pair.Item1);
                }
                var label=(Label)type.GetField("_sourceNotice",Private).GetValue(form);
                Assert.That(label.Text,Is.Not.Empty);
            }
        }
        [Test,Apartment(ApartmentState.STA)]
        public void DynamicLabelsKeepLatestContentAcrossTranslation()
        {
            var manager=typeof(QLyNSu.Functions.TranslationManager);
            using(var form=new Form())
            using(var label=new Label {Text="0"}) {
                form.Controls.Add(label);
                QLyNSu.Functions.TranslationManager.Translate(form);
                QLyNSu.Functions.TranslationManager.SetTranslatedText(label,"5");
                QLyNSu.Functions.TranslationManager.Translate(form);
                Assert.That(label.Text,Is.EqualTo("5"));
                QLyNSu.Functions.TranslationManager.SetTranslatedText(label,"Phụ cấp hợp đồng: quản lý tại mục Phụ cấp nhân viên.");
                QLyNSu.Functions.TranslationManager.SetTranslatedText(label,"Tạm ứng: quản lý tại mục Ứng lương.");
                QLyNSu.Functions.TranslationManager.Translate(form);
                Assert.That(label.Text,Is.EqualTo(QLyNSu.Functions.TranslationManager.Translate("Tạm ứng: quản lý tại mục Ứng lương.")));
            }
        }
    }
}
