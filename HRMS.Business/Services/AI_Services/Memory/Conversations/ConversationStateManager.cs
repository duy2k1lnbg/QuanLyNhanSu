using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Bu.Services.AI_Services.Core;

namespace Bu.Services.AI_Services.Memory
{
    public sealed class ConversationConflictException : Exception { }
    public class ConversationSession
    {
        internal readonly object Sync = new object();
        internal long RequestSequence;
        public int UserId { get; set; }
        public string ConversationId { get; set; }
        public int Version { get; set; } = 1;
        public long Generation { get; set; } = 1;
        public QueryUnderstandingResult LastResolvedRequest { get; set; }
        public ClarificationPrompt PendingClarification { get; set; }
        public string AuthorizationFingerprint { get; set; }
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public DateTime LastAccessed { get; set; }
        internal IClockProvider Clock = new SystemClockProvider();
        public void AddMessage(string role, string content)
        {
            lock (Sync) { Messages.Add(new ChatMessage { Role = role, Content = content }); if (Messages.Count > 20) Messages.RemoveAt(0); LastAccessed = Clock.UtcNow; Version++; }
        }
        public string GetHistoryString() { lock (Sync) return string.Join(Environment.NewLine, Messages.Select(m => m.Role + ": " + m.Content)); }
        public void Clear() { lock (Sync) { Messages.Clear(); LastResolvedRequest = null; PendingClarification = null; Version++; Generation++; RequestSequence++; LastAccessed = Clock.UtcNow; } }
    }
    public sealed class ConversationLease
    {
        internal ConversationSession Session;
        internal long Sequence;
        internal long Generation;
        public int Version { get; internal set; }
        public QueryUnderstandingResult Previous { get; internal set; }
        public ClarificationPrompt Pending { get; internal set; }
    }
    public class ConversationStateManager
    {
        private static readonly Lazy<ConversationStateManager> Lazy = new Lazy<ConversationStateManager>(() => new ConversationStateManager());
        public static ConversationStateManager Instance => Lazy.Value;
        private readonly Dictionary<string, ConversationSession> _sessions = new Dictionary<string, ConversationSession>(StringComparer.Ordinal);
        private readonly object _sync = new object();
        private readonly IClockProvider _clock;
        private readonly int _capacity;
        private readonly TimeSpan _ttl;
        public ConversationStateManager(IClockProvider clock = null, int capacity = 5000, TimeSpan? ttl = null)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            _clock = clock ?? new SystemClockProvider(); _capacity = capacity; _ttl = ttl ?? TimeSpan.FromMinutes(60);
        }
        private string Key(int userId, string id) => userId + ":" + (string.IsNullOrWhiteSpace(id) ? "default" : id.Trim());
        private void Remove(string key) { if (_sessions.TryGetValue(key, out var s)) { s.Clear(); _sessions.Remove(key); } }
        private void Prune() { foreach (var k in _sessions.Where(p => _clock.UtcNow - p.Value.LastAccessed >= _ttl).Select(p => p.Key).ToArray()) Remove(k); }
        public ConversationSession GetOrCreateSession(int userId, string id)
        {
            lock (_sync)
            {
                Prune(); string key = Key(userId, id);
                if (!_sessions.TryGetValue(key, out var s))
                {
                    if (_sessions.Count >= _capacity) Remove(_sessions.OrderBy(p => p.Value.LastAccessed).First().Key);
                    s = new ConversationSession { UserId = userId, ConversationId = id, LastAccessed = _clock.UtcNow, Clock = _clock };
                    _sessions.Add(key, s);
                }
                s.LastAccessed = _clock.UtcNow; return s;
            }
        }
        public bool TryGetSession(int userId, string id, out ConversationSession s) { lock (_sync) { Prune(); return _sessions.TryGetValue(Key(userId, id), out s); } }
        public ConversationLease Begin(int userId, string id, long? expectedVersion, string fingerprint)
        {
            lock (_sync)
            {
                var s = GetOrCreateSession(userId, id);
                lock (s.Sync)
                {
                    if (expectedVersion.HasValue && expectedVersion.Value != s.Version) throw new ConversationConflictException();
                    if (s.AuthorizationFingerprint != null && s.AuthorizationFingerprint != fingerprint) s.Clear();
                    s.AuthorizationFingerprint = fingerprint;
                    return new ConversationLease { Session = s, Sequence = ++s.RequestSequence, Generation = s.Generation,
                        Version = s.Version, Previous = Clone(s.LastResolvedRequest), Pending = Clone(s.PendingClarification) };
                }
            }
        }
        public bool Commit(ConversationLease lease, QueryUnderstandingResult understood, ClarificationPrompt prompt, string question, string answer, out int version, System.Threading.CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                Prune(); var s = lease.Session; version = s.Version;
                if (!_sessions.TryGetValue(Key(s.UserId, s.ConversationId), out var current) || !ReferenceEquals(current, s)) return false;
                lock (s.Sync)
                {
                    if (cancellationToken.IsCancellationRequested || s.Generation != lease.Generation || s.RequestSequence != lease.Sequence) return false;
                    s.LastResolvedRequest = Clone(understood); s.PendingClarification = Clone(prompt);
                    if (question != null) { s.Messages.Add(new ChatMessage { Role = "user", Content = question }); s.Messages.Add(new ChatMessage { Role = "assistant", Content = answer }); }
                    if (s.Messages.Count > 20) s.Messages.RemoveRange(0, s.Messages.Count - 20);
                    s.LastAccessed = _clock.UtcNow; version = ++s.Version; return true;
                }
            }
        }
        public bool IsCurrent(ConversationLease lease) { lock (_sync) { Prune(); var s = lease.Session; return _sessions.TryGetValue(Key(s.UserId,s.ConversationId),out var current) && ReferenceEquals(s,current) && s.Generation == lease.Generation && s.RequestSequence == lease.Sequence; } }
        public void ClearSession(int userId, string id) { lock (_sync) Remove(Key(userId, id)); }
        public void ClearAll() { lock (_sync) foreach (var k in _sessions.Keys.ToArray()) Remove(k); }
        public static T Clone<T>(T value) => value == null ? default(T) : JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
    }
}