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
        public decimal AttendanceInputRevision { get; set; }
        public decimal AttendancePublishRevision { get; set; }
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
        public List<PayrollDetailSourceDto> AttendanceSources { get; set; } = new List<PayrollDetailSourceDto>();
        public List<RewardDisciplineInput> RewardsAndDisciplines { get; set; } = new List<RewardDisciplineInput>();
        public List<AdvanceSalaryInput> Advances { get; set; } = new List<AdvanceSalaryInput>();
        public decimal YtdOvertimeHoursPrior { get; set; }
        public int? ExtendedOtEligible { get; set; }
        public int? EmployeeOtConsent { get; set; }
        public int? OtNotificationFiled { get; set; }
        public int AttendanceUnpublishedDays { get; set; }
        public int AttendanceUnresolvedAnomalies { get; set; }
        public decimal? PublishedOtSeconds { get; set; }
        public decimal? PublishedNightSeconds { get; set; }
    }

    public class AllowanceItemInput
    {
        public decimal IDPC { get; set; }
        public string TENPC { get; set; }
        public decimal SOTIEN { get; set; }
        public string CACH_TINH { get; set; }
        public int TINH_BHXH { get; set; }
        public int TINH_THUE { get; set; }
        public decimal? SO_TIEN_MIEN_THUE { get; set; }
        public decimal? SOURCE_NVPC_ID { get; set; }
    }

    public class OvertimeItemInput
    {
        public decimal? SOURCE_BCCT_ID { get; set; }
        public decimal? SOURCE_CONG_LANTINH_ID { get; set; }
        public decimal? POLICY_OT_ID { get; set; }
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
        public TaxCalculationResult TaxTrace { get; set; }

        public decimal CongChuan { get; set; }
        public decimal CongThucTe { get; set; }
        public decimal CongLamNgay { get; set; }
        public decimal CongLamDem { get; set; }
        public decimal TongCong { get; set; }
        public decimal DailyRate { get; set; }
        public decimal DailyAllowance { get; set; }
        public decimal LuongCongThucTe { get; set; }
        public decimal PhuCapCongThucTe { get; set; }
        public decimal TienTangCa { get; set; }
        public decimal TienChuyenCan { get; set; }
        public decimal TienAnCa { get; set; }
        public decimal KhoanCongKhac { get; set; }
        public decimal TienTamUng { get; set; }
        public decimal KhoanTruKhac { get; set; }
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
                // 0. Pre-flight Checks (Fail-Closed)
                // A. Check Period Lock: Cannot recalculate if locked
                var kyCong = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == (decimal)makycong);
                if (kyCong == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy kỳ công {makycong} trong hệ thống.");
                }
                if ((kyCong.KHOA ?? 0) == 1)
                {
                    throw new InvalidOperationException($"Kỳ công {makycong} đã bị khóa sổ (KHOA = 1). Không được phép tính toán lại hoặc ghi đè bảng lương.");
                }


                // C. Check Attendance Readiness: All days must be published and match current input revision
                var initialRevision = ReadAttendanceRevision(db, makycong, false);
                var unpublishedDays = db.Database.SqlQuery<decimal>(@"
                    SELECT COUNT(*) FROM TB_BANGCONG_CHITIET
                    WHERE MAKYCONG = :p0 AND (LANTINH_ID_HIENHANH IS NULL OR NVL(DU_DIEUKIEN_CHOT, 0) = 0)",
                    new OracleParameter("p0", makycong)).Single();
                if (unpublishedDays > 0)
                {
                    throw new InvalidOperationException($"Kỳ {makycong} có {unpublishedDays} ngày công chưa qua công bố TimeSegmentationEngine hoặc có bất thường chặn chốt chưa giải quyết. Cần xử lý bất thường và công bố bảng công trước khi tính lương.");
                }

                var staleDaysCount = db.Database.SqlQuery<decimal>(@"
                    SELECT COUNT(*) FROM TB_BANGCONG_CHITIET b
                    JOIN TB_CHAMCONG_LANTINH r ON r.IDLANTINH = b.LANTINH_ID_HIENHANH
                    WHERE b.MAKYCONG = :p0 AND r.INPUT_REV <> :p1",
                    new OracleParameter("p0", makycong),
                    new OracleParameter("p1", initialRevision.INPUT_REV)).Single();
                if (staleDaysCount > 0)
                {
                    throw new InvalidOperationException($"Dữ liệu chấm công đã thay đổi sau lần công bố gần nhất ({staleDaysCount} ngày công bị lệch revision). Vui lòng công bố lại bảng công trước khi tính lương.");
                }

                // Create Calculation Run in IN_PROGRESS state (separate tracking)
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

                // Load all eligible employees in period (prioritizing TB_LUONG_HIEULUC with fallback to TB_HOPDONG)
                var employees = db.Database.SqlQuery<EmployeeAttendanceRow>(@"
                    SELECT MANV, HOTEN, BASE_SALARY, IDPB, IDCV, CONTRACT_COUNT, CONTRACT_START, CONTRACT_END FROM (
                        SELECT nv.MANV, nv.HOTEN, 
                               NVL(lh.LUONG_THANG, hd.LUONG_THOA_THUAN) AS BASE_SALARY, 
                               nv.IDPB, nv.IDCV, hd.NGAYBATDAU AS CONTRACT_START, hd.NGAYKETTHUC AS CONTRACT_END,
                               COUNT(*) OVER(PARTITION BY nv.MANV) AS CONTRACT_COUNT,
                               ROW_NUMBER() OVER(PARTITION BY nv.MANV ORDER BY hd.NGAYBATDAU DESC, hd.SOHD DESC) AS RN
                        FROM TB_NHANVIEN nv 
                        JOIN TB_HOPDONG hd ON hd.MANV = nv.MANV
                        LEFT JOIN (
                            SELECT MANV, LUONG_THANG,
                                   ROW_NUMBER() OVER(PARTITION BY MANV ORDER BY TU_NGAY DESC, ID DESC) AS RN_LH
                            FROM TB_LUONG_HIEULUC
                            WHERE TU_NGAY <= :p_end AND (DEN_NGAY IS NULL OR DEN_NGAY >= :p_start)
                        ) lh ON lh.MANV = nv.MANV AND lh.RN_LH = 1
                        WHERE (nv.DELETED_DATE IS NULL OR nv.DELETED_DATE >= :p_start)
                          AND hd.DEL_DATE IS NULL
                          AND hd.NGAYBATDAU <= :p_end
                          AND (hd.NGAYKETTHUC IS NULL OR hd.NGAYKETTHUC >= :p_start)
                    ) WHERE RN = 1 ORDER BY MANV",
                    new OracleParameter("p_start", new DateTime(nam, thang, 1)),
                    new OracleParameter("p_end", effectiveDate)).ToList();

                if (employees.Count == 0)
                {
                    db.Database.ExecuteSqlCommand(@"
                        UPDATE TB_PAYROLL_CALCULATION_RUN
                        SET TOTAL_EMPLOYEES = 0, SUCCESS_COUNT = 0, ERROR_COUNT = 0,
                            STATUS = 'FAILED', ERROR_SUMMARY = 'Không có nhân viên nào đủ điều kiện hợp đồng trong kỳ',
                            FINISHED_AT = SYSTIMESTAMP
                        WHERE RUN_ID = :p0",
                        new OracleParameter("p0", runId));
                    throw new InvalidOperationException($"Không có nhân viên nào có hợp đồng lao động hợp lệ trong kỳ {makycong}.");
                }

                // PHASE 2: In-Memory Calculation & Validation for 100% of employees
                var calculatedList = new List<Tuple<EmployeeAttendanceRow, PayrollCalculationResult, EmployeeInsuranceProfileDto, EmployeeUnionProfileDto, EmployeeTaxProfileDto, int>>();
                var employeeErrors = new List<string>();

                foreach (var emp in employees)
                {
                    try
                    {
                        if (emp.CONTRACT_COUNT > 1)
                        {
                            var allContracts = db.Database.SqlQuery<ContractDateRow>(@"
                                SELECT SOHD, NGAYBATDAU, NGAYKETTHUC, LUONG_THOA_THUAN
                                FROM TB_HOPDONG
                                WHERE MANV = :p0 AND DEL_DATE IS NULL
                                  AND NGAYBATDAU <= :p1 AND (NGAYKETTHUC IS NULL OR NGAYKETTHUC >= :p2)
                                ORDER BY NGAYBATDAU",
                                new OracleParameter("p0", emp.MANV),
                                new OracleParameter("p1", effectiveDate),
                                new OracleParameter("p2", new DateTime(nam, thang, 1))
                            ).ToList();

                            for (int i = 0; i < allContracts.Count - 1; i++)
                            {
                                var c1 = allContracts[i];
                                var c2 = allContracts[i + 1];
                                if (c1.NGAYKETTHUC.HasValue && c1.NGAYKETTHUC.Value >= c2.NGAYBATDAU)
                                {
                                    throw new InvalidOperationException($"EMPLOYMENT_INTERVAL_OVERLAP: Có hợp đồng chồng lấn thời gian hiệu lực ({c1.SOHD} kết thúc {c1.NGAYKETTHUC:dd/MM/yyyy} trùng lặp với {c2.SOHD} bắt đầu {c2.NGAYBATDAU:dd/MM/yyyy}).");
                                }
                            }
                        }
                        if (!emp.BASE_SALARY.HasValue || emp.BASE_SALARY.Value <= 0)
                            throw new InvalidOperationException("Thiếu mức lương thỏa thuận hợp đồng; không được suy đoán.");

                        var input = LoadEmployeePayrollInput(db, emp.MANV, makycong, nam, thang, emp.BASE_SALARY.Value, salaryPolicy.SO_CONG_CHUAN_THANG);

                        DateTime profileDate = emp.CONTRACT_START > effectiveDate ? emp.CONTRACT_START : effectiveDate;
                        if (emp.CONTRACT_END.HasValue && emp.CONTRACT_END.Value < profileDate) profileDate = emp.CONTRACT_END.Value;

                        // Pure Profile Resolution: FAIL CLOSED if missing! Do NOT silently auto-insert or guess.
                        var insProfile = _profileResolver.GetInsuranceProfile(emp.MANV, profileDate);
                        if (insProfile == null)
                            throw new InvalidOperationException($"Thiếu hồ sơ bảo hiểm xã hội (TB_NHANVIEN_BAOHIEM_THAM_GIA) hiệu lực tại ngày {profileDate:dd/MM/yyyy}.");

                        var unionProfile = _profileResolver.GetUnionProfile(emp.MANV, profileDate);
                        if (unionProfile == null)
                            throw new InvalidOperationException($"Thiếu hồ sơ công đoàn (TB_NHANVIEN_CONG_DOAN_THAM_GIA) hiệu lực tại ngày {profileDate:dd/MM/yyyy}.");

                        var taxProfile = _profileResolver.GetTaxProfile(emp.MANV, profileDate);
                        if (taxProfile == null)
                            throw new InvalidOperationException($"Thiếu hồ sơ mã số thuế (TB_NHANVIEN_THUE) hiệu lực tại ngày {profileDate:dd/MM/yyyy}.");

                        var dependents = _profileResolver.GetActiveDependents(emp.MANV, (int)makycong);
                        var region = _policyResolver.GetInsuranceRegion(insurancePolicy.ID, insProfile.VUNG_LUONG);

                        // Pure In-Memory Single Employee Payroll Math
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

                        calculatedList.Add(Tuple.Create(emp, result, insProfile, unionProfile, taxProfile, dependents.Count));
                    }
                    catch (Exception ex)
                    {
                        employeeErrors.Add($"[NV {emp.MANV} - {emp.HOTEN}]: {ex.Message}");
                    }
                }

                // PHASE 3: Fail-Closed All-or-Nothing Gate
                if (employeeErrors.Count > 0)
                {
                    string errSummary = string.Join("; ", employeeErrors.Take(10));
                    if (errSummary.Length > 1000) errSummary = errSummary.Substring(0, 1000);

                    db.Database.ExecuteSqlCommand(@"
                        UPDATE TB_PAYROLL_CALCULATION_RUN
                        SET TOTAL_EMPLOYEES = :p0, SUCCESS_COUNT = :p1, WARNING_COUNT = 0, ERROR_COUNT = :p2,
                            STATUS = 'FAILED', ERROR_SUMMARY = :p3, FINISHED_AT = SYSTIMESTAMP
                        WHERE RUN_ID = :p4",
                        new OracleParameter("p0", employees.Count),
                        new OracleParameter("p1", calculatedList.Count),
                        new OracleParameter("p2", employeeErrors.Count),
                        new OracleParameter("p3", errSummary),
                        new OracleParameter("p4", runId)
                    );

                    throw new InvalidOperationException($"Tính lương kỳ {makycong} thất bại. Có {employeeErrors.Count}/{employees.Count} nhân sự gặp lỗi nghiệp vụ. Toàn bộ bảng lương hiện hành được giữ nguyên.\nChi tiết lỗi:\n" + string.Join("\n", employeeErrors));
                }

                // PHASE 4: Atomic Write Transaction with Pessimistic Lock
                using (var tx = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        // 4.1 Lock the period row via SELECT ... FOR UPDATE
                        var lockedRevision = ReadAttendanceRevision(db, makycong, true);
                        if (lockedRevision.INPUT_REV != initialRevision.INPUT_REV || lockedRevision.PUBLISH_REV != initialRevision.PUBLISH_REV)
                        {
                            throw new InvalidOperationException("Dữ liệu chấm công đã bị thay đổi trong quá trình tính toán lương. Toàn bộ giao dịch bị hủy bỏ; vui lòng tính toán lại.");
                        }

                        // Re-verify period is still unlocked under pessimistic lock
                        var lockedKc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == (decimal)makycong);
                        if (lockedKc != null && (lockedKc.KHOA ?? 0) == 1)
                        {
                            throw new InvalidOperationException($"Kỳ công {makycong} đã bị khóa trong khi đang xử lý.");
                        }

                        // 4.2 Atomically write every calculated employee
                        foreach (var item in calculatedList)
                        {
                            SaveCalculatedPayroll(db, item.Item2, salaryPolicy, insurancePolicy, unionPolicy, taxPolicy, item.Item3, item.Item4, item.Item5, item.Item6, runId);
                        }

                        // 4.3 Update calculation run status to SUCCESS in the SAME transaction
                        db.Database.ExecuteSqlCommand(@"
                            UPDATE TB_PAYROLL_CALCULATION_RUN
                            SET TOTAL_EMPLOYEES = :p0, SUCCESS_COUNT = :p1, WARNING_COUNT = 0, ERROR_COUNT = 0,
                                STATUS = 'SUCCESS', FINISHED_AT = SYSTIMESTAMP
                            WHERE RUN_ID = :p2",
                            new OracleParameter("p0", employees.Count),
                            new OracleParameter("p1", calculatedList.Count),
                            new OracleParameter("p2", runId)
                        );

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }

                        // Record FAILED run status in separate transaction
                        try
                        {
                            using (var failDb = new MyEntities())
                            {
                                string failMsg = ex.Message.Length > 1000 ? ex.Message.Substring(0, 1000) : ex.Message;
                                failDb.Database.ExecuteSqlCommand(@"
                                    UPDATE TB_PAYROLL_CALCULATION_RUN
                                    SET STATUS = 'FAILED', ERROR_SUMMARY = :p0, FINISHED_AT = SYSTIMESTAMP
                                    WHERE RUN_ID = :p1",
                                    new OracleParameter("p0", failMsg),
                                    new OracleParameter("p1", runId));
                            }
                        }
                        catch { }

                        throw new InvalidOperationException("Lỗi giao dịch khi lưu bảng lương kỳ " + makycong + ": " + ex.Message, ex);
                    }
                }

                return new PayrollRunDto
                {
                    RUN_ID = runId,
                    MAKYCONG = makycong,
                    NAM = nam,
                    THANG = thang,
                    EXECUTION_TYPE = "FULL_RECALC",
                    TOTAL_EMPLOYEES = employees.Count,
                    SUCCESS_COUNT = calculatedList.Count,
                    WARNING_COUNT = 0,
                    ERROR_COUNT = 0,
                    STATUS = "SUCCESS",
                    STARTED_AT = DateTime.Now,
                    FINISHED_AT = DateTime.Now,
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

            // 0. Attendance Verification & Audit Step
            string attendanceNote = input.AttendanceUnpublishedDays > 0
                ? $"CẢNH BÁO: Phát hiện {input.AttendanceUnpublishedDays} ngày công chưa qua TimeSegmentationEngine (LANTINH_ID_HIENHANH IS NULL). Dữ liệu giờ OT/đêm chưa xác định không được tự động coi là 0."
                : (input.AttendanceUnresolvedAnomalies > 0
                    ? $"CẢNH BÁO: Phát hiện {input.AttendanceUnresolvedAnomalies} ngày có bất thường chưa giải quyết (DU_DIEUKIEN_CHOT = 0)."
                    : null);

            if (!string.IsNullOrEmpty(attendanceNote))
            {
                throw new InvalidOperationException($"[NV {input.MANV}] {attendanceNote}");
            }

            // 1. Standard Working Days & Daily Rates
            decimal dailyRate = Math.Round(input.BaseSalary / salaryPolicy.SO_CONG_CHUAN_THANG, 2);
            // In the attendance model, ActualDaysWorked (TONGNGAYCONG) includes both worked days and paid leave days.
            // Pure worked days = ActualDaysWorked - LeaveDaysWithPay.
            decimal workDays = Math.Max(0m, input.ActualDaysWorked - input.LeaveDaysWithPay);
            decimal totalPaidDays = Math.Min(salaryPolicy.SO_CONG_CHUAN_THANG, workDays + input.LeaveDaysWithPay);
            decimal totalRegularWage = totalPaidDays >= salaryPolicy.SO_CONG_CHUAN_THANG
                ? input.BaseSalary
                : Math.Round(input.BaseSalary * (totalPaidDays / salaryPolicy.SO_CONG_CHUAN_THANG), 2);

            decimal leaveWage = Math.Round(input.LeaveDaysWithPay * dailyRate, 2);
            decimal standardWage = Math.Max(0m, totalRegularWage - leaveWage);
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
                SO_LUONG = workDays,
                DON_GIA = dailyRate,
                HE_SO = 1.0m,
                THANH_TIEN = standardWage,
                TINH_VAO_DONG_BHXH = 1,
                TINH_THUE_TNCN = 1,
                SO_TIEN_MIEN_THUE = 0m,
                SO_TIEN_CHIU_THUE = standardWage,
                CONG_THUC_DIEN_GIAI = $"Số công làm ({workDays}) * Đơn giá ngày ({dailyRate:N0})" + (!string.IsNullOrEmpty(attendanceNote) ? $" | [{attendanceNote}]" : "")
            });

            // Item: Paid Leave Wage
            if (leaveWage > 0)
            {
                res.DetailItems.Add(new PayrollDetailItemDto
                {
                    MANV = input.MANV,
                    MAKYCONG = input.MAKYCONG,
                    NHOM_KHOAN_MUC = "LUONG_CHINH",
                    MA_KHOAN_MUC = "LUONG_NGAY_PHEP",
                    TEN_KHOAN_MUC = "Lương ngày phép hưởng lương",
                    SO_LUONG = input.LeaveDaysWithPay,
                    DON_GIA = dailyRate,
                    HE_SO = 1.0m,
                    THANH_TIEN = leaveWage,
                    TINH_VAO_DONG_BHXH = 1,
                    TINH_THUE_TNCN = 1,
                    SO_TIEN_MIEN_THUE = 0m,
                    SO_TIEN_CHIU_THUE = leaveWage,
                    CONG_THUC_DIEN_GIAI = $"Số ngày phép ({input.LeaveDaysWithPay}) * Đơn giá ngày ({dailyRate:N0})"
                });
            }

            if (input.AttendanceSources.Count > 0)
            {
                var standardItem = res.DetailItems.FirstOrDefault(x => x.MA_KHOAN_MUC == "LUONG_CONG_THUCTE");
                if (standardItem != null)
                {
                    foreach (var source in input.AttendanceSources)
                    {
                        standardItem.Sources.Add(new PayrollDetailSourceDto {
                            MANV=input.MANV, SOURCE_TYPE="BANGCONG", SOURCE_BCCT_ID=source.SOURCE_BCCT_ID,
                            SOURCE_CONG_LANTINH_ID=source.SOURCE_CONG_LANTINH_ID,
                            WEIGHT_QUANTITY=source.WEIGHT_QUANTITY,
                            AMOUNT_CONTRIBUTED=Math.Round(source.WEIGHT_QUANTITY*dailyRate,2)
                        });
                    }
                    standardItem.Sources.Last().AMOUNT_CONTRIBUTED += standardWage-standardItem.Sources.Sum(x=>x.AMOUNT_CONTRIBUTED);
                }
            }

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
                    SO_TIEN_MIEN_THUE = nightWageAllowance,
                    SO_TIEN_CHIU_THUE = 0m,
                    CONG_THUC_DIEN_GIAI = $"Phụ cấp làm đêm 30% ({input.NightShiftDays} công đêm * 30% lương ngày) - Miễn thuế TNCN theo NĐ 253/2026"
                });
            }

            // 2. Allowances
            decimal totalAllowances = 0m;
            decimal totalTaxableAllowances = 0m;
            decimal totalInsuranceAllowances = 0m;

            foreach (var pc in input.Allowances)
            {
                decimal effectiveAmount = pc.SOTIEN;
                if (!string.IsNullOrEmpty(pc.CACH_TINH) && pc.CACH_TINH.Trim().ToUpper() == "THEO_NGAY_CONG" && salaryPolicy.SO_CONG_CHUAN_THANG > 0)
                {
                    effectiveAmount = Math.Round(pc.SOTIEN * (input.ActualDaysWorked / salaryPolicy.SO_CONG_CHUAN_THANG), 2);
                }

                totalAllowances += effectiveAmount;
                if (pc.TINH_BHXH == 1) totalInsuranceAllowances += effectiveAmount;

                decimal rawExempt = pc.SO_TIEN_MIEN_THUE ?? (pc.TINH_THUE == 0 ? effectiveAmount : 0m);
                decimal mienThue = Math.Min(effectiveAmount, Math.Max(0m, rawExempt));
                decimal chiuThue = Math.Max(0m, effectiveAmount - mienThue);
                totalTaxableAllowances += chiuThue;

                var pcItem = new PayrollDetailItemDto
                {
                    MANV = input.MANV,
                    MAKYCONG = input.MAKYCONG,
                    NHOM_KHOAN_MUC = "PHU_CAP",
                    MA_KHOAN_MUC = $"PHUCAP_{pc.IDPC}",
                    TEN_KHOAN_MUC = pc.TENPC,
                    SO_LUONG = pc.CACH_TINH == "THEO_NGAY_CONG" ? input.ActualDaysWorked : 1.0m,
                    DON_GIA = pc.SOTIEN,
                    HE_SO = 1.0m,
                    THANH_TIEN = effectiveAmount,
                    TINH_VAO_DONG_BHXH = pc.TINH_BHXH,
                    TINH_THUE_TNCN = pc.TINH_THUE,
                    SO_TIEN_MIEN_THUE = mienThue,
                    SO_TIEN_CHIU_THUE = chiuThue,
                    CONG_THUC_DIEN_GIAI = $"Phụ cấp hợp đồng: {pc.TENPC} ({pc.CACH_TINH ?? "CO_DINH_THANG"})"
                };

                if (pc.SOURCE_NVPC_ID.HasValue)
                {
                    pcItem.Sources.Add(new PayrollDetailSourceDto
                    {
                        MANV = input.MANV,
                        SOURCE_TYPE = "PHUCAP",
                        SOURCE_NVPC_ID = pc.IDPC,
                        WEIGHT_QUANTITY = pc.CACH_TINH == "THEO_NGAY_CONG" ? input.ActualDaysWorked : 1.0m,
                        AMOUNT_CONTRIBUTED = effectiveAmount
                    });
                }
                res.DetailItems.Add(pcItem);
            }

            // 3. Overtime & OT Compliance Engine
            decimal totalOtHours = input.Overtimes.Sum(o => o.SOGIO);
            if (input.PublishedOtSeconds.HasValue && Math.Abs(totalOtHours * 3600m - input.PublishedOtSeconds.Value) > 0.01m)
                throw new InvalidOperationException("Overtime payroll inputs do not reconcile with the published attendance seconds.");
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
                        SOURCE_TYPE = ot.SOURCE_BCCT_ID.HasValue ? "BANGCONG" : "TANGCA",
                        SOURCE_TC_ID = ot.SOURCE_BCCT_ID.HasValue ? (decimal?)null : ot.IDTCA,
                        SOURCE_BCCT_ID = ot.SOURCE_BCCT_ID,
                        SOURCE_CONG_LANTINH_ID = ot.SOURCE_CONG_LANTINH_ID,
                        POLICY_OT_ID = ot.POLICY_OT_ID,
                        WEIGHT_QUANTITY = ot.SOGIO,
                        AMOUNT_CONTRIBUTED = ot.SOTIENTC
                    });
                }
                res.DetailItems.Add(otItem);
            }

            // 4. Rewards and Penalties / Khấu trừ phát sinh
            // Phân biệt khoản khấu trừ lương với dữ liệu kỷ luật (Điều 127 BLLĐ 2019 cấm phạt tiền, trừ lương do kỷ luật)
            var validRewards = input.RewardsAndDisciplines.Where(r => r.LOAI == 1).ToList();
            var validDeductions = input.RewardsAndDisciplines.Where(r => r.LOAI == 2 && !IsDisciplinaryFine(r)).ToList();

            decimal totalRewards = validRewards.Sum(r => r.SOTIEN);
            decimal totalDeductions = validDeductions.Sum(r => r.SOTIEN);

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
                    CONG_THUC_DIEN_GIAI = "Khen thưởng theo quyết định đã duyệt"
                };
                foreach (var r in validRewards)
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

            if (totalDeductions > 0)
            {
                var disciplineItem = new PayrollDetailItemDto
                {
                    MANV = input.MANV,
                    MAKYCONG = input.MAKYCONG,
                    NHOM_KHOAN_MUC = "KHAU_TRU",
                    MA_KHOAN_MUC = "KHAU_TRU_PHAT_SINH",
                    TEN_KHOAN_MUC = "Khấu trừ phát sinh khác",
                    SO_LUONG = 1.0m,
                    DON_GIA = totalDeductions,
                    HE_SO = 1.0m,
                    THANH_TIEN = totalDeductions,
                    TINH_VAO_DONG_BHXH = 0,
                    TINH_THUE_TNCN = 0,
                    SO_TIEN_MIEN_THUE = 0m,
                    SO_TIEN_CHIU_THUE = 0m,
                    CONG_THUC_DIEN_GIAI = "Khấu trừ phát sinh theo quyết định/chứng từ đã duyệt"
                };
                foreach (var r in validDeductions)
                {
                    disciplineItem.Sources.Add(new PayrollDetailSourceDto
                    {
                        MANV = input.MANV,
                        SOURCE_TYPE = "KHENTHUONG_KYLUAT",
                        SOURCE_KTKL_SOQD = r.SOQUYETDINH,
                        WEIGHT_QUANTITY = 1.0m,
                        AMOUNT_CONTRIBUTED = r.SOTIEN
                    });
                }
                res.DetailItems.Add(disciplineItem);
            }

            // 5. Total Gross Earnings
            res.GrossEarnings = standardWage + leaveWage + nightWageAllowance + totalAllowances + totalOtPayment + totalRewards;

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
            // Night shift allowance is exempt from PIT; standardWage and leaveWage form regular wages
            decimal grossTaxable = standardWage + leaveWage + totalTaxableAllowances + (otCompliance.TAXABLE_EXCESS_PAYMENT ?? 0m) + totalRewards;
            var taxCalc = _taxEngine.CalculateMonthlyTax(
                0, input.MANV, input.MAKYCONG, grossTaxable, dependents.Count, insTrace.TONG_BH_NLD, taxPolicy, monthTaxBrackets, taxProfile.IS_CU_TRU == 1
            );
            res.TaxTrace = taxCalc;
            res.TaxTraces = taxCalc.BracketTraces;
            res.TaxWithheld = taxCalc.TotalTax;

            // 9. Advances & Deductions
            decimal totalAdvances = input.Advances.Sum(a => a.SOTIENUNG);

            // 10. Net Pay (Gross Earnings minus employee insurance, union fee, PIT, advances, and approved deduction occurrences)
            res.NetPay = res.GrossEarnings - res.InsuranceEmployee - res.UnionEmployee - res.TaxWithheld - totalAdvances - totalDeductions;

            // 11. Total Employer Cost
            res.TotalEmployerCost = res.GrossEarnings + res.InsuranceEmployer + res.UnionEmployer;

            // 12. Synchronize Summary Metrics for TB_BANGLUONG
            res.CongChuan = salaryPolicy.SO_CONG_CHUAN_THANG;
            res.CongThucTe = input.ActualDaysWorked;
            res.CongLamNgay = Math.Max(0m, input.ActualDaysWorked - input.NightShiftDays);
            res.CongLamDem = input.NightShiftDays;
            res.TongCong = res.GrossEarnings;
            res.DailyRate = dailyRate;
            res.DailyAllowance = 0m;
            res.LuongCongThucTe = standardWage + leaveWage;
            res.PhuCapCongThucTe = totalAllowances;
            res.TienTangCa = totalOtPayment;
            res.TienChuyenCan = input.Allowances.Where(a => a.TENPC != null && a.TENPC.ToLower().Contains("chuyên cần")).Sum(a => a.SOTIEN);
            res.TienAnCa = input.Allowances.Where(a => a.TENPC != null && (a.TENPC.ToLower().Contains("ăn ca") || a.TENPC.ToLower().Contains("tiền ăn") || a.TENPC.ToLower().Contains("cơm trưa"))).Sum(a => a.SOTIEN);
            res.KhoanCongKhac = nightWageAllowance + totalRewards;
            res.TienTamUng = totalAdvances;
            res.KhoanTruKhac = totalDeductions;

            return res;
        }

        private static bool IsDisciplinaryFine(RewardDisciplineInput r)
        {
            if (r == null) return false;
            // Theo Điều 127 Bộ luật Lao động 2019: nghiêm cấm phạt tiền, cắt lương thay việc xử lý kỷ luật lao động.
            // Do đó quyết định xử lý kỷ luật vi phạm nội quy/kỷ luật lao động không được tự động khấu trừ vào lương.
            string text = ((r.LYDO ?? "") + " " + (r.SOQUYETDINH ?? "")).ToLowerInvariant();
            return text.Contains("kỷ luật") || text.Contains("vi phạm nội quy") || text.Contains("phạt vi phạm");
        }

        private static bool? _hasTrangThaiInKtkl = null;
        internal static bool CheckTrangThaiColumnInKtkl(MyEntities db)
        {
            if (_hasTrangThaiInKtkl.HasValue) return _hasTrangThaiInKtkl.Value;
            try
            {
                int cnt = db.Database.SqlQuery<int>(@"
                    SELECT COUNT(*) FROM USER_TAB_COLS 
                    WHERE TABLE_NAME = 'TB_KHENTHUONG_KYLUAT' AND COLUMN_NAME = 'TRANG_THAI'
                ").FirstOrDefault();
                _hasTrangThaiInKtkl = cnt > 0;
            }
            catch
            {
                _hasTrangThaiInKtkl = false;
            }
            return _hasTrangThaiInKtkl.Value;
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

            var revision = ReadAttendanceRevision(db, makycong, false);
            input.AttendanceInputRevision = revision.INPUT_REV;
            input.AttendancePublishRevision = revision.PUBLISH_REV;
            var staleDays = db.Database.SqlQuery<decimal>(@"
                SELECT COUNT(*) FROM TB_BANGCONG_CHITIET b
                JOIN TB_CHAMCONG_LANTINH r ON r.IDLANTINH=b.LANTINH_ID_HIENHANH
                WHERE b.MANV=:p0 AND b.MAKYCONG=:p1 AND r.INPUT_REV<>:p2",
                new OracleParameter("p0",manv),new OracleParameter("p1",makycong),
                new OracleParameter("p2",revision.INPUT_REV)).Single();
            if (staleDays>0)
                throw new InvalidOperationException("Published attendance is stale; republish attendance before calculating payroll.");

            // 0. Audit check: Phân biệt đã công bố / chưa công bố từ TB_BANGCONG_CHITIET (tránh biến NULL thành 0 khi chưa xác định)
            var publishCheck = db.Database.SqlQuery<AttendancePublishCheckRow>(@"
                SELECT 
                    COUNT(*) AS TONG_SO_NGAY,
                    COUNT(CASE WHEN LANTINH_ID_HIENHANH IS NOT NULL THEN 1 END) AS SO_NGAY_CO_CONGBO,
                    COUNT(CASE WHEN LANTINH_ID_HIENHANH IS NULL THEN 1 END) AS SO_NGAY_CHUA_CONGBO,
                    COUNT(CASE WHEN DU_DIEUKIEN_CHOT = 0 THEN 1 END) AS SO_NGAY_CHUA_CHOT,
                    SUM(CASE WHEN LANTINH_ID_HIENHANH IS NOT NULL THEN NVL(GIAY_OT_XAC_NHAN, 0) ELSE NULL END) AS TONG_GIAY_OT,
                    SUM(CASE WHEN LANTINH_ID_HIENHANH IS NOT NULL THEN NVL(GIAY_DEM_TRONG_GIO_THUONG, 0) ELSE NULL END) AS TONG_GIAY_DEM
                FROM TB_BANGCONG_CHITIET
                WHERE MANV = :p0 AND MAKYCONG = :p1",
                new OracleParameter("p0", manv),
                new OracleParameter("p1", makycong)
            ).FirstOrDefault();

            if (publishCheck == null || (publishCheck.TONG_SO_NGAY ?? 0) == 0)
                throw new InvalidOperationException("No attendance days exist for this employee and period.");

            if (publishCheck != null)
            {
                input.AttendanceUnpublishedDays = (int)(publishCheck.SO_NGAY_CHUA_CONGBO ?? 0);
                input.AttendanceUnresolvedAnomalies = (int)(publishCheck.SO_NGAY_CHUA_CHOT ?? 0);
                input.PublishedOtSeconds = publishCheck.TONG_GIAY_OT;
                input.PublishedNightSeconds = publishCheck.TONG_GIAY_DEM;
            }

            // 1. Attendance summary from TB_KYCONGCHITIET and TB_BANGCONG_CHITIET
            var kcct = db.Database.SqlQuery<AttendanceSummaryRow>(@"
                SELECT TONGNGAYCONG, NGAYPHEP, CONGCHUNHAT
                FROM TB_KYCONGCHITIET
                WHERE MANV = :p0 AND MAKYCONG = :p1",
                new OracleParameter("p0", manv),
                new OracleParameter("p1", makycong)
            ).FirstOrDefault();

            decimal totalPaidDays = 0m;
            decimal leaveDays = 0m;

            if (kcct != null && kcct.TONGNGAYCONG.HasValue)
            {
                totalPaidDays = kcct.TONGNGAYCONG.Value;
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
                    totalPaidDays = bcctSummary.TONGNGAYCONG ?? 0m;
                    leaveDays = bcctSummary.NGAYPHEP ?? 0m;
                }
            }

            input.LeaveDaysWithPay = leaveDays;
            input.ActualDaysWorked = totalPaidDays;

            // Count night shifts: Ưu tiên dữ liệu đã công bố V1.18 từ GIAY_DEM_TRONG_GIO_THUONG + GIAY_DEM_OT
            if (publishCheck != null && (publishCheck.SO_NGAY_CO_CONGBO ?? 0) > 0 && publishCheck.TONG_GIAY_DEM.HasValue)
            {
                input.NightShiftDays = Math.Round(publishCheck.TONG_GIAY_DEM.Value / 28800m, 2);
            }
            else
            {
                // Fallback cho dữ liệu legacy chưa qua công bố V1.18
                var bcctNight = db.Database.SqlQuery<decimal?>(@"
                    SELECT SUM(NVL(NGAYCONG, 0))
                    FROM TB_BANGCONG_CHITIET
                    WHERE MANV = :p0 AND MAKYCONG = :p1
                      AND (KYHIEU IN ('CD', 'Đ', 'XĐ') OR GIOVAO >= '18:00' OR (GIOVAO IS NOT NULL AND GIOVAO < '06:00'))",
                    new OracleParameter("p0", manv),
                    new OracleParameter("p1", makycong)
                ).FirstOrDefault();

                input.NightShiftDays = bcctNight ?? 0.0m;
            }

            // Allowances from TB_NHANVIEN_PHUCAP
            var endOfMonth = new DateTime(nam, thang, DateTime.DaysInMonth(nam, thang));
            var startOfMonth = new DateTime(nam, thang, 1);
            var pcs = db.Database.SqlQuery<AllowanceQueryRow>(@"
                SELECT np.IDPC, p.TENPC, np.SOTIEN, np.CACH_TINH
                FROM TB_NHANVIEN_PHUCAP np
                JOIN TB_PHUCAP p ON np.IDPC = p.IDPC
                WHERE np.MANV = :p0 AND np.SOTIEN > 0
                  AND np.DELETED_DATE IS NULL
                  AND (np.TU_NGAY IS NULL OR np.TU_NGAY <= :p1)
                  AND (np.DEN_NGAY IS NULL OR np.DEN_NGAY >= :p2)",
                new OracleParameter("p0", manv),
                new OracleParameter("p1", endOfMonth),
                new OracleParameter("p2", startOfMonth)
            ).ToList();

            foreach (var p in pcs)
            {
                string nameLower = (p.TENPC ?? "").ToLower();
                bool isChuyenCan = nameLower.Contains("chuyên cần") || nameLower.Contains("chuyen can");
                bool isAnCa = nameLower.Contains("ăn ca") || nameLower.Contains("an ca") || nameLower.Contains("cơm");
                bool isTrachNhiem = nameLower.Contains("trách nhiệm") || nameLower.Contains("chức vụ") || nameLower.Contains("thâm niên");

                // Under Circular 59/2015 & Decree 158/2025: Welfare/allowances (lunch, attendance, travel) excluded from BHXH.
                int tinhBhxh = isTrachNhiem ? 1 : 0;
                int tinhThue = 1;
                // Circular 26/2015/TT-BTC: Lunch allowance exempt up to 730,000 VND/month. Attendance bonus is fully taxable.
                decimal? mienThue = isAnCa ? Math.Min(p.SOTIEN, 730000m) : 0m;

                input.Allowances.Add(new AllowanceItemInput
                {
                    IDPC = p.IDPC,
                    TENPC = p.TENPC,
                    SOTIEN = p.SOTIEN,
                    CACH_TINH = p.CACH_TINH,
                    TINH_BHXH = tinhBhxh,
                    TINH_THUE = tinhThue,
                    SO_TIEN_MIEN_THUE = mienThue,
                    SOURCE_NVPC_ID = p.IDPC
                });
            }

            // The published day version is the only attendance quantity used by the new payroll.
            var daySources = db.Database.SqlQuery<PublishedDayRow>(@"
                SELECT IDBANGCONGCT, LANTINH_ID_HIENHANH, NVL(NGAYCONG,0) NGAYCONG
                FROM TB_BANGCONG_CHITIET WHERE MANV=:p0 AND MAKYCONG=:p1
                  AND LANTINH_ID_HIENHANH IS NOT NULL ORDER BY NGAY",
                new OracleParameter("p0",manv), new OracleParameter("p1",makycong)).ToList();
            input.AttendanceSources = daySources.Select(x => new PayrollDetailSourceDto {
                MANV=manv, SOURCE_TYPE="BANGCONG", SOURCE_BCCT_ID=x.IDBANGCONGCT,
                SOURCE_CONG_LANTINH_ID=x.LANTINH_ID_HIENHANH,WEIGHT_QUANTITY=x.NGAYCONG
            }).ToList();

            if ((publishCheck.SO_NGAY_CO_CONGBO ?? 0) > 0)
            {
                var slices = db.Database.SqlQuery<PublishedOtRow>(@"
                    SELECT p.IDBANGCONGCT,p.IDLANTINH,p.BATDAU_LUC,p.LOAI_NGAY,p.LA_BAN_DEM,
                           p.CO_OT_BAN_NGAY_TRUOC,p.GIAY_OT_XAC_NHAN
                    FROM TB_CONG_PHANDOAN p JOIN TB_BANGCONG_CHITIET b
                      ON b.IDBANGCONGCT=p.IDBANGCONGCT AND b.MANV=p.MANV
                     AND b.LANTINH_ID_HIENHANH=p.IDLANTINH
                    WHERE b.MANV=:p0 AND b.MAKYCONG=:p1 AND p.GIAY_OT_XAC_NHAN>0
                    ORDER BY p.BATDAU_LUC",
                    new OracleParameter("p0",manv),new OracleParameter("p1",makycong)).ToList();
                if (slices.Count > 0)
                {
                    var rules=db.Database.SqlQuery<PublishedOtRule>(@"SELECT ID,LOAICONG,
                        LA_BAN_DEM,CO_OT_BAN_NGAY_TRUOC,HESO_CHINHTHUC,TU_NGAY,DEN_NGAY
                        FROM TB_HESO_TANGCA WHERE TRANG_THAI='PUBLISHED'").ToList();
                    var payPolicy = db.Database.SqlQuery<SalaryPolicyDto>(@"SELECT * FROM TB_CHINH_SACH_LUONG
                        WHERE TRANG_THAI='ACTIVE' AND NGAY_HIEU_LUC<=:p0
                        AND (NGAY_HET_HIEU_LUC IS NULL OR NGAY_HET_HIEU_LUC>=:p0)",
                        new OracleParameter("p0",new DateTime(nam,thang,1))).Single();
                    decimal rate=Math.Round(baseSalary/payPolicy.SO_CONG_CHUAN_THANG,2)/payPolicy.SO_GIO_CHUAN_NGAY;
                    foreach(var slice in slices)
                    {
                        var rule=rules.Where(x=>x.LOAICONG==slice.LOAI_NGAY && x.LA_BAN_DEM==slice.LA_BAN_DEM
                            && (x.CO_OT_BAN_NGAY_TRUOC==null || x.CO_OT_BAN_NGAY_TRUOC==slice.CO_OT_BAN_NGAY_TRUOC)
                            && x.TU_NGAY<=slice.BATDAU_LUC && (!x.DEN_NGAY.HasValue || slice.BATDAU_LUC<x.DEN_NGAY)).SingleOrDefault();
                        if(rule==null) throw new InvalidOperationException("Missing unambiguous effective OT rule.");
                        decimal hours=slice.GIAY_OT_XAC_NHAN/3600m;
                        input.Overtimes.Add(new OvertimeItemInput {
                            SOURCE_BCCT_ID=slice.IDBANGCONGCT,SOURCE_CONG_LANTINH_ID=slice.IDLANTINH,
                            POLICY_OT_ID=rule.ID,SOGIO=hours,HESOTC=rule.HESO_CHINHTHUC,DONGIATC=rate,
                            SOTIENTC=Math.Round(hours*rate*rule.HESO_CHINHTHUC,2)
                        });
                    }
                }
            }

            // Advances from TB_UNGLUONG (filter soft-deleted records)
            var uls = db.Database.SqlQuery<AdvanceQueryRow>(@"
                SELECT IDUL, SOTIENUNG
                FROM TB_UNGLUONG
                WHERE MANV = :p0 AND NAM = :p1 AND THANG = :p2
                  AND DELETED_DATE IS NULL",
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

            // Rewards and Disciplines from TB_KHENTHUONG_KYLUAT
            // Only APPROVED occurrences for this period are loaded into payroll.
            // Drafts, revoked occurrences, or unapproved records are strictly excluded.
            bool hasTrangThaiCol = CheckTrangThaiColumnInKtkl(db);
            string approvalCondition = hasTrangThaiCol
                ? @"AND k.TRANG_THAI = 'APPROVED'"
                : @"AND EXISTS (
                        SELECT 1 FROM (
                            SELECT ID_BAN_GHI, HANHDONG, ROW_NUMBER() OVER (PARTITION BY ID_BAN_GHI ORDER BY THOIGIAN DESC, ID_LOG DESC) as rn
                            FROM TB_SYS_LOG
                            WHERE TEN_BANG = 'TB_KHENTHUONG_KYLUAT'
                              AND HANHDONG IN ('DUYET_PHATSINH', 'THU_HOI_PHATSINH', 'THEM_PHATSINH_NHAP', 'SUA_PHATSINH_NHAP')
                        ) sl
                        WHERE sl.rn = 1 AND sl.HANHDONG = 'DUYET_PHATSINH' AND sl.ID_BAN_GHI = k.SOQUYETDINH
                   )";

            string sqlKtkl = $@"
                SELECT k.SOQUYETDINH, k.LOAI, NVL(k.SOTIEN, 0) AS SOTIEN, k.LYDO
                FROM TB_KHENTHUONG_KYLUAT k
                WHERE k.MANV = :p0 AND k.DELETED_DATE IS NULL
                  {approvalCondition}
                  AND (
                      (k.NAM_APDUNG = :p1 AND k.THANG_APDUNG = :p2)
                      OR (k.NAM_APDUNG IS NULL AND k.THANG_APDUNG IS NULL AND k.NGAY >= :p3 AND k.NGAY < :p4)
                  )";

            var ktkls = db.Database.SqlQuery<RewardDisciplineQueryRow>(
                sqlKtkl,
                new OracleParameter("p0", manv),
                new OracleParameter("p1", nam),
                new OracleParameter("p2", thang),
                new OracleParameter("p3", startOfMonth),
                new OracleParameter("p4", startOfMonth.AddMonths(1))
            ).ToList();

            foreach (var item in ktkls)
            {
                input.RewardsAndDisciplines.Add(new RewardDisciplineInput
                {
                    SOQUYETDINH = item.SOQUYETDINH,
                    LOAI = item.LOAI,
                    SOTIEN = item.SOTIEN,
                    LYDO = item.LYDO
                });
            }

            // YTD Overtime prior in this year
            decimal priorStartPeriod = nam * 100 + 1;
            var ytdPriorHours = db.Database.SqlQuery<decimal?>(@"
                SELECT NVL(SUM(NVL(GIAY_OT_XAC_NHAN, 0)), 0) / 3600.0
                FROM TB_BANGCONG_CHITIET
                WHERE MANV = :p0 AND MAKYCONG >= :p1 AND MAKYCONG < :p2
                  AND LANTINH_ID_HIENHANH IS NOT NULL",
                new OracleParameter("p0", manv),
                new OracleParameter("p1", priorStartPeriod),
                new OracleParameter("p2", makycong)
            ).FirstOrDefault();
            input.YtdOvertimeHoursPrior = Math.Round(ytdPriorHours ?? 0m, 2);

            // Overtime consent and notification status (Labor Code 2019 Art 107 & Decree 145/2020)
            input.EmployeeOtConsent = 1;
            input.ExtendedOtEligible = 1;
            input.OtNotificationFiled = 1;

            return input;
        }

        private sealed class AttendanceRevisionRow
        {
            public decimal INPUT_REV { get; set; }
            public decimal PUBLISH_REV { get; set; }
        }

        private static AttendanceRevisionRow ReadAttendanceRevision(MyEntities db, decimal period, bool forUpdate)
        {
            return db.Database.SqlQuery<AttendanceRevisionRow>(
                "SELECT NVL(CONG_INPUT_REV,0) INPUT_REV,NVL(CONG_PUBLISH_REV,0) PUBLISH_REV FROM TB_KYCONG WHERE MAKYCONG=:p0" +
                (forUpdate ? " FOR UPDATE" : ""),
                new OracleParameter("p0",period)).Single();
        }

        private static OracleParameter Param(string name, object value)
        {
            return new OracleParameter(name, value ?? DBNull.Value);
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
            // 1. Check previous record
            ExistingBlRow existingBl = db.Database.SqlQuery<ExistingBlRow>(@"
                SELECT IDBL FROM TB_BANGLUONG
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
                    SET CONG_CHUAN = :p0,
                        CONG_THUCTE = :p1,
                        CONG_LAMNGAY = :p2,
                        CONG_LAMDEM = :p3,
                        TONG_CONG = :p4,
                        DAILY_RATE = :p5,
                        DAILY_ALLOWANCE = :p6,
                        LUONG_CONG_THUCTE = :p7,
                        PHUCAP_CONG_THUCTE = :p8,
                        TIEN_TANGCA = :p9,
                        TIEN_CHUYENCAN = :p10,
                        TIEN_AN_CA = :p11,
                        KHOAN_CONG_KHAC = :p12,
                        TIEN_BHXH_TRICH = :p13,
                        TIEN_TAMUNG = :p14,
                        KHOAN_TRU_KHAC = :p15,
                        THUC_LINH = :p16,
                        LUONG_BHXH = :p17,
                        TIEN_BHXH = :p18,
                        TIEN_BHYT = :p19,
                        TIEN_BHTN = :p20,
                        TIEN_CONG_DOAN = :p21,
                        THUE_TNCN = :p22,
                        HOAN_THUE = :p23,
                        IS_LEGACY = 0,
                        TRANG_THAI = 'CALCULATED',
                        POLICY_LUONG_ID = :p24,
                        POLICY_BHXH_ID = :p25,
                        POLICY_CONGDOAN_ID = :p26,
                        POLICY_THUE_ID = :p27,
                        PROFILE_BH_ID = :p28,
                        PROFILE_CD_ID = :p29,
                        PROFILE_THUE_ID = :p30,
                        VUNG_LUONG = :p31,
                        LUONG_TOI_THIEU_VUNG = :p32,
                        MUC_THAM_CHIEU_BH = :p33,
                        LUONG_DONG_BHXH = :p34,
                        TIEN_BHYT_NLD = :p35,
                        TIEN_BHTN_NLD = :p36,
                        TIEN_BHXH_NSDLD = :p37,
                        TIEN_BHYT_NSDLD = :p38,
                        TIEN_BHTN_NSDLD = :p39,
                        TIEN_TNLD_BNN_NSDLD = :p40,
                        TIEN_DOAN_PHI_NLD = :p41,
                        TIEN_KINH_PHI_CD_NSDLD = :p42,
                        SO_NGUOI_PHU_THUOC = :p43,
                        GIAM_TRU_BAN_THAN = :p44,
                        GIAM_TRU_PHU_THUOC = :p45,
                        GIAM_TRU_BAO_HIEM = :p46,
                        TONG_THU_NHAP_CHIU_THUE = :p47,
                        THU_NHAP_TINH_THUE = :p48,
                        TONG_CHI_PHI_NSDLD = :p49,
                        RUN_ID = :p50,
                        CALCULATED_AT = SYSTIMESTAMP
                    WHERE IDBL = :p51",
                    Param("p0", res.CongChuan),
                    Param("p1", res.CongThucTe),
                    Param("p2", res.CongLamNgay),
                    Param("p3", res.CongLamDem),
                    Param("p4", res.TongCong),
                    Param("p5", res.DailyRate),
                    Param("p6", res.DailyAllowance),
                    Param("p7", res.LuongCongThucTe),
                    Param("p8", res.PhuCapCongThucTe),
                    Param("p9", res.TienTangCa),
                    Param("p10", res.TienChuyenCan),
                    Param("p11", res.TienAnCa),
                    Param("p12", res.KhoanCongKhac),
                    Param("p13", res.InsuranceEmployee),
                    Param("p14", res.TienTamUng),
                    Param("p15", res.KhoanTruKhac),
                    Param("p16", res.NetPay),
                    Param("p17", res.InsuranceTrace.LUONG_DONG_BHXH_AP_DUNG),
                    Param("p18", res.InsuranceTrace.TIEN_BHXH_NLD),
                    Param("p19", res.InsuranceTrace.TIEN_BHYT_NLD),
                    Param("p20", res.InsuranceTrace.TIEN_BHTN_NLD),
                    Param("p21", res.UnionEmployee),
                    Param("p22", res.TaxWithheld),
                    Param("p23", 0m),
                    Param("p24", salaryPolicy.ID),
                    Param("p25", insurancePolicy.ID),
                    Param("p26", unionPolicy.ID),
                    Param("p27", taxPolicy.ID),
                    Param("p28", insProfile.ID),
                    Param("p29", unionProfile.ID),
                    Param("p30", taxProfile.ID),
                    Param("p31", insProfile.VUNG_LUONG),
                    Param("p32", res.InsuranceTrace.LUONG_TOI_THIEU_VUNG),
                    Param("p33", insurancePolicy.MUC_THAM_CHIEU),
                    Param("p34", res.InsuranceTrace.LUONG_DONG_BHXH_AP_DUNG),
                    Param("p35", res.InsuranceTrace.TIEN_BHYT_NLD),
                    Param("p36", res.InsuranceTrace.TIEN_BHTN_NLD),
                    Param("p37", res.InsuranceTrace.TIEN_BHXH_NSDLD),
                    Param("p38", res.InsuranceTrace.TIEN_BHYT_NSDLD),
                    Param("p39", res.InsuranceTrace.TIEN_BHTN_NSDLD),
                    Param("p40", res.InsuranceTrace.TIEN_TNLD_BNN_NSDLD),
                    Param("p41", res.UnionEmployee),
                    Param("p42", res.UnionEmployer),
                    Param("p43", dependentCount),
                    Param("p44", taxPolicy.GIAM_TRU_BAN_THAN_THANG),
                    Param("p45", dependentCount * taxPolicy.GIAM_TRU_PHU_THUOC_THANG),
                    Param("p46", res.InsuranceEmployee),
                    Param("p47", res.TaxTrace != null ? res.TaxTrace.GrossTaxableIncome : res.GrossEarnings),
                    Param("p48", res.TaxTrace != null ? res.TaxTrace.TaxableAssessableIncome : Math.Max(0m, res.GrossEarnings - res.InsuranceEmployee - taxPolicy.GIAM_TRU_BAN_THAN_THANG - (dependentCount * taxPolicy.GIAM_TRU_PHU_THUOC_THANG))),
                    Param("p49", res.TotalEmployerCost),
                    Param("p50", runId),
                    Param("p51", idbl)
                );
            }
            else
            {
                // Insert new TB_BANGLUONG row
                decimal nextIdbl = db.Database.SqlQuery<decimal>("SELECT NVL(MAX(IDBL), 0) + 1 FROM TB_BANGLUONG").First();
                idbl = nextIdbl;
                res.IDBL = idbl;

                int thang = (int)(res.MAKYCONG % 100);
                int nam = (int)(res.MAKYCONG / 100);

                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_BANGLUONG (
                        IDBL, MANV, MAKYCONG, THANG, NAM,
                        CONG_CHUAN, CONG_THUCTE, CONG_LAMNGAY, CONG_LAMDEM, TONG_CONG,
                        DAILY_RATE, DAILY_ALLOWANCE, LUONG_CONG_THUCTE, PHUCAP_CONG_THUCTE,
                        TIEN_TANGCA, TIEN_CHUYENCAN, TIEN_AN_CA, KHOAN_CONG_KHAC,
                        TIEN_BHXH_TRICH, TIEN_TAMUNG, KHOAN_TRU_KHAC, THUC_LINH,
                        LUONG_BHXH, TIEN_BHXH, TIEN_BHYT, TIEN_BHTN, TIEN_CONG_DOAN,
                        THUE_TNCN, HOAN_THUE, IS_LEGACY, TRANG_THAI,
                        POLICY_LUONG_ID, POLICY_BHXH_ID, POLICY_CONGDOAN_ID, POLICY_THUE_ID,
                        PROFILE_BH_ID, PROFILE_CD_ID, PROFILE_THUE_ID, VUNG_LUONG,
                        LUONG_TOI_THIEU_VUNG, MUC_THAM_CHIEU_BH, LUONG_DONG_BHXH,
                        TIEN_BHYT_NLD, TIEN_BHTN_NLD, TIEN_BHXH_NSDLD, TIEN_BHYT_NSDLD,
                        TIEN_BHTN_NSDLD, TIEN_TNLD_BNN_NSDLD, TIEN_DOAN_PHI_NLD,
                        TIEN_KINH_PHI_CD_NSDLD, SO_NGUOI_PHU_THUOC, GIAM_TRU_BAN_THAN,
                        GIAM_TRU_PHU_THUOC, GIAM_TRU_BAO_HIEM, TONG_THU_NHAP_CHIU_THUE,
                        THU_NHAP_TINH_THUE, TONG_CHI_PHI_NSDLD, RUN_ID, CALCULATED_AT
                    ) VALUES (
                        :p0, :p1, :p2, :p3, :p4,
                        :p5, :p6, :p7, :p8, :p9,
                        :p10, :p11, :p12, :p13,
                        :p14, :p15, :p16, :p17,
                        :p18, :p19, :p20, :p21,
                        :p22, :p23, :p24, :p25, :p26,
                        :p27, :p28, 0, 'CALCULATED',
                        :p29, :p30, :p31, :p32,
                        :p33, :p34, :p35, :p36,
                        :p37, :p38, :p39,
                        :p40, :p41, :p42, :p43,
                        :p44, :p45, :p46,
                        :p47, :p48, :p49,
                        :p50, :p51, :p52,
                        :p53, :p54, :p55, SYSTIMESTAMP
                    )",
                    Param("p0", idbl),
                    Param("p1", res.MANV),
                    Param("p2", res.MAKYCONG),
                    Param("p3", thang),
                    Param("p4", nam),
                    Param("p5", res.CongChuan),
                    Param("p6", res.CongThucTe),
                    Param("p7", res.CongLamNgay),
                    Param("p8", res.CongLamDem),
                    Param("p9", res.TongCong),
                    Param("p10", res.DailyRate),
                    Param("p11", res.DailyAllowance),
                    Param("p12", res.LuongCongThucTe),
                    Param("p13", res.PhuCapCongThucTe),
                    Param("p14", res.TienTangCa),
                    Param("p15", res.TienChuyenCan),
                    Param("p16", res.TienAnCa),
                    Param("p17", res.KhoanCongKhac),
                    Param("p18", res.InsuranceEmployee),
                    Param("p19", res.TienTamUng),
                    Param("p20", res.KhoanTruKhac),
                    Param("p21", res.NetPay),
                    Param("p22", res.InsuranceTrace.LUONG_DONG_BHXH_AP_DUNG),
                    Param("p23", res.InsuranceTrace.TIEN_BHXH_NLD),
                    Param("p24", res.InsuranceTrace.TIEN_BHYT_NLD),
                    Param("p25", res.InsuranceTrace.TIEN_BHTN_NLD),
                    Param("p26", res.UnionEmployee),
                    Param("p27", res.TaxWithheld),
                    Param("p28", 0m),
                    Param("p29", salaryPolicy.ID),
                    Param("p30", insurancePolicy.ID),
                    Param("p31", unionPolicy.ID),
                    Param("p32", taxPolicy.ID),
                    Param("p33", insProfile.ID),
                    Param("p34", unionProfile.ID),
                    Param("p35", taxProfile.ID),
                    Param("p36", insProfile.VUNG_LUONG),
                    Param("p37", res.InsuranceTrace.LUONG_TOI_THIEU_VUNG),
                    Param("p38", insurancePolicy.MUC_THAM_CHIEU),
                    Param("p39", res.InsuranceTrace.LUONG_DONG_BHXH_AP_DUNG),
                    Param("p40", res.InsuranceTrace.TIEN_BHYT_NLD),
                    Param("p41", res.InsuranceTrace.TIEN_BHTN_NLD),
                    Param("p42", res.InsuranceTrace.TIEN_BHXH_NSDLD),
                    Param("p43", res.InsuranceTrace.TIEN_BHYT_NSDLD),
                    Param("p44", res.InsuranceTrace.TIEN_BHTN_NSDLD),
                    Param("p45", res.InsuranceTrace.TIEN_TNLD_BNN_NSDLD),
                    Param("p46", res.UnionEmployee),
                    Param("p47", res.UnionEmployer),
                    Param("p48", dependentCount),
                    Param("p49", taxPolicy.GIAM_TRU_BAN_THAN_THANG),
                    Param("p50", dependentCount * taxPolicy.GIAM_TRU_PHU_THUOC_THANG),
                    Param("p51", res.InsuranceEmployee),
                    Param("p52", res.TaxTrace != null ? res.TaxTrace.GrossTaxableIncome : res.GrossEarnings),
                    Param("p53", res.TaxTrace != null ? res.TaxTrace.TaxableAssessableIncome : Math.Max(0m, res.GrossEarnings - res.InsuranceEmployee - taxPolicy.GIAM_TRU_BAN_THAN_THANG - (dependentCount * taxPolicy.GIAM_TRU_PHU_THUOC_THANG))),
                    Param("p54", res.TotalEmployerCost),
                    Param("p55", runId)
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
                    Param("p0", idblct),
                    Param("p1", idbl),
                    Param("p2", res.MANV),
                    Param("p3", res.MAKYCONG),
                    Param("p4", item.NHOM_KHOAN_MUC),
                    Param("p5", item.MA_KHOAN_MUC),
                    Param("p6", item.TEN_KHOAN_MUC),
                    Param("p7", item.SO_LUONG),
                    Param("p8", item.DON_GIA),
                    Param("p9", item.HE_SO),
                    Param("p10", item.THANH_TIEN),
                    Param("p11", item.TINH_VAO_DONG_BHXH),
                    Param("p12", item.TINH_THUE_TNCN),
                    Param("p13", item.SO_TIEN_MIEN_THUE),
                    Param("p14", item.SO_TIEN_CHIU_THUE),
                    Param("p15", item.CONG_THUC_DIEN_GIAI)
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
                        Param("p0", srcId),
                        Param("p1", idblct),
                        Param("p2", res.MANV),
                        Param("p3", src.SOURCE_TYPE),
                        Param("p4", src.SOURCE_BCCT_ID),
                        Param("p5", src.SOURCE_TC_ID),
                        Param("p6", src.SOURCE_NVPC_ID),
                        Param("p7", src.SOURCE_KTKL_SOQD),
                        Param("p8", src.SOURCE_UL_ID),
                        Param("p9", src.WEIGHT_QUANTITY),
                        Param("p10", src.AMOUNT_CONTRIBUTED)
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
                    Param("p0", otId),
                    Param("p1", idbl),
                    Param("p2", res.MANV),
                    Param("p3", res.MAKYCONG),
                    Param("p4", res.OtCompliance.MONTHLY_STANDARD_LIMIT),
                    Param("p5", res.OtCompliance.ANNUAL_STANDARD_LIMIT),
                    Param("p6", res.OtCompliance.ANNUAL_EXTENDED_LIMIT),
                    Param("p7", res.OtCompliance.ACTUAL_HOURS_MONTH),
                    Param("p8", res.OtCompliance.ACTUAL_HOURS_YTD),
                    Param("p9", res.OtCompliance.LEGAL_HOURS_MONTH),
                    Param("p10", res.OtCompliance.EXCESS_HOURS_MONTH),
                    Param("p11", res.OtCompliance.EXTENDED_ELIGIBLE),
                    Param("p12", res.OtCompliance.EMPLOYEE_CONSENT),
                    Param("p13", res.OtCompliance.NOTIFICATION_FILED),
                    Param("p14", res.OtCompliance.COMPLIANCE_STATUS),
                    Param("p15", res.OtCompliance.TOTAL_OT_PAYMENT),
                    Param("p16", res.OtCompliance.LEGAL_ALLOWED_PAYMENT),
                    Param("p17", res.OtCompliance.EXEMPT_OT_PAYMENT),
                    Param("p18", res.OtCompliance.TAXABLE_EXCESS_PAYMENT)
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
                    Param("p0", insId),
                    Param("p1", idbl),
                    Param("p2", res.MANV),
                    Param("p3", res.MAKYCONG),
                    Param("p4", res.InsuranceTrace.POLICY_BHXH_ID),
                    Param("p5", res.InsuranceTrace.PROFILE_BH_ID),
                    Param("p6", res.InsuranceTrace.MUC_THAM_CHIEU),
                    Param("p7", res.InsuranceTrace.VUNG_LUONG),
                    Param("p8", res.InsuranceTrace.LUONG_TOI_THIEU_VUNG),
                    Param("p9", res.InsuranceTrace.LUONG_DONG_BHXH_GOC),
                    Param("p10", res.InsuranceTrace.LUONG_DONG_BHXH_AP_DUNG),
                    Param("p11", res.InsuranceTrace.LUONG_DONG_BHTN_AP_DUNG),
                    Param("p12", res.InsuranceTrace.TY_LE_BHXH_NLD),
                    Param("p13", res.InsuranceTrace.TIEN_BHXH_NLD),
                    Param("p14", res.InsuranceTrace.TY_LE_BHYT_NLD),
                    Param("p15", res.InsuranceTrace.TIEN_BHYT_NLD),
                    Param("p16", res.InsuranceTrace.TY_LE_BHTN_NLD),
                    Param("p17", res.InsuranceTrace.TIEN_BHTN_NLD),
                    Param("p18", res.InsuranceTrace.TONG_BH_NLD),
                    Param("p19", res.InsuranceTrace.TY_LE_BHXH_NSDLD),
                    Param("p20", res.InsuranceTrace.TIEN_BHXH_NSDLD),
                    Param("p21", res.InsuranceTrace.TY_LE_BHYT_NSDLD),
                    Param("p22", res.InsuranceTrace.TIEN_BHYT_NSDLD),
                    Param("p23", res.InsuranceTrace.TY_LE_BHTN_NSDLD),
                    Param("p24", res.InsuranceTrace.TIEN_BHTN_NSDLD),
                    Param("p25", res.InsuranceTrace.TY_LE_TNLD_BNN_NSDLD),
                    Param("p26", res.InsuranceTrace.TIEN_TNLD_BNN_NSDLD),
                    Param("p27", res.InsuranceTrace.TONG_BH_NSDLD)
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
                    Param("p0", unionId),
                    Param("p1", idbl),
                    Param("p2", res.MANV),
                    Param("p3", res.MAKYCONG),
                    Param("p4", res.UnionTrace.POLICY_CD_ID),
                    Param("p5", res.UnionTrace.PROFILE_CD_ID),
                    Param("p6", res.UnionTrace.LA_DOAN_VIEN),
                    Param("p7", res.UnionTrace.LUONG_CAN_CU_DONG),
                    Param("p8", res.UnionTrace.TY_LE_DOAN_PHI_NLD),
                    Param("p9", res.UnionTrace.MUC_TRAN_DOAN_PHI),
                    Param("p10", res.UnionTrace.TIEN_DOAN_PHI_NLD),
                    Param("p11", res.UnionTrace.TY_LE_KPCD_NSDLD),
                    Param("p12", res.UnionTrace.TIEN_KPCD_NSDLD)
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
                    Param("p0", taxId),
                    Param("p1", idbl),
                    Param("p2", res.MANV),
                    Param("p3", res.MAKYCONG),
                    Param("p4", t.POLICY_THUE_ID),
                    Param("p5", t.BAC_THUE),
                    Param("p6", t.CAN_DUOI),
                    Param("p7", t.CAN_TREN),
                    Param("p8", t.THU_NHAP_CHIU_THUE_BAC),
                    Param("p9", t.THUE_SUAT),
                    Param("p10", t.TIEN_THUE_BAC)
                );
            }
        }

        private class PublishedDayRow
        {
            public decimal IDBANGCONGCT {get;set;}
            public decimal LANTINH_ID_HIENHANH {get;set;}
            public decimal NGAYCONG {get;set;}
        }
        private class PublishedOtRow
        {
            public decimal IDBANGCONGCT {get;set;}
            public decimal IDLANTINH {get;set;}
            public DateTime BATDAU_LUC {get;set;}
            public string LOAI_NGAY {get;set;}
            public int LA_BAN_DEM {get;set;}
            public int CO_OT_BAN_NGAY_TRUOC {get;set;}
            public decimal GIAY_OT_XAC_NHAN {get;set;}
        }
        private class PublishedOtRule
        {
            public decimal ID {get;set;}
            public string LOAICONG {get;set;}
            public int LA_BAN_DEM {get;set;}
            public int? CO_OT_BAN_NGAY_TRUOC {get;set;}
            public decimal HESO_CHINHTHUC {get;set;}
            public DateTime TU_NGAY {get;set;}
            public DateTime? DEN_NGAY {get;set;}
        }

        private class EmployeeAttendanceRow
        {
            public decimal MANV { get; set; }
            public string HOTEN { get; set; }
            public decimal? BASE_SALARY { get; set; }
            public decimal CONTRACT_COUNT { get; set; }
            public DateTime CONTRACT_START { get; set; }
            public DateTime? CONTRACT_END { get; set; }
            public decimal? IDPB { get; set; }
            public decimal? IDCV { get; set; }
        }

        private class ContractDateRow
        {
            public string SOHD { get; set; }
            public DateTime NGAYBATDAU { get; set; }
            public DateTime? NGAYKETTHUC { get; set; }
            public decimal? LUONG_THOA_THUAN { get; set; }
        }

        private class AttendanceSummaryRow
        {
            public decimal? TONGNGAYCONG { get; set; }
            public decimal? NGAYPHEP { get; set; }
            public decimal? CONGCHUNHAT { get; set; }
        }

        private class AttendancePublishCheckRow
        {
            public decimal? TONG_SO_NGAY { get; set; }
            public decimal? SO_NGAY_CO_CONGBO { get; set; }
            public decimal? SO_NGAY_CHUA_CONGBO { get; set; }
            public decimal? SO_NGAY_CHUA_CHOT { get; set; }
            public decimal? TONG_GIAY_OT { get; set; }
            public decimal? TONG_GIAY_DEM { get; set; }
        }

        private class AllowanceQueryRow
        {
            public decimal IDPC { get; set; }
            public string TENPC { get; set; }
            public decimal SOTIEN { get; set; }
            public string CACH_TINH { get; set; }
        }

        private class RewardDisciplineQueryRow
        {
            public string SOQUYETDINH { get; set; }
            public decimal LOAI { get; set; }
            public decimal SOTIEN { get; set; }
            public string LYDO { get; set; }
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
