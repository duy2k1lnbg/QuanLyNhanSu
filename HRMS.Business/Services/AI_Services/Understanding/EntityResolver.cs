using System;
using System.Collections.Generic;
using System.Linq;
using Bu.Services.AI_Services.Security;
using DA;
using Oracle.ManagedDataAccess.Client;

namespace Bu.Services.AI_Services.Core
{
    public interface IEntityLookupProvider
    {
        List<EntityCandidate> FindEmployees(string query, AiAuthorizationContext ctx);
        EntityCandidate FindEmployeeById(int manv, AiAuthorizationContext ctx);
        EntityCandidate FindDepartmentByName(string deptName, AiAuthorizationContext ctx);
    }

    public class EmployeeLookupRow
    {
        public decimal MANV { get; set; }
        public string HOTEN { get; set; }
        public string TEN_PHONGBAN { get; set; }
        public string TEN_CHUCVU { get; set; }
        public decimal? IDPB { get; set; }
        public string MACTY { get; set; }
    }

    public class DepartmentLookupRow
    {
        public decimal IDPB { get; set; }
        public string TENPB { get; set; }
        public string MACTY { get; set; }
    }

    public interface IOrganizationLookupProvider { List<EntityCandidate> FindDepartments(string name, AiAuthorizationContext ctx); }
    public class DatabaseEntityLookupProvider : IEntityLookupProvider, IOrganizationLookupProvider
    {
        private readonly ScopedSqlExecutor _executor = new ScopedSqlExecutor();
        private static string EscapeLike(string value) => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
        private List<EntityCandidate> Employees(string query, int? id, AiAuthorizationContext ctx)
        {
            var scope = AiScopeEvaluator.BuildSqlScopeFilter(ctx,ctx.LookupCapability);
            if (!scope.IsAllowed) return new List<EntityCandidate>();
            var parameters = new Dictionary<string,object>(scope.Parameters);
            string target = id.HasValue ? "MANV = :lookup_id" : "UPPER(HOTEN) LIKE UPPER(:lookup_name)";
            if (id.HasValue) parameters[":lookup_id"]=id.Value; else parameters[":lookup_name"]="%"+query.Trim()+"%";
            var result = _executor.ExecuteAuthorizedQuery("SELECT MANV,HOTEN,TEN_PHONGBAN,TEN_CHUCVU,IDPB,MACTY FROM AI_OWNER.V_AI_EMPLOYEE_LOOKUP WHERE ("+scope.SqlPredicate+") AND "+target+" ORDER BY MANV FETCH FIRST 21 ROWS ONLY",parameters,ctx,ctx.LookupCapability);
            if (result.Status != Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.SuccessWithData && result.Status != Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.SuccessEmpty) throw new AiSourceUnavailableException();
            return result.Data.Rows.Cast<System.Data.DataRow>().Select(r => new EntityCandidate { Id=Convert.ToInt32(r["MANV"]), Code=Convert.ToString(r["MANV"]), Name=Convert.ToString(r["HOTEN"]), DepartmentId=r["IDPB"]==DBNull.Value ? (int?)null : Convert.ToInt32(r["IDPB"]), CompanyCode=Convert.ToString(r["MACTY"]), DepartmentName=Convert.ToString(r["TEN_PHONGBAN"]), PositionName=Convert.ToString(r["TEN_CHUCVU"]) }).ToList();
        }
        public List<EntityCandidate> FindEmployees(string query,AiAuthorizationContext ctx) => string.IsNullOrWhiteSpace(query) ? new List<EntityCandidate>() : Employees(query,null,ctx);
        public EntityCandidate FindEmployeeById(int id,AiAuthorizationContext ctx) => id>0 ? Employees(null,id,ctx).SingleOrDefault() : null;
        public EntityCandidate FindDepartmentByName(string name,AiAuthorizationContext ctx) { var rows=FindDepartments(name,ctx); return rows.Count==1 ? rows[0] : null; }
        public List<EntityCandidate> FindDepartments(string name,AiAuthorizationContext ctx)
        {
            var result = _executor.ExecuteAuthorizedQuery("SELECT ENTITY_ID,ENTITY_NAME FROM AI_OWNER.V_AI_ORG_LOOKUP WHERE ENTITY_TYPE = 'DEPARTMENT' AND UPPER(ENTITY_NAME) LIKE UPPER(:lookup_name) ORDER BY ENTITY_ID FETCH FIRST 21 ROWS ONLY",new Dictionary<string,object>{{":lookup_name","%"+name.Trim()+"%"}},ctx,ctx.LookupCapability);
            if (result.Status != Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.SuccessWithData && result.Status != Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.SuccessEmpty) throw new AiSourceUnavailableException();
            return result.Data.Rows.Cast<System.Data.DataRow>().Select(r => new EntityCandidate { Id=Convert.ToInt32(r["ENTITY_ID"]), Name=Convert.ToString(r["ENTITY_NAME"]) }).ToList();
        }
    }
    public class EntityResolver
    {
        private readonly IEntityLookupProvider _provider;

