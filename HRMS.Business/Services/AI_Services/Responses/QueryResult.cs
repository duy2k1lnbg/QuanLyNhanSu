using System;
using System.Data;
using Bu.Services.AI_Services.Core;

namespace Bu.Services.AI_Services
{
    /// <summary>
    /// DTO kết quả truy vấn dùng cho ChatboxManager và UI Presentation Adapter.
    /// Đã được tách riêng khỏi HybridRagService để đảm bảo an toàn kiến trúc.
    /// </summary>
    public class QueryResult
    {
        public string Answer { get; set; }
        public string SqlQuery { get; set; }
        public DataTable Data { get; set; }
        public string Status { get; set; }
        public ClarificationPrompt Clarification { get; set; }
        public int ConversationVersion { get; set; }
        public SafeInterpretedRequest InterpretedRequest { get; set; }
    }
}
