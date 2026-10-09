using System;
using System.Collections.Generic;

namespace Bu.Services.AI_Services.Core
{
    public enum QuerySupportStatus
    {
        Supported,
        NeedsClarification,
        Unsupported,
        Forbidden
    }

    public class EntityCandidate
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public int? DepartmentId { get; set; }
        public string CompanyCode { get; set; }
    }

    public class EntityMention
    {
        public string MentionText { get; set; }
        public string EntityType { get; set; } // "EMPLOYEE", "DEPARTMENT", "COMPANY"
        public int? ResolvedId { get; set; }
        public string ResolvedName { get; set; }
        public int? DepartmentId { get; set; }
        public string CompanyCode { get; set; }
        public string Status { get; set; } // "RESOLVED", "AMBIGUOUS", "NOT_FOUND"
        public List<EntityCandidate> Candidates { get; set; } = new List<EntityCandidate>();
    }

    public class FilterCondition
    {
        public string Field { get; set; }
        public string Operator { get; set; } // "=", "<", ">", "<=", ">=", "BETWEEN", "LIKE"
        public object Value { get; set; }
        public object ValueTo { get; set; }
        public string DisplayText { get; set; }
    }

    public class TimeResolution
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? Day { get; set; }
        public int? BirthdayMonth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsRelativeTime { get; set; }
        public string RelativeDescription { get; set; } // "tháng trước", "kỳ này"
        public string AnchorKind { get; set; } // "CALENDAR_MONTH", "PERIOD", "EXACT_DATE", "BIRTHDAY"
    }

    public class ClarificationOption
    {
        public string Token { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
    }

    public class ClarificationPrompt
    {
        public string ClarificationId { get; set; } = Guid.NewGuid().ToString("N");
        public string Field { get; set; } // "MANV", "TIME_PERIOD", "OPERATION"
        public string Question { get; set; }
        public List<ClarificationOption> Options { get; set; } = new List<ClarificationOption>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class QueryUnderstandingResult
    {
        public string OriginalQuestion { get; set; }
        public string NormalizedQuestion { get; set; }
        public string ResolvedQuestion { get; set; }

        public string Domain { get; set; } // "EMPLOYEE", "ATTENDANCE", "OVERTIME", "INSURANCE", "ADVANCE", "ALLOWANCE", "PAYROLL", "CONTRACT", "SALARY_CHANGE", "GENERAL"
        public string Operation { get; set; } // "LOOKUP", "LIST", "COUNT", "SUM", "RANK", "COMPARE", "SELF"
        public string Metric { get; set; } // "SOGIO", "SOTIEN", "LUONG_COBAN", "THUCLANH", "TONGNGAYCONG", "HEADCOUNT"

        public List<EntityMention> Entities { get; set; } = new List<EntityMention>();
        public string RequestedScope { get; set; } // "SELF", "DEPARTMENT", "COMPANY", "ALL"
        public List<FilterCondition> Filters { get; set; } = new List<FilterCondition>();
        public TimeResolution Time { get; set; } = new TimeResolution();

        public List<string> MissingFields { get; set; } = new List<string>();
        public List<string> Ambiguities { get; set; } = new List<string>();
        public List<string> Assumptions { get; set; } = new List<string>();

        public QuerySupportStatus SupportStatus { get; set; } = QuerySupportStatus.Supported;
        public string UnsupportedReason { get; set; }
        public ClarificationPrompt Clarification { get; set; }

        public bool IsGreetingOnly { get; set; }
    }
}
