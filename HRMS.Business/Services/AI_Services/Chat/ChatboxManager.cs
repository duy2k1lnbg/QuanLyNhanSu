using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Bu.CLASS_SYSTEM;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Security;

namespace Bu.Services.AI_Services
{
    public delegate Task<AiChatExecutionResult> RemoteChatHandlerDelegate(
        string question,
        string conversationId,
        string optionToken,
        long? expectedConversationVersion,
        string clarificationId,
        CancellationToken cancellationToken
    );

    public class ChatboxManager
    {
        private string _conversationId;
        private readonly AiExecutionService _executionService;
        private readonly List<ChatMessage> _messages = new List<ChatMessage>();
        private readonly object _sync = new object();
        private ClarificationPrompt _pending;
        private int? _version;
        private int _generation;
        private int _actorId;
        private CancellationTokenSource _active;
        private readonly RemoteChatHandlerDelegate _remoteChatHandler;
        private readonly Func<string, CancellationToken, Task<bool>> _remoteResetHandler;

        public ChatboxManager(
            string conversationId = null,
            AiExecutionService executionService = null,
            RemoteChatHandlerDelegate remoteChatHandler = null,
            Func<string, CancellationToken, Task<bool>> remoteResetHandler = null)
        {
            _conversationId = string.IsNullOrWhiteSpace(conversationId) ? Guid.NewGuid().ToString("N") : conversationId;
            _remoteChatHandler = remoteChatHandler;
            _remoteResetHandler = remoteResetHandler;
            _executionService = executionService ?? (remoteChatHandler != null ? null : AiServiceLocator.GetService<AiExecutionService>());
        }

        public ChatboxManager(
            Func<string, string, string, CancellationToken, Task<AiChatExecutionResult>> remoteChatHandler,
            string conversationId = null,
            AiExecutionService executionService = null)
            : this(
                conversationId,
                executionService,
                remoteChatHandler != null ? (q, c, o, v, cl, ct) => remoteChatHandler(q, c, o, ct) : (RemoteChatHandlerDelegate)null,
                null)
        {
        }

        public ChatboxManager(
            string conversationId,
            AiExecutionService executionService,
            Func<string, string, string, CancellationToken, Task<AiChatExecutionResult>> remoteChatHandler)
            : this(remoteChatHandler, conversationId, executionService)
        {
        }

        public string ConversationId => _conversationId;
        public Task<QueryResult> ProcessQuery(string query, Action<string> onTokenReceived = null) => ProcessQueryAsync(query, null, onTokenReceived);

        public async Task<QueryResult> ProcessQueryAsync(string query, string optionToken = null, Action<string> onTokenReceived = null, CancellationToken cancellationToken = default)
        {
            var auth = GetCurrentDesktopAuthContext();
            if (!auth.IsAuthenticated && _remoteChatHandler == null) return new QueryResult { Status = "forbidden", Answer = "Vui lòng đăng nhập để sử dụng trợ lý AI." };
            int generation; ClarificationPrompt pending; int? version; CancellationTokenSource active;
            lock (_sync)
            {
                if (_actorId != 0 && _actorId != auth.UserId) { _active?.Cancel(); _messages.Clear(); _pending = null; _version = null; }
                _actorId = auth.UserId; generation = ++_generation; pending = _pending; version = _version;
                _active?.Cancel(); active = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken); active.CancelAfter(TimeSpan.FromSeconds(35)); _active = active;
            }
            try
            {
                var result = _remoteChatHandler != null
                    ? await _remoteChatHandler(query, _conversationId, optionToken, version, pending?.ClarificationId, active.Token)
                    : await _executionService.ProcessChatAsync(query, auth, _conversationId, Guid.NewGuid().ToString("N"), optionToken, active.Token, version, pending?.ClarificationId);
                lock (_sync)
                {
                    if (_generation != generation || active.IsCancellationRequested || GetCurrentDesktopAuthContext().UserId != auth.UserId)
                    {
                        return new QueryResult { Status = "error", Answer = "Kết quả cũ đã được bỏ qua vì phiên làm việc thay đổi." };
                    }
                    if (!string.IsNullOrWhiteSpace(result.ConversationId)) _conversationId = result.ConversationId;
                    if (result.ConversationVersion > 0) _version = result.ConversationVersion;
                    _pending = result.Status == "needs_clarification" ? result.Clarification : null;
                    if (result.Status != "error")
                    {
                        _messages.Add(new ChatMessage { Role = "User", Content = query });
                        _messages.Add(new ChatMessage { Role = "AI", Content = result.Answer });
                        if (_messages.Count > 20) _messages.RemoveRange(0, _messages.Count - 20);
                    }
                    onTokenReceived?.Invoke(result.Answer);
                    return new QueryResult
                    {
                        Answer = result.Answer,
                        SqlQuery = "",
                        Status = result.Status,
                        Clarification = result.Clarification,
                        ConversationVersion = result.ConversationVersion,
                        InterpretedRequest = result.InterpretedRequest,
                        Data = result.Data
                    };
                }
            }
            finally
            {
                lock (_sync) { if (ReferenceEquals(_active, active)) _active = null; }
                active.Dispose();
            }
        }

        public Task<QueryResult> ProcessClarificationAsync(string token, CancellationToken cancellationToken = default) => ProcessQueryAsync("", token, null, cancellationToken);
        public List<ChatMessage> GetMessages() { lock (_sync) return new List<ChatMessage>(_messages); }

        public void Reset()
        {
            string oldConversationId;
            int actorId;
            lock (_sync)
            {
                _generation++;
                _active?.Cancel();
                _messages.Clear();
                _pending = null;
                _version = null;
                oldConversationId = _conversationId;
                _conversationId = Guid.NewGuid().ToString("N");
                actorId = _actorId;
            }

            if (_executionService != null && actorId > 0 && !string.IsNullOrWhiteSpace(oldConversationId))
            {
                try { _executionService.Reset(actorId, oldConversationId); } catch { }
            }
            else if (_remoteResetHandler != null && !string.IsNullOrWhiteSpace(oldConversationId))
            {
                Task.Run(async () =>
                {
                    try { await _remoteResetHandler(oldConversationId, CancellationToken.None); } catch { }
                });
            }
        }

        public async Task ResetAsync(CancellationToken cancellationToken = default)
        {
            string oldConversationId;
            int actorId;
            lock (_sync)
            {
                _generation++;
                _active?.Cancel();
                _messages.Clear();
                _pending = null;
                _version = null;
                oldConversationId = _conversationId;
                _conversationId = Guid.NewGuid().ToString("N");
                actorId = _actorId;
            }

            if (_executionService != null && actorId > 0 && !string.IsNullOrWhiteSpace(oldConversationId))
            {
                try { _executionService.Reset(actorId, oldConversationId); } catch { }
            }
            else if (_remoteResetHandler != null && !string.IsNullOrWhiteSpace(oldConversationId))
            {
                try { await _remoteResetHandler(oldConversationId, cancellationToken); } catch { }
            }
        }

        private AiAuthorizationContext GetCurrentDesktopAuthContext()
        {
            if (UserSession.CurrentUser == null || UserSession.CurrentUser.IDUSER <= 0) return AiAuthorizationContext.CreateAnonymous();
            var user = UserSession.CurrentUser;
            return new AiAuthorizationContext 
            { 
                UserId = (int)user.IDUSER, 
                Username = user.USERNAME,
                FullName = user.FULLNAME,
                FunctionRights = new HashSet<string>(UserSession.UserRights ?? new List<string>(), StringComparer.OrdinalIgnoreCase) 
            };
        }
    }
}