using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.CLASS_PAYROLL
{
    public interface IAnnualTaxFinalizationService
    {
        AnnualTaxFinalizationDto CalculateAnnualTaxFinalization(
            decimal manv,
            int taxYear,
            TaxPolicyDto policy,
            List<TaxBracketDto> yearBrackets,
            EmployeeTaxProfileDto profile
        );

        AnnualTaxFinalizationDto SaveAnnualTaxFinalization(AnnualTaxFinalizationDto finalization);
    }

    public class AnnualTaxFinalizationService : IAnnualTaxFinalizationService
    {
        public AnnualTaxFinalizationDto CalculateAnnualTaxFinalization(
            decimal manv,
            int taxYear,
            TaxPolicyDto policy,
            List<TaxBracketDto> yearBrackets,
            EmployeeTaxProfileDto profile)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (yearBrackets == null || yearBrackets.Count == 0) throw new ArgumentNullException(nameof(yearBrackets));
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            using (var db = new MyEntities())
            {
                // Query all payroll records of the employee in the tax year
                var monthlyPayrolls = db.Database.SqlQuery<MonthlyTaxSummaryRow>(@"
                    SELECT IDBL, THANG, TONG_THU_NHAP_CHIU_THUE, GIAM_TRU_BAO_HIEM, THUE_TNCN, SO_NGUOI_PHU_THUOC
                    FROM TB_BANGLUONG
                    WHERE MANV = :p0 AND NAM = :p1
                    ORDER BY THANG",
                    new OracleParameter("p0", manv),
                    new OracleParameter("p1", taxYear)
                ).ToList();

                int workMonths = monthlyPayrolls.Count;
                decimal totalGrossTaxable = monthlyPayrolls.Sum(p => p.TONG_THU_NHAP_CHIU_THUE ?? 0m);
                decimal totalInsuranceDeduction = monthlyPayrolls.Sum(p => p.GIAM_TRU_BAO_HIEM ?? 0m);
                decimal totalTaxWithheld = monthlyPayrolls.Sum(p => p.THUE_TNCN ?? 0m);

                // Personal and dependent deductions for the full year
                decimal personalDeduction = policy.GIAM_TRU_BAN_THAN_NAM;
                int maxDependents = monthlyPayrolls.Count > 0 ? (int)monthlyPayrolls.Max(p => p.SO_NGUOI_PHU_THUOC ?? 0) : 0;
                decimal dependentDeduction = maxDependents * policy.GIAM_TRU_PHU_THUOC_NAM;

                decimal totalDeductions = personalDeduction + dependentDeduction + totalInsuranceDeduction;
                decimal taxableAssessableIncome = Math.Max(0m, totalGrossTaxable - totalDeductions);

                var result = new AnnualTaxFinalizationDto
                {
                    MANV = manv,
                    TAX_YEAR = taxYear,
                    POLICY_THUE_ID = policy.ID,
                    PROFILE_THUE_ID = profile.ID,
                    SO_THANG_LAM_VIEC = workMonths > 0 ? workMonths : 12,
                    TONG_THU_NHAP_CHIU_THUE = totalGrossTaxable,
                    GIAM_TRU_BAN_THAN = personalDeduction,
                    GIAM_TRU_PHU_THUOC = dependentDeduction,
                    GIAM_TRU_BAO_HIEM = totalInsuranceDeduction,
                    GIAM_TRU_KHAC = 0m,
                    TONG_GIAM_TRU = totalDeductions,
                    THU_NHAP_TINH_THUE = taxableAssessableIncome,
                    THUE_DA_KHAU_TRU = totalTaxWithheld,
                    TRANG_THAI = "CALCULATED"
                };

                // Compute 5 annual progressive brackets
                decimal totalAnnualTax = 0m;
                var orderedBrackets = yearBrackets.OrderBy(b => b.BAC_THUE).ToList();

                foreach (var b in orderedBrackets)
                {
                    decimal canDuoi = b.CAN_DUOI;
                    decimal canTren = b.CAN_TREN ?? decimal.MaxValue;

                    if (taxableAssessableIncome > canDuoi)
                    {
                        decimal incomeInBracket = Math.Min(taxableAssessableIncome, canTren) - canDuoi;
                        decimal taxInBracket = Math.Round(incomeInBracket * b.THUE_SUAT, 2);
                        totalAnnualTax += taxInBracket;

                        result.Brackets.Add(new AnnualTaxFinalizationBracketDto
                        {
                            MANV = manv,
                            TAX_YEAR = taxYear,
                            BAC_THUE = b.BAC_THUE,
                            CAN_DUOI = canDuoi,
                            CAN_TREN = b.CAN_TREN,
                            THU_NHAP_CHIU_THUE_BAC = incomeInBracket,
                            THUE_SUAT = b.THUE_SUAT,
                            TIEN_THUE_BAC = taxInBracket
                        });
                    }
                }

                result.TONG_THUE_PHAI_NOP = totalAnnualTax;

                if (totalAnnualTax > totalTaxWithheld)
                {
                    result.THUE_CON_PHAI_NOP = totalAnnualTax - totalTaxWithheld;
                    result.THUE_NOP_THUA = 0m;
                }
                else
                {
                    result.THUE_CON_PHAI_NOP = 0m;
                    result.THUE_NOP_THUA = totalTaxWithheld - totalAnnualTax;
                }

                return result;
            }
        }

        public AnnualTaxFinalizationDto SaveAnnualTaxFinalization(AnnualTaxFinalizationDto finalization)
        {
            if (finalization == null) throw new ArgumentNullException(nameof(finalization));

            using (var db = new MyEntities())
            {
                // Delete previous draft for this employee and tax year if exists
                db.Database.ExecuteSqlCommand(@"
                    DELETE FROM TB_QUYET_TOAN_THUE_NAM_CT WHERE MANV = :p0 AND TAX_YEAR = :p1",
                    new OracleParameter("p0", finalization.MANV),
                    new OracleParameter("p1", finalization.TAX_YEAR)
                );

                db.Database.ExecuteSqlCommand(@"
                    DELETE FROM TB_QUYET_TOAN_THUE_NAM WHERE MANV = :p0 AND TAX_YEAR = :p1",
                    new OracleParameter("p0", finalization.MANV),
                    new OracleParameter("p1", finalization.TAX_YEAR)
                );

                decimal newId = db.Database.SqlQuery<decimal>("SELECT SEQ_QUYET_TOAN_THUE_NAM.NEXTVAL FROM DUAL").First();
                finalization.ID = newId;

                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_QUYET_TOAN_THUE_NAM (
                        ID, MANV, TAX_YEAR, POLICY_THUE_ID, PROFILE_THUE_ID, SO_THANG_LAM_VIEC,
                        TONG_THU_NHAP_CHIU_THUE, GIAM_TRU_BAN_THAN, GIAM_TRU_PHU_THUOC, GIAM_TRU_BAO_HIEM,
                        GIAM_TRU_KHAC, TONG_GIAM_TRU, THU_NHAP_TINH_THUE, TONG_THUE_PHAI_NOP,
                        THUE_DA_KHAU_TRU, THUE_CON_PHAI_NOP, THUE_NOP_THUA, TRANG_THAI, CREATED_AT
                    ) VALUES (
                        :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, :p10, :p11, :p12, :p13, :p14, :p15, :p16, :p17, SYSTIMESTAMP
                    )",
                    new OracleParameter("p0", finalization.ID),
                    new OracleParameter("p1", finalization.MANV),
                    new OracleParameter("p2", finalization.TAX_YEAR),
                    new OracleParameter("p3", finalization.POLICY_THUE_ID),
                    new OracleParameter("p4", finalization.PROFILE_THUE_ID),
                    new OracleParameter("p5", finalization.SO_THANG_LAM_VIEC),
                    new OracleParameter("p6", finalization.TONG_THU_NHAP_CHIU_THUE),
                    new OracleParameter("p7", finalization.GIAM_TRU_BAN_THAN),
                    new OracleParameter("p8", finalization.GIAM_TRU_PHU_THUOC),
                    new OracleParameter("p9", finalization.GIAM_TRU_BAO_HIEM),
                    new OracleParameter("p10", finalization.GIAM_TRU_KHAC),
                    new OracleParameter("p11", finalization.TONG_GIAM_TRU),
                    new OracleParameter("p12", finalization.THU_NHAP_TINH_THUE),
                    new OracleParameter("p13", finalization.TONG_THUE_PHAI_NOP),
                    new OracleParameter("p14", finalization.THUE_DA_KHAU_TRU),
                    new OracleParameter("p15", finalization.THUE_CON_PHAI_NOP),
                    new OracleParameter("p16", finalization.THUE_NOP_THUA),
                    new OracleParameter("p17", finalization.TRANG_THAI)
                );

                foreach (var b in finalization.Brackets)
                {
                    decimal bracketId = db.Database.SqlQuery<decimal>("SELECT SEQ_QUYET_TOAN_THUE_NAM_CT.NEXTVAL FROM DUAL").First();
                    b.ID = bracketId;
                    b.QUYET_TOAN_ID = newId;

                    db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_QUYET_TOAN_THUE_NAM_CT (
                            ID, QUYET_TOAN_ID, MANV, TAX_YEAR, BAC_THUE, CAN_DUOI, CAN_TREN,
                            THU_NHAP_CHIU_THUE_BAC, THUE_SUAT, TIEN_THUE_BAC, CREATED_AT
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, SYSTIMESTAMP
                        )",
                        new OracleParameter("p0", b.ID),
                        new OracleParameter("p1", b.QUYET_TOAN_ID),
                        new OracleParameter("p2", b.MANV),
                        new OracleParameter("p3", b.TAX_YEAR),
                        new OracleParameter("p4", b.BAC_THUE),
                        new OracleParameter("p5", b.CAN_DUOI),
                        new OracleParameter("p6", (object)b.CAN_TREN ?? DBNull.Value),
                        new OracleParameter("p7", b.THU_NHAP_CHIU_THUE_BAC),
                        new OracleParameter("p8", b.THUE_SUAT),
                        new OracleParameter("p9", b.TIEN_THUE_BAC)
                    );
                }

                return finalization;
            }
        }

        private class MonthlyTaxSummaryRow
        {
            public decimal IDBL { get; set; }
            public byte THANG { get; set; }
            public decimal? TONG_THU_NHAP_CHIU_THUE { get; set; }
            public decimal? GIAM_TRU_BAO_HIEM { get; set; }
            public decimal? THUE_TNCN { get; set; }
            public decimal? SO_NGUOI_PHU_THUOC { get; set; }
        }
    }
}
