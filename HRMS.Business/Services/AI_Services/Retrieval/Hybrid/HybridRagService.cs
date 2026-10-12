using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Memory;

namespace Bu.Services.AI_Services
{
    /// <summary>
    /// Bộ điều phối RAG kết hợp (Hybrid RAG Orchestrator)
    /// Điều phối luồng xử lý giữa tiền xử lý, phân loại ý định, trích xuất dữ liệu đa nguồn (SQL + Vector), và tổng hợp câu trả lời qua LLM.
    /// </summary>
    [Obsolete("HybridRagService is a legacy entry point without zero-trust authorization context. Use AiExecutionService instead.", false)]
    public class HybridRagService
    {
        private readonly IRagContextRetriever _contextRetriever;
        private readonly IRagSynthesizer _synthesizer;
        private readonly AiRouterService _aiRouter;
        private readonly AiChatHistory _history = new AiChatHistory();

        public HybridRagService() { }

        public HybridRagService(IRagContextRetriever contextRetriever, IRagSynthesizer synthesizer, AiRouterService aiRouter)
        {
            _contextRetriever = contextRetriever;
            _synthesizer = synthesizer;
            _aiRouter = aiRouter;
        }

        [Obsolete("Ask without context is disabled. Use AiExecutionService.ExecuteAsync instead.", false)]
        public Task<QueryResult> Ask(string question, Action<string> onTokenReceived = null)
        {
            // This legacy entry point has no actor/policy context and must never query protected data.
            return Task.FromResult(new QueryResult { Status = "unsupported", Answer = "Vui lòng sử dụng luồng chat có xác thực và kiểm tra quyền.", SqlQuery = "", Data = null });
        }
        private void UpdateHistory(string q, string a)
        {
            _history.AddMessage("User", q);
            _history.AddMessage("AI", a);
        }

        public List<ChatMessage> GetMessages()
        {
            return _history.GetMessages();
        }

        public void ResetConversation()
        {
            _history.Clear();
        }
    }
}