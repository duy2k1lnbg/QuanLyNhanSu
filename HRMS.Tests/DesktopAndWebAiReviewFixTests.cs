using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bu.CLASS_SYSTEM;
using Bu.Services.AI_Services;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Security;
using Bu.Tests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using QLyNSu.Functions;

namespace HRMS.Tests
{
    public class MockFakeLlmService : ILlmService
    {
        private readonly string _response;
        public MockFakeLlmService(string response) { _response = response; }

        public Task<string> AskSql(string prompt) => Task.FromResult(_response);
        public Task<string> AskIntent(string prompt) => Task.FromResult(_response);
        public Task<string> AskChat(string context, string question, string history, Action<string> onTokenReceived = null) => Task.FromResult(_response);
        public Task<float[]> GetEmbedding(string text, CancellationToken cancellationToken = default) => Task.FromResult(new float[0]);
        public Task<string> AskStructuredJsonAsync(string prompt, string system, CancellationToken cancellationToken = default) => Task.FromResult(_response);
        public Task<string> GenerateGroundedAnswerAsync(string question, string factsEvidence, string history = null, CancellationToken cancellationToken = default) => Task.FromResult(_response);
    }

    public class MockHttpAiHandler : HttpMessageHandler
    {
        public string LastRequestBody { get; set; }
        public bool HadAuthorizationHeader { get; set; }
        public HttpStatusCode StatusCodeToReturn { get; set; } = HttpStatusCode.OK;
        public string ResponseJsonToReturn { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HadAuthorizationHeader = request.Headers.Authorization != null;
            if (request.Content != null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync();
            }

            var resp = new HttpResponseMessage(StatusCodeToReturn);
            if (!string.IsNullOrEmpty(ResponseJsonToReturn))
            {
                resp.Content = new StringContent(ResponseJsonToReturn, System.Text.Encoding.UTF8, "application/json");
            }
            return resp;
        }
    }

    [TestFixture]
    public class DesktopAndWebAiReviewFixTests
    {
        [SetUp]
        public void Setup()
        {
            UserSession.CurrentUser = null;
            UserSession.CurrentSessionId = null;
            UserSession.CurrentJti = null;
            UserSession.UserRights = new List<string>();
        }

        [TearDown]
        public void TearDown()
        {
            UserSession.CurrentUser = null;
            UserSession.CurrentSessionId = null;
            UserSession.CurrentJti = null;
            UserSession.UserRights = new List<string>();
        }

        [Test]
        public async Task Grounding_LLM_Hallucination_Count15_Returns15_And_BypassesLlm()
        {
            // Requirement E: Khi SQL fixture trả về 15 nhân viên, và LLM bịa ra 999 nhân viên:
            // Hệ thống phát hiện dữ kiện không đúng và trả về kết quả Renderer chuẩn xác 15, BypassedLlm = true.
            var clock = new FakeClockProvider(new DateTime(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc));
            var sqlExecutor = new RegressionSqlExecutor(); // returns TOTAL_COUNT = 15
            var fakeLlm = new MockFakeLlmService("Hệ thống có 999 nhân viên. Đây là số liệu đã được xác nhận.");
            var sessions = new ConversationStateManager(clock);
            var cache = new AiCacheCoordinator(clock);
            var understanding = new QueryUnderstandingService(clock, new EntityResolver(new FakeEntityLookupProvider()));

            var service = new AiExecutionService(
                understandingService: understanding,
                sqlExecutor: sqlExecutor,
                conversationManager: sessions,
                cacheCoordinator: cache,
                clock: clock,
                llmService: fakeLlm
            );

            var ctx = AiTestContexts.All();
            var result = await service.ProcessChatAsync("Có bao nhiêu nhân viên trong công ty?", ctx, "grounding_test_conv");

            Assert.IsNotNull(result);
            Assert.AreEqual("answered", result.Status);
            StringAssert.Contains("15", result.Answer, "Số lượng nhân sự nguồn (15) phải được bảo toàn.");
            Assert.IsFalse(result.Answer.Contains("999"), "Số lượng bịa đặt từ LLM (999) không được xuất hiện trong câu trả lời.");
            Assert.IsTrue(result.BypassedLlm, "Hệ thống phải đánh dấu BypassedLlm = true vì LLM không đúng với dữ kiện thực tế.");
        }

