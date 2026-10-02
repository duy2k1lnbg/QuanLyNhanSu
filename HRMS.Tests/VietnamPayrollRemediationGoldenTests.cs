using Bu.CLASS_CHAMCONG;
using Bu.CLASS_PAYROLL;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Tests
{
    [TestFixture]
    public class VietnamPayrollRemediationGoldenTests
    {
        private SalaryPolicyDto _salaryPolicy;
        private InsurancePolicyDto _insurancePolicy;
        private InsuranceRegionDto _region1;
        private UnionPolicyDto _unionPolicy;
        private TaxPolicyDto _taxPolicy;
        private List<TaxBracketDto> _monthTaxBrackets;
        private List<TaxBracketDto> _yearTaxBrackets;

        [SetUp]
        public void Setup()
        {
            _salaryPolicy = new SalaryPolicyDto
            {
                SO_CONG_CHUAN_THANG = 26m,
                SO_GIO_CHUAN_NGAY = 8m,
                HE_SO_LAM_DEM = 0.30m
            };

            _insurancePolicy = new InsurancePolicyDto
            {
                MUC_THAM_CHIEU = 2530000m,
                TY_LE_BHXH_NLD = 0.08m,
                TY_LE_BHYT_NLD = 0.015m,
                TY_LE_BHTN_NLD = 0.01m,
                TY_LE_BHXH_NSDLD = 0.17m,
                TY_LE_BHYT_NSDLD = 0.03m,
                TY_LE_BHTN_NSDLD = 0.01m,
                TY_LE_TNLD_BNN_NSDLD = 0.005m,
                AP_DUNG_TRAN_BHXH_BHYT = 1,
                AP_DUNG_TRAN_BHTN = 1
            };

            _region1 = new InsuranceRegionDto
            {
                VUNG_LUONG = 1,
                LUONG_TOI_THIEU_THANG = 5310000m,
                HE_SO_SAN_DOANH_NGHIEP = 1.0m
            };

            _unionPolicy = new UnionPolicyDto
            {
                TY_LE_DOAN_PHI_NLD = 0.005m,
                CAP_PERCENT_STATUTORY_BASE_SALARY = 0.10m,
                KINH_PHI_CONG_DOAN_NSDLD = 0.02m
            };

            _taxPolicy = new TaxPolicyDto
            {
                GIAM_TRU_BAN_THAN_THANG = 15500000m,
                GIAM_TRU_PHU_THUOC_THANG = 6200000m,
                GIAM_TRU_BAN_THAN_NAM = 186000000m,
                GIAM_TRU_PHU_THUOC_NAM = 74400000m
            };

            _monthTaxBrackets = new List<TaxBracketDto>
            {
                new TaxBracketDto { BAC_THUE = 1, CAN_DUOI = 0m, CAN_TREN = 10000000m, THUE_SUAT = 0.05m },
                new TaxBracketDto { BAC_THUE = 2, CAN_DUOI = 10000000m, CAN_TREN = 30000000m, THUE_SUAT = 0.10m },
                new TaxBracketDto { BAC_THUE = 3, CAN_DUOI = 30000000m, CAN_TREN = 60000000m, THUE_SUAT = 0.20m },
                new TaxBracketDto { BAC_THUE = 4, CAN_DUOI = 60000000m, CAN_TREN = 100000000m, THUE_SUAT = 0.30m },
                new TaxBracketDto { BAC_THUE = 5, CAN_DUOI = 100000000m, CAN_TREN = null, THUE_SUAT = 0.35m }
            };

            _yearTaxBrackets = new List<TaxBracketDto>
            {
                new TaxBracketDto { BAC_THUE = 1, CAN_DUOI = 0m, CAN_TREN = 120000000m, THUE_SUAT = 0.05m },
                new TaxBracketDto { BAC_THUE = 2, CAN_DUOI = 120000000m, CAN_TREN = 360000000m, THUE_SUAT = 0.10m },
                new TaxBracketDto { BAC_THUE = 3, CAN_DUOI = 360000000m, CAN_TREN = 720000000m, THUE_SUAT = 0.20m },
                new TaxBracketDto { BAC_THUE = 4, CAN_DUOI = 720000000m, CAN_TREN = 1200000000m, THUE_SUAT = 0.30m },
                new TaxBracketDto { BAC_THUE = 5, CAN_DUOI = 1200000000m, CAN_TREN = null, THUE_SUAT = 0.35m }
            };
        }

        private PayrollCalculationResult Calculate(EmployeePayrollInput input,
            EmployeeInsuranceProfileDto insProfile = null,
            EmployeeUnionProfileDto unionProfile = null,
            EmployeeTaxProfileDto taxProfile = null,
            List<DependentDto> dependents = null)
        {
            insProfile = insProfile ?? new EmployeeInsuranceProfileDto
            {
                THAM_GIA_BHXH = 1,
                THAM_GIA_BHYT = 1,
                THAM_GIA_BHTN = 1,
                THAM_GIA_TNLD_BNN = 1
            };

            unionProfile = unionProfile ?? new EmployeeUnionProfileDto
            {
                LA_DOAN_VIEN = 1
            };

            taxProfile = taxProfile ?? new EmployeeTaxProfileDto
            {
                IS_CU_TRU = 1
            };

            dependents = dependents ?? new List<DependentDto>();

            var engine = new PayrollEngine(new MockPolicyResolver(), new EmployeeProfileResolver());
            return engine.CalculateSingleEmployeePayroll(
                input,
                _salaryPolicy,
                _insurancePolicy,
                _region1,
                _unionPolicy,
                _taxPolicy,
                _monthTaxBrackets,
                insProfile,
                unionProfile,
                taxProfile,
                dependents
            );
        }

        private EmployeePayrollInput BaseInput() => new EmployeePayrollInput
        {
            MANV = 1,
            MAKYCONG = 202609,
            NAM = 2026,
            THANG = 9,
            BaseSalary = 26000000m,
            ActualDaysWorked = 26m,
            LeaveDaysWithPay = 0m,
            NightShiftDays = 0m,
            PublishedOtSeconds = 0m
        };

        [Test]
        public void SAL_01_FixtureA_Baseline_2026_Exact_Amounts()
        {
            // Fixture A: 26M gross, 26 workdays, full insurance, resident, union member
            var res = Calculate(BaseInput());

            // 1. Social Insurance (10.5% of 26M = 2,730,000)
            Assert.AreEqual(2730000m, res.InsuranceEmployee);

            // 2. Taxable Income (26M - 2.73M - 15.5M = 7,770,000)
            Assert.AreEqual(7770000m, res.TaxTrace.TaxableAssessableIncome);

            // 3. PIT (7,770,000 * 5% = 388,500)
            Assert.AreEqual(388500m, res.TaxWithheld);

            // 4. Trade Union Member Dues (0.5% of 26M = 130,000)
            Assert.AreEqual(130000m, res.UnionEmployee);

            // 5. Net Salary: 26,000,000 - 2,730,000 - 388,500 - 130,000 = 22,751,500
            Assert.AreEqual(22751500m, res.NetPay);
        }

        [Test]
        public void SAL_02_FixtureB_PaidLeave_Must_Not_Be_Paid_Twice()
        {
            // Fixture B: 25 workdays + 1 paid leave day. Full-month regular gross must equal 26M!
            var input = BaseInput();
            input.ActualDaysWorked = 26m; // 25 worked + 1 leave equivalent
            input.LeaveDaysWithPay = 1m;

            var res = Calculate(input);
            Assert.AreEqual(26000000m, res.GrossEarnings, "Gross must remain 26,000,000 (not 27M or +200k)");

            var workItem = res.DetailItems.Single(x => x.MA_KHOAN_MUC == "LUONG_CONG_THUCTE");
            var leaveItem = res.DetailItems.Single(x => x.MA_KHOAN_MUC == "LUONG_NGAY_PHEP");

            Assert.AreEqual(25m, workItem.SO_LUONG);
            Assert.AreEqual(25000000m, workItem.THANH_TIEN);
            Assert.AreEqual(1m, leaveItem.SO_LUONG);
            Assert.AreEqual(1000000m, leaveItem.THANH_TIEN);
        }

        [Test]
        public void DED_01_FixtureC_DisciplinaryFine_Must_Not_Automatically_Reduce_Wages()
        {
            // Fixture C: 500k disciplinary fine added (LOAI = 2). Art 127 Labor Code 2019 prohibits wage cuts.
            var baseline = Calculate(BaseInput());

            var penaltyInput = BaseInput();
            penaltyInput.RewardsAndDisciplines.Add(new RewardDisciplineInput
            {
                SOQUYETDINH = "QD-KL-01",
                LOAI = 2,
                SOTIEN = 500000m,
                LYDO = "Kỷ luật vi phạm nội quy"
            });

            var res = Calculate(penaltyInput);
            Assert.AreEqual(baseline.NetPay, res.NetPay, "Disciplinary fine must not reduce NetPay");
            Assert.AreEqual(0m, res.KhoanTruKhac);
        }

        [Test]
        public void INC_05_FixtureD_Allowance_TaxExemption_Cannot_Be_Negative()
        {
            // Fixture D: Allowance 100k, exempt limit 200k. Taxable portion must be 0 (cannot be -100k).
            var input = BaseInput();
            input.Allowances.Add(new AllowanceItemInput
            {
                IDPC = 99,
                TENPC = "Phụ cấp hỗ trợ",
                SOTIEN = 100000m,
                TINH_THUE = 1,
                SO_TIEN_MIEN_THUE = 200000m
            });

            var res = Calculate(input);
            var item = res.DetailItems.Single(x => x.MA_KHOAN_MUC == "PHUCAP_99");
            Assert.AreEqual(100000m, item.SO_TIEN_MIEN_THUE);
            Assert.AreEqual(0m, item.SO_TIEN_CHIU_THUE);
        }

        [Test]
        public void UNION_02_FixtureE_Excluded_BHXH_Employee_Generates_No_KPCD()
        {
            // Fixture E: Employee not in compulsory social insurance fund -> KPCĐ = 0đ
            var noIns = new InsuranceEngine().CalculateInsurance(
                0, 1, 202609, 26000000m,
                _insurancePolicy, _region1,
                new EmployeeInsuranceProfileDto() // flags = 0
            );

            var union = new UnionEngine().CalculateUnion(
                0, 1, 202609, noIns.LUONG_DONG_BHXH_AP_DUNG,
                _unionPolicy, new EmployeeUnionProfileDto { LA_DOAN_VIEN = 0 }, 2530000m
            );

            Assert.AreEqual(0m, noIns.LUONG_DONG_BHXH_AP_DUNG);
            Assert.AreEqual(0m, union.TIEN_KPCD_NSDLD);
            Assert.AreEqual(0m, union.TIEN_DOAN_PHI_NLD);
        }

        [Test]
        public void TIME_01_FixtureF_NightWorkPremium_2026_Is_TaxExempt()
        {
            // Fixture F: 30% night work premium under Decree 253/2026 is fully exempt from PIT
            var input = BaseInput();
            input.NightShiftDays = 1m;

            var res = Calculate(input);
            var nightItem = res.DetailItems.Single(x => x.MA_KHOAN_MUC == "PHUCAP_LAM_DEM");
            Assert.AreEqual(300000m, nightItem.THANH_TIEN);
            Assert.AreEqual(300000m, nightItem.SO_TIEN_MIEN_THUE);
            Assert.AreEqual(0m, nightItem.SO_TIEN_CHIU_THUE);
        }

        [Test]
        public void SAL_04_FixtureH_Month_With_27_Days_Capped_At_Monthly_Base()
        {
            // Fixture H: 27 days worked in a 27-day month under monthly salary -> regular salary capped at 26,000,000
            var input = BaseInput();
            input.ActualDaysWorked = 27m;

            var res = Calculate(input);
            Assert.AreEqual(26000000m, res.GrossEarnings, "Monthly salary must not inflate to 27/26 of base salary");
        }

        [Test]
        public void SAL_06_Occurrence_Deduction_100k_Reduces_NetPay_Exactly_100k_And_Sets_KhoanTruKhac()
        {
            var input = BaseInput();
            input.RewardsAndDisciplines.Add(new RewardDisciplineInput
            {
                SOQUYETDINH = "QD-KL-100K",
                LOAI = 2,
                SOTIEN = 100000m,
                LYDO = "Khấu trừ phát sinh"
            });

            var res = Calculate(input);

            // 1. KhoanTruKhac must equal 100,000
            Assert.AreEqual(100000m, res.KhoanTruKhac);

            // 2. NetPay must be reduced by exactly 100,000 (22,751,500 - 100,000 = 22,651,500)
            Assert.AreEqual(22751500m - 100000m, res.NetPay);

            // 3. DetailItems must contain the deduction under KHAU_TRU
            var item = res.DetailItems.FirstOrDefault(x => x.NHOM_KHOAN_MUC == "KHAU_TRU" && x.MA_KHOAN_MUC == "KHAU_TRU_PHAT_SINH");
            Assert.IsNotNull(item, "Detail item for deduction occurrence must exist");
            Assert.AreEqual(100000m, item.THANH_TIEN);
            Assert.AreEqual(0, item.TINH_VAO_DONG_BHXH);
            Assert.AreEqual(0, item.TINH_THUE_TNCN);
        }

        [Test]
        public void SAL_07_Occurrence_Income_100k_Increases_Gross_And_KhoanCongKhac()
        {
            var input = BaseInput();
            input.RewardsAndDisciplines.Add(new RewardDisciplineInput
            {
                SOQUYETDINH = "QD-KT-100K",
                LOAI = 1,
                SOTIEN = 100000m,
                LYDO = "Thưởng thành tích phát sinh"
            });

            var res = Calculate(input);

            // 1. Gross earnings must increase by 100,000 (26,000,000 + 100,000 = 26,100,000)
            Assert.AreEqual(26100000m, res.GrossEarnings);

            // 2. KhoanCongKhac must include 100,000
            Assert.AreEqual(100000m, res.KhoanCongKhac);

            // 3. DetailItems must contain reward under KHEN_THUONG
            var item = res.DetailItems.FirstOrDefault(x => x.NHOM_KHOAN_MUC == "KHEN_THUONG" && x.MA_KHOAN_MUC == "TIEN_KHEN_THUONG");
            Assert.IsNotNull(item, "Detail item for reward occurrence must exist");
            Assert.AreEqual(100000m, item.THANH_TIEN);
        }

        [Test]
        public void SAL_08_Occurrence_Both_Income_And_Deduction_Calculated_Without_Double_Counting()
        {
            var input = BaseInput();
            input.RewardsAndDisciplines.Add(new RewardDisciplineInput
            {
                SOQUYETDINH = "QD-KT-100K",
                LOAI = 1,
                SOTIEN = 100000m,
                LYDO = "Thưởng"
            });
            input.RewardsAndDisciplines.Add(new RewardDisciplineInput
            {
                SOQUYETDINH = "QD-KL-100K",
                LOAI = 2,
                SOTIEN = 100000m,
                LYDO = "Khấu trừ"
            });

            var res = Calculate(input);

            Assert.AreEqual(100000m, res.KhoanCongKhac);
            Assert.AreEqual(100000m, res.KhoanTruKhac);
            Assert.AreEqual(1, res.DetailItems.Count(x => x.MA_KHOAN_MUC == "TIEN_KHEN_THUONG"));
            Assert.AreEqual(1, res.DetailItems.Count(x => x.MA_KHOAN_MUC == "KHAU_TRU_PHAT_SINH"));
        }

        [Test]
        public void OT_07_Extension_Without_Notification_Marked_Unnotified()
        {
            // OT beyond 200 hours without notification filed -> UNNOTIFIED_EXTENSION
            var otEngine = new OtComplianceEngine();
            var res = otEngine.EvaluateCompliance(
                0, 1, 202609,
                actualHoursMonth: 10m,
                actualHoursYtdPrior: 195m, // Total = 205h (> 200h)
                totalOtPayment: 1500000m,
                standardHourlyRate: 100000m,
                extendedEligible: 1,
                employeeConsent: 1,
                notificationFiled: 0 // No notification filed
            );

            Assert.AreEqual("UNNOTIFIED_EXTENSION", res.COMPLIANCE_STATUS);
        }

        [Test]
        public void TAX_04_NonResident_Flat20Percent_No_Personal_Deductions()
        {
            // Non-resident flat 20% on gross taxable income
            var input = BaseInput();
            var nonResidentProfile = new EmployeeTaxProfileDto { IS_CU_TRU = 0 };

            var res = Calculate(input, taxProfile: nonResidentProfile);
            // Gross taxable = 26M. 20% flat = 5,200,000đ
            Assert.AreEqual(5200000m, res.TaxWithheld);
        }

        [Test]
        public void Phase1_24HoursPaidLeave_OnlyIncreasesPaidDaysOnce()
        {
            // Base: 26 standard days, 26,000,000 VND -> 1,000,000 VND / day
            // Case A: 20 actual work days, 0 leave days
            var inputA = BaseInput();
            inputA.ActualDaysWorked = 20m;
            inputA.LeaveDaysWithPay = 0m;
            var resA = Calculate(inputA);
            Assert.AreEqual(20000000m, resA.GrossEarnings);

            // Case B: 20 actual work days, 3 days paid leave (24 hours) => Total days = 23
            var inputB = BaseInput();
            inputB.ActualDaysWorked = 23m; // 20 worked + 3 leave
            inputB.LeaveDaysWithPay = 3m; // 3 days * 8h = 24h
            var resB = Calculate(inputB);

            // Paid days must be exactly 20 + 3 = 23 days (1,000,000 * 23 = 23,000,000 VND)
            // It must NOT double-count to 20 + 3 + 3 = 26 days
            Assert.AreEqual(23000000m, resB.GrossEarnings);

            // Standard wage should be 20,000,000 and Leave wage should be 3,000,000
            var stdWageItem = resB.DetailItems.FirstOrDefault(d => d.MA_KHOAN_MUC == "LUONG_CONG_THUCTE");
            var leaveWageItem = resB.DetailItems.FirstOrDefault(d => d.MA_KHOAN_MUC == "LUONG_NGAY_PHEP");
            Assert.IsNotNull(stdWageItem);
            Assert.IsNotNull(leaveWageItem);
            Assert.AreEqual(20000000m, stdWageItem.THANH_TIEN);
            Assert.AreEqual(3000000m, leaveWageItem.THANH_TIEN);
            Assert.AreEqual(20m, stdWageItem.SO_LUONG);
            Assert.AreEqual(3m, leaveWageItem.SO_LUONG);
        }

        [Test]
        public void Phase1_1HourUnpaidLeave_OnlyReducesSalaryOnce()
        {
            // 25 full work days + 1 day with 7h worked and 1h unpaid leave (7/8 = 0.875 công)
            // Total actual work days = 25.875 công
            var input = BaseInput();
            input.ActualDaysWorked = 25.875m;
            input.LeaveDaysWithPay = 0m;
            var res = Calculate(input);

            // Base salary = 26,000,000. Daily rate = 1,000,000. Hourly rate = 125,000.
            // 25.875 days * 1,000,000 = 25,875,000 (reduced by exactly 125,000 for 1h unpaid leave)
            Assert.AreEqual(25875000m, res.GrossEarnings);
            var stdWageItem = res.DetailItems.FirstOrDefault(d => d.MA_KHOAN_MUC == "LUONG_CONG_THUCTE");
            Assert.IsNotNull(stdWageItem);
            Assert.AreEqual(25875000m, stdWageItem.THANH_TIEN);
            Assert.AreEqual(25.875m, stdWageItem.SO_LUONG);
        }

        [Test]
        public void Phase1_DetailItems_SumEqualsGrossEarnings()
        {
            var input = BaseInput();
            input.ActualDaysWorked = 22m;
            input.LeaveDaysWithPay = 2m;
            input.NightShiftDays = 3m;
            input.Allowances.Add(new AllowanceItemInput
            {
                IDPC = 1,
                TENPC = "Phụ cấp chức vụ",
                SOTIEN = 1500000m,
                TINH_BHXH = 1,
                TINH_THUE = 1
            });
            input.Allowances.Add(new AllowanceItemInput
            {
                IDPC = 2,
                TENPC = "Phụ cấp ăn ca",
                SOTIEN = 730000m,
                TINH_BHXH = 0,
                TINH_THUE = 0,
                SO_TIEN_MIEN_THUE = 730000m
            });

            var res = Calculate(input);
            // Sum of earning detail items must match GrossEarnings exactly
            var earningItems = res.DetailItems.Where(d => d.NHOM_KHOAN_MUC == "LUONG_CHINH" || d.NHOM_KHOAN_MUC == "PHU_CAP" || d.NHOM_KHOAN_MUC == "TANG_CA" || d.NHOM_KHOAN_MUC == "KHEN_THUONG");
            decimal detailSum = earningItems.Sum(d => d.THANH_TIEN);
            Assert.AreEqual(res.GrossEarnings, detailSum);
        }

        [Test]
        public void Phase2_SalaryIncrease_HistoricalPeriodUnchanged()
        {
            // Period 2026-01: Base salary = 20,000,000
            var janInput = BaseInput();
            janInput.NAM = 2026;
            janInput.THANG = 1;
            janInput.MAKYCONG = 202601;
            janInput.BaseSalary = 20000000m;
            janInput.ActualDaysWorked = 26m;
            janInput.LeaveDaysWithPay = 0m;
            var janResult = Calculate(janInput);

            // Salary increase happens in February 2026 to 25,000,000
            // Period 2026-03: Base salary = 25,000,000
            var marInput = BaseInput();
            marInput.NAM = 2026;
            marInput.THANG = 3;
            marInput.MAKYCONG = 202603;
            marInput.BaseSalary = 25000000m;
            marInput.ActualDaysWorked = 26m;
            marInput.LeaveDaysWithPay = 0m;
            var marResult = Calculate(marInput);

            Assert.AreEqual(20000000m, janResult.GrossEarnings, "January GrossEarnings must remain 20M regardless of later increase");
            Assert.AreEqual(25000000m, marResult.GrossEarnings, "March GrossEarnings must reflect new 25M base salary");
        }

        [Test]
        [Explicit("Requires Phase 2A schema migration applied to Oracle DB")]
        public void INT_01_ExecuteFullPayrollRecalculation_Period202602_SucceedsForAllEmployees()
        {
            var engine = new PayrollEngine();
            var run = engine.ExecuteFullPayrollRecalculation(2026, 2, "UNIT_TEST");
            Assert.AreEqual("SUCCESS", run.STATUS, run.ERROR_SUMMARY);
            Assert.AreEqual(194, run.TOTAL_EMPLOYEES);
            Assert.AreEqual(194, run.SUCCESS_COUNT);
            Assert.AreEqual(0, run.ERROR_COUNT);
        }

        [Test]
        [Explicit("Requires Phase 2A schema migration applied to Oracle DB")]
        public void INT_02_ExecuteFullPayrollRecalculation_Period202601_SucceedsForAllEmployees()
        {
            var engine = new PayrollEngine();
            var run = engine.ExecuteFullPayrollRecalculation(2026, 1, "UNIT_TEST");
            Assert.AreEqual("SUCCESS", run.STATUS, run.ERROR_SUMMARY);
            Assert.AreEqual(194, run.TOTAL_EMPLOYEES);
            Assert.AreEqual(194, run.SUCCESS_COUNT);
            Assert.AreEqual(0, run.ERROR_COUNT);
        }

        private class MockPolicyResolver : IPolicyResolver
        {
            public SalaryPolicyDto GetSalaryPolicy(DateTime d) => throw new NotSupportedException();
            public InsurancePolicyDto GetInsurancePolicy(DateTime d) => throw new NotSupportedException();
            public List<InsuranceRegionDto> GetInsuranceRegions(decimal d) => throw new NotSupportedException();
            public InsuranceRegionDto GetInsuranceRegion(decimal d, int r) => throw new NotSupportedException();
            public UnionPolicyDto GetUnionPolicy(DateTime d) => throw new NotSupportedException();
            public TaxPolicyDto GetTaxPolicy(int y, DateTime d) => throw new NotSupportedException();
            public List<TaxBracketDto> GetTaxBrackets(decimal d, string p) => throw new NotSupportedException();
            public void RefreshCache() { }
        }
    }
}
