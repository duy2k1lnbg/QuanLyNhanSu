using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Services.AI_Services.Security
{
    public enum AiScopeType { Self, Department, Company, All }
    public class ScopePredicateResult
    {
        public bool IsAllowed { get; set; }
        public string SqlPredicate { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public string DenialReason { get; set; }
    }
    public static class AiScopeEvaluator
    {
        public static List<AiScopeGrant> Grants(AiAuthorizationContext ctx, string cap)
        {
            if (ctx.PolicyLoaded || (ctx.ScopeGrants != null && ctx.ScopeGrants.Count > 0))
                return ctx.ScopeGrants.Where(g => string.Equals(g.CapabilityCode, cap, StringComparison.OrdinalIgnoreCase)).ToList();
            // Explicit, server-created scope fixtures / legacy integration only. IsAdmin and '*' confer no data scope.
            var grants = new List<AiScopeGrant>();
            if (ctx.HasAllScope) grants.Add(new AiScopeGrant { ScopeType = "ALL", Effect = "ALLOW" });
            if (ctx.HasCompanyScope && !string.IsNullOrEmpty(ctx.MaCty)) grants.Add(new AiScopeGrant { ScopeType = "COMPANY", ScopeKey = ctx.MaCty, Effect = "ALLOW" });
            foreach (var id in ctx.AllowedDepartmentIds) grants.Add(new AiScopeGrant { ScopeType = "DEPARTMENT", ScopeKey = id.ToString(), Effect = "ALLOW" });
            if (ctx.Manv > 0) grants.Add(new AiScopeGrant { ScopeType = "SELF", Effect = "ALLOW" });
            return grants;
        }
        public static ScopePredicateResult BuildSqlScopeFilter(AiAuthorizationContext ctx, string capabilityCode, string requestedScope = null)
        {
            var r = new ScopePredicateResult { DenialReason = "Tài khoản chưa được cấp phạm vi truy vấn AI phù hợp." };
            if (ctx == null || !ctx.IsAuthenticated || AiCapabilityCatalog.Get(capabilityCode) == null) return r;
            var grants = Grants(ctx, capabilityCode);
            var now = DateTime.Now;

            // PKG_AI_AUTH rule: IF v_deny_grant > 0 THEN RAISE_APPLICATION_ERROR(-20008, 'Access denied: Explicit DENY scope policy matches actor.');
            if (grants.Any(g => string.Equals(g.Effect, "DENY", StringComparison.OrdinalIgnoreCase) &&
                (!g.ValidFrom.HasValue || g.ValidFrom.Value <= now) &&
                (!g.ValidTo.HasValue || g.ValidTo.Value > now)))
            {
                r.IsAllowed = false;
                r.DenialReason = "Access denied: Explicit DENY scope policy matches actor.";
                return r;
            }

            bool self = capabilityCode.EndsWith("_SELF", StringComparison.OrdinalIgnoreCase) || requestedScope == "SELF";
            var allow = new List<string>(); var deny = new List<string>(); int n = 0;
            foreach (var g in grants)
            {
                // Bỏ qua các grant chưa đến ngày hiệu lực hoặc đã hết hạn
                if (g.ValidFrom.HasValue && g.ValidFrom.Value > now) continue;
                if (g.ValidTo.HasValue && g.ValidTo.Value <= now) continue;

                string clause = null;
                string param = ":p_scope_" + n++;
                switch (g.ScopeType)
                {
                    case "ALL": clause = "1 = 1"; break;
                    case "SELF":
                        if (capabilityCode == "PAYROLL_SUMMARY") break;
                        if (ctx.Manv > 0) { clause = "MANV = " + param; r.Parameters[param] = ctx.Manv.Value; }
                        break;
                    case "DEPARTMENT":
                        if (int.TryParse(g.ScopeKey, out var dept) && dept > 0) { clause = "IDPB = " + param; r.Parameters[param] = dept; }
                        break;
                    case "COMPANY":
                        if (!string.IsNullOrWhiteSpace(g.ScopeKey)) { clause = "MACTY = " + param; r.Parameters[param] = g.ScopeKey; }
                        break;
                }
                if (clause == null) continue;
                if (g.Effect == "DENY") deny.Add(clause); else if (g.Effect == "ALLOW") allow.Add(clause);
            }
            if (allow.Count == 0) return r;
            string predicate = "(" + string.Join(" OR ", allow) + ")";
            if (deny.Count > 0) predicate += " AND NOT (" + string.Join(" OR ", deny) + ")";
            if (self)
            {
                if (!(ctx.Manv > 0)) return r;
                predicate += " AND MANV = :p_scope_self";
                r.Parameters[":p_scope_self"] = ctx.Manv.Value;
            }
            r.IsAllowed = true; r.SqlPredicate = predicate; r.DenialReason = null;
            return r;
        }
        public static bool IsEmployeeInScope(AiAuthorizationContext ctx, int targetManv, int? targetDeptId = null, string targetCompany = null, string capabilityCode = "EMPLOYEE_LOOKUP")
        {
            if (ctx == null || !ctx.IsAuthenticated) return false;
            var now = DateTime.Now;
            var grants = Grants(ctx, capabilityCode)
                .Where(g => (!g.ValidFrom.HasValue || g.ValidFrom.Value <= now) && (!g.ValidTo.HasValue || g.ValidTo.Value > now))
                .ToList();

            Func<AiScopeGrant, bool> matches = g => g.ScopeType == "ALL" ||
                (g.ScopeType == "SELF" && ctx.Manv == targetManv) ||
                (g.ScopeType == "DEPARTMENT" && targetDeptId.HasValue && g.ScopeKey == targetDeptId.Value.ToString()) ||
                (g.ScopeType == "COMPANY" && targetCompany != null && g.ScopeKey == targetCompany);

            // DENY wins: Nếu bất kỳ grant DENY nào khớp với nhân viên này -> chặn
            if (grants.Any(g => string.Equals(g.Effect, "DENY", StringComparison.OrdinalIgnoreCase) && matches(g)))
            {
                return false;
            }

            // Với các capability tự tra cứu (_SELF): luôn ràng buộc đúng nhân viên hiện hành
            if (capabilityCode != null && capabilityCode.EndsWith("_SELF", StringComparison.OrdinalIgnoreCase))
            {
                if (!ctx.Manv.HasValue || ctx.Manv.Value <= 0) return false;
                return ctx.Manv.Value == targetManv;
            }

            return grants.Any(g => string.Equals(g.Effect, "ALLOW", StringComparison.OrdinalIgnoreCase) && matches(g));
        }
    }
}