        [Test]
        public async Task DesktopAiApiClient_TypedDto_ParsesCorrectFields_And_MaintainsState()
        {
            // Requirement C: AiApiClient parse đúng targetField/optionToken và gửi expectedConversationVersion/clarificationId
            var mockHandler = new MockHttpAiHandler
            {
                ResponseJsonToReturn = JsonConvert.SerializeObject(new
                {
                    status = "needs_clarification",
                    answer = "Vui lòng chọn nhân viên cần tra cứu",
                    conversationId = "server-conv-101",
                    conversationVersion = 2,
                    clarification = new
                    {
                        clarificationId = "clar-uuid-1",
                        targetField = "MANV",
                        question = "Bạn muốn xem ai?",
                        options = new[]
                        {
                            new { optionToken = "tok-an-123", label = "Nguyễn Văn An", value = "10" }
                        }
                    }
                })
            };

            var client = new AiApiClient();
            typeof(AiApiClient).GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(client, new HttpClient(mockHandler));

            // Turn 1: Gửi câu hỏi khởi tạo
            var res1 = await client.SendChatAsync("Tra cứu thông tin nhân viên An", "client-temp-conv");

            Assert.IsNotNull(res1);
            Assert.AreEqual("needs_clarification", res1.Status);
            Assert.AreEqual("server-conv-101", res1.ConversationId, "Phải cập nhật conversationId server trả về.");
            Assert.AreEqual(2, res1.ConversationVersion);
            Assert.IsNotNull(res1.Clarification);
            Assert.AreEqual("MANV", res1.Clarification.Field, "targetField phải được parse thành Field thành công.");
            Assert.AreEqual("clar-uuid-1", res1.Clarification.ClarificationId);
            Assert.AreEqual(1, res1.Clarification.Options.Count);
            Assert.AreEqual("tok-an-123", res1.Clarification.Options[0].Token, "optionToken phải được parse thành Token thành công.");

            // Turn 2: Chọn phương án làm rõ
            await client.SendChatAsync("", "server-conv-101", "tok-an-123");

            Assert.IsNotNull(mockHandler.LastRequestBody);
            var reqJson = JObject.Parse(mockHandler.LastRequestBody);

            Assert.AreEqual(2, (long)reqJson["expectedConversationVersion"], "Phải truyền expectedConversationVersion từ turn trước.");
            Assert.IsNotNull(reqJson["clarification"], "Phải có clarification object trong request.");
            Assert.AreEqual("clar-uuid-1", (string)reqJson["clarification"]["clarificationId"], "Phải truyền clarificationId trong DTO.");
            Assert.AreEqual("tok-an-123", (string)reqJson["optionToken"], "Phải truyền optionToken trong request.");
        }

        [Test]
        public async Task DesktopChatboxManager_Remote_Reset_ClearsState_WithoutNullReference()
        {
            // Requirement D: Reset remote không được ném NullReferenceException và phải làm sạch state
            bool resetCalledOnRemote = false;
            string resetConvId = null;

            var manager = new ChatboxManager(
                conversationId: "conv-initial",
                executionService: null,
                remoteChatHandler: (q, c, o, v, cl, ct) => Task.FromResult(new AiChatExecutionResult
                {
                    Status = "answered",
                    Answer = "Kết quả câu hỏi 1",
                    ConversationId = c,
                    ConversationVersion = 1
                }),
                remoteResetHandler: (cid, ct) =>
                {
                    resetCalledOnRemote = true;
                    resetConvId = cid;
                    return Task.FromResult(true);
                }
            );

            await manager.ProcessQueryAsync("Câu hỏi đầu tiên");
            Assert.AreEqual(2, manager.GetMessages().Count);

            // Thực hiện Reset
            Assert.DoesNotThrow(() => manager.Reset(), "Reset không được ném NullReferenceException.");

            Assert.AreEqual(0, manager.GetMessages().Count, "Lịch sử tin nhắn phải được làm sạch sau Reset.");
            Assert.AreNotEqual("conv-initial", manager.ConversationId, "Reset phải sinh conversationId mới.");
        }

        [Test]
        public async Task DesktopDashboard_ErrorAndForbidden_DoesNotShowZeroEmployees()
        {
            // Requirement G: Lỗi hoặc thiếu quyền không được coi là "0 nhân viên"
            var mockHandler = new MockHttpAiHandler
            {
                StatusCodeToReturn = HttpStatusCode.Forbidden,
                ResponseJsonToReturn = JsonConvert.SerializeObject(new
                {
                    status = "forbidden",
                    countStatus = "forbidden",
                    listStatus = "forbidden",
                    message = "Bạn chưa được cấp quyền xem dữ liệu bảng điều khiển."
                })
            };

            var client = new AiApiClient();
            typeof(AiApiClient).GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(client, new HttpClient(mockHandler));

            var result = await client.GetDashboardAsync();

            Assert.AreEqual("error", result.Status);
            Assert.IsFalse(result.HasCountData, "Không có dữ liệu đếm khi bị từ chối quyền.");
            Assert.AreNotEqual("ok", result.CountStatus);
            StringAssert.Contains("quyền", result.Message.ToLower());
        }

        [Test]
        public void DesktopAuthContext_RequiresDesktopLoginRight_WhenCheckingChannel()
        {
            // Requirement B: Kiểm tra quyền kênh Desktop và F_LOGIN_DESKTOP
            UserSession.CurrentUser = new DA.TB_SYS_USER
            {
                IDUSER = 88,
                USERNAME = "desktop_staff",
                FULLNAME = "Desktop Staff"
            };
            UserSession.CurrentSessionId = "sess-88";
            UserSession.CurrentJti = "jti-88";
            UserSession.CurrentChannel = "DESKTOP";
            UserSession.UserRights = new List<string> { "F_LOGIN_DESKTOP", "F_SYSTEM_AI" };

            // Người dùng có F_LOGIN_DESKTOP và F_SYSTEM_AI được dùng AI trên kênh Desktop mà không cần F_LOGIN_WEB
            Assert.IsTrue(UserSession.IsLoggedIn);
            Assert.IsFalse(UserSession.UserRights.Contains("F_LOGIN_WEB"));
            Assert.IsTrue(UserSession.UserRights.Contains("F_LOGIN_DESKTOP"));
        }
    }
}
