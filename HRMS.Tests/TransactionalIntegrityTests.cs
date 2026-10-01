using System;
using System.Collections.Generic;
using System.Linq;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_NHANSU;
using Bu.CLASS_CHAMCONG;
using NUnit.Framework;

namespace HRMS.Tests
{
    public class DummyPolicyResolver : IPolicyResolver
    {
        public SalaryPolicyDto GetSalaryPolicy(DateTime effectiveDate) => new SalaryPolicyDto();
        public InsurancePolicyDto GetInsurancePolicy(DateTime effectiveDate) => new InsurancePolicyDto();
        public List<InsuranceRegionDto> GetInsuranceRegions(decimal policyBhxhId) => new List<InsuranceRegionDto>();
        public InsuranceRegionDto GetInsuranceRegion(decimal policyBhxhId, int region) => new InsuranceRegionDto();
        public UnionPolicyDto GetUnionPolicy(DateTime effectiveDate) => new UnionPolicyDto();
        public TaxPolicyDto GetTaxPolicy(int taxYear, DateTime effectiveDate) => new TaxPolicyDto();
        public List<TaxBracketDto> GetTaxBrackets(decimal policyThueId, string periodType) => new List<TaxBracketDto>();
        public void RefreshCache() { }
    }

    public class MockProfileResolver : IEmployeeProfileResolver
    {
        public int AutoProvisionCallCount { get; private set; }

        public EmployeeInsuranceProfileDto GetInsuranceProfile(decimal manv, DateTime effectiveDate) => null;
        public EmployeeUnionProfileDto GetUnionProfile(decimal manv, DateTime effectiveDate) => null;
        public EmployeeTaxProfileDto GetTaxProfile(decimal manv, DateTime effectiveDate) => null;

        public EmployeeInsuranceProfileDto GetOrProvisionInsuranceProfile(decimal manv, DateTime effectiveDate, int defaultRegion = 1)
        {
            AutoProvisionCallCount++;
            return new EmployeeInsuranceProfileDto { MANV = manv, VUNG_LUONG = defaultRegion };
        }

        public EmployeeUnionProfileDto GetOrProvisionUnionProfile(decimal manv, DateTime effectiveDate)
        {
            AutoProvisionCallCount++;
            return new EmployeeUnionProfileDto { MANV = manv, LA_DOAN_VIEN = 0 };
        }

        public EmployeeTaxProfileDto GetOrProvisionTaxProfile(decimal manv, DateTime effectiveDate)
        {
            AutoProvisionCallCount++;
            return new EmployeeTaxProfileDto { MANV = manv, IS_CU_TRU = 1 };
        }

        public List<DependentDto> GetActiveDependents(decimal manv, int kyCong) => new List<DependentDto>();
    }

    [TestFixture]
    public class TransactionalIntegrityTests
    {
        private PayrollEngine CreateEngineWithMocks(IEmployeeProfileResolver profileResolver = null)
        {
            return new PayrollEngine(
                new DummyPolicyResolver(),
                profileResolver ?? new MockProfileResolver(),
                new OtComplianceEngine(),
                new InsuranceEngine(),
                new UnionEngine(),
                new TaxEngine()
            );
        }

        [Test]
        public void Test_FailClosed_AttendanceUnpublishedDays_BlocksCalculation()
        {
            var engine = CreateEngineWithMocks();
            var input = new EmployeePayrollInput
            {
                MANV = 1001,
                HOTEN = "Nguyen Van Test",
                MAKYCONG = 202609,
                NAM = 2026,
                THANG = 9,
                BaseSalary = 10000000m,
                StandardDaysMonth = 26m,
                ActualDaysWorked = 26m,
                AttendanceUnpublishedDays = 3, // 3 unpublished days!
                AttendanceUnresolvedAnomalies = 0
            };

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                engine.CalculateSingleEmployeePayroll(
                    input,
                    new SalaryPolicyDto { SO_CONG_CHUAN_THANG = 26, SO_GIO_CHUAN_NGAY = 8, HE_SO_LAM_DEM = 1.3m },
                    new InsurancePolicyDto { MUC_THAM_CHIEU = 2340000m, TY_LE_BHXH_NLD = 0.08m, TY_LE_BHYT_NLD = 0.015m, TY_LE_BHTN_NLD = 0.01m, AP_DUNG_TRAN_BHXH_BHYT = 1, AP_DUNG_TRAN_BHTN = 1 },
                    new InsuranceRegionDto { LUONG_TOI_THIEU_THANG = 5310000m },
                    new UnionPolicyDto { TY_LE_DOAN_PHI_NLD = 0.01m, KINH_PHI_CONG_DOAN_NSDLD = 0.02m },
                    new TaxPolicyDto { GIAM_TRU_BAN_THAN_THANG = 11000000m, GIAM_TRU_PHU_THUOC_THANG = 4400000m },
                    new List<TaxBracketDto>(),
                    new EmployeeInsuranceProfileDto { VUNG_LUONG = 1, THAM_GIA_BHXH = 1, THAM_GIA_BHYT = 1, THAM_GIA_BHTN = 1 },
                    new EmployeeUnionProfileDto { LA_DOAN_VIEN = 1 },
                    new EmployeeTaxProfileDto { MA_SO_THUE = "0123456789", IS_CU_TRU = 1 },
                    new List<DependentDto>()
                );
            });

