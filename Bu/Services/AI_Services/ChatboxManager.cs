//using System.Threading.Tasks;
//using Bu.Services.AI_Servies;

//namespace Bu.Services.AI_Servies
//{
//    //public class ChatboxManager
//    //{
//    //    private readonly HybridRagService _rag = new HybridRagService();

//    //    public async Task<string> ProcessQuery(string query)
//    //    {
//    //        return await _rag.Ask(query);
//    //    }
//    //}
//    public class ChatboxManager
//    {
//        private readonly HybridRagService _rag;

//        public ChatboxManager()
//        {
//            _rag = new HybridRagService();
//        }

//        public async Task<string> ProcessQuery(string query)
//        {
//            if (string.IsNullOrWhiteSpace(query))
//                return "Vui lòng nhập câu hỏi.";

//            return await _rag.Ask(query);
//        }
//    }
//}
using System.Threading.Tasks;

namespace Bu.Services.AI_Services
{
    public class ChatboxManager
    {
        // Tối ưu: Singleton pattern đơn giản để giữ kết nối và dữ liệu Vector không bị reset
        private static HybridRagService _rag;

        public ChatboxManager()
        {
            if (_rag == null)
            {
                _rag = new HybridRagService();
            }
        }

        public async Task<string> ProcessQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return "Vui lòng nhập câu hỏi.";

            return await _rag.Ask(query);
        }
    }
}