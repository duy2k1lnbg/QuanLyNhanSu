using System.Collections.Generic;

namespace Bu.Services.AI_Services.Memory
{
    public class AiCacheService
    {
        private static readonly Dictionary<string, string> _storage = new Dictionary<string, string>();
        private static readonly object _lock = new object();

        public string Get(string key)
        {
            lock (_lock)
            {
                return _storage.ContainsKey(key) ? _storage[key] : null;
            }
        }

        public void Set(string key, string value)
        {
            lock (_lock)
            {
                _storage[key] = value;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _storage.Clear();
            }
        }
    }
}