using System;

namespace Bu.CLASS_PAYROLL
{
    public interface IOtComplianceEngine
    {
        OtComplianceSnapshotDto EvaluateCompliance(
            decimal idbl,
            decimal manv,
            decimal makycong,
            decimal actualHoursMonth,
            decimal actualHoursYtdPrior,
            decimal totalOtPayment,
            decimal standardHourlyRate,
            int? extendedEligible = null,
            int? employeeConsent = null,
            int? notificationFiled = null
        );
    }

    public class OtComplianceEngine : IOtComplianceEngine
    {
        public const decimal MONTHLY_STANDARD_LIMIT = 40.0m;
        public const decimal ANNUAL_STANDARD_LIMIT = 200.0m;
        public const decimal ANNUAL_EXTENDED_LIMIT = 300.0m;

        public OtComplianceSnapshotDto EvaluateCompliance(
            decimal idbl,
            decimal manv,
            decimal makycong,
            decimal actualHoursMonth,
            decimal actualHoursYtdPrior,
            decimal totalOtPayment,
            decimal standardHourlyRate,
            int? extendedEligible = null,
            int? employeeConsent = null,
            int? notificationFiled = null)
        {
            decimal actualHoursYtdTotal = actualHoursYtdPrior + actualHoursMonth;

            // Determine effective annual ceiling
            decimal effectiveAnnualLimit = ANNUAL_STANDARD_LIMIT;
            bool isExtendedApproved = extendedEligible == 1 && employeeConsent == 1;
            if (isExtendedApproved)
            {
                effectiveAnnualLimit = ANNUAL_EXTENDED_LIMIT;
            }

            // Legal monthly hours allowed
            decimal remainingAnnualQuota = Math.Max(0m, effectiveAnnualLimit - actualHoursYtdPrior);
            decimal legalHoursAllowed = Math.Min(actualHoursMonth, Math.Min(MONTHLY_STANDARD_LIMIT, remainingAnnualQuota));
            decimal excessHours = Math.Max(0m, actualHoursMonth - legalHoursAllowed);

            // Compliance status evaluation
            string status;
            if (actualHoursMonth > MONTHLY_STANDARD_LIMIT || actualHoursYtdTotal > effectiveAnnualLimit)
            {
                status = "EXCEEDED_LEGAL_CAP";
            }
            else if (actualHoursYtdTotal > ANNUAL_STANDARD_LIMIT && (!extendedEligible.HasValue || !employeeConsent.HasValue))
            {
                status = "INSUFFICIENT_DATA";
            }
            else if (actualHoursYtdTotal > ANNUAL_STANDARD_LIMIT && !isExtendedApproved)
            {
                status = "UNAPPROVED_EXTENSION";
            }
            else
            {
                status = "COMPLIANT";
            }

            // Decree 253/2026/ND-CP Article 26: Tax Exemption Allocation
            // Legal eligible OT payment is exempt from PIT; excess is taxable.
            decimal exemptPayment;
            decimal taxableExcess;

            if (actualHoursMonth > 0 && totalOtPayment > 0)
            {
                decimal legalRatio = legalHoursAllowed / actualHoursMonth;
                exemptPayment = Math.Round(totalOtPayment * legalRatio, 2);
                taxableExcess = totalOtPayment - exemptPayment;
            }
            else
            {
                exemptPayment = 0m;
                taxableExcess = 0m;
            }

            return new OtComplianceSnapshotDto
            {
                IDBL = idbl,
                MANV = manv,
                MAKYCONG = makycong,
                MONTHLY_STANDARD_LIMIT = MONTHLY_STANDARD_LIMIT,
                ANNUAL_STANDARD_LIMIT = ANNUAL_STANDARD_LIMIT,
                ANNUAL_EXTENDED_LIMIT = ANNUAL_EXTENDED_LIMIT,
                ACTUAL_HOURS_MONTH = actualHoursMonth,
                ACTUAL_HOURS_YTD = actualHoursYtdTotal,
                LEGAL_HOURS_MONTH = legalHoursAllowed,
                EXCESS_HOURS_MONTH = excessHours,
                EXTENDED_ELIGIBLE = extendedEligible,
                EMPLOYEE_CONSENT = employeeConsent,
                NOTIFICATION_FILED = notificationFiled,
                COMPLIANCE_STATUS = status,
                TOTAL_OT_PAYMENT = totalOtPayment,
                LEGAL_ALLOWED_PAYMENT = exemptPayment,
                EXEMPT_OT_PAYMENT = exemptPayment,
                TAXABLE_EXCESS_PAYMENT = taxableExcess
            };
        }
    }
}