        public EntityResolver(IEntityLookupProvider provider = null)
        {
            _provider = provider ?? new DatabaseEntityLookupProvider();
        }

        public EntityMention ResolveEmployee(string mentionText, AiAuthorizationContext ctx)
        {
            var mention = new EntityMention
            {
                MentionText = mentionText,
                EntityType = "EMPLOYEE",
                Status = "NOT_FOUND"
            };

            if (string.IsNullOrWhiteSpace(mentionText)) return mention;

            // 1. Kiểm tra nếu là số mã nhân viên
            if (int.TryParse(mentionText.Trim(), out int id) && id > 0)
            {
                var cand = _provider.FindEmployeeById(id, ctx);
                if (cand != null && AiScopeEvaluator.IsEmployeeInScope(ctx,cand.Id,cand.DepartmentId,cand.CompanyCode,ctx.LookupCapability))
                {
                    mention.ResolvedId = cand.Id;
                    mention.ResolvedName = cand.Name;
                    mention.DepartmentId = cand.DepartmentId;
                    mention.CompanyCode = cand.CompanyCode;
                    mention.Status = "RESOLVED";
                    mention.Candidates.Add(cand);
                    return mention;
                }
                return mention;
            }

            // 2. Tra cứu theo tên
            var candidates = _provider.FindEmployees(mentionText, ctx).Where(c => AiScopeEvaluator.IsEmployeeInScope(ctx,c.Id,c.DepartmentId,c.CompanyCode,ctx.LookupCapability)).ToList();

            if (candidates.Count == 1)
            {
                var single = candidates[0];
                mention.ResolvedId = single.Id;
                mention.ResolvedName = single.Name;
                mention.DepartmentId = single.DepartmentId;
                mention.CompanyCode = single.CompanyCode;
                mention.Status = "RESOLVED";
                mention.Candidates.Add(single);
            }
            else if (candidates.Count > 1)
            {
                mention.Status = "AMBIGUOUS";
                mention.Candidates = candidates;
            }
            else
            {
                mention.Status = "NOT_FOUND";
            }

            return mention;
        }

        public EntityMention ResolveDepartment(string name, AiAuthorizationContext ctx)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var candidates = _provider is IOrganizationLookupProvider org ? org.FindDepartments(name,ctx) : new List<EntityCandidate>();
            if (!(_provider is IOrganizationLookupProvider)) { var one=_provider.FindDepartmentByName(name,ctx); if (one!=null) candidates.Add(one); }
            var mention=new EntityMention { MentionText=name, EntityType="DEPARTMENT", Status=candidates.Count==0 ? "NOT_FOUND" : candidates.Count==1 ? "RESOLVED" : "AMBIGUOUS", Candidates=candidates };
            if (candidates.Count==1) { mention.ResolvedId=candidates[0].Id; mention.ResolvedName=candidates[0].Name; }
            return mention;
        }
    }
}