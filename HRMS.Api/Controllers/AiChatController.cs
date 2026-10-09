using Bu.Services.AI_Services;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Security;
using HRMS_API.Filters;
using HRMS_API.Services;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    /// <summary>
    /// API điều khiển Trợ lý AI HRMS Copilot V2.
    /// Toàn bộ thao tác truy vấn và quản lý hội thoại đều yêu cầu xác thực JWT hợp lệ (A01, C16).
    /// Loại bỏ hoàn toàn fallback regex và truy vấn CSDL không kiểm soát quyền.
    /// </summary>
    [JwtAuthorize]
    [RoutePrefix("api/ai")]
    public class AiChatController : ApiController
    {
        private readonly AiExecutionService _executionService;
        private readonly IAiReadinessProbe _readiness;
        private readonly Func<bool> _llmProbe;

        public AiChatController() : this(null)
        {
        }

        public AiChatController(AiExecutionService executionService) : this(executionService,null) { }

        public AiChatController(AiExecutionService executionService, IAiReadinessProbe readiness, Func<bool> llmProbe = null)
        {
            _executionService = executionService;
            _readiness = readiness ?? new OracleAiReadinessProbe();
            _llmProbe = llmProbe ?? IsOllamaOnline;
        }

        private AiExecutionService GetExecutionService()
        {
            return _executionService ?? AiServiceLocator.GetService<AiExecutionService>();
        }

        /// <summary>
        /// GET: api/ai
        /// Trả về trạng thái tổng quan của máy chủ AI (Tối thiểu hóa thông tin lộ lọt)
        /// </summary>
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public IHttpActionResult Index()
        {
            return GetStatus();
        }

        /// <summary>
        /// GET: api/ai/status
        /// Kiểm tra trạng thái máy chủ AI mà không để lộ IP nội bộ, connection string hay stack trace.
        /// </summary>
        [HttpGet]
        [Route("status")]
        [AllowAnonymous]
        [RateLimit(Policy = RateLimitPolicy.BusinessRead)]
        public IHttpActionResult GetStatus()
        {
            try
            {
                bool isOllama = _llmProbe();
                bool queryReady = _readiness.IsQueryReady();
                return Ok(new
                {
                    connected = true,
                    isOnline = true,
                    engine = "HRMS AI",
                    llmAvailable = isOllama,
                    queryReady = queryReady,
                    message = queryReady ? "Nguồn tra cứu AI đã sẵn sàng; kết quả phụ thuộc quyền được cấp." : "API đã kết nối, nhưng nguồn tra cứu AI chưa được thiết lập đầy đủ."
                });
            }
            catch
            {
                return Ok(new
                {
                    connected = false,
                    isOnline = false,
                    engine = "Error",
                    queryReady = false,
                    message = "Dịch vụ AI hiện không khả dụng."
                });
            }
        }

        /// <summary>
        /// GET: api/ai/chat
        /// Từ chối phương thức GET để ngăn rò rỉ nội dung câu hỏi nhạy cảm qua URL log máy chủ (C16).
        /// </summary>
        [HttpGet]
        [Route("chat")]
        public IHttpActionResult ChatGet()
        {
            return Content(HttpStatusCode.MethodNotAllowed, new
            {
                success = false,
                status = "error",
                message = "Phương thức GET không được hỗ trợ để đảm bảo an toàn thông tin truy vấn. Vui lòng gửi yêu cầu POST đến /api/ai/chat với Bearer Token."
            });
        }

        /// <summary>
        /// POST: api/ai/chat
        /// Gửi câu hỏi đến Trợ lý AI HRMS V2 (xác thực danh tính, phân quyền, hiểu câu hỏi và truy vấn dữ liệu an toàn).
        /// </summary>
        [HttpPost]
        [Route("chat")]
        [RateLimit(Policy = RateLimitPolicy.AiChat)]
        public async Task<IHttpActionResult> Chat([FromBody] ChatRequest request)
        {
            if (request == null || (string.IsNullOrWhiteSpace(request.Question) && string.IsNullOrWhiteSpace(request.OptionToken) && request.Clarification == null))
            {
                return BadRequest("Vui lòng cung cấp nội dung câu hỏi hoặc mã lựa chọn làm rõ.");
            }

            if ((request.Question?.Length ?? 0) > 2000 || (request.Clarification?.FreeTextAnswer?.Length ?? 0) > 2000 || (request.ConversationId?.Length ?? 0) > 100 || (request.ClientRequestId?.Length ?? 0) > 100) return BadRequest("Nội dung hoặc mã yêu cầu vượt giới hạn.");
            // 1. Trích xuất thông tin người dùng từ JWT Token hợp lệ
            var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (jwtUser == null || !int.TryParse(jwtUser.UserId, out int userId) || userId <= 0)
            {
                return Content(HttpStatusCode.Unauthorized, new AiChatResponseDto
                {
                    Status = "forbidden",
                    Answer = "Phiên làm việc không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại."
                });
            }

            // 2. Kiểm tra quyền truy cập tính năng AI (F_SYSTEM_AI)
            bool hasAiRight = jwtUser.IsAdmin ||
                             (jwtUser.Rights != null && (jwtUser.Rights.Contains("*") || jwtUser.Rights.Contains("F_SYSTEM_AI")));

            if (!hasAiRight)
            {
                return Content(HttpStatusCode.Forbidden, new AiChatResponseDto
                {
                    Status = "forbidden",
                    Answer = "Bạn không có quyền sử dụng tính năng Trợ lý AI (Mã quyền: F_SYSTEM_AI)."
                });
            }

            // Only the validated actor ID is passed; live identity, rights and policies are loaded by the service.
            var authContext = new AiAuthorizationContext { UserId = userId };
            // 4. Giải quyết conversationId và optionToken
            string conversationId = string.IsNullOrWhiteSpace(request.ConversationId)
                ? Guid.NewGuid().ToString("N")
                : request.ConversationId.Trim();

            string optionToken = !string.IsNullOrWhiteSpace(request.OptionToken)
                ? request.OptionToken
                : request.Clarification?.OptionToken;

            // R13 fix: Nếu client gửi FreeTextAnswer trong clarification DTO, sử dụng nó làm question
            string question = request.Question?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(question) && !string.IsNullOrWhiteSpace(request.Clarification?.FreeTextAnswer))
            {
                question = request.Clarification.FreeTextAnswer.Trim();
            }

            // 5. Khởi tạo CancellationToken với thời gian chờ tối đa 35 giây
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(35)))
            {
                try
                {
                    var executionService = GetExecutionService();
                    var execResult = await executionService.ProcessChatAsync(
                        question,
                        authContext,
                        conversationId: conversationId,
                        clientRequestId: request.ClientRequestId,
                        optionToken: optionToken,
                        cancellationToken: cts.Token,
                        expectedConversationVersion: request.ExpectedConversationVersion,
                        clarificationId: request.Clarification?.ClarificationId);

                    // 6. Chuyển đổi sang DTO phản hồi chuẩn hóa V2
                    var response = new AiChatResponseDto
                    {
                        Status = execResult.Status,
                        Answer = execResult.Answer,
                        ErrorCode = execResult.ErrorCode,
                        ConversationId = execResult.ConversationId,
                        ConversationVersion = execResult.ConversationVersion,
                        RequestId = execResult.ClientRequestId ?? Guid.NewGuid().ToString("N"),
                        Source = execResult.Provenance ?? (execResult.BypassedLlm ? "Deterministic_Engine" : "RAG_Engine"),
                        SqlQuery = string.Empty, // Không lộ cấu trúc SQL ra client
                        InterpretedRequest = execResult.InterpretedRequest != null ? new InterpretedRequestSummaryDto
                        {
                            Domain = execResult.InterpretedRequest.Domain,
                            Operation = execResult.InterpretedRequest.Operation,
                            Metric = execResult.InterpretedRequest.Metric,
                            SelectedEntityDisplay = execResult.InterpretedRequest.EntityDisplay,
                            RequestedScope = execResult.InterpretedRequest.RequestedScope,
                            EffectiveScope = execResult.InterpretedRequest.EffectiveScopeDisplay,
                            ResolvedPeriod = execResult.InterpretedRequest.EffectivePeriodDisplay,
                            Assumptions = execResult.InterpretedRequest.Assumptions ?? new List<string>()
                        } : null,
                        Clarification = execResult.Clarification != null ? new AiClarificationDto
                        {
                            ClarificationId = execResult.Clarification.ClarificationId,
                            TargetField = execResult.Clarification.Field,
                            Question = execResult.Clarification.Question,
                            Options = execResult.Clarification.Options?.Select(o => new AiClarificationOptionDto
                            {
                                OptionToken = o.Token,
                                Label = o.Label,
                                Value = o.Value
                            }).ToList() ?? new List<AiClarificationOptionDto>()
                        } : null,
                        ResultMetadata = new AiResultMetadataDto
                        {
                            Total = execResult.TotalRecords,
                            HasMore = execResult.HasMore,
                            AsOf = execResult.AsOf,
                            SourcePublicLabel = execResult.Provenance ?? "Cơ sở dữ liệu an toàn AI_READONLY"
                        },
                        Data = execResult.Data != null ? ConvertDataTableToDto(execResult.Data) : null
                    };

                    if (execResult.Status == "forbidden")
                    {
                        return Content(HttpStatusCode.Forbidden, response);
                    }

                    if (execResult.HttpStatus == 200) return Ok(response);
                    return Content((HttpStatusCode)execResult.HttpStatus,response);
                }
                catch (OperationCanceledException)
                {
                    return Content(HttpStatusCode.GatewayTimeout, new AiChatResponseDto
                    {
                        Status = "error",
                        Answer = "Yêu cầu xử lý đã quá thời gian cho phép (35 giây). Vui lòng thử lại với câu hỏi ngắn gọn hơn.",
                        ConversationId = conversationId,
                        Source = "Timeout_Handler"
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Lỗi trong AiChatController.Chat: " + ex.GetType().Name);
                    return Content(HttpStatusCode.InternalServerError, new AiChatResponseDto
                    {
                        Status = "error",
                        Answer = "Đã xảy ra sự cố nội bộ trong quá trình xử lý câu hỏi. Vui lòng liên hệ quản trị viên.",
                        ConversationId = conversationId,
                        Source = "Fallback_Error"
                    });
                }
            }
        }

        /// <summary>
        /// POST: api/ai/reset
        /// Đặt lại phiên trò chuyện AI của người dùng hiện tại (Yêu cầu xác thực, không cho phép ẩn danh C16).
        /// </summary>
        [HttpPost]
        [Route("reset")]
        public IHttpActionResult Reset([FromBody] ResetChatRequest request)
        {
            var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (jwtUser == null || !int.TryParse(jwtUser.UserId, out int userId) || userId <= 0)
            {
                return Unauthorized();
            }

            string conversationId = request?.ConversationId;
            GetExecutionService().Reset(userId, conversationId);

            return Ok(new
            {
                success = true,
                message = "Đã đặt lại phiên trò chuyện AI thành công.",
                conversationId = conversationId
            });
        }

        /// <summary>
        /// GET: api/ai/reset
        /// Từ chối phương thức GET để ngăn việc reset phiên trò chuyện ngoài ý muốn qua URL (C16).
        /// </summary>
        [HttpGet]
        [Route("reset")]
        public IHttpActionResult ResetGet()
        {
            return Content(HttpStatusCode.MethodNotAllowed, new
            {
                success = false,
                message = "Phương thức GET không được hỗ trợ. Vui lòng gửi yêu cầu POST đến /api/ai/reset."
            });
        }

        /// <summary>
        /// POST: api/ai/reconcile
        /// Chạy job đối soát toàn bộ nhân sự sang Qdrant Vector Service
        /// </summary>
        [HttpPost]
        [Route("reconcile")]
        [JwtAuthorize(RequireAdmin = true)]
        public async Task<IHttpActionResult> ReconcileVectors()
        {
            try
            {
                var result = await Bu.Services.AI_Services.Vector.QdrantOutboxManager.Instance.ReconcileAllEmployeesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi trong ReconcileVectors: " + ex.ToString());
                return InternalServerError();
            }
        }

        /// <summary>
        /// GET: api/ai/outbox-status
        /// Kiểm tra số lượng tin nhắn đang chờ đồng bộ sang Qdrant
        /// </summary>
        [HttpGet]
        [Route("outbox-status")]
        [JwtAuthorize(RequireAdmin = true)]
        public IHttpActionResult GetOutboxStatus()
        {
            try
            {
                int pending = Bu.Services.AI_Services.Vector.QdrantOutboxManager.Instance.PendingCount;
                return Ok(new { pendingMessages = pending });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi trong GetOutboxStatus: " + ex.ToString());
                return InternalServerError();
            }
        }

        /// <summary>
        /// GET: api/ai/config
        /// Lấy thông tin cấu hình AI dùng chung (Yêu cầu quyền F_SYSTEM_AI_CONFIG hoặc Admin)
        /// </summary>
        [HttpGet]
        [Route("config")]
        public IHttpActionResult GetAiConfiguration()
        {
            var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (jwtUser == null || !int.TryParse(jwtUser.UserId, out int userId) || userId <= 0)
            {
                return Content(HttpStatusCode.Unauthorized, new { success = false, message = "Phiên làm việc không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại." });
            }

            string channel = Bu.CLASS_SECURITY.AppChannels.Normalize(jwtUser.ClientType) ?? Bu.CLASS_SECURITY.AppChannels.Web;
            bool hasConfigRight = jwtUser.IsAdmin || Bu.CLASS_SECURITY.PlatformAccessGuard.Current.CanExecute(
                userId, channel, "F_SYSTEM_AI_CONFIG", Bu.CLASS_SECURITY.ChannelAction.View, jwtUser.Username
            );
            if (!hasConfigRight)
            {
                return Content(HttpStatusCode.Forbidden, new { success = false, message = "Bạn không có quyền xem cấu hình AI (Yêu cầu quyền Xem F_SYSTEM_AI_CONFIG trên kênh hiện tại)." });
            }

            var snap = AiConfigurationCoordinator.Instance.CurrentSnapshot;
            return Ok(new
            {
                success = true,
                ollamaHost = snap.OllamaHost,
                aiModel = snap.AiModel,
                qdrantUrl = snap.QdrantUrl,
                aiTemp = snap.AiTemp,
                aiMaxTokens = snap.AiMaxTokens,
                aiCtx = snap.AiCtx,
                aiTopK = snap.AiTopK,
                aiTopP = snap.AiTopP,
                aiRepeat = snap.AiRepeat,
                version = snap.Version,
                updatedAt = snap.UpdatedAt
            });
        }

        /// <summary>
        /// POST: api/ai/config
        /// Cập nhật cấu hình AI dùng chung vào TB_CONFIG có version control và transaction (Yêu cầu F_SYSTEM_AI_CONFIG)
        /// </summary>
        [HttpPost]
        [Route("config")]
        public async Task<IHttpActionResult> UpdateAiConfiguration([FromBody] AiConfigUpdateDto update)
        {
            if (update == null) return BadRequest("Dữ liệu cấu hình không hợp lệ.");

            var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (jwtUser == null || !int.TryParse(jwtUser.UserId, out int userId) || userId <= 0)
            {
                return Content(HttpStatusCode.Unauthorized, new { success = false, message = "Phiên làm việc không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại." });
            }

            string channel = Bu.CLASS_SECURITY.AppChannels.Normalize(jwtUser.ClientType) ?? Bu.CLASS_SECURITY.AppChannels.Web;
            bool hasConfigRight = jwtUser.IsAdmin || Bu.CLASS_SECURITY.PlatformAccessGuard.Current.CanExecute(
                userId, channel, "F_SYSTEM_AI_CONFIG", Bu.CLASS_SECURITY.ChannelAction.Edit, jwtUser.Username
            );
            if (!hasConfigRight)
            {
                return Content(HttpStatusCode.Forbidden, new { success = false, message = "Bạn không có quyền quản trị cấu hình AI (Yêu cầu quyền Sửa F_SYSTEM_AI_CONFIG trên kênh hiện tại)." });
            }

            string correlationId = Request.Headers.Contains("X-Correlation-Id")
                ? Request.Headers.GetValues("X-Correlation-Id").FirstOrDefault()
                : Guid.NewGuid().ToString("N");

            var (success, msg, snapshot) = await AiConfigurationCoordinator.Instance.UpdateConfigurationAsync(update, userId, correlationId);
            if (!success)
            {
                if (msg != null && msg.Contains("Xung đột phiên bản"))
                {
                    return Content(HttpStatusCode.Conflict, new { success = false, message = msg, version = snapshot?.Version ?? 0 });
                }
                return Content(HttpStatusCode.BadRequest, new { success = false, message = msg });
            }

            return Ok(new
            {
                success = true,
                message = msg,
                snapshot = new
                {
                    ollamaHost = snapshot.OllamaHost,
                    aiModel = snapshot.AiModel,
                    qdrantUrl = snapshot.QdrantUrl,
                    aiTemp = snapshot.AiTemp,
                    aiMaxTokens = snapshot.AiMaxTokens,
                    aiCtx = snapshot.AiCtx,
                    aiTopK = snapshot.AiTopK,
                    aiTopP = snapshot.AiTopP,
                    aiRepeat = snapshot.AiRepeat,
                    version = snapshot.Version,
                    updatedAt = snapshot.UpdatedAt
                }
            });
        }

        public class TestEndpointRequestDto
        {
            public string Target { get; set; } // "OLLAMA" or "QDRANT"
            public string Url { get; set; }
            public string Model { get; set; }
        }

        /// <summary>
        /// POST: api/ai/config/test
        /// Kiểm tra kết nối an toàn đến máy chủ Ollama / Qdrant từ backend
        /// </summary>
        [HttpPost]
        [Route("config/test")]
        public async Task<IHttpActionResult> TestAiEndpoint([FromBody] TestEndpointRequestDto req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Target) || string.IsNullOrWhiteSpace(req.Url))
            {
                return BadRequest("Vui lòng cung cấp loại máy chủ (Target) và địa chỉ URL.");
            }

            var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (jwtUser == null || !int.TryParse(jwtUser.UserId, out int userId) || userId <= 0)
            {
                return Content(HttpStatusCode.Unauthorized, new { success = false, message = "Phiên làm việc không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại." });
            }

            string channel = Bu.CLASS_SECURITY.AppChannels.Normalize(jwtUser.ClientType) ?? Bu.CLASS_SECURITY.AppChannels.Web;
            bool hasConfigRight = jwtUser.IsAdmin || Bu.CLASS_SECURITY.PlatformAccessGuard.Current.CanExecute(
                userId, channel, "F_SYSTEM_AI_CONFIG", Bu.CLASS_SECURITY.ChannelAction.View, jwtUser.Username
            );
            if (!hasConfigRight)
            {
                return Content(HttpStatusCode.Forbidden, new { success = false, message = "Bạn không có quyền kiểm tra cấu hình AI (Yêu cầu quyền F_SYSTEM_AI_CONFIG trên kênh hiện tại)." });
            }

            var testResult = await AiConfigurationCoordinator.Instance.TestTargetEndpointAsync(req.Target, req.Url, req.Model);
            return Ok(testResult);
        }

        private static bool IsOllamaOnline()
        {
            try
            {
                string ollamaUrl = AiConfigurationCoordinator.Instance.CurrentSnapshot.OllamaHost ?? "http://localhost:11434";
                string host = "127.0.0.1";
                int port = 11434;

                try
                {
                    var uri = new Uri(ollamaUrl);
                    host = uri.Host;
                    port = uri.Port > 0 ? uri.Port : 11434;
                }
                catch { }

                using (var tcp = new TcpClient())
                {
                    var ar = tcp.BeginConnect(host, port, null, null);
                    bool success = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(600));
                    if (!success) return false;
                    tcp.EndConnect(ar);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// GET: api/ai/dashboard
        /// Trả về thống kê tổng hợp và danh sách nhân sự mẫu cho Desktop Dashboard (Typed Rows Contract)
        /// </summary>
        [HttpGet]
        [Route("dashboard")]
        public async Task<IHttpActionResult> GetDashboard()
        {
            var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            if (jwtUser == null || !int.TryParse(jwtUser.UserId, out int userId) || userId <= 0)
            {
                return Content(HttpStatusCode.Unauthorized, new { message = "Chưa xác thực danh tính." });
            }

            try
            {
                var policyProvider = new OracleAiPolicyProvider();
                var ctx = policyProvider.Load(userId);
                if (!ctx.HasFunctionRight("F_SYSTEM_AI"))
                {
                    return Content(HttpStatusCode.Forbidden, new { message = "Thiếu quyền F_SYSTEM_AI." });
                }

                var planner = new QueryPlanner();
                var executor = new ScopedSqlExecutor();

                var countPlan = planner.CreatePlan(new QueryUnderstandingResult { Domain = "EMPLOYEE", Operation = "COUNT", Metric = "HEADCOUNT" }, ctx);
                if (countPlan.Strategy == Bu.Services.AI_Services.Core.ExecutionStrategy.Forbidden)
                {
                    return Content(HttpStatusCode.Forbidden, new { status = "forbidden", message = countPlan.DenialOrUnsupportedReason ?? "Bạn chưa được cấp quyền xem dữ liệu nhân sự." });
                }

                var listPlan = planner.CreatePlan(new QueryUnderstandingResult { Domain = "EMPLOYEE", Operation = "LIST" }, ctx);

                var countResult = await executor.ExecutePlanAsync(countPlan, ctx);
                if (countResult.Status == Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.AuthorizationDenied)
                {
                    return Content(HttpStatusCode.Forbidden, new { status = "forbidden", message = "Truy vấn bị từ chối theo chính sách phân quyền dữ liệu AI." });
                }
                if (countResult.Status == Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.SourceUnavailable || countResult.Status == Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.ConnectionError)
                {
                    return Content(HttpStatusCode.ServiceUnavailable, new { status = "error", message = "Nguồn dữ liệu AI chưa sẵn sàng. Vui lòng liên hệ quản trị viên." });
                }

                var listResult = await executor.ExecutePlanAsync(listPlan, ctx);

                int? totalCount = null;
                string countStatus = "ok";
                if (countResult.Status == Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.SuccessWithData && countResult.Data?.Rows.Count > 0)
                {
                    totalCount = Convert.ToInt32(countResult.Data.Rows[0]["TOTAL_COUNT"]);
                }
                else if (countResult.Status == Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.SuccessEmpty)
                {
                    totalCount = 0;
                }
                else
                {
                    countStatus = countResult.Status.ToString();
                }

                var rows = new List<Dictionary<string, object>>();
                string listStatus = "ok";
                if (listPlan.Strategy == Bu.Services.AI_Services.Core.ExecutionStrategy.Forbidden || listResult.Status == Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.AuthorizationDenied)
                {
                    listStatus = "forbidden";
                }
                else if (listResult.Status == Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.SuccessWithData && listResult.Data != null)
                {
                    foreach (System.Data.DataRow r in listResult.Data.Rows)
                    {
                        var row = new Dictionary<string, object>();
                        foreach (System.Data.DataColumn c in listResult.Data.Columns)
                        {
                            row[c.ColumnName] = r[c] == DBNull.Value ? null : r[c];
                        }
                        rows.Add(row);
                    }
                }
                else if (listResult.Status != Bu.Services.AI_Services.Interfaces.SqlExecutionStatus.SuccessEmpty)
                {
                    listStatus = listResult.Status.ToString();
                }

                return Ok(new
                {
                    status = "ok",
                    countStatus = countStatus,
                    listStatus = listStatus,
                    totalEmployees = totalCount ?? 0,
                    hasCountData = totalCount.HasValue,
                    rows = rows,
                    columns = listResult.Data != null ? listResult.Data.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName).ToList() : new List<string>()
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi trong AiChatController.GetDashboard: " + ex);
                return Content(HttpStatusCode.InternalServerError, new
                {
                    status = "error",
                    message = "Đã xảy ra sự cố nội bộ trong quá trình tải dữ liệu bảng điều khiển AI."
                });
            }
        }

        private static AiTableDataDto ConvertDataTableToDto(DataTable dt)
        {
            if (dt == null) return null;
            var dto = new AiTableDataDto();
            foreach (DataColumn col in dt.Columns)
            {
                dto.Columns.Add(col.ColumnName);
            }
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                }
                dto.Rows.Add(dict);
            }
            return dto;
        }
    }

    #region DTO Contracts

    public class ChatRequest
    {
        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("lang")]
        public string Lang { get; set; }

        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }

        [JsonProperty("clientRequestId")]
        public string ClientRequestId { get; set; }

        [JsonProperty("expectedConversationVersion")]
        public long? ExpectedConversationVersion { get; set; }

        [JsonProperty("optionToken")]
        public string OptionToken { get; set; }

        [JsonProperty("clarification")]
        public ClarificationAnswerDto Clarification { get; set; }
    }

    public class ClarificationAnswerDto
    {
        [JsonProperty("clarificationId")]
        public string ClarificationId { get; set; }

        [JsonProperty("optionToken")]
        public string OptionToken { get; set; }

        [JsonProperty("freeTextAnswer")]
        public string FreeTextAnswer { get; set; }
    }

    public class ResetChatRequest
    {
        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }
    }

    public class AiChatResponseDto
    {
        [JsonProperty("status")]
        public string Status { get; set; } = "answered"; // answered / needs_clarification / forbidden / unsupported / no_data / error

        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("answer")]
        public string Answer { get; set; }

        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }

        [JsonProperty("conversationVersion")]
        public int ConversationVersion { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("clarification")]
        public AiClarificationDto Clarification { get; set; }

        [JsonProperty("interpretedRequest")]
        public InterpretedRequestSummaryDto InterpretedRequest { get; set; }

        [JsonProperty("resultMetadata")]
        public AiResultMetadataDto ResultMetadata { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("sqlQuery")]
        public string SqlQuery { get; set; } = string.Empty;

        [JsonProperty("data")]
        public AiTableDataDto Data { get; set; }
    }

    public class AiTableDataDto
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; } = new List<string>();

        [JsonProperty("rows")]
        public List<Dictionary<string, object>> Rows { get; set; } = new List<Dictionary<string, object>>();
    }

    public class AiClarificationDto
    {
        [JsonProperty("clarificationId")]
        public string ClarificationId { get; set; }

        [JsonProperty("targetField")]
        public string TargetField { get; set; }

        [JsonProperty("question")]
        public string Question { get; set; }

        [JsonProperty("options")]
        public List<AiClarificationOptionDto> Options { get; set; } = new List<AiClarificationOptionDto>();
    }

    public class AiClarificationOptionDto
    {
        [JsonProperty("optionToken")]
        public string OptionToken { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class InterpretedRequestSummaryDto
    {
        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("operation")]
        public string Operation { get; set; }

        [JsonProperty("metric")]
        public string Metric { get; set; }

        [JsonProperty("selectedEntityDisplay")]
        public string SelectedEntityDisplay { get; set; }

        [JsonProperty("requestedScope")]
        public string RequestedScope { get; set; }

        [JsonProperty("effectiveScope")]
        public string EffectiveScope { get; set; }

        [JsonProperty("resolvedPeriod")]
        public string ResolvedPeriod { get; set; }

        [JsonProperty("assumptions")]
        public List<string> Assumptions { get; set; } = new List<string>();
    }

    public class AiResultMetadataDto
    {
        [JsonProperty("total")]
        public int? Total { get; set; }

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("asOf")]
        public DateTime? AsOf { get; set; }

        [JsonProperty("sourcePublicLabel")]
        public string SourcePublicLabel { get; set; }
    }

    #endregion
}
