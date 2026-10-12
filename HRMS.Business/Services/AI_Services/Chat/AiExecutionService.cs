using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Security;
using Bu.Services.AI_Services.Vector;

namespace Bu.Services.AI_Services.Core
{
    public class SafeInterpretedRequest
    {
        public string Domain { get; set; }
        public string Operation { get; set; }
        public string Metric { get; set; }
        public string EntityDisplay { get; set; }
        public string RequestedScope { get; set; }
        public string EffectiveScopeDisplay { get; set; }
        public string EffectivePeriodDisplay { get; set; }
        public List<string> Assumptions { get; set; } = new List<string>();
    }
    public class AiChatExecutionResult
    {
        public string Status { get; set; }
        public string Answer { get; set; }
        public string ErrorCode { get; set; }
        public string ConversationId { get; set; }
        public int ConversationVersion { get; set; }
        public string ClientRequestId { get; set; }
        public ClarificationPrompt Clarification { get; set; }
        public SafeInterpretedRequest InterpretedRequest { get; set; }
        public string Provenance { get; set; }
        public int? TotalRecords { get; set; }
        public bool HasMore { get; set; }
        public bool BypassedLlm { get; set; } = true;
        public long ExecutionDurationMs { get; set; }
        public int HttpStatus { get; set; } = 200;
        public DateTime? AsOf { get; set; }
        public DataTable Data { get; set; }
        public List<AiTraceEvent> Diagnostics { get; set; } = new List<AiTraceEvent>();
    }
    public class AiExecutionService
    {
        private readonly IQueryUnderstandingService _understandingService;
        private readonly IQueryPlanner _planner;
        private readonly IScopedSqlExecutor _sqlExecutor;
        private readonly ConversationStateManager _conversationManager;
        private readonly AiCacheCoordinator _cacheCoordinator;
        private readonly ClarificationPolicy _clarificationPolicy;
        private readonly IClockProvider _clock;
        private readonly IAiPolicyProvider _policyProvider;
        private readonly ILlmService _llmService;
        private readonly Func<int> _requestTimeoutReader;
        private readonly Bu.Services.AI_Services.Interfaces.IVectorService _vectorService;

