using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Security;
using HRMS_API.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiRuntimeReadinessRegressionTests
    {
        private sealed class UnavailablePolicies : IAiPolicyProvider, IAiIdentityProvider
        {
            public AiAuthorizationContext Actor=AiTestContexts.All();
            public int FullLoads;
            public bool IdentityFails;
            public AiAuthorizationContext Load(int userId)
            {
                FullLoads++;
                throw new AiSourceUnavailableException("AI_SETUP_REQUIRED","revisions",new InvalidOperationException("internal schema detail must never reach client"));
            }
            public AiAuthorizationContext LoadIdentity(int userId)
            {
                if(IdentityFails) throw new AiSourceUnavailableException("AI_SOURCE_UNAVAILABLE","identity",new InvalidOperationException("private connection detail"));
                return Actor;
            }
        }
        private sealed class Readiness : IAiReadinessProbe
        {
            private readonly bool ready;
            public Readiness(bool ready) { this.ready=ready; }
            public bool IsQueryReady() => ready;
        }
        private static AiExecutionService Create(UnavailablePolicies policies,RegressionSqlExecutor executor,ConversationStateManager sessions=null)
            => new AiExecutionService(sqlExecutor:executor,policyProvider:policies,conversationManager:sessions ?? new ConversationStateManager());

        [TestCase("xin chào")]
        [TestCase("Hello!")]
        [TestCase("chào bạn?")]
        public async Task PublicGreetingUsesLiveIdentityWithoutRequiringQuerySchema(string question)
        {
            var policy=new UnavailablePolicies();var sql=new RegressionSqlExecutor();var service=Create(policy,sql);
            var r=await service.ProcessChatAsync(question,AiTestContexts.All(),"greeting");
            Assert.AreEqual(200,r.HttpStatus);Assert.AreEqual("answered",r.Status);Assert.AreEqual(0,policy.FullLoads);Assert.AreEqual(0,sql.Calls);
        }
        [TestCase("Nhân viên tên Thái?")]
        [TestCase("Xin chào, có bao nhiêu nhân viên?")]
        public async Task BusinessQuestionStillFailsClosedWithExplicitSetupCode(string question)
        {
            var policy=new UnavailablePolicies();var sql=new RegressionSqlExecutor();
            var r=await Create(policy,sql).ProcessChatAsync(question,AiTestContexts.All(),"business");
            Assert.AreEqual(503,r.HttpStatus);Assert.AreEqual("AI_SETUP_REQUIRED",r.ErrorCode);Assert.AreEqual(1,policy.FullLoads);Assert.AreEqual(0,sql.Calls);
            StringAssert.DoesNotContain("internal",r.Answer);StringAssert.DoesNotContain("revisions",r.Answer);
        }
        [Test]
        public async Task GreetingStillRequiresLiveAiFunctionRight()
        {
            var policy=new UnavailablePolicies();policy.Actor.FunctionRights.Clear();var sql=new RegressionSqlExecutor();
            var r=await Create(policy,sql).ProcessChatAsync("xin chào",AiTestContexts.All(),"greeting");
            Assert.AreEqual(403,r.HttpStatus);Assert.AreEqual(0,sql.Calls);Assert.AreEqual(0,policy.FullLoads);
        }
        [Test]
        public async Task DisabledActorCannotUseGreetingThroughOldAuthenticatedContext()
        {
            var policy=new UnavailablePolicies {Actor=AiAuthorizationContext.CreateAnonymous()};var sql=new RegressionSqlExecutor();
            var r=await Create(policy,sql).ProcessChatAsync("xin chào",AiTestContexts.All(),"greeting");
            Assert.AreEqual(401,r.HttpStatus);Assert.AreEqual(0,sql.Calls);
        }
        [Test]
        public async Task GreetingDoesNotPretendToWorkWhenIdentityConnectionFails()
        {
            var policy=new UnavailablePolicies {IdentityFails=true};var sql=new RegressionSqlExecutor();
            var r=await Create(policy,sql).ProcessChatAsync("xin chào",AiTestContexts.All(),"greeting");
            Assert.AreEqual(503,r.HttpStatus);Assert.AreEqual("AI_SOURCE_UNAVAILABLE",r.ErrorCode);StringAssert.DoesNotContain("private",r.Answer);
        }
        [Test]
        public async Task GreetingRespectsConversationVersionAndDoesNotMutateBusinessHistory()
        {
            var policy=new UnavailablePolicies();var sql=new RegressionSqlExecutor();var sessions=new ConversationStateManager();
            var lease=sessions.Begin(1,"greeting",null,policy.Actor.Fingerprint());
            sessions.Commit(lease,new QueryUnderstandingResult {Domain="EMPLOYEE"},null,"old question","old answer",out var version);
            var service=Create(policy,sql,sessions);
            var stale=await service.ProcessChatAsync("xin chào",AiTestContexts.All(),"greeting",expectedConversationVersion:0);
            Assert.AreEqual(409,stale.HttpStatus);Assert.AreEqual(version,stale.ConversationVersion);
            var r=await service.ProcessChatAsync("xin chào",AiTestContexts.All(),"greeting",expectedConversationVersion:version);
            Assert.AreEqual(200,r.HttpStatus);sessions.TryGetSession(1,"greeting",out var state);Assert.AreEqual(version,state.Version);
        }
        [TestCase(false,true)]
        [TestCase(true,false)]
        public async Task ApiStatusSeparatesOracleSchemaReadinessFromOllamaAvailability(bool queryReady,bool llmAvailable)
        {
            var controller=new AiChatController(null,new Readiness(queryReady),()=>llmAvailable) {Configuration=new HttpConfiguration(),Request=new HttpRequestMessage()};
            controller.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response=await controller.GetStatus().ExecuteAsync(CancellationToken.None);
            var json=JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.True((bool)json["connected"]);Assert.AreEqual(queryReady,(bool)json["queryReady"]);Assert.AreEqual(llmAvailable,(bool)json["llmAvailable"]);
            if(!queryReady) StringAssert.Contains("chưa", (string)json["message"]);
        }
        [Test]
        public void SetupErrorIsPresentInWireContractWithoutInternalException()
        {
            var json=JObject.Parse(JsonConvert.SerializeObject(new AiChatResponseDto {ErrorCode="AI_SETUP_REQUIRED",Status="error"}));
            Assert.AreEqual("AI_SETUP_REQUIRED",(string)json["errorCode"]);Assert.IsNull(json["stage"]);Assert.IsNull(json["exception"]);
        }

        [Test]
        public void Test_Live_OracleAiPolicyProvider_LoadIdentity()
        {
            var provider = new OracleAiPolicyProvider();
            var ctx = provider.LoadIdentity(80);
            Assert.IsNotNull(ctx);
            Assert.AreEqual(80, ctx.UserId);
        }

        [Test]
        public void Test_Live_OracleAiPolicyProvider_Load()
        {
            var provider = new OracleAiPolicyProvider();
            var ctx = provider.Load(80);
            Assert.IsNotNull(ctx);
            Assert.AreEqual(80, ctx.UserId);
        }

        [Test]
        public void Test_Live_EntityResolver_FindEmployees()
        {
            var provider = new OracleAiPolicyProvider();
            var ctx = provider.Load(80);
            ctx.LookupCapability = "EMPLOYEE_LOOKUP";
            var resolver = new EntityResolver();
            try
            {
                var mention = resolver.ResolveEmployee("Thái", ctx);
                TestContext.WriteLine("Status: " + mention.Status + ", Candidates: " + mention.Candidates.Count);
            }
            catch (AiSourceUnavailableException ex)
            {
                Assert.Fail($"Failed with ErrorCode={ex.ErrorCode}, Stage={ex.Stage}, Inner={ex.InnerException}");
            }
        }
    }
}