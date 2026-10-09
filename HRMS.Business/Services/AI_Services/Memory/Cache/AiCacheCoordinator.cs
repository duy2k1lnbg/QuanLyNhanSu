using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Bu.Services.AI_Services.Core;

namespace Bu.Services.AI_Services.Memory
{
    public class CacheEntry<T>
    {
        public string Key { get; set; }
        public T Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime LastAccessed { get; set; }
        public long Revision { get; set; }
    }

    public class BoundedCacheStore<T>
    {
        private readonly ConcurrentDictionary<string, CacheEntry<T>> _store = new ConcurrentDictionary<string, CacheEntry<T>>(StringComparer.Ordinal);
        private readonly int _maxCapacity;
        private readonly TimeSpan _defaultTtl;
        private readonly IClockProvider _clock;
        private readonly object _evictionLock = new object();

        public BoundedCacheStore(int maxCapacity, TimeSpan defaultTtl, IClockProvider clock = null)
        {
            if (maxCapacity < 1) throw new ArgumentOutOfRangeException(nameof(maxCapacity));
            _maxCapacity = maxCapacity;
            _defaultTtl = defaultTtl;
            _clock = clock ?? new SystemClockProvider();
        }

        public int Count => _store.Count;

        public bool TryGet(string key, out T value)
        {
            value = default(T);
            if (string.IsNullOrWhiteSpace(key)) return false;

            if (_store.TryGetValue(key, out var entry))
            {
                if (_clock.UtcNow >= entry.ExpiresAt)
                {
                    ((ICollection<KeyValuePair<string, CacheEntry<T>>>)_store).Remove(new KeyValuePair<string, CacheEntry<T>>(key,entry));
                    return false;
                }

                entry.LastAccessed = _clock.UtcNow;
                value = entry.Value;
                return true;
            }

            return false;
        }

        public void Set(string key, T value, TimeSpan? ttl = null, long revision = 1)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            lock (_evictionLock)
            {
            PruneIfNecessary();

            var now = _clock.UtcNow;
            var expiry = now.Add(ttl ?? _defaultTtl);

            var entry = new CacheEntry<T>
            {
                Key = key,
                Value = value,
                CreatedAt = now,
                ExpiresAt = expiry,
                LastAccessed = now,
                Revision = revision
            };

            _store[key] = entry;
            }
        }

        public bool Invalidate(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            return _store.TryRemove(key, out _);
        }

        public void InvalidateAll()
        {
            _store.Clear();
        }

        public void PruneExpired()
        {
            var now = _clock.UtcNow;
            foreach (var kvp in _store)
            {
                if (now >= kvp.Value.ExpiresAt)
                {
                    ((ICollection<KeyValuePair<string, CacheEntry<T>>>)_store).Remove(kvp);
                }
            }
        }

