using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Bu.CLASS_PAYROLL
{
    public class EmployeePayrollInput
    {
        public decimal MANV { get; set; }
        public string HOTEN { get; set; }
        public decimal MAKYCONG { get; set; }
        public int NAM { get; set; }
        public int THANG { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal StandardDaysMonth { get; set; }
        public decimal ActualDaysWorked { get; set; }
        public decimal NightShiftDays { get; set; }
        public decimal LeaveDaysWithPay { get; set; }
        public List<AllowanceItemInput> Allowances { get; set; } = new List<AllowanceItemInput>();
        public List<OvertimeItemInput> Overtimes { get; set; } = new List<OvertimeItemInput>();
        public List<RewardDisciplineInput> RewardsAndDisciplines { get; set; } = new List<RewardDisciplineInput>();
        public List<AdvanceSalaryInput> Advances { get; set; } = new List<AdvanceSalaryInput>();
        public decimal YtdOvertimeHoursPrior { get; set; }
        public int? ExtendedOtEligible { get; set; }
        public int? EmployeeOtConsent { get; set; }
        public int? OtNotificationFiled { get; set; }
    }

    public class AllowanceItemInput
    {
        public decimal IDPC { get; set; }
        public string TENPC { get; set; }
        public decimal SOTIEN { get; set; }
        public int TINH_BHXH { get; set; }
        public int TINH_THUE { get; set; }
        public decimal? SO_TIEN_MIEN_THUE { get; set; }
        public decimal? SOURCE_NVPC_ID { get; set; }
    }

    public class OvertimeItemInput
    {
        public decimal IDTCA { get; set; }
        public decimal SOGIO { get; set; }
        public decimal HESOTC { get; set; }
        public decimal DONGIATC { get; set; }
        public decimal SOTIENTC { get; set; }
    }

    public class RewardDisciplineInput
    {
        public string SOQUYETDINH { get; set; }
        public decimal LOAI { get; set; } // 1: Reward, 2: Discipline
        public decimal SOTIEN { get; set; }
        public string LYDO { get; set; }
    }

    public class AdvanceSalaryInput
    {
        public decimal IDUL { get; set; }
        public decimal SOTIENUNG { get; set; }
    }

    public class PayrollCalculationResult
    {
        public decimal IDBL { get; set; }
        public decimal MANV { get; set; }
        public decimal MAKYCONG { get; set; }
        public decimal GrossEarnings { get; set; }
        public decimal NetPay { get; set; }
        public decimal InsuranceEmployee { get; set; }
        public decimal InsuranceEmployer { get; set; }
        public decimal UnionEmployee { get; set; }
        public decimal UnionEmployer { get; set; }
        public decimal TaxWithheld { get; set; }
        public decimal TotalEmployerCost { get; set; }
        public string ComplianceStatus { get; set; }
        public List<PayrollDetailItemDto> DetailItems { get; set; } = new List<PayrollDetailItemDto>();
        public OtComplianceSnapshotDto OtCompliance { get; set; }
        public PayrollInsuranceTraceDto InsuranceTrace { get; set; }
        public PayrollUnionTraceDto UnionTrace { get; set; }
        public List<PayrollTaxTraceDto> TaxTraces { get; set; } = new List<PayrollTaxTraceDto>();
    }

    public interface IPayrollEngine
    {
        PayrollRunDto ExecuteFullPayrollRecalculation(int nam, int thang, string executedBy);
        PayrollCalculationResult CalculateSingleEmployeePayroll(
            EmployeePayrollInput input,
            SalaryPolicyDto salaryPolicy,
            InsurancePolicyDto insurancePolicy,
            InsuranceRegionDto region,
            UnionPolicyDto unionPolicy,
            TaxPolicyDto taxPolicy,
            List<TaxBracketDto> monthTaxBrackets,
            EmployeeInsuranceProfileDto insProfile,
            EmployeeUnionProfileDto unionProfile,
            EmployeeTaxProfileDto taxProfile,
            List<DependentDto> dependents,
            decimal? runId = null
        );
    }

    public class PayrollEngine : IPayrollEngine
    {
        private readonly IPolicyResolver _policyResolver;
        private readonly IEmployeeProfileResolver _profileResolver;
        private readonly IOtComplianceEngine _otEngine;
        private readonly IInsuranceEngine _insuranceEngine;
        private readonly IUnionEngine _unionEngine;
        private readonly ITaxEngine _taxEngine;

        public PayrollEngine(
            IPolicyResolver policyResolver = null,
            IEmployeeProfileResolver profileResolver = null,
            IOtComplianceEngine otEngine = null,
            IInsuranceEngine insuranceEngine = null,
            IUnionEngine unionEngine = null,
            ITaxEngine taxEngine = null)
        {
            _policyResolver = policyResolver ?? new PolicyResolver();
            _profileResolver = profileResolver ?? new EmployeeProfileResolver();
            _otEngine = otEngine ?? new OtComplianceEngine();
            _insuranceEngine = insuranceEngine ?? new InsuranceEngine();
            _unionEngine = unionEngine ?? new UnionEngine();
            _taxEngine = taxEngine ?? new TaxEngine();
        }

        public PayrollRunDto ExecuteFullPayrollRecalculation(int nam, int thang, string executedBy)
        {
            decimal makycong = nam * 100 + thang;
            DateTime effectiveDate = new DateTime(nam, thang, DateTime.DaysInMonth(nam, thang));

            // 1. Resolve Effective-Dated Policies (Fail-Fast)
            var salaryPolicy = _policyResolver.GetSalaryPolicy(effectiveDate);
            var insurancePolicy = _policyResolver.GetInsurancePolicy(effectiveDate);
            var unionPolicy = _policyResolver.GetUnionPolicy(effectiveDate);
            var taxPolicy = _policyResolver.GetTaxPolicy(nam, effectiveDate);
            var monthTaxBrackets = _policyResolver.GetTaxBrackets(taxPolicy.ID, "MONTH");

            using (var db = new MyEntities())
            {
                // Create or find Payroll Calculation Run
                decimal runId = db.Database.SqlQuery<decimal>("SELECT SEQ_PAYROLL_CALC_RUN.NEXTVAL FROM DUAL").First();
                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_PAYROLL_CALCULATION_RUN (
                        RUN_ID, MAKYCONG, NAM, THANG, EXECUTION_TYPE, STATUS, STARTED_AT, EXECUTED_BY
                    ) VALUES (
                        :p0, :p1, :p2, :p3, 'FULL_RECALC', 'IN_PROGRESS', SYSTIMESTAMP, :p4
                    )",
                    new OracleParameter("p0", runId),
                    new OracleParameter("p1", makycong),
                    new OracleParameter("p2", nam),
                    new OracleParameter("p3", thang),
                    new OracleParameter("p4", executedBy ?? "SYSTEM")
                );

                // Load all active employees in period
                var employees = db.Database.SqlQuery<EmployeeAttendanceRow>(@"
                    SELECT nv.MANV, nv.HOTEN, hd.HESOLUONG AS BASE_SALARY,
                           nv.IDPB, nv.IDCV
                    FROM TB_NHANVIEN nv
                    LEFT JOIN TB_HOPDONG hd ON nv.MANV = hd.MANV
                    WHERE (nv.DATHOIVIEC = 0 OR nv.DATHOIVIEC IS NULL)
                    ORDER BY nv.MANV"
                ).ToList();

                int successCount = 0;
                int warningCount = 0;
                int errorCount = 0;

                foreach (var emp in employees)
                {
                    try
                    {
                        // Check if existing record is legacy (IDBL = 1934 immutability protection)
                        var existingLegacy = db.Database.SqlQuery<int?>(@"
                            SELECT IS_LEGACY FROM TB_BANGLUONG
                            WHERE MANV = :p0 AND MAKYCONG = :p1",
                            new OracleParameter("p0", emp.MANV),
                            new OracleParameter("p1", makycong)
                        ).FirstOrDefault();

                        if (existingLegacy.HasValue && existingLegacy.Value == 1)
                        {
                            // Immutable legacy snapshot, strictly preserved
                            continue;
                        }

                        // Build input
                        var input = LoadEmployeePayrollInput(db, emp.MANV, makycong, nam, thang, emp.BASE_SALARY ?? 10000000m, salaryPolicy.SO_CONG_CHUAN_THANG);

                        // Resolve Profiles
                        var insProfile = _profileResolver.GetOrProvisionInsuranceProfile(emp.MANV, effectiveDate);
                        var unionProfile = _profileResolver.GetOrProvisionUnionProfile(emp.MANV, effectiveDate);
                        var taxProfile = _profileResolver.GetOrProvisionTaxProfile(emp.MANV, effectiveDate);
                        var dependents = _profileResolver.GetActiveDependents(emp.MANV, (int)makycong);
                        var region = _policyResolver.GetInsuranceRegion(insurancePolicy.ID, insProfile.VUNG_LUONG);

                        // Calculate Single
                        var result = CalculateSingleEmployeePayroll(
                            input,
                            salaryPolicy,
                            insurancePolicy,
                            region,
                            unionPolicy,
                            taxPolicy,
                            monthTaxBrackets,
                            insProfile,
                            unionProfile,
                            taxProfile,
                            dependents,
                            runId
                        );

                        // Persist to Database
                        SaveCalculatedPayroll(db, result, salaryPolicy, insurancePolicy, unionPolicy, taxPolicy, insProfile, unionProfile, taxProfile, dependents.Count, runId);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        Console.WriteLine($"Error calculating payroll for MANV {emp.MANV}: {ex.Message}");
                    }
                }

                // Update run status
                db.Database.ExecuteSqlCommand(@"
                    UPDATE TB_PAYROLL_CALCULATION_RUN
                    SET TOTAL_EMPLOYEES = :p0, SUCCESS_COUNT = :p1, WARNING_COUNT = :p2, ERROR_COUNT = :p3,
                        STATUS = :p4, FINISHED_AT = SYSTIMESTAMP
                    WHERE RUN_ID = :p5",
                    new OracleParameter("p0", employees.Count),
                    new OracleParameter("p1", successCount),
                    new OracleParameter("p2", warningCount),
                    new OracleParameter("p3", errorCount),
                    new OracleParameter("p4", errorCount == 0 ? "SUCCESS" : "PARTIAL_SUCCESS"),
                    new OracleParameter("p5", runId)
                );

                return new PayrollRunDto
                {
                    RUN_ID = runId,
                    MAKYCONG = makycong,
                    NAM = nam,
                    THANG = thang,
                    EXECUTION_TYPE = "FULL_RECALC",
                    TOTAL_EMPLOYEES = employees.Count,
                    SUCCESS_COUNT = successCount,
                    WARNING_COUNT = warningCount,
                    ERROR_COUNT = errorCount,
                    STATUS = errorCount == 0 ? "SUCCESS" : "PARTIAL_SUCCESS",
                    STARTED_AT = DateTime.Now,
                    EXECUTED_BY = executedBy
                };
            }
        }

        public PayrollCalculationResult CalculateSingleEmployeePayroll(
            EmployeePayrollInput input,
            SalaryPolicyDto salaryPolicy,
            InsurancePolicyDto insurancePolicy,
            InsuranceRegionDto region,
            UnionPolicyDto unionPolicy,
            TaxPolicyDto taxPolicy,
            List<TaxBracketDto> monthTaxBrackets,
            EmployeeInsuranceProfileDto insProfile,
            EmployeeUnionProfileDto unionProfile,
            EmployeeTaxProfileDto taxProfile,
            List<DependentDto> dependents,
            decimal? runId = null)
        {
            var res = new PayrollCalculationResult
            {
                MANV = input.MANV,
                MAKYCONG = input.MAKYCONG
            };

            // 1. Standard Working Days & Daily Rates
            decimal dailyRate = Math.Round(input.BaseSalary / salaryPolicy.SO_CONG_CHUAN_THANG, 2);
            decimal standardWage = Math.Round(input.ActualDaysWorked * dailyRate, 2);
            decimal hourlyRate = Math.Round(dailyRate / salaryPolicy.SO_GIO_CHUAN_NGAY, 2);
            decimal nightWageAllowance = Math.Round(input.NightShiftDays * dailyRate * salaryPolicy.HE_SO_LAM_DEM, 2);

            // Item: Standard Wage
            res.DetailItems.Add(new PayrollDetailItemDto
            {
                MANV = input.MANV,
                MAKYCONG = input.MAKYCONG,
                NHOM_KHOAN_MUC = "LUONG_CHINH",
                MA_KHOAN_MUC = "LUONG_CONG_THUCTE",
                TEN_KHOAN_MUC = "Lương công nhật thực tế",
                SO_LUONG = input.ActualDaysWorked,
                DON_GIA = dailyRate,
                HE_SO = 1.0m,
                THANH_TIEN = standardWage,
                TINH_VAO_DONG_BHXH = 1,
                TINH_THUE_TNCN = 1,
                SO_TIEN_MIEN_THUE = 0m,
                SO_TIEN_CHIU_THUE = standardWage,
                CONG_THUC_DIEN_GIAI = $"Số công ({input.ActualDaysWorked}) * Đơn giá ngày ({dailyRate:N0})"
            });

            // Item: Night shift allowance
            if (nightWageAllowance > 0)
            {
                res.DetailItems.Add(new PayrollDetailItemDto
                {
                    MANV = input.MANV,
                    MAKYCONG = input.MAKYCONG,
                    NHOM_KHOAN_MUC = "PHU_CAP",
                    MA_KHOAN_MUC = "PHUCAP_LAM_DEM",
                    TEN_KHOAN_MUC = "Phụ cấp làm việc ban đêm (30%)",
                    SO_LUONG = input.NightShiftDays,
                    DON_GIA = dailyRate * salaryPolicy.HE_SO_LAM_DEM,
                    HE_SO = salaryPolicy.HE_SO_LAM_DEM,
                    THANH_TIEN = nightWageAllowance,
                    TINH_VAO_DONG_BHXH = 0,
                    TINH_THUE_TNCN = 1,
                    SO_TIEN_MIEN_THUE = 0m,
                    SO_TIEN_CHIU_THUE = nightWageAllowance,
                    CONG_THUC_DIEN_GIAI = $"Số công đêm ({input.NightShiftDays}) * 30% lương ngày"
                });
            }

            // 2. Allowances
            decimal totalAllowances = 0m;
            decimal totalTaxableAllowances = 0m;
            decimal totalInsuranceAllowances = 0m;

            foreach (var pc in input.Allowances)
            {
                totalAllowances += pc.SOTIEN;
                if (pc.TINH_BHXH == 1) totalInsuranceAllowances += pc.SOTIEN;

                decimal mienThue = pc.SO_TIEN_MIEN_THUE ?? (pc.TINH_THUE == 0 ? pc.SOTIEN : 0m);
                decimal chiuThue = pc.SOTIEN - mienThue;
                totalTaxableAllowances += chiuThue;

                var pcItem = new PayrollDetailItemDto
                {
                    MANV = input.MANV,
                    MAKYCONG = input.MAKYCONG,
                    NHOM_KHOAN_MUC = "PHU_CAP",
                    MA_KHOAN_MUC = $"PHUCAP_{pc.IDPC}",
                    TEN_KHOAN_MUC = pc.TENPC,
                    SO_LUONG = 1.0m,
                    DON_GIA = pc.SOTIEN,
                    HE_SO = 1.0m,
                    THANH_TIEN = pc.SOTIEN,
                    TINH_VAO_DONG_BHXH = pc.TINH_BHXH,
                    TINH_THUE_TNCN = pc.TINH_THUE,
                    SO_TIEN_MIEN_THUE = mienThue,
                    SO_TIEN_CHIU_THUE = chiuThue,
                    CONG_THUC_DIEN_GIAI = $"Phụ cấp hợp đồng: {pc.TENPC}"
                };

                if (pc.SOURCE_NVPC_ID.HasValue)
                {
                    pcItem.Sources.Add(new PayrollDetailSourceDto
                    {
                        MANV = input.MANV,
                        SOURCE_TYPE = "PHUCAP",
                        SOURCE_NVPC_ID = pc.IDPC,
                        WEIGHT_QUANTITY = 1.0m,
                        AMOUNT_CONTRIBUTED = pc.SOTIEN
                    });
                }
                res.DetailItems.Add(pcItem);
            }

            // 3. Overtime & OT Compliance Engine
            decimal totalOtHours = input.Overtimes.Sum(o => o.SOGIO);
            decimal totalOtPayment = input.Overtimes.Sum(o => o.SOTIENTC);

            var otCompliance = _otEngine.EvaluateCompliance(
                0, input.MANV, input.MAKYCONG, totalOtHours, input.YtdOvertimeHoursPrior,
                totalOtPayment, hourlyRate, input.ExtendedOtEligible, input.EmployeeOtConsent, input.OtNotificationFiled
            );
            res.OtCompliance = otCompliance;
            res.ComplianceStatus = otCompliance.COMPLIANCE_STATUS;

            if (totalOtPayment > 0)
            {
                var otItem = new PayrollDetailItemDto
                {
                    MANV = input.MANV,
                    MAKYCONG = input.MAKYCONG,
                    NHOM_KHOAN_MUC = "TANG_CA",
                    MA_KHOAN_MUC = "TIEN_TANG_CA",
                    TEN_KHOAN_MUC = "Tiền làm thêm giờ (Overtime)",
                    SO_LUONG = totalOtHours,
                    DON_GIA = hourlyRate,
                    HE_SO = totalOtHours > 0 ? Math.Round(totalOtPayment / (totalOtHours * hourlyRate), 2) : 1.5m,
                    THANH_TIEN = totalOtPayment,
                    TINH_VAO_DONG_BHXH = 0,
                    TINH_THUE_TNCN = 1,
                    SO_TIEN_MIEN_THUE = otCompliance.EXEMPT_OT_PAYMENT,
                    SO_TIEN_CHIU_THUE = otCompliance.TAXABLE_EXCESS_PAYMENT,
                    CONG_THUC_DIEN_GIAI = $"Tổng {totalOtHours} giờ OT (Miễn thuế: {otCompliance.EXEMPT_OT_PAYMENT:N0}, Chịu thuế: {otCompliance.TAXABLE_EXCESS_PAYMENT:N0})"
                };

                foreach (var ot in input.Overtimes)
                {
                    otItem.Sources.Add(new PayrollDetailSourceDto
                    {
                        MANV = input.MANV,
                        SOURCE_TYPE = "TANGCA",
                        SOURCE_TC_ID = ot.IDTCA,
                        WEIGHT_QUANTITY = ot.SOGIO,
                        AMOUNT_CONTRIBUTED = ot.SOTIENTC
                    });
                }
                res.DetailItems.Add(otItem);
            }

            // 4. Rewards and Penalties
            decimal totalRewards = input.RewardsAndDisciplines.Where(r => r.LOAI == 1).Sum(r => r.SOTIEN);
            decimal totalDisciplines = input.RewardsAndDisciplines.Where(r => r.LOAI == 2).Sum(r => r.SOTIEN);

            if (totalRewards > 0)
            {
                var rewardItem = new PayrollDetailItemDto
                {
                    MANV = input.MANV,
                    MAKYCONG = input.MAKYCONG,
                    NHOM_KHOAN_MUC = "KHEN_THUONG",
                    MA_KHOAN_MUC = "TIEN_KHEN_THUONG",
                    TEN_KHOAN_MUC = "Tiền khen thưởng",
                    SO_LUONG = 1.0m,
                    DON_GIA = totalRewards,
                    HE_SO = 1.0m,
                    THANH_TIEN = totalRewards,
                    TINH_VAO_DONG_BHXH = 0,
                    TINH_THUE_TNCN = 1,
                    SO_TIEN_MIEN_THUE = 0m,
                    SO_TIEN_CHIU_THUE = totalRewards,
                    CONG_THUC_DIEN_GIAI = "Khen thưởng theo quyết định"
                };
                foreach (var r in input.RewardsAndDisciplines.Where(x => x.LOAI == 1))
                {
                    rewardItem.Sources.Add(new PayrollDetailSourceDto
                    {
                        MANV = input.MANV,
                        SOURCE_TYPE = "KHENTHUONG_KYLUAT",
                        SOURCE_KTKL_SOQD = r.SOQUYETDINH,
                        WEIGHT_QUANTITY = 1.0m,
                        AMOUNT_CONTRIBUTED = r.SOTIEN
                    });
                }
                res.DetailItems.Add(rewardItem);
            }

            // 5. Total Gross Earnings
            res.GrossEarnings = standardWage + nightWageAllowance + totalAllowances + totalOtPayment + totalRewards;

            // 6. Insurance Calculation
            decimal contractualInsuranceBase = input.BaseSalary + totalInsuranceAllowances;
            var insTrace = _insuranceEngine.CalculateInsurance(0, input.MANV, input.MAKYCONG, contractualInsuranceBase, insurancePolicy, region, insProfile);
            res.InsuranceTrace = insTrace;
            res.InsuranceEmployee = insTrace.TONG_BH_NLD;
            res.InsuranceEmployer = insTrace.TONG_BH_NSDLD;

            // 7. Union Fee Calculation
            var unionTrace = _unionEngine.CalculateUnion(0, input.MANV, input.MAKYCONG, insTrace.LUONG_DONG_BHXH_AP_DUNG, unionPolicy, unionProfile, insurancePolicy.MUC_THAM_CHIEU);
            res.UnionTrace = unionTrace;
            res.UnionEmployee = unionTrace.TIEN_DOAN_PHI_NLD;
            res.UnionEmployer = unionTrace.TIEN_KPCD_NSDLD;

            // 8. Tax Calculation
            decimal grossTaxable = standardWage + nightWageAllowance + totalTaxableAllowances + (otCompliance.TAXABLE_EXCESS_PAYMENT ?? 0m) + totalRewards;
            var taxCalc = _taxEngine.CalculateMonthlyTax(
                0, input.MANV, input.MAKYCONG, grossTaxable, dependents.Count, insTrace.TONG_BH_NLD, taxPolicy, monthTaxBrackets, taxProfile.IS_CU_TRU == 1
            );
            res.TaxTraces = taxCalc.BracketTraces;
            res.TaxWithheld = taxCalc.TotalTax;

            // 9. Advances & Disciplinary Deductions
            decimal totalAdvances = input.Advances.Sum(a => a.SOTIENUNG);

            // 10. Net Pay
            res.NetPay = res.GrossEarnings - res.InsuranceEmployee - res.UnionEmployee - res.TaxWithheld - totalAdvances - totalDisciplines;

            // 11. Total Employer Cost
            res.TotalEmployerCost = res.GrossEarnings + res.InsuranceEmployer + res.UnionEmployer;

            return res;
        }

        private static EmployeePayrollInput LoadEmployeePayrollInput(MyEntities db, decimal manv, decimal makycong, int nam, int thang, decimal baseSalary, decimal standardDays)
        {
            var input = new EmployeePayrollInput
            {
                MANV = manv,
                MAKYCONG = makycong,
                NAM = nam,
                THANG = thang,
                BaseSalary = baseSalary,
                StandardDaysMonth = standardDays
            };

            // 1. Attendance summary from TB_KYCONGCHITIET and TB_BANGCONG_CHITIET
            var kcct = db.Database.SqlQuery<AttendanceSummaryRow>(@"
                SELECT TONGNGAYCONG, NGAYPHEP, CONGCHUNHAT
                FROM TB_KYCONGCHITIET
                WHERE MANV = :p0 AND MAKYCONG = :p1",
                new OracleParameter("p0", manv),
                new OracleParameter("p1", makycong)
            ).FirstOrDefault();

            decimal actualDays = 0m;
            decimal leaveDays = 0m;

            if (kcct != null && kcct.TONGNGAYCONG.HasValue)
            {
                actualDays = kcct.TONGNGAYCONG.Value;
                leaveDays = kcct.NGAYPHEP ?? 0.0m;
            }
            else
            {
                // Fallback to TB_BANGCONG_CHITIET directly
                var bcctSummary = db.Database.SqlQuery<AttendanceSummaryRow>(@"
                    SELECT SUM(NVL(NGAYCONG, 0)) AS TONGNGAYCONG,
                           SUM(NVL(NGAYPHEP, 0)) AS NGAYPHEP,
                           SUM(NVL(CONGCHUNHAT, 0)) AS CONGCHUNHAT
                    FROM TB_BANGCONG_CHITIET
                    WHERE MANV = :p0 AND MAKYCONG = :p1",
                    new OracleParameter("p0", manv),
                    new OracleParameter("p1", makycong)
                ).FirstOrDefault();

                if (bcctSummary != null)
                {
                    actualDays = bcctSummary.TONGNGAYCONG ?? 0m;
                    leaveDays = bcctSummary.NGAYPHEP ?? 0m;
                }
            }

            // Count night shifts from TB_BANGCONG_CHITIET (ca đêm: ký hiệu CD, Đ, XĐ hoặc giờ vào ban đêm)
            var bcctNight = db.Database.SqlQuery<decimal?>(@"
                SELECT SUM(NVL(NGAYCONG, 0))
                FROM TB_BANGCONG_CHITIET
                WHERE MANV = :p0 AND MAKYCONG = :p1
                  AND (KYHIEU IN ('CD', 'Đ', 'XĐ') OR GIOVAO >= '18:00' OR (GIOVAO IS NOT NULL AND GIOVAO < '06:00'))",
                new OracleParameter("p0", manv),
                new OracleParameter("p1", makycong)
            ).FirstOrDefault();

            input.ActualDaysWorked = actualDays;
            input.LeaveDaysWithPay = leaveDays;
            input.NightShiftDays = bcctNight ?? 0.0m;

            // Allowances from TB_NHANVIEN_PHUCAP
            var pcs = db.Database.SqlQuery<AllowanceQueryRow>(@"
                SELECT np.IDPC, p.TENPC, np.SOTIEN
                FROM TB_NHANVIEN_PHUCAP np
                JOIN TB_PHUCAP p ON np.IDPC = p.IDPC
                WHERE np.MANV = :p0 AND np.SOTIEN > 0",
                new OracleParameter("p0", manv)
            ).ToList();

            foreach (var p in pcs)
            {
                input.Allowances.Add(new AllowanceItemInput
                {
                    IDPC = p.IDPC,
                    TENPC = p.TENPC,
                    SOTIEN = p.SOTIEN,
                    TINH_BHXH = (p.IDPC == 1 || p.IDPC == 2 || p.IDPC == 5 || p.IDPC == 7) ? 1 : 0,
                    TINH_THUE = (p.IDPC == 2 || p.IDPC == 9) ? 0 : 1, // Đi lại & ăn ca miễn thuế theo quy chế
                    SOURCE_NVPC_ID = p.IDPC
                });
            }

            // Overtime from TB_TANGCA
            var tcs = db.Database.SqlQuery<OvertimeQueryRow>(@"
                SELECT IDTCA, SOGIO, HESOTC, DONGIATC, SOTIENTC
                FROM TB_TANGCA
                WHERE MANV = :p0 AND NAM = :p1 AND THANG = :p2",
                new OracleParameter("p0", manv),
                new OracleParameter("p1", nam),
                new OracleParameter("p2", thang)
            ).ToList();

            foreach (var t in tcs)
            {
                input.Overtimes.Add(new OvertimeItemInput
                {
                    IDTCA = t.IDTCA,
                    SOGIO = t.SOGIO ?? 0m,
                    HESOTC = t.HESOTC ?? 1.5m,
                    DONGIATC = t.DONGIATC ?? 0m,
                    SOTIENTC = t.SOTIENTC ?? 0m
                });
            }

            // Advances from TB_UNGLUONG
            var uls = db.Database.SqlQuery<AdvanceQueryRow>(@"
                SELECT IDUL, SOTIENUNG
                FROM TB_UNGLUONG
                WHERE MANV = :p0 AND NAM = :p1 AND THANG = :p2",
                new OracleParameter("p0", manv),
                new OracleParameter("p1", nam),
                new OracleParameter("p2", thang)
            ).ToList();

            foreach (var u in uls)
            {
                input.Advances.Add(new AdvanceSalaryInput
                {
                    IDUL = u.IDUL,
                    SOTIENUNG = u.SOTIENUNG ?? 0m
                });
            }

            return input;
        }

        private static void SaveCalculatedPayroll(
            MyEntities db,
            PayrollCalculationResult res,
            SalaryPolicyDto salaryPolicy,
            InsurancePolicyDto insurancePolicy,
            UnionPolicyDto unionPolicy,
            TaxPolicyDto taxPolicy,
            EmployeeInsuranceProfileDto insProfile,
            EmployeeUnionProfileDto unionProfile,
            EmployeeTaxProfileDto taxProfile,
            int dependentCount,
            decimal? runId)
        {
            // 1. Delete previous calculated details for employee in period if not legacy
            var existingBl = db.Database.SqlQuery<ExistingBlRow>(@"
                SELECT IDBL, IS_LEGACY FROM TB_BANGLUONG
                WHERE MANV = :p0 AND MAKYCONG = :p1",
                new OracleParameter("p0", res.MANV),
                new OracleParameter("p1", res.MAKYCONG)
            ).FirstOrDefault();

            decimal idbl;

            if (existingBl != null)
            {
                idbl = existingBl.IDBL;
                res.IDBL = idbl;

                // Clean child tables before rewriting
                db.Database.ExecuteSqlCommand("DELETE FROM TB_BANGLUONG_THUE_CT WHERE IDBL = :p0", new OracleParameter("p0", idbl));
                db.Database.ExecuteSqlCommand("DELETE FROM TB_BANGLUONG_CONG_DOAN WHERE IDBL = :p0", new OracleParameter("p0", idbl));
                db.Database.ExecuteSqlCommand("DELETE FROM TB_BANGLUONG_BAOHIEM WHERE IDBL = :p0", new OracleParameter("p0", idbl));
                db.Database.ExecuteSqlCommand("DELETE FROM TB_BANGLUONG_OT_COMPLIANCE WHERE IDBL = :p0", new OracleParameter("p0", idbl));
                db.Database.ExecuteSqlCommand("DELETE FROM TB_BANGLUONG_CT_SOURCE WHERE IDBLCT IN (SELECT IDBLCT FROM TB_BANGLUONG_CT WHERE IDBL = :p0)", new OracleParameter("p0", idbl));
                db.Database.ExecuteSqlCommand("DELETE FROM TB_BANGLUONG_CT WHERE IDBL = :p0", new OracleParameter("p0", idbl));

                // Update master TB_BANGLUONG
                db.Database.ExecuteSqlCommand(@"
                    UPDATE TB_BANGLUONG
                    SET THUC_LINH = :p0,
                        TIEN_BHXH_TRICH = :p1,
                        THUE_TNCN = :p2,
                        IS_LEGACY = 0,
                        TRANG_THAI = 'CALCULATED',
                        POLICY_LUONG_ID = :p3,
                        POLICY_BHXH_ID = :p4,
                        POLICY_CONGDOAN_ID = :p5,
                        POLICY_THUE_ID = :p6,
                        PROFILE_BH_ID = :p7,
                        PROFILE_CD_ID = :p8,
                        PROFILE_THUE_ID = :p9,
                        VUNG_LUONG = :p10,
                        LUONG_TOI_THIEU_VUNG = :p11,
                        MUC_THAM_CHIEU_BH = :p12,
                        LUONG_DONG_BHXH = :p13,
                        TIEN_BHYT_NLD = :p14,
                        TIEN_BHTN_NLD = :p15,
                        TIEN_BHXH_NSDLD = :p16,
                        TIEN_BHYT_NSDLD = :p17,
                        TIEN_BHTN_NSDLD = :p18,
                        TIEN_TNLD_BNN_NSDLD = :p19,
                        TIEN_DOAN_PHI_NLD = :p20,
                        TIEN_KINH_PHI_CD_NSDLD = :p21,
                        SO_NGUOI_PHU_THUOC = :p22,
                        GIAM_TRU_BAN_THAN = :p23,
                        GIAM_TRU_PHU_THUOC = :p24,
                        GIAM_TRU_BAO_HIEM = :p25,
                        TONG_THU_NHAP_CHIU_THUE = :p26,
                        THU_NHAP_TINH_THUE = :p27,
                        TONG_CHI_PHI_NSDLD = :p28,
                        RUN_ID = :p29,
                        CALCULATED_AT = SYSTIMESTAMP
                    WHERE IDBL = :p30",
                    new OracleParameter("p0", res.NetPay),
                    new OracleParameter("p1", res.InsuranceEmployee),
                    new OracleParameter("p2", res.TaxWithheld),
                    new OracleParameter("p3", salaryPolicy.ID),
                    new OracleParameter("p4", insurancePolicy.ID),
                    new OracleParameter("p5", unionPolicy.ID),
                    new OracleParameter("p6", taxPolicy.ID),
                    new OracleParameter("p7", insProfile.ID),
                    new OracleParameter("p8", unionProfile.ID),
                    new OracleParameter("p9", taxProfile.ID),
                    new OracleParameter("p10", insProfile.VUNG_LUONG),
                    new OracleParameter("p11", res.InsuranceTrace.LUONG_TOI_THIEU_VUNG),
                    new OracleParameter("p12", insurancePolicy.MUC_THAM_CHIEU),
                    new OracleParameter("p13", res.InsuranceTrace.LUONG_DONG_BHXH_AP_DUNG),
                    new OracleParameter("p14", res.InsuranceTrace.TIEN_BHYT_NLD),
                    new OracleParameter("p15", res.InsuranceTrace.TIEN_BHTN_NLD),
                    new OracleParameter("p16", res.InsuranceTrace.TIEN_BHXH_NSDLD),
                    new OracleParameter("p17", res.InsuranceTrace.TIEN_BHYT_NSDLD),
                    new OracleParameter("p18", res.InsuranceTrace.TIEN_BHTN_NSDLD),
                    new OracleParameter("p19", res.InsuranceTrace.TIEN_TNLD_BNN_NSDLD),
                    new OracleParameter("p20", res.UnionEmployee),
                    new OracleParameter("p21", res.UnionEmployer),
                    new OracleParameter("p22", dependentCount),
                    new OracleParameter("p23", taxPolicy.GIAM_TRU_BAN_THAN_THANG),
                    new OracleParameter("p24", dependentCount * taxPolicy.GIAM_TRU_PHU_THUOC_THANG),
                    new OracleParameter("p25", res.InsuranceEmployee),
                    new OracleParameter("p26", res.GrossEarnings),
                    new OracleParameter("p27", Math.Max(0m, res.GrossEarnings - res.InsuranceEmployee - taxPolicy.GIAM_TRU_BAN_THAN_THANG - (dependentCount * taxPolicy.GIAM_TRU_PHU_THUOC_THANG))),
                    new OracleParameter("p28", res.TotalEmployerCost),
                    new OracleParameter("p29", (object)runId ?? DBNull.Value),
                    new OracleParameter("p30", idbl)
                );
            }
            else
            {
                // Insert new TB_BANGLUONG row
                // For new IDBL, compute next value from MAX(IDBL) + 1 or sequence
                decimal nextIdbl = db.Database.SqlQuery<decimal>("SELECT NVL(MAX(IDBL), 0) + 1 FROM TB_BANGLUONG").First();
                idbl = nextIdbl;
                res.IDBL = idbl;

                int thang = (int)(res.MAKYCONG % 100);
                int nam = (int)(res.MAKYCONG / 100);

                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_BANGLUONG (
                        IDBL, MANV, MAKYCONG, THANG, NAM, THUC_LINH, TIEN_BHXH_TRICH, THUE_TNCN,
                        IS_LEGACY, TRANG_THAI, POLICY_LUONG_ID, POLICY_BHXH_ID, POLICY_CONGDOAN_ID, POLICY_THUE_ID,
                        PROFILE_BH_ID, PROFILE_CD_ID, PROFILE_THUE_ID, VUNG_LUONG, LUONG_TOI_THIEU_VUNG,
                        MUC_THAM_CHIEU_BH, LUONG_DONG_BHXH, TIEN_BHYT_NLD, TIEN_BHTN_NLD,
                        TIEN_BHXH_NSDLD, TIEN_BHYT_NSDLD, TIEN_BHTN_NSDLD, TIEN_TNLD_BNN_NSDLD,
                        TIEN_DOAN_PHI_NLD, TIEN_KINH_PHI_CD_NSDLD, SO_NGUOI_PHU_THUOC,
                        GIAM_TRU_BAN_THAN, GIAM_TRU_PHU_THUOC, GIAM_TRU_BAO_HIEM,
                        TONG_THU_NHAP_CHIU_THUE, THU_NHAP_TINH_THUE, TONG_CHI_PHI_NSDLD,
                        RUN_ID, CALCULATED_AT
                    ) VALUES (
                        :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7,
                        0, 'CALCULATED', :p8, :p9, :p10, :p11,
                        :p12, :p13, :p14, :p15, :p16,
                        :p17, :p18, :p19, :p20,
                        :p21, :p22, :p23, :p24,
                        :p25, :p26, :p27,
                        :p28, :p29, :p30,
                        :p31, :p32, :p33,
                        :p34, SYSTIMESTAMP
                    )",
                    new OracleParameter("p0", idbl),
                    new OracleParameter("p1", res.MANV),
                    new OracleParameter("p2", res.MAKYCONG),
                    new OracleParameter("p3", thang),
                    new OracleParameter("p4", nam),
                    new OracleParameter("p5", res.NetPay),
                    new OracleParameter("p6", res.InsuranceEmployee),
                    new OracleParameter("p7", res.TaxWithheld),
                    new OracleParameter("p8", salaryPolicy.ID),
                    new OracleParameter("p9", insurancePolicy.ID),
                    new OracleParameter("p10", unionPolicy.ID),
                    new OracleParameter("p11", taxPolicy.ID),
                    new OracleParameter("p12", insProfile.ID),
                    new OracleParameter("p13", unionProfile.ID),
                    new OracleParameter("p14", taxProfile.ID),
                    new OracleParameter("p15", insProfile.VUNG_LUONG),
                    new OracleParameter("p16", res.InsuranceTrace.LUONG_TOI_THIEU_VUNG),
                    new OracleParameter("p17", insurancePolicy.MUC_THAM_CHIEU),
                    new OracleParameter("p18", res.InsuranceTrace.LUONG_DONG_BHXH_AP_DUNG),
                    new OracleParameter("p19", res.InsuranceTrace.TIEN_BHYT_NLD),
                    new OracleParameter("p20", res.InsuranceTrace.TIEN_BHTN_NLD),
                    new OracleParameter("p21", res.InsuranceTrace.TIEN_BHXH_NSDLD),
                    new OracleParameter("p22", res.InsuranceTrace.TIEN_BHYT_NSDLD),
                    new OracleParameter("p23", res.InsuranceTrace.TIEN_BHTN_NSDLD),
                    new OracleParameter("p24", res.InsuranceTrace.TIEN_TNLD_BNN_NSDLD),
                    new OracleParameter("p25", res.UnionEmployee),
                    new OracleParameter("p26", res.UnionEmployer),
                    new OracleParameter("p27", dependentCount),
                    new OracleParameter("p28", taxPolicy.GIAM_TRU_BAN_THAN_THANG),
                    new OracleParameter("p29", dependentCount * taxPolicy.GIAM_TRU_PHU_THUOC_THANG),
                    new OracleParameter("p30", res.InsuranceEmployee),
                    new OracleParameter("p31", res.GrossEarnings),
                    new OracleParameter("p32", Math.Max(0m, res.GrossEarnings - res.InsuranceEmployee - taxPolicy.GIAM_TRU_BAN_THAN_THANG - (dependentCount * taxPolicy.GIAM_TRU_PHU_THUOC_THANG))),
                    new OracleParameter("p33", res.TotalEmployerCost),
                    new OracleParameter("p34", (object)runId ?? DBNull.Value)
                );
            }

            // 2. Save Itemized Details (TB_BANGLUONG_CT) and Sources
            foreach (var item in res.DetailItems)
            {
                decimal idblct = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_CT.NEXTVAL FROM DUAL").First();
                item.IDBLCT = idblct;
                item.IDBL = idbl;

                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_BANGLUONG_CT (
                        IDBLCT, IDBL, MANV, MAKYCONG, NHOM_KHOAN_MUC, MA_KHOAN_MUC, TEN_KHOAN_MUC,
                        SO_LUONG, DON_GIA, HE_SO, THANH_TIEN, TINH_VAO_DONG_BHXH, TINH_THUE_TNCN,
                        SO_TIEN_MIEN_THUE, SO_TIEN_CHIU_THUE, CONG_THUC_DIEN_GIAI, IS_LEGACY, CREATED_AT
                    ) VALUES (
                        :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, :p10, :p11, :p12, :p13, :p14, :p15, 0, SYSTIMESTAMP
                    )",
                    new OracleParameter("p0", idblct),
                    new OracleParameter("p1", idbl),
                    new OracleParameter("p2", res.MANV),
                    new OracleParameter("p3", res.MAKYCONG),
                    new OracleParameter("p4", item.NHOM_KHOAN_MUC),
                    new OracleParameter("p5", item.MA_KHOAN_MUC),
                    new OracleParameter("p6", item.TEN_KHOAN_MUC),
                    new OracleParameter("p7", item.SO_LUONG),
                    new OracleParameter("p8", item.DON_GIA),
                    new OracleParameter("p9", item.HE_SO),
                    new OracleParameter("p10", item.THANH_TIEN),
                    new OracleParameter("p11", item.TINH_VAO_DONG_BHXH),
                    new OracleParameter("p12", item.TINH_THUE_TNCN),
                    new OracleParameter("p13", (object)item.SO_TIEN_MIEN_THUE ?? DBNull.Value),
                    new OracleParameter("p14", (object)item.SO_TIEN_CHIU_THUE ?? DBNull.Value),
                    new OracleParameter("p15", (object)item.CONG_THUC_DIEN_GIAI ?? DBNull.Value)
                );

                foreach (var src in item.Sources)
                {
                    decimal srcId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_CT_SOURCE.NEXTVAL FROM DUAL").First();
                    db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_BANGLUONG_CT_SOURCE (
                            ID, IDBLCT, MANV, SOURCE_TYPE, SOURCE_BCCT_ID, SOURCE_TC_ID,
                            SOURCE_NVPC_ID, SOURCE_KTKL_SOQD, SOURCE_UL_ID, WEIGHT_QUANTITY,
                            AMOUNT_CONTRIBUTED, CREATED_AT
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, :p10, SYSTIMESTAMP
                        )",
                        new OracleParameter("p0", srcId),
                        new OracleParameter("p1", idblct),
                        new OracleParameter("p2", res.MANV),
                        new OracleParameter("p3", src.SOURCE_TYPE),
                        new OracleParameter("p4", (object)src.SOURCE_BCCT_ID ?? DBNull.Value),
                        new OracleParameter("p5", (object)src.SOURCE_TC_ID ?? DBNull.Value),
                        new OracleParameter("p6", (object)src.SOURCE_NVPC_ID ?? DBNull.Value),
                        new OracleParameter("p7", (object)src.SOURCE_KTKL_SOQD ?? DBNull.Value),
                        new OracleParameter("p8", (object)src.SOURCE_UL_ID ?? DBNull.Value),
                        new OracleParameter("p9", src.WEIGHT_QUANTITY),
                        new OracleParameter("p10", src.AMOUNT_CONTRIBUTED)
                    );
                }
            }

            // 3. Save OT Compliance Snapshot
            if (res.OtCompliance != null)
            {
                decimal otId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_OT_COMPLIANCE.NEXTVAL FROM DUAL").First();
                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_BANGLUONG_OT_COMPLIANCE (
                        ID, IDBL, MANV, MAKYCONG, MONTHLY_STANDARD_LIMIT, ANNUAL_STANDARD_LIMIT,
                        ANNUAL_EXTENDED_LIMIT, ACTUAL_HOURS_MONTH, ACTUAL_HOURS_YTD, LEGAL_HOURS_MONTH,
                        EXCESS_HOURS_MONTH, EXTENDED_ELIGIBLE, EMPLOYEE_CONSENT, NOTIFICATION_FILED,
                        COMPLIANCE_STATUS, TOTAL_OT_PAYMENT, LEGAL_ALLOWED_PAYMENT, EXEMPT_OT_PAYMENT,
                        TAXABLE_EXCESS_PAYMENT, CREATED_AT
                    ) VALUES (
                        :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, :p10, :p11, :p12, :p13, :p14, :p15, :p16, :p17, :p18, SYSTIMESTAMP
                    )",
                    new OracleParameter("p0", otId),
                    new OracleParameter("p1", idbl),
                    new OracleParameter("p2", res.MANV),
                    new OracleParameter("p3", res.MAKYCONG),
                    new OracleParameter("p4", res.OtCompliance.MONTHLY_STANDARD_LIMIT),
                    new OracleParameter("p5", res.OtCompliance.ANNUAL_STANDARD_LIMIT),
                    new OracleParameter("p6", res.OtCompliance.ANNUAL_EXTENDED_LIMIT),
                    new OracleParameter("p7", res.OtCompliance.ACTUAL_HOURS_MONTH),
                    new OracleParameter("p8", res.OtCompliance.ACTUAL_HOURS_YTD),
                    new OracleParameter("p9", res.OtCompliance.LEGAL_HOURS_MONTH),
                    new OracleParameter("p10", res.OtCompliance.EXCESS_HOURS_MONTH),
                    new OracleParameter("p11", (object)res.OtCompliance.EXTENDED_ELIGIBLE ?? DBNull.Value),
                    new OracleParameter("p12", (object)res.OtCompliance.EMPLOYEE_CONSENT ?? DBNull.Value),
                    new OracleParameter("p13", (object)res.OtCompliance.NOTIFICATION_FILED ?? DBNull.Value),
                    new OracleParameter("p14", res.OtCompliance.COMPLIANCE_STATUS),
                    new OracleParameter("p15", (object)res.OtCompliance.TOTAL_OT_PAYMENT ?? DBNull.Value),
                    new OracleParameter("p16", (object)res.OtCompliance.LEGAL_ALLOWED_PAYMENT ?? DBNull.Value),
                    new OracleParameter("p17", (object)res.OtCompliance.EXEMPT_OT_PAYMENT ?? DBNull.Value),
                    new OracleParameter("p18", (object)res.OtCompliance.TAXABLE_EXCESS_PAYMENT ?? DBNull.Value)
                );
            }

            // 4. Save Insurance Trace
            if (res.InsuranceTrace != null)
            {
                decimal insId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_BAOHIEM.NEXTVAL FROM DUAL").First();
                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_BANGLUONG_BAOHIEM (
                        ID, IDBL, MANV, MAKYCONG, POLICY_BHXH_ID, PROFILE_BH_ID, MUC_THAM_CHIEU,
                        VUNG_LUONG, LUONG_TOI_THIEU_VUNG, LUONG_DONG_BHXH_GOC, LUONG_DONG_BHXH_AP_DUNG,
                        LUONG_DONG_BHTN_AP_DUNG, TY_LE_BHXH_NLD, TIEN_BHXH_NLD, TY_LE_BHYT_NLD,
                        TIEN_BHYT_NLD, TY_LE_BHTN_NLD, TIEN_BHTN_NLD, TONG_BH_NLD, TY_LE_BHXH_NSDLD,
                        TIEN_BHXH_NSDLD, TY_LE_BHYT_NSDLD, TIEN_BHYT_NSDLD, TY_LE_BHTN_NSDLD,
                        TIEN_BHTN_NSDLD, TY_LE_TNLD_BNN_NSDLD, TIEN_TNLD_BNN_NSDLD, TONG_BH_NSDLD, CREATED_AT
                    ) VALUES (
                        :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, :p10, :p11, :p12, :p13, :p14,
                        :p15, :p16, :p17, :p18, :p19, :p20, :p21, :p22, :p23, :p24, :p25, :p26, :p27, SYSTIMESTAMP
                    )",
                    new OracleParameter("p0", insId),
                    new OracleParameter("p1", idbl),
                    new OracleParameter("p2", res.MANV),
                    new OracleParameter("p3", res.MAKYCONG),
                    new OracleParameter("p4", res.InsuranceTrace.POLICY_BHXH_ID),
                    new OracleParameter("p5", res.InsuranceTrace.PROFILE_BH_ID),
                    new OracleParameter("p6", res.InsuranceTrace.MUC_THAM_CHIEU),
                    new OracleParameter("p7", res.InsuranceTrace.VUNG_LUONG),
                    new OracleParameter("p8", res.InsuranceTrace.LUONG_TOI_THIEU_VUNG),
                    new OracleParameter("p9", res.InsuranceTrace.LUONG_DONG_BHXH_GOC),
                    new OracleParameter("p10", res.InsuranceTrace.LUONG_DONG_BHXH_AP_DUNG),
                    new OracleParameter("p11", res.InsuranceTrace.LUONG_DONG_BHTN_AP_DUNG),
                    new OracleParameter("p12", res.InsuranceTrace.TY_LE_BHXH_NLD),
                    new OracleParameter("p13", res.InsuranceTrace.TIEN_BHXH_NLD),
                    new OracleParameter("p14", res.InsuranceTrace.TY_LE_BHYT_NLD),
                    new OracleParameter("p15", res.InsuranceTrace.TIEN_BHYT_NLD),
                    new OracleParameter("p16", res.InsuranceTrace.TY_LE_BHTN_NLD),
                    new OracleParameter("p17", res.InsuranceTrace.TIEN_BHTN_NLD),
                    new OracleParameter("p18", res.InsuranceTrace.TONG_BH_NLD),
                    new OracleParameter("p19", res.InsuranceTrace.TY_LE_BHXH_NSDLD),
                    new OracleParameter("p20", res.InsuranceTrace.TIEN_BHXH_NSDLD),
                    new OracleParameter("p21", res.InsuranceTrace.TY_LE_BHYT_NSDLD),
                    new OracleParameter("p22", res.InsuranceTrace.TIEN_BHYT_NSDLD),
                    new OracleParameter("p23", res.InsuranceTrace.TY_LE_BHTN_NSDLD),
                    new OracleParameter("p24", res.InsuranceTrace.TIEN_BHTN_NSDLD),
                    new OracleParameter("p25", res.InsuranceTrace.TY_LE_TNLD_BNN_NSDLD),
                    new OracleParameter("p26", res.InsuranceTrace.TIEN_TNLD_BNN_NSDLD),
                    new OracleParameter("p27", res.InsuranceTrace.TONG_BH_NSDLD)
                );
            }

            // 5. Save Union Trace
            if (res.UnionTrace != null)
            {
                decimal unionId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_CONG_DOAN.NEXTVAL FROM DUAL").First();
                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_BANGLUONG_CONG_DOAN (
                        ID, IDBL, MANV, MAKYCONG, POLICY_CD_ID, PROFILE_CD_ID, LA_DOAN_VIEN,
                        LUONG_CAN_CU_DONG, TY_LE_DOAN_PHI_NLD, MUC_TRAN_DOAN_PHI, TIEN_DOAN_PHI_NLD,
                        TY_LE_KPCD_NSDLD, TIEN_KPCD_NSDLD, CREATED_AT
                    ) VALUES (
                        :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, :p10, :p11, :p12, SYSTIMESTAMP
                    )",
                    new OracleParameter("p0", unionId),
                    new OracleParameter("p1", idbl),
                    new OracleParameter("p2", res.MANV),
                    new OracleParameter("p3", res.MAKYCONG),
                    new OracleParameter("p4", res.UnionTrace.POLICY_CD_ID),
                    new OracleParameter("p5", res.UnionTrace.PROFILE_CD_ID),
                    new OracleParameter("p6", res.UnionTrace.LA_DOAN_VIEN),
                    new OracleParameter("p7", res.UnionTrace.LUONG_CAN_CU_DONG),
                    new OracleParameter("p8", res.UnionTrace.TY_LE_DOAN_PHI_NLD),
                    new OracleParameter("p9", res.UnionTrace.MUC_TRAN_DOAN_PHI),
                    new OracleParameter("p10", res.UnionTrace.TIEN_DOAN_PHI_NLD),
                    new OracleParameter("p11", res.UnionTrace.TY_LE_KPCD_NSDLD),
                    new OracleParameter("p12", res.UnionTrace.TIEN_KPCD_NSDLD)
                );
            }

            // 6. Save Tax Traces
            foreach (var t in res.TaxTraces)
            {
                decimal taxId = db.Database.SqlQuery<decimal>("SELECT SEQ_BANGLUONG_THUE_CT.NEXTVAL FROM DUAL").First();
                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_BANGLUONG_THUE_CT (
                        ID, IDBL, MANV, MAKYCONG, POLICY_THUE_ID, BAC_THUE, CAN_DUOI, CAN_TREN,
                        THU_NHAP_CHIU_THUE_BAC, THUE_SUAT, TIEN_THUE_BAC, CREATED_AT
                    ) VALUES (
                        :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, :p10, SYSTIMESTAMP
                    )",
                    new OracleParameter("p0", taxId),
                    new OracleParameter("p1", idbl),
                    new OracleParameter("p2", res.MANV),
                    new OracleParameter("p3", res.MAKYCONG),
                    new OracleParameter("p4", t.POLICY_THUE_ID),
                    new OracleParameter("p5", t.BAC_THUE),
                    new OracleParameter("p6", t.CAN_DUOI),
                    new OracleParameter("p7", (object)t.CAN_TREN ?? DBNull.Value),
                    new OracleParameter("p8", t.THU_NHAP_CHIU_THUE_BAC),
                    new OracleParameter("p9", t.THUE_SUAT),
                    new OracleParameter("p10", t.TIEN_THUE_BAC)
                );
            }
        }

        private class EmployeeAttendanceRow
        {
            public decimal MANV { get; set; }
            public string HOTEN { get; set; }
            public decimal? BASE_SALARY { get; set; }
            public decimal? IDPB { get; set; }
            public decimal? IDCV { get; set; }
        }

        private class AttendanceSummaryRow
        {
            public decimal? TONGNGAYCONG { get; set; }
            public decimal? NGAYPHEP { get; set; }
            public decimal? CONGCHUNHAT { get; set; }
        }

        private class AllowanceQueryRow
        {
            public decimal IDPC { get; set; }
            public string TENPC { get; set; }
            public decimal SOTIEN { get; set; }
        }

        private class OvertimeQueryRow
        {
            public decimal IDTCA { get; set; }
            public decimal? SOGIO { get; set; }
            public decimal? HESOTC { get; set; }
            public decimal? DONGIATC { get; set; }
            public decimal? SOTIENTC { get; set; }
        }

        private class AdvanceQueryRow
        {
            public decimal IDUL { get; set; }
            public decimal? SOTIENUNG { get; set; }
        }

        private class ExistingBlRow
        {
            public decimal IDBL { get; set; }
            public int? IS_LEGACY { get; set; }
        }
    }
}
