using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
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
        public int HttpStatusCode { get; set; }
        public List<Bu.Services.AI_Services.Core.AiTraceEvent> Diagnostics { get; set; } = new List<Bu.Services.AI_Services.Core.AiTraceEvent>();
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
        private readonly HttpClient _chatHttpClient;
        private string _bearerToken;
        private string _baseUrl;
        private readonly SemaphoreSlim _authenticationGate = new SemaphoreSlim(1, 1);
        private SecureString _desktopPassword;
        private string _desktopUsername, _desktopSessionId, _desktopJti;
        private int _sessionGeneration;

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

        public AiApiClient() : this(null, null) { }

        public AiApiClient(HttpClient httpClient, HttpClient chatHttpClient = null)
        {
            _baseUrl = "https://tryhardagain.com/api";
            try
            {
                string configUrl = System.Configuration.ConfigurationManager.AppSettings["AiApiBaseUrl"];
                if (!string.IsNullOrWhiteSpace(configUrl))
                {
                    _baseUrl = configUrl.TrimEnd('/');
                }
            }
            catch { }

            _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(35) };
            _chatHttpClient = chatHttpClient ?? httpClient ?? new HttpClient
            { Timeout = TimeSpan.FromSeconds(Bu.Services.AI_Services.Core.AiConfigurationSnapshot.ClientTransportTimeoutSeconds) };

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

        // Keep the credential in protected memory only; never persist it to config or disk.
        public void SetDesktopCredentials(string username, string password, string sessionId, string jti)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(jti))
                throw new ArgumentException("Thiếu thông tin phiên Desktop.");
            var secure = new SecureString();
            foreach (char ch in password) secure.AppendChar(ch);
            secure.MakeReadOnly();
            lock (_stateLock)
            {
                _sessionGeneration++;
                _desktopPassword?.Dispose();
                _desktopPassword = secure;
                _desktopUsername = username;
                _desktopSessionId = sessionId;
                _desktopJti = jti;
                _bearerToken = null;
                Bu.CLASS_SYSTEM.UserSession.CurrentToken = null;
                _conversationStates.Clear();
            }
        }

        public void ClearSession()
        {
            lock (_stateLock)
            {
                _sessionGeneration++;
                _bearerToken = null;
                Bu.CLASS_SYSTEM.UserSession.CurrentToken = null;
                _desktopPassword?.Dispose();
                _desktopPassword = null;
                _desktopUsername = _desktopSessionId = _desktopJti = null;
                _conversationStates.Clear();
            }
        }

        public async Task<bool> EnsureAuthenticatedAsync(CancellationToken cancellationToken = default)
        {
            await _authenticationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                SecureString secure;
                string username, sessionId, jti;
                int generation;
                lock (_stateLock)
                {
                    if (HasToken) return true;
                    if (_desktopPassword == null || !Bu.CLASS_SYSTEM.UserSession.IsLoggedIn
                        || _desktopSessionId != Bu.CLASS_SYSTEM.UserSession.CurrentSessionId
                        || _desktopJti != Bu.CLASS_SYSTEM.UserSession.CurrentJti) return false;
                    secure = _desktopPassword.Copy();
                    username = _desktopUsername;
                    sessionId = _desktopSessionId;
                    jti = _desktopJti;
                    generation = _sessionGeneration;
                }
                using (secure)
                {
                    IntPtr pointer = Marshal.SecureStringToBSTR(secure);
                    try
                    {
                        // Exchange the existing Oracle session; do not create a second login/session.
                        string token = await RequestDesktopTokenAsync(username, Marshal.PtrToStringBSTR(pointer),
                            sessionId, jti, cancellationToken).ConfigureAwait(false);
                        lock (_stateLock)
                        {
                            if (generation != _sessionGeneration || !Bu.CLASS_SYSTEM.UserSession.IsLoggedIn
                                || sessionId != Bu.CLASS_SYSTEM.UserSession.CurrentSessionId
                                || jti != Bu.CLASS_SYSTEM.UserSession.CurrentJti || string.IsNullOrEmpty(token)) return false;
                            SetToken(token);
                            return true;
                        }
                    }
                    finally { Marshal.ZeroFreeBSTR(pointer); }
                }
            }
            finally { _authenticationGate.Release(); }
        }

        private async Task RequireAuthenticationAsync(CancellationToken cancellationToken)
        {
            if (!await EnsureAuthenticatedAsync(cancellationToken).ConfigureAwait(false))
                throw new InvalidOperationException("Không thể xác thực phiên AI. Kiểm tra Backend AI hoặc đăng nhập lại. Các chức năng Oracle vẫn sử dụng độc lập.");
        }

        public async Task<bool> ExchangeDesktopTokenAsync(string username, string password, string sessionId, string jti)
        {
            int generation;
            lock (_stateLock) generation = _sessionGeneration;
            string token = await RequestDesktopTokenAsync(username, password, sessionId, jti, CancellationToken.None).ConfigureAwait(false);
            lock (_stateLock)
            {
                if (generation != _sessionGeneration || string.IsNullOrEmpty(token)) return false;
                SetToken(token);
                return true;
            }
        }

        private async Task<string> RequestDesktopTokenAsync(string username, string password, string sessionId, string jti,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(jti)) return null;
            try
            {
                var body = new { username = username.Trim(), password, sessionId = sessionId.Trim(), jti = jti.Trim() };
                using (var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"))
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(15));
                    using (var response = await _httpClient.PostAsync($"{_baseUrl}/api/auth/desktop-token", content, cts.Token).ConfigureAwait(false))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            AiLogger.Warn("AUTH", $"Trao đổi phiên AI thất bại: HTTP {(int)response.StatusCode}");
                            return null;
                        }
                        var obj = Newtonsoft.Json.Linq.JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                        return (obj["token"] ?? obj["Token"])?.ToString();
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                AiLogger.Error("AUTH", "Không thể kết nối hoặc trao đổi phiên AI.", ex);
                return null;
            }
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            AiLogger.Info("AUTH", $"Đang xác thực đăng nhập trực tiếp API: User='{username}', Endpoint='{_baseUrl}/api/auth/desktop-login'");
            try
            {
                var body = new { username = username, password = password, clientType = "DESKTOP" };
                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                {
                    var response = await _httpClient.PostAsync($"{_baseUrl}/api/auth/desktop-login", content, cts.Token).ConfigureAwait(false);
                    var jsonStr = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic obj = Newtonsoft.Json.Linq.JObject.Parse(jsonStr);
                        string token = obj?.token?.ToString() ?? obj?.Token?.ToString();
                        if (!string.IsNullOrEmpty(token))
                        {
                            SetToken(token);
                            AiLogger.Info("AUTH", $"Đăng nhập trực tiếp API thành công cho User='{username}'");
                            return true;
                        }
                        else
                        {
                            AiLogger.Warn("AUTH", "API desktop-login trả về 200 OK nhưng không có token", jsonStr);
                        }
                    }
                    else
                    {
                        AiLogger.Warn("AUTH", $"Đăng nhập trực tiếp API thất bại: HTTP {(int)response.StatusCode} ({response.ReasonPhrase})", jsonStr);
                    }
                }
            }
            catch (Exception ex)
            {
                AiLogger.Error("AUTH", $"Lỗi ngoại lệ khi gọi AuthenticateAsync: {ex.Message}", ex);
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
                ConversationId = conversationId,
                Status = "error",
                RequestId = Guid.NewGuid().ToString("N")
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
                await RequireAuthenticationAsync(cancellationToken);
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
                    clientRequestId = result.RequestId,
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

                string tokenInfo = string.IsNullOrEmpty(effectiveToken) ? "KHÔNG CÓ (Anonymous)" : $"Có (dài {effectiveToken.Length} chars)";
                AiLogger.Info("AI_CLIENT", $"[HTTP POST] /api/ai/chat | QuestionChars={question?.Length ?? 0} | Authenticated={!string.IsNullOrEmpty(effectiveToken)} | RequestId={result.RequestId}");

                var sw = System.Diagnostics.Stopwatch.StartNew();
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    linkedCts.CancelAfter(TimeSpan.FromSeconds(Bu.Services.AI_Services.Core.AiConfigurationSnapshot.ClientTransportTimeoutSeconds));
                    var response = await _chatHttpClient.SendAsync(request, linkedCts.Token);
                    sw.Stop();

                    var jsonStr = await response.Content.ReadAsStringAsync();
                    dynamic obj = null;
                    try
                    {
                        obj = Newtonsoft.Json.Linq.JObject.Parse(jsonStr);
                    }
                    catch { }

                    result.HttpStatusCode = (int)response.StatusCode;
                    var diagnosticArray = (obj as Newtonsoft.Json.Linq.JObject)?["diagnostics"] as Newtonsoft.Json.Linq.JArray;
                    if (diagnosticArray != null) {
                        result.Diagnostics = diagnosticArray.ToObject<List<Bu.Services.AI_Services.Core.AiTraceEvent>>() ?? new List<Bu.Services.AI_Services.Core.AiTraceEvent>();
                        foreach (var diagnostic in result.Diagnostics)
                            AiLogger.Info("AI_PIPELINE", $"RequestId={result.RequestId} ServerTime={diagnostic.TimestampUtc:O} [{diagnostic.Stage}] {diagnostic.Message}", diagnostic.Details);
                    }
                    AiLogger.Info("AI_CLIENT", $"[HTTP RESPONSE] Mã {(int)response.StatusCode} ({response.StatusCode}) sau {sw.ElapsedMilliseconds}ms | Độ dài Body: {jsonStr?.Length ?? 0}");

                    if (!response.IsSuccessStatusCode || obj == null)
                    {
                        AiLogger.Warn("AI_CLIENT", $"[HTTP WARN] Backend trả về mã lỗi {(int)response.StatusCode} | RequestId={result.RequestId}");
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        result.Status = "error";
                        result.ErrorCode = "UNAUTHORIZED";
                        result.Answer = "Phiên làm việc đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.";
                        AiLogger.Warn("AI_CLIENT", "Từ chối xác thực 401 Unauthorized. Cần đăng nhập lại để nhận Token mới.");
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

                    if (!response.IsSuccessStatusCode)
                    {
                        result.Status = "error";
                        result.ErrorCode = obj?.errorCode?.ToString() ?? "HTTP_" + result.HttpStatusCode;
                        result.Answer = obj?.answer?.ToString();
                        if (string.IsNullOrWhiteSpace(result.Answer))
                        {
                            result.Answer = DescribeChatHttpError(response.StatusCode);
                        }
                        result.RequestId = obj?.requestId?.ToString() ?? result.RequestId;
                        return result;
                    }

                    if (obj == null)
                    {
                        result.Status = "error";
                        result.ErrorCode = "INVALID_AI_RESPONSE";
                        result.Answer = "Máy chủ AI trả về dữ liệu không đúng định dạng (HTTP " + result.HttpStatusCode + "). Vui lòng liên hệ quản trị viên.";
                        return result;
                    }

                    string answer = obj?.answer?.ToString() ?? obj?.Answer?.ToString();
                    if (string.IsNullOrWhiteSpace(answer))
                    {
                        result.Status = "error";
                        result.ErrorCode = "EMPTY_AI_ANSWER";
                        result.Answer = "Phản hồi từ máy chủ AI thiếu nội dung trả lời. Vui lòng thử lại hoặc liên hệ quản trị viên.";
                        AiLogger.Warn("AI_CLIENT", $"[EMPTY ANSWER] RequestId={result.RequestId}, HttpStatus={result.HttpStatusCode}");
                        return result;
                    }
                    result.Status = obj?.status?.ToString() ?? obj?.Status?.ToString() ?? "error";
                    result.Answer = answer;
                    result.ErrorCode = obj?.errorCode?.ToString() ?? obj?.ErrorCode?.ToString();
                    result.RequestId = obj?.requestId?.ToString() ?? obj?.RequestId?.ToString() ?? result.RequestId;

                    AiLogger.Info("AI_CLIENT", $"[ANSWER OK] Status={result.Status}, AnswerChars={answer.Length}, RequestId={result.RequestId}");

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
            catch (OperationCanceledException ex)
            {
                result.Status = "error";
                result.ErrorCode = "TIMEOUT";
                result.Answer = "Yêu cầu đã quá thời gian chờ hoặc bị hủy.";
                AiLogger.Warn("AI_CLIENT", "Yêu cầu gọi AI đã quá thời gian chờ hoặc bị hủy", ex.Message);
                return result;
            }
            catch (Exception ex)
            {
                result.Status = "error";
                result.ErrorCode = "NETWORK_ERROR";
                result.Answer = $"Không thể kết nối đến máy chủ Trợ lý AI ({ex.Message}). Vui lòng kiểm tra lại dịch vụ Backend hoặc bấm 'Nhật ký AI' để xem chi tiết.";
                AiLogger.Error("AI_CLIENT", $"Lỗi ngoại lệ khi kết nối tới máy chủ AI ({_baseUrl}/api/ai/chat): {ex.Message}", ex);
                return result;
            }
        }

        private static string DescribeChatHttpError(HttpStatusCode statusCode)
        {
            switch ((int)statusCode)
            {
                case 400:
                    return "Máy chủ AI từ chối nội dung yêu cầu (HTTP 400). Vui lòng gửi lại câu hỏi.";
                case 404:
                    return "Không tìm thấy API Trợ lý AI (HTTP 404). Vui lòng kiểm tra địa chỉ máy chủ.";
                case 429:
                    return "Có quá nhiều yêu cầu tới máy chủ AI (HTTP 429). Vui lòng chờ rồi thử lại.";
                case 502:
                case 503:
                    return "Máy chủ Trợ lý AI tạm thời không sẵn sàng (HTTP " + (int)statusCode + "). Vui lòng thử lại sau.";
                case 504:
                    return "Máy chủ AI xử lý quá thời gian (HTTP 504). Vui lòng thử lại.";
                default:
                    return "Máy chủ AI trả về lỗi HTTP " + (int)statusCode + ". Vui lòng thử lại hoặc liên hệ quản trị viên.";
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
                await RequireAuthenticationAsync(cancellationToken);
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
                await RequireAuthenticationAsync(cancellationToken);
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
                    dynamic obj = Newtonsoft.Json.Linq.JObject.Parse(jsonStr);

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
                await RequireAuthenticationAsync(cancellationToken);
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
                    dynamic obj = Newtonsoft.Json.Linq.JObject.Parse(jsonStr);

                    result.Connected = true;
                    result.IsOnline = obj?.isOnline == true;
                    result.LlmAvailable = obj?.llmAvailable == true;
                    result.QueryReady = obj?.queryReady == true;
                    result.Message = obj?.message?.ToString() ?? "Máy chủ AI hoạt động bình thường.";
                    AiLogger.Info("AI_PROBE", $"Kiểm tra Backend AI ({_baseUrl}/api/ai/status): Connected=True, LLM={result.LlmAvailable}, QueryReady={result.QueryReady}");
                }
                else
                {
                    result.Connected = false;
                    result.Message = $"Backend AI trả về mã lỗi HTTP {(int)response.StatusCode}";
                    AiLogger.Warn("AI_PROBE", $"Probe Backend AI thất bại: HTTP {(int)response.StatusCode} ({response.ReasonPhrase})");
                }
            }
            catch (Exception ex)
            {
                result.Connected = false;
                result.Message = "Không thể kết nối đến máy chủ AI: " + ex.Message;
                AiLogger.Error("AI_PROBE", $"Lỗi ngoại lệ khi probe Backend AI ({_baseUrl}/api/ai/status): {ex.Message}", ex);
            }
            return result;
        }

        public async Task<AiClientConfigResult> GetAiConfigAsync(CancellationToken cancellationToken = default)
        {
            var result = new AiClientConfigResult();
            try
            {
                await RequireAuthenticationAsync(cancellationToken);
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
                    dynamic obj = Newtonsoft.Json.Linq.JObject.Parse(jsonStr);

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
                    result.AiRequestTimeoutSeconds = Bu.Services.AI_Services.Core.AiConfigurationSnapshot.NormalizeRequestTimeout((int?)(obj?.aiRequestTimeoutSeconds));
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
                await RequireAuthenticationAsync(cancellationToken);
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
                dynamic obj = Newtonsoft.Json.Linq.JObject.Parse(jsonStr);

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
                await RequireAuthenticationAsync(cancellationToken);
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
                dynamic obj = Newtonsoft.Json.Linq.JObject.Parse(jsonStr);

                if (response.IsSuccessStatusCode)
                {
                    result.Success = obj?.success == true || obj?.Success == true;
                    result.Message = obj?.message?.ToString() ?? obj?.Message?.ToString();
                    result.StatusCode = (int)(obj?.statusCode ?? obj?.StatusCode ?? 200);
                    result.LatencyMs = (long)(obj?.latencyMs ?? obj?.LatencyMs ?? 0);
                }
                else
                {
                    result.Success = false;
                    result.Message = obj?.message?.ToString() ?? obj?.Message?.ToString() ?? $"Kiểm tra thất bại ({response.StatusCode}).";
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
        public int AiRequestTimeoutSeconds { get; set; } = Bu.Services.AI_Services.Core.AiConfigurationSnapshot.DefaultRequestTimeoutSeconds;
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