        private void PruneIfNecessary()
        {
            if (_store.Count < _maxCapacity) return;

            lock (_evictionLock)
            {
                if (_store.Count < _maxCapacity) return;

                PruneExpired();

                // Nếu vẫn vượt ngưỡng capacity sau khi xóa hết hạn, xóa bớt 20% bản ghi cũ nhất (LRU)
                if (_store.Count >= _maxCapacity)
                {
                    int itemsToRemove = Math.Max(1, _maxCapacity / 5);
                    var oldestKeys = _store.OrderBy(x => x.Value.LastAccessed)
                                           .Take(itemsToRemove)
                                           .Select(x => x.Key)
                                           .ToList();

                    foreach (var k in oldestKeys)
                    {
                        _store.TryRemove(k, out _);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Điều phối đồng thời Single-Flight: Ngăn chặn Cache Stampede / Thundering Herd
    /// Đảm bảo nhiều request cùng key chỉ kích hoạt 1 lần truy vấn dữ liệu duy nhất.
    /// </summary>
    public class SingleFlightCoordinator
    {
        private readonly ConcurrentDictionary<string, Lazy<Task<object>>> _tasks = new ConcurrentDictionary<string, Lazy<Task<object>>>(StringComparer.Ordinal);

        public async Task<T> ExecuteAsync<T>(string key, Func<Task<T>> action)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return await action();
            }

            var lazyTask = _tasks.GetOrAdd(key, k => new Lazy<Task<object>>(async () => (object)await action()));
            try
            {
                return (T)await lazyTask.Value;
            }
            finally
            {
                // A late waiter must never remove a newer flight for the same key.
                ((ICollection<KeyValuePair<string, Lazy<Task<object>>>>)_tasks)
                    .Remove(new KeyValuePair<string, Lazy<Task<object>>>(key, lazyTask));
            }
        }
    }

    /// <summary>
    /// Bộ điều phối Cache phân tầng chuẩn hóa cho HRMS AI RAG V2 (Tuân thủ Section 10).
    /// </summary>
    public class AiCacheCoordinator
    {
        private static readonly Lazy<AiCacheCoordinator> _lazy = new Lazy<AiCacheCoordinator>(() => new AiCacheCoordinator());
        public static AiCacheCoordinator Instance => _lazy.Value;

        private readonly IClockProvider _clock;
        private readonly SingleFlightCoordinator _singleFlight = new SingleFlightCoordinator();
        private long _resultHits, _resultMisses, _resultBypasses, _resultLoads, _resultPublished;
        public Dictionary<string,long> GetResultMetrics() => new Dictionary<string,long> {
            {"entries",ResultCache.Count},{"hits",Interlocked.Read(ref _resultHits)},{"misses",Interlocked.Read(ref _resultMisses)},
            {"bypasses",Interlocked.Read(ref _resultBypasses)},{"loads",Interlocked.Read(ref _resultLoads)},{"published",Interlocked.Read(ref _resultPublished)} };

        // 1. Plan/Template Cache: TTL 1h, Bounded 1,000 items
        public BoundedCacheStore<QueryExecutionPlan> PlanCache { get; }

        // 2. Embedding Cache: TTL 24h, Bounded 5,000 items
        public BoundedCacheStore<float[]> EmbeddingCache { get; }

        // 3. Entity Lookup Cache: TTL 5m, Bounded 2,000 items
        public BoundedCacheStore<List<EntityCandidate>> EntityCache { get; }

        // 4. Result Cache: TTL 30-60s, Bounded 1,000 items (Chỉ dành cho dữ liệu không nhạy cảm)
        public BoundedCacheStore<RenderedResponse> ResultCache { get; }

        public AiCacheCoordinator(IClockProvider clock = null)
        {
            _clock = clock ?? new SystemClockProvider();

            PlanCache = new BoundedCacheStore<QueryExecutionPlan>(1000, TimeSpan.FromHours(1), _clock);
            EmbeddingCache = new BoundedCacheStore<float[]>(5000, TimeSpan.FromHours(24), _clock);
            EntityCache = new BoundedCacheStore<List<EntityCandidate>>(2000, TimeSpan.FromMinutes(5), _clock);
            ResultCache = new BoundedCacheStore<RenderedResponse>(1000, TimeSpan.FromSeconds(60), _clock);
        }

        #region Key Generation (Hashed & Sanitized)

        public static string HashKey(string prefix, string rawData)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawData ?? string.Empty));
                var sb = new StringBuilder(prefix).Append(":");
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static string BuildPlanKey(string domain, string operation, string metric, string filters, int schemaVersion = 1)
        {
            string raw = $"{domain}|{operation}|{metric}|{filters}|v{schemaVersion}";
            return HashKey("plan", raw);
        }

        public static string BuildEmbeddingKey(string text, string modelVersion = "bge-m3:v1", string ns = "public")
        {
            string raw = $"{ns}|{modelVersion}|{text?.Trim().ToLowerInvariant()}";
            return HashKey("emb", raw);
        }

        public static string BuildEntityKey(string mention, string capability, int userId, long revision = 1)
        {
            string raw = $"{mention?.Trim().ToLowerInvariant()}|{capability}|u:{userId}|rev:{revision}";
            return HashKey("ent", raw);
        }

        public static string BuildResultKey(
            int userId,
            string domain, 
            string operation, 
            string metric,
            string entityId, 
            string period, 
            string scope, 
            string filters = "",
            string fieldProfile = "",
            long policyRev = 1,
            long sourceRev = 1)
        {
            string raw = $"u:{userId}|d:{domain}|op:{operation}|m:{metric}|e:{entityId}|p:{period}|s:{scope}|f:{filters}|fp:{fieldProfile}|prev:{policyRev}|srev:{sourceRev}";
            return HashKey("res", raw);
        }

        public static string BuildResultKey(string domain, string operation, string entityId, string period, string scope, long policyRev = 1)
        {
            return BuildResultKey(0, domain, operation, "", entityId, period, scope, "", "", policyRev, 1);
        }

        public static string BuildResultKey(QueryExecutionPlan plan, Bu.Services.AI_Services.Security.AiAuthorizationContext ctx)
        {
            var parameters = plan.Parameters.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => new { p.Key, Type = p.Value?.GetType().FullName, p.Value }).ToArray();
            return HashKey("res-v3", Newtonsoft.Json.JsonConvert.SerializeObject(new {
                Schema = 3, ctx.UserId, Auth = ctx.Fingerprint(), plan.Domain, plan.Operation, plan.Metric,
                plan.TargetView, plan.SqlStatement, Parameters = parameters, plan.RequiredCapability, plan.RequiredScope, plan.RequestedScope,
                plan.RowLimit, Fields = plan.FieldModes.OrderBy(p => p.Key, StringComparer.Ordinal).ToArray(), plan.SourceRevision
            }, new Newtonsoft.Json.JsonSerializerSettings { Culture = System.Globalization.CultureInfo.InvariantCulture, DateFormatString = "o" }));
        }
        #endregion