        public AiExecutionService(IQueryUnderstandingService understandingService = null, IQueryPlanner planner = null,
            IScopedSqlExecutor sqlExecutor = null, ConversationStateManager conversationManager = null,
            AiCacheCoordinator cacheCoordinator = null, ClarificationPolicy clarificationPolicy = null,
            IClockProvider clock = null, IAiPolicyProvider policyProvider = null, ILlmService llmService = null,
            Bu.Services.AI_Services.Interfaces.IVectorService vectorService = null, Func<int> requestTimeoutReader = null)
        {
            _clock = clock ?? new SystemClockProvider(); _understandingService = understandingService ?? new QueryUnderstandingService(clock:_clock);
            _planner = planner ?? new QueryPlanner(_clock); _sqlExecutor = sqlExecutor ?? new ScopedSqlExecutor();
            _conversationManager = conversationManager ?? ConversationStateManager.Instance; _cacheCoordinator = cacheCoordinator ?? AiCacheCoordinator.Instance;
            _clarificationPolicy = clarificationPolicy ?? new ClarificationPolicy();
            // Production always reloads policy. Explicit injected test executors may use fixture contexts.
            _policyProvider = policyProvider ?? (_sqlExecutor is ScopedSqlExecutor ? new OracleAiPolicyProvider() : null);
            _llmService = llmService ?? (_sqlExecutor is ScopedSqlExecutor ? AiServiceLocator.GetService<ILlmService>() : null);
            _vectorService = vectorService ?? (_sqlExecutor is ScopedSqlExecutor ? AiServiceLocator.GetService<Bu.Services.AI_Services.Interfaces.IVectorService>() : null);
            _requestTimeoutReader = requestTimeoutReader ?? (_sqlExecutor is ScopedSqlExecutor
                ? (Func<int>)(() => AiConfigurationCoordinator.Instance.CurrentSnapshot.AiRequestTimeoutSeconds)
                : (() => AiConfigurationSnapshot.DefaultRequestTimeoutSeconds));
        }
        public void Reset(int userId, string conversationId) => _conversationManager.ClearSession(userId,conversationId);
        private AiAuthorizationContext Reload(AiAuthorizationContext ctx) => _policyProvider == null ? ctx : _policyProvider.Load(ctx.UserId);
        private void CheckCurrent(AiAuthorizationContext original, string domain)
        {
            var fresh = Reload(original);
            if (!fresh.IsAuthenticated || fresh.Fingerprint() != original.Fingerprint() || (domain != null && original.SourceRevisions.TryGetValue(domain,out var originalRevision) && (!fresh.SourceRevisions.TryGetValue(domain,out var freshRevision) || originalRevision != freshRevision))) throw new ConversationConflictException();
        }
        public async Task<AiChatExecutionResult> ProcessChatAsync(string question, AiAuthorizationContext authContext,
            string conversationId = null, string clientRequestId = null, string optionToken = null,
            CancellationToken cancellationToken = default, long? expectedConversationVersion = null, string clarificationId = null)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            // API caller token is the overall deadline; per-LLM budgets must never override it with a shorter hard-coded limit.
            var result = new AiChatExecutionResult { ConversationId = string.IsNullOrWhiteSpace(conversationId) ? Guid.NewGuid().ToString("N") : conversationId.Trim(), ClientRequestId = clientRequestId ?? Guid.NewGuid().ToString("N") };
            using (var trace = AiRequestTrace.Begin(result.ClientRequestId))
            {
            trace.OnCompleted = events => result.Diagnostics = events;
            AiRequestTrace.Record("REQUEST", "Tiếp nhận câu hỏi", new { questionChars = question?.Length ?? 0, optionSelected = !string.IsNullOrEmpty(optionToken) });
            Func<string,string,int,AiChatExecutionResult> error = (status,answer,http) => { result.Status=status; result.Answer=answer; result.HttpStatus=http; result.Clarification=null; result.InterpretedRequest=null; result.Provenance=null; result.TotalRecords=null; result.AsOf=null; result.ExecutionDurationMs=sw.ElapsedMilliseconds; AiRequestTrace.Record("ERROR", "Yêu cầu dừng", new { status, http, code = result.ErrorCode }); return result; };
            if (authContext == null || !authContext.IsAuthenticated) return error("forbidden","Bạn cần đăng nhập để sử dụng trợ lý.",401);
            if ((question?.Length ?? 0) > 2000 || result.ConversationId.Length > 100 || result.ClientRequestId.Length > 100 || (optionToken?.Length ?? 0) > 100 || (clarificationId?.Length ?? 0) > 100) return error("error","Nội dung hoặc mã yêu cầu vượt giới hạn cho phép.",400);
            if (string.IsNullOrWhiteSpace(question) && string.IsNullOrWhiteSpace(optionToken)) return error("error","Vui lòng nhập câu hỏi hoặc chọn phương án.",400);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                // A public greeting needs current identity/AI permission, but no data scope or policy schema.
                // It never mutates a pending business request or reads employee data.
                if(_policyProvider is IAiIdentityProvider identity && QueryUnderstandingService.IsPureGreeting(question) && string.IsNullOrEmpty(optionToken) && clarificationId==null)
                {
                    var actor=identity.LoadIdentity(authContext.UserId);
                    if(!actor.IsAuthenticated) return error("forbidden","Phiên đăng nhập không còn hợp lệ.",401);
                    if(!actor.HasFunctionRight("F_SYSTEM_AI")) return error("forbidden","Bạn chưa được cấp quyền sử dụng trợ lý AI.",403);
                    if(_conversationManager.TryGetSession(actor.UserId,result.ConversationId,out var existing)) result.ConversationVersion=existing.Version;
                    if(expectedConversationVersion.HasValue && expectedConversationVersion.Value!=result.ConversationVersion) return error("error","Hội thoại đã thay đổi. Vui lòng gửi lại lời chào.",409);
                    cancellationToken.ThrowIfCancellationRequested();
                    string greetName = !string.IsNullOrWhiteSpace(actor.FullName) ? actor.FullName : actor.Username;
                    string greetAnswer = $"Xin chào {greetName}! Tôi là Trợ lý AI Quản lý Nhân sự (HRMS Copilot). Tôi có thể hỗ trợ bạn tra cứu hồ sơ nhân viên, bảng lương, ngày công, tăng ca, hợp đồng lao động, nâng lương, bảo hiểm và chính sách nội quy công ty theo quyền được cấp. Bạn cần tra cứu thông tin gì hôm nay?";
                    if (_llmService != null)
                    {
                        try
                        {
                            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(AiConfigurationSnapshot.NormalizeRequestTimeout(_requestTimeoutReader()))))
                            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                            {
                                string llmGreet = await _llmService.GenerateGroundedAnswerAsync(question, greetAnswer, null, linked.Token);
                                if (!string.IsNullOrWhiteSpace(llmGreet) && !llmGreet.StartsWith("Lỗi", StringComparison.OrdinalIgnoreCase) && llmGreet.Length > 10)
                                {
                                    greetAnswer = llmGreet.Trim();
                                }
                            }
                        }
                        catch { }
                    }
                    result.Status="answered";result.Answer=greetAnswer;
                    result.ExecutionDurationMs=sw.ElapsedMilliseconds;return result;
                }
                var ctx = Reload(authContext);
                if (!ctx.IsAuthenticated) return error("forbidden","Phiên đăng nhập không còn hợp lệ.",401);
                if (ctx.PolicyLoaded && !ctx.HasFunctionRight("F_SYSTEM_AI")) return error("forbidden","Bạn chưa được cấp quyền sử dụng trợ lý AI.",403);
                var lease = _conversationManager.Begin(ctx.UserId,result.ConversationId,expectedConversationVersion,ctx.Fingerprint());
                string generalHistory = string.Join("\n", lease.GeneralHistory.Select(m => m.Role + ": " + m.Content));
                if (generalHistory.Length > 4000) generalHistory = generalHistory.Substring(generalHistory.Length - 4000);
                string businessHistory = lease.Previous == null ? null : Newtonsoft.Json.JsonConvert.SerializeObject(new { lease.Previous.Domain, lease.Previous.Operation, lease.Previous.Metric, lease.Previous.Time });
                AiRequestTrace.Record("HISTORY", "Nạp lịch sử riêng của phiên và quyền hiện tại", new { generalTurns = lease.GeneralHistory.Count, generalChars = generalHistory.Length, businessContext = lease.Previous != null, previousBusinessAnswersExcluded = true }, new { generalHistory = lease.GeneralHistory.Select(m => new { m.Role, m.Domain, content = AiRequestTrace.RedactText(m.Content) }).ToArray(), resolvedBusinessContext = businessHistory });
                result.ConversationVersion = lease.Version;
                QueryUnderstandingResult understood = null;
                if (lease.Pending != null && _clock.UtcNow - lease.Pending.CreatedAt >= TimeSpan.FromMinutes(10))
                {
                    if (optionToken != null || clarificationId != null) return error("error","Phiên làm rõ đã hết hạn. Hãy đặt lại câu hỏi.",409);
                    lease.Pending = null; lease.Previous = null;
                }
                if (!string.IsNullOrEmpty(optionToken))
                {
                    if (lease.Pending == null || lease.Previous == null || clarificationId != lease.Pending.ClarificationId || !expectedConversationVersion.HasValue) return error("error","Lựa chọn không thuộc phiên làm rõ hiện tại.",409);
                    understood = lease.Previous; understood.Clarification = lease.Pending;
                    if (!_clarificationPolicy.ApplyOptionSelection(understood,optionToken,lease.Pending.Field)) return error("error","Mã lựa chọn không hợp lệ.",409);
                }
                else if (lease.Pending != null && lease.Previous != null)
                {
                    string q = (question ?? "").Trim();
                    if (clarificationId != null && clarificationId != lease.Pending.ClarificationId) return error("error","Phiên làm rõ đã thay đổi.",409);
                    var supplement = _understandingService.UnderstandQuery(q,ctx);
                    bool newTopic = q.StartsWith("hủy",StringComparison.OrdinalIgnoreCase) || q.StartsWith("thôi",StringComparison.OrdinalIgnoreCase) ||
                        (supplement.Domain != "EMPLOYEE" && supplement.Domain != lease.Previous.Domain && supplement.Domain != "GENERAL" &&
                         !(supplement.Domain == "CHAT_GENERAL" && (lease.Pending.Field == "MANV" || lease.Pending.Field == "TIME_PERIOD" || lease.Pending.Field == "BIRTHDAY_MONTH")));
                    if (newTopic) understood = _understandingService.UnderstandQuery(q,ctx);
                    else
                    {
                        understood = lease.Previous;
                        if (lease.Pending.Field == "TIME_PERIOD" || lease.Pending.Field == "BIRTHDAY_MONTH")
                        {
                            if (supplement.Time.AnchorKind == "INVALID") understood.Time.AnchorKind = "INVALID";
                            else { if (supplement.Time.Month.HasValue) understood.Time.Month=supplement.Time.Month; if (supplement.Time.Year.HasValue) understood.Time.Year=supplement.Time.Year; if (supplement.Time.BirthdayMonth.HasValue) understood.Time.BirthdayMonth=supplement.Time.BirthdayMonth; }
                            understood.Clarification=null; understood.SupportStatus=QuerySupportStatus.Supported;
                        }
                        else
                        {
                            var opts = lease.Pending.Options.Where(o => string.Equals(o.Value,q,StringComparison.OrdinalIgnoreCase) || string.Equals(o.Label,q,StringComparison.OrdinalIgnoreCase) || (q.Length >= 2 && o.Label.IndexOf(q,StringComparison.OrdinalIgnoreCase) >= 0)).ToArray();
                            if (lease.Pending.Field == "MANV" && supplement.Entities.Any(e => e.EntityType == "DEPARTMENT" && e.ResolvedId.HasValue))
                            {
                                var dept = supplement.Entities.First(e => e.EntityType == "DEPARTMENT");
                                var candidates = understood.Entities.First(e => e.Status == "AMBIGUOUS").Candidates.Where(c => c.DepartmentId == dept.ResolvedId || string.Equals(c.DepartmentName,dept.ResolvedName,StringComparison.OrdinalIgnoreCase)).ToArray();
                                opts = lease.Pending.Options.Where(o => candidates.Any(c => c.Id.ToString() == o.Value)).ToArray();
                            }
                            if (opts.Length == 1) { understood.Clarification=lease.Pending; _clarificationPolicy.ApplyOptionSelection(understood,opts[0].Token,lease.Pending.Field); }
                            else { understood.Clarification=lease.Pending; understood.SupportStatus=QuerySupportStatus.NeedsClarification; }
                            if (q.IndexOf("tổng",StringComparison.OrdinalIgnoreCase) >= 0 && understood.Domain == "OVERTIME") understood.Operation="SUM";
                        }
                    }
                }
                else understood = _understandingService.UnderstandQuery(question,ctx,lease.Previous);

