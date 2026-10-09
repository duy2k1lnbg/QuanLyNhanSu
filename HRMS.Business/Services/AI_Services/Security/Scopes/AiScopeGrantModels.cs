using System;
using System.Collections.Generic;

namespace Bu.Services.AI_Services.Security
{
    /// <summary>
    /// Bản ghi phân quyền phạm vi AI lưu trữ trong TB_AI_SCOPE_GRANT
    /// </summary>
    public class AiScopeGrantRecord
    {
        public long GrantId { get; set; }
        public string SubjectType { get; set; } // "USER" hoặc "GROUP"
        public int SubjectId { get; set; }
        public string CapabilityCode { get; set; }
        public string ScopeType { get; set; } // "SELF", "DEPARTMENT", "COMPANY", "ALL"
        public string ScopeKey { get; set; }
        public string Effect { get; set; } // "ALLOW", "DENY"
        public bool IsEnabled { get; set; } = true;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO chi tiết một Grant cụ thể (trực tiếp)
    /// </summary>
    public class AiScopeGrantItemDto
    {
        public long? GrantId { get; set; }
        public string CapabilityCode { get; set; }
        public string Effect { get; set; } // "ALLOW", "DENY"
        public string ScopeType { get; set; } // "SELF", "DEPARTMENT", "COMPANY", "ALL"
        public string ScopeKey { get; set; }
        public string ScopeName { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        public bool IsFuture { get; set; }
    }

    /// <summary>
    /// DTO chi tiết một Grant kế thừa từ Nhóm quyền
    /// </summary>
    public class AiInheritedScopeGrantDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string CapabilityCode { get; set; }
        public string Effect { get; set; } // "ALLOW", "DENY"
        public string ScopeType { get; set; }
        public string ScopeKey { get; set; }
        public string ScopeName { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO chi tiết hiển thị từng Capability cho màn hình quản trị tài khoản / nhóm quyền
    /// </summary>
    public class AiSubjectCapabilityScopeItemDto
    {
        public string CapabilityCode { get; set; }
        public string Domain { get; set; }
        public string Description { get; set; }
        public string SourceView { get; set; }
        public bool IsAggregateOnly { get; set; }

        // Trạng thái khả dụng của hệ thống (từ AiCapabilityCatalog / TB_AI_CAPABILITY)
        public bool IsCapabilityEnabled { get; set; }
        public string AvailabilityStatus { get; set; } // "Sẵn sàng" hoặc "Chưa khả dụng trong hệ thống"

        // Quyền nghiệp vụ nền bắt buộc
        public string RequiredFunctionCode { get; set; }
        public bool HasRequiredFunctionRight { get; set; }
        public string FunctionRightStatus { get; set; } // "Đã có quyền nền", "Chưa có quyền nền", "Không yêu cầu (Tự tra cứu)"

        // Danh sách đầy đủ các grant trực tiếp (Hỗ trợ nhiều grant trên 1 capability)
        public List<AiScopeGrantItemDto> DirectGrants { get; set; } = new List<AiScopeGrantItemDto>();

        // Thuộc tính tiện ích đại diện grant trực tiếp chính (phục vụ hiển thị trên GridView)
        public long? DirectGrantId { get; set; }
        public string DirectEffect { get; set; } = "NONE"; // "NONE", "ALLOW", "DENY"
        public string DirectScopeType { get; set; } // "SELF", "DEPARTMENT", "COMPANY", "ALL", null
        public string DirectScopeKey { get; set; }
        public string DirectScopeName { get; set; }
        public DateTime? DirectValidFrom { get; set; }
        public DateTime? DirectValidTo { get; set; }
        public string DirectSummary { get; set; }

        // Danh sách đầy đủ các grant kế thừa từ các nhóm
        public List<AiInheritedScopeGrantDto> InheritedGrants { get; set; } = new List<AiInheritedScopeGrantDto>();

        // Thuộc tính tiện ích đại diện grant kế thừa (phục vụ hiển thị trên GridView)
        public bool HasInheritedGrant { get; set; }
        public string InheritedEffect { get; set; } = "NONE";
        public string InheritedScopeType { get; set; }
        public string InheritedScopeKey { get; set; }
        public string InheritedScopeName { get; set; }
        public DateTime? InheritedValidTo { get; set; }
        public string InheritedFromGroup { get; set; }
        public string InheritedSummary { get; set; }

        // Kết quả có hiệu lực (Effective) theo single resolver engine
        public string EffectiveEffect { get; set; } // "ALLOW", "DENY", "NOT_CONFIGURED", "BLOCKED_NO_FUNCTION", "DISABLED", "UNMAPPED_EMPLOYEE", "EXPIRED"
        public string EffectiveScopeType { get; set; } // "ALL", "DEPARTMENT", "COMPANY", "SELF", "NONE"
        public string EffectiveScopeKey { get; set; }
        public string EffectiveScopeSummary { get; set; }
        public string ExplanationNotes { get; set; }
        public List<int> EffectiveDepartmentIds { get; set; } = new List<int>();
        public List<string> EffectiveCompanyCodes { get; set; } = new List<string>();
        public bool EffectiveIsAll { get; set; }
        public bool EffectiveIsSelf { get; set; }
    }

    /// <summary>
    /// Tổng quan phân quyền AI cho một đối tượng (User hoặc Group)
    /// </summary>
    public class AiSubjectScopeOverviewDto
    {
        public string SubjectType { get; set; } // "USER" hoặc "GROUP"
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } // Username hoặc Group Name
        public string SubjectName { get; set; } // FullName hoặc Group Description
        public int? EmployeeId { get; set; } // MANV của tài khoản (nếu là USER)
        public bool HasAiFeatureRight { get; set; } // Có quyền F_SYSTEM_AI hay không
        public bool IsReady { get; set; } = true; // Schema/Hệ thống đã sẵn sàng hay chưa
        public string NotReadyReason { get; set; } // Lý do nếu chưa sẵn sàng
        public long CurrentRevision { get; set; } // Concurrency token / Revision hiện hành
        public List<AiSubjectCapabilityScopeItemDto> Capabilities { get; set; } = new List<AiSubjectCapabilityScopeItemDto>();
    }

    /// <summary>
    /// DTO gửi từ UI/API để lưu cấu hình
    /// </summary>
    public class SaveAiSubjectScopeGrantItemDto
    {
        public long? GrantId { get; set; }
        public string CapabilityCode { get; set; }
        public string Effect { get; set; } // "NONE", "ALLOW", "DENY"
        public string ScopeType { get; set; } // "SELF", "DEPARTMENT", "COMPANY", "ALL"
        public string ScopeKey { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }

    public class SaveAiSubjectScopeGrantsRequest
    {
        public string SubjectType { get; set; } // "USER" hoặc "GROUP"
        public int SubjectId { get; set; }
        public int ActorUserId { get; set; }
        public string ActorUsername { get; set; }
        public string ClientType { get; set; } = "DESKTOP";
        public long? BaseRevision { get; set; } // Optimistic Concurrency token
        public List<SaveAiSubjectScopeGrantItemDto> Grants { get; set; } = new List<SaveAiSubjectScopeGrantItemDto>();
    }

    public class SaveAiSubjectScopeGrantsResult
    {
        public bool Success { get; set; }
        public bool IsConcurrencyConflict { get; set; }
        public bool IsSchemaNotReady { get; set; }
        public string Message { get; set; }
        public long? NewPolicyRevision { get; set; }
        public int SavedCount { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }
}
