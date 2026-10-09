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

        private string Host
        {
            get
            {
                var snap = AiConfigurationCoordinator.Instance.CurrentSnapshot;
                if (!string.IsNullOrWhiteSpace(snap?.OllamaHost)) return snap.OllamaHost;
                try
                {
                    string dbHost = _getConfiguration("OllamaHost", "").TrimEnd('/');
                    return string.IsNullOrWhiteSpace(dbHost) ? "http://127.0.0.1:11434" : dbHost;
                }
                catch
                {
                    return "http://127.0.0.1:11434";
                }
            }
        }

        private string URL => Host + "/api/generate";
        private string EMBEDDING_URL => Host + "/api/embeddings";
        private string MODEL
        {
            get
            {
                var snap = AiConfigurationCoordinator.Instance.CurrentSnapshot;
                if (!string.IsNullOrWhiteSpace(snap?.AiModel)) return snap.AiModel;
                try
                {
                    string dbModel = _getConfiguration("AiModel", "qwen2.5:latest");
                    return string.IsNullOrWhiteSpace(dbModel) ? "qwen2.5:latest" : dbModel;
                }
                catch
                {
                    return "qwen2.5:latest";
                }
            }
        }
        public const string DEFAULT_EMBEDDING_MODEL = "bge-m3";
        private string EMBEDDING_MODEL
        {
            get
            {
                try
                {
                    string configuredModel = _getConfiguration("Ollama:EmbeddingModel", DEFAULT_EMBEDDING_MODEL);
                    return string.IsNullOrWhiteSpace(configuredModel) ? DEFAULT_EMBEDDING_MODEL : configuredModel.Trim();
                }
                catch
                {
                    return DEFAULT_EMBEDDING_MODEL;
                }
            }
        }
        private readonly IPromptManager _promptManager;

        private readonly Func<string,string,string> _getConfiguration;

        public OllamaService(IPromptManager promptManager) : this(promptManager,null) { }

        // Configuration defaults to AppSettings to avoid opening any HR database connection.
        public OllamaService(IPromptManager promptManager, Func<string,string,string> configurationReader)
        {
            _promptManager = promptManager;
            _getConfiguration = configurationReader ?? ((name,fallback) => {
                try
                {
                    string val = System.Configuration.ConfigurationManager.AppSettings[name];
                    return string.IsNullOrWhiteSpace(val) ? fallback : val;
                }
                catch
                {
                    return fallback;
                }
            });
        }

        public async Task<string> AskStructuredJsonAsync(string prompt, string system, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var body = new
                {
                    model = MODEL,
                    prompt = prompt,
                    system = system,
                    format = "json",
                    stream = false,
                    options = new
                    {
                        temperature = 0.0,
                        num_predict = 400
                    }
                };
                var json = JsonConvert.SerializeObject(body);
                var contentString = new StringContent(json, Encoding.UTF8, "application/json");
                var res = await _client.PostAsync(URL, contentString, cancellationToken);
                if (!res.IsSuccessStatusCode) return null;
                var content = await res.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(content);
                return obj?.response?.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($">>> [OLLAMA JSON ERROR]: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GenerateGroundedAnswerAsync(string question, string factsEvidence, string history = null, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                string prompt = $"[DỮ LIỆU ĐÃ ĐƯỢC PHÊ DUYỆT TỪ HỆ THỐNG]\n{factsEvidence}\n\n[CÂU HỎI CỦA NGƯỜI DÙNG]\n{question}\n\n[YÊU CẦU NGHIÊM NGẶT]\n- Chỉ trả lời dựa trên dữ liệu hệ thống được cung cấp ở trên.\n- Tuyệt đối không tự suy diễn số liệu, họ tên hay ngày tháng ngoài dữ liệu trên.\n- Diễn đạt ngắn gọn, lịch sự, đúng trọng tâm bằng tiếng Việt.";
                string system = "Bạn là Trợ lý AI Quản trị Nhân sự HRMS. Bạn chỉ được trả lời đúng sự thật dựa trên bằng chứng dữ liệu được cung cấp.";
                var snap = AiConfigurationCoordinator.Instance.CurrentSnapshot;
                string resp = await Send(prompt, system, snap?.AiTemp ?? 0.2, 500, null, cancellationToken);
                if (string.IsNullOrWhiteSpace(resp) || resp.Contains("chưa khởi động") || resp.Contains("Lỗi")) return null;
                return resp;
            }
            catch
            {
                return null;
            }
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
            double.TryParse(_getConfiguration("AiTemp", "0.4"), out temp);
            int.TryParse(_getConfiguration("AiMaxTokens", "1000"), out maxTokens);

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
                    Debug.WriteLine("[OLLAMA] Embedding response received.");
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
        // ===== GENERAL SEND REQUEST =====
        private async Task<string> Send(string prompt, string system, double temperature, int maxTokens, Action<string> onTokenReceived = null, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                var snap = AiConfigurationCoordinator.Instance.CurrentSnapshot;
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
                        num_ctx = snap?.AiCtx ?? (int.TryParse(_getConfiguration("AiCtx", "3072"), out int ctx) ? ctx : 3072),
                        top_k = snap?.AiTopK ?? (int.TryParse(_getConfiguration("AiTopK", "30"), out int k) ? k : 30),
                        top_p = snap?.AiTopP ?? (double.TryParse(_getConfiguration("AiTopP", "0.8"), out double p) ? p : 0.8),
                        repeat_penalty = snap?.AiRepeat ?? (double.TryParse(_getConfiguration("AiRepeat", "1.15"), out double rp) ? rp : 1.15)
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
                    using (var res = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
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
                                cancellationToken.ThrowIfCancellationRequested();
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
                    var res = await _client.PostAsync(URL, contentString, cancellationToken);

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
            catch (OperationCanceledException)
            {
                // Timeout hoặc người dùng hủy: không in lỗi thô, ném ra để controller xử lý timeout chuẩn
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> [SYSTEM ERROR]: {ex.Message}");
                return "Hệ thống AI đang bận hoặc chưa khởi động.";
            }
        }
    }
}