                // LLM Fallback nếu quy tắc cứng chưa xác định được nghiệp vụ
                if (understood.SupportStatus == QuerySupportStatus.Unsupported &&
                    understood.UnsupportedReason == "Chưa xác định được nghiệp vụ hoặc đối tượng cần tra cứu." &&
                    _llmService != null)
                {
                    understood.Domain = "GENERAL";
                    understood.SupportStatus = QuerySupportStatus.Supported;
                    understood.UnsupportedReason = null;
                }
                _clarificationPolicy.EvaluateClarificationNeeded(understood);
                var plan = _planner.CreatePlan(understood,ctx);
                AiRequestTrace.Record("PLAN", "Định tuyến và kiểm quyền", new { plan.Domain, plan.Operation, plan.RequiredCapability, strategy = plan.Strategy.ToString(), sourceRevision = plan.SourceRevision });
                result.InterpretedRequest = new SafeInterpretedRequest { Domain=plan.Domain, Operation=plan.Operation, Metric=plan.Metric, EntityDisplay=plan.SelectedEntityDisplay, RequestedScope=plan.RequestedScope, EffectiveScopeDisplay=plan.EffectiveScopeDisplay, EffectivePeriodDisplay=plan.EffectivePeriodDisplay, Assumptions=plan.Assumptions };
                RenderedResponse rendered;
                if (plan.Strategy == ExecutionStrategy.SqlTemplate)
                {
                    bool cacheable = plan.Domain == "EMPLOYEE" && plan.Operation == "COUNT" && plan.SourceRevision > 0;
                    string key = AiCacheCoordinator.BuildResultKey(plan,ctx);
                    bool sourceLoaded = false;
                    Func<Task<RenderedResponse>> read = async () =>
                    {
                        sourceLoaded = true;
                        AiRequestTrace.Record("SQL", "Planner tạo SQL từ mẫu kiểm soát; không phải Ollama sinh SQL", new { origin = "planner_template", parameterCount = plan.Parameters.Count }, new { sql = plan.SqlStatement, parameters = plan.Parameters.ToDictionary(p => p.Key, p => "[REDACTED:" + (p.Value?.GetType().Name ?? "null") + "]") });
                        var sqlResult = _sqlExecutor is IAuthorizedSqlExecutor authorized
                            ? await authorized.ExecutePlanAsync(plan,ctx,cacheable ? CancellationToken.None : cancellationToken)
                            : await _sqlExecutor.ExecuteScopedQueryAsync(plan.SqlStatement,plan.Parameters,cacheable ? CancellationToken.None : cancellationToken);
                        CheckCurrent(ctx,plan.Domain);
                        var response = DeterministicResponseRenderer.Render(plan,sqlResult);
                        response.AsOf=_clock.UtcNow;

                        response.EvidenceData = sqlResult.Data?.Copy();
                        AiRequestTrace.Record("SQL_RESULT", "Nhận kết quả nguồn đã kiểm quyền", new { status = sqlResult.Status.ToString(), rows = sqlResult.Data?.Rows.Count ?? 0, total = sqlResult.TotalRecords, rowLimit = plan.RowLimit, hasMore = sqlResult.HasMore, columns = sqlResult.Data?.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray(), fieldPolicies = plan.FieldModes });
                        return response;
                    };
                    var pending = _cacheCoordinator.GetOrExecuteResultAsync(key,!cacheable,read,() => !cancellationToken.IsCancellationRequested && _conversationManager.IsCurrent(lease));
                    if (!pending.IsCompleted && cancellationToken.CanBeCanceled)
                    {
                        var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                        using (cancellationToken.Register(() => cancelled.TrySetResult(true)))
                        {
                            if (await Task.WhenAny(pending,cancelled.Task) != pending)
                            {
                                _ = pending.ContinueWith(t => { var ignored=t.Exception; },TaskContinuationOptions.OnlyOnFaulted);
                                cancellationToken.ThrowIfCancellationRequested();
                            }
                        }
                    }
                    rendered = await pending;
                    AiRequestTrace.Record("CACHE", "Nguồn kết quả", new { eligible = cacheable, origin = sourceLoaded ? "source_load" : "cache_or_singleflight" });
                    rendered = await ComposeSqlAnswerAsync(plan, rendered, question, businessHistory, cancellationToken);
                }
                else if (plan.Strategy == ExecutionStrategy.ChatSocial)
                {
                    string ans;
                    if (understood.SocialType == "THANK")
                    {
                        ans = "Không có gì, rất vui được hỗ trợ bạn! Nếu cần thêm thông tin gì, bạn cứ hỏi nhé.";
                    }
                    else if (understood.SocialType == "GOODBYE")
                    {
                        ans = "Tạm biệt bạn! Chúc bạn một ngày làm việc hiệu quả và nhiều niềm vui.";
                    }
                    else
                    {
                        ans = "Xin chào! Tôi là Trợ lý AI Quản trị Nhân sự (HRMS Copilot). Tôi có thể giúp gì cho bạn hôm nay?";
                    }
                    rendered = new RenderedResponse { Status = "answered", Answer = ans, BypassedLlm = true, HttpStatus = 200, AsOf = _clock.UtcNow };
                }
                else if (plan.Strategy == ExecutionStrategy.HrHelp)
                {
                    string helpAnswer;
                    if (question.IndexOf("hỏi thế nào", StringComparison.OrdinalIgnoreCase) >= 0 || question.IndexOf("ngày công", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        helpAnswer = "Để tra cứu ngày công chính xác, bạn có thể hỏi kèm mã nhân viên (hoặc họ tên), tháng và năm.\n\nVí dụ:\n• \"Ngày công của nhân viên 21 tháng 3 năm 2026 là bao nhiêu?\"\n• \"Ngày công của tôi tháng này\" (áp dụng khi tài khoản đã liên kết mã nhân viên).\n• \"Bảng công Phòng Kế toán tháng 9 năm 2026\".";
                    }
                    else
                    {
                        helpAnswer = "Tôi là Trợ lý AI Quản lý Nhân sự (HRMS Copilot).\n\nTôi có thể hỗ trợ bạn tra cứu các thông tin sau theo phân quyền:\n• Hồ sơ nhân sự: Tra cứu danh sách nhân viên theo tên, mã số, phòng ban, xem sinh nhật theo tháng, thống kê số lượng nhân viên.\n• Chấm công & Tăng ca: Tra cứu ngày công tổng hợp, ngày phép, tổng giờ tăng ca, xếp hạng tăng ca nhiều nhất theo kỳ.\n• Hợp đồng lao động: Tra cứu hợp đồng, danh sách hợp đồng sắp hết hạn.\n• Quyết định nâng lương: Tra cứu các quyết định nâng bậc, hệ số lương mới.\n• Phụ cấp & Tạm ứng: Tra cứu thông tin phụ cấp theo kỳ, giao dịch tạm ứng lương.\n• Quy chế & Văn bản: Tra cứu nội quy, quy trình và chính sách lao động qua kho tài liệu công ty.\n\n*Lưu ý: Chức năng chi tiết bảng lương hiện đang tạm thời bảo trì.\n\nBạn cần tra cứu thông tin gì?";
                    }
                    rendered = new RenderedResponse { Status = "answered", Answer = helpAnswer, BypassedLlm = true, HttpStatus = 200, AsOf = _clock.UtcNow };
                }
                else if (plan.Strategy == ExecutionStrategy.ChatGeneral)
                {
                    string generalAnswer = null;
                    bool bypassed = true;

                    // 1. Kiểm tra nếu hỏi về thời tiết
                    if (understood.GeneralType == "WEATHER" || question.IndexOf("thời tiết", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        generalAnswer = "Hiện tại tôi không có kết nối thời gian thực hoặc công cụ tra cứu thời tiết trực tiếp, vì vậy không thể cung cấp thông tin thời tiết. Bạn vui lòng tra cứu trên các ứng dụng thời tiết chuyên dụng nhé.";
                        bypassed = true;
                    }
                    // 2. Kiểm tra nếu là câu hỏi toán học đơn giản
                    else if (understood.GeneralType == "MATH")
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(question, @"^\s*(\d+(?:[.,]\d+)?)\s*([\+\-\*\/xX]|cộng|trừ|nhân|chia)\s*(\d+(?:[.,]\d+)?)\s*(?:bằng|là)?\s*(?:mấy|bao nhiêu|\?)?\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (m.Success && double.TryParse(m.Groups[1].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double n1)
                                      && double.TryParse(m.Groups[3].Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double n2))
                        {
                            string op = m.Groups[2].Value.ToLowerInvariant();
                            double resVal = 0;
                            bool validOp = true;
                            if (op == "+" || op == "cộng") resVal = n1 + n2;
                            else if (op == "-" || op == "trừ") resVal = n1 - n2;
                            else if (op == "*" || op == "x" || op == "nhân") resVal = n1 * n2;
                            else if (op == "/" || op == "chia")
                            {
                                if (Math.Abs(n2) < 0.0000001) { generalAnswer = "Không thể chia cho số 0."; validOp = false; }
                                else resVal = n1 / n2;
                            }
                            else validOp = false;

                            if (validOp)
                            {
                                generalAnswer = $"{n1} {op} {n2} bằng {resVal}.";
                                bypassed = true;
                            }
                        }
                    }

                    // 3. Nếu chưa có câu trả lời, gọi LLM Ollama chuyên biệt cho Chat General
                    if (string.IsNullOrWhiteSpace(generalAnswer) && _llmService != null)
                    {
                        try
                        {
                            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(AiConfigurationSnapshot.NormalizeRequestTimeout(_requestTimeoutReader()))))
                            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                            {
                                AiRequestTrace.Record("LLM_INPUT", "Gửi câu hỏi và lịch sử chat chung tới Ollama", new { mode = "general_chat", historyChars = generalHistory.Length, businessFactsExcluded = true });
                                string llmAns = await _llmService.GenerateGeneralChatAsync(question, generalHistory, linked.Token);
                                if (!string.IsNullOrWhiteSpace(llmAns) && !llmAns.StartsWith("Lỗi", StringComparison.OrdinalIgnoreCase))
                                {
                                    generalAnswer = llmAns.Trim();
                                    bypassed = false;
                                }
                            }
                        }
                        catch
                        {
                            bypassed = true;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(generalAnswer))
                    {
                        AiRequestTrace.Record("FALLBACK", "Mô hình chat chưa phản hồi; không tạo câu trả lời mẫu sai yêu cầu", new { reason = "llm_unavailable_or_timeout" });
                        rendered = new RenderedResponse { Status = "error", Answer = "Mô hình AI chưa phản hồi hoặc quá thời gian chờ. Vui lòng thử lại.", BypassedLlm = true, HttpStatus = 503 };
                    }
                    else rendered = new RenderedResponse { Status = "answered", Answer = generalAnswer, BypassedLlm = bypassed, HttpStatus = 200, AsOf = _clock.UtcNow };
                }
                else if (plan.Strategy == ExecutionStrategy.DeterministicDirect)
                {
                    string guideAnswer = "Tôi là Trợ lý AI Quản lý Nhân sự (HRMS Copilot).\n\nTôi có thể hỗ trợ bạn các nghiệp vụ theo quyền được cấp:\n• Hồ sơ nhân sự: Tra cứu danh sách nhân viên theo tên, mã số, phòng ban, xem sinh nhật theo tháng, thống kê số lượng nhân viên.\n• Tiền lương & Thu nhập: Xem bảng lương, phiếu lương cá nhân, tiền lương theo công thực tế, tổng quỹ lương.\n• Chấm công & Tăng ca: Tra cứu ngày công, ngày phép, tổng giờ tăng ca, xếp hạng tăng ca nhiều nhất.\n• Hợp đồng lao động: Tra cứu hợp đồng, danh sách hợp đồng sắp hết hạn.\n• Quyết định nâng lương: Tra cứu các quyết định nâng bậc, hệ số lương mới.\n• Bảo hiểm & Tạm ứng: Tra cứu thông tin bảo hiểm xã hội, tạm ứng lương.\n• Quy chế & Văn bản: Tra cứu nội quy, chính sách lao động qua kho tri thức công ty.\n\nBạn cần tra cứu thông tin gì?";
                    bool bypassed = true;
                    if (_llmService != null)
                    {
                        try
                        {
                            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(AiConfigurationSnapshot.NormalizeRequestTimeout(_requestTimeoutReader()))))
                            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                            {
                                string llmAns = await _llmService.GenerateGroundedAnswerAsync(question, guideAnswer, null, linked.Token);
                                if (!string.IsNullOrWhiteSpace(llmAns) && !llmAns.StartsWith("Lỗi", StringComparison.OrdinalIgnoreCase) && llmAns.Length > 10)
                                {
                                    guideAnswer = llmAns.Trim();
                                    bypassed = false;
                                }
                            }
                        }
                        catch
                        {
                            bypassed = true;
                        }
                    }
                    rendered = new RenderedResponse { Status="answered", Answer=guideAnswer, BypassedLlm=bypassed, HttpStatus=200 };
                }
                else if (plan.Strategy == ExecutionStrategy.VectorSearch)
                {
                    if (_vectorService != null)
                    {
                        var bizFilter = new Bu.Services.AI_Services.Vector.VectorBusinessFilter { Tag = plan.Domain == "POLICY" ? "REGULATION" : plan.Domain, Domain = plan.Domain };
                        var secFilter = BuildVectorSecurityFilter(ctx, plan.VectorCapability ?? "POLICY_LOOKUP");
                        var vResult = await _vectorService.SearchScopedAsync(question, bizFilter, secFilter, null, cancellationToken);
                        LogVectorResult(vResult);
                        if (vResult != null && vResult.Status == Bu.Services.AI_Services.Vector.VectorSearchStatus.Unavailable)
                        {
                            rendered = new RenderedResponse
                            {
                                Status = "error",
                                Answer = "Dịch vụ tra cứu văn bản quy chế hiện chưa sẵn sàng do kết nối tới kho dữ liệu tri thức bị gián đoạn. Vui lòng liên hệ quản trị viên.",
                                SourceProvenance = "Qdrant Vector RAG (Unavailable)",
                                BypassedLlm = true,
                                HttpStatus = 503,
                                AsOf = _clock.UtcNow
                            };
                        }
                        else if (vResult != null && vResult.Status == Bu.Services.AI_Services.Vector.VectorSearchStatus.Forbidden)
                        {
                            rendered = new RenderedResponse
                            {
                                Status = "forbidden",
                                Answer = "Tài khoản của bạn không có quyền tra cứu văn bản quy chế hoặc hồ sơ trong phạm vi yêu cầu.",
                                SourceProvenance = "Qdrant Vector RAG (Forbidden)",
                                BypassedLlm = true,
                                HttpStatus = 403,
                                AsOf = _clock.UtcNow
                            };
                        }
                        else if (vResult == null || vResult.Status == VectorSearchStatus.InvalidIndex ||
                            vResult.Status == VectorSearchStatus.Timeout || vResult.Status == VectorSearchStatus.Cancelled)
                        {
                            bool timeout = vResult?.Status == VectorSearchStatus.Timeout;
                            bool cancelled = vResult?.Status == VectorSearchStatus.Cancelled;
                            rendered = new RenderedResponse
                            {
                                Status = "error",
                                Answer = timeout ? "Tra cứu văn bản quy chế quá thời gian chờ. Vui lòng thử lại." :
                                    cancelled ? "Yêu cầu tra cứu văn bản quy chế đã bị hủy." :
                                    "Kho tri thức chưa sẵn sàng hoặc chỉ mục không tương thích. Vui lòng liên hệ quản trị viên.",
                                SourceProvenance = "Qdrant Vector RAG (" + (vResult?.Status.ToString() ?? "Unavailable") + ")",
                                BypassedLlm = true,
                                HttpStatus = timeout ? 504 : cancelled ? 499 : 503,
                                AsOf = _clock.UtcNow
                            };
                        }
                        else if (vResult != null && vResult.Hits != null && vResult.Hits.Count > 0)
                        {
                            var topHits = SelectPolicyEvidence(vResult.Hits, PolicyEvidenceBudget(question, businessHistory));
                            if (topHits.Count > 0)
                            {
                                string combinedText = BuildPolicyEvidenceContext(topHits);
                                AiRequestTrace.Record("VECTOR_CONTEXT", "Nạp các đoạn Qdrant được phép vào prompt", new { selectedHits = topHits.Count, contextChars = combinedText.Length }, new { citations = topHits.Select(h => new { h.PointId, h.Score, h.SourceId, h.Title, h.Section, h.Version }).ToArray(), context = AiRequestTrace.RedactText(combinedText) });
                                string ans = combinedText;
                                bool bypassedLlm = true;
                                if (_llmService != null)
                                {
                                    try
                                    {
                                        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(AiConfigurationSnapshot.NormalizeRequestTimeout(_requestTimeoutReader()))))
                                        using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                                        {
                                            string grounded = await _llmService.GenerateGroundedAnswerAsync(question, combinedText, businessHistory, linked.Token);
                                            if (!string.IsNullOrWhiteSpace(grounded) && !grounded.StartsWith("Lỗi", StringComparison.OrdinalIgnoreCase) && grounded.Length > 10 && HasOnlyKnownEvidenceCitations(grounded, topHits.Count))
                                            {
                                                ans = grounded.Trim();
                                                bypassedLlm = false;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        bypassedLlm = true;
                                    }
                                }
                                var citationList = topHits.Select(h =>
                                {
                                    if (!string.IsNullOrWhiteSpace(h.Title))
                                    {
                                        string doc = !string.IsNullOrWhiteSpace(h.DocumentTitle) ? $"{h.DocumentTitle} - " : (!string.IsNullOrWhiteSpace(h.SourceId) ? $"[{h.SourceId}] " : "");
                                        string sec = !string.IsNullOrWhiteSpace(h.Section) ? $"Mục {h.Section}: " : "";
                                        string ver = !string.IsNullOrWhiteSpace(h.Version) ? $" (v{h.Version})" : "";
                                        return $"{doc}{sec}{h.Title}{ver}";
                                    }
                                    if (!string.IsNullOrWhiteSpace(h.SourceId)) return $"Văn bản [{h.SourceId}]";
                                    if (h.EmployeeId.HasValue) return $"Hồ sơ nhân sự #{h.EmployeeId}";
                                    return "Văn bản quy chế & chính sách";
                                }).Distinct().ToList();
                                string citation = string.Join("; ", citationList);

                                rendered = new RenderedResponse
                                {
                                    Status = "answered",
                                    Answer = ans + "\n\nNguồn tham khảo: " + citation,
                                    SourceProvenance = citation,
                                    TotalRecords = topHits.Count,
                                    BypassedLlm = bypassedLlm,
                                    HttpStatus = 200,
                                    AsOf = _clock.UtcNow
                                };
                            }
                            else
                            {
                                rendered = new RenderedResponse
                                {
                                    Status = "answered",
                                    Answer = "Không tìm thấy nội dung văn bản quy chế trong phạm vi được cấp quyền.",
                                    SourceProvenance = "Qdrant Vector RAG",
                                    BypassedLlm = true,
                                    HttpStatus = 200,
                                    AsOf = _clock.UtcNow
                                };
                            }
                        }
                        else
                        {
                            rendered = new RenderedResponse
                            {
                                Status = "answered",
                                Answer = "Không tìm thấy tài liệu quy chế hoặc văn bản liên quan phù hợp trong cơ sở tri thức.",
                                SourceProvenance = "Qdrant Vector RAG",
                                BypassedLlm = true,
                                HttpStatus = 200,
                                AsOf = _clock.UtcNow
                            };
                        }
                    }
                    else
                    {
                        rendered = new RenderedResponse
                        {
                            Status = "error",
                            Answer = "Dịch vụ tra cứu văn bản quy chế hiện chưa sẵn sàng. Vui lòng liên hệ quản trị viên.",
                            HttpStatus = 503
                        };
                    }
                }
                else if (plan.Strategy == ExecutionStrategy.Hybrid)
                {
                    // Branch Hybrid: Đồng thời tra cứu số liệu Oracle SQL và đối chiếu văn bản quy chế Qdrant
                    var sqlSubPlan = plan.CloneForSubPlan(ExecutionStrategy.SqlTemplate);
                    AiRequestTrace.Record("SQL", "Tạo subplan SQL của hybrid", new { origin = "planner_template", parameterCount = plan.Parameters.Count }, new { sql = plan.SqlStatement, parameters = plan.Parameters.ToDictionary(p => p.Key, p => "[REDACTED]") });
                    var sqlTask = _sqlExecutor is IAuthorizedSqlExecutor authorized
                        ? authorized.ExecutePlanAsync(sqlSubPlan, ctx, cancellationToken)
                        : _sqlExecutor.ExecuteScopedQueryAsync(plan.SqlStatement, plan.Parameters, cancellationToken);

                    string vectorCap = plan.VectorCapability ?? "POLICY_LOOKUP";
                    var capValidation = AiAuthorizationService.ValidateCapability(ctx, vectorCap);

                    Bu.Services.AI_Services.Vector.VectorSearchResult vResult = null;
                    if (capValidation.IsAllowed && _vectorService != null)
                    {
                        var bizFilter = new Bu.Services.AI_Services.Vector.VectorBusinessFilter { Tag = "REGULATION", Domain = "POLICY" };
                        var secFilter = BuildVectorSecurityFilter(ctx, vectorCap);
                        vResult = await _vectorService.SearchScopedAsync(question, bizFilter, secFilter, null, cancellationToken);
                        LogVectorResult(vResult);
                    }

                    var sqlResult = await sqlTask;
                    AiRequestTrace.Record("SQL_RESULT", "Nhận dữ kiện SQL của hybrid", new { status = sqlResult.Status.ToString(), rows = sqlResult.Data?.Rows.Count ?? 0, hasMore = sqlResult.HasMore });
                    var sqlResponse = DeterministicResponseRenderer.Render(sqlSubPlan, sqlResult);
                    sqlResponse.AsOf = _clock.UtcNow;

                    // Fail-closed if SQL branch failed or was denied:
                    if (sqlResult.Status == SqlExecutionStatus.AuthorizationDenied || sqlResponse.Status == "forbidden")
                    {
                        rendered = new RenderedResponse
                        {
                            Status = "forbidden",
                            HttpStatus = 403,
                            Answer = "Bạn không có quyền truy cập số liệu nhân sự được yêu cầu.",
                            SourceProvenance = "Oracle Database (Access Denied)",
                            BypassedLlm = true,
                            AsOf = _clock.UtcNow
                        };
                    }
                    else if (sqlResult.Status == SqlExecutionStatus.ExecutionError || sqlResult.Status == SqlExecutionStatus.Timeout || sqlResponse.Status == "error")
                    {
                        rendered = new RenderedResponse
                        {
                            Status = "error",
                            HttpStatus = sqlResponse.HttpStatus > 0 ? sqlResponse.HttpStatus : 500,
                            Answer = "Không thể truy xuất số liệu nhân sự do lỗi dịch vụ cơ sở dữ liệu.",
                            SourceProvenance = "Oracle Database (Error)",
                            BypassedLlm = true,
                            AsOf = _clock.UtcNow
                        };
                    }
                    else if (!capValidation.IsAllowed)
                    {
                        // Nhánh Vector bị từ chối quyền: Trả về số liệu SQL và ghi rõ giới hạn quyền văn bản
                        rendered = new RenderedResponse
                        {
                            Status = sqlResponse.Status,
                            Answer = sqlResponse.Answer + "\n\n(Lưu ý: Không thể tra cứu văn bản quy chế đối chiếu do tài khoản chưa được cấp quyền POLICY_LOOKUP).",
                            SourceProvenance = "Oracle Database (SQL Facts)",
                            Data = sqlResponse.Data,
                            TotalRecords = sqlResponse.TotalRecords,
                            HasMore = sqlResponse.HasMore,
                            BypassedLlm = true,
                            HttpStatus = sqlResponse.HttpStatus,
                            AsOf = _clock.UtcNow
                        };
                    }
                    else if (_vectorService != null)
                    {
                        var authorizedHits = SelectPolicyEvidence(vResult?.Hits, Math.Max(0, PolicyEvidenceBudget(question, businessHistory) - sqlResponse.Answer.Length));

                        string combinedFacts = $"Dữ liệu thực tế từ hệ thống:\n{sqlResponse.Answer}";
                        if (authorizedHits.Count > 0)
                        {
                            string policyContext = BuildPolicyEvidenceContext(authorizedHits);
                            AiRequestTrace.Record("VECTOR_CONTEXT", "Nạp tài liệu đối chiếu hybrid", new { selectedHits = authorizedHits.Count, contextChars = policyContext.Length }, new { context = AiRequestTrace.RedactText(policyContext) });
                            var citationList = authorizedHits.Select(h =>
                            {
                                if (!string.IsNullOrWhiteSpace(h.Title))
                                {
                                    string sec = !string.IsNullOrWhiteSpace(h.Section) ? $" - Mục {h.Section}" : "";
                                    string ver = !string.IsNullOrWhiteSpace(h.Version) ? $" (v{h.Version})" : "";
                                    return $"{h.Title}{sec}{ver}";
                                }
                                if (!string.IsNullOrWhiteSpace(h.SourceId)) return $"Văn bản [{h.SourceId}]";
                                return "Quy chế & văn bản chính sách";
                            }).Distinct().ToList();
                            string citation = string.Join("; ", citationList);

                            string ans = combinedFacts + "\n\nQuy chế áp dụng:\n" + policyContext;
                            bool bypassedLlm = true;

                            if (_llmService != null)
                            {
                                try
                                {
                                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(AiConfigurationSnapshot.NormalizeRequestTimeout(_requestTimeoutReader()))))
                                    using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                                    {
                                        string synthesisPrompt = $"Dựa vào số liệu thực tế và quy định dưới đây, hãy giải thích và trả lời câu hỏi của người dùng:\n{combinedFacts}\n\nQuy định:\n{policyContext}";
                                        string grounded = await _llmService.GenerateGroundedAnswerAsync(question, synthesisPrompt, businessHistory, linked.Token);
                                        if (!string.IsNullOrWhiteSpace(grounded) && HasOnlyKnownEvidenceCitations(grounded, authorizedHits.Count) && ValidateGroundedFacts(sqlSubPlan, sqlResult, sqlResponse.Answer, grounded))
                                        {
                                            ans = grounded.Trim();
                                            bypassedLlm = false;
                                        }
                                    }
                                }
                                catch
                                {
                                    bypassedLlm = true;
                                }
                            }

                            rendered = new RenderedResponse
                            {
                                Status = "answered",
                                Answer = ans + $"\n\nNguồn: Oracle DB & {citation}",
                                SourceProvenance = $"Oracle DB & {citation}",
                                Data = sqlResponse.Data,
                                TotalRecords = sqlResponse.TotalRecords,
                                HasMore = sqlResponse.HasMore,
                                BypassedLlm = bypassedLlm,
                                HttpStatus = 200,
                                AsOf = _clock.UtcNow
                            };
                        }
                        else
                        {
                            string policyNote;
                            if (vResult?.Status == Bu.Services.AI_Services.Vector.VectorSearchStatus.Unavailable)
                            {
                                policyNote = "\n\n(Lưu ý: Dịch vụ đối chiếu quy chế hiện tạm thời gián đoạn, chỉ hiển thị số liệu từ cơ sở dữ liệu).";
                            }
                            else if (vResult?.Status == Bu.Services.AI_Services.Vector.VectorSearchStatus.Forbidden)
                            {
                                policyNote = "\n\n(Lưu ý: Tài khoản chưa được cấp quyền đối chiếu văn bản quy chế cho nội dung này).";
                            }
                            else if (vResult?.Status == Bu.Services.AI_Services.Vector.VectorSearchStatus.Timeout)
                            {
                                policyNote = "\n\n(Lưu ý: Tra cứu văn bản quy chế quá thời gian chờ, chỉ hiển thị số liệu từ cơ sở dữ liệu).";
                            }
                            else if (vResult == null || vResult.Status == VectorSearchStatus.InvalidIndex || vResult.Status == VectorSearchStatus.Cancelled)
                            {
                                policyNote = "\n\n(Lưu ý: Kho tri thức chưa sẵn sàng hoặc yêu cầu đối chiếu đã bị hủy, chỉ hiển thị số liệu từ cơ sở dữ liệu).";
                            }
                            else
                            {
                                policyNote = "\n\n(Không tìm thấy tài liệu quy chế đối chiếu phù hợp trong cơ sở tri thức).";
                            }

                            rendered = new RenderedResponse
                            {
                                Status = sqlResponse.Status,
                                Answer = sqlResponse.Answer + policyNote,
                                SourceProvenance = "Oracle Database (SQL Facts)",
                                Data = sqlResponse.Data,
                                TotalRecords = sqlResponse.TotalRecords,
                                HasMore = sqlResponse.HasMore,
                                BypassedLlm = true,
                                HttpStatus = sqlResponse.HttpStatus,
                                AsOf = _clock.UtcNow
                            };
                        }
                    }
                    else
                    {
                        rendered = sqlResponse;
                    }
                }
                else rendered = DeterministicResponseRenderer.Render(plan,null);
                cancellationToken.ThrowIfCancellationRequested(); CheckCurrent(ctx,plan.Domain);
                result.Status=rendered.Status; result.Answer=rendered.Answer; result.Provenance=rendered.SourceProvenance; result.TotalRecords=rendered.TotalRecords; result.HasMore=rendered.HasMore; result.AsOf=rendered.AsOf; result.BypassedLlm=rendered.BypassedLlm; result.Data=rendered.Data;
                if (rendered.Status == "error") { result.HttpStatus=rendered.HttpStatus; return result; }
                if (rendered.Status == "forbidden") { result.HttpStatus=403; return result; }
                var clarification = plan.Strategy == ExecutionStrategy.NeedsClarification ? plan.Clarification : null;
                if (clarification != null) clarification.CreatedAt=_clock.UtcNow;
                if (!_conversationManager.Commit(lease,understood,clarification,question,rendered.Answer,out int version,cancellationToken)) return error("error","Hội thoại đã thay đổi. Kết quả cũ đã được bỏ qua.",409);
                result.ConversationVersion=version; result.Clarification=clarification; result.ExecutionDurationMs=sw.ElapsedMilliseconds;
                AiRequestTrace.Record("ANSWER", "Hoàn tất câu trả lời và lưu lịch sử", new { result.Status, answerOrigin = result.BypassedLlm ? "verified_fallback_or_rule" : "ollama", answerChars = result.Answer?.Length ?? 0, durationMs = result.ExecutionDurationMs, conversationVersion = version });

                System.Diagnostics.Trace.TraceInformation("[AI_EXEC] ClientRequestId={0}, UserId={1}, Domain={2}, Operation={3}, Capability={4}, LatencyMs={5}, Status={6}, BypassedLlm={7}",
                    result.ClientRequestId, authContext.UserId, plan?.Domain, plan?.Operation, plan?.RequiredCapability, result.ExecutionDurationMs, result.Status, result.BypassedLlm);

                return result;
            }
            catch (AiSourceUnavailableException ex) { result.ErrorCode=ex.ErrorCode; return error("error",ex.ErrorCode=="AI_SETUP_REQUIRED" ? "Nguồn tra cứu AI chưa được thiết lập đầy đủ. Quản trị viên cần hoàn tất cấu hình dữ liệu và phân quyền." : "Nguồn dữ liệu hoặc chính sách AI chưa sẵn sàng. Vui lòng liên hệ quản trị viên.",503); }
            catch (ConversationConflictException) { if (_conversationManager.TryGetSession(authContext.UserId,result.ConversationId,out var current)) result.ConversationVersion=current.Version; return error("error","Phiên hội thoại, quyền truy cập hoặc dữ liệu đã thay đổi. Vui lòng gửi lại câu hỏi.",409); }
            catch (OperationCanceledException) { System.Diagnostics.Trace.TraceWarning("[AI_TIMEOUT] ClientRequestId={0}, UserId={1}, LatencyMs={2}", result.ClientRequestId, authContext.UserId, sw.ElapsedMilliseconds); return error("error","Yêu cầu đã bị hủy hoặc quá thời gian chờ.",504); }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("AI request {0}: {1}",result.ClientRequestId,ex.GetType().Name); return error("error","Có lỗi khi xử lý yêu cầu. Vui lòng thử lại sau.",500); }
        }

        }


        private async Task<RenderedResponse> ComposeSqlAnswerAsync(QueryExecutionPlan plan, RenderedResponse source, string question, string history, CancellationToken cancellationToken)
        {
            // Never mutate a response shared by the result cache.
            var response = new RenderedResponse {
                Status = source.Status, Answer = source.Answer, SourceProvenance = source.SourceProvenance,
                TotalRecords = source.TotalRecords, HasMore = source.HasMore, BypassedLlm = true,
                AsOf = source.AsOf, HttpStatus = source.HttpStatus, Data = source.Data?.Copy(), EvidenceData = source.EvidenceData?.Copy()
            };
            if (response.Status != "answered" || _llmService == null || string.IsNullOrWhiteSpace(response.Answer)) {
                AiRequestTrace.Record("FALLBACK", "Giữ kết quả xác minh", new { reason = response.Status != "answered" ? response.Status : "llm_not_configured" });
                return response;
            }
            if (response.Answer.Length > 12000) {
                AiRequestTrace.Record("FALLBACK", "Kết quả vượt giới hạn ngữ cảnh; giữ bảng đầy đủ", new { reason = "evidence_too_large", evidenceChars = response.Answer.Length });
                return response;
            }
            AiRequestTrace.Record("LLM_INPUT", "Đưa kết quả đã kiểm quyền cho AI diễn đạt", new {
                mode = "grounded_sql", rows = plan.IsScalar ? 1 : Math.Min(plan.RowLimit, response.EvidenceData?.Rows.Count ?? 0), hasMore = response.HasMore,
                evidenceChars = response.Answer.Length, historyChars = history?.Length ?? 0,
                maskedFields = plan.MaskedFields, deniedFields = plan.FieldModes.Where(p => p.Value == "DENY").Select(p => p.Key).ToArray()
            }, new { authorizedEvidence = AiRequestTrace.RedactText(response.Answer), source = plan.TargetView });
            try {
                using (var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(AiConfigurationSnapshot.NormalizeRequestTimeout(_requestTimeoutReader()))))
                using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token)) {
                    string facts = response.Answer;
                    if (!plan.IsScalar) facts = "Hãy viết một đoạn diễn giải ngắn về kết quả dưới đây, không thêm dữ kiện và không chép lại bảng. Bảng dữ kiện sẽ được hiển thị nguyên vẹn bên dưới.\n\n" + facts;
                    string generated = await _llmService.GenerateGroundedAnswerAsync(question, facts, history, linked.Token);
                    var sqlEvidence = new SqlExecutionResult { Data = response.EvidenceData, Status = SqlExecutionStatus.SuccessWithData };
                    if (string.IsNullOrWhiteSpace(generated)) {
                        AiRequestTrace.Record("FALLBACK", "AI trả rỗng; dùng bằng chứng nguồn", new { reason = "empty_llm_answer" });
                        return response;
                    }
                    if (!ValidateGroundedFacts(plan, sqlEvidence, response.Answer, generated) ||
                        !NumbersHaveEvidence(generated, response.Answer)) {
                        AiRequestTrace.Record("FALLBACK", "Loại câu trả lời không khớp dữ kiện", new { reason = "facts_validation_failed", generatedChars = generated.Length });
                        return response;
                    }
                    if (!plan.IsScalar && RepeatsVerifiedRows(generated, response.Answer)) {
                        AiRequestTrace.Record("FALLBACK", "AI chép lại danh sách; chỉ hiển thị kết quả nguồn một lần", new { reason = "llm_repeated_verified_rows", generatedChars = generated.Length });
                        return response;
                    }
                    response.Answer = plan.IsScalar ? generated.Trim() : generated.Trim() + "\n\n" + response.Answer;
                    if (plan.IsScalar && !response.Answer.Contains("Nguồn: ")) {
                        int provenance = source.Answer.LastIndexOf("\n\nNguồn:", StringComparison.Ordinal);
                        if (provenance >= 0) response.Answer += source.Answer.Substring(provenance);
                    }
                    response.BypassedLlm = false;
                    AiRequestTrace.Record("LLM_ACCEPTED", "Câu trả lời AI đã đối chiếu dữ kiện", new { answerChars = response.Answer.Length, verifiedTablePreserved = !plan.IsScalar });
                }
            } catch (OperationCanceledException) {
                cancellationToken.ThrowIfCancellationRequested();
                AiRequestTrace.Record("FALLBACK", "AI diễn đạt quá thời gian; giữ kết quả nguồn", new { reason = "llm_timeout" });
            } catch (Exception ex) {
                AiRequestTrace.Record("FALLBACK", "AI diễn đạt gặp lỗi; giữ kết quả nguồn", new { reason = ex.GetType().Name });
            }
            return response;
        }

        private static bool RepeatsVerifiedRows(string narrative, string verifiedAnswer)
        {
            // Lists and tables belong to the verified renderer. A model may ignore
            // the summary-only instruction, so enforce this before concatenation.
            string normalized = NormalizeAnswerText(narrative);
            if (Regex.IsMatch(narrative, @"(?m)^\s*(?:[-*•]\s+|\d+\s*[.)]\s+|\|)") ||
                normalized.IndexOf("Kết quả tra cứu:", StringComparison.OrdinalIgnoreCase) >= 0 ||
                normalized.IndexOf("Nguồn:", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            foreach (Match row in Regex.Matches(verifiedAnswer, @"(?m)^\s*\d+\. (?<fields>[^\r\n]+)")) {
                foreach (string field in row.Groups["fields"].Value.Split(new[] { " • " }, StringSplitOptions.None)) {
                    int separator = field.IndexOf(": ", StringComparison.Ordinal);
                    if (separator < 0) continue;
                    string value = NormalizeAnswerText(field.Substring(separator + 2));
                    // Detect prose copies as well as numbered copies. Ignore generic
                    // placeholders and numbers that a legitimate summary can share.
                    if (value.Length >= 3 && value != "Chưa có" && value.Any(char.IsLetter) &&
                        normalized.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }
            return false;
        }

        private static string NormalizeAnswerText(string text)
        {
            return Regex.Replace((text ?? "").Normalize(System.Text.NormalizationForm.FormKC), @"\s+", " ").Trim();
        }

        private static bool NumbersHaveEvidence(string answer, string evidence)
        {
            var known = new HashSet<string>(Regex.Matches(evidence ?? "", @"\b\d+(?:[.,]\d+)?\b").Cast<Match>().Select(m => m.Value.Replace(',', '.')));
            return Regex.Matches(answer ?? "", @"\b\d+(?:[.,]\d+)?\b").Cast<Match>().All(m => known.Contains(m.Value.Replace(',', '.')));
        }


        public static List<VectorHit> SelectPolicyEvidence(IEnumerable<VectorHit> hits, int characterBudget = 6000)
        {
            var selected=new List<VectorHit>();
            int used=0;
            var groups=(hits ?? Enumerable.Empty<VectorHit>()).Where(h=>h.IsAuthorized && !string.IsNullOrWhiteSpace(h.Text))
                .GroupBy(h=>new {h.TenantId,h.SourceId,h.Version,Clause=h.ClauseId ?? h.Section ?? h.PointId}).OrderByDescending(g=>g.Max(h=>h.Score));
            foreach(var group in groups)
            {
                var chunks=group.GroupBy(h=>h.ChunkIndex).Select(g=>g.First()).OrderBy(h=>h.ChunkIndex).ToList();
                int total=chunks.Max(h=>h.TotalChunks);
                if(total>1 && (chunks.Count!=total || !chunks.Select(h=>h.ChunkIndex).SequenceEqual(Enumerable.Range(0,total)))) continue;
                int size=chunks.Sum(h=>h.Text.Length + 350);
                if(used+size>characterBudget) continue; // Never trim away an exception or half a clause.
                selected.AddRange(chunks); used+=size;
            }
            return selected;
        }
        public static string BuildPolicyEvidenceContext(IList<VectorHit> hits) => string.Join("\n\n", hits.Select((h,i)=>
            "[E"+(i+1)+"] "+h.DocumentTitle+" | "+h.ClauseId+" | v"+h.Version+"\n"+h.Text));
        public static bool HasOnlyKnownEvidenceCitations(string answer, int count)
        {
            return Regex.Matches(answer ?? "", @"\[E(\d+)\]").Cast<Match>()
                .All(m=>int.TryParse(m.Groups[1].Value,out var id) && id>0 && id<=count);
        }
        private int PolicyEvidenceBudget(string question, string history)
        {
            int budget=(_llmService as OllamaService)?.EvidenceCharacterBudget ?? 6000;
            return Math.Max(0,budget-(question?.Length ?? 0)-(history?.Length ?? 0));
        }

        private static void LogVectorResult(VectorSearchResult result)
        {
            var hits = result?.Hits?.Where(h => h.IsAuthorized).ToList() ?? new List<VectorHit>();
            AiRequestTrace.Record("QDRANT_RESULT", "Kết quả retrieval sau kiểm quyền", new {
                status = result?.Status.ToString() ?? "null", returnedHits = result?.Hits?.Count ?? 0,
                authorizedHits = hits.Count, excludedHits = (result?.Hits?.Count ?? 0) - hits.Count
            }, new { authorizedHits = hits.Select(h => new { h.PointId, h.Score, h.Domain, h.DocumentType, h.SourceId, h.Title, h.Section, h.Version, h.UpdatedAt, text = AiRequestTrace.RedactText(h.Text) }).ToArray() });
        }

        private Bu.Services.AI_Services.Vector.VectorSecurityFilter BuildVectorSecurityFilter(AiAuthorizationContext ctx, string capabilityCode)
        {
            var filter = new Bu.Services.AI_Services.Vector.VectorSecurityFilter
            {
                UserId = ctx.UserId,
                CallerEmployeeId = ctx.Manv,
                AllowedCompanyId = ctx.MaCty,
                AllowedDepartmentIds = new List<int>(),
                AllowedRoleIds = ctx.AllowedRoleIds?.ToList() ?? new List<string>(),
                AsOf = new DateTimeOffset(DateTime.SpecifyKind(_clock.UtcNow, DateTimeKind.Utc))
            };

            var grants = ctx.ScopeGrants != null
                ? ctx.ScopeGrants.Where(g => string.Equals(g.CapabilityCode, capabilityCode, StringComparison.OrdinalIgnoreCase)
                                          || string.Equals(g.CapabilityCode, "ALL", StringComparison.OrdinalIgnoreCase)).Where(g => (!g.ValidFrom.HasValue || g.ValidFrom.Value <= DateTime.Now) && (!g.ValidTo.HasValue || g.ValidTo.Value > DateTime.Now)).ToList()
                : new List<AiScopeGrant>();

            string effectiveScope = "SELF";
            if (grants.Any(g => string.Equals(g.ScopeType, "ALL", StringComparison.OrdinalIgnoreCase) && string.Equals(g.Effect, "ALLOW", StringComparison.OrdinalIgnoreCase)))
            {
                effectiveScope = "ALL";
            }
            else if (grants.Any(g => string.Equals(g.ScopeType, "COMPANY", StringComparison.OrdinalIgnoreCase) && string.Equals(g.Effect, "ALLOW", StringComparison.OrdinalIgnoreCase)))
            {
                effectiveScope = "COMPANY";
            }
            else if (grants.Any(g => string.Equals(g.ScopeType, "DEPARTMENT", StringComparison.OrdinalIgnoreCase) && string.Equals(g.Effect, "ALLOW", StringComparison.OrdinalIgnoreCase)))
            {
                effectiveScope = "DEPARTMENT";
            }
            else if (grants.Any(g => string.Equals(g.ScopeType, "SELF", StringComparison.OrdinalIgnoreCase) && string.Equals(g.Effect, "ALLOW", StringComparison.OrdinalIgnoreCase)))
            {
                effectiveScope = "SELF";
            }

            filter.AllowedDepartmentIds = grants.Where(g => g.Effect == "ALLOW" && g.ScopeType == "DEPARTMENT").Select(g => int.TryParse(g.ScopeKey, out int departmentId) ? departmentId : 0).Where(id => id > 0).Distinct().ToList();
            if (effectiveScope == "COMPANY") {
                var companies = grants.Where(g => g.Effect == "ALLOW" && g.ScopeType == "COMPANY").Select(g => g.ScopeKey).Where(key => !string.IsNullOrWhiteSpace(key)).Distinct().ToArray();
                filter.AllowedCompanyId = companies.Length == 1 ? companies[0] : null;
            }

            filter.EffectiveScope = effectiveScope;
            filter.IsAdmin = ctx.IsAdmin && (effectiveScope == "ALL");

            var denies = grants.Where(g => string.Equals(g.Effect, "DENY", StringComparison.OrdinalIgnoreCase)).Select(g => g.CapabilityCode).Distinct().ToList();
            filter.DenyCodes = denies;

            return filter;
        }

        private static bool ValidateGroundedFacts(QueryExecutionPlan plan, SqlExecutionResult sqlResult, string deterministicAnswer, string groundedAnswer)
        {
            if (string.IsNullOrWhiteSpace(deterministicAnswer) || string.IsNullOrWhiteSpace(groundedAnswer))
                return false;

            if (groundedAnswer.StartsWith("Lỗi", StringComparison.OrdinalIgnoreCase) ||
                (groundedAnswer.IndexOf("không thể", StringComparison.OrdinalIgnoreCase) >= 0 && groundedAnswer.Length < 30))
            {
                return false;
            }

            if (plan.IsScalar)
            {
                var table = sqlResult?.Data;
                if (table == null || table.Rows.Count == 0) return false;
                string measure = plan.Operation == "COUNT" ? "TOTAL_COUNT" : plan.Domain == "OVERTIME" ? "TONG_SOGIO" : plan.Metric == "LUONG_CONG_THUCTE" ? "TONG_LUONG_CONG_THUCTE" : "TONG_THUCLANH";
                if (!table.Columns.Contains(measure)) return false;

                decimal expectedVal = table.Rows.Cast<DataRow>().Sum(r => r[measure] == DBNull.Value ? 0 : Convert.ToDecimal(r[measure]));
                string expectedInv = expectedVal.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
                string expectedVi = expectedVal.ToString("0.##", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));

                // Grounded answer MUST contain the exact scalar quantity
                if (!groundedAnswer.Contains(expectedInv) && !groundedAnswer.Contains(expectedVi))
                {
                    return false;
                }

                // Verify that no conflicting quantities or hallucinations are present
                var matches = Regex.Matches(groundedAnswer, @"\b\d+(?:[\.,]\d+)?\b");
                foreach (Match m in matches)
                {
                    string numStr = m.Value.Replace(",", ".");
                    if (decimal.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal val))
                    {
                        if (val == expectedVal) continue;
                        if (val >= 2020 && val <= 2030) continue; // Calendar year
                        if (val >= 1 && val <= 12 && plan.EffectivePeriodDisplay != null) continue; // Month
                        if (val == 30 || val == 31) continue; // 30-day window
                        if (!deterministicAnswer.Contains(m.Value))
                        {
                            // Hallucinated number not in evidence!
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                var matches = Regex.Matches(groundedAnswer, @"\b\d+(?:[\.,]\d+)?\b");
                foreach (Match m in matches)
                {
                    string numStr = m.Value.Replace(",", ".");
                    if (decimal.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal val))
                    {
                        if (val >= 2020 && val <= 2030) continue;
                        if (val >= 1 && val <= 31) continue;
                        if (!deterministicAnswer.Contains(m.Value))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
    }
}
