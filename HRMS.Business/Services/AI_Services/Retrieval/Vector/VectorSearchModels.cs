using System;
using System.Collections.Generic;

namespace Bu.Services.AI_Services.Vector
{
    public class VectorSecurityFilter
    {
        public int? UserId { get; set; }
        public int? CallerEmployeeId { get; set; }
        public bool IsAdmin { get; set; }
        public List<int> AllowedDepartmentIds { get; set; } = new List<int>();
        public string AllowedCompanyId { get; set; }
        public int? TargetEmployeeId { get; set; }
        public string EffectiveScope { get; set; } // SELF, DEPARTMENT, COMPANY, ALL
        public List<string> DenyCodes { get; set; } = new List<string>();
    }

    public class VectorBusinessFilter
    {
        public string Domain { get; set; }
        public string DocumentType { get; set; }
        public string Tag { get; set; }
        public int? TargetDepartmentId { get; set; }
        public int? TargetEmployeeId { get; set; }
    }

    public class VectorHit
    {
        public string PointId { get; set; }
        public float Score { get; set; }
        public string Domain { get; set; }
        public string DocumentType { get; set; }
        public int? EmployeeId { get; set; }
        public int? DepartmentId { get; set; }
        public string CompanyId { get; set; }
        public string Text { get; set; }
        public string Version { get; set; }
        public string SourceId { get; set; }
        public string Title { get; set; }
        public string Section { get; set; }
        public string UpdatedAt { get; set; }
        public bool IsAuthorized { get; set; } = false;
    }

    public class VectorSearchResult
    {
        public List<VectorHit> Hits { get; set; } = new List<VectorHit>();
        public int TotalFound { get; set; }
        public bool ReusedEmbedding { get; set; }
        public string SecurityFilterApplied { get; set; }
    }
}
