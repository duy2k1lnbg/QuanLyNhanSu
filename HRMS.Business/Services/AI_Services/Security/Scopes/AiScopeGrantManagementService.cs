using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Services.AI_Services.Security
{
    public interface IAiScopeGrantManagementService
    {
        AiSubjectScopeOverviewDto GetSubjectScopeOverview(string subjectType, int subjectId, DateTime? asOf = null);
        SaveAiSubjectScopeGrantsResult SaveSubjectScopeGrants(SaveAiSubjectScopeGrantsRequest request);
        List<KeyValuePair<string, string>> GetDepartmentOptions();
        List<KeyValuePair<string, string>> GetCompanyOptions();
    }

    public class AiScopeGrantManagementService : IAiScopeGrantManagementService
    {
        private readonly IAiScopeGrantRepository _repository;
        private readonly IAdminSecurityContext _securityContext;

        public AiScopeGrantManagementService()
            : this(new OracleAiScopeGrantRepository(), new DesktopAdminSecurityContext())
        {
        }

        public AiScopeGrantManagementService(
            IAiScopeGrantRepository repository,
            IAdminSecurityContext securityContext = null)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _securityContext = securityContext ?? new DesktopAdminSecurityContext();
        }

        public AiSubjectScopeOverviewDto GetSubjectScopeOverview(string subjectType, int subjectId, DateTime? asOf = null)
        {
            // 1. Kiểm tra quyền Quản trị viên của Actor
            if (!_securityContext.IsAdmin() || _securityContext.GetActorUserId() <= 0)
            {
                throw new UnauthorizedAccessException("Từ chối truy cập: Chỉ Quản trị viên hệ thống (Admin) mới có quyền xem phân quyền tra cứu AI.");
            }

            var now = asOf ?? DateTime.Now;
            string sType = (subjectType ?? "USER").Trim().ToUpperInvariant();
            if (sType != "USER" && sType != "GROUP")
            {
                throw new ArgumentException("Loại đối tượng (subjectType) phải là 'USER' hoặc 'GROUP'.");
            }

            if (subjectId <= 0)
            {
                throw new ArgumentException("Mã đối tượng (subjectId) không hợp lệ.");
            }

            var overview = new AiSubjectScopeOverviewDto
            {
                SubjectType = sType,
                SubjectId = subjectId
            };

            // 2. Kiểm tra tính sẵn sàng của Schema CSDL
            if (!_repository.IsSchemaReady(out string notReadyReason))
            {
                overview.IsReady = false;
                overview.NotReadyReason = notReadyReason;
            }

            // 3. Lấy thông tin đối tượng từ CSDL
            if (!_repository.SubjectExists(sType, subjectId, out string sCode, out string sName, out int? manv, out bool isDisabled))
            {
                throw new InvalidOperationException($"Không tìm thấy đối tượng {sType} có mã ID={subjectId} trong hệ thống.");
            }

            overview.SubjectCode = sCode;
            overview.SubjectName = sName;
            overview.EmployeeId = manv;

            // 4. Lấy danh sách nhóm và quyền chức năng nghiệp vụ nền
            var userGroupIds = new List<int>();
            var functionRights = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (sType == "USER")
            {
                userGroupIds = _repository.GetUserGroupIds(subjectId) ?? new List<int>();
                functionRights = _repository.GetUserFunctionRights(subjectId, userGroupIds) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            else // GROUP
            {
                functionRights = _repository.GetGroupFunctionRights(subjectId) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            overview.HasAiFeatureRight = functionRights.Contains("F_SYSTEM_AI");

            // 5. Lấy danh sách grant trực tiếp
            var directGrants = _repository.GetGrants(sType, subjectId) ?? new List<AiScopeGrantRecord>();

            // 6. Lấy danh sách grant kế thừa từ các nhóm (đối với USER)
            var inheritedGrantsList = new List<(AiScopeGrantRecord Grant, string GroupName)>();
            if (sType == "USER" && userGroupIds.Count > 0)
            {
                foreach (var gid in userGroupIds)
                {
                    string groupName = _repository.GetGroupName(gid);
                    var gGrants = _repository.GetGrants("GROUP", gid) ?? new List<AiScopeGrantRecord>();
                    foreach (var gg in gGrants)
                    {
                        inheritedGrantsList.Add((gg, groupName));
                    }
                }
            }

            // 7. Lấy danh mục phòng ban và công ty để format tên
            var deptOptions = _repository.GetDepartmentOptions() ?? new List<KeyValuePair<string, string>>();
            var compOptions = _repository.GetCompanyOptions() ?? new List<KeyValuePair<string, string>>();
            var deptDict = deptOptions.ToDictionary(k => k.Key, v => v.Value);
            var compDict = compOptions.ToDictionary(k => k.Key, v => v.Value);

            // 8. Đánh giá từng Capability theo danh mục AiCapabilityCatalog bằng AiScopePolicyResolver thống nhất
            var catalog = AiCapabilityCatalog.GetAll();
            foreach (var cap in catalog)
            {
                var item = new AiSubjectCapabilityScopeItemDto
                {
                    CapabilityCode = cap.CapabilityCode,
                    Domain = cap.Domain,
                    Description = cap.Description,
                    SourceView = cap.SourceView,
                    IsAggregateOnly = cap.IsAggregateOnly,
                    IsCapabilityEnabled = cap.IsEnabled,
                    AvailabilityStatus = cap.IsEnabled ? "Sẵn sàng" : "Chưa khả dụng trong hệ thống",
                    RequiredFunctionCode = cap.RequiredFunctionCode
                };

                // Kiểm tra quyền nghiệp vụ nền
                if (string.IsNullOrEmpty(cap.RequiredFunctionCode))
                {
                    item.HasRequiredFunctionRight = true;
                    item.FunctionRightStatus = "Không yêu cầu (Tự tra cứu)";
                }
                else
                {
                    item.HasRequiredFunctionRight = functionRights.Contains(cap.RequiredFunctionCode);
                    item.FunctionRightStatus = item.HasRequiredFunctionRight
                        ? "Đã có quyền nền"
                        : $"Chưa có quyền nền ({cap.RequiredFunctionCode})";
                }

                // Tập hợp các grant trực tiếp của capability này
                var capDirects = directGrants.Where(g => string.Equals(g.CapabilityCode, cap.CapabilityCode, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var dg in capDirects)
                {
                    item.DirectGrants.Add(new AiScopeGrantItemDto
                    {
                        GrantId = dg.GrantId,
                        CapabilityCode = dg.CapabilityCode,
                        Effect = dg.Effect,
                        ScopeType = dg.ScopeType,
                        ScopeKey = dg.ScopeKey,
                        ScopeName = FormatScopeKeyName(dg.ScopeType, dg.ScopeKey, deptDict, compDict),
                        ValidFrom = dg.ValidFrom,
                        ValidTo = dg.ValidTo,
                        IsActive = dg.IsEnabled && (!dg.ValidFrom.HasValue || dg.ValidFrom.Value <= now) && (!dg.ValidTo.HasValue || dg.ValidTo.Value > now),
                        IsExpired = dg.ValidTo.HasValue && dg.ValidTo.Value <= now,
                        IsFuture = dg.ValidFrom.HasValue && dg.ValidFrom.Value > now
                    });
                }

                // Thiết lập các thuộc tính tiện ích cho grid trực tiếp
                var primaryDirect = capDirects.FirstOrDefault();
                if (primaryDirect != null)
                {
                    item.DirectGrantId = primaryDirect.GrantId;
                    item.DirectEffect = primaryDirect.Effect;
                    item.DirectScopeType = primaryDirect.ScopeType;
                    item.DirectScopeKey = primaryDirect.ScopeKey;
                    item.DirectValidFrom = primaryDirect.ValidFrom;
                    item.DirectValidTo = primaryDirect.ValidTo;
                    item.DirectScopeName = FormatScopeKeyName(primaryDirect.ScopeType, primaryDirect.ScopeKey, deptDict, compDict);
                    item.DirectSummary = capDirects.Count > 1
                        ? $"{capDirects.Count} cấu hình trực tiếp"
                        : $"{primaryDirect.Effect} {DescribeScope(primaryDirect.ScopeType, item.DirectScopeName)}";
                }
                else
                {
                    item.DirectEffect = "NONE";
                    item.DirectSummary = "Chưa cấu hình";
                }

                // Tập hợp các grant kế thừa từ các nhóm của capability này
                var capInherited = inheritedGrantsList.Where(x => string.Equals(x.Grant.CapabilityCode, cap.CapabilityCode, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var inh in capInherited)
                {
                    item.InheritedGrants.Add(new AiInheritedScopeGrantDto
                    {
                        GroupId = inh.Grant.SubjectId,
                        GroupName = inh.GroupName,
                        CapabilityCode = inh.Grant.CapabilityCode,
                        Effect = inh.Grant.Effect,
                        ScopeType = inh.Grant.ScopeType,
                        ScopeKey = inh.Grant.ScopeKey,
                        ScopeName = FormatScopeKeyName(inh.Grant.ScopeType, inh.Grant.ScopeKey, deptDict, compDict),
                        ValidFrom = inh.Grant.ValidFrom,
                        ValidTo = inh.Grant.ValidTo,
                        IsActive = inh.Grant.IsEnabled && (!inh.Grant.ValidFrom.HasValue || inh.Grant.ValidFrom.Value <= now) && (!inh.Grant.ValidTo.HasValue || inh.Grant.ValidTo.Value > now)
                    });
                }

                var primaryInherited = capInherited.FirstOrDefault();
                if (primaryInherited.Grant != null)
                {
                    item.HasInheritedGrant = true;
                    item.InheritedEffect = primaryInherited.Grant.Effect;
                    item.InheritedScopeType = primaryInherited.Grant.ScopeType;
                    item.InheritedScopeKey = primaryInherited.Grant.ScopeKey;
                    item.InheritedValidTo = primaryInherited.Grant.ValidTo;
                    item.InheritedFromGroup = primaryInherited.GroupName;
                    item.InheritedScopeName = FormatScopeKeyName(primaryInherited.Grant.ScopeType, primaryInherited.Grant.ScopeKey, deptDict, compDict);
                    item.InheritedSummary = capInherited.Count > 1
                        ? $"{capInherited.Count} quyền từ các nhóm"
                        : $"[{primaryInherited.GroupName}] {primaryInherited.Grant.Effect} {DescribeScope(primaryInherited.Grant.ScopeType, item.InheritedScopeName)}";
                }
                else
                {
                    item.InheritedEffect = "NONE";
                    item.InheritedSummary = "Không có";
                }

                // 9. Đánh giá kết quả có hiệu lực bằng AiScopePolicyResolver
                var resolutionInput = new AiScopePolicyResolver.ResolutionInput
                {
                    CapabilityCode = cap.CapabilityCode,
                    IsCapabilityEnabled = cap.IsEnabled,
                    RequiredFunctionCode = cap.RequiredFunctionCode,
                    HasRequiredFunctionRight = item.HasRequiredFunctionRight,
                    ActorManv = manv,
                    DirectGrants = capDirects,
                    InheritedGrants = capInherited,
                    DepartmentNames = deptDict,
                    CompanyNames = compDict,
                    AsOf = now
                };

                var resolutionOutput = AiScopePolicyResolver.Resolve(resolutionInput);

                item.EffectiveEffect = resolutionOutput.EffectiveEffect;
                item.EffectiveScopeType = resolutionOutput.EffectiveScopeType;
                item.EffectiveScopeKey = resolutionOutput.EffectiveScopeKey;
                item.EffectiveScopeSummary = resolutionOutput.EffectiveScopeSummary;
                item.ExplanationNotes = resolutionOutput.ExplanationNotes;
                item.EffectiveDepartmentIds = resolutionOutput.DepartmentIds;
                item.EffectiveCompanyCodes = resolutionOutput.CompanyCodes;
                item.EffectiveIsAll = resolutionOutput.IsAll;
                item.EffectiveIsSelf = resolutionOutput.IsSelf;

                overview.Capabilities.Add(item);
            }

            // Gắn revision hiện hành làm concurrency token
            overview.CurrentRevision = _repository.GetCurrentPolicyRevision();

            return overview;
        }

        public SaveAiSubjectScopeGrantsResult SaveSubjectScopeGrants(SaveAiSubjectScopeGrantsRequest request)
        {
            var res = new SaveAiSubjectScopeGrantsResult();

            if (request == null)
            {
                res.Success = false;
                res.Message = "Yêu cầu lưu cấu hình không hợp lệ (Dữ liệu rỗng).";
                return res;
            }

            // 1. Kiểm tra danh tính và quyền Quản trị viên từ ngữ cảnh tin cậy
            if (!_securityContext.IsAdmin() || _securityContext.GetActorUserId() <= 0)
            {
                res.Success = false;
                res.Message = "Từ chối truy cập: Thao tác này yêu cầu quyền Quản trị viên hệ thống (Admin).";
                return res;
            }

            int actorId = _securityContext.GetActorUserId();
            string actorUsername = _securityContext.GetActorUsername();
            string clientType = _securityContext.GetClientType();

            // 2. Kiểm tra tính sẵn sàng của Schema CSDL
            if (!_repository.IsSchemaReady(out string notReadyReason))
            {
                res.Success = false;
                res.IsSchemaNotReady = true;
                res.Message = notReadyReason;
                return res;
            }

            // 3. Xác thực đối tượng (Subject)
            string sType = (request.SubjectType ?? "USER").Trim().ToUpperInvariant();
            if (sType != "USER" && sType != "GROUP")
            {
                res.Success = false;
                res.Message = "Loại đối tượng (SubjectType) phải là 'USER' hoặc 'GROUP'.";
                return res;
            }

            if (request.SubjectId <= 0)
            {
                res.Success = false;
                res.Message = "Mã đối tượng (SubjectId) không hợp lệ.";
                return res;
            }

            if (!_repository.SubjectExists(sType, request.SubjectId, out string sCode, out string sName, out int? manv, out bool isDisabled))
            {
                res.Success = false;
                res.Message = $"Không tìm thấy đối tượng {sType} ID={request.SubjectId} trong hệ thống.";
                return res;
            }

            if (request.Grants == null)
            {
                res.Success = false;
                res.Message = "Danh sách phân quyền (Grants) không được để null.";
                return res;
            }

            // 4. Xác thực từng Grant
            var validGrants = new List<AiScopeGrantRecord>();
            var deptOptions = _repository.GetDepartmentOptions() ?? new List<KeyValuePair<string, string>>();
            var compOptions = _repository.GetCompanyOptions() ?? new List<KeyValuePair<string, string>>();
            var validDeptIds = new HashSet<string>(deptOptions.Select(d => d.Key));
            var validCompCodes = new HashSet<string>(compOptions.Select(c => c.Key), StringComparer.OrdinalIgnoreCase);

            var seenGrantKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var g in request.Grants)
            {
                if (g == null) continue;

                if (string.IsNullOrWhiteSpace(g.CapabilityCode) || !AiCapabilityCatalog.Exists(g.CapabilityCode))
                {
                    res.ValidationErrors.Add($"Capability [{g.CapabilityCode}] không tồn tại trong danh mục hệ thống.");
                    continue;
                }

                var capDef = AiCapabilityCatalog.Get(g.CapabilityCode);
                string effect = (g.Effect ?? "NONE").Trim().ToUpperInvariant();

                if (effect == "NONE")
                {
                    // NONE đại diện cho việc xóa hoặc không cấp grant này
                    continue;
                }

                // Không cho phép thêm grant vào capability đang bị tắt ở mức hệ thống
                if (!capDef.IsEnabled)
                {
                    res.ValidationErrors.Add($"Không thể cấp quyền cho [{g.CapabilityCode}] vì nghiệp vụ này đang bị vô hiệu hóa trong hệ thống.");
                    continue;
                }

                if (effect != "ALLOW" && effect != "DENY")
                {
                    res.ValidationErrors.Add($"Loại quyền [{effect}] không hợp lệ cho capability [{g.CapabilityCode}].");
                    continue;
                }

                string scopeType = (g.ScopeType ?? "SELF").Trim().ToUpperInvariant();
                if (new[] { "SELF", "DEPARTMENT", "COMPANY", "ALL" }.All(s => s != scopeType))
                {
                    res.ValidationErrors.Add($"Phạm vi [{scopeType}] không hợp lệ cho capability [{g.CapabilityCode}].");
                    continue;
                }

                string scopeKey = g.ScopeKey?.Trim();

                // Xác thực và chuẩn hóa ScopeKey
                if (scopeType == "SELF" || scopeType == "ALL")
                {
                    scopeKey = null; // Tự động xóa ScopeKey nếu chọn SELF hoặc ALL
                }
                else if (scopeType == "DEPARTMENT")
                {
                    if (string.IsNullOrWhiteSpace(scopeKey) || !validDeptIds.Contains(scopeKey))
                    {
                        res.ValidationErrors.Add($"Phòng ban [{scopeKey}] không tồn tại trong danh mục hệ thống cho capability [{g.CapabilityCode}].");
                        continue;
                    }
                }
                else if (scopeType == "COMPANY")
                {
                    if (string.IsNullOrWhiteSpace(scopeKey) || !validCompCodes.Contains(scopeKey))
                    {
                        res.ValidationErrors.Add($"Công ty [{scopeKey}] không tồn tại trong danh mục hệ thống cho capability [{g.CapabilityCode}].");
                        continue;
                    }
                }

                // Kiểm tra ngày hiệu lực
                if (g.ValidFrom.HasValue && g.ValidTo.HasValue && g.ValidTo.Value <= g.ValidFrom.Value)
                {
                    res.ValidationErrors.Add($"Ngày hết hạn ({g.ValidTo:dd/MM/yyyy HH:mm}) phải sau ngày hiệu lực ({g.ValidFrom:dd/MM/yyyy HH:mm}) cho [{g.CapabilityCode}].");
                    continue;
                }

                // Kiểm tra trùng lặp grant trong cùng một payload
                string grantDuplicateKey = $"{g.CapabilityCode}|{effect}|{scopeType}|{scopeKey ?? ""}";
                if (seenGrantKeys.Contains(grantDuplicateKey))
                {
                    res.ValidationErrors.Add($"Phát hiện cấu hình trùng lặp cho capability [{g.CapabilityCode}] với phạm vi [{scopeType} : {scopeKey}].");
                    continue;
                }
                seenGrantKeys.Add(grantDuplicateKey);

                validGrants.Add(new AiScopeGrantRecord
                {
                    GrantId = g.GrantId ?? 0,
                    SubjectType = sType,
                    SubjectId = request.SubjectId,
                    CapabilityCode = g.CapabilityCode.Trim().ToUpperInvariant(),
                    ScopeType = scopeType,
                    ScopeKey = scopeKey,
                    Effect = effect,
                    IsEnabled = true,
                    ValidFrom = g.ValidFrom,
                    ValidTo = g.ValidTo,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (res.ValidationErrors.Count > 0)
            {
                res.Success = false;
                res.Message = "Có lỗi xác thực dữ liệu: " + string.Join("; ", res.ValidationErrors);
                return res;
            }

            // 5. Lưu vào CSDL qua Repository có đảm bảo Transaction và Concurrency
            long expectedRev = request.BaseRevision ?? 0;
            bool saved = _repository.SaveGrantsTransactional(
                sType,
                request.SubjectId,
                validGrants,
                expectedRev,
                actorId,
                actorUsername,
                clientType,
                out long newRevision,
                out int affectedCount,
                out string errorMessage
            );

            if (!saved)
            {
                res.Success = false;
                if (!string.IsNullOrEmpty(errorMessage) && errorMessage.StartsWith("Xung đột phiên bản"))
                {
                    res.IsConcurrencyConflict = true;
                }
                res.Message = errorMessage;
                return res;
            }

            res.Success = true;
            res.NewPolicyRevision = newRevision;
            res.SavedCount = affectedCount;

            if (errorMessage == "NO_CHANGES")
            {
                res.Message = "Không có thay đổi nào cần lưu đối với phân quyền tra cứu AI.";
            }
            else
            {
                res.Message = $"Lưu cấu hình quyền tra cứu AI thành công ({affectedCount} bản ghi). Phiên bản chính sách mới: {newRevision}.";
            }

            return res;
        }

        public List<KeyValuePair<string, string>> GetDepartmentOptions()
        {
            if (!_securityContext.IsAdmin())
            {
                throw new UnauthorizedAccessException("Chỉ Quản trị viên hệ thống mới có quyền truy cập danh mục phân quyền AI.");
            }
            return _repository.GetDepartmentOptions() ?? new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> GetCompanyOptions()
        {
            if (!_securityContext.IsAdmin())
            {
                throw new UnauthorizedAccessException("Chỉ Quản trị viên hệ thống mới có quyền truy cập danh mục phân quyền AI.");
            }
            return _repository.GetCompanyOptions() ?? new List<KeyValuePair<string, string>>();
        }

        private static string DescribeScope(string scopeType, string scopeName)
        {
            switch (scopeType)
            {
                case "ALL": return "Toàn bộ hệ thống";
                case "SELF": return "Bản thân";
                case "DEPARTMENT": return string.IsNullOrEmpty(scopeName) ? "Phòng ban" : scopeName;
                case "COMPANY": return string.IsNullOrEmpty(scopeName) ? "Công ty" : scopeName;
                default: return scopeType ?? "Chưa rõ";
            }
        }

        private static string FormatScopeKeyName(string scopeType, string scopeKey, Dictionary<string, string> depts, Dictionary<string, string> comps)
        {
            if (string.IsNullOrEmpty(scopeType)) return "";
            if (scopeType == "DEPARTMENT" && !string.IsNullOrEmpty(scopeKey) && depts.TryGetValue(scopeKey, out var dname))
                return $"{dname} (ID: {scopeKey})";
            if (scopeType == "COMPANY" && !string.IsNullOrEmpty(scopeKey) && comps.TryGetValue(scopeKey, out var cname))
                return $"{cname} (Mã: {scopeKey})";
            if (scopeType == "SELF") return "Bản thân";
            if (scopeType == "ALL") return "Toàn bộ";
            return scopeKey ?? "";
        }
    }
}
