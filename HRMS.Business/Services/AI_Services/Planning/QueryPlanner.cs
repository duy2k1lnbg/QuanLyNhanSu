using System;
using System.Collections.Generic;
using System.Linq;
using Bu.Services.AI_Services.Security;

namespace Bu.Services.AI_Services.Core
{
    public interface IQueryPlanner { QueryExecutionPlan CreatePlan(QueryUnderstandingResult understood, AiAuthorizationContext authContext); }
    public class QueryPlanner : IQueryPlanner
    {
        private readonly IClockProvider _clock;
        public QueryPlanner(IClockProvider clock = null) { _clock = clock ?? new SystemClockProvider(); }
        public static string DetermineCapability(QueryUnderstandingResult u, AiAuthorizationContext ctx = null, string question = null)
        {
            bool explicitOther = u.Entities.Any(e => e.EntityType == "EMPLOYEE" && (!e.ResolvedId.HasValue || e.ResolvedId != ctx?.Manv));
            if (question != null && u.RequestedScope != "SELF")
            {
                var id = System.Text.RegularExpressions.Regex.Match(question, @"\b(?:mã nhân viên|mã nv|mã|manv|nv)\s*[:=]?\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (id.Success) explicitOther |= !int.TryParse(id.Groups[1].Value,out var employeeId) || employeeId != ctx?.Manv;
                else explicitOther |= System.Text.RegularExpressions.Regex.IsMatch(question, @"\b(?:anh ấy|cô ấy|người đó|nhân viên đó)\b|(?:của|tên)\s+(?!tôi\b|mình\b)[\p{L}]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            switch (u.Domain)
            {
                case "EMPLOYEE": return u.Operation == "COUNT" ? "EMPLOYEE_COUNT" : u.Metric == "BIRTHDAY" || u.Metric == "PROFILE" ? "EMPLOYEE_PROFILE" : "EMPLOYEE_LOOKUP";
                case "OVERTIME": return new[] { "SUM", "RANK", "TOP" }.Contains(u.Operation) ? "OVERTIME_SUM" : "OVERTIME_VIEW";
                case "PAYROLL": return new[] { "SUM", "SUMMARY" }.Contains(u.Operation) ? "PAYROLL_SUMMARY" : explicitOther ? "PAYROLL_VIEW" : "PAYROLL_SELF";
                case "INSURANCE": return explicitOther ? "INSURANCE_VIEW" : "INSURANCE_SELF";
                case "ATTENDANCE": return u.Operation == "DETAIL" ? "ATTENDANCE_DETAIL" : "ATTENDANCE_SUMMARY";
                case "ALLOWANCE": return "ALLOWANCE_VIEW";
                case "ADVANCE": return "ADVANCE_VIEW";
                case "CONTRACT": return "CONTRACT_VIEW";
                case "SALARY_CHANGE": return "SALARY_CHANGE_VIEW";
                case "POLICY": return "POLICY_LOOKUP";
                default: return null;
            }
        }
        private QueryExecutionPlan Stop(QueryExecutionPlan p, ExecutionStrategy strategy, string reason) { p.Strategy = strategy; p.DenialOrUnsupportedReason = reason; p.SqlStatement = null; return p; }
        public QueryExecutionPlan CreatePlan(QueryUnderstandingResult u, AiAuthorizationContext ctx)
        {
            var p = new QueryExecutionPlan();
            if (u == null) return Stop(p, ExecutionStrategy.Unsupported, "Không có yêu cầu để phân tích.");
            p.Domain = u.Domain; p.Operation = u.Operation; p.Metric = u.Metric; p.RequestedScope = u.RequestedScope; p.Assumptions = u.Assumptions.ToList();
            if (ctx == null || !ctx.IsAuthenticated) return Stop(p, ExecutionStrategy.Forbidden, "Bạn cần đăng nhập để sử dụng trợ lý.");
            if (u.Domain == "GENERAL") { p.Strategy = ExecutionStrategy.DeterministicDirect; return p; }
            p.RequiredCapability = DetermineCapability(u, ctx);
            if (AiCapabilityCatalog.Get(p.RequiredCapability)?.IsEnabled == false) return Stop(p,ExecutionStrategy.Unsupported,"Nguồn dữ liệu hoặc ánh xạ quyền cho chức năng này chưa sẵn sàng.");
            if (!AiAuthorizationService.ValidateCapability(ctx, p.RequiredCapability).IsAllowed || u.SupportStatus == QuerySupportStatus.Forbidden) return Stop(p, ExecutionStrategy.Forbidden, "Bạn chưa được cấp quyền tra cứu nghiệp vụ này.");
            if (u.Domain == "POLICY")
            {
                p.Strategy = ExecutionStrategy.VectorSearch;
                p.EffectiveScopeDisplay = "Quy chế & văn bản chính sách";
                p.AuthorizationFingerprint = ctx.Fingerprint();
                return p;
            }
            if (u.Domain == "EMPLOYEE" && u.Metric != "BIRTHDAY" && (u.Time.Month.HasValue || u.Time.Year.HasValue)) return Stop(p,ExecutionStrategy.Unsupported,"Chưa có snapshot nhân sự theo kỳ; không thể dùng hồ sơ hiện tại thay cho dữ liệu lịch sử.");
            if (u.SupportStatus == QuerySupportStatus.Unsupported) return Stop(p, ExecutionStrategy.Unsupported, u.UnsupportedReason);
            if (u.Time?.AnchorKind == "INVALID") return Stop(p, ExecutionStrategy.Unsupported, "Kỳ thời gian không hợp lệ. Tháng phải từ 1 đến 12, năm từ 1900 đến 2100.");
            if (u.SupportStatus == QuerySupportStatus.NeedsClarification) { p.Strategy = ExecutionStrategy.NeedsClarification; p.Clarification = u.Clarification; return p; }
            var scope = AiScopeEvaluator.BuildSqlScopeFilter(ctx, p.RequiredCapability, u.RequestedScope);
            if (!scope.IsAllowed) return Stop(p, ExecutionStrategy.Forbidden, scope.DenialReason);
            p.Parameters = new Dictionary<string, object>(scope.Parameters);
            p.AuthorizationFingerprint = ctx.Fingerprint();
            p.SourceRevision = ctx.SourceRevisions.TryGetValue(u.Domain, out var rev) ? rev : 0;
            p.EffectiveScopeDisplay = u.RequestedScope == "SELF" || p.RequiredCapability.EndsWith("_SELF") ? "Cá nhân" : "Phạm vi AI được cấp";
            var where = new List<string> { "(" + scope.SqlPredicate + ")" };
            if (ctx.PolicyLoaded && p.RequiredCapability == "EMPLOYEE_COUNT")
            {
                // The database applies scope before aggregation; this view has no employee identities.
                p.Parameters.Clear(); where = new List<string> { "1 = 1" };
            }
            foreach (var e in u.Entities)
            {
                if (e.Status != "RESOLVED" || !e.ResolvedId.HasValue) return Stop(p, ExecutionStrategy.Unsupported, string.IsNullOrWhiteSpace(e.MentionText) ? "Không xác định được đối tượng trong phạm vi được phép. Vui lòng kiểm tra tên hoặc mã." : $"Không tìm thấy nhân viên nào có tên \"{e.MentionText}\" trong phạm vi được phép.");
                if (e.EntityType == "EMPLOYEE")
                {
                    if (p.RequiredCapability == "EMPLOYEE_COUNT") return Stop(p,ExecutionStrategy.Unsupported,"Thống kê số người không trả danh tính nhân viên từ quyền tổng hợp.");
                    if (!AiScopeEvaluator.IsEmployeeInScope(ctx, e.ResolvedId.Value, e.DepartmentId, e.CompanyCode, p.RequiredCapability)) return Stop(p, ExecutionStrategy.Forbidden, "Đối tượng nằm ngoài phạm vi AI được cấp.");
                    where.Add("MANV = :p_manv"); p.Parameters[":p_manv"] = e.ResolvedId.Value; p.SelectedEntityDisplay = e.ResolvedName + " (#" + e.ResolvedId + ")";
                }
                else if (e.EntityType == "DEPARTMENT") { where.Add("IDPB = :p_dept_id"); p.Parameters[":p_dept_id"] = e.ResolvedId.Value; p.EffectiveScopeDisplay += " • " + e.ResolvedName; }
            }
            bool period = new[] { "OVERTIME", "ATTENDANCE", "ADVANCE", "PAYROLL" }.Contains(u.Domain);
            if (period)
            {
                if (!(u.Time?.Month >= 1 && u.Time.Month <= 12 && u.Time.Year >= 1900 && u.Time.Year <= 2100)) return Stop(p, ExecutionStrategy.Unsupported, "Cần xác định đầy đủ tháng và năm trước khi tra cứu.");
                p.Parameters[":p_thang"] = u.Time.Month.Value; p.Parameters[":p_nam"] = u.Time.Year.Value;
                where.Add("THANG = :p_thang"); where.Add("NAM = :p_nam"); p.EffectivePeriodDisplay = $"{u.Time.Month:D2}/{u.Time.Year}";
            }
            string fields = null, order = null, group = null;
            switch (u.Domain)
            {
                case "EMPLOYEE":
                    p.TargetView = u.Operation == "COUNT" ? "V_AI_EMPLOYEE_COUNT" : u.Metric == "BIRTHDAY" || u.Metric == "PROFILE" ? "V_AI_EMPLOYEE" : "V_AI_EMPLOYEE_LOOKUP";
                    if (u.Operation != "COUNT") where.Add("DATHOIVIEC = 0");
                    if (u.Operation == "COUNT") { fields = u.Metric == "HEADCOUNT_BY_DEPT" ? "TEN_PHONGBAN,SUM(TOTAL_COUNT) AS TOTAL_COUNT" : "NVL(SUM(TOTAL_COUNT),0) AS TOTAL_COUNT"; p.IsScalar = u.Metric != "HEADCOUNT_BY_DEPT"; p.ScalarUnit = "nhân viên"; if (!p.IsScalar) { group="IDPB,TEN_PHONGBAN"; order="IDPB"; } }
                    else if (u.Metric == "BIRTHDAY")
                    {
                        if (!(u.Time.BirthdayMonth >= 1 && u.Time.BirthdayMonth <= 12)) return Stop(p, ExecutionStrategy.Unsupported, "Cần xác định tháng sinh nhật.");
                        p.Parameters[":p_bday_month"] = u.Time.BirthdayMonth.Value; where.Add("BIRTHDAY_MONTH = :p_bday_month"); fields = "MANV,HOTEN,TEN_PHONGBAN,TEN_CHUCVU,BIRTHDAY_DAY,BIRTHDAY_MONTH"; order = "BIRTHDAY_DAY,MANV"; p.EffectivePeriodDisplay = "Sinh nhật tháng " + u.Time.BirthdayMonth;
                    }
                    else { fields = "MANV,HOTEN,TEN_PHONGBAN,TEN_CHUCVU"; order = "MANV"; if (u.Metric == "PROFILE") fields += ",NGAYSINH,DIENTHOAI,DIACHI"; }
                    break;
                case "OVERTIME":
                    p.TargetView = p.RequiredCapability == "OVERTIME_SUM" ? "V_AI_OVERTIME_SUMMARY" : "V_AI_OVERTIME";
                    if (u.Operation == "SUM") { fields = "NVL(SUM(SOGIO),0) AS TONG_SOGIO"; p.IsScalar = true; p.ScalarUnit = "giờ"; }
                    else if (u.Operation == "RANK" || u.Operation == "TOP") { fields = "MANV,HOTEN,TEN_PHONGBAN,SUM(SOGIO) AS TONG_SOGIO"; group = "MANV,HOTEN,TEN_PHONGBAN"; order = "TONG_SOGIO DESC,MANV"; p.RowLimit = 5; }
                    else { fields = "MANV,HOTEN,TEN_PHONGBAN,SOGIO,NGAY,THANG,NAM"; order = "NAM,THANG,NGAY,MANV,IDTCA"; }
                    break;
                case "ALLOWANCE":
                    p.TargetView = "V_AI_ALLOWANCE"; fields = "MANV,HOTEN,TEN_PHONGBAN,TENPC,SOTIEN,TU_NGAY,DEN_NGAY"; order = "MANV,IDPC";
                    if (u.Time.Month.HasValue)
                    {
                        if (!u.Time.Year.HasValue) return Stop(p, ExecutionStrategy.Unsupported, "Cần xác định năm áp dụng phụ cấp.");
                        var start = new DateTime(u.Time.Year.Value,u.Time.Month.Value,1); p.Parameters[":p_start"] = start; p.Parameters[":p_end"] = start.AddMonths(1);
                        where.Add("(TU_NGAY IS NULL OR TU_NGAY < :p_end)"); where.Add("(DEN_NGAY IS NULL OR DEN_NGAY >= :p_start)"); p.EffectivePeriodDisplay = start.ToString("MM/yyyy");
                    }
                    break;
                case "PAYROLL":
                    if (u.Metric == "LUONG_COBAN") return Stop(p, ExecutionStrategy.Unsupported, "Chưa có ánh xạ lương cơ bản tháng được xác nhận.");
                    if (u.Operation == "SUMMARY" || u.Operation == "SUM")
                    {
                        if (u.Entities.Any(e => e.EntityType == "EMPLOYEE") || u.RequestedScope == "SELF") return Stop(p, ExecutionStrategy.Unsupported, "Quỹ lương tổng hợp cần phạm vi tổ chức đã được cấp rõ ràng.");
                        p.TargetView = "V_AI_PAYROLL_SUMMARY"; fields = u.Metric == "LUONG_CONG_THUCTE" ? "NVL(SUM(TONG_LUONG_CONG_THUCTE),0) AS TONG_LUONG_CONG_THUCTE,SUM(SO_NHANVIEN) AS SO_NHANVIEN" : "NVL(SUM(TONG_THUCLANH),0) AS TONG_THUCLANH,SUM(SO_NHANVIEN) AS SO_NHANVIEN"; p.IsScalar = true;
                    }
                    else { p.TargetView = "V_AI_PAYROLL"; fields = "MANV,HOTEN,TEN_PHONGBAN,THANG,NAM,MAKYCONG," + (u.Metric == "LUONG_CONG_THUCTE" ? "LUONG_CONG_THUCTE" : "THUCLANH") + ",NGAYCONG_THUCTE,TRANGTHAI_CHITRA,IS_LOCKED"; order = "MAKYCONG,MANV"; }
                    break;
                case "ATTENDANCE":
                    if (u.Operation == "DETAIL") return Stop(p, ExecutionStrategy.Unsupported, "Nguồn lượt vào/ra hàng ngày chưa được ánh xạ cho AI. Bạn có thể hỏi tổng ngày công hoặc ngày phép theo kỳ.");
                    p.TargetView = "V_AI_ATTENDANCE_SUMMARY"; fields = "MAKYCONG,MANV,HOTEN,TEN_PHONGBAN,THANG,NAM,TONGNGAYCONG,NGAYPHEP,NGHIKHONGPHEP,CONGNGAYLE,CONGCHUNHAT"; order = "MAKYCONG,MANV"; break;
                case "CONTRACT":
                    p.TargetView = "V_AI_CONTRACT"; fields = "SOHD,MANV,HOTEN,TEN_PHONGBAN,NGAYBATDAU,NGAYKETTHUC,NGAYKY,LANKY,HESOLUONG"; order = "NGAYKETTHUC,SOHD";
                    if (u.Operation == "EXPIRING") { var start = u.Time.StartDate ?? _clock.Now.Date; var end = u.Time.EndDate ?? start.AddDays(31); p.Parameters[":p_today"] = start; p.Parameters[":p_limit_date"] = end; where.Add("NGAYKETTHUC >= :p_today"); where.Add("NGAYKETTHUC < :p_limit_date"); p.EffectivePeriodDisplay = start.ToString("dd/MM/yyyy") + "–" + end.AddDays(-1).ToString("dd/MM/yyyy"); }
                    break;
                case "SALARY_CHANGE":
                    p.TargetView = "V_AI_SALARY_CHANGE"; fields = "SOQD,SOHD,MANV,HOTEN,TEN_PHONGBAN,NGAYKY,NGAYLENLUONG,HESOLUONGHIENTAI,HESOLUONGMOI"; order = "NGAYLENLUONG DESC,SOQD";
                    if (u.Time.Month.HasValue) { if (!u.Time.Year.HasValue) return Stop(p, ExecutionStrategy.Unsupported, "Cần xác định năm của quyết định nâng lương."); var start = new DateTime(u.Time.Year.Value,u.Time.Month.Value,1); p.Parameters[":p_start"] = start; p.Parameters[":p_end"] = start.AddMonths(1); where.Add("NGAYLENLUONG >= :p_start AND NGAYLENLUONG < :p_end"); }
                    if (!u.Time.Month.HasValue && u.Time.StartDate.HasValue) { p.Parameters[":p_start"] = u.Time.StartDate.Value; where.Add("NGAYLENLUONG >= :p_start"); }
                    break;
                case "INSURANCE": p.TargetView = "V_AI_INSURANCE"; fields = "MANV,HOTEN,SOBH,NGAYCAP,NOICAP,NOIKHAMBENH"; order = "MANV,IDBH"; break;
                case "ADVANCE": p.TargetView = "V_AI_ADVANCE"; fields = "MANV,HOTEN,NGAY,THANG,NAM,SOTIEN"; order = "NAM,THANG,NGAY,MANV,IDUL"; break;
                default: return Stop(p, ExecutionStrategy.Unsupported, "Nghiệp vụ chưa được hỗ trợ.");
            }
            if (u.Filters.Count > 0)
            {
                if ((u.Domain != "ALLOWANCE" && u.Domain != "ADVANCE") || u.Filters.Count != 1) return Stop(p, ExecutionStrategy.Unsupported, "Chỉ hỗ trợ một điều kiện số tiền cho phụ cấp hoặc tạm ứng; chưa hỗ trợ khoảng tiền hay bộ lọc tiền cho nghiệp vụ này.");
                var f = u.Filters[0];
                if (f.Field != "SOTIEN" || !new[] { "<", ">", "<=", ">=", "=" }.Contains(f.Operator)) return Stop(p, ExecutionStrategy.Unsupported, "Điều kiện lọc chưa được hỗ trợ.");
                if (AiAuthorizationService.FieldMode(ctx,p.RequiredCapability,"SOTIEN") != "FULL" || !AiAuthorizationService.FieldOperationAllowed(ctx,p.RequiredCapability,"SOTIEN","FILTER")) return Stop(p, ExecutionStrategy.Forbidden,"Không được phép lọc theo trường số tiền.");
                p.Parameters[":p_sotien"] = Convert.ToDecimal(f.Value); where.Add("SOTIEN " + f.Operator + " :p_sotien");
            }
            // Every projected or aggregated logical field must be approved before SQL execution.
            var identifiers = System.Text.RegularExpressions.Regex.Matches(fields, @"\b[A-Z][A-Z0-9_]*\b").Cast<System.Text.RegularExpressions.Match>().Select(m => m.Value).Distinct();
            foreach (var f in identifiers.Where(f => f != "AS" && f != "NVL" && f != "SUM" && f != "COUNT"))
            {
                var mode = AiAuthorizationService.FieldMode(ctx,p.RequiredCapability,f); p.FieldModes[f] = mode;
                if (mode == "DENY" || (p.IsScalar && mode != "FULL") || !AiAuthorizationService.FieldOperationAllowed(ctx,p.RequiredCapability,f,u.Operation)) return Stop(p, ExecutionStrategy.Forbidden, "Chính sách trường dữ liệu chưa cho phép trả lời yêu cầu này.");
                if (mode == "MASK") p.MaskedFields.Add(f);
            }
            p.Strategy = ExecutionStrategy.SqlTemplate;
            string q = u.OriginalQuestion ?? u.NormalizedQuestion ?? "";
            if (q.IndexOf("quy định", StringComparison.OrdinalIgnoreCase) >= 0 ||
                q.IndexOf("chính sách", StringComparison.OrdinalIgnoreCase) >= 0 ||
                q.IndexOf("quy chế", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                p.Strategy = ExecutionStrategy.Hybrid;
                p.VectorCapability = "POLICY_LOOKUP";
                p.VectorQuery = q;
            }
            p.SqlStatement = "SELECT " + fields + (!p.IsScalar ? ",COUNT(*) OVER() AS AI_TOTAL_COUNT" : "") + " FROM AI_OWNER." + p.TargetView + " WHERE " + string.Join(" AND ",where) + (group != null ? " GROUP BY " + group : "") + (order != null ? " ORDER BY " + order : "") + (!p.IsScalar ? " FETCH FIRST " + (p.RowLimit + 1) + " ROWS ONLY" : "");
            var validation = OracleSqlAstValidator.Validate(p.SqlStatement);
            if (!validation.IsValid) return Stop(p,ExecutionStrategy.Unsupported,"Không thể lập truy vấn an toàn cho yêu cầu này.");
            return p;
        }
    }
}