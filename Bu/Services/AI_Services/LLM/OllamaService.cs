using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using Bu.Services.AI_Services.Interfaces;

namespace Bu.Services.AI_Services.Core
{
    public class OllamaService : ILlmService
    {
        private static readonly HttpClient _client = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2) // 2 minutes timeout for large local models
        };

        private string URL => new Bu.CLASS_CHAMCONG.SYS_CONFIG().getValue("OllamaHost", "http://localhost:11434") + "/api/generate";
        private string EMBEDDING_URL => new Bu.CLASS_CHAMCONG.SYS_CONFIG().getValue("OllamaHost", "http://localhost:11434") + "/api/embeddings";
        private string MODEL => new Bu.CLASS_CHAMCONG.SYS_CONFIG().getValue("AiModel", "qwen2.5:latest");
        private string EMBEDDING_MODEL => "bge-m3";
        private readonly IPromptManager _promptManager;

        public OllamaService(IPromptManager promptManager)
        {
            _promptManager = promptManager;
        }

        // ===== SQL MODE & ROUTER MODE =====
        public async Task<string> AskSql(string prompt)
        {
            Debug.WriteLine($">>> [OLLAMA] SQL PROMPT:\n{prompt}");

            return await Send(prompt,
                "Bạn là chuyên gia SQL Oracle. Chỉ trả về câu lệnh SELECT đúng, không giải thích, không dùng markdown.",
                0.1, 250);
        }

        public async Task<string> AskIntent(string prompt)
        {
            Debug.WriteLine($">>> [OLLAMA] INTENT PROMPT:\n{prompt}");

            // Cực kỳ tiết kiệm: temperature = 0.0, maxTokens = 10 vì chỉ cần 1 từ khóa
            return await Send(prompt,
                "Bạn là hệ thống phân loại câu hỏi (Router). Chỉ in ra đúng 1 từ khóa nhãn (Label), không giải thích.",
                0.0, 10);
        }

        // ===== CHAT MODE (RAG) =====
        public async Task<string> AskChat(string context, string question, string history, Action<string> onTokenReceived = null)
        {
            string template = _promptManager.GetPrompt("ChatPromptTemplate");
            if (string.IsNullOrEmpty(template))
            {
                // Fallback prompt template if config is missing
                template = "\n[DỮ LIỆU HỆ THỐNG]\n{Context}\n\n[LỊCH SỬ HỘI THOẠI]\n{History}\n\n[CÂU HỎI HIỆN TẠI]\n{Question}\n\n[YÊU CẦU TRẢ LỜI]\n- Nếu có dữ liệu hệ thống, hãy dùng nó để giải đáp chính xác.\n- Trả lời cực kỳ ngắn gọn, súc tích.\n";
            }

            string fullPrompt = template.Replace("{Context}", context)
                                        .Replace("{History}", history)
                                        .Replace("{Question}", question);

            Debug.WriteLine($">>> [OLLAMA] CHAT PROMPT:\n{fullPrompt}");

            double temp = 0.4;
            int maxTokens = 1000;
            var config = new Bu.CLASS_CHAMCONG.SYS_CONFIG();
            double.TryParse(config.getValue("AiTemp", "0.4"), out temp);
            int.TryParse(config.getValue("AiMaxTokens", "1000"), out maxTokens);

            return await Send(fullPrompt, "Bạn là trợ lý nhân sự HRM chuyên nghiệp.", temp, maxTokens, onTokenReceived);
        }

        public async Task<float[]> GetEmbedding(string text, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var body = new
                {
                    model = EMBEDDING_MODEL,
                    prompt = text
                };
                var json = JsonConvert.SerializeObject(body);
                var contentString = new StringContent(json, Encoding.UTF8, "application/json");

                // Call local Ollama embeddings endpoint with cancellation token
                Console.WriteLine("EMBEDDING URL: " + EMBEDDING_URL);
                var res = await _client.PostAsync(EMBEDDING_URL, contentString, cancellationToken);
                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    Console.WriteLine("RAW OLLAMA: " + content);
                    dynamic obj = JsonConvert.DeserializeObject(content);
                    var embedList = obj?.embedding?.ToObject<List<float>>();
                    if (embedList != null)
                    {
                        return embedList.ToArray();
                    }
                }
                else
                {
                    var err = await res.Content.ReadAsStringAsync();
                    Console.WriteLine($"[OLLAMA EMBEDDING ERROR STATUS]: {res.StatusCode} - {err}");
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException) && !(ex is TaskCanceledException))
                {
                    Console.WriteLine($">>> [OLLAMA EMBEDDING ERROR]: {ex.Message}");
                }
            }
            return null;
        }

        // ===== GENERAL SEND REQUEST =====
        private async Task<string> Send(string prompt, string system, double temperature, int maxTokens, Action<string> onTokenReceived = null)
        {
            try
            {
                var body = new
                {
                    model = MODEL,
                    prompt = prompt,
                    system = system,
                    stream = onTokenReceived != null,
                    options = new
                    {
                        temperature = temperature,
                        num_predict = maxTokens,
                        num_ctx = int.TryParse(new Bu.CLASS_CHAMCONG.SYS_CONFIG().getValue("AiCtx", "3072"), out int ctx) ? ctx : 3072,
                        top_k = int.TryParse(new Bu.CLASS_CHAMCONG.SYS_CONFIG().getValue("AiTopK", "30"), out int k) ? k : 30,
                        top_p = double.TryParse(new Bu.CLASS_CHAMCONG.SYS_CONFIG().getValue("AiTopP", "0.8"), out double p) ? p : 0.8,
                        repeat_penalty = double.TryParse(new Bu.CLASS_CHAMCONG.SYS_CONFIG().getValue("AiRepeat", "1.15"), out double rp) ? rp : 1.15
                    }
                };

                var json = JsonConvert.SerializeObject(body);
                var contentString = new StringContent(json, Encoding.UTF8, "application/json");

                if (onTokenReceived != null)
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, URL)
                    {
                        Content = contentString
                    };
                    using (var res = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (!res.IsSuccessStatusCode)
                        {
                            Debug.WriteLine($">>> [OLLAMA ERROR]: HTTP {res.StatusCode}");
                            return "Lỗi kết nối dịch vụ AI.";
                        }

                        using (var stream = await res.Content.ReadAsStreamAsync())
                        using (var reader = new System.IO.StreamReader(stream))
                        {
                            var fullResponse = new StringBuilder();
                            string line;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                if (string.IsNullOrWhiteSpace(line)) continue;

                                try
                                {
                                    dynamic obj = JsonConvert.DeserializeObject(line);
                                    string token = obj?.response?.ToString();
                                    if (!string.IsNullOrEmpty(token))
                                    {
                                        fullResponse.Append(token);
                                        onTokenReceived(token);
                                    }
                                }
                                catch (Exception parseEx)
                                {
                                    Debug.WriteLine($">>> [STREAM PARSE ERROR]: {parseEx.Message}");
                                }
                            }

                            string result = fullResponse.ToString().Trim();
                            Debug.WriteLine($">>> [OLLAMA RESPONSE]: {result}");
                            return result;
                        }
                    }
                }
                else
                {
                    var res = await _client.PostAsync(URL, contentString);

                    if (!res.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($">>> [OLLAMA ERROR]: HTTP {res.StatusCode}");
                        return "Lỗi kết nối dịch vụ AI.";
                    }

                    var content = await res.Content.ReadAsStringAsync();

                    dynamic obj = JsonConvert.DeserializeObject(content);
                    string response = obj?.response?.ToString()?.Trim() ?? "AI không phản hồi.";

                    Console.WriteLine($">>> [OLLAMA RESPONSE]: {response}");
                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> [SYSTEM ERROR]: {ex.Message}");
                return "Hệ thống AI đang bận hoặc chưa khởi động.";
            }
        }
    }
}