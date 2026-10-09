using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Bu.Services.AI_Services.Memory;

namespace Bu.Services.AI_Services.Security
{
    public class AiScopeGrant
    {
        public string CapabilityCode { get; set; }
        public string ScopeType { get; set; }
        public string ScopeKey { get; set; }
        public string Effect { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
    public class AiPolicyCapability
    {
        public string CapabilityCode { get; set; }
        public string RequiredFunctionCode { get; set; }
        public string SourceView { get; set; }
        public bool Enabled { get; set; }
    }
    // Only the server-side policy provider constructs production snapshots.
    public class AiAuthorizationContext
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public int? Manv { get; set; }
        public string MaCty { get; set; }
        public bool IsAdmin { get; set; }
        public HashSet<string> FunctionRights { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<int> AllowedDepartmentIds { get; set; } = new HashSet<int>();
        public bool HasCompanyScope { get; set; }
        public bool HasAllScope { get; set; }
        public long PolicyVersion { get; set; }
        public bool PolicyLoaded { get; set; }
        public string LookupCapability { get; set; } = "EMPLOYEE_LOOKUP";
        public Dictionary<string, AiPolicyCapability> Capabilities { get; set; } = new Dictionary<string, AiPolicyCapability>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> FieldPolicies { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> FieldOperations { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public List<AiScopeGrant> ScopeGrants { get; set; } = new List<AiScopeGrant>();
        public Dictionary<string, long> SourceRevisions { get; set; } = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        public bool IsAuthenticated => UserId > 0;
        public bool HasFunctionRight(string code) => string.IsNullOrWhiteSpace(code) || (FunctionRights != null && FunctionRights.Contains(code.Trim()));
        public string Fingerprint()
        {
            return AiCacheCoordinator.HashKey("auth", JsonConvert.SerializeObject(new {
                UserId, Manv, MaCty, PolicyVersion, PolicyLoaded, HasAllScope, HasCompanyScope,
                Rights = (FunctionRights ?? new HashSet<string>()).OrderBy(x => x, StringComparer.Ordinal).ToArray(),
                Departments = AllowedDepartmentIds.OrderBy(x => x).ToArray(),
                Caps = Capabilities.OrderBy(x => x.Key, StringComparer.Ordinal).ToArray(),
                Operations = FieldOperations.OrderBy(x => x.Key, StringComparer.Ordinal).ToArray(),
                Fields = FieldPolicies.OrderBy(x => x.Key, StringComparer.Ordinal).ToArray(),
                Grants = ScopeGrants.OrderBy(x => x.CapabilityCode).ThenBy(x => x.ScopeType).ThenBy(x => x.ScopeKey).ThenBy(x => x.Effect).ToArray()
            }));
        }
        public static AiAuthorizationContext CreateAnonymous() => new AiAuthorizationContext();
    }
}