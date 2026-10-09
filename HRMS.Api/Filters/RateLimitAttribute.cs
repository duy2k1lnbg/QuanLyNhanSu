using HRMS_API.Services;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace HRMS_API.Filters
{
    public enum RateLimitPolicy
    {
        Login,
        DesktopToken,
        AiChat,
        BusinessRead,
        BusinessWrite
    }

    /// <summary>
    /// ActionFilterAttribute áp dụng Rate Limiting và Concurrency Control theo Section 11.
    /// Trả về HTTP 429 Too Many Requests kèm Retry-After header và DTO chuẩn.
    /// </summary>
    public class RateLimitAttribute : ActionFilterAttribute
    {
        public RateLimitPolicy Policy { get; set; } = RateLimitPolicy.BusinessRead;

        private const string AiSlotAcquiredKey = "__RateLimit_AiSlotAcquired__";
        private const string AiSlotUserIdKey = "__RateLimit_AiSlotUserId__";
        private const string AiSlotLeaseIdKey = "__RateLimit_AiSlotLeaseId__";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var request = actionContext.Request;
            var limiter = RateLimiterService.Instance;
            string clientIp = RateLimiterService.ResolveClientIp(request);
            string correlationId = Guid.NewGuid().ToString("N");

            // OPTIONS preflight không tiêu thụ quota
            if (request.Method == HttpMethod.Options)
            {
                base.OnActionExecuting(actionContext);
                return;
            }

            // Lấy User ID nếu đã xác thực
            var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(request);
            int userId = (jwtUser != null && int.TryParse(jwtUser.UserId, out int uid)) ? uid : 0;

            int retryAfter = 0;
            string violationMessage = null;

            switch (Policy)
            {
                case RateLimitPolicy.Login:
                    // 30 req/phút theo IP
                    if (!limiter.CheckIpRate(clientIp, "login_ip", 30, TimeSpan.FromMinutes(1), out retryAfter))
                    {
                        violationMessage = $"Địa chỉ IP của bạn gửi quá nhiều yêu cầu đăng nhập. Vui lòng thử lại sau {retryAfter} giây.";
                    }
                    break;

                case RateLimitPolicy.DesktopToken:
                    // 30 req/phút theo IP và 15 req/phút theo User
                    if (!limiter.CheckIpRate(clientIp, "desktop_token_ip", 30, TimeSpan.FromMinutes(1), out retryAfter))
                    {
                        violationMessage = $"Quá nhiều yêu cầu cấp token từ IP này. Vui lòng thử lại sau {retryAfter} giây.";
                    }
                    else if (userId > 0 && !limiter.CheckUserRate(userId, "desktop_token_user", 15, TimeSpan.FromMinutes(1), out retryAfter))
                    {
                        violationMessage = $"Tài khoản của bạn yêu cầu cấp token quá nhanh. Vui lòng thử lại sau {retryAfter} giây.";
                    }
                    break;

                case RateLimitPolicy.AiChat:
                    // 6 yêu cầu/phút/tài khoản
                    int aiUser = userId > 0 ? userId : clientIp.GetHashCode();
                    if (!limiter.CheckUserRate(aiUser, "ai_chat_rate", 6, TimeSpan.FromMinutes(1), out retryAfter))
                    {
                        violationMessage = $"Bạn gửi tin nhắn AI quá nhanh (tối đa 6 tin/phút). Vui lòng thử lại sau {retryAfter} giây.";
                    }
                    else if (!limiter.TryAcquireAiChatSlot(aiUser, out string leaseId, out string rejectionReason, out retryAfter))
                    {
                        violationMessage = rejectionReason;
                    }
                    else
                    {
                        // Ghi nhận slot thành công để OnActionExecuted giải phóng
                        actionContext.Request.Properties[AiSlotAcquiredKey] = true;
                        actionContext.Request.Properties[AiSlotUserIdKey] = aiUser;
                        actionContext.Request.Properties[AiSlotLeaseIdKey] = leaseId;
                    }
                    break;

                case RateLimitPolicy.BusinessRead:
                    // 120 yêu cầu/phút/tài khoản (hoặc IP nếu chưa đăng nhập)
                    int readActor = userId > 0 ? userId : clientIp.GetHashCode();
                    if (!limiter.CheckUserRate(readActor, "biz_read", 120, TimeSpan.FromMinutes(1), out retryAfter))
                    {
                        violationMessage = $"Bạn gửi quá nhiều yêu cầu tải dữ liệu. Vui lòng thử lại sau {retryAfter} giây.";
                    }
                    break;

                case RateLimitPolicy.BusinessWrite:
                    // 30 yêu cầu/phút/tài khoản
                    int writeActor = userId > 0 ? userId : clientIp.GetHashCode();
                    if (!limiter.CheckUserRate(writeActor, "biz_write", 30, TimeSpan.FromMinutes(1), out retryAfter))
                    {
                        violationMessage = $"Bạn gửi quá nhiều thao tác ghi dữ liệu. Vui lòng thử lại sau {retryAfter} giây.";
                    }
                    break;
            }

            if (!string.IsNullOrWhiteSpace(violationMessage))
            {
                actionContext.Response = CreateRateLimitResponse(violationMessage, retryAfter, correlationId);
                return;
            }

            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                // Giải phóng slot AI Chat nếu đã chiếm giữ
                if (actionExecutedContext.Request.Properties.TryGetValue(AiSlotAcquiredKey, out object acquiredObj) && (bool)acquiredObj)
                {
                    if (actionExecutedContext.Request.Properties.TryGetValue(AiSlotUserIdKey, out object userObj))
                    {
                        string leaseId = null;
                        if (actionExecutedContext.Request.Properties.TryGetValue(AiSlotLeaseIdKey, out object leaseObj))
                        {
                            leaseId = leaseObj as string;
                        }
                        RateLimiterService.Instance.ReleaseAiChatSlot((int)userObj, leaseId);
                    }
                }
            }
            finally
            {
                base.OnActionExecuted(actionExecutedContext);
            }
        }

        private static HttpResponseMessage CreateRateLimitResponse(string message, int retryAfterSeconds, string correlationId)
        {
            var response = new HttpResponseMessage((HttpStatusCode)429);
            response.Headers.Add("Retry-After", retryAfterSeconds.ToString());
            response.Headers.Add("X-Correlation-Id", correlationId);

            var payload = new
            {
                success = false,
                code = "RATE_LIMITED",
                message,
                retryAfterSeconds,
                correlationId
            };

            string json = JsonConvert.SerializeObject(payload);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return response;
        }
    }
}
