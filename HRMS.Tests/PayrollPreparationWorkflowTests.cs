using System;
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
    [TestFixture,NonParallelizable]
    public class PayrollPreparationWorkflowTests
    {
        [SetUp] public void Setup()=>UserSession.Clear();
        [TearDown] public void Cleanup()=>UserSession.Clear();
        private static void Login(bool edit,bool add=false)
        {
            UserSession.CurrentUser=new TB_SYS_USER {IDUSER=80,USERNAME="ADMIN",DISABLED=0};UserSession.CurrentSessionId="test";UserSession.CurrentJti="test";UserSession.CurrentChannel="DESKTOP";
            UserSession.DetailedRights=new Dictionary<string,UserRightDetail> {
                {"F_LOGIN_DESKTOP",new UserRightDetail {CAN_VIEW=true}},
                {"F_CC_BANGLUONG",new UserRightDetail {CAN_VIEW=true,CAN_EDIT=edit,CAN_ADD=add}},
                {"F_CC_BANGCONG",new UserRightDetail {CAN_VIEW=true,CAN_EDIT=edit,CAN_ADD=add}},
                {"F_NV_PHEDUYET",new UserRightDetail {CAN_VIEW=true,CAN_EDIT=edit,CAN_ADD=add}}
            };
        }
        [TestCase(202609,true)][TestCase(202610,true)][TestCase(202600,false)][TestCase(202613,false)][TestCase(199912,false)][TestCase(210101,false)]
        public void PeriodMustRepresentAnActualMonth(int period,bool valid)=>Assert.That(PayrollReadinessService.IsValidPeriod(period),Is.EqualTo(valid));
        [Test] public void AnyBlockingStepPreventsCalculationEvenWhenOtherStepsPass()
        {
            var r=new PayrollReadinessReport();Assert.That(r.CanCalculate,Is.False);
            r.Steps.Add(new PayrollPreparationStep {Blocking=false});Assert.That(r.CanCalculate,Is.True);
            r.Steps.Add(new PayrollPreparationStep {Blocking=true,Target=PayrollPreparationTarget.Attendance});Assert.That(r.CanCalculate,Is.False);
        }
        [Test] public void ProfileDateFollowsContractEndingWithinMonth()
        {
            var end=new DateTime(2026,9,30);Assert.That(PayrollReadinessService.ProfileDate(new PayrollReadinessService.EmployeeRow {CONTRACT_END=new DateTime(2026,9,18)},end),Is.EqualTo(new DateTime(2026,9,18)));
            Assert.That(PayrollReadinessService.ProfileDate(new PayrollReadinessService.EmployeeRow(),end),Is.EqualTo(end));
        }
        [Test] public void ProfileResolutionDetectsInclusiveBoundaryOverlapAndMissingDate()
        {
            var rows=new[]{new PayrollReadinessService.ProfileRow {MANV=1,START_DATE=new DateTime(2026,1,1),END_DATE=new DateTime(2026,9,30)},new PayrollReadinessService.ProfileRow {MANV=1,START_DATE=new DateTime(2026,9,30)}};
            Assert.That(PayrollReadinessService.EffectiveProfileCount(rows,1,new DateTime(2026,9,30)),Is.EqualTo(2));
            Assert.That(PayrollReadinessService.EffectiveProfileCount(rows,2,new DateTime(2026,9,30)),Is.Zero);
            Assert.That(PayrollReadinessService.EffectiveProfileCount(rows,1,new DateTime(2026,10,1)),Is.EqualTo(1));
        }
        [Test] public void OpenEndedContractCannotOverlapASecondContract()=>Assert.That(PayrollReadinessService.ContractsOverlap(new DateTime(2026,1,1),null,new DateTime(2026,9,1),null),Is.True);
        [Test] public void AdjacentContractDatesWithoutOverlapRemainValid()=>Assert.That(PayrollReadinessService.ContractsOverlap(new DateTime(2026,1,1),new DateTime(2026,8,31),new DateTime(2026,9,1),null),Is.False);
        private static List<TaxBracketDto> Brackets()=>Enumerable.Range(1,5).Select(i=>new TaxBracketDto {BAC_THUE=(byte)i,CAN_DUOI=(i-1)*10,CAN_TREN=i==5?(decimal?)null:i*10,THUE_SUAT=0.05m*i}).ToList();
        [Test] public void TaxTableMustBeContiguousFromZeroToUnlimited()
        {
            var rows=Brackets();Assert.That(PayrollReadinessService.ValidMonthlyBrackets(rows),Is.True);rows[1].CAN_DUOI=11;Assert.That(PayrollReadinessService.ValidMonthlyBrackets(rows),Is.False);
            rows=Brackets();rows[4].CAN_TREN=50;Assert.That(PayrollReadinessService.ValidMonthlyBrackets(rows),Is.False);
            rows=Brackets();rows[0].CAN_DUOI=1;Assert.That(PayrollReadinessService.ValidMonthlyBrackets(rows),Is.False);
        }
        [Test] public void AdminNameDoesNotAuthorizeEditingOrCreatingProfiles()
        {
            Login(false);Assert.Throws<UnauthorizedAccessException>(()=>PayrollPreparationDataService.Demand("TB_NHANVIEN_THUE",false,true));Assert.Throws<UnauthorizedAccessException>(()=>PayrollPreparationDataService.Demand("TB_NHANVIEN_THUE",true,false));
        }
        [Test] public void ExplicitAddAndEditPermissionsAreRespected() {Login(true,true);Assert.DoesNotThrow(()=>PayrollPreparationDataService.Demand("TB_NHANVIEN_THUE",true,true));}
        [Test] public void TableAndFieldAllowlistRejectsUnexpectedSqlIdentifiers()
        {
            Assert.Throws<InvalidOperationException>(()=>PayrollPreparationDataService.Find("TB_SYS_USER"));
            var section=PayrollPreparationDataService.Find("TB_NHANVIEN_CONG_DOAN_THAM_GIA");
            var values=section.Fields.Split(',').ToDictionary(k=>k,k=>(object)DBNull.Value);values["NOT_AN_ALLOWED_COLUMN"]=1;
            Assert.Throws<ArgumentException>(()=>PayrollPreparationDataService.Validate(section,values));
        }
        [Test] public void RequestCreationRequiresExplicitAddGrant() {
            Login(true,true);UserSession.DetailedRights["F_NV_PHEDUYET"]=new UserRightDetail {CAN_VIEW=true,CAN_EDIT=true,CAN_ADD=false};
            Assert.Throws<UnauthorizedAccessException>(()=>PayrollPreparationDataService.Demand("TB_YEUCAU_NGHIPHEP",true,false));
            UserSession.DetailedRights["F_NV_PHEDUYET"].CAN_ADD=true;
            Assert.DoesNotThrow(()=>PayrollPreparationDataService.Demand("TB_YEUCAU_NGHIPHEP",true,false));
        }
        [TestCase("VANG_CHUA_XAC_NHAN","TB_YEUCAU_NGHIPHEP")]
        [TestCase("THIEU_GIO_VAO","TB_YEUCAU_DIEUCHINHCONG")]
        [TestCase("THIEU_GIO_RA","TB_YEUCAU_DIEUCHINHCONG")]
        [TestCase("OT_CHUA_DUYET","TB_YEUCAU_TANGCA")]
        [TestCase("CHAM_NGOAI_LICH","TB_LICH_LAMVIEC")]
        public void AttendanceBlockerOpensExistingActionableEditor(string code,string expected) {
            Assert.That(PayrollReadinessService.AttendanceAction(code),Is.EqualTo(expected));
            Assert.That(PayrollPreparationDataService.Find(expected),Is.Not.Null);
            Assert.That(PayrollReadinessService.AttendanceAdvice(code),Is.Not.Empty);
        }
        private static Dictionary<string,object> Schedule()=>new Dictionary<string,object> { {"NGAY",new DateTime(2026,9,21)},{"TRANG_THAI_PHAN_CONG","LAM_VIEC"},{"IDCAPHIENBAN",1m},{"IDQUYDINH",1m},{"BATDAU_KEHOACH",new DateTime(2026,9,21,22,0,0)},{"KETTHUC_KEHOACH",new DateTime(2026,9,22,6,0,0)} };
        [Test] public void OvernightSchedulePreservesNextDayEnd() =>Assert.DoesNotThrow(()=>PayrollPreparationDataService.ValidateSchedule(Schedule(),202609));
        [Test] public void MissingShiftOrWrongPeriodIsActionableValidation()
        {
            var schedule=Schedule();schedule["IDCAPHIENBAN"]=DBNull.Value;Assert.Throws<ArgumentException>(()=>PayrollPreparationDataService.ValidateSchedule(schedule,202609));
            Assert.Throws<ArgumentException>(()=>PayrollPreparationDataService.ValidateSchedule(Schedule(),202610));
        }
        private static DayCalculationResult UnreadyDay(string secondCode=null)
        {
            var day=new DayCalculationResult {InputSnapshotJson="{}",GiayThucTe=32400,GiayHuongCongThuong=28800,GiayOtXacNhan=0,DuDieuKienChot=0};
            day.Anomalies.Add(new AttendanceAnomalyDto {MaLoi="OT_CHUA_DUYET",MaSuViecHash="case",InputHash="evidence",TrangThai="CHO_XU_LY"});
            if(secondCode!=null)day.Anomalies.Add(new AttendanceAnomalyDto {MaLoi=secondCode,MaSuViecHash="second",InputHash="evidence",TrangThai="CHO_XU_LY"});return day;
        }
        private static AttendancePreparationReviewService.Decision Decision(string hash="evidence",decimal history=1)=>new AttendancePreparationReviewService.Decision {MA_SU_VIEC_HASH="case",INPUT_HASH=hash,HISTORY_ID=history};
        [Test] public void MatchingExplicitDecisionRetainsPunchSecondsAndPayQuantities()
        {
            var day=UnreadyDay();AttendancePreparationReviewService.ApplyDecisions(day,new[]{Decision()});
            Assert.That(day.DuDieuKienChot,Is.EqualTo(1));Assert.That(day.GiayThucTe,Is.EqualTo(32400));Assert.That(day.GiayHuongCongThuong,Is.EqualTo(28800));Assert.That(day.GiayOtXacNhan,Is.Zero);StringAssert.Contains("ReviewedUnpaidCases",day.InputSnapshotJson);
        }
        [TestCase("changed",1)][TestCase("evidence",0)]
        public void ChangedEvidenceOrMissingDecisionHistoryCannotUnblockDay(string hash,int history) {var day=UnreadyDay();AttendancePreparationReviewService.ApplyDecisions(day,new[]{Decision(hash,history)});Assert.That(day.DuDieuKienChot,Is.Zero);}
        [Test] public void ADecisionCannotWaiveOtherAnomalies() {var day=UnreadyDay("THIEU_CAU_HINH");AttendancePreparationReviewService.ApplyDecisions(day,new[]{Decision()});Assert.That(day.DuDieuKienChot,Is.Zero);}
        [TestCase("VANG_CHUA_XAC_NHAN")][TestCase("THIEU_CAU_HINH")][TestCase("OT_VUOT_GIAY_DUYET")]
        public void StructuralOrPayableCasesRequireSourceCorrection(string code)=>Assert.That(AttendancePreparationReviewService.CanExclude(code),Is.False);
        [Test] public void EvidenceProofRejectsChangedPunchesAndOldSnapshotFormat()
        {
            string current="{\"MaNV\":1,\"Schedule\":{\"IdLich\":2},\"Punches\":[{\"Mabc\":1,\"ThoiDiemVao\":\"08:00\"}]}";
            string prior="{\"EvidenceVersion\":2,\"MaNV\":1,\"ScheduleId\":2,\"Punches\":[{\"Mabc\":1,\"ThoiDiemVao\":\"08:00\"}]}";
            Assert.That(AttendancePreparationReviewService.SameEvidence(prior,current),Is.True);
            Assert.That(AttendancePreparationReviewService.SameEvidence(prior,current.Replace("08:00","09:00")),Is.False);
            Assert.That(AttendancePreparationReviewService.SameEvidence(prior.Replace("\"EvidenceVersion\":2,",""),current),Is.False);
        }
        [Test,Apartment(ApartmentState.STA)] public void WorkflowUsesSelectedPeriodAndDisablesCalculationForBlockedOrUnauthorizedUser()
        {
            Login(true);var type=typeof(QLyNSu.Functions.TranslationManager).Assembly.GetType("QLyNSu.FORM_CHAMCONG.FrmPayrollWorkflow",true);
            using(var form=(Form)Activator.CreateInstance(type,new object[]{202609})) {
                var report=new PayrollReadinessReport {Period=202609,CheckedAt=DateTime.Now};report.Steps.Add(new PayrollPreparationStep {Title="Bảng công",Guidance="394 ngày",Status="Cần xử lý",Blocking=true});
                type.GetMethod("ApplyReport").Invoke(form,new[]{(object)report});var button=(Control)type.GetField("_calculate",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(form);Assert.That(button.Enabled,Is.False);
                report.Steps[0].Blocking=false;type.GetMethod("ApplyReport").Invoke(form,new[]{(object)report});Assert.That(button.Enabled,Is.True);
                Login(false);type.GetMethod("ApplyReport").Invoke(form,new[]{(object)report});Assert.That(button.Enabled,Is.False);
                Assert.That((int)type.GetProperty("Period").GetValue(form),Is.EqualTo(202609));
                report.Period=202610;Assert.Throws<TargetInvocationException>(()=>type.GetMethod("ApplyReport").Invoke(form,new[]{(object)report}));
            }
        }
    }
}
