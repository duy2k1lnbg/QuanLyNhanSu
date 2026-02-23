using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.Services
{
    public class ChatboxManager
    {
        #region
        private readonly AiRepository _repo = new AiRepository();
        private readonly OllamaService _service = new OllamaService();
        private List<ChatMessage> _history = new List<ChatMessage>();

        public ChatboxManager()
        {
            _history.Add(new ChatMessage
            {
                role = "system",
                content = "Bạn là trợ lý nhân sự thông minh của phần mềm QuanLyNhanSu. Trả lời ngắn gọn dựa trên dữ liệu Oracle được cung cấp. PHẢI trả lời bằng tiếng Việt."
            });
        }

        public async Task<string> ProcessQuery(string userText)
        {
            string context = DetectContextAndFetchData(userText);
            string finalPrompt = string.IsNullOrEmpty(context) ? userText : $"DỮ LIỆU: {context}\n\nCÂU HỎI: {userText}";

            _history.Add(new ChatMessage { role = "user", content = finalPrompt });

            string response = await _service.SendChatRequest(_history);

            // Dọn dẹp lịch sử: Xóa prompt kèm context, thay bằng câu hỏi sạch của user
            _history.RemoveAt(_history.Count - 1);
            _history.Add(new ChatMessage { role = "user", content = userText });
            _history.Add(new ChatMessage { role = "assistant", content = response });

            if (_history.Count > 12) _history.RemoveRange(1, 2); // Giữ System Prompt

            return response;
        }

        private string DetectContextAndFetchData(string query)
        {
            query = query.ToLower();
            if (query.Contains("nhân viên") || query.Contains("ai"))
                return JsonConvert.SerializeObject(_repo.GetEmployeeData(""));
            if (query.Contains("chấm công") || query.Contains("giờ"))
                return JsonConvert.SerializeObject(_repo.GetAttendanceData(null));
            if (query.Contains("bảo hiểm"))
                return JsonConvert.SerializeObject(_repo.GetInsuranceData(""));
            if (query.Contains("lương") || query.Contains("ứng"))
                return JsonConvert.SerializeObject(_repo.GetPayrollData(0));

            return "";
        }
        #endregion
    }
}
