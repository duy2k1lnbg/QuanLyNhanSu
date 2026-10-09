using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QLyNSu.Functions
{
    public class AiClientChatResult
    {
        public string Status { get; set; } = "answered";
        public string Answer { get; set; }
        public string ErrorCode { get; set; }
        public string ConversationId { get; set; }
        public int ConversationVersion { get; set; }
        public string RequestId { get; set; }
        public Bu.Services.AI_Services.Core.ClarificationPrompt Clarification { get; set; }
        public Bu.Services.AI_Services.Core.SafeInterpretedRequest InterpretedRequest { get; set; }
        public DataTable Data { get; set; }
    }

    public class AiClientDashboardResult
    {
        public string Status { get; set; } = "ok";
        public int TotalEmployees { get; set; }
        public bool HasCountData { get; set; }
        public string CountStatus { get; set; }
        public string ListStatus { get; set; }
        public string Message { get; set; }
        public DataTable Data { get; set; }
    }

    public class AiClientStatusResult
    {
        public bool Connected { get; set; }
        public bool IsOnline { get; set; }
        public bool LlmAvailable { get; set; }
        public bool QueryReady { get; set; }
        public string Message { get; set; }
    }

    public class AiApiClient
    {
        private static readonly Lazy<AiApiClient> _instance = new Lazy<AiApiClient>(() => new AiApiClient());
        public static AiApiClient Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        private string _bearerToken;
        private string _baseUrl;

        private class ConversationClientState
        {
            public int Version { get; set; }
            public string ClarificationId { get; set; }
        }

        private readonly Dictionary<string, ConversationClientState> _conversationStates = new Dictionary<string, ConversationClientState>(StringComparer.OrdinalIgnoreCase);
        private readonly object _stateLock = new object();

        public string BaseUrl => _baseUrl;
        public string BearerToken => _bearerToken ?? Bu.CLASS_SYSTEM.UserSession.CurrentToken;
        public bool HasToken => !string.IsNullOrEmpty(BearerToken);

        public AiApiClient()
        {
            _baseUrl = "http://localhost:55463";
            try
            {
                string configUrl = System.Configuration.ConfigurationManager.AppSettings["AiApiBaseUrl"];
                if (!string.IsNullOrWhiteSpace(configUrl))
                {
                    _baseUrl = configUrl.TrimEnd('/');
                }
            }
            catch { }

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(35)
            };

            try
            {
                Bu.CLASS_SYSTEM.UserSession.SessionCleared += ClearSession;
            }
            catch { }
        }

        public void SetBaseUrl(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                _baseUrl = url.TrimEnd('/');
            }
        }

        public void SetToken(string token)
        {
            _bearerToken = token;
            try
            {
                Bu.CLASS_SYSTEM.UserSession.CurrentToken = token;
            }
            catch { }
        }

        public void ClearSession()
        {
            _bearerToken = null;
            lock (_stateLock)
            {
                _conversationStates.Clear();
            }
        }

        public async Task<bool> ExchangeDesktopTokenAsync(string username, string password, string sessionId, string jti)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(jti))
            {
                return false;
            }

            try
            {
                var body = new
                {
                    username = username.Trim(),
                    password = password,
                    sessionId = sessionId.Trim(),
                    jti = jti.Trim()
                };

                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/auth/desktop-token", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    dynamic obj = JsonConvert.DeserializeObject(jsonStr);
                    string token = obj?.token?.ToString() ?? obj?.Token?.ToString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        SetToken(token);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AiApiClient ExchangeDesktopTokenAsync ERROR]: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            try
            {
                var body = new { username = username, password = password, clientType = "DESKTOP" };
                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/auth/desktop-login", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    dynamic obj = JsonConvert.DeserializeObject(jsonStr);
                    string token = obj?.token?.ToString() ?? obj?.Token?.ToString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        SetToken(token);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AiApiClient AuthenticateAsync ERROR]: {ex.Message}");
            }
            return false;
        }

        public async Task<AiClientChatResult> SendChatAsync(
            string question,
            string conversationId,
            string optionToken = null,
            long? expectedConversationVersion = null,
            string clarificationId = null,
            string freeTextAnswer = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                conversationId = Guid.NewGuid().ToString("N");
            }

            var result = new AiClientChatResult
            {
                ConversationId = conversationId
            };

            lock (_stateLock)
            {
                if (_conversationStates.TryGetValue(conversationId, out var state))
                {
                    if (!expectedConversationVersion.HasValue && state.Version > 0)
                    {
                        expectedConversationVersion = state.Version;
                    }
                    if (string.IsNullOrEmpty(clarificationId) && !string.IsNullOrEmpty(state.ClarificationId))
                    {
                        clarificationId = state.ClarificationId;
                    }
                }
            }

            try
            {
                object clarificationDto = null;
                if (!string.IsNullOrEmpty(clarificationId) || !string.IsNullOrEmpty(optionToken) || !string.IsNullOrEmpty(freeTextAnswer))
                {
                    clarificationDto = new
                    {
                        clarificationId = clarificationId,
                        optionToken = optionToken,
                        freeTextAnswer = !string.IsNullOrEmpty(optionToken) ? null : (!string.IsNullOrEmpty(freeTextAnswer) ? freeTextAnswer.Trim() : question?.Trim())
                    };
                }

                var requestBody = new
                {
                    question = !string.IsNullOrEmpty(optionToken) ? "" : question?.Trim(),
                    conversationId = conversationId,
                    clientRequestId = Guid.NewGuid().ToString("N"),
                    expectedConversationVersion = expectedConversationVersion,
                    optionToken = optionToken,
                    clarification = clarificationDto
                };

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/ai/chat")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
                };

                string effectiveToken = BearerToken;
                if (!string.IsNullOrEmpty(effectiveToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", effectiveToken);
                }

                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    linkedCts.CancelAfter(TimeSpan.FromSeconds(35));
                    var response = await _httpClient.SendAsync(request, linkedCts.Token);

                    var jsonStr = await response.Content.ReadAsStringAsync();
                    dynamic obj = null;
                    try
                    {
                        obj = JsonConvert.DeserializeObject(jsonStr);
                    }
                    catch { }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        result.Status = "error";
                        result.ErrorCode = "UNAUTHORIZED";
                        result.Answer = "Phiên làm việc đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.";
                        return result;
                    }

                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        result.Status = "forbidden";
                        result.ErrorCode = "FORBIDDEN";
                        result.Answer = obj?.answer?.ToString() ?? "Bạn không có quyền thực hiện tra cứu này.";
                        return result;
                    }

                    if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        result.Status = "error";
                        result.ErrorCode = "CONVERSATION_CONFLICT";
                        result.Answer = obj?.answer?.ToString() ?? "Phiên bản hội thoại không đồng bộ. Vui lòng gửi lại câu hỏi.";
                        if (obj?.conversationVersion != null)
                        {
                            result.ConversationVersion = (int)obj.conversationVersion;
                        }
                        return result;
                    }

                    result.Status = obj?.status?.ToString() ?? "error";
                    result.Answer = obj?.answer?.ToString() ?? "Không nhận được phản hồi từ máy chủ AI.";
                    result.ErrorCode = obj?.errorCode?.ToString();
                    result.RequestId = obj?.requestId?.ToString();

                    string returnedConvId = obj?.conversationId?.ToString();
                    if (!string.IsNullOrWhiteSpace(returnedConvId))
                    {
                        result.ConversationId = returnedConvId;
                    }

                    if (obj?.conversationVersion != null)
                    {
                        result.ConversationVersion = (int)obj.conversationVersion;
                    }

                    if (obj?.clarification != null)
                    {
                        string targetField = obj.clarification?.targetField?.ToString() ?? obj.clarification?.field?.ToString();
                        string clarId = obj.clarification?.clarificationId?.ToString();

                        result.Clarification = new Bu.Services.AI_Services.Core.ClarificationPrompt
                        {
                            ClarificationId = clarId,
                            Question = obj.clarification?.question?.ToString(),
                            Field = targetField
                        };

                        if (obj.clarification.options != null)
                        {
                            foreach (var opt in obj.clarification.options)
                            {
                                string optToken = opt?.optionToken?.ToString() ?? opt?.token?.ToString();
                                result.Clarification.Options.Add(new Bu.Services.AI_Services.Core.ClarificationOption
                                {
                                    Token = optToken,
                                    Label = opt?.label?.ToString(),
                                    Value = opt?.value?.ToString()
                                });
                            }
                        }
                    }

                    if (obj?.interpretedRequest != null)
                    {
                        result.InterpretedRequest = new Bu.Services.AI_Services.Core.SafeInterpretedRequest
                        {
                            Domain = obj.interpretedRequest?.domain?.ToString(),
                            Operation = obj.interpretedRequest?.operation?.ToString(),
                            Metric = obj.interpretedRequest?.metric?.ToString(),
                            EntityDisplay = obj.interpretedRequest?.selectedEntityDisplay?.ToString(),
                            RequestedScope = obj.interpretedRequest?.requestedScope?.ToString(),
                            EffectiveScopeDisplay = obj.interpretedRequest?.effectiveScope?.ToString(),
                            EffectivePeriodDisplay = obj.interpretedRequest?.resolvedPeriod?.ToString()
                        };
                    }

                    if (obj?.data != null && obj.data.rows != null)
                    {
                        var dt = new DataTable();
                        if (obj.data.columns != null)
                        {
                            foreach (var col in obj.data.columns) dt.Columns.Add(col.ToString());
                        }
                        foreach (var rowObj in obj.data.rows)
                        {
                            var row = dt.NewRow();
                            foreach (DataColumn col in dt.Columns)
                            {
                                row[col.ColumnName] = rowObj[col.ColumnName]?.ToObject<object>() ?? DBNull.Value;
                            }
                            dt.Rows.Add(row);
                        }
                        result.Data = dt;
                    }

                    lock (_stateLock)
                    {
                        string activeConv = result.ConversationId ?? conversationId;
                        if (!string.IsNullOrEmpty(activeConv))
                        {
                            _conversationStates[activeConv] = new ConversationClientState
                            {
                                Version = result.ConversationVersion,
                                ClarificationId = result.Status == "needs_clarification" ? result.Clarification?.ClarificationId : null
                            };
                        }
                    }

                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                result.Status = "error";
                result.ErrorCode = "TIMEOUT";
                result.Answer = "Yêu cầu đã quá thời gian chờ (35s) hoặc bị hủy.";
                return result;
            }
            catch (Exception)
            {
                result.Status = "error";
                result.ErrorCode = "NETWORK_ERROR";
                result.Answer = "Không thể kết nối đến máy chủ Trợ lý AI. Vui lòng kiểm tra lại kết nối mạng hoặc liên hệ quản trị viên.";
                return result;
            }
        }

        // Backward compatibility overload for existing 3/4-argument callers
        public Task<AiClientChatResult> SendChatAsync(string question, string conversationId, string optionToken, CancellationToken cancellationToken)
        {
            return SendChatAsync(question, conversationId, optionToken, null, null, null, cancellationToken);
        }

        public async Task<bool> ResetConversationAsync(string conversationId, CancellationToken cancellationToken = default)
        {
            lock (_stateLock)
            {
                if (!string.IsNullOrEmpty(conversationId))
                {
                    _conversationStates.Remove(conversationId);
                }
            }

            try
            {
                var body = new { conversationId = conversationId };
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/ai/reset")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                };

                string effectiveToken = BearerToken;
                if (!string.IsNullOrEmpty(effectiveToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", effectiveToken);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AiClientDashboardResult> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            var result = new AiClientDashboardResult();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/ai/dashboard");
                string effectiveToken = BearerToken;
                if (!string.IsNullOrEmpty(effectiveToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", effectiveToken);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    dynamic obj = JsonConvert.DeserializeObject(jsonStr);

                    result.Status = obj?.status?.ToString() ?? "ok";
                    result.TotalEmployees = (int)(obj?.totalEmployees ?? 0);
                    result.HasCountData = obj?.hasCountData == true;
                    result.CountStatus = obj?.countStatus?.ToString();
                    result.ListStatus = obj?.listStatus?.ToString();

                    var dt = new DataTable();
                    if (obj?.columns != null)
                    {
                        foreach (var col in obj.columns)
                        {
                            dt.Columns.Add(col.ToString());
                        }
                    }

                    if (obj?.rows != null)
                    {
                        foreach (var rowObj in obj.rows)
                        {
                            var row = dt.NewRow();
                            foreach (DataColumn col in dt.Columns)
                            {
                                row[col.ColumnName] = rowObj[col.ColumnName]?.ToObject<object>() ?? DBNull.Value;
                            }
                            dt.Rows.Add(row);
                        }
                    }
                    result.Data = dt;
                }
                else
                {
                    result.Status = "error";
                    result.Message = response.StatusCode == HttpStatusCode.Forbidden
                        ? "Bạn chưa được cấp quyền xem dữ liệu bảng điều khiển."
                        : "Không thể tải dữ liệu bảng điều khiển từ máy chủ AI.";
                }
            }
            catch (Exception ex)
            {
                result.Status = "error";
                result.Message = "Lỗi kết nối máy chủ bảng điều khiển.";
                System.Diagnostics.Debug.WriteLine($"[AiApiClient GetDashboardAsync ERROR]: {ex.Message}");
            }
            return result;
        }

        public async Task<AiClientStatusResult> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            var result = new AiClientStatusResult();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/ai/status");
                string effectiveToken = BearerToken;
                if (!string.IsNullOrEmpty(effectiveToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", effectiveToken);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    dynamic obj = JsonConvert.DeserializeObject(jsonStr);

                    result.Connected = true;
                    result.IsOnline = obj?.isOnline == true;
                    result.LlmAvailable = obj?.llmAvailable == true;
                    result.QueryReady = obj?.queryReady == true;
                    result.Message = obj?.message?.ToString() ?? "Máy chủ AI hoạt động bình thường.";
                }
            }
            catch
            {
                result.Connected = false;
                result.Message = "Không thể kết nối đến máy chủ AI.";
            }
            return result;
        }

        public async Task<AiClientConfigResult> GetAiConfigAsync(CancellationToken cancellationToken = default)
        {
            var result = new AiClientConfigResult();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/api/ai/config");
                string effectiveToken = BearerToken;
                if (!string.IsNullOrEmpty(effectiveToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", effectiveToken);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                result.StatusCode = (int)response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    dynamic obj = JsonConvert.DeserializeObject(jsonStr);

                    result.Success = true;
                    result.OllamaHost = obj?.ollamaHost?.ToString();
                    result.AiModel = obj?.aiModel?.ToString();
                    result.QdrantUrl = obj?.qdrantUrl?.ToString();
                    result.AiTemp = (double)(obj?.aiTemp ?? 0.4);
                    result.AiMaxTokens = (int)(obj?.aiMaxTokens ?? 1000);
                    result.AiCtx = (int)(obj?.aiCtx ?? 3072);
                    result.AiTopK = (int)(obj?.aiTopK ?? 30);
                    result.AiTopP = (double)(obj?.aiTopP ?? 0.8);
                    result.AiRepeat = (double)(obj?.aiRepeat ?? 1.15);
                    result.Version = (long)(obj?.version ?? 1);
                }
                else
                {
                    result.Success = false;
                    result.Message = response.StatusCode == HttpStatusCode.Forbidden
                        ? "Bạn không có quyền F_SYSTEM_AI_CONFIG để xem cấu hình AI."
                        : $"Máy chủ trả về mã lỗi: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                result.StatusCode = 0;
                result.Success = false;
                result.Message = "Không thể kết nối đến backend để lấy cấu hình AI: " + ex.Message;
            }
            return result;
        }

        public async Task<AiClientConfigResult> SaveAiConfigAsync(object payload, CancellationToken cancellationToken = default)
        {
            var result = new AiClientConfigResult();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/ai/config")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
                };
                string effectiveToken = BearerToken;
                if (!string.IsNullOrEmpty(effectiveToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", effectiveToken);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var jsonStr = await response.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(jsonStr);

                result.StatusCode = (int)response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    result.Success = true;
                    result.Message = obj?.message?.ToString() ?? "Đã lưu cấu hình AI thành công.";
                    if (obj?.snapshot != null)
                    {
                        result.Version = (long)(obj.snapshot.version ?? 1);
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = obj?.message?.ToString() ?? $"Lỗi khi lưu cấu hình AI ({response.StatusCode}).";
                }
            }
            catch (Exception ex)
            {
                result.StatusCode = 0;
                result.Success = false;
                result.Message = "Không thể kết nối đến backend để lưu cấu hình AI: " + ex.Message;
            }
            return result;
        }

        public async Task<AiClientConfigTestResult> TestAiConfigDraftAsync(string target, string url, string model = null, CancellationToken cancellationToken = default)
        {
            var result = new AiClientConfigTestResult();
            try
            {
                var body = new { Target = target, Url = url, Model = model };
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/ai/config/test")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                };
                string effectiveToken = BearerToken;
                if (!string.IsNullOrEmpty(effectiveToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", effectiveToken);
                }

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var jsonStr = await response.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(jsonStr);

                if (response.IsSuccessStatusCode)
                {
                    result.Success = obj?.success == true;
                    result.Message = obj?.message?.ToString();
                    result.StatusCode = (int)(obj?.statusCode ?? 200);
                    result.LatencyMs = (long)(obj?.latencyMs ?? 0);
                }
                else
                {
                    result.Success = false;
                    result.Message = obj?.message?.ToString() ?? $"Kiểm tra thất bại ({response.StatusCode}).";
                    result.StatusCode = (int)response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Không thể kết nối đến backend để kiểm tra máy chủ AI: " + ex.Message;
            }
            return result;
        }
    }

    public class AiClientConfigResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OllamaHost { get; set; }
        public string AiModel { get; set; }
        public string QdrantUrl { get; set; }
        public double AiTemp { get; set; }
        public int AiMaxTokens { get; set; }
        public int AiCtx { get; set; }
        public int AiTopK { get; set; }
        public double AiTopP { get; set; }
        public double AiRepeat { get; set; }
        public long Version { get; set; }
        public int StatusCode { get; set; }
    }

    public class AiClientConfigTestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public long LatencyMs { get; set; }
    }
}
