using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bu.Services.AI_Servies
{
    public class OllamaService
    {
        private static readonly HttpClient _client = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2) // 🔥 tăng timeout
        };

        private const string URL = "http://localhost:11434/api/generate";
        private const string MODEL = "qwen2.5:latest";

        public async Task<string> Ask(string prompt)
        {
            try
            {
                var body = new
                {
                    model = MODEL,
                    prompt = prompt,
                    stream = false
                };

                var json = JsonConvert.SerializeObject(body);

                for (int i = 0; i < 2; i++) // 🔥 retry 2 lần
                {
                    var res = await _client.PostAsync(
                        URL,
                        new StringContent(json, Encoding.UTF8, "application/json")
                    );

                    var content = await res.Content.ReadAsStringAsync();

                    if (!res.IsSuccessStatusCode)
                    {
                        // retry nếu server lỗi
                        if ((int)res.StatusCode >= 500 && i == 0)
                            continue;

                        return FormatError(res.StatusCode.ToString(), content);
                    }

                    return ParseResponse(content);
                }

                return "AI không phản hồi (retry fail).";
            }
            catch (TaskCanceledException)
            {
                return "AI timeout (model quá nặng hoặc máy yếu).";
            }
            catch (Exception ex)
            {
                return "Lỗi gọi AI: " + ex.Message;
            }
        }

        // ================= PARSE =================
        private string ParseResponse(string content)
        {
            try
            {
                dynamic obj = JsonConvert.DeserializeObject(content);

                if (obj == null || obj.response == null)
                    return "AI trả về dữ liệu rỗng.";

                return obj.response.ToString().Trim();
            }
            catch
            {
                return "Lỗi parse JSON từ AI.";
            }
        }

        // ================= ERROR =================
        private string FormatError(string code, string content)
        {
            return "AI ERROR\nCode: " + code + "\nDetail: " + content;
        }
    }
}