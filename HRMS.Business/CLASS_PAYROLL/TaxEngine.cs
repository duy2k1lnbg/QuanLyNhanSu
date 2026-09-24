using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.CLASS_PAYROLL
{
    public class TaxCalculationResult
    {
        public decimal TotalTax { get; set; }
        public decimal GrossTaxableIncome { get; set; }
        public decimal PersonalDeduction { get; set; }
        public decimal DependentDeduction { get; set; }
        public decimal InsuranceDeduction { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TaxableAssessableIncome { get; set; }
        public List<PayrollTaxTraceDto> BracketTraces { get; set; } = new List<PayrollTaxTraceDto>();
    }

    public interface ITaxEngine
    {
        TaxCalculationResult CalculateMonthlyTax(
            decimal idbl,
            decimal manv,
            decimal makycong,
            decimal grossTaxableIncome,
            int dependentCount,
            decimal insuranceDeduction,
            TaxPolicyDto policy,
            List<TaxBracketDto> brackets,
            bool isResident = true
        );
    }

    public class TaxEngine : ITaxEngine
    {
        public TaxCalculationResult CalculateMonthlyTax(
            decimal idbl,
            decimal manv,
            decimal makycong,
            decimal grossTaxableIncome,
            int dependentCount,
            decimal insuranceDeduction,
            TaxPolicyDto policy,
            List<TaxBracketDto> brackets,
            bool isResident = true)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (brackets == null || brackets.Count == 0) throw new ArgumentNullException(nameof(brackets));

            var result = new TaxCalculationResult
            {
                GrossTaxableIncome = grossTaxableIncome,
                PersonalDeduction = policy.GIAM_TRU_BAN_THAN_THANG,
                DependentDeduction = dependentCount * policy.GIAM_TRU_PHU_THUOC_THANG,
                InsuranceDeduction = insuranceDeduction
            };

            result.TotalDeductions = result.PersonalDeduction + result.DependentDeduction + result.InsuranceDeduction;
            result.TaxableAssessableIncome = Math.Max(0m, grossTaxableIncome - result.TotalDeductions);

            if (!isResident)
            {
                // Non-resident flat 20% on gross taxable income
                result.TotalTax = Math.Round(grossTaxableIncome * 0.20m, 2);
                return result;
            }

            if (result.TaxableAssessableIncome <= 0m)
            {
                result.TotalTax = 0m;
                return result;
            }

            decimal totalTax = 0m;
            var orderedBrackets = brackets.OrderBy(b => b.BAC_THUE).ToList();

            foreach (var b in orderedBrackets)
            {
                decimal canDuoi = b.CAN_DUOI;
                decimal canTren = b.CAN_TREN ?? decimal.MaxValue;

                if (result.TaxableAssessableIncome > canDuoi)
                {
                    decimal incomeInBracket = Math.Min(result.TaxableAssessableIncome, canTren) - canDuoi;
                    decimal taxInBracket = Math.Round(incomeInBracket * b.THUE_SUAT, 2);
                    totalTax += taxInBracket;

                    result.BracketTraces.Add(new PayrollTaxTraceDto
                    {
                        IDBL = idbl,
                        MANV = manv,
                        MAKYCONG = makycong,
                        POLICY_THUE_ID = policy.ID,
                        BAC_THUE = b.BAC_THUE,
                        CAN_DUOI = canDuoi,
                        CAN_TREN = b.CAN_TREN,
                        THU_NHAP_CHIU_THUE_BAC = incomeInBracket,
                        THUE_SUAT = b.THUE_SUAT,
                        TIEN_THUE_BAC = taxInBracket
                    });
                }
            }

            result.TotalTax = totalTax;
            return result;
        }
    }
}
