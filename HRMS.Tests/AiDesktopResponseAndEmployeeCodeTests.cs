using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Security;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using QLyNSu.Functions;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiDesktopResponseAndEmployeeCodeTests
    {
        private sealed class ResponseHandler : HttpMessageHandler
        {
            public HttpStatusCode Code;
            public string Body;
            public string MediaType = "application/json";
            public string RequestBody;
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
            {
                RequestBody = await request.Content.ReadAsStringAsync();
                return new HttpResponseMessage(Code) { Content = new StringContent(Body, System.Text.Encoding.UTF8, MediaType) };
            }
        }

        private static async Task<AiClientChatResult> Call(HttpStatusCode code, string body, string mediaType = "application/json")
        {
            var handler = new ResponseHandler { Code = code, Body = body, MediaType = mediaType };
            using (var http = new HttpClient(handler))
            {
                var client = new AiApiClient(http);
                client.SetToken("mock-parser-token");
                try
                {
                    var result = await client.SendChatAsync("thông tin nhân viên mã số 21", "test-conversation");
                    Assert.That(JObject.Parse(handler.RequestBody)["clientRequestId"].Value<string>(), Is.Not.Empty);
                    return result;
                }
                finally { client.ClearSession(); Bu.CLASS_SYSTEM.UserSession.SessionCleared -= client.ClearSession; }
            }
        }

        [TestCase(400)]
        [TestCase(404)]
        [TestCase(429)]
        [TestCase(500)]
        [TestCase(502)]
        [TestCase(503)]
        [TestCase(504)]
        public async Task HttpFailureReportsActualStatusWithoutDisplayingServerInternals(int code)
        {
            var result = await Call((HttpStatusCode)code, "<html>private backend exception</html>", "text/html");
            Assert.That(result.Status, Is.EqualTo("error"));
            Assert.That(result.HttpStatusCode, Is.EqualTo(code));
            Assert.That(result.ErrorCode, Is.EqualTo("HTTP_" + code));
            Assert.That(result.Answer, Does.Contain("HTTP " + code));
            Assert.That(result.Answer, Does.Not.Contain("private backend exception"));
            Assert.That(result.RequestId, Is.Not.Empty);
        }

        [TestCase("not json")]
        [TestCase("[]")]
        [TestCase("null")]
        [TestCase("\"unexpected text\"")]
        public async Task SuccessfulHttpWithInvalidPayloadIsAProtocolError(string body)
        {
            var result = await Call(HttpStatusCode.OK, body);
            Assert.That(result.ErrorCode, Is.EqualTo("INVALID_AI_RESPONSE"));
            Assert.That(result.Status, Is.EqualTo("error"));
        }

        [TestCase("{}")]
        [TestCase("{\"status\":\"answered\",\"answer\":\"  \"}")]
        public async Task EmptyAnswerCannotBeTreatedAsSuccessfulChat(string body)
        {
            var result = await Call(HttpStatusCode.OK, body);
            Assert.That(result.ErrorCode, Is.EqualTo("EMPTY_AI_ANSWER"));
            Assert.That(result.Status, Is.EqualTo("error"));
        }

        [Test]
        public async Task StructuredBackendErrorAndCorrelationArePreserved()
        {
            var result = await Call(HttpStatusCode.ServiceUnavailable,
                "{\"status\":\"error\",\"answer\":\"Nguồn dữ liệu AI chưa sẵn sàng.\",\"errorCode\":\"AI_SETUP_REQUIRED\",\"requestId\":\"server-request-1\"}");
            Assert.That(result.ErrorCode, Is.EqualTo("AI_SETUP_REQUIRED"));
            Assert.That(result.RequestId, Is.EqualTo("server-request-1"));
            Assert.That(result.Answer, Is.EqualTo("Nguồn dữ liệu AI chưa sẵn sàng."));
        }

        [Test]
        public async Task SuccessfulAnswerMaintainsConversationContract()
        {
            var result = await Call(HttpStatusCode.OK,
                "{\"status\":\"answered\",\"answer\":\"Nhân viên mã 21.\",\"conversationId\":\"server-conversation\",\"conversationVersion\":3}");
            Assert.That(result.Status, Is.EqualTo("answered"));
            Assert.That(result.Answer, Is.EqualTo("Nhân viên mã 21."));
            Assert.That(result.ConversationId, Is.EqualTo("server-conversation"));
            Assert.That(result.ConversationVersion, Is.EqualTo(3));
            Assert.That(result.RequestId, Is.Not.Empty);
        }

        private static AiAuthorizationContext CreateActor()
        {
            return new AiAuthorizationContext
            {
                UserId = 1,
                Manv = 10,
                HasAllScope = true,
                FunctionRights = new HashSet<string>(new[] { "F_DM_NHANVIEN", "F_CC_BANGLUONG" }, StringComparer.OrdinalIgnoreCase)
            };
        }
        private sealed class CodeLookup : IEntityLookupProvider
        {
            public List<EntityCandidate> FindEmployees(string text, AiAuthorizationContext ctx) { return new List<EntityCandidate>(); }
            public EntityCandidate FindEmployeeById(int id, AiAuthorizationContext ctx)
            {
                return id == 21 ? new EntityCandidate { Id = 21, Name = "Nhân viên thử nghiệm", DepartmentId = 2, CompanyCode = "1" } : null;
            }
            public EntityCandidate FindDepartmentByName(string text, AiAuthorizationContext ctx) { return null; }
        }

        [TestCase("thông tin nhân viên mã số 21")]
        [TestCase("thông tin nhân viên mã số: 21")]
        [TestCase("thông tin nhân viên mã 21")]
        [TestCase("thông tin nhân viên mã nv 21")]
        [TestCase("thông tin nhân viên mã nhân viên 21")]
        [TestCase("thông tin nhân viên manv=21")]
        public void EmployeeCodeIsResolvedAndSqlIsRestrictedToThatEmployee(string question)
        {
            var ctx = CreateActor();
            var understanding = new QueryUnderstandingService(entityResolver: new EntityResolver(new CodeLookup()));
            var request = understanding.UnderstandQuery(question, ctx);
            Assert.That(request.Entities.Single(e => e.EntityType == "EMPLOYEE").ResolvedId, Is.EqualTo(21));
            var plan = new QueryPlanner().CreatePlan(request, ctx);
            Assert.That(plan.Strategy, Is.EqualTo(ExecutionStrategy.SqlTemplate));
            Assert.That(plan.Parameters[":p_manv"], Is.EqualTo(21));
        }

        [Test]
        public void PayrollForCode21RequiresOtherEmployeePermission()
        {
            var request = new QueryUnderstandingResult { Domain = "PAYROLL", Operation = "VIEW", Metric = "THUCLANH" };
            var ctx = CreateActor();
            Assert.That(QueryPlanner.DetermineCapability(request, ctx, "lương mã số 21 tháng 3 năm 2026"), Is.EqualTo("PAYROLL_VIEW"));
        }
    }
}