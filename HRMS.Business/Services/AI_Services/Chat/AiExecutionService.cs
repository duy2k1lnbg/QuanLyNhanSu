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
        private readonly Bu.Services.AI_Services.Interfaces.IVectorService _vectorService;

        public AiExecutionService(IQueryUnderstandingService understandingService = null, IQueryPlanner planner = null,
            IScopedSqlExecutor sqlExecutor = null, ConversationStateManager conversationManager = null,
            AiCacheCoordinator cacheCoordinator = null, ClarificationPolicy clarificationPolicy = null,
            IClockProvider clock = null, IAiPolicyProvider policyProvider = null, ILlmService llmService = null,
            Bu.Services.AI_Services.Interfaces.IVectorService vectorService = null)
        {
            _clock = clock ?? new SystemClockProvider(); _understandingService = understandingService ?? new QueryUnderstandingService(clock:_clock);
            _planner = planner ?? new QueryPlanner(_clock); _sqlExecutor = sqlExecutor ?? new ScopedSqlExecutor();
            _conversationManager = conversationManager ?? ConversationStateManager.Instance; _cacheCoordinator = cacheCoordinator ?? AiCacheCoordinator.Instance;
            _clarificationPolicy = clarificationPolicy ?? new ClarificationPolicy();
            // Production always reloads policy. Explicit injected test executors may use fixture contexts.
            _policyProvider = policyProvider ?? (_sqlExecutor is ScopedSqlExecutor ? new OracleAiPolicyProvider() : null);
            _llmService = llmService ?? (_sqlExecutor is ScopedSqlExecutor ? AiServiceLocator.GetService<ILlmService>() : null);
            _vectorService = vectorService ?? (_sqlExecutor is ScopedSqlExecutor ? AiServiceLocator.GetService<Bu.Services.AI_Services.Interfaces.IVectorService>() : null);
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
            var result = new AiChatExecutionResult { ConversationId = string.IsNullOrWhiteSpace(conversationId) ? Guid.NewGuid().ToString("N") : conversationId.Trim(), ClientRequestId = clientRequestId ?? Guid.NewGuid().ToString("N") };
            Func<string,string,int,AiChatExecutionResult> error = (status,answer,http) => { result.Status=status; result.Answer=answer; result.HttpStatus=http; result.Clarification=null; result.InterpretedRequest=null; result.Provenance=null; result.TotalRecords=null; result.AsOf=null; result.ExecutionDurationMs=sw.ElapsedMilliseconds; return result; };
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
                    result.Status="answered";result.Answer="Xin chào! Tôi có thể hỗ trợ tra cứu nhân sự theo quyền được cấp. Tra cứu dữ liệu cần nguồn và chính sách AI đã được thiết lập.";
                    result.ExecutionDurationMs=sw.ElapsedMilliseconds;return result;
                }
                var ctx = Reload(authContext);
                if (!ctx.IsAuthenticated) return error("forbidden","Phiên đăng nhập không còn hợp lệ.",401);
                if (ctx.PolicyLoaded && !ctx.HasFunctionRight("F_SYSTEM_AI")) return error("forbidden","Bạn chưa được cấp quyền sử dụng trợ lý AI.",403);
                var lease = _conversationManager.Begin(ctx.UserId,result.ConversationId,expectedConversationVersion,ctx.Fingerprint());
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
                        (supplement.Domain != "EMPLOYEE" && supplement.Domain != lease.Previous.Domain && supplement.Domain != "GENERAL");
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
                _clarificationPolicy.EvaluateClarificationNeeded(understood);
                var plan = _planner.CreatePlan(understood,ctx);
                result.InterpretedRequest = new SafeInterpretedRequest { Domain=plan.Domain, Operation=plan.Operation, Metric=plan.Metric, EntityDisplay=plan.SelectedEntityDisplay, RequestedScope=plan.RequestedScope, EffectiveScopeDisplay=plan.EffectiveScopeDisplay, EffectivePeriodDisplay=plan.EffectivePeriodDisplay, Assumptions=plan.Assumptions };
                RenderedResponse rendered;
                if (plan.Strategy == ExecutionStrategy.SqlTemplate)
                {
                    bool cacheable = plan.Domain == "EMPLOYEE" && plan.Operation == "COUNT" && plan.SourceRevision > 0;
                    string key = AiCacheCoordinator.BuildResultKey(plan,ctx);
                    Func<Task<RenderedResponse>> read = async () =>
                    {
                        var sqlResult = _sqlExecutor is IAuthorizedSqlExecutor authorized
                            ? await authorized.ExecutePlanAsync(plan,ctx,cacheable ? CancellationToken.None : cancellationToken)
                            : await _sqlExecutor.ExecuteScopedQueryAsync(plan.SqlStatement,plan.Parameters,cacheable ? CancellationToken.None : cancellationToken);
                        CheckCurrent(ctx,plan.Domain);
                        var response = DeterministicResponseRenderer.Render(plan,sqlResult);
                        response.AsOf=_clock.UtcNow;

                        // Phase C: If LLM is available, ground the response from Oracle evidence
                        // Phase C: If LLM is available, ground the response from Oracle evidence
                        // Tránh gọi LLM tóm tắt với danh sách nhiều dòng hoặc bảng kết quả lớn để chặn timeout 35s
                        bool isMultiRecordList = (!plan.IsScalar && (response.Data?.Rows.Count > 1 || response.TotalRecords > 1)) ||
                                                 (response.Answer != null && response.Answer.Length > 800) ||
                                                 (plan.Operation == "LIST" || (plan.Operation == "LOOKUP" && response.Data?.Rows.Count > 1));

                        if (response.Status == "answered" && _llmService != null && !string.IsNullOrWhiteSpace(response.Answer) && !isMultiRecordList)
                        {
                            try
                            {
                                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                                using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                                {
                                    string grounded = await _llmService.GenerateGroundedAnswerAsync(question, response.Answer, null, linked.Token);
                                    if (!string.IsNullOrWhiteSpace(grounded) && !grounded.StartsWith("Lỗi", StringComparison.OrdinalIgnoreCase) && grounded.Length > 10)
                                    {
                                        if (ValidateGroundedFacts(plan, sqlResult, response.Answer, grounded))
                                        {
                                            string trimmedGrounded = grounded.Trim();
                                            int provIndex = response.Answer.LastIndexOf("\n\nNguồn: ", StringComparison.Ordinal);
                                            string provenance = provIndex >= 0 ? response.Answer.Substring(provIndex) : null;
                                            if (!string.IsNullOrEmpty(provenance) && !trimmedGrounded.Contains("Nguồn: "))
                                            {
                                                trimmedGrounded = trimmedGrounded + provenance;
                                            }
                                            response.Answer = trimmedGrounded;
                                            response.BypassedLlm = false;
                                        }
                                        else
                                        {
                                            // Hallucinated or contradictory answer from LLM: reject and keep deterministic renderer output
                                            response.BypassedLlm = true;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // Graceful fallback to deterministic response
                                response.BypassedLlm = true;
                            }
                        }

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
                }
                else if (plan.Strategy == ExecutionStrategy.DeterministicDirect) rendered = new RenderedResponse { Status="answered", Answer="Chào bạn. Tôi có thể tra cứu nhân sự, ngày công, tăng ca và các nghiệp vụ được cấp quyền. Hãy cho biết đối tượng và kỳ cần xem.", BypassedLlm=true, HttpStatus=200 };
                else if (plan.Strategy == ExecutionStrategy.VectorSearch)
                {
                    if (_vectorService != null)
                    {
                        var bizFilter = new Bu.Services.AI_Services.Vector.VectorBusinessFilter { Tag = plan.Domain == "POLICY" ? "REGULATION" : plan.Domain };
                        var secFilter = BuildVectorSecurityFilter(ctx, plan.VectorCapability ?? "POLICY_LOOKUP");
                        var vResult = await _vectorService.SearchScopedAsync(question, bizFilter, secFilter, null, cancellationToken);
                        if (vResult != null && vResult.Hits != null && vResult.Hits.Count > 0)
                        {
                            var topHits = vResult.Hits.Where(h => h.IsAuthorized).Take(3).ToList();
                            if (topHits.Count > 0)
                            {
                                string combinedText = string.Join("\n\n", topHits.Select(h => h.Text));
                                string ans = combinedText;
                                bool bypassedLlm = true;
                                if (_llmService != null)
                                {
                                    try
                                    {
                                        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                                        using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                                        {
                                            string grounded = await _llmService.GenerateGroundedAnswerAsync(question, combinedText, null, linked.Token);
                                            if (!string.IsNullOrWhiteSpace(grounded) && !grounded.StartsWith("Lỗi", StringComparison.OrdinalIgnoreCase) && grounded.Length > 10)
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
                                        string sec = !string.IsNullOrWhiteSpace(h.Section) ? $" - Mục {h.Section}" : "";
                                        string ver = !string.IsNullOrWhiteSpace(h.Version) ? $" (v{h.Version})" : "";
                                        return $"{h.Title}{sec}{ver}";
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
                    var sqlSubPlan = new QueryExecutionPlan
                    {
                        Strategy = ExecutionStrategy.SqlTemplate,
                        Domain = plan.Domain,
                        Operation = plan.Operation,
                        Metric = plan.Metric,
                        TargetView = plan.TargetView,
                        SqlStatement = plan.SqlStatement,
                        Parameters = plan.Parameters,
                        RequestedScope = plan.RequestedScope,
                        RequiredCapability = plan.RequiredCapability,
                        IsScalar = plan.IsScalar,
                        ScalarUnit = plan.ScalarUnit,
                        AuthorizationFingerprint = plan.AuthorizationFingerprint
                    };
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
                    }

                    var sqlResult = await sqlTask;
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
                            BypassedLlm = true,
                            HttpStatus = sqlResponse.HttpStatus,
                            AsOf = _clock.UtcNow
                        };
                    }
                    else if (_vectorService != null)
                    {
                        var authorizedHits = vResult?.Hits?.Where(h => h.IsAuthorized).Take(3).ToList() ?? new List<VectorHit>();

                            string combinedFacts = $"Dữ liệu thực tế từ hệ thống:\n{sqlResponse.Answer}";
                            if (authorizedHits.Count > 0)
                            {
                                string policyContext = string.Join("\n\n", authorizedHits.Select(h => h.Text));
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
                                        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                                        using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                                        {
                                            string synthesisPrompt = $"Dựa vào số liệu thực tế và quy định dưới đây, hãy giải thích và trả lời câu hỏi của người dùng:\n{combinedFacts}\n\nQuy định:\n{policyContext}";
                                            string grounded = await _llmService.GenerateGroundedAnswerAsync(question, synthesisPrompt, null, linked.Token);
                                            if (!string.IsNullOrWhiteSpace(grounded) && !grounded.StartsWith("Lỗi", StringComparison.OrdinalIgnoreCase) && grounded.Length > 10)
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
                                    BypassedLlm = bypassedLlm,
                                    HttpStatus = 200,
                                    AsOf = _clock.UtcNow
                                };
                            }
                            else
                            {
                                rendered = new RenderedResponse
                                {
                                    Status = sqlResponse.Status,
                                    Answer = sqlResponse.Answer + "\n\n(Không tìm thấy tài liệu quy chế đối chiếu phù hợp trong cơ sở tri thức).",
                                    SourceProvenance = "Oracle Database (SQL Facts)",
                                    Data = sqlResponse.Data,
                                    TotalRecords = sqlResponse.TotalRecords,
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

                System.Diagnostics.Trace.TraceInformation("[AI_EXEC] ClientRequestId={0}, UserId={1}, Domain={2}, Operation={3}, Capability={4}, LatencyMs={5}, Status={6}, BypassedLlm={7}",
                    result.ClientRequestId, authContext.UserId, plan?.Domain, plan?.Operation, plan?.RequiredCapability, result.ExecutionDurationMs, result.Status, result.BypassedLlm);

                return result;
            }
            catch (AiSourceUnavailableException ex) { result.ErrorCode=ex.ErrorCode; return error("error",ex.ErrorCode=="AI_SETUP_REQUIRED" ? "Nguồn tra cứu AI chưa được thiết lập đầy đủ. Quản trị viên cần hoàn tất cấu hình dữ liệu và phân quyền." : "Nguồn dữ liệu hoặc chính sách AI chưa sẵn sàng. Vui lòng liên hệ quản trị viên.",503); }
            catch (ConversationConflictException) { if (_conversationManager.TryGetSession(authContext.UserId,result.ConversationId,out var current)) result.ConversationVersion=current.Version; return error("error","Phiên hội thoại, quyền truy cập hoặc dữ liệu đã thay đổi. Vui lòng gửi lại câu hỏi.",409); }
            catch (OperationCanceledException) { System.Diagnostics.Trace.TraceWarning("[AI_TIMEOUT] ClientRequestId={0}, UserId={1}, LatencyMs={2}", result.ClientRequestId, authContext.UserId, sw.ElapsedMilliseconds); return error("error","Yêu cầu đã bị hủy hoặc quá thời gian chờ.",504); }
            catch (Exception ex) { System.Diagnostics.Trace.TraceError("AI request {0}: {1}",result.ClientRequestId,ex.GetType().Name); return error("error","Có lỗi khi xử lý yêu cầu. Vui lòng thử lại sau.",500); }
        }

        private static Bu.Services.AI_Services.Vector.VectorSecurityFilter BuildVectorSecurityFilter(AiAuthorizationContext ctx, string capabilityCode)
        {
            var filter = new Bu.Services.AI_Services.Vector.VectorSecurityFilter
            {
                UserId = ctx.UserId,
                CallerEmployeeId = ctx.Manv,
                AllowedCompanyId = ctx.MaCty,
                AllowedDepartmentIds = ctx.AllowedDepartmentIds != null ? new List<int>(ctx.AllowedDepartmentIds) : new List<int>()
            };

            var grants = ctx.ScopeGrants != null
                ? ctx.ScopeGrants.Where(g => string.Equals(g.CapabilityCode, capabilityCode, StringComparison.OrdinalIgnoreCase)
                                          || string.Equals(g.CapabilityCode, "ALL", StringComparison.OrdinalIgnoreCase)).ToList()
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