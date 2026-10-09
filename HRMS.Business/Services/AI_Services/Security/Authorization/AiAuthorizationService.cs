using System;
using System.Collections.Generic;
using System.Linq;
namespace Bu.Services.AI_Services.Security
{
    public class CapabilityCheckResult
    {
        public bool IsAllowed { get; set; }
        public string CapabilityCode { get; set; }
        public string SourceView { get; set; }
        public bool IsAggregateOnly { get; set; }
        public string DenialReason { get; set; }
    }
    public class AiAuthorizationService
    {
        public CapabilityCheckResult CheckCapability(AiAuthorizationContext ctx, string code)
        {
            var r = new CapabilityCheckResult { CapabilityCode = code, DenialReason = "Bạn chưa được cấp quyền tra cứu nghiệp vụ này." };
            if (ctx == null || !ctx.IsAuthenticated) return r;
            var cap = AiCapabilityCatalog.Get(code); if (cap == null || !cap.IsEnabled) return r;
            string required = cap.RequiredFunctionCode;
            if (ctx.PolicyLoaded)
            {
                if (!ctx.Capabilities.TryGetValue(code, out var policy) || !policy.Enabled || policy.SourceView != cap.SourceView) return r;
                required = policy.RequiredFunctionCode;
                // A policy may restrict a compiled capability but cannot remove its function-right requirement.
                if (!ctx.HasFunctionRight(cap.RequiredFunctionCode)) return r;
            }
            if (!ctx.HasFunctionRight(required)) return r;

            // Kiểm tra DENY và thời hạn hiệu lực của Scope Grant
            var now = DateTime.Now;
            if (ctx.ScopeGrants != null && ctx.ScopeGrants.Any(g =>
                string.Equals(g.CapabilityCode, code, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(g.Effect, "DENY", StringComparison.OrdinalIgnoreCase) &&
                (!g.ValidTo.HasValue || g.ValidTo.Value > now)))
            {
                r.IsAllowed = false;
                r.DenialReason = "Access denied: Explicit DENY scope policy matches actor.";
                return r;
            }

            r.IsAllowed = true; r.SourceView = cap.SourceView; r.IsAggregateOnly = cap.IsAggregateOnly; r.DenialReason = null; return r;
        }
        public static string FieldMode(AiAuthorizationContext ctx, string cap, string field)
        {
            var upper = (field ?? "").ToUpperInvariant();
            if (new[] { "PASSWORD", "PASS", "MATKHAU", "SALT", "TOKEN", "REFRESH_TOKEN", "CCCD", "CMND" }.Any(x => upper.Contains(x))) return "DENY";
            if (ctx != null && ctx.PolicyLoaded)
            {
                return ctx.FieldPolicies.TryGetValue(cap + ":" + upper, out var mode) && new[] { "FULL", "MASK", "DENY" }.Contains(mode) ? mode : "DENY";
            }
            return new[] { "NGAYSINH", "DIENTHOAI", "DIACHI", "SOBH" }.Contains(upper) ? "MASK" : "FULL";
        }
        public static bool FieldOperationAllowed(AiAuthorizationContext ctx, string cap, string field, string operation)
        {
            if (FieldMode(ctx, cap, field) == "DENY") return false;
            if (ctx == null || !ctx.PolicyLoaded || !ctx.FieldOperations.TryGetValue(cap + ":" + field.ToUpperInvariant(), out var configured) || string.IsNullOrWhiteSpace(configured)) return true;
            return configured.Split(',').Select(x => x.Trim()).Any(x => string.Equals(x, operation, StringComparison.OrdinalIgnoreCase));
        }
        public string GetFieldAccessMode(string cap, string field) => FieldMode(null, cap, field);
        public static CapabilityCheckResult ValidateCapability(AiAuthorizationContext ctx, string code) => new AiAuthorizationService().CheckCapability(ctx, code);
        public static List<string> GetProhibitedFields(AiAuthorizationContext ctx, string cap) => new[] { "PASSWORD", "MATKHAU", "TOKEN", "CCCD", "CMND", "NGAYSINH", "DIENTHOAI", "DIACHI", "SOBH" }.Where(f => FieldMode(ctx,cap,f) != "FULL").ToList();
    }
}