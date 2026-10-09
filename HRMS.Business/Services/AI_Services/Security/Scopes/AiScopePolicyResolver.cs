using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Services.AI_Services.Security
{
    /// <summary>
    /// Bộ máy giải quyết chính sách phạm vi AI tập trung và thống nhất (Single Unified Evaluation Engine)
    /// Dùng chung cho:
    /// 1. Tải tổng quan trên Service (GetSubjectScopeOverview)
    /// 2. Hiển thị / Xem trước trên Form UI (Desktop / Web)
    /// 3. Đánh giá kiểm soát truy cập tại Runtime (AiScopeEvaluator / BuildSqlScopeFilter)
    /// </summary>
    public static class AiScopePolicyResolver
    {
        public class ResolutionInput
        {
            public string CapabilityCode { get; set; }
            public bool IsCapabilityEnabled { get; set; }
            public string RequiredFunctionCode { get; set; }
            public bool HasRequiredFunctionRight { get; set; }
            public int? ActorManv { get; set; }
            public List<AiScopeGrantRecord> DirectGrants { get; set; } = new List<AiScopeGrantRecord>();
            public List<(AiScopeGrantRecord Grant, string GroupName)> InheritedGrants { get; set; } = new List<(AiScopeGrantRecord, string)>();
            public Dictionary<string, string> DepartmentNames { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> CompanyNames { get; set; } = new Dictionary<string, string>();
            public DateTime AsOf { get; set; } = DateTime.Now;
        }

        public class ResolutionOutput
        {
            public string EffectiveEffect { get; set; } // "ALLOW", "DENY", "NOT_CONFIGURED", "BLOCKED_NO_FUNCTION", "DISABLED", "UNMAPPED_EMPLOYEE", "EXPIRED"
            public string EffectiveScopeType { get; set; } // "ALL", "DEPARTMENT", "COMPANY", "SELF", "NONE"
            public string EffectiveScopeKey { get; set; }
            public string EffectiveScopeSummary { get; set; }
            public string ExplanationNotes { get; set; }
            public List<int> DepartmentIds { get; set; } = new List<int>();
            public List<string> CompanyCodes { get; set; } = new List<string>();
            public bool IsAll { get; set; }
            public bool IsSelf { get; set; }
        }

        public static ResolutionOutput Resolve(ResolutionInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var output = new ResolutionOutput
            {
                EffectiveEffect = "NOT_CONFIGURED",
                EffectiveScopeType = "NONE",
                EffectiveScopeSummary = "Chưa cấp quyền tra cứu AI",
                ExplanationNotes = "Tài khoản chưa được cấu hình phạm vi tra cứu cho chức năng này."
            };

            // 1. Kiểm tra trạng thái Capability trong hệ thống
            if (!input.IsCapabilityEnabled)
            {
                output.EffectiveEffect = "DISABLED";
                output.EffectiveScopeSummary = "Nghiệp vụ chưa khả dụng";
                output.ExplanationNotes = "Capability này đang bị tắt ở mức hệ thống (chưa hỗ trợ hoặc tạm ngừng).";
                return output;
            }

            // 2. Lọc các Grant đang có hiệu lực tại thời điểm AsOf
            bool IsGrantActive(AiScopeGrantRecord g)
            {
                if (g == null || !g.IsEnabled) return false;
                if (g.ValidFrom.HasValue && g.ValidFrom.Value > input.AsOf) return false; // Chưa tới ngày hiệu lực
                if (g.ValidTo.HasValue && g.ValidTo.Value <= input.AsOf) return false; // Đã hết hạn
                return true;
            }

            var activeDirect = (input.DirectGrants ?? new List<AiScopeGrantRecord>()).Where(IsGrantActive).ToList();
            var activeInherited = (input.InheritedGrants ?? new List<(AiScopeGrantRecord, string)>()).Where(x => IsGrantActive(x.Grant)).ToList();

            // 3. Kiểm tra DENY: Theo đúng cơ chế PKG_AI_AUTH:
            // "IF v_deny_grant > 0 THEN RAISE_APPLICATION_ERROR(-20008, 'Access denied: Explicit DENY scope policy matches actor.');"
            // Khi có bất kỳ DENY còn hiệu lực nào (trực tiếp hoặc kế thừa từ nhóm), chặn TOÀN BỘ capability.
            // Chú ý: DENY đã hết hạn hoặc tương lai không chặn hiện tại.
            var directDenies = activeDirect.Where(g => string.Equals(g.Effect, "DENY", StringComparison.OrdinalIgnoreCase)).ToList();
            var inheritedDenies = activeInherited.Where(x => string.Equals(x.Grant.Effect, "DENY", StringComparison.OrdinalIgnoreCase)).ToList();

            if (directDenies.Count > 0 || inheritedDenies.Count > 0)
            {
                string reason = directDenies.Count > 0
                    ? "Bị chặn trực tiếp bằng cấu hình DENY."
                    : $"Bị chặn do kế thừa DENY từ [{string.Join(", ", inheritedDenies.Select(x => x.GroupName).Distinct())}].";

                output.EffectiveEffect = "DENY";
                output.EffectiveScopeSummary = "Bị CHẶN toàn bộ capability (DENY)";
                output.ExplanationNotes = $"{reason} Cơ chế PKG_AI_AUTH: cấu hình DENY sẽ chặn toàn bộ capability đối với tài khoản/nhóm (không hỗ trợ chỉ loại trừ một phòng ban).";
                return output;
            }

            // 4. Kiểm tra quyền nghiệp vụ nền bắt buộc
            if (!string.IsNullOrEmpty(input.RequiredFunctionCode) && !input.HasRequiredFunctionRight)
            {
                output.EffectiveEffect = "BLOCKED_NO_FUNCTION";
                output.EffectiveScopeSummary = $"Thiếu quyền nghiệp vụ nền ({input.RequiredFunctionCode})";
                output.ExplanationNotes = $"Tài khoản chưa có quyền chức năng nghiệp vụ tương ứng ({input.RequiredFunctionCode}) trên ứng dụng. Quyền F_SYSTEM_AI tuyệt đối không thay thế quyền nghiệp vụ nền.";
                return output;
            }

            // 5. Kiểm tra trường hợp Capability chỉ dành riêng cho tự tra cứu cá nhân (INSURANCE_SELF, PAYROLL_SELF)
            bool isSelfOnlyCap = string.Equals(input.CapabilityCode, "INSURANCE_SELF", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(input.CapabilityCode, "PAYROLL_SELF", StringComparison.OrdinalIgnoreCase);

            if (isSelfOnlyCap)
            {
                if (!input.ActorManv.HasValue || input.ActorManv.Value <= 0)
                {
                    output.EffectiveEffect = "UNMAPPED_EMPLOYEE";
                    output.EffectiveScopeSummary = "Chưa liên kết nhân sự (Thiếu MANV)";
                    output.ExplanationNotes = "Tài khoản người dùng chưa được liên kết với nhân viên trong hồ sơ nhân sự (thiếu MANV). Không thể thực hiện tự tra cứu hồ sơ cá nhân.";
                    return output;
                }

                output.EffectiveEffect = "ALLOW";
                output.EffectiveScopeType = "SELF";
                output.IsSelf = true;
                output.EffectiveScopeSummary = $"Bản thân (NV #{input.ActorManv.Value})";
                output.ExplanationNotes = "Nghiệp vụ tự tra cứu cá nhân luôn bị ràng buộc chặt chẽ vào chính nhân viên đăng nhập theo quy định an ninh.";
                return output;
            }

            // 6. Tập hợp tất cả các ALLOW còn hiệu lực (Hợp tất cả ALLOW - Union of ALLOWs)
            var directAllows = activeDirect.Where(g => string.Equals(g.Effect, "ALLOW", StringComparison.OrdinalIgnoreCase)).ToList();
            var inheritedAllows = activeInherited.Where(x => string.Equals(x.Grant.Effect, "ALLOW", StringComparison.OrdinalIgnoreCase)).ToList();

            var allAllows = directAllows.Concat(inheritedAllows.Select(x => x.Grant)).ToList();

            if (allAllows.Count == 0)
            {
                // Kiểm tra xem có cấu hình nào đã hết hạn không
                bool hasExpired = (input.DirectGrants ?? new List<AiScopeGrantRecord>()).Any(g => g.ValidTo.HasValue && g.ValidTo.Value <= input.AsOf) ||
                                  (input.InheritedGrants ?? new List<(AiScopeGrantRecord, string)>()).Any(x => x.Grant.ValidTo.HasValue && x.Grant.ValidTo.Value <= input.AsOf);

                if (hasExpired)
                {
                    output.EffectiveEffect = "EXPIRED";
                    output.EffectiveScopeSummary = "Quyền AI đã hết hạn";
                    output.ExplanationNotes = "Cấu hình phân quyền tra cứu AI trước đó đã hết hạn hiệu lực. Cần gia hạn để tiếp tục tra cứu.";
                    return output;
                }

                output.EffectiveEffect = "NOT_CONFIGURED";
                output.EffectiveScopeSummary = "Chưa cấp quyền tra cứu AI";
                output.ExplanationNotes = "Tài khoản chưa được cấp phạm vi tra cứu cho chức năng AI này.";
                return output;
            }

            // 7. Đánh giá hợp phạm vi (Union evaluation)
            output.EffectiveEffect = "ALLOW";

            // A. Nếu có bất kỳ grant nào là ALL -> Phạm vi là ALL
            bool hasAll = allAllows.Any(g => string.Equals(g.ScopeType, "ALL", StringComparison.OrdinalIgnoreCase));
            if (hasAll)
            {
                output.EffectiveScopeType = "ALL";
                output.IsAll = true;

                bool isGroupAll = inheritedAllows.Any(x => string.Equals(x.Grant.ScopeType, "ALL", StringComparison.OrdinalIgnoreCase));
                bool isDirectAll = directAllows.Any(g => string.Equals(g.ScopeType, "ALL", StringComparison.OrdinalIgnoreCase));
                bool hasDirectSelf = directAllows.Any(g => string.Equals(g.ScopeType, "SELF", StringComparison.OrdinalIgnoreCase));

                if (isGroupAll && hasDirectSelf)
                {
                    var groupNames = string.Join(", ", inheritedAllows.Where(x => string.Equals(x.Grant.ScopeType, "ALL", StringComparison.OrdinalIgnoreCase)).Select(x => x.GroupName).Distinct());
                    output.EffectiveScopeSummary = "Toàn bộ hệ thống (Mở rộng từ nhóm)";
                    output.ExplanationNotes = $"Được mở rộng lên Toàn bộ hệ thống do kế thừa từ [{groupNames}]. Cấu hình trực tiếp [Bản thân] không thu hẹp quyền của nhóm.";
                }
                else if (isGroupAll && !isDirectAll)
                {
                    var groupNames = string.Join(", ", inheritedAllows.Where(x => string.Equals(x.Grant.ScopeType, "ALL", StringComparison.OrdinalIgnoreCase)).Select(x => x.GroupName).Distinct());
                    output.EffectiveScopeSummary = $"Toàn bộ hệ thống (Kế thừa từ [{groupNames}])";
                    output.ExplanationNotes = $"Kế thừa phạm vi Toàn bộ hệ thống từ [{groupNames}].";
                }
                else
                {
                    output.EffectiveScopeSummary = "Toàn bộ hệ thống";
                    output.ExplanationNotes = "Cấu hình cho phép tra cứu toàn bộ dữ liệu trong hệ thống.";
                }
                return output;
            }

            // B. Tập hợp Company và Department
            var companyCodes = allAllows
                .Where(g => string.Equals(g.ScopeType, "COMPANY", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(g.ScopeKey))
                .Select(g => g.ScopeKey.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var deptIds = new List<int>();
            foreach (var g in allAllows.Where(g => string.Equals(g.ScopeType, "DEPARTMENT", StringComparison.OrdinalIgnoreCase)))
            {
                if (int.TryParse(g.ScopeKey, out int did) && did > 0 && !deptIds.Contains(did))
                {
                    deptIds.Add(did);
                }
            }

            bool hasSelf = allAllows.Any(g => string.Equals(g.ScopeType, "SELF", StringComparison.OrdinalIgnoreCase));

            output.DepartmentIds = deptIds;
            output.CompanyCodes = companyCodes;
            output.IsSelf = hasSelf;

            var summaryParts = new List<string>();
            var noteParts = new List<string>();

            if (companyCodes.Count > 0)
            {
                output.EffectiveScopeType = "COMPANY";
                var compLabels = companyCodes.Select(c => input.CompanyNames.TryGetValue(c, out var cn) ? $"{cn} ({c})" : $"Công ty {c}");
                summaryParts.Add($"Công ty: {string.Join(", ", compLabels)}");
            }

            if (deptIds.Count > 0)
            {
                if (output.EffectiveScopeType == "NONE") output.EffectiveScopeType = "DEPARTMENT";
                var deptLabels = deptIds.Select(d => input.DepartmentNames.TryGetValue(d.ToString(), out var dn) ? $"{dn}" : $"PB #{d}");
                summaryParts.Add($"{deptIds.Count} phòng ban: {string.Join(", ", deptLabels)}");
            }

            if (summaryParts.Count == 0 && hasSelf)
            {
                output.EffectiveScopeType = "SELF";
                output.IsSelf = true;
                output.EffectiveScopeSummary = "Bản thân";
                output.ExplanationNotes = "Cho phép tra cứu hồ sơ cá nhân của chính mình.";
                return output;
            }

            if (summaryParts.Count > 0)
            {
                output.EffectiveScopeSummary = string.Join("; ", summaryParts);
                if (inheritedAllows.Count > 0 && directAllows.Count > 0)
                {
                    noteParts.Add("Kết hợp quyền trực tiếp và quyền kế thừa từ nhóm.");
                }
                else if (inheritedAllows.Count > 0)
                {
                    var groupNames = string.Join(", ", inheritedAllows.Select(x => x.GroupName).Distinct());
                    noteParts.Add($"Kế thừa từ [{groupNames}].");
                }
                else
                {
                    noteParts.Add("Cấu hình trực tiếp.");
                }
                output.ExplanationNotes = string.Join(" ", noteParts);
                return output;
            }

            return output;
        }
    }
}