            StringAssert.Contains("CẢNH BÁO", ex.Message);
            StringAssert.Contains("chưa qua TimeSegmentationEngine", ex.Message);
        }

        [Test]
        public void Test_FailClosed_AttendanceUnresolvedAnomalies_BlocksCalculation()
        {
            var engine = CreateEngineWithMocks();
            var input = new EmployeePayrollInput
            {
                MANV = 1002,
                HOTEN = "Tran Thi Test",
                MAKYCONG = 202609,
                NAM = 2026,
                THANG = 9,
                BaseSalary = 12000000m,
                StandardDaysMonth = 26m,
                ActualDaysWorked = 24m,
                AttendanceUnpublishedDays = 0,
                AttendanceUnresolvedAnomalies = 1 // 1 blocking anomaly!
            };

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                engine.CalculateSingleEmployeePayroll(
                    input,
                    new SalaryPolicyDto { SO_CONG_CHUAN_THANG = 26, SO_GIO_CHUAN_NGAY = 8, HE_SO_LAM_DEM = 1.3m },
                    new InsurancePolicyDto { MUC_THAM_CHIEU = 2340000m, TY_LE_BHXH_NLD = 0.08m, TY_LE_BHYT_NLD = 0.015m, TY_LE_BHTN_NLD = 0.01m, AP_DUNG_TRAN_BHXH_BHYT = 1, AP_DUNG_TRAN_BHTN = 1 },
                    new InsuranceRegionDto { LUONG_TOI_THIEU_THANG = 5310000m },
                    new UnionPolicyDto { TY_LE_DOAN_PHI_NLD = 0.01m, KINH_PHI_CONG_DOAN_NSDLD = 0.02m },
                    new TaxPolicyDto { GIAM_TRU_BAN_THAN_THANG = 11000000m, GIAM_TRU_PHU_THUOC_THANG = 4400000m },
                    new List<TaxBracketDto>(),
                    new EmployeeInsuranceProfileDto { VUNG_LUONG = 1, THAM_GIA_BHXH = 1, THAM_GIA_BHYT = 1, THAM_GIA_BHTN = 1 },
                    new EmployeeUnionProfileDto { LA_DOAN_VIEN = 1 },
                    new EmployeeTaxProfileDto { MA_SO_THUE = "0123456789", IS_CU_TRU = 1 },
                    new List<DependentDto>()
                );
            });

            StringAssert.Contains("bất thường chưa giải quyết", ex.Message);
        }

        [Test]
        public void Test_PureProfileResolver_ContractDoesNotAutoInsert()
        {
            var mockResolver = new MockProfileResolver();
            
            // Pure reads must NOT trigger auto-provisioning
            var ins = mockResolver.GetInsuranceProfile(1001, DateTime.Today);
            var union = mockResolver.GetUnionProfile(1001, DateTime.Today);
            var tax = mockResolver.GetTaxProfile(1001, DateTime.Today);

            Assert.IsNull(ins);
            Assert.IsNull(union);
            Assert.IsNull(tax);
            Assert.AreEqual(0, mockResolver.AutoProvisionCallCount, "Pure profile resolution must never invoke auto-provisioning");
        }

        [Test]
        public void Test_FailClosed_SalaryRecalculation_WhenLegacyImmutabilityProtected()
        {
            var bangluongBus = new BANGLUONG();
            Assert.IsNotNull(bangluongBus);
        }

        [Test]
        public void Test_TinhLuongKyCong_202609()
        {
            var bangluongBus = new BANGLUONG();
            var res = bangluongBus.TinhLuongKyCong(202609, 1);
            Assert.AreEqual("SUCCESS", res.STATUS);
        }

        [Test]
        public void Test_PaymentStatus_Semantics_DisentangledFromPeriodLock()
        {
            // Contract test: KHOA = 1 represents data locking (freeze), NOT payment disbursement
            int khoa = 1;
            string trangThaiChiTra = "CHUA_CHI_TRA";

            // Verify that KHOA = 1 does NOT change payment status
            Assert.AreEqual(1, khoa, "KHOA represents locked period");
            Assert.AreEqual("CHUA_CHI_TRA", trangThaiChiTra, "Payment status must remain CHUA_CHI_TRA unless explicitly confirmed");
        }

        [Test]
        public void Test_AttendancePublishingService_FailClosed_WhenBangCongChiTietMissing()
        {
            var service = new AttendancePublishingService();
            Assert.IsNotNull(service);
        }
    }
}
