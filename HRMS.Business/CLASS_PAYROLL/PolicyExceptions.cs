using System;

namespace Bu.CLASS_PAYROLL
{
    public class PolicyNotFoundException : Exception
    {
        public string PolicyType { get; }
        public DateTime EffectiveDate { get; }

        public PolicyNotFoundException(string policyType, DateTime effectiveDate)
            : base($"No active policy of type '{policyType}' found for effective date {effectiveDate:yyyy-MM-dd}.")
        {
            PolicyType = policyType;
            EffectiveDate = effectiveDate;
        }
    }

    public class AmbiguousPolicyException : Exception
    {
        public string PolicyType { get; }
        public DateTime EffectiveDate { get; }
        public int MatchCount { get; }

        public AmbiguousPolicyException(string policyType, DateTime effectiveDate, int matchCount)
            : base($"Ambiguous policy resolution: found {matchCount} active policies of type '{policyType}' for effective date {effectiveDate:yyyy-MM-dd}.")
        {
            PolicyType = policyType;
            EffectiveDate = effectiveDate;
            MatchCount = matchCount;
        }
    }

    public class PolicyOverlapException : Exception
    {
        public string PolicyType { get; }
        public string PolicyCode1 { get; }
        public string PolicyCode2 { get; }

        public PolicyOverlapException(string policyType, string policyCode1, string policyCode2)
            : base($"Overlapping policy effective ranges detected for '{policyType}': '{policyCode1}' and '{policyCode2}'.")
        {
            PolicyType = policyType;
            PolicyCode1 = policyCode1;
            PolicyCode2 = policyCode2;
        }
    }

    public class ComplianceViolationException : Exception
    {
        public decimal EmployeeId { get; }
        public string RuleName { get; }

        public ComplianceViolationException(decimal employeeId, string ruleName, string message)
            : base($"Compliance Violation [Rule: {ruleName}, Employee: {employeeId}]: {message}")
        {
            EmployeeId = employeeId;
            RuleName = ruleName;
        }
    }
}
