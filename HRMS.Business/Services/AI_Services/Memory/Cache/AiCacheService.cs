using System;

namespace Bu.Services.AI_Services.Memory
{
    /// <summary>
    /// Bộ nhớ tạm tương thích ngược (Legacy Cache Adapter)
    /// Được chuyển đổi để sử dụng BoundedCacheStore có giới hạn dung lượng và TTL tuyệt đối.
    /// </summary>
    public class AiCacheService
    {
        private static readonly BoundedCacheStore<string> _boundedStore = new BoundedCacheStore<string>(1000, TimeSpan.FromHours(1));

        public string Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            return _boundedStore.TryGet(key, out var val) ? val : null;
        }

        public void Set(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            _boundedStore.Set(key, value);
        }

        public void Clear()
        {
            _boundedStore.InvalidateAll();
        }
    }
}