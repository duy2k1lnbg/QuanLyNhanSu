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
    public class HybridRagService
    {
        private readonly IRagContextRetriever _contextRetriever;
        private readonly IRagSynthesizer _synthesizer;
        private readonly AiRouterService _aiRouter;
        private readonly AiChatHistory _history = new AiChatHistory();

        public HybridRagService()
        {
            _contextRetriever = AiServiceLocator.GetService<IRagContextRetriever>();
            _synthesizer = AiServiceLocator.GetService<IRagSynthesizer>();
            _aiRouter = AiServiceLocator.GetService<AiRouterService>();
        }

        public HybridRagService(IRagContextRetriever contextRetriever, IRagSynthesizer synthesizer, AiRouterService aiRouter)
        {
            _contextRetriever = contextRetriever;
            _synthesizer = synthesizer;
            _aiRouter = aiRouter;
        }

        public async Task<QueryResult> Ask(string question, Action<string> onTokenReceived = null)
        {
            // 0. Tiền xử lý câu hỏi: phục hồi dấu tiếng Việt và bổ sung gợi ý schema theo ý định
            question = QueryPreprocessor.Preprocess(question);

            var result = new QueryResult
            {
                Answer = "Xin lỗi, tôi gặp chút trục trặc khi kết nối dữ liệu. Bạn thử hỏi lại nhé!",
                SqlQuery = "",
                Data = null
            };

            try
            {
                // 1. Kiểm tra phản hồi nhanh (Fast Response / Cache) trước tiên để giảm tải AI
                var fastResponse = FastResponseService.GetFastResponse(question);
                if (!string.IsNullOrEmpty(fastResponse))
                {
                    result.Answer = fastResponse;
                    UpdateHistory(question, fastResponse);

                    if (onTokenReceived != null)
                    {
                        await Task.Delay(500); // Giảm trễ xuống 500ms
                        var words = fastResponse.Split(' ');
                        for (int i = 0; i < words.Length; i++)
                        {
                            onTokenReceived(words[i] + (i < words.Length - 1 ? " " : ""));
                            await Task.Delay(30);
                        }
                        onTokenReceived("");
                    }

                    return result;
                }

                string currentHistory = _history.GetHistoryString();

                // 2. Phân loại ý định người dùng
                string intent = await _aiRouter.DetectIntent(question);

                // 3. Trích xuất ngữ cảnh RAG (SQL có kiểm duyệt AST + Vector search tương đồng)
                var contextData = await _contextRetriever.RetrieveContextAsync(question, intent);
                result.SqlQuery = contextData.SqlQuery;
                result.Data = contextData.SqlDataTable;

                // 4. Tổng hợp câu trả lời từ LLM với streaming
                string finalResponse = await _synthesizer.SynthesizeAnswerAsync(
                    contextData.CombinedContext,
                    question,
                    currentHistory,
                    onTokenReceived);

                // 5. Cập nhật lịch sử hội thoại
                UpdateHistory(question, finalResponse);

                result.Answer = finalResponse;
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RAG ORCHESTRATOR ERROR]: {ex.Message}");
                return result;
            }
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

    public class QueryResult
    {
        public string Answer { get; set; }
        public string SqlQuery { get; set; }
        public DataTable Data { get; set; }
    }
}