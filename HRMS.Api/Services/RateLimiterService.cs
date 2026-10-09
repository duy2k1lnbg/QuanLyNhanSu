using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace HRMS_API.Services
{
    /// <summary>
    /// Quản lý giới hạn tần suất yêu cầu (Rate Limiting) và tải đồng thời (Concurrency Limiting)
    /// theo nguyên tắc sliding-window in-memory, thread-safe, có TTL và tự thu hồi tài nguyên.
    /// Tuân thủ đặc tả Section 11 của hệ thống.
    /// </summary>
    public class RateLimiterService
    {
        private static readonly Lazy<RateLimiterService> _instance =
            new Lazy<RateLimiterService>(() => new RateLimiterService());

        public static RateLimiterService Instance => _instance.Value;

        private readonly ConcurrentDictionary<string, WindowEntry> _rateBuckets =
            new ConcurrentDictionary<string, WindowEntry>(StringComparer.OrdinalIgnoreCase);

        // Quản lý concurrency cho AI Chat: tối đa 1 tác vụ/tài khoản, tối đa 2 tác vụ toàn máy
        private readonly ConcurrentDictionary<int, DateTime> _activeAiUsers =
            new ConcurrentDictionary<int, DateTime>();

        private const int MaxGlobalAiTasks = 2;
        private const int MaxUserAiTasks = 1;

        private DateTime _lastCleanup = DateTime.UtcNow;
        private readonly object _cleanupLock = new object();

        private class WindowEntry
        {
            public readonly object Lock = new object();
            public readonly List<DateTime> Timestamps = new List<DateTime>();
        }

        private RateLimiterService()
        {
        }

        /// <summary>
        /// Kiểm tra sliding-window rate limit cho một khóa nhất định.
        /// </summary>
        public bool CheckRateLimit(string bucketKey, int maxRequests, TimeSpan window, out int retryAfterSeconds)
        {
            retryAfterSeconds = 0;
            if (string.IsNullOrWhiteSpace(bucketKey) || maxRequests <= 0)
                return true;

            MaybeCleanupOldEntries();

            var entry = _rateBuckets.GetOrAdd(bucketKey, _ => new WindowEntry());
            var now = DateTime.UtcNow;
            var windowStart = now - window;

            lock (entry.Lock)
            {
                // Loại bỏ các mốc thời gian ngoài cửa sổ
                entry.Timestamps.RemoveAll(t => t < windowStart);

                if (entry.Timestamps.Count >= maxRequests)
                {
                    // Đạt ngưỡng: tính Retry-After từ request cũ nhất trong cửa sổ
                    var oldest = entry.Timestamps.FirstOrDefault();
                    var expiry = oldest + window;
                    var remaining = (int)Math.Ceiling((expiry - now).TotalSeconds);
                    retryAfterSeconds = Math.Max(1, remaining);
                    return false;
                }

                entry.Timestamps.Add(now);
                return true;
            }
        }

        /// <summary>
        /// Kiểm tra rate limit theo username chuẩn hóa cho đăng nhập mật khẩu (5 req/phút).
        /// </summary>
        public bool CheckLoginUsernameRate(string username, int maxRequests, TimeSpan window, out int retryAfterSeconds)
        {
            string key = $"login:user:{(username ?? "unknown").Trim().ToUpperInvariant()}";
            return CheckRateLimit(key, maxRequests, window, out retryAfterSeconds);
        }

        /// <summary>
        /// Kiểm tra rate limit theo IP máy khách (cho đăng nhập, anonymous, pre-auth).
        /// </summary>
        public bool CheckIpRate(string ip, string actionKey, int maxRequests, TimeSpan window, out int retryAfterSeconds)
        {
            string key = $"ip:{(ip ?? "unknown").Trim()}:{actionKey}";
            return CheckRateLimit(key, maxRequests, window, out retryAfterSeconds);
        }

        /// <summary>
        /// Kiểm tra rate limit theo User ID đã xác minh.
        /// </summary>
        public bool CheckUserRate(int userId, string actionKey, int maxRequests, TimeSpan window, out int retryAfterSeconds)
        {
            string key = $"user:{userId}:{actionKey}";
            return CheckRateLimit(key, maxRequests, window, out retryAfterSeconds);
        }

        public class AiConcurrencyLease
        {
            public string LeaseId { get; set; }
            public int UserId { get; set; }
            public DateTime AcquiredAt { get; set; }
        }

        private readonly object _concurrencyLock = new object();
        private readonly Dictionary<int, AiConcurrencyLease> _activeAiLeases = new Dictionary<int, AiConcurrencyLease>();

        /// <summary>
        /// Chiếm giữ slot concurrency cho AI Chat (tối đa 1 slot/user, tối đa 2 slot toàn máy) nguyên tử tuyệt đối.
        /// Trả về lease token định danh quyền sở hữu slot.
        /// </summary>
        public bool TryAcquireAiChatSlot(int userId, out string leaseId, out string reason, out int retryAfterSeconds)
        {
            leaseId = null;
            reason = null;
            retryAfterSeconds = 0;

            var now = DateTime.UtcNow;

            lock (_concurrencyLock)
            {
                // Active leases are strictly tracked and preserved until explicitly released via ReleaseAiChatSlot.
                // 1. Kiểm tra slot tài khoản
                if (_activeAiLeases.ContainsKey(userId))
                {
                    reason = "Bạn đang có một tác vụ AI đang xử lý. Vui lòng chờ tác vụ hiện tại hoàn tất.";
                    retryAfterSeconds = 5;
                    return false;
                }

                // 2. Kiểm tra slot toàn hệ thống (Nguyên tử dưới lock - chặn đứng race condition!)
                if (_activeAiLeases.Count >= MaxGlobalAiTasks)
                {
                    reason = "Máy chủ AI hiện đang phục vụ tối đa tải đồng thời. Vui lòng thử lại sau giây lát.";
                    retryAfterSeconds = 10;
                    return false;
                }

                // Cấp lease mới
                leaseId = Guid.NewGuid().ToString("N");
                _activeAiLeases[userId] = new AiConcurrencyLease
                {
                    LeaseId = leaseId,
                    UserId = userId,
                    AcquiredAt = now
                };
                return true;
            }
        }

        /// <summary>
        /// Overload tương thích ngược khi caller không nhận leaseId.
        /// </summary>
        public bool TryAcquireAiChatSlot(int userId, out string reason, out int retryAfterSeconds)
        {
            return TryAcquireAiChatSlot(userId, out _, out reason, out retryAfterSeconds);
        }

        /// <summary>
        /// Giải phóng slot concurrency AI Chat an toàn, có kiểm tra lease token sở hữu.
        /// </summary>
        public void ReleaseAiChatSlot(int userId, string leaseId = null)
        {
            lock (_concurrencyLock)
            {
                if (_activeAiLeases.TryGetValue(userId, out var lease))
                {
                    if (string.IsNullOrEmpty(leaseId) || string.Equals(lease.LeaseId, leaseId, StringComparison.Ordinal))
                    {
                        _activeAiLeases.Remove(userId);
                    }
                }
            }
        }

        /// <summary>
        /// Trích xuất IP client an toàn từ HttpRequestMessage mà không tin cậy header giả mạo tùy tiện.
        /// </summary>
        public static string ResolveClientIp(HttpRequestMessage request)
        {
            if (request == null) return "127.0.0.1";

            if (request.Properties.TryGetValue("ClientIpAddress", out object directIp) && directIp is string strIp && !string.IsNullOrWhiteSpace(strIp))
            {
                return strIp;
            }

            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                var ctx = request.Properties["MS_HttpContext"] as HttpContextBase;
                if (ctx != null && !string.IsNullOrWhiteSpace(ctx.Request.UserHostAddress))
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            if (request.Properties.ContainsKey("MS_OwinContext"))
            {
                try
                {
                    dynamic owin = request.Properties["MS_OwinContext"];
                    if (owin?.Request?.RemoteIpAddress != null)
                    {
                        return (string)owin.Request.RemoteIpAddress;
                    }
                }
                catch { }
            }

            return "127.0.0.1";
        }

        private void MaybeCleanupOldEntries()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastCleanup).TotalMinutes < 5) return;

            lock (_cleanupLock)
            {
                if ((now - _lastCleanup).TotalMinutes < 5) return;
                _lastCleanup = now;

                var cutoff = now.AddMinutes(-5);
                foreach (var kvp in _rateBuckets)
                {
                    lock (kvp.Value.Lock)
                    {
                        kvp.Value.Timestamps.RemoveAll(t => t < cutoff);
                    }
                    if (kvp.Value.Timestamps.Count == 0)
                    {
                        _rateBuckets.TryRemove(kvp.Key, out _);
                    }
                }
            }
        }
    }
}
