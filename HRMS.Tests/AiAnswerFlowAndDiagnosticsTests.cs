using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Security;
using Bu.Services.AI_Services.Vector;
using Bu.Tests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiAnswerFlowAndDiagnosticsTests
    {
        private sealed class RecordingLlm : ILlmService {
            public int GroundedCalls;
            public string GroundedAnswer = "Đây là kết quả tra cứu đã được xác minh theo phạm vi được cấp.";
            public string Evidence;
            public string GeneralHistory;
            public bool ThrowTimeout;
            public Task<string> GenerateGroundedAnswerAsync(string question, string factsEvidence, string history = null, CancellationToken cancellationToken = default) {
                GroundedCalls++; Evidence = factsEvidence;
                if (ThrowTimeout) throw new OperationCanceledException();
                return Task.FromResult(GroundedAnswer);
            }
            public Task<string> GenerateGeneralChatAsync(string question, string history = null, CancellationToken cancellationToken = default) { GeneralHistory=history;return Task.FromResult("Một đoạn trả lời sáng tác riêng cho hội thoại này."); }
            public Task<string> AskSql(string p)=>Task.FromResult<string>(null);
            public Task<string> AskIntent(string p)=>Task.FromResult<string>(null);
            public Task<string> AskChat(string c,string q,string h,Action<string> a=null)=>Task.FromResult<string>(null);
            public Task<float[]> GetEmbedding(string t,CancellationToken ct=default)=>Task.FromResult(new float[0]);
            public Task<string> AskStructuredJsonAsync(string p,string s,CancellationToken ct=default)=>Task.FromResult<string>(null);
        }
        private sealed class FailedVector : IVectorService {
            public VectorSearchStatus Status; public int Calls;
            public Task<VectorSearchResult> SearchScopedAsync(string q, VectorBusinessFilter b, VectorSecurityFilter s, float[] e=null, CancellationToken ct=default) {
                Calls++; return Task.FromResult(new VectorSearchResult {Status=Status});
            }
            public void Add(string t,string tag="GENERAL") => throw new NotSupportedException();
            public void Add(string t,string tag,int? id) => throw new NotSupportedException();
            public List<string> Search(string q,string tag=null) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public void RemoveByEmployeeId(int id) => throw new NotSupportedException();
            public void SyncEmployeeData(int id) => throw new NotSupportedException();
        }
        [TestCase(VectorSearchStatus.InvalidIndex,503)]
        [TestCase(VectorSearchStatus.Timeout,504)]
        [TestCase(VectorSearchStatus.Cancelled,499)]
        public async Task PolicyRetrievalFailure_IsNotReportedAsMissingDocument(VectorSearchStatus status,int expectedHttp) {
            var vector=new FailedVector {Status=status}; var llm=new RecordingLlm();
            var clock=new FakeClockProvider(new DateTime(2026,10,11,10,0,0));
            var ctx=AiTestContexts.All();
            ctx.Capabilities["POLICY_LOOKUP"]=new AiPolicyCapability {CapabilityCode="POLICY_LOOKUP",Enabled=true,SourceView="hrms_knowledge_active"};
            var service=new AiExecutionService(new QueryUnderstandingService(clock,new EntityResolver(new FakeEntityLookupProvider())),
                sqlExecutor:new RegressionSqlExecutor(),conversationManager:new ConversationStateManager(clock),
                cacheCoordinator:new AiCacheCoordinator(clock),clock:clock,llmService:llm,vectorService:vector);
            var result=await service.ProcessChatAsync("Quy định tính tiền tăng ca ngày lễ như thế nào?",ctx,"vector-failure");
            Assert.AreEqual(1,vector.Calls); Assert.AreEqual("error",result.Status);
            Assert.AreEqual(expectedHttp,result.HttpStatus); Assert.AreEqual(0,llm.GroundedCalls);
            StringAssert.DoesNotContain("Không tìm thấy",result.Answer);
        }
        private sealed class TableSql : IScopedSqlExecutor {
            public int Calls;
            public DataTable Table;
            public bool HasMore;
            public SqlExecutionResult ExecuteScopedQuery(string sql,Dictionary<string,object> p=null,CancellationToken cancellationToken=default) {
                Calls++;return new SqlExecutionResult {Status=SqlExecutionStatus.SuccessWithData,Data=Table.Copy(),TotalRecords=Table.Rows.Count,HasMore=HasMore};
            }
            public Task<SqlExecutionResult> ExecuteScopedQueryAsync(string sql,Dictionary<string,object> p=null,CancellationToken cancellationToken=default)=>Task.FromResult(ExecuteScopedQuery(sql,p,cancellationToken));
        }
        private sealed class MaskPlanner : IQueryPlanner {
            public QueryExecutionPlan CreatePlan(QueryUnderstandingResult u,AiAuthorizationContext c) {
                var p=new QueryPlanner().CreatePlan(u,c);p.FieldModes["DIENTHOAI"]="MASK";p.FieldModes["SOBH"]="DENY";return p;
            }
        }
        private static DataTable Employees() {
            var t=new DataTable();t.Columns.Add("MANV",typeof(int));t.Columns.Add("HOTEN");t.Rows.Add(10,"Nguyễn Văn An");t.Rows.Add(15,"Trần Thị Thu An");return t;
        }
        private static AiExecutionService Service(IScopedSqlExecutor sql,ILlmService llm,IQueryPlanner planner=null) {
            var clock=new FakeClockProvider(new DateTime(2026,10,11,10,0,0));
            return new AiExecutionService(new QueryUnderstandingService(clock,new EntityResolver(new FakeEntityLookupProvider())),planner,sql,new ConversationStateManager(clock),new AiCacheCoordinator(clock),clock:clock,llmService:llm);
        }
        [Test] public async Task ListResultsReachLlmAndKeepEveryVerifiedRow() {
            var sql=new TableSql {Table=Employees()};var llm=new RecordingLlm();
            var r=await Service(sql,llm).ProcessChatAsync("Danh sách nhân viên",AiTestContexts.All(),"list");
            Assert.AreEqual(1,llm.GroundedCalls);Assert.False(r.BypassedLlm);
            Assert.That(llm.Evidence,Does.Contain("Nguyễn Văn An").And.Contain("Trần Thị Thu An"));
            Assert.That(r.Answer,Does.Contain("Nguyễn Văn An").And.Contain("Trần Thị Thu An"));
            Assert.That(r.Diagnostics.Any(e=>e.Stage=="LLM_INPUT"),Is.True);
            Assert.That(r.Diagnostics.Any(e=>e.Stage=="LLM_ACCEPTED"),Is.True);
        }
        [TestCase("1. Nguyễn Văn An (mã 10)\n2. Trần Thị Thu An (mã 15)")]
        [TestCase("- Nguyễn Văn An: mã 10\n- Trần Thị Thu An: mã 15")]
        [TestCase("| Nhân viên | Mã |\n| Nguyễn Văn An | 10 |\n| Trần Thị Thu An | 15 |")]
        [TestCase("Danh sách gồm Nguyễn Văn An và Trần Thị Thu An.")]
        [TestCase("Người đầu tiên là **Nguyễn   Văn An**.")]
        [TestCase("Kết quả tra cứu:\nThông tin trong phạm vi được cấp.")]
        public async Task RepeatedLlmListIsDiscardedWithoutLosingVerifiedRows(string generated) {
            var sql=new TableSql {Table=Employees(),HasMore=true};
            var llm=new RecordingLlm {GroundedAnswer=generated};
            var r=await Service(sql,llm).ProcessChatAsync("Danh sách nhân viên",AiTestContexts.All(),"duplicate");
            Assert.AreEqual("answered",r.Status);Assert.True(r.BypassedLlm);Assert.True(r.HasMore);
            Assert.AreEqual(1,System.Text.RegularExpressions.Regex.Matches(r.Answer,"Nguyễn Văn An").Count);
            Assert.AreEqual(1,System.Text.RegularExpressions.Regex.Matches(r.Answer,"Trần Thị Thu An").Count);
            Assert.AreEqual(1,System.Text.RegularExpressions.Regex.Matches(r.Answer,"Kết quả tra cứu:").Count);
            Assert.AreEqual(1,System.Text.RegularExpressions.Regex.Matches(r.Answer,"Nguồn:").Count);
            Assert.That(r.Answer,Does.Contain("kết quả đầu tiên"));
            Assert.That(r.Diagnostics.Any(e=>e.Stage=="FALLBACK" && e.Details.Contains("llm_repeated_verified_rows")),Is.True);
            Assert.That(r.Diagnostics.Any(e=>e.Stage=="LLM_ACCEPTED"),Is.False);
        }

        [Test] public async Task TwentyDepartmentCountsAppearOnceAsInReportedScreenshot() {
            var t=new DataTable();t.Columns.Add("TEN_PHONGBAN");t.Columns.Add("TOTAL_COUNT",typeof(int));
            for(int i=1;i<=20;i++) t.Rows.Add("Phòng thử nghiệm "+i.ToString("00"),i+3);
            var generated=string.Join("\n",t.Rows.Cast<DataRow>().Select((row,i)=>(i+1)+". "+row["TEN_PHONGBAN"]+": "+row["TOTAL_COUNT"]+" nhân viên"));
            var llm=new RecordingLlm {GroundedAnswer=generated};
            var r=await Service(new TableSql{Table=t,HasMore=true},llm).ProcessChatAsync("Thống kê nhân sự theo từng phòng ban?",AiTestContexts.All(),"departments");
            Assert.AreEqual("answered",r.Status);Assert.True(r.BypassedLlm);Assert.True(r.HasMore);
            Assert.AreEqual(20,r.Data.Rows.Count);
            foreach(DataRow row in t.Rows) {
                string name=row["TEN_PHONGBAN"].ToString();
                Assert.AreEqual(1,System.Text.RegularExpressions.Regex.Matches(r.Answer,name).Count,name);
                Assert.That(r.Answer,Does.Contain(name+" • Số nhân viên: "+row["TOTAL_COUNT"]));
            }
            Assert.AreEqual(1,System.Text.RegularExpressions.Regex.Matches(r.Answer,"Nguồn:").Count);
        }

        [Test] public async Task MaskedAndDeniedValuesNeverReachGroundedPrompt() {
            var t=Employees();t.Columns.Add("DIENTHOAI");t.Columns.Add("SOBH");
            foreach(DataRow row in t.Rows){row["DIENTHOAI"]="0912345678";row["SOBH"]="SECRET_INSURANCE";}
            var llm=new RecordingLlm();await Service(new TableSql{Table=t},llm,new MaskPlanner()).ProcessChatAsync("Danh sách nhân viên",AiTestContexts.All(),"mask");
            Assert.That(llm.Evidence,Does.Contain("***").And.Not.Contain("0912345678").And.Not.Contain("SECRET_INSURANCE"));
        }
        [Test] public async Task CacheSharesFactsButGeneratesAnswerPerRequest() {
            var sql=new RegressionSqlExecutor();var llm=new RecordingLlm {GroundedAnswer="Trong phạm vi hiện tại có 15 nhân viên."};
            var service=Service(sql,llm);var ctx=AiTestContexts.All();
            var first=await service.ProcessChatAsync("Có bao nhiêu nhân viên?",ctx,"cache");
            var second=await service.ProcessChatAsync("Số lượng nhân viên hiện có?",ctx,"cache");
            Assert.AreEqual(1,sql.Calls);Assert.AreEqual(2,llm.GroundedCalls);
            Assert.False(first.BypassedLlm);Assert.False(second.BypassedLlm);
            Assert.That(second.Diagnostics.Any(e=>e.Stage=="CACHE" && e.Details.Contains("cache_or_singleflight")),Is.True);
        }
        [Test] public async Task HallucinatedQuantityIsRejectedWithReason() {
            var llm=new RecordingLlm{GroundedAnswer="Có 999 nhân viên trong công ty."};
            var r=await Service(new RegressionSqlExecutor(),llm).ProcessChatAsync("Có bao nhiêu nhân viên?",AiTestContexts.All(),"hallucination");
            Assert.True(r.BypassedLlm);Assert.That(r.Answer,Does.Contain("15").And.Not.Contain("999"));
            Assert.That(r.Diagnostics.Any(e=>e.Stage=="FALLBACK" && e.Details.Contains("facts_validation_failed")),Is.True);
        }
        [Test] public async Task TimeoutFallsBackToVerifiedFactsAndLogsReason() {
            var r=await Service(new RegressionSqlExecutor(),new RecordingLlm{ThrowTimeout=true}).ProcessChatAsync("Có bao nhiêu nhân viên?",AiTestContexts.All(),"timeout");
            Assert.AreEqual("answered",r.Status);Assert.True(r.BypassedLlm);
            Assert.That(r.Diagnostics.Any(e=>e.Stage=="FALLBACK" && e.Details.Contains("llm_timeout")),Is.True);
        }
        [Test] public async Task GeneralHistoryIncludesGeneralTurnsAndExcludesBusinessAnswers() {
            var llm=new RecordingLlm();var service=Service(new RegressionSqlExecutor(),llm);var ctx=AiTestContexts.All();
            await service.ProcessChatAsync("Thông tin nhân viên mã 10",ctx,"history");
            await service.ProcessChatAsync("Viết lời chúc sinh nhật cho đồng nghiệp",ctx,"history");
            await service.ProcessChatAsync("Viết lại ngắn hơn",ctx,"history");
            Assert.That(llm.GeneralHistory,Does.Contain("Viết lời chúc sinh nhật").And.Not.Contain("Nguyễn Văn An").And.Not.Contain("Thông tin nhân viên mã 10"));
            ctx.PolicyVersion++;
            await service.ProcessChatAsync("Viết lại ngắn hơn",ctx,"history");
            Assert.That(llm.GeneralHistory,Is.Empty);
        }
        [Test] public async Task ExplicitMissingEmployeeNeverFallsBackToPreviousEmployee() {
            var sql=new RegressionSqlExecutor();var service=Service(sql,new RecordingLlm());var ctx=AiTestContexts.All();
            await service.ProcessChatAsync("Thông tin nhân viên mã 10",ctx,"target");
            var second=await service.ProcessChatAsync("Tăng ca mã 999 tháng 9 năm 2026",ctx,"target");
            Assert.AreEqual("unsupported",second.Status);Assert.AreEqual(1,sql.Calls);
        }
        [Test] public void VectorScopeUsesOnlyRequestedCapabilityDepartments() {
            var ctx=AiTestContexts.All();ctx.AllowedDepartmentIds=new HashSet<int>{2,3};
            ctx.ScopeGrants.Add(new AiScopeGrant{CapabilityCode="EMPLOYEE_LOOKUP",ScopeType="DEPARTMENT",ScopeKey="2",Effect="ALLOW"});
            ctx.ScopeGrants.Add(new AiScopeGrant{CapabilityCode="POLICY_LOOKUP",ScopeType="DEPARTMENT",ScopeKey="3",Effect="ALLOW"});
            var method=typeof(AiExecutionService).GetMethod("BuildVectorSecurityFilter",BindingFlags.NonPublic|BindingFlags.Instance);
            var filter=(VectorSecurityFilter)method.Invoke(Service(new RegressionSqlExecutor(),new RecordingLlm()),new object[]{ctx,"POLICY_LOOKUP"});
            CollectionAssert.AreEquivalent(new[]{3},filter.AllowedDepartmentIds);
        }
        [Test] public void FutureVectorGrantIsNotApplied() {
            var ctx=AiTestContexts.All();ctx.ScopeGrants.Clear();
            ctx.ScopeGrants.Add(new AiScopeGrant{CapabilityCode="POLICY_LOOKUP",ScopeType="DEPARTMENT",ScopeKey="3",Effect="ALLOW",ValidFrom=DateTime.Now.AddDays(1)});
            var method=typeof(AiExecutionService).GetMethod("BuildVectorSecurityFilter",BindingFlags.NonPublic|BindingFlags.Instance);
            var filter=(VectorSecurityFilter)method.Invoke(Service(new RegressionSqlExecutor(),new RecordingLlm()),new object[]{ctx,"POLICY_LOOKUP"});
            Assert.That(filter.AllowedDepartmentIds,Is.Empty);
            Assert.AreEqual("SELF",filter.EffectiveScope);
        }
        [Test] public async Task CompanyScopeWithoutCompanyNeverFallsBackToOtherDepartments() {
            var service=new QdrantService();
            var result=await service.SearchScopedAsync("Quy định",new VectorBusinessFilter(),new VectorSecurityFilter{
                EffectiveScope="COMPANY",CallerEmployeeId=10,AllowedDepartmentIds=new List<int>{2},AllowedCompanyId=null
            });
            Assert.AreEqual(VectorSearchStatus.Forbidden,result.Status);
            Assert.That(result.SecurityFilterApplied,Does.Contain("COMPANY"));
        }
        [Test] public void QdrantLogsAuthorizedPayloadAndOmitsUnauthorizedContent() {
            string request="vector-"+Guid.NewGuid().ToString("N");
            using(var trace=AiRequestTrace.Begin(request)) {
                var result=new VectorSearchResult{Hits=new List<VectorHit>{
                    new VectorHit{PointId="allowed",Score=.9f,IsAuthorized=true,Text="VISIBLE_POLICY phone 0912345678",Version="2"},
                    new VectorHit{PointId="denied",Score=.8f,IsAuthorized=false,Text="HIDDEN_SALARY_SECRET"}
                }};
                typeof(AiExecutionService).GetMethod("LogVectorResult",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{result});
                var lines=File.ReadAllLines(AiRequestTrace.LogFilePath).Where(line=>line.Contains(request)).ToArray();
                Assert.That(string.Join("\n",lines),Does.Contain("VISIBLE_POLICY").And.Not.Contain("HIDDEN_SALARY_SECRET").And.Not.Contain("0912345678"));
                Assert.That(trace.Events.Single().Details,Does.Not.Contain("VISIBLE_POLICY"));
            }
        }
        [Test] public void DiagnosticsRedactSecretsAndPromptValues() {
            string json=AiRequestTrace.RedactObject(new{password="dont-log-me",content="Bearer abc.def.ghi token=SECRET email a@b.com"}).ToString();
            Assert.That(json,Does.Not.Contain("dont-log-me").And.Not.Contain("abc.def.ghi").And.Not.Contain("SECRET").And.Not.Contain("a@b.com"));
            string prompt=AiRequestTrace.PromptStructure("[EVIDENCE]\nPRIVATE_VALUE\n[QUESTION]\nPRIVATE_QUESTION");
            Assert.That(prompt,Does.Contain("[EVIDENCE]").And.Not.Contain("PRIVATE_VALUE").And.Not.Contain("PRIVATE_QUESTION"));
        }
        private sealed class ModelHandler : HttpMessageHandler {
            public string Requested;
            public string Returned="qwen2.5:latest";
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct) {
                Requested=JObject.Parse(await request.Content.ReadAsStringAsync())["model"].ToString();
                return new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonConvert.SerializeObject(new{model=Returned,response="Một câu trả lời đúng model được yêu cầu.",done=true}))};
            }
        }
        [TestCase("qwen2.5:latest",true)]
        [TestCase("qwen2.5:3b",false)]
        [TestCase("llama3:latest",false)]
        public async Task OllamaUsesConfiguredModelAndRejectsDifferentReturnedModel(string returned,bool accepted) {
            var handler=new ModelHandler{Returned=returned};
            var ollama=new OllamaService(null,(key,fallback)=>fallback,new HttpClient(handler),()=>new AiConfigurationSnapshot{AiModel="qwen2.5:latest"});
            using(var trace=AiRequestTrace.Begin("model-"+Guid.NewGuid().ToString("N"))) {
                string answer=await ollama.GenerateGeneralChatAsync("Viết một câu ngắn.");
                Assert.AreEqual("qwen2.5:latest",handler.Requested);
                Assert.AreEqual(accepted,!string.IsNullOrWhiteSpace(answer));
                Assert.That(trace.Events.Any(e=>e.Stage==(accepted?"OLLAMA_RESPONSE":"MODEL_MISMATCH")),Is.True);
            }
        }
        [TestCase("qwen2.5","qwen2.5:latest",true)]
        [TestCase("qwen2.5:3b","qwen2.5:latest",false)]
        [TestCase("qwen2.5:latest",null,false)]
        public void ModelAliasComparisonNeverSwitchesToAnotherTag(string requested,string returned,bool expected) {
            Assert.AreEqual(expected,OllamaService.SameModel(requested,returned));
        }
    }
}

