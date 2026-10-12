using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bu.CLASS_CHAMCONG;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using NUnit.Framework;
namespace HRMS.Tests
{
    [TestFixture,NonParallelizable]
    public class PayrollProfileSaveRegressionTests
    {
        [TearDown] public void Cleanup() => UserSession.Clear();
        private static PayrollProfileUsage Usage(string status="CALCULATED",int locked=0,int legacy=0,int paid=0) =>
            new PayrollProfileUsage { MAKYCONG=202610,STATUS=status,LOCKED=locked,LEGACY=legacy,PAYMENT_BLOCKED=paid };
        [TestCase("DRAFT")][TestCase("CALCULATED")][TestCase("CAN_TINH_LAI")]
        public void TaxProfileReferencedOnlyByOpenDraftsCanBeEdited(string status)
        {
            Assert.That(PayrollPreparationDataService.CanEditOpenProfile(PayrollPreparationDataService.Find("TB_NHANVIEN_THUE"),new[]{Usage(status)}),Is.True);
        }
        [TestCase("APPROVED")][TestCase("PUBLISHED")][TestCase("PAID")][TestCase("LEGACY_READONLY")][TestCase("UNKNOWN")][TestCase(null)]
        public void ApprovedPaidHistoricalAndUnknownStatusesStayProtected(string status)
        {
            Assert.That(PayrollPreparationDataService.CanEditOpenProfile(PayrollPreparationDataService.Find("TB_NHANVIEN_THUE"),new[]{Usage(status)}),Is.False);
        }
        [TestCase(1,0,0)][TestCase(0,1,0)][TestCase(0,0,1)]
        public void LockedLegacyAndPaymentStartedPeriodsStayProtected(int locked,int legacy,int paid)
        {
            Assert.That(PayrollPreparationDataService.CanEditOpenProfile(PayrollPreparationDataService.Find("TB_NHANVIEN_THUE"),new[]{Usage(locked:locked,legacy:legacy,paid:paid)}),Is.False);
        }
        [Test]
        public void AClosedPeriodCannotBeOverriddenByAnOpenPeriod()
        {
            Assert.That(PayrollPreparationDataService.CanEditOpenProfile(PayrollPreparationDataService.Find("TB_NHANVIEN_THUE"),new[]{Usage(),Usage(locked:1)}),Is.False);
        }
        [TestCase("TB_NHANVIEN_BAOHIEM_THAM_GIA")][TestCase("TB_NHANVIEN_CONG_DOAN_THAM_GIA")]
        public void OtherReusableEmployeeProfilesFollowTheSameRule(string table)
        {
            Assert.That(PayrollPreparationDataService.CanEditOpenProfile(PayrollPreparationDataService.Find(table),new[]{Usage()}),Is.True);
        }
        [TestCase("TB_CHINH_SACH_LUONG")][TestCase("TB_THUE_TNCN_CHINH_SACH")]
        public void UsedPayrollPoliciesRemainImmutable(string table)
        {
            Assert.That(PayrollPreparationDataService.CanEditOpenProfile(PayrollPreparationDataService.Find(table),new[]{Usage()}),Is.False);
        }
        [Test]
        public async Task DeniedSaveReturnsAResultInsideTheWorkerWithoutAccessingDatabase()
        {
            UserSession.Clear();
            var result=await Task.Run(()=>new PayrollPreparationDataService().TrySave("TB_NHANVIEN_THUE",new Dictionary<string,object>(),null,202609,"test"));
            Assert.That(result.Success,Is.False); Assert.That(result.ErrorCode,Is.EqualTo("PERMISSION_DENIED"));
        }
        [Test]
        public async Task MissingIdentityReturnsTheFieldWithoutAnUnhandledWorkerException()
        {
            PayrollDataEditorRegressionTests.Login(true);
            var page=PayrollDataEditorRegressionTests.Page();
            var values=page.Fields.ToDictionary(f=>f.Name,f=>page.Rows.Rows[0][f.Name]); values["TRANG_THAI_HO_SO"]="DA_XAC_MINH";
            var result=await Task.Run(()=>new PayrollPreparationDataService().TrySave(page.Section.Table,values,null,202609,"test"));
            Assert.That(result.Success,Is.False); Assert.That(result.ErrorCode,Is.EqualTo("INVALID_INPUT")); Assert.That(result.TargetField,Is.EqualTo("SO_HO_CHIEU"));
        }
        [Test]
        public void AReusableTaxProfileDoesNotNeedAnArtificialExpiryDate()
        {
            var page=PayrollDataEditorRegressionTests.Page(); var values=page.Fields.ToDictionary(f=>f.Name,f=>page.Rows.Rows[0][f.Name]);
            Assert.That(values["NGAY_HET_HIEU_LUC"],Is.EqualTo(DBNull.Value));
            Assert.DoesNotThrow(()=>PayrollPreparationDataService.Validate(page.Section,values));
        }
        [TestCase(0,true)][TestCase(1,false)]
        public void StalePayrollMustBeRecalculatedBeforePeriodLock(int count,bool success)
        {
            var result=KYCONG.AssessLockPayroll(202610,count);
            Assert.That(result.Success,Is.EqualTo(success));
            if(!success) Assert.That(result.ErrorCode,Is.EqualTo("PAYROLL_INPUT_CHANGED"));
        }
    }
}
