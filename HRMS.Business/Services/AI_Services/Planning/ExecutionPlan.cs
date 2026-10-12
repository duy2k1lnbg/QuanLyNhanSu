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
        NoAction,
        ChatSocial,
        ChatGeneral,
        HrHelp
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

        /// <summary>
        /// Tạo bản sao độc lập của kế hoạch cho subtask (ví dụ: Hybrid SQL SubPlan)
        /// Bảo toàn toàn bộ RowLimit, FieldModes, MaskedFields, SourceRevision và các tham số phân quyền.
        /// </summary>
        public QueryExecutionPlan CloneForSubPlan(ExecutionStrategy subStrategy)
        {
            var clone = new QueryExecutionPlan
            {
                PlanId = Guid.NewGuid().ToString("N"),
                Strategy = subStrategy,
                Domain = this.Domain,
                Operation = this.Operation,
                Metric = this.Metric,
                RequiredCapability = this.RequiredCapability,
                RequiredScope = this.RequiredScope,
                TargetView = this.TargetView,
                RequestedScope = this.RequestedScope,
                AuthorizationFingerprint = this.AuthorizationFingerprint,
                SourceRevision = this.SourceRevision,
                RowLimit = this.RowLimit,
                SqlStatement = this.SqlStatement,
                IsScalar = this.IsScalar,
                ScalarUnit = this.ScalarUnit,
                SelectedEntityDisplay = this.SelectedEntityDisplay,
                EffectiveScopeDisplay = this.EffectiveScopeDisplay,
                EffectivePeriodDisplay = this.EffectivePeriodDisplay,
                DenialOrUnsupportedReason = this.DenialOrUnsupportedReason,
                VectorQuery = this.VectorQuery,
                VectorCapability = this.VectorCapability
            };

            if (this.Parameters != null)
            {
                clone.Parameters = new Dictionary<string, object>(this.Parameters);
            }
            if (this.FieldModes != null)
            {
                clone.FieldModes = new Dictionary<string, string>(this.FieldModes, StringComparer.OrdinalIgnoreCase);
            }
            if (this.MaskedFields != null)
            {
                clone.MaskedFields = new List<string>(this.MaskedFields);
            }
            if (this.Assumptions != null)
            {
                clone.Assumptions = new List<string>(this.Assumptions);
            }

            return clone;
        }
    }
}
