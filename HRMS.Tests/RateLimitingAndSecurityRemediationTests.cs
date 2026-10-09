using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using HRMS_API.Filters;
using HRMS_API.Services;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class RateLimitingAndSecurityRemediationTests
    {
        [Test]
        public void SlidingWindow_AllowsUpToLimit_BlocksExceeded_AndCalculatesRetryAfter()
        {
            var limiter = RateLimiterService.Instance;
            string testBucket = "test:window:" + Guid.NewGuid().ToString("N");

            // Cho phép đúng 3 yêu cầu trong 10 giây
            Assert.IsTrue(limiter.CheckRateLimit(testBucket, 3, TimeSpan.FromSeconds(10), out int retry1));
            Assert.AreEqual(0, retry1);

            Assert.IsTrue(limiter.CheckRateLimit(testBucket, 3, TimeSpan.FromSeconds(10), out int retry2));
            Assert.AreEqual(0, retry2);

            Assert.IsTrue(limiter.CheckRateLimit(testBucket, 3, TimeSpan.FromSeconds(10), out int retry3));
            Assert.AreEqual(0, retry3);

            // Yêu cầu thứ 4 phải bị từ chối
            Assert.IsFalse(limiter.CheckRateLimit(testBucket, 3, TimeSpan.FromSeconds(10), out int retry4));
            Assert.GreaterOrEqual(retry4, 1);
            Assert.LessOrEqual(retry4, 10);
        }

        [Test]
        public void LoginUsernameRate_IndependentPerUser_CaseInsensitive()
        {
            var limiter = RateLimiterService.Instance;
            string userA = "testuser_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            string userB = "testuser_other_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            // Đăng nhập 5 lần cho userA -> Đạt ngưỡng
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(limiter.CheckLoginUsernameRate(userA, 5, TimeSpan.FromMinutes(1), out _));
            }

            // Lần thứ 6 với userA (thử cả chữ hoa/thường) -> Bị chặn
            Assert.IsFalse(limiter.CheckLoginUsernameRate(userA.ToUpperInvariant(), 5, TimeSpan.FromMinutes(1), out int retrySec));
            Assert.Greater(retrySec, 0);

            // userB độc lập hoàn toàn, vẫn được phép
            Assert.IsTrue(limiter.CheckLoginUsernameRate(userB, 5, TimeSpan.FromMinutes(1), out _));
        }

        [Test]
        public void IpRateLimiting_SeparatesDifferentIps()
        {
            var limiter = RateLimiterService.Instance;
            string ip1 = "192.168.1.101";
            string ip2 = "192.168.1.102";
            string action = "login_action_" + Guid.NewGuid().ToString("N");

            // Ip1 gửi 2 request với limit = 2
            Assert.IsTrue(limiter.CheckIpRate(ip1, action, 2, TimeSpan.FromMinutes(1), out _));
            Assert.IsTrue(limiter.CheckIpRate(ip1, action, 2, TimeSpan.FromMinutes(1), out _));
            Assert.IsFalse(limiter.CheckIpRate(ip1, action, 2, TimeSpan.FromMinutes(1), out int retryIp1));
            Assert.Greater(retryIp1, 0);

            // Ip2 không bị ảnh hưởng bởi Ip1
            Assert.IsTrue(limiter.CheckIpRate(ip2, action, 2, TimeSpan.FromMinutes(1), out _));
        }

        [Test]
        public void AiChatConcurrency_EnforcesUserLimitAndGlobalCap_WithSafeRelease()
        {
            var limiter = RateLimiterService.Instance;
            int uid1 = 99901;
            int uid2 = 99902;
            int uid3 = 99903;

            // Dọn sạch trước khi test
            limiter.ReleaseAiChatSlot(uid1);
            limiter.ReleaseAiChatSlot(uid2);
            limiter.ReleaseAiChatSlot(uid3);

            try
            {
                // 1. User 1 chiếm slot đầu tiên -> Thành công
                Assert.IsTrue(limiter.TryAcquireAiChatSlot(uid1, out string reason1, out int retry1));
                Assert.IsNull(reason1);

                // User 1 cố chiếm slot thứ 2 khi tác vụ chưa hoàn thành -> Từ chối (Tối đa 1 slot/user)
                Assert.IsFalse(limiter.TryAcquireAiChatSlot(uid1, out string reasonDup, out int retryDup));
                StringAssert.Contains("Bạn đang có một tác vụ AI đang xử lý", reasonDup);
                Assert.Greater(retryDup, 0);

                // 2. User 2 chiếm slot thứ 2 -> Thành công (Global = 2)
                Assert.IsTrue(limiter.TryAcquireAiChatSlot(uid2, out string reason2, out int retry2));

                // 3. User 3 cố chiếm slot -> Từ chối vì đã chạm Global Cap = 2
                Assert.IsFalse(limiter.TryAcquireAiChatSlot(uid3, out string reasonGlobal, out int retryGlobal));
                StringAssert.Contains("tối đa tải đồng thời", reasonGlobal);
                Assert.Greater(retryGlobal, 0);

                // 4. User 1 hoàn tất và giải phóng slot
                limiter.ReleaseAiChatSlot(uid1);

                // User 3 bây giờ có thể chiếm slot thành công!
                Assert.IsTrue(limiter.TryAcquireAiChatSlot(uid3, out string reason3, out int retry3));

                // 5. Giải phóng hết slot
                limiter.ReleaseAiChatSlot(uid2);
                limiter.ReleaseAiChatSlot(uid3);
            }
            finally
            {
                limiter.ReleaseAiChatSlot(uid1);
                limiter.ReleaseAiChatSlot(uid2);
                limiter.ReleaseAiChatSlot(uid3);
            }
        }

        [Test]
        public void AiChatConcurrency_ConcurrentRace16Threads_NeverExceedsGlobalCap2()
        {
            var limiter = RateLimiterService.Instance;
            int threadCount = 16;
            var barrier = new System.Threading.Barrier(threadCount);
            int successCount = 0;
            var acquiredUsers = new System.Collections.Concurrent.ConcurrentBag<int>();

            // Dọn sạch trước khi test
            for (int i = 0; i < threadCount; i++) limiter.ReleaseAiChatSlot(88800 + i);

            try
            {
                var tasks = new System.Threading.Tasks.Task[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    int uid = 88800 + i;
                    tasks[i] = System.Threading.Tasks.Task.Run(() =>
                    {
                        barrier.SignalAndWait(); // Đồng loạt kích hoạt tại cùng một thời điểm
                        if (limiter.TryAcquireAiChatSlot(uid, out string lease, out string reason, out int retry))
                        {
                            System.Threading.Interlocked.Increment(ref successCount);
                            acquiredUsers.Add(uid);
                        }
                    });
                }

                System.Threading.Tasks.Task.WaitAll(tasks);

                // Khẳng định: MaxGlobalAiTasks = 2, tuyệt đối không bị race vượt ngưỡng!
                Assert.AreEqual(2, successCount, "Tuyệt đối chỉ có đúng 2 luồng được cấp slot dù 16 luồng cạnh tranh đồng thời!");
            }
            finally
            {
                foreach (var u in acquiredUsers) limiter.ReleaseAiChatSlot(u);
            }
        }

        [Test]
        public void RateLimitAttribute_OptionsPreflight_BypassesRateLimit()
        {
            var filter = new RateLimitAttribute { Policy = RateLimitPolicy.Login };
            var request = new HttpRequestMessage(HttpMethod.Options, "http://localhost/api/auth/login");
            var actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext { Request = request }
            };

            filter.OnActionExecuting(actionContext);

            // Response không bị set thành 429
            Assert.IsNull(actionContext.Response);
        }

        [Test]
        public void RateLimitAttribute_WhenRateLimited_Produces429_WithRequiredHeadersAndPayload()
        {
            var filter = new RateLimitAttribute { Policy = RateLimitPolicy.Login };
            string fakeIp = "10.0.0.99";

            // Làm cạn kiệt bucket 30 req/min của fakeIp
            for (int i = 0; i < 30; i++)
            {
                RateLimiterService.Instance.CheckIpRate(fakeIp, "login_ip", 30, TimeSpan.FromMinutes(1), out _);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/auth/login");
            // Set test client IP
            request.Properties["ClientIpAddress"] = fakeIp;

            var actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext { Request = request }
            };

            filter.OnActionExecuting(actionContext);

            Assert.IsNotNull(actionContext.Response);
            Assert.AreEqual((HttpStatusCode)429, actionContext.Response.StatusCode);

            // Kiểm tra các header bắt buộc
            Assert.IsTrue(actionContext.Response.Headers.Contains("Retry-After"));
            Assert.IsTrue(actionContext.Response.Headers.Contains("X-Correlation-Id"));

            string body = actionContext.Response.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(body);
            Assert.AreEqual(false, (bool)json["success"]);
            Assert.AreEqual("RATE_LIMITED", (string)json["code"]);
            Assert.Greater((int)json["retryAfterSeconds"], 0);
        }

        [Test]
        public void Planner_PolicyDomain_CapabilityDenied_ReturnsForbiddenNotVectorPlan()
        {
            var planner = new Bu.Services.AI_Services.Core.QueryPlanner();
            var understanding = new Bu.Services.AI_Services.Core.QueryUnderstandingResult
            {
                Domain = "POLICY",
                Operation = "LOOKUP",
                OriginalQuestion = "Quy chế làm việc của công ty thế nào?"
            };

            // Context with explicit DENY on POLICY_LOOKUP
            var ctx = new Bu.Services.AI_Services.Security.AiAuthorizationContext
            {
                UserId = 999,
                ScopeGrants = new List<Bu.Services.AI_Services.Security.AiScopeGrant>
                {
                    new Bu.Services.AI_Services.Security.AiScopeGrant
                    {
                        CapabilityCode = "POLICY_LOOKUP",
                        Effect = "DENY"
                    }
                }
            };

            var plan = planner.CreatePlan(understanding, ctx);
            Assert.AreEqual(Bu.Services.AI_Services.Core.ExecutionStrategy.Forbidden, plan.Strategy);
            Assert.AreNotEqual(Bu.Services.AI_Services.Core.ExecutionStrategy.VectorSearch, plan.Strategy);
        }

        [Test]
        public void Planner_PolicyDomain_CapabilityAllowed_ReturnsVectorSearchPlan()
        {
            var planner = new Bu.Services.AI_Services.Core.QueryPlanner();
            var understanding = new Bu.Services.AI_Services.Core.QueryUnderstandingResult
            {
                Domain = "POLICY",
                Operation = "LOOKUP",
                OriginalQuestion = "Quy trình xin nghỉ phép ra sao?"
            };

            var ctx = new Bu.Services.AI_Services.Security.AiAuthorizationContext
            {
                UserId = 888
            };

            var plan = planner.CreatePlan(understanding, ctx);
            Assert.AreEqual(Bu.Services.AI_Services.Core.ExecutionStrategy.VectorSearch, plan.Strategy);
            Assert.AreEqual("POLICY_LOOKUP", plan.RequiredCapability);
        }

        [Test]
        public void Planner_HybridQuery_QuestionMentionsBothOvertimeAndPolicy_GeneratesHybridStrategy()
        {
            var planner = new Bu.Services.AI_Services.Core.QueryPlanner();
            var understanding = new Bu.Services.AI_Services.Core.QueryUnderstandingResult
            {
                Domain = "OVERTIME",
                Operation = "SUM",
                Metric = "SOGIO",
                RequestedScope = "SELF",
                OriginalQuestion = "Số giờ tăng ca tháng này của tôi và quy định tăng ca đang áp dụng?",
                Time = new Bu.Services.AI_Services.Core.TimeResolution { Month = 10, Year = 2026 }
            };

            var ctx = new Bu.Services.AI_Services.Security.AiAuthorizationContext
            {
                UserId = 777,
                Manv = 132,
                FunctionRights = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "F_CC_TANGCA" }
            };

            var plan = planner.CreatePlan(understanding, ctx);
            Assert.AreEqual(Bu.Services.AI_Services.Core.ExecutionStrategy.Hybrid, plan.Strategy);
            Assert.AreEqual("POLICY_LOOKUP", plan.VectorCapability);
        }

        [Test]
        public async Task QdrantService_MissingSecurityFilter_FailsClosed()
        {
            var service = new Bu.Services.AI_Services.Vector.QdrantService();
            var result = await service.SearchScopedAsync("test query", new Bu.Services.AI_Services.Vector.VectorBusinessFilter(), null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.SecurityFilterApplied.StartsWith("fail_closed"));
            Assert.AreEqual(0, result.Hits.Count);
        }

        [Test]
        public async Task QdrantService_TargetOutsideScope_FailsClosed()
        {
            var service = new Bu.Services.AI_Services.Vector.QdrantService();
            var secFilter = new Bu.Services.AI_Services.Vector.VectorSecurityFilter
            {
                IsAdmin = false,
                CallerEmployeeId = 100,
                TargetEmployeeId = 999, // Outside caller scope
                AllowedDepartmentIds = new List<int>()
            };

            var result = await service.SearchScopedAsync("test query", new Bu.Services.AI_Services.Vector.VectorBusinessFilter(), secFilter);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.SecurityFilterApplied.StartsWith("fail_closed"));
            Assert.AreEqual(0, result.Hits.Count);
        }

        private class FakeOwinContext
        {
            public FakeOwinRequest Request { get; }
            public FakeOwinContext(string ip)
            {
                Request = new FakeOwinRequest { RemoteIpAddress = ip };
            }
        }

        private class FakeOwinRequest
        {
            public string RemoteIpAddress { get; set; }
        }
    }
}