        #region Cache Access with Single-Flight

        public async Task<float[]> GetOrComputeEmbeddingAsync(string text, string modelVersion, Func<Task<float[]>> computeFunc, string ns = "public")
        {
            string key = BuildEmbeddingKey(text, modelVersion, ns);
            if (EmbeddingCache.TryGet(key, out var cached))
            {
                return cached;
            }

            return await _singleFlight.ExecuteAsync(key, async () =>
            {
                if (EmbeddingCache.TryGet(key, out var existing))
                {
                    return existing;
                }

                var vec = await computeFunc();
                if (vec != null && vec.Length > 0)
                {
                    EmbeddingCache.Set(key, vec);
                }
                return vec;
            });
        }

        public async Task<RenderedResponse> GetOrExecuteResultAsync(
            string resultKey, 
            bool isSensitive, 
            Func<Task<RenderedResponse>> executeFunc, Func<bool> canPublish = null)
        {
            // C13: Dữ liệu nhạy cảm (Lương, Bảo hiểm, Thu nhập) không bao giờ lưu trong Result Cache
            if (isSensitive)
            {
                Interlocked.Increment(ref _resultBypasses);
                Interlocked.Increment(ref _resultLoads);
                return await executeFunc();
            }

            if (ResultCache.TryGet(resultKey, out var cached))
            {
                Interlocked.Increment(ref _resultHits);
                return cached;
            }

            Interlocked.Increment(ref _resultMisses);
            return await _singleFlight.ExecuteAsync(resultKey, async () =>
            {
                if (ResultCache.TryGet(resultKey, out var existing))
                {
                    return existing;
                }

                Interlocked.Increment(ref _resultLoads);
                var res = await executeFunc();
                // C12: Không cache các kết quả lỗi, timeout, hoặc bị từ chối
                if (res != null && (res.Status == "answered" || res.Status == "no_data") && (canPublish == null || canPublish()))
                {
                    ResultCache.Set(resultKey, res);
                    Interlocked.Increment(ref _resultPublished);
                }
                return res;
            });
        }

        #endregion
    }
}
