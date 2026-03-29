using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bu.Services.AI_Services.Core
{
    public class OllamaService
    {
        private static readonly HttpClient _client = new HttpClient
        {
            //Timeout = TimeSpan.FromSeconds(50) // Tăng nhẹ timeout đề phòng model lớn
            Timeout = TimeSpan.FromMinutes(2) // Tăng timeout lên 2 phút để đảm bảo model lớn có đủ thời gian phản hồi
        };

        private const string URL = "http://localhost:11434/api/generate";
        private const string MODEL = "qwen2.5:latest";

        // ===== SQL MODE & ROUTER MODE =====
        // Dùng Temperature cực thấp (0.1) để AI không "sáng tạo" lung tung khi viết code
        public async Task<string> AskSql(string prompt)
        {
            Debug.WriteLine($">>> [OLLAMA] SQL PROMPT:\n{prompt}");

            return await Send(prompt,
                "Bạn là chuyên gia SQL Oracle. Chỉ trả về câu lệnh SELECT đúng, không giải thích, không dùng markdown.",
                0.1, 250);
        }

        // ===== CHAT MODE (RAG) =====
        // Dùng Temperature vừa phải (0.4 - 0.7) để câu trả lời tự nhiên, trôi chảy hơn
        public async Task<string> AskChat(string context, string question, string history)
        {
            string fullPrompt = $@"
[DỮ LIỆU HỆ THỐNG]
{context}

[LỊCH SỬ HỘI THOẠI]
{history}

[CÂU HỎI HIỆN TẠI]
{question}

[YÊU CẦU TRẢ LỜI]
- Nếu có dữ liệu hệ thống, hãy dùng nó để giải đáp chính xác.
- Nếu là câu chào hỏi, hãy phản hồi thân thiện với tư cách trợ lý nhân sự HRM.
- Không bịa đặt dữ liệu nếu không tìm thấy trong hệ thống.
- Trình bày câu trả lời rõ ràng bằng tiếng Việt.
";

            Debug.WriteLine($">>> [OLLAMA] CHAT PROMPT:\n{fullPrompt}");

            return await Send(fullPrompt, "Bạn là trợ lý nhân sự HRM chuyên nghiệp.", 0.4, 600);
        }

        // ===== HÀM GỬI REQUEST TỔNG QUÁT =====
        private async Task<string> Send(string prompt, string system, double temperature, int maxTokens)
        {
            try
            {
                var body = new
                {
                    model = MODEL,
                    prompt = prompt,
                    system = system,
                    stream = false,
                    options = new
                    {
                        temperature = temperature,
                        num_predict = maxTokens, // Giới hạn số lượng ký tự trả về
                        top_k = 40,
                        top_p = 0.9
                    }
                };

                var json = JsonConvert.SerializeObject(body);
                var contentString = new StringContent(json, Encoding.UTF8, "application/json");

                var res = await _client.PostAsync(URL, contentString);

                if (!res.IsSuccessStatusCode)
                {
                    Debug.WriteLine($">>> [OLLAMA ERROR]: HTTP {res.StatusCode}");
                    return "Lỗi kết nối dịch vụ AI.";
                }

                var content = await res.Content.ReadAsStringAsync();

                // Debug log raw content nếu cần kiểm tra lỗi parse
                // Debug.WriteLine($">>> [OLLAMA RAW]: {content}");

                dynamic obj = JsonConvert.DeserializeObject(content);
                string response = obj?.response?.ToString()?.Trim() ?? "AI không phản hồi.";

                Debug.WriteLine($">>> [OLLAMA RESPONSE]: {response}");
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [SYSTEM ERROR]: {ex.Message}");
                return "Hệ thống AI đang bận hoặc chưa khởi động.";
            }
        }
    }
}