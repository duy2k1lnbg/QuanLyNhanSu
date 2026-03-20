using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bu.Services.AI_Services
{
    public class OllamaService
    {
        private static readonly HttpClient _client = new HttpClient
        {
            //Timeout = TimeSpan.FromMinutes(2)
            Timeout = TimeSpan.FromSeconds(40)
        };

        private const string URL = "http://localhost:11434/api/generate";
        private const string MODEL = "qwen2.5:latest";

        // ===== SQL MODE =====
        public async Task<string> AskSql(string prompt)
        {
            return await Send(prompt,
                "Bạn là chuyên gia SQL Oracle. Chỉ trả về SELECT đúng.");
        }

        // ===== CHAT MODE =====
        public async Task<string> AskChat(string context, string question)
        {
            string fullPrompt = $@"
Dữ liệu:
{context}

Câu hỏi:
{question}

Trả lời:
- Nếu có dữ liệu → trả lời dựa trên dữ liệu
- Nếu là câu hỏi chung → trả lời bình thường
- Không bịa dữ liệu
";

            return await Send(fullPrompt, "Bạn là trợ lý nhân sự HRM.");
        }

        private async Task<string> Send(string prompt, string system)
        {
            var body = new
            {
                model = MODEL,
                prompt = prompt,
                system = system,
                stream = false,
                options = new { 
                    temperature = 0.1,
                    num_predict = 200
                }
            };

            var json = JsonConvert.SerializeObject(body);

            var res = await _client.PostAsync(
                URL,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var content = await res.Content.ReadAsStringAsync();

            dynamic obj = JsonConvert.DeserializeObject(content);
            return obj?.response?.ToString()?.Trim() ?? "AI lỗi";
        }
    }
}