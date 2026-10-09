using System;
using System.Collections.Generic;
using Bu.Services.AI_Services.Core;

namespace Bu.Services.AI_Services.Core
{
    public enum ExecutionStrategy
    {
        SqlTemplate,
        DeterministicDirect,
        VectorSearch,
        Hybrid,
        Unsupported,
        Forbidden,
        NeedsClarification,
        NoAction
    }

    /// <summary>
    /// Kế hoạch thực thi truy vấn đã được kiểm duyệt phân quyền và chuẩn hóa.
    /// </summary>
    public class QueryExecutionPlan
    {
        public string PlanId { get; set; } = Guid.NewGuid().ToString("N");
        public ExecutionStrategy Strategy { get; set; }
        public string Domain { get; set; }
        public string Operation { get; set; }
        public string Metric { get; set; }
        public string RequiredCapability { get; set; }
        public string RequiredScope { get; set; } // SELF, DEPARTMENT, COMPANY, ALL
        public string TargetView { get; set; }
        public string RequestedScope { get; set; }
        public string AuthorizationFingerprint { get; set; }
        public long SourceRevision { get; set; }
        public int RowLimit { get; set; } = 20;
        public Dictionary<string, string> FieldModes { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string SqlStatement { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public bool IsScalar { get; set; }
        public string ScalarUnit { get; set; }

        public string SelectedEntityDisplay { get; set; }
        public string EffectiveScopeDisplay { get; set; }
        public string EffectivePeriodDisplay { get; set; }
        public List<string> Assumptions { get; set; } = new List<string>();

        public string DenialOrUnsupportedReason { get; set; }
        public ClarificationPrompt Clarification { get; set; }
        public List<string> MaskedFields { get; set; } = new List<string>();
        public string VectorQuery { get; set; }
        public string VectorCapability { get; set; } = "POLICY_LOOKUP";
    }
}
