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

        public async Task<string> ProcessQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return "Vui lòng nhập câu hỏi.";

            return await _rag.Ask(query);
        }
    }
}