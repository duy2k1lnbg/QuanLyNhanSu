using System.Threading.Tasks;

namespace Bu.Services.AI_Services
{
    public class ChatboxManager
    {
        private static HybridRagService _rag;

        public ChatboxManager()
        {
            if (_rag == null)
                _rag = new HybridRagService();
        }

        public async Task<QueryResult> ProcessQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new QueryResult
                {
                    Answer = "Vui lòng nhập câu hỏi.",
                    SqlQuery = "",
                    Data = null
                };
            }

            return await _rag.Ask(query);
        }

        public void Reset()
        {
            _rag?.ResetConversation();
        }
    }
}