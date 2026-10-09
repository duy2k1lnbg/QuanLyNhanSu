using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Security;
using Bu.Tests;
using NUnit.Framework;

namespace HRMS.Tests
{
    public static class AiTestContexts
    {
        public static HashSet<string> Rights() => new HashSet<string>(new[] { "F_SYSTEM_AI","F_DM_NHANVIEN","F_CC_TANGCA","F_CC_BANGCONG","F_CC_PHUCAP","F_CC_UNGLUONG","F_CC_BANGLUONG","F_NV_HOPDONG","F_NV_NANGLUONG" },StringComparer.OrdinalIgnoreCase);
        public static AiAuthorizationContext All(int user=1) => new AiAuthorizationContext { UserId=user, Manv=10, HasAllScope=true, FunctionRights=Rights(), PolicyVersion=1, SourceRevisions=new Dictionary<string,long>{{"EMPLOYEE",1}} };
    }
    public class RegressionSqlExecutor : IScopedSqlExecutor
    {
        public int Calls;
        public string LastSql;
        public Dictionary<string,object> LastParameters;
        public TaskCompletionSource<bool> Release;
        public SqlExecutionStatus Status=SqlExecutionStatus.SuccessWithData;
        public SqlExecutionResult ExecuteScopedQuery(string sql,Dictionary<string,object> parameters=null,CancellationToken cancellationToken=default)
        {
            LastSql=sql; LastParameters=parameters; var table=new DataTable();
            if (sql.Contains("TONG_SOGIO")) { table.Columns.Add("TONG_SOGIO",typeof(decimal));table.Rows.Add(24.5m); }
            else if (sql.Contains("TOTAL_COUNT")) { table.Columns.Add("TOTAL_COUNT",typeof(int));table.Rows.Add(15); }
            else if (sql.Contains("TONG_THUCLANH")) { table.Columns.Add("TONG_THUCLANH",typeof(decimal));table.Columns.Add("SO_NHANVIEN",typeof(int));table.Rows.Add(12000000m,2); }
            else { table.Columns.Add("MANV",typeof(int));table.Columns.Add("HOTEN",typeof(string));table.Rows.Add(10,"Nguyễn Văn An"); }
            return new SqlExecutionResult { Status=Status,Data=table,TotalRecords=table.Rows.Count };
        }
        public async Task<SqlExecutionResult> ExecuteScopedQueryAsync(string sql,Dictionary<string,object> parameters=null,CancellationToken cancellationToken=default)
        { Interlocked.Increment(ref Calls); if (Release!=null) await Release.Task; cancellationToken.ThrowIfCancellationRequested();return ExecuteScopedQuery(sql,parameters,cancellationToken); }
    }
    [TestFixture]
    public class AiProductionFlowRegressionTests
    {
        private FakeClockProvider clock;
        private ConversationStateManager sessions;
        private AiCacheCoordinator cache;
        private RegressionSqlExecutor sql;
        private AiExecutionService service;
        private AiAuthorizationContext ctx;
        [SetUp] public void Setup()
        {
            clock=new FakeClockProvider(new DateTime(2026,10,3,9,0,0,DateTimeKind.Utc)); sessions=new ConversationStateManager(clock);cache=new AiCacheCoordinator(clock);sql=new RegressionSqlExecutor();ctx=AiTestContexts.All();
            service=Create();
        }
        private AiExecutionService Create(IAiPolicyProvider policy=null) => new AiExecutionService(new QueryUnderstandingService(clock,new EntityResolver(new FakeEntityLookupProvider())),sqlExecutor:sql,conversationManager:sessions,cacheCoordinator:cache,clock:clock,policyProvider:policy);
        private Task<AiChatExecutionResult> Ask(string q,string id="a") => service.ProcessChatAsync(q,ctx,id);
        [Test] public void ProtectedViewsReferenceCurrentModelsOrDocumentedMigrationColumns()
        {
            var root=new System.IO.DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while(root!=null && !System.IO.Directory.Exists(System.IO.Path.Combine(root.FullName,"database","migrations"))) root=root.Parent;
            Assert.IsNotNull(root,"Run schema contract tests from the source checkout.");
            var edmx=System.IO.File.ReadAllText(System.IO.Path.Combine(root.FullName,"HRMS.DataAccess","QLNhanSu.edmx"));
            var v16=System.IO.File.ReadAllText(System.IO.Path.Combine(root.FullName,"database","migrations","V1_16__payroll_production_policies_and_itemized_details.sql"));
            var v18=System.IO.File.ReadAllText(System.IO.Path.Combine(root.FullName,"database","migrations","V1_18__attendance_scheduling_and_segmentation.sql"));
            var v20=System.IO.File.ReadAllText(System.IO.Path.Combine(root.FullName,"database","migrations","V1_20__payroll_payment_status_and_audit.sql"));
            StringAssert.Contains("CONG_INPUT_REV NUMBER",v18);StringAssert.Contains("CONG_PUBLISH_REV NUMBER",v18);
            StringAssert.Contains("TB_KYCONG ADD",v20);StringAssert.Contains("RUN_ID NUMBER",v16);StringAssert.Contains("TRANGTHAI_CHITRA VARCHAR2",v20);
            foreach(var script in new[]{"V1_22__ai_rag_v2_foundation_and_policies.sql","V1_23__ai_rag_v2_security_and_correctness.sql"})
            {
                var ddl=System.IO.File.ReadAllText(System.IO.Path.Combine(root.FullName,"database","migrations",script));
                // All physical view dependencies must be checked before the first implicit-commit DDL.
                int firstDdl=ddl.IndexOf("CREATE TABLE",StringComparison.OrdinalIgnoreCase);
                Assert.Greater(firstDdl,0,script);
                string preflight=ddl.Substring(0,firstDdl);
                StringAssert.Contains("c.STATUS='ENABLED'",preflight);
                StringAssert.Contains("RAISE_APPLICATION_ERROR(-20031",preflight);
                foreach(System.Text.RegularExpressions.Match view in System.Text.RegularExpressions.Regex.Matches(ddl,@"(?is)CREATE OR REPLACE VIEW HR\.(V_AI_\w+) AS (.*?)(?=\r?\n/\r?\n)"))
                {
                    var aliases=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
                    foreach(System.Text.RegularExpressions.Match source in System.Text.RegularExpressions.Regex.Matches(view.Groups[2].Value,@"(?i)\b(?:FROM|JOIN)\s+(TB_\w+)\s+(\w+)")) aliases[source.Groups[2].Value]=source.Groups[1].Value;
                    foreach(System.Text.RegularExpressions.Match reference in System.Text.RegularExpressions.Regex.Matches(view.Groups[2].Value,@"\b([a-z]+)\.([A-Z][A-Z0-9_]*)\b"))
                    {
                        if(!aliases.TryGetValue(reference.Groups[1].Value,out var table)) continue;
                        string column=reference.Groups[2].Value;
                        StringAssert.Contains("need_column('"+table+"','"+column+"')",preflight,script+": missing early source guard");
                        if(table=="TB_PAYROLL_CALCULATION_RUN")
                        {
                            var definition=System.Text.RegularExpressions.Regex.Match(v16,@"(?s)CREATE TABLE TB_PAYROLL_CALCULATION_RUN \((.*?)\);");
                            Assert.True(System.Text.RegularExpressions.Regex.IsMatch(definition.Groups[1].Value,@"\b"+column+@"\s+(?:NUMBER|TIMESTAMP|NVARCHAR2)\b"),"Undocumented run column "+column);continue;
                        }
                        var model=System.Text.RegularExpressions.Regex.Match(edmx,@"(?s)<EntityType Name="""+table+@"""\s*>(.*?)</EntityType>");
                        Assert.True(model.Success,"Source table missing from EDMX: "+table);
                        bool addedByMigration=(table=="TB_BANGLUONG" && (column=="RUN_ID" || column=="TRANGTHAI_CHITRA")) || (table=="TB_KYCONG" && new[]{"CONG_INPUT_REV","CONG_PUBLISH_REV","TRANGTHAI_CHITRA"}.Contains(column));
                        Assert.True(addedByMigration || model.Groups[1].Value.Contains("Name=\""+column+"\""),script+" "+view.Groups[1].Value+": "+table+"."+column+" is not mapped");
                    }
                }
                StringAssert.Contains("FROM TB_NANGLUONG_NHANVIEN nl",ddl);StringAssert.Contains("nl.DELETED_DATE IS NULL",ddl);
                StringAssert.DoesNotContain("FROM TB_NANGLUONG nl",ddl);StringAssert.Contains("pr.STATUS='SUCCESS'",ddl);StringAssert.Contains("AND kc.KHOA=1",ddl);
            }
        }
        [Test] public void UnmappedDailyAttendanceIsUnsupportedRatherThanPermissionDenied()
        {var p=new QueryPlanner(clock).CreatePlan(new QueryUnderstandingResult {Domain="ATTENDANCE",Operation="DETAIL",Time=new TimeResolution {Month=9,Year=2026}},ctx);Assert.AreEqual(ExecutionStrategy.Unsupported,p.Strategy);Assert.IsNull(p.SqlStatement);}
        [Test] public void AggregateCapabilitiesUseDistinctSourcesWithoutRawDetail()
        {
            var planner=new QueryPlanner(clock);
            var count=planner.CreatePlan(new QueryUnderstandingResult {Domain="EMPLOYEE",Operation="COUNT"},ctx);
            var sum=planner.CreatePlan(new QueryUnderstandingResult {Domain="OVERTIME",Operation="SUM",Time=new TimeResolution {Month=9,Year=2026}},ctx);
            Assert.AreEqual("V_AI_EMPLOYEE_COUNT",count.TargetView);Assert.AreEqual("V_AI_OVERTIME_SUMMARY",sum.TargetView);
            StringAssert.DoesNotContain("NGAY,",sum.SqlStatement);StringAssert.DoesNotContain("HOTEN",count.SqlStatement);
        }
        [Test] public void AdminAndWildcardDoNotGrantDataAccess()
        {
            var actor=new AiAuthorizationContext {UserId=1,IsAdmin=true,FunctionRights=new HashSet<string>{"*"}};
            Assert.False(AiAuthorizationService.ValidateCapability(actor,"PAYROLL_VIEW").IsAllowed);
            Assert.False(AiScopeEvaluator.BuildSqlScopeFilter(actor,"PAYROLL_VIEW").IsAllowed);
            Assert.False(AiScopeEvaluator.IsEmployeeInScope(actor,50,3,"1"));
        }
        [Test] public void SelfRemainsSelfWithExplicitAllGrant()
        { var p=AiScopeEvaluator.BuildSqlScopeFilter(ctx,"PAYROLL_SELF");StringAssert.Contains("AND MANV =",p.SqlPredicate);Assert.That(p.Parameters.Values,Does.Contain(10)); }
        [Test] public void DenyWinsOverAllGrantPerCapability()
        {
            ctx.PolicyLoaded=true;ctx.ScopeGrants.Add(new AiScopeGrant {CapabilityCode="OVERTIME_SUM",ScopeType="ALL",Effect="ALLOW"});ctx.ScopeGrants.Add(new AiScopeGrant {CapabilityCode="OVERTIME_SUM",ScopeType="DEPARTMENT",ScopeKey="2",Effect="DENY"});
            Assert.False(AiScopeEvaluator.IsEmployeeInScope(ctx,18,2,"1","OVERTIME_SUM"));Assert.True(AiScopeEvaluator.IsEmployeeInScope(ctx,15,3,"1","OVERTIME_SUM"));Assert.False(AiScopeEvaluator.IsEmployeeInScope(ctx,15,3,"1","PAYROLL_VIEW"));
        }
        [Test] public async Task OpaqueSelectionPreservesRequestAndRejectsTokenPrefix()
        {
            var first=await Ask("An tăng ca tổng giờ tháng 9 năm 2026");Assert.AreEqual("needs_clarification",first.Status);Assert.AreEqual(0,sql.Calls);
            var opt=first.Clarification.Options[0];Assert.That(opt.Token,Does.Not.StartWith("opt_emp_"));
            var bad=await service.ProcessChatAsync("",ctx,"a",optionToken:opt.Token.Substring(0,6),expectedConversationVersion:first.ConversationVersion,clarificationId:first.Clarification.ClarificationId);
            Assert.AreEqual(409,bad.HttpStatus);Assert.AreEqual(0,sql.Calls);
            var answer=await service.ProcessChatAsync("",ctx,"a",optionToken:opt.Token,expectedConversationVersion:first.ConversationVersion,clarificationId:first.Clarification.ClarificationId);
            Assert.AreEqual("answered",answer.Status,answer.Answer);Assert.AreEqual(10,sql.LastParameters[":p_manv"]);Assert.AreEqual(9,sql.LastParameters[":p_thang"]);Assert.AreEqual(2026,sql.LastParameters[":p_nam"]);Assert.AreEqual("SUM",answer.InterpretedRequest.Operation);
        }
        [Test] public async Task ClarificationCannotCrossActorOrConversation()
        {
            var first=await Ask("An tăng ca tổng giờ tháng 9 năm 2026");
            foreach (var actor in new[] {ctx,AiTestContexts.All(2)}) {var reply=await service.ProcessChatAsync("",actor,actor.UserId==1 ? "other" : "a",optionToken:first.Clarification.Options[0].Token,expectedConversationVersion:first.ConversationVersion,clarificationId:first.Clarification.ClarificationId);Assert.AreEqual(409,reply.HttpStatus);}
            Assert.AreEqual(0,sql.Calls);
        }
        [Test] public async Task AmbiguousFreeTextDoesNotPickFirstCandidate()
        { await Ask("An tăng ca tổng giờ tháng 9 năm 2026");var r=await Ask("An");Assert.AreEqual("needs_clarification",r.Status);Assert.AreEqual(0,sql.Calls); }
        [Test] public async Task MissingYearCanBeFilledWithoutLosingEmployee()
        {
            var first=await Ask("Tăng ca mã 10 tháng 9");Assert.AreEqual("needs_clarification",first.Status);Assert.AreEqual("TIME_PERIOD",first.Clarification.Field);
            var r=await Ask("năm 2025");Assert.AreEqual("answered",r.Status,r.Answer);Assert.AreEqual(2025,sql.LastParameters[":p_nam"]);Assert.AreEqual(10,sql.LastParameters[":p_manv"]);
        }
        [TestCase("Tăng ca mã 10 tháng 13 năm 2026")]
        [TestCase("Nhân viên sinh nhật tháng 13")]
        [TestCase("Hợp đồng hết hạn trong 500 ngày")]
        public async Task InvalidTimeDoesNotFallBackToCurrentPeriod(string q) {var r=await Ask(q);Assert.AreEqual("unsupported",r.Status,r.Answer);Assert.AreEqual(0,sql.Calls);}
        [Test] public async Task GreetingReturnsHttp200AndDoesNotExecuteSql()
        {var r=await Ask("Xin chào");Assert.AreEqual("answered",r.Status);Assert.AreEqual(200,r.HttpStatus);Assert.AreEqual(0,sql.Calls);}
        [TestCase("Thống kê nhân sự theo từng phòng ban?", false)]
        [TestCase("Có bao nhiêu nhân viên phòng IT theo từng phòng ban?", true)]
        public async Task GroupCountDoesNotMistakeGroupingForDepartmentName(string q,bool department)
        {var r=await Ask(q);Assert.AreEqual("answered",r.Status,r.Answer);Assert.AreEqual("HEADCOUNT_BY_DEPT",r.InterpretedRequest.Metric);StringAssert.Contains("GROUP BY IDPB,TEN_PHONGBAN",sql.LastSql);Assert.AreEqual(department,sql.LastParameters.ContainsKey(":p_dept_id"));}
        [TestCase("Có bao nhiêu nhân viên tháng 9 năm 2025?")]
        [TestCase("Danh sách nhân viên năm 2025")]
        public async Task HistoricalEmployeeRequestCannotSilentlyUseCurrentHeadcount(string q)
        {var r=await Ask(q);Assert.AreEqual("unsupported",r.Status);Assert.AreEqual(0,sql.Calls);Assert.AreEqual(0,cache.ResultCache.Count);}
        [Test] public async Task NotFoundEmployeeAndDepartmentDoNotBroadenQuery()
        {foreach(var q in new[]{"Tăng ca mã 999 tháng 9 năm 2026","Tăng ca phòng Atlantis tháng 9 năm 2026"}) {var r=await Ask(q,Guid.NewGuid().ToString());Assert.AreEqual("unsupported",r.Status,r.Answer);}Assert.AreEqual(0,sql.Calls);}
        [Test] public async Task ContractCombinesTargetWithRequestedExpiryWindow()
        {await Ask("Hợp đồng mã 10 hết hạn trong 7 ngày");StringAssert.Contains("MANV = :p_manv",sql.LastSql);StringAssert.Contains("NGAYKETTHUC < :p_limit_date",sql.LastSql);Assert.AreEqual(clock.Now.Date.AddDays(8),sql.LastParameters[":p_limit_date"]);}
        [Test] public async Task AmountDoesNotParseEmployeeCodeAsMoneyAndNeverCachesIncome()
        {await Ask("Phụ cấp mã 10 trên 1 triệu");Assert.AreEqual(1000000m,sql.LastParameters[":p_sotien"]);await Ask("Phụ cấp mã 10 trên 2 triệu");Assert.AreEqual(2000000m,sql.LastParameters[":p_sotien"]);await Ask("Phụ cấp mã 10 trên 2 triệu");Assert.AreEqual(3,sql.Calls);Assert.AreEqual(0,cache.ResultCache.Count);}
        [TestCase("mã 10")]
        [TestCase("mã nhân viên 10")]
        public async Task OwnEmployeeCodeUsesSelfPayrollWithoutManagementRight(string code)
        {
            ProductionPolicy("PAYROLL_SELF","VIEW","MANV","HOTEN","TEN_PHONGBAN","THANG","NAM","MAKYCONG","THUCLANH","NGAYCONG_THUCTE","TRANGTHAI_CHITRA","IS_LOCKED");
            ctx.FunctionRights=new HashSet<string>{"F_SYSTEM_AI"};
            var own=await Ask("Lương thực lĩnh "+code+" tháng 9 năm 2026");
            Assert.AreEqual("answered",own.Status,own.Answer);Assert.AreEqual(10,sql.LastParameters[":p_manv"]);Assert.AreEqual(1,sql.Calls);
            var other=await Ask("Lương thực lĩnh mã 20 tháng 9 năm 2026");
            Assert.AreEqual(403,other.HttpStatus);Assert.AreEqual(1,sql.Calls);
        }
        [TestCase("dưới","<")]
        [TestCase("trên",">")]
        [TestCase("tối đa","<=")]
        [TestCase("ít nhất",">=")]
        public async Task AdvanceAmountFilterReachesSqlWithEmployeeAndPeriod(string words,string expected)
        {
            var r=await Ask("Tạm ứng lương mã 10 "+words+" 1,5 triệu tháng 9 năm 2026");
            Assert.AreEqual("answered",r.Status,r.Answer);StringAssert.Contains("AI_OWNER.V_AI_ADVANCE",sql.LastSql);
            StringAssert.Contains("SOTIEN "+expected+" :p_sotien",sql.LastSql);
            Assert.AreEqual(1500000m,sql.LastParameters[":p_sotien"]);Assert.AreEqual(10,sql.LastParameters[":p_manv"]);Assert.AreEqual(9,sql.LastParameters[":p_thang"]);Assert.AreEqual(2026,sql.LastParameters[":p_nam"]);Assert.AreEqual(0,cache.ResultCache.Count);
        }
        [TestCase("Phụ cấp từ 1 triệu đến 2 triệu")]
        [TestCase("Lương thực lĩnh mã 10 trên 2 triệu tháng 9 năm 2026")]
        public async Task UnsupportedMoneyConditionsNeverBecomeUnfilteredSql(string question)
        { var r=await Ask(question);Assert.AreEqual("unsupported",r.Status,r.Answer);Assert.AreEqual(0,sql.Calls); }
        [Test] public void CountSelfAndGrantedAllNeverShareResultCacheKey()
        {
            ProductionPolicy("EMPLOYEE_COUNT","COUNT","TOTAL_COUNT");ctx.ScopeGrants.Add(new AiScopeGrant {CapabilityCode="EMPLOYEE_COUNT",ScopeType="ALL",Effect="ALLOW"});
            var planner=new QueryPlanner(clock);var all=planner.CreatePlan(new QueryUnderstandingResult {Domain="EMPLOYEE",Operation="COUNT"},ctx);var self=planner.CreatePlan(new QueryUnderstandingResult {Domain="EMPLOYEE",Operation="COUNT",RequestedScope="SELF"},ctx);
            Assert.AreEqual(ExecutionStrategy.SqlTemplate,self.Strategy);Assert.AreEqual("SELF",self.RequestedScope);Assert.AreNotEqual(AiCacheCoordinator.BuildResultKey(all,ctx),AiCacheCoordinator.BuildResultKey(self,ctx));
        }
        [Test] public void CanonicalKeyIncludesTypedValuesAndPolicy()
        {
            var p=new QueryExecutionPlan {Domain="ALLOWANCE",SqlStatement="SELECT SOTIEN FROM AI_OWNER.V_AI_ALLOWANCE WHERE SOTIEN > :a"};p.Parameters[":a"]=1000000m;
            var before=AiCacheCoordinator.BuildResultKey(p,ctx);p.Parameters[":a"]=2000000m;Assert.AreNotEqual(before,AiCacheCoordinator.BuildResultKey(p,ctx));p.Parameters[":a"]=1000000m;ctx.PolicyVersion++;Assert.AreNotEqual(before,AiCacheCoordinator.BuildResultKey(p,ctx));
        }
        [Test] public async Task CountCacheWarmHitAndSourceRevisionInvalidation()
        {
            await Ask("Có bao nhiêu nhân viên?");await Ask("Số lượng nhân viên?");Assert.AreEqual(1,sql.Calls);
            ctx.SourceRevisions["EMPLOYEE"]++;await Ask("Có bao nhiêu nhân viên?");Assert.AreEqual(2,sql.Calls);
            clock.Advance(TimeSpan.FromSeconds(61));await Ask("Có bao nhiêu nhân viên?");Assert.AreEqual(3,sql.Calls);
        }
        [Test] public async Task CacheWithoutTrustedSourceRevisionIsBypassed()
        {ctx.SourceRevisions.Clear();await Ask("Có bao nhiêu nhân viên?");await Ask("Có bao nhiêu nhân viên?");Assert.AreEqual(2,sql.Calls);Assert.AreEqual(0,cache.ResultCache.Count);}
        [Test] public async Task ConcurrentIdenticalCountsUseOneSqlCall()
        {sql.Release=new TaskCompletionSource<bool>();var a=Ask("Có bao nhiêu nhân viên?","a");var b=Ask("Có bao nhiêu nhân viên?","b");Assert.AreEqual(1,sql.Calls);sql.Release.SetResult(true);var r=await Task.WhenAll(a,b);Assert.True(r.All(x=>x.Status=="answered"));}
        [Test] public async Task ResetDiscardsLateResponseAndDoesNotPublishCache()
        {sql.Release=new TaskCompletionSource<bool>();var pending=Ask("Có bao nhiêu nhân viên?");service.Reset(1,"a");sql.Release.SetResult(true);var r=await pending;Assert.AreEqual(409,r.HttpStatus);Assert.AreEqual(0,cache.ResultCache.Count);Assert.False(sessions.TryGetSession(1,"a",out _));}
        [Test] public async Task CancellationOfOneWaiterDoesNotCancelAnother()
        {
            sql.Release=new TaskCompletionSource<bool>();using(var cts=new CancellationTokenSource()) {var a=service.ProcessChatAsync("Có bao nhiêu nhân viên?",ctx,"a",cancellationToken:cts.Token);var b=Ask("Có bao nhiêu nhân viên?","b");cts.Cancel();sql.Release.SetResult(true);Assert.AreEqual("error",(await a).Status);Assert.AreEqual("answered",(await b).Status);Assert.AreEqual(1,sql.Calls);Assert.AreEqual(0,cache.ResultCache.Count);}
        }
        private void ProductionPolicy(string capability, string operation, params string[] fields)
        {
            var cap=AiCapabilityCatalog.Get(capability);
            ctx.PolicyLoaded=true;ctx.HasAllScope=false;
            ctx.Capabilities[capability]=new AiPolicyCapability { CapabilityCode=capability, SourceView=cap.SourceView, RequiredFunctionCode=cap.RequiredFunctionCode, Enabled=true };
            ctx.ScopeGrants.Add(new AiScopeGrant {CapabilityCode=capability,ScopeType="SELF",Effect="ALLOW"});
            foreach(var field in fields) { ctx.FieldPolicies[capability+":"+field]="FULL";ctx.FieldOperations[capability+":"+field]=operation; }
        }
        [Test] public async Task LoadedProductionPolicyRequiresExplicitCapabilityScopeAndFields()
        {
            ProductionPolicy("EMPLOYEE_COUNT","COUNT","TOTAL_COUNT");
            Assert.AreEqual("answered",(await Ask("Có bao nhiêu nhân viên?")).Status);
            StringAssert.Contains("AI_OWNER.V_AI_EMPLOYEE_COUNT",sql.LastSql);StringAssert.DoesNotContain("MANV",sql.LastSql);Assert.IsEmpty(sql.LastParameters);
            ctx.ScopeGrants.Clear();Assert.AreEqual(403,(await Ask("Có bao nhiêu nhân viên?")).HttpStatus);Assert.AreEqual(1,sql.Calls);
        }
        [Test] public async Task MissingProductionFieldPolicyPreventsSql()
        {
            ProductionPolicy("EMPLOYEE_COUNT","COUNT");
            Assert.AreEqual(403,(await Ask("Có bao nhiêu nhân viên?")).HttpStatus);Assert.AreEqual(0,sql.Calls);
        }
        [Test] public async Task FieldOperationPolicyPreventsAggregatingViewOnlyField()
        {
            ProductionPolicy("EMPLOYEE_COUNT","VIEW","TOTAL_COUNT");
            Assert.AreEqual(403,(await Ask("Có bao nhiêu nhân viên?")).HttpStatus);Assert.AreEqual(0,sql.Calls);
        }
        [Test] public async Task MaskedScalarCannotBeAggregatedOrReusedFromCache()
        {
            ProductionPolicy("EMPLOYEE_COUNT","COUNT","TOTAL_COUNT");ctx.FieldPolicies["EMPLOYEE_COUNT:TOTAL_COUNT"]="MASK";
            Assert.AreEqual(403,(await Ask("Có bao nhiêu nhân viên?")).HttpStatus);Assert.AreEqual(0,sql.Calls);Assert.AreEqual(0,cache.ResultCache.Count);
        }
        [Test] public async Task PolicyCannotRemoveCompiledPayrollFunctionRight()
        {
            ProductionPolicy("PAYROLL_VIEW","VIEW","THUCLANH");ctx.Capabilities["PAYROLL_VIEW"].RequiredFunctionCode=null;ctx.FunctionRights.Remove("F_CC_BANGLUONG");
            Assert.False(AiAuthorizationService.ValidateCapability(ctx,"PAYROLL_VIEW").IsAllowed);
            Assert.AreEqual(403,(await Ask("Lương thực lĩnh mã 20 tháng 9 năm 2026")).HttpStatus);Assert.AreEqual(0,sql.Calls);
        }
        [Test] public async Task FieldPolicyChangeInvalidatesWarmCountBeforeReuse()
        {
            ProductionPolicy("EMPLOYEE_COUNT","COUNT","TOTAL_COUNT");await Ask("Có bao nhiêu nhân viên?");ctx.FieldPolicies["EMPLOYEE_COUNT:TOTAL_COUNT"]="DENY";
            Assert.AreEqual(403,(await Ask("Có bao nhiêu nhân viên?")).HttpStatus);Assert.AreEqual(1,sql.Calls);
        }
        [Test] public async Task ConflictReturnsCurrentVersionForClientRecovery()
        {
            var first=await Ask("An tăng ca tổng giờ tháng 9 năm 2026");
            var stale=await service.ProcessChatAsync("tháng này",ctx,"a",expectedConversationVersion:0);
            Assert.AreEqual(409,stale.HttpStatus);Assert.AreEqual(first.ConversationVersion,stale.ConversationVersion);Assert.AreEqual(0,sql.Calls);
        }
        [Test] public async Task LegacyRagWithoutIdentityCannotCallRetrievalOrLlm()
        {
            var r=await new Bu.Services.AI_Services.HybridRagService().Ask("Lương Nguyễn Văn An?");
            Assert.AreEqual("unsupported",r.Status);Assert.IsNull(r.Data);Assert.IsEmpty(r.SqlQuery);
        }
        [Test] public async Task FailedSingleFlightCanRetryWithoutKeepingFaultedTask()
        {
            var flight=new SingleFlightCoordinator();var release=new TaskCompletionSource<bool>();int calls=0;
            Func<Task<int>> failing=async()=>{Interlocked.Increment(ref calls);await release.Task;throw new InvalidOperationException();};
            var a=flight.ExecuteAsync("same",failing);var b=flight.ExecuteAsync("same",failing);Assert.AreEqual(1,calls);release.SetResult(true);
            foreach(var pending in new[]{a,b}) {try {await pending;Assert.Fail("Must fail");}catch(InvalidOperationException){}}
            var retry=await flight.ExecuteAsync("same",()=>Task.FromResult(42));Assert.AreEqual(42,retry);
        }
        private class MutablePolicy : IAiPolicyProvider { public AiAuthorizationContext Current;public AiAuthorizationContext Load(int id)=>ConversationStateManager.Clone(Current); }
        [Test] public async Task SourceChangeDuringQueryPreventsPublishingOldRevision()
        {var policy=new MutablePolicy {Current=ctx};service=Create(policy);sql.Release=new TaskCompletionSource<bool>();var pending=Ask("Có bao nhiêu nhân viên?");policy.Current.SourceRevisions["EMPLOYEE"]++;sql.Release.SetResult(true);Assert.AreEqual(409,(await pending).HttpStatus);Assert.AreEqual(0,cache.ResultCache.Count);}
        [Test] public async Task CacheMetricsDistinguishWarmHitsLoadsAndBypasses()
        {await Ask("Có bao nhiêu nhân viên?");await Ask("Có bao nhiêu nhân viên?");await Ask("Phụ cấp mã 10 trên 1 triệu");var metrics=cache.GetResultMetrics();Assert.AreEqual(1,metrics["hits"]);Assert.AreEqual(2,metrics["loads"]);Assert.AreEqual(1,metrics["bypasses"]);Assert.AreEqual(1,metrics["published"]);}
        [Test] public async Task RevocationDuringQueryRejectsResultBeforeCacheAndHistory()
        {var policy=new MutablePolicy {Current=ctx};service=Create(policy);sql.Release=new TaskCompletionSource<bool>();var pending=Ask("Có bao nhiêu nhân viên?");policy.Current.PolicyVersion++;sql.Release.SetResult(true);Assert.AreEqual(409,(await pending).HttpStatus);Assert.AreEqual(0,cache.ResultCache.Count);Assert.True(sessions.TryGetSession(1,"a",out var session));Assert.AreEqual(0,session.Messages.Count);}
        [Test] public async Task ExpiredClarificationAndVersionConflictDoNotExecuteSql()
        {var first=await Ask("An tăng ca tổng giờ tháng 9 năm 2026");clock.Advance(TimeSpan.FromMinutes(11));var r=await service.ProcessChatAsync("",ctx,"a",optionToken:first.Clarification.Options[0].Token,expectedConversationVersion:first.ConversationVersion,clarificationId:first.Clarification.ClarificationId);Assert.AreEqual(409,r.HttpStatus);Assert.AreEqual(0,sql.Calls);}
        [Test] public async Task SessionExpiryInvalidatesInFlightLease()
        {sql.Release=new TaskCompletionSource<bool>();var pending=Ask("Có bao nhiêu nhân viên?");clock.Advance(TimeSpan.FromMinutes(61));sessions.GetOrCreateSession(1,"a");sql.Release.SetResult(true);Assert.AreEqual(409,(await pending).HttpStatus);Assert.AreEqual(0,cache.ResultCache.Count);}
        [Test] public async Task SqlErrorIsNotEmptyDataAndNotCached()
        {sql.Status=SqlExecutionStatus.SourceUnavailable;var r=await Ask("Có bao nhiêu nhân viên?");Assert.AreEqual("error",r.Status);Assert.AreEqual(503,r.HttpStatus);Assert.AreEqual(0,cache.ResultCache.Count);}
        [Test] public void SensitiveRendererDoesNotExposeMaskedValue()
        {var p=new QueryExecutionPlan {Domain="INSURANCE",Strategy=ExecutionStrategy.SqlTemplate,MaskedFields=new List<string>{"SOBH"}};var t=new DataTable();t.Columns.Add("SOBH");t.Rows.Add("1234567890");var r=DeterministicResponseRenderer.Render(p,new SqlExecutionResult {Status=SqlExecutionStatus.SuccessWithData,Data=t});StringAssert.DoesNotContain("1234567890",r.Answer);StringAssert.Contains("***",r.Answer);}
        [Test] public void SummaryRendererCombinesEveryReturnedRowAndUsesCorrectMetric()
        {var p=new QueryExecutionPlan {Domain="PAYROLL",Operation="SUMMARY",IsScalar=true,Strategy=ExecutionStrategy.SqlTemplate};var t=new DataTable();t.Columns.Add("TONG_THUCLANH",typeof(decimal));t.Columns.Add("SO_NHANVIEN",typeof(int));t.Rows.Add(100m,1);t.Rows.Add(200m,2);var r=DeterministicResponseRenderer.Render(p,new SqlExecutionResult {Status=SqlExecutionStatus.SuccessWithData,Data=t});StringAssert.Contains("300",r.Answer);StringAssert.Contains("3",r.Answer);}
        [Test] public void SqlWithoutAuthorizationContextCannotExecute()
        {Assert.AreEqual(SqlExecutionStatus.AuthorizationDenied,new ScopedSqlExecutor().ExecuteScopedQuery("SELECT MANV FROM AI_OWNER.V_AI_EMPLOYEE_LOOKUP").Status);Assert.False(OracleSqlAstValidator.Validate("SELECT MANV FROM EVIL.V_AI_EMPLOYEE_LOOKUP").IsValid);}
    }
}