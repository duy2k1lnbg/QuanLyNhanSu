using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bu.Services.AI_Services.Vector
{
    public class QdrantService : IVectorService
    {
        private readonly ILlmService _llm;
        private static readonly HttpClient SharedClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) }; // Fail fast (5s) to allow IPv6/IPv4 fallback
        private readonly HttpClient _client;
        private readonly string _qdrantUrl;
        public string QdrantUrl => _qdrantUrl;
        public const string DEFAULT_COLLECTION_NAME = "hrms_vectors_v2";
        public const string DEFAULT_KNOWLEDGE_COLLECTION_NAME = "hrms_knowledge_active";
        public const string DEFAULT_EMPLOYEE_COLLECTION_NAME = "hrms_vectors_v2";

        public string KnowledgeCollection
        {
            get
            {
                try
                {
                    string configured = System.Configuration.ConfigurationManager.AppSettings["Qdrant:KnowledgeCollection"];
                    return string.IsNullOrWhiteSpace(configured) ? DEFAULT_KNOWLEDGE_COLLECTION_NAME : configured.Trim();
                }
                catch { return DEFAULT_KNOWLEDGE_COLLECTION_NAME; }
            }
        }

        public string EmployeeCollection
        {
            get
            {
                try
                {
                    string configured = System.Configuration.ConfigurationManager.AppSettings["Qdrant:EmployeeCollection"];
                    if (!string.IsNullOrWhiteSpace(configured)) return configured.Trim();
                    string legacy = System.Configuration.ConfigurationManager.AppSettings["Qdrant:Collection"];
                    return string.IsNullOrWhiteSpace(legacy) ? DEFAULT_EMPLOYEE_COLLECTION_NAME : legacy.Trim();
                }
                catch { return DEFAULT_EMPLOYEE_COLLECTION_NAME; }
            }
        }

        private string CollectionName => EmployeeCollection;
        private string COLLECTION_NAME => CollectionName;
        private const float THRESHOLD = 0.6f; // Giảm threshold để dễ match hơn
        private const int TOPK = 10;

        private bool _isInitialized = false;
        private bool _isOffline = false;

        public static string GeneratePointIdFromLogicalIdentity(string logicalIdentity)
        {
            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes("HRMS_QDRANT_NAMESPACE:" + (logicalIdentity ?? string.Empty)));
                byte[] b = new byte[16];
                Array.Copy(hash, 0, b, 0, 16);
                b[6] = (byte)((b[6] & 0x0F) | 0x50); // version 5
                b[8] = (byte)((b[8] & 0x3F) | 0x80); // RFC 4122 variant
                return string.Format("{0:x2}{1:x2}{2:x2}{3:x2}-{4:x2}{5:x2}-{6:x2}{7:x2}-{8:x2}{9:x2}-{10:x2}{11:x2}{12:x2}{13:x2}{14:x2}{15:x2}",
                    b[0], b[1], b[2], b[3],
                    b[4], b[5],
                    b[6], b[7],
                    b[8], b[9],
                    b[10], b[11], b[12], b[13], b[14], b[15]);
            }
        }

        public static string GeneratePointId(string sourceCode, string entityKey, string visibilityProfile = "PUBLIC", string chunkKey = "0")
        {
            string input = $"{sourceCode}:{entityKey}:{visibilityProfile}:{chunkKey}";
            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes("HRMS_QDRANT_NAMESPACE:" + input));
                byte[] b = new byte[16];
                Array.Copy(hash, 0, b, 0, 16);
                b[6] = (byte)((b[6] & 0x0F) | 0x50); // version 5
                b[8] = (byte)((b[8] & 0x3F) | 0x80); // RFC 4122 variant
                return string.Format("{0:x2}{1:x2}{2:x2}{3:x2}-{4:x2}{5:x2}-{6:x2}{7:x2}-{8:x2}{9:x2}-{10:x2}{11:x2}{12:x2}{13:x2}{14:x2}{15:x2}",
                    b[0], b[1], b[2], b[3],
                    b[4], b[5],
                    b[6], b[7],
                    b[8], b[9],
                    b[10], b[11], b[12], b[13], b[14], b[15]);
            }
        }

        public QdrantService(ILlmService llm = null, HttpClient httpClient = null)
        {
            _llm = llm;
            _client = httpClient ?? SharedClient;
            // Configuration from AppSettings (Zero-Trust, does not touch Oracle HR directly)
            string configUrl = null;
            try
            {
                configUrl = System.Configuration.ConfigurationManager.AppSettings["QdrantUrl"];
            }
            catch { }

            _qdrantUrl = !string.IsNullOrWhiteSpace(configUrl) ? configUrl.TrimEnd('/') : "http://127.0.0.1:6333";
        }

        private async Task EnsureCollectionExistsAsync()
        {
            try
            {
                var res = await _client.GetAsync($"{QdrantUrl}/collections/{COLLECTION_NAME}");
                if (res.IsSuccessStatusCode)
                {
                    _isInitialized = true;
                    _isOffline = false;
                    System.Diagnostics.Debug.WriteLine($"[QDRANT] Connected to {QdrantUrl}/collections/{COLLECTION_NAME} successfully.");
                    return;
                }

                // Lấy vector mẫu từ Ollama để đo kích thước Kích thước (Dimensions)
                var sampleVector = await _llm.GetEmbedding("test");
                if (sampleVector == null || sampleVector.Length == 0)
                {
                    Console.WriteLine("[QDRANT] Không thể lấy vector mẫu từ LLM.");
                    return;
                }

                int vectorSize = sampleVector.Length;

                var body = new
                {
                    vectors = new
                    {
                        size = vectorSize,
                        distance = "Cosine"
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var createRes = await _client.PutAsync($"{QdrantUrl}/collections/{COLLECTION_NAME}", content);

                if (createRes.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[QDRANT] Tạo thành công Collection {COLLECTION_NAME} (Size: {vectorSize}).");
                    _isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QDRANT INIT ERROR]: {ex.Message}");
                _isOffline = true; // Mark as offline so we don't block subsequent requests for 2 seconds every time
            }
        }

        private async Task CheckInitAsync()
        {
            if (!_isInitialized)
            {
                await EnsureCollectionExistsAsync();
            }
        }

        public async Task AddAsync(string text, string tag = "GENERAL", int? employeeId = null, string domain = null, string documentType = null, string companyId = null, int? departmentId = null, string version = "2", string sourceId = null, string title = null, string section = null)
        {
            if (string.Equals(domain, "POLICY", StringComparison.OrdinalIgnoreCase) || string.Equals(tag, "REGULATION", StringComparison.OrdinalIgnoreCase) || string.Equals(tag, "POLICY", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Policy documents require the validated DataDoc ingestion pipeline.");
            if (_isOffline || string.IsNullOrWhiteSpace(text)) return;
            await CheckInitAsync();

            if (_isOffline) return; // double check after init

            var vec = await Bu.Services.AI_Services.Memory.AiCacheCoordinator.Instance.GetOrComputeEmbeddingAsync(
                text,
                EmbeddingModelName,
                async () => await _llm.GetEmbedding(text),
                tag ?? "public");
            if (vec == null) return;

            try
            {
                string resolvedDomain = !string.IsNullOrWhiteSpace(domain) ? domain : (tag == "REGULATION" || tag == "POLICY" ? "POLICY" : (tag == "EMPLOYEE" ? "EMPLOYEE" : tag));
                string resolvedDocType = !string.IsNullOrWhiteSpace(documentType) ? documentType : (resolvedDomain == "POLICY" ? "POLICY_DOC" : (resolvedDomain == "EMPLOYEE" ? "EMPLOYEE_PROFILE" : "GENERAL_DOC"));

                // Dùng UUID v5 xác định theo namespace chuẩn của HRMS
                var pointId = GeneratePointId(tag, employeeId.HasValue ? employeeId.Value.ToString() : (sourceId ?? text.GetHashCode().ToString()));
                var payload = new Dictionary<string, object>
                {
                    { "text", text },
                    { "tag", tag },
                    { "domain", resolvedDomain },
                    { "documentType", resolvedDocType },
                    { "version", version ?? "2" }
                };

                if (employeeId.HasValue) payload["employeeId"] = employeeId.Value;
                if (departmentId.HasValue) payload["departmentId"] = departmentId.Value;
                if (!string.IsNullOrWhiteSpace(companyId)) payload["companyId"] = companyId;
                if (!string.IsNullOrWhiteSpace(sourceId)) payload["sourceId"] = sourceId;
                if (!string.IsNullOrWhiteSpace(title)) payload["title"] = title;
                if (!string.IsNullOrWhiteSpace(section)) payload["section"] = section;

                var body = new
                {
                    points = new[]
                    {
                        new { id = pointId, vector = vec, payload = payload }
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var putRes = await _client.PutAsync($"{QdrantUrl}/collections/{COLLECTION_NAME}/points?wait=true", content);
                if (!putRes.IsSuccessStatusCode)
                {
                    string err = await putRes.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"[Qdrant Add Failed]: {putRes.StatusCode} - {err}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QDRANT ADD ERROR]: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> SearchAsync(string query, string tag = null)
        {
            if (_isOffline || string.IsNullOrWhiteSpace(query)) return new List<string>();
            await CheckInitAsync();

            if (_isOffline) return new List<string>(); // double check after init

            var qVec = await _llm.GetEmbedding(query);
            if (qVec == null) return new List<string>();

            try
            {
                object filter = null;
                if (!string.IsNullOrEmpty(tag))
                {
                    filter = new
                    {
                        must = new[]
                        {
                            new { key = "tag", match = new { value = tag } }
                        }
                    };
                }

                var body = new
                {
                    vector = qVec,
                    limit = TOPK,
                    filter = filter,
                    with_payload = true,
                    score_threshold = THRESHOLD
                };

                var content = new StringContent(JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");
                var res = await _client.PostAsync($"{QdrantUrl}/collections/{COLLECTION_NAME}/points/search", content);

                if (res.IsSuccessStatusCode)
                {
                    var jsonStr = await res.Content.ReadAsStringAsync();
                    var jsonObj = JObject.Parse(jsonStr);
                    var results = new List<string>();

                    var resultArr = jsonObj["result"] as JArray;
                    if (resultArr != null)
                    {
                        foreach (var item in resultArr)
                        {
                            var payloadText = item["payload"]?["text"]?.ToString();
                            if (!string.IsNullOrEmpty(payloadText))
                            {
                                results.Add(payloadText);
                            }
                        }
                    }
                    return results;
                }
            }
            catch (Exception)
            {
            }
            return new List<string>();
        }

        private string EmbeddingModelName => (_llm as OllamaService)?.EmbeddingModelName
            ?? System.Configuration.ConfigurationManager.AppSettings["Ollama:EmbeddingModel"] ?? OllamaService.DEFAULT_EMBEDDING_MODEL;

        private static JObject Match(string key, string value) => JObject.FromObject(new { key, match = new { value } });
        private static JObject Any(string key, object values) => JObject.FromObject(new { key, match = new { any = values } });
        private static JObject Or(params JObject[] filters) => new JObject { ["should"] = new JArray(filters) };
        private static JObject And(params JObject[] filters) => new JObject { ["must"] = new JArray(filters) };
        public static JObject BuildPolicyFilter(VectorSecurityFilter security)
        {
            if (security == null) throw new ArgumentNullException(nameof(security));
            var tenants = new List<string> { "GLOBAL" };
            if (!string.IsNullOrWhiteSpace(security.AllowedCompanyId)) tenants.Add(security.AllowedCompanyId.Trim());
            var aclMatches = new List<JObject>();
            var roles = security.AllowedRoleIds ?? new List<string>();
            var departments = (security.AllowedDepartmentIds ?? new List<int>()).Where(d => d > 0).ToArray();
            if (roles.Count > 0) aclMatches.Add(Any("allowedRoleIds", roles));
            if (departments.Length > 0) aclMatches.Add(Any("allowedDepartmentIds", departments));
            JObject hasAcl = Or(
                new JObject { ["must_not"] = new JArray(JObject.FromObject(new { is_empty = new { key = "allowedRoleIds" } })) },
                new JObject { ["must_not"] = new JArray(JObject.FromObject(new { is_empty = new { key = "allowedDepartmentIds" } })) });
            JObject authorizedAcl = security.IsAdmin && security.EffectiveScope == "ALL" ? hasAcl
                : aclMatches.Count > 0 ? And(hasAcl, Or(aclMatches.ToArray())) : Match("accessProfile", "__NO_ACCESS__");
            var asOf = security.AsOf.ToUniversalTime().ToString("o", System.Globalization.CultureInfo.InvariantCulture);
            return And(Match("domain", "POLICY"), Match("tag", "REGULATION"), Match("status", "ACTIVE_OFFICIAL"),
                Match("verificationStatus", "OFFICIAL_VERIFIED"), Any("tenantId", tenants),
                JObject.FromObject(new { key = "appliesToCurrentCompany", match = new { value = true } }),
                JObject.FromObject(new { key = "effectiveFrom", range = new { lte = asOf } }),
                Or(JObject.FromObject(new { is_empty = new { key = "effectiveTo" } }),
                    JObject.FromObject(new { key = "effectiveTo", range = new { gt = asOf } })),
                Or(Match("accessProfile", "PUBLIC_INTERNAL"), And(Any("accessProfile", new[] { "ROLE_RESTRICTED", "CONFIDENTIAL" }), authorizedAcl)),
                Or(Match("applicabilityMode", "ALL"), And(Match("applicabilityMode", "RESTRICTED"), authorizedAcl)));
        }

        public static bool IsPolicyPayloadAuthorized(JToken payload, VectorSecurityFilter security)
        {
            if (payload == null || security == null || security.UserId.GetValueOrDefault() <= 0 || (security.DenyCodes?.Count ?? 0) > 0) return false;
            if (payload["domain"]?.ToString() != "POLICY" || payload["tag"]?.ToString() != "REGULATION"
                || payload["status"]?.ToString() != "ACTIVE_OFFICIAL" || payload["verificationStatus"]?.ToString() != "OFFICIAL_VERIFIED"
                || payload["appliesToCurrentCompany"]?.Type != JTokenType.Boolean || payload["appliesToCurrentCompany"].Value<bool>() != true) return false;
            var tenant = payload["tenantId"]?.ToString();
            if (tenant != "GLOBAL" && (string.IsNullOrWhiteSpace(security.AllowedCompanyId) || !string.Equals(tenant, security.AllowedCompanyId.Trim(), StringComparison.Ordinal))) return false;
            if (tenant == "GLOBAL" && (payload["sourceKind"]?.ToString() != "VIETNAM_LAW" || !string.IsNullOrWhiteSpace(payload["companyId"]?.ToString()))) return false;
            if (tenant != "GLOBAL" && payload["companyId"]?.ToString() != tenant) return false;
            DateTimeOffset from, to;
            if (!DateTimeOffset.TryParse(payload["effectiveFrom"]?.ToString(), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out from) || from > security.AsOf) return false;
            var end = payload["effectiveTo"]?.ToString();
            if (!string.IsNullOrWhiteSpace(end) && (!DateTimeOffset.TryParse(end, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out to) || to <= security.AsOf)) return false;
            var roles = payload["allowedRoleIds"] as JArray;
            var departments = payload["allowedDepartmentIds"] as JArray;
            if ((roles?.Any(r => r.Type != JTokenType.String || string.IsNullOrWhiteSpace(r.Value<string>())) ?? false)
                || (departments?.Any(d => d.Type != JTokenType.Integer || d.Value<int>() <= 0) ?? false)) return false;
            bool hasAcl = (roles?.Count ?? 0) + (departments?.Count ?? 0) > 0;
            bool aclMatches = hasAcl && ((security.IsAdmin && security.EffectiveScope == "ALL")
                || (roles?.Any(r => (security.AllowedRoleIds ?? new List<string>()).Contains(r.ToString())) ?? false)
                || (departments?.Any(d => d.Type == JTokenType.Integer && (security.AllowedDepartmentIds ?? new List<int>()).Contains(d.Value<int>())) ?? false));
            string profile = payload["accessProfile"]?.ToString();
            if (profile != "PUBLIC_INTERNAL" && (!(profile == "ROLE_RESTRICTED" || profile == "CONFIDENTIAL") || !aclMatches)) return false;
            string mode = payload["applicabilityMode"]?.ToString();
            return mode == "ALL" || (mode == "RESTRICTED" && aclMatches);
        }

        private async Task ExpandPolicyEvidenceAsync(VectorSearchResult result, VectorSecurityFilter security, string digest, System.Threading.CancellationToken token)
        {
            var selected = result.Hits.Where(h => h.IsAuthorized).ToList();
            var requests = new Dictionary<string, VectorHit>(StringComparer.Ordinal);
            foreach (var hit in selected)
            {
                if (hit.TotalChunks > 1) requests[hit.TenantId + "|" + hit.SourceId + "|" + hit.Version + "|" + hit.ClauseId] = hit;
                foreach (var clause in (hit.RelatedClauseIds ?? new List<string>()).Concat(new[] { hit.ParentClauseId }).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct())
                    if (clause != hit.ClauseId)
                        requests[hit.TenantId + "|" + hit.SourceId + "|" + hit.Version + "|" + clause] = new VectorHit { TenantId=hit.TenantId, SourceId=hit.SourceId, Version=hit.Version, ClauseId=clause, Score=hit.Score };
            }
            if (requests.Count > 20) throw new InvalidOperationException("Too many linked policy clauses; narrow the question.");
            foreach (var wanted in requests.Values)
            {
                var filter = BuildPolicyFilter(security);
                var must = (JArray)filter["must"];
                must.Add(Match("tenantId", wanted.TenantId)); must.Add(Match("sourceId", wanted.SourceId));
                must.Add(Match("version", wanted.Version)); must.Add(Match("clauseId", wanted.ClauseId));
                var body = new JObject { ["filter"]=filter, ["limit"]=100, ["with_payload"]=true, ["with_vector"]=false };
                var siblings = new List<VectorHit>();
                do
                {
                    using (var content = new StringContent(body.ToString(), Encoding.UTF8, "application/json"))
                    using (var response = await _client.PostAsync($"{QdrantUrl}/collections/{KnowledgeCollection}/points/scroll", content, token))
                    {
                        response.EnsureSuccessStatusCode();
                        var page = JObject.Parse(await response.Content.ReadAsStringAsync())["result"];
                        foreach (var point in page["points"] ?? new JArray())
                        {
                            var payload = point["payload"];
                            if (payload?["tenantId"]?.ToString() != wanted.TenantId || payload?["sourceId"]?.ToString() != wanted.SourceId
                                || payload?["version"]?.ToString() != wanted.Version || payload?["clauseId"]?.ToString() != wanted.ClauseId) continue;
                            if (!IsPolicyPayloadAuthorized(payload, security) || !OllamaService.SameModel(EmbeddingModelName,payload?["embeddingModel"]?.ToString())
                                || (digest != null && payload?["embeddingDigest"]?.ToString() != digest.Substring(digest.LastIndexOf('|')+1))) continue;
                            siblings.Add(new VectorHit {
                                PointId=point["id"]?.ToString(), Score=wanted.Score, IsAuthorized=true, TenantId=payload["tenantId"]?.ToString(),
                                SourceId=payload["sourceId"]?.ToString(), Version=payload["version"]?.ToString(), ClauseId=payload["clauseId"]?.ToString(),
                                Section=payload["section"]?.ToString(), Domain="POLICY", DocumentType=payload["documentType"]?.ToString(),
                                Text=payload["text"]?.ToString(), Title=payload["title"]?.ToString(), DocumentTitle=payload["documentTitle"]?.ToString(),
                                ChunkIndex=payload["chunkIndex"]?.Value<int>() ?? 0, TotalChunks=payload["totalChunks"]?.Value<int>() ?? 1,
                                SourceUrls=payload["sourceUrls"]?.ToObject<List<string>>() ?? new List<string>() });
                        }
                        if (siblings.Count > 1000) throw new InvalidOperationException("Policy clause exceeds retrieval limit.");
                        var offset=page["next_page_offset"];
                        if (offset==null || offset.Type==JTokenType.Null) break;
                        body["offset"]=offset.DeepClone();
                    }
                } while (true);
                if (siblings.Count==0 || siblings.Any(h => h.TotalChunks != siblings[0].TotalChunks)
                    || siblings.Select(h => h.ChunkIndex).Distinct().Count()!=siblings[0].TotalChunks
                    || !siblings.Select(h => h.ChunkIndex).OrderBy(i=>i).SequenceEqual(Enumerable.Range(0,siblings[0].TotalChunks)))
                    throw new InvalidOperationException("Cannot assemble a complete authorized policy clause.");
                foreach (var sibling in siblings.OrderBy(h=>h.ChunkIndex))
                    if (!result.Hits.Any(h=>h.PointId==sibling.PointId)) result.Hits.Add(sibling);
            }
        }

        public async Task<VectorSearchResult> SearchScopedAsync(string query, VectorBusinessFilter businessFilter, VectorSecurityFilter securityFilter, float[] precomputedEmbedding = null, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool isPolicySearch = string.Equals(businessFilter?.Domain, "POLICY", StringComparison.OrdinalIgnoreCase)
                || string.Equals(businessFilter?.Tag, "REGULATION", StringComparison.OrdinalIgnoreCase);
            string targetCollection = isPolicySearch ? KnowledgeCollection : EmployeeCollection;
            if (isPolicySearch && securityFilter?.UserId.GetValueOrDefault() <= 0)
                return new VectorSearchResult { Status = VectorSearchStatus.Forbidden, ErrorMessage = "Policy search requires an authenticated actor" };
            // Fail closed: Missing or invalid security filter returns empty/fail
            if (securityFilter == null)
            {
                return new VectorSearchResult
                {
                    Status = VectorSearchStatus.Forbidden,
                    ErrorMessage = "Missing valid security context",
                    SecurityFilterApplied = "fail_closed: missing valid security context"
                };
            }

            // Explicit deny check
            if (securityFilter.DenyCodes != null && securityFilter.DenyCodes.Count > 0)
            {
                return new VectorSearchResult
                {
                    Status = VectorSearchStatus.Forbidden,
                    ErrorMessage = "Explicit deny applied for capability",
                    SecurityFilterApplied = "fail_closed: explicit deny applied"
                };
            }

            if (!isPolicySearch && string.Equals(securityFilter.EffectiveScope, "COMPANY", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(securityFilter.AllowedCompanyId))
            {
                return new VectorSearchResult {
                    Status = VectorSearchStatus.Forbidden,
                    ErrorMessage = "COMPANY scope requires an unambiguous company id",
                    SecurityFilterApplied = "fail_closed: COMPANY scope requires a company id"
                };
            }

            // Non-admin without any valid scope (no departments, no caller ID, no company) fails closed
            if (!isPolicySearch && !securityFilter.IsAdmin && (securityFilter.AllowedDepartmentIds == null || securityFilter.AllowedDepartmentIds.Count == 0) && !securityFilter.CallerEmployeeId.HasValue && string.IsNullOrWhiteSpace(securityFilter.AllowedCompanyId))
            {
                return new VectorSearchResult
                {
                    Status = VectorSearchStatus.Forbidden,
                    ErrorMessage = "Missing valid security scope",
                    SecurityFilterApplied = "fail_closed: missing valid security scope"
                };
            }

            // Target Employee ID outside caller's scope for non-admin without dept access:
            if (!isPolicySearch && !securityFilter.IsAdmin && securityFilter.TargetEmployeeId.HasValue && securityFilter.CallerEmployeeId.HasValue &&
                securityFilter.TargetEmployeeId.Value != securityFilter.CallerEmployeeId.Value &&
                (securityFilter.AllowedDepartmentIds == null || securityFilter.AllowedDepartmentIds.Count == 0))
            {
                return new VectorSearchResult
                {
                    Status = VectorSearchStatus.Forbidden,
                    ErrorMessage = "Target employee outside caller scope",
                    SecurityFilterApplied = "fail_closed: target outside caller scope"
                };
            }

            if (_llm == null && precomputedEmbedding == null)
            {
                return new VectorSearchResult
                {
                    Status = VectorSearchStatus.Unavailable,
                    ErrorMessage = "Vector embedding service is not configured",
                    SecurityFilterApplied = "disabled: vector search is not configured"
                };
            }

            if (_isOffline && !isPolicySearch)
            {
                return new VectorSearchResult
                {
                    Status = VectorSearchStatus.Unavailable,
                    ErrorMessage = "Qdrant vector engine is currently offline"
                };
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return new VectorSearchResult
                {
                    Status = VectorSearchStatus.NoMatch,
                    ErrorMessage = "Query is empty"
                };
            }

            // Reader path: verify collection exists with GET, never call PUT to create collection!
            try
            {
                var checkRes = await _client.GetAsync($"{QdrantUrl}/collections/{targetCollection}", cancellationToken);
                if (!checkRes.IsSuccessStatusCode)
                {
                    return new VectorSearchResult
                    {
                        Status = VectorSearchStatus.Unavailable,
                        ErrorMessage = $"Collection '{targetCollection}' does not exist or is not ready ({checkRes.StatusCode})",
                        SecurityFilterApplied = $"unavailable: collection '{targetCollection}' does not exist or is not ready ({checkRes.StatusCode})"
                    };
                }
            }
            catch (Exception ex)
            {
                return new VectorSearchResult
                {
                    Status = VectorSearchStatus.Unavailable,
                    ErrorMessage = $"Cannot contact Qdrant ({ex.Message})",
                    SecurityFilterApplied = $"unavailable: error contacting Qdrant ({ex.Message})"
                };
            }

            string modelDigest = null;
            if (isPolicySearch && _llm is OllamaService ollama)
            {
                try { modelDigest = await ollama.GetEmbeddingModelDigestAsync(cancellationToken); }
                catch (Exception ex) {
                    if (cancellationToken.IsCancellationRequested) throw;
                    return new VectorSearchResult { Status = VectorSearchStatus.Unavailable, ErrorMessage = "Cannot verify embedding model: " + ex.GetType().Name };
                }
            }
            var qVec = precomputedEmbedding ?? await Bu.Services.AI_Services.Memory.AiCacheCoordinator.Instance.GetOrComputeEmbeddingAsync(
                query, EmbeddingModelName + "|" + (modelDigest ?? QdrantUrl),
                async () => await _llm.GetEmbedding(query, cancellationToken), "query");
            if (isPolicySearch && qVec != null && (qVec.Length != 1024 || qVec.Any(v => float.IsNaN(v) || float.IsInfinity(v))))
                return new VectorSearchResult { Status = VectorSearchStatus.InvalidIndex, ErrorMessage = "Knowledge index requires finite 1024-dimensional vectors" };
            if (qVec == null)
            {
                return new VectorSearchResult
                {
                    Status = VectorSearchStatus.Unavailable,
                    ErrorMessage = "Failed to calculate query vector embedding"
                };
            }

            try
            {

                var mustConditions = new List<object>();
                var shouldConditions = new List<object>();

                if (isPolicySearch)
                {
                    mustConditions.Add(new { key = "domain", match = new { value = "POLICY" } });
                    mustConditions.Add(new { key = "tag", match = new { value = "REGULATION" } });
                    mustConditions.Add(new { key = "status", match = new { value = "ACTIVE_OFFICIAL" } });

                    // Tenant isolation: Cho phép đọc GLOBAL và công ty của người dùng (nếu có)
                    shouldConditions.Add(new { key = "tenantId", match = new { value = "GLOBAL" } });
                    if (!string.IsNullOrWhiteSpace(securityFilter.AllowedCompanyId))
                    {
                        shouldConditions.Add(new { key = "tenantId", match = new { value = securityFilter.AllowedCompanyId.Trim() } });
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(businessFilter?.Tag))
                    {
                        mustConditions.Add(new { key = "tag", match = new { value = businessFilter.Tag } });
                    }
                    if (!string.IsNullOrEmpty(businessFilter?.Domain))
                    {
                        mustConditions.Add(new { key = "domain", match = new { value = businessFilter.Domain } });
                    }
                    if (!string.IsNullOrEmpty(businessFilter?.DocumentType))
                    {
                        mustConditions.Add(new { key = "documentType", match = new { value = businessFilter.DocumentType } });
                    }

                    // Company scope cho Employee search
                    if (!string.IsNullOrEmpty(securityFilter.AllowedCompanyId))
                    {
                        if (int.TryParse(securityFilter.AllowedCompanyId, out int compIdInt))
                        {
                            mustConditions.Add(new { key = "companyId", match = new { value = compIdInt } });
                        }
                        else
                        {
                            mustConditions.Add(new { key = "companyId", match = new { value = securityFilter.AllowedCompanyId } });
                        }
                    }

                    // Scope-based enforcement cho Employee search
                    if (string.Equals(securityFilter.EffectiveScope, "SELF", StringComparison.OrdinalIgnoreCase))
                    {
                        if (securityFilter.CallerEmployeeId.HasValue)
                        {
                            mustConditions.Add(new { key = "employeeId", match = new { value = securityFilter.CallerEmployeeId.Value } });
                        }
                        else
                        {
                            return new VectorSearchResult
                            {
                                Status = VectorSearchStatus.Forbidden,
                                ErrorMessage = "SELF scope requires caller employee id",
                                SecurityFilterApplied = "fail_closed: SELF scope requires caller employee id"
                            };
                        }
                    }
                    else if (string.Equals(securityFilter.EffectiveScope, "DEPARTMENT", StringComparison.OrdinalIgnoreCase))
                    {
                        var validDepts = securityFilter.AllowedDepartmentIds?.Where(d => d > 0).ToList();
                        if (validDepts != null && validDepts.Count > 0)
                        {
                            mustConditions.Add(new { key = "departmentId", match = new { any = validDepts } });
                        }
                        else
                        {
                            return new VectorSearchResult
                            {
                                Status = VectorSearchStatus.Forbidden,
                                ErrorMessage = "DEPARTMENT scope requires valid positive department ids",
                                SecurityFilterApplied = "fail_closed: DEPARTMENT scope requires valid positive department ids"
                            };
                        }
                    }
                    else if (securityFilter.TargetEmployeeId.HasValue)
                    {
                        mustConditions.Add(new { key = "employeeId", match = new { value = securityFilter.TargetEmployeeId.Value } });
                    }
                    else if (!securityFilter.IsAdmin && securityFilter.CallerEmployeeId.HasValue && (securityFilter.AllowedDepartmentIds == null || securityFilter.AllowedDepartmentIds.Count == 0))
                    {
                        // Fallback SELF scope
                        mustConditions.Add(new { key = "employeeId", match = new { value = securityFilter.CallerEmployeeId.Value } });
                    }
                    else if (!securityFilter.IsAdmin && securityFilter.AllowedDepartmentIds != null && securityFilter.AllowedDepartmentIds.Count > 0)
                    {
                        var validDepts = securityFilter.AllowedDepartmentIds.Where(d => d > 0).ToList();
                        if (validDepts.Count > 0)
                        {
                            mustConditions.Add(new { key = "departmentId", match = new { any = validDepts } });
                        }
                    }
                }

                object filterObj = null;
                if (mustConditions.Count > 0 && shouldConditions.Count > 0)
                {
                    filterObj = new { must = mustConditions, should = shouldConditions };
                }
                else if (mustConditions.Count > 0)
                {
                    filterObj = new { must = mustConditions };
                }
                else if (shouldConditions.Count > 0)
                {
                    filterObj = new { should = shouldConditions };
                }

                var body = new
                {
                    vector = qVec,
                    limit = TOPK,
                    filter = isPolicySearch ? (object)BuildPolicyFilter(securityFilter) : filterObj,
                    with_payload = true,
                    score_threshold = THRESHOLD
                };

                var content = new StringContent(JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");
                AiRequestTrace.Record("QDRANT_REQUEST", "Gửi vector search với filter quyền", new { topK = TOPK, scoreThreshold = THRESHOLD, effectiveScope = securityFilter.EffectiveScope, collection = targetCollection }, new { collection = targetCollection, filter = body.filter });
                var res = await _client.PostAsync($"{QdrantUrl}/collections/{targetCollection}/points/search", content, cancellationToken);
                AiRequestTrace.Record("QDRANT_HTTP", "Nhận HTTP từ Qdrant", new { httpStatus = (int)res.StatusCode });

                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized || res.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return new VectorSearchResult
                    {
                        Status = VectorSearchStatus.Forbidden,
                        ErrorMessage = $"Qdrant vector store access denied: {res.StatusCode}",
                        SecurityFilterApplied = "forbidden: qdrant http access denied"
                    };
                }
                if ((int)res.StatusCode >= 500)
                {
                    return new VectorSearchResult
                    {
                        Status = VectorSearchStatus.Unavailable,
                        ErrorMessage = $"Qdrant vector store server error: {res.StatusCode}",
                        SecurityFilterApplied = "unavailable: server error"
                    };
                }

                if (res.IsSuccessStatusCode)
                {
                    var jsonStr = await res.Content.ReadAsStringAsync();
                    var jsonObj = JObject.Parse(jsonStr);
                    var result = new VectorSearchResult { SecurityFilterApplied = "active_acl" };
                    var resultArr = jsonObj["result"] as JArray;
                    if (resultArr != null)
                    {
                        foreach (var item in resultArr)
                        {
                            var payload = item["payload"];
                            int? hitEmpId = payload?["employeeId"]?.Value<int?>();
                            int? hitDeptId = payload?["departmentId"]?.Value<int?>();
                            string hitCompId = payload?["companyId"]?.ToString();
                            string hitTag = payload?["tag"]?.ToString();
                            string hitDomain = payload?["domain"]?.ToString();

                            // Evaluate post-retrieval ACL on hit payload before setting IsAuthorized
                            bool isHitAuthorized = true;

                            if (isPolicySearch)
                            {
                                isHitAuthorized = IsPolicyPayloadAuthorized(payload, securityFilter)
                                    && OllamaService.SameModel(EmbeddingModelName, payload?["embeddingModel"]?.ToString())
                                    && (modelDigest == null || payload?["embeddingDigest"]?.ToString() == modelDigest.Substring(modelDigest.LastIndexOf('|') + 1));
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(securityFilter.AllowedCompanyId) && !string.IsNullOrEmpty(hitCompId) && !string.Equals(hitCompId, securityFilter.AllowedCompanyId, StringComparison.OrdinalIgnoreCase))
                                {
                                    isHitAuthorized = false;
                                }

                                if (string.Equals(securityFilter.EffectiveScope, "SELF", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!hitEmpId.HasValue || hitEmpId.Value != securityFilter.CallerEmployeeId.GetValueOrDefault())
                                    {
                                        isHitAuthorized = false;
                                    }
                                }
                                else if (string.Equals(securityFilter.EffectiveScope, "DEPARTMENT", StringComparison.OrdinalIgnoreCase))
                                {
                                    var validDepts = securityFilter.AllowedDepartmentIds?.Where(d => d > 0).ToList() ?? new List<int>();
                                    if (!hitDeptId.HasValue || hitDeptId.Value <= 0 || !validDepts.Contains(hitDeptId.Value))
                                    {
                                        isHitAuthorized = false; // Exclude departmentId = 0 (unassigned)
                                    }
                                }
                                else if (!securityFilter.IsAdmin && securityFilter.CallerEmployeeId.HasValue && (securityFilter.AllowedDepartmentIds == null || securityFilter.AllowedDepartmentIds.Count == 0))
                                {
                                    if (hitEmpId.HasValue && hitEmpId.Value != securityFilter.CallerEmployeeId.Value)
                                    {
                                        isHitAuthorized = false;
                                    }
                                }
                                else if (!securityFilter.IsAdmin && securityFilter.AllowedDepartmentIds != null && securityFilter.AllowedDepartmentIds.Count > 0 && hitDeptId.HasValue)
                                {
                                    if (hitDeptId.Value <= 0 || !securityFilter.AllowedDepartmentIds.Contains(hitDeptId.Value))
                                    {
                                        isHitAuthorized = false;
                                    }
                                }
                            }

                            result.Hits.Add(new VectorHit
                            {
                                PointId = item["id"]?.ToString(),
                                Score = item["score"]?.Value<float>() ?? 0f,
                                Text = payload?["text"]?.ToString(),
                                EmployeeId = hitEmpId,
                                DepartmentId = hitDeptId,
                                CompanyId = hitCompId,
                                Domain = hitDomain ?? hitTag,
                                DocumentType = payload?["documentType"]?.ToString(),
                                Version = payload?["version"]?.ToString() ?? "2",
                                SourceId = payload?["sourceId"]?.ToString() ?? (hitEmpId.HasValue ? $"EMP_{hitEmpId}" : null),
                                Title = payload?["title"]?.ToString(),
                                Section = payload?["section"]?.ToString(),
                                UpdatedAt = payload?["updatedAt"]?.ToString(),
                                TenantId = payload?["tenantId"]?.ToString(),
                                ClauseId = payload?["clauseId"]?.ToString() ?? payload?["section"]?.ToString(),
                                Status = payload?["status"]?.ToString(),
                                AccessProfile = payload?["accessProfile"]?.ToString(),
                                DocumentTitle = payload?["documentTitle"]?.ToString(),
                                EffectiveFrom = payload?["effectiveFrom"]?.ToString(),
                                EffectiveTo = payload?["effectiveTo"]?.ToString(),
                                ChunkId = payload?["chunkId"]?.ToString(),
                                ChunkIndex = payload?["chunkIndex"]?.Value<int>() ?? 0,
                                TotalChunks = payload?["totalChunks"]?.Value<int>() ?? 1,
                                ParentClauseId = payload?["parentClauseId"]?.ToString(),
                                RelatedClauseIds = payload?["relatedClauseIds"]?.ToObject<List<string>>() ?? new List<string>(),
                                SourceUrls = payload?["sourceUrls"]?.ToObject<List<string>>() ?? new List<string>(),
                                IsAuthorized = isHitAuthorized
                            });
                        }
                    }
                    if (isPolicySearch) await ExpandPolicyEvidenceAsync(result, securityFilter, modelDigest, cancellationToken);
                    var authorizedCount = result.Hits.Count(h => h.IsAuthorized);
                    result.TotalFound = authorizedCount;
                    result.Status = authorizedCount > 0 ? VectorSearchStatus.Success : VectorSearchStatus.NoMatch;
                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                return new VectorSearchResult
                {
                    Status = cancellationToken.IsCancellationRequested ? VectorSearchStatus.Cancelled : VectorSearchStatus.Timeout,
                    ErrorMessage = cancellationToken.IsCancellationRequested ? "Yêu cầu tra cứu vector đã bị hủy." : "Tra cứu vector quá thời gian chờ."
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QDRANT SCOPED SEARCH ERROR]: {ex.Message}");
                return new VectorSearchResult
                {
                    Status = VectorSearchStatus.Unavailable,
                    ErrorMessage = ex.Message,
                    SecurityFilterApplied = $"unavailable: {ex.Message}"
                };
            }
            return new VectorSearchResult { Status = VectorSearchStatus.NoMatch };
        }

        public async Task RemoveByEmployeeIdAsync(int manv)
        {
            await CheckInitAsync();
            try
            {
                var body = new
                {
                    filter = new
                    {
                        must = new[]
                        {
                            new { key = "employeeId", match = new { value = manv } }
                        }
                    }
                };

                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var delRes = await _client.PostAsync($"{QdrantUrl}/collections/{COLLECTION_NAME}/points/delete", content);
                if (!delRes.IsSuccessStatusCode)
                {
                    string err = await delRes.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"[QDRANT REMOVE FAILED]: {delRes.StatusCode} - {err}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QDRANT REMOVE ERROR]: {ex.Message}");
                throw;
            }
        }

        public async Task SyncEmployeeDataAsync(int manv)
        {
            try
            {
                using (var db = new DA.MyEntities())
                {
                    var emp = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == manv && x.DATHOIVIEC != 1);
                    if (emp != null)
                    {
                        string deptName = emp.TB_PHONGBAN?.TENPB ?? "Chưa phân bổ";
                        string chucVuName = emp.TB_CHUCVU?.TENCV ?? "Chưa phân bổ";
                        string boPhanName = emp.TB_BOPHAN?.TENBP ?? "Chưa phân bổ";
                        string text = $"Nhân viên {emp.HOTEN} (Mã NV: {emp.MANV}), thuộc phòng ban {deptName}, chức vụ {chucVuName}, bộ phận {boPhanName}.";
                        int? deptId = emp.IDPB.HasValue ? (int?)Convert.ToInt32(emp.IDPB.Value) : null;
                        string compId = emp.IDCTY.HasValue ? emp.IDCTY.Value.ToString() : null;

                        await AddAsync(
                            text: text,
                            tag: "EMPLOYEE",
                            employeeId: (int)emp.MANV,
                            domain: "EMPLOYEE",
                            documentType: "EMPLOYEE_PROFILE",
                            companyId: compId,
                            departmentId: deptId,
                            version: "2",
                            sourceId: $"EMP_{emp.MANV}",
                            title: emp.HOTEN);
                    }
                    else
                    {
                        await RemoveByEmployeeIdAsync(manv);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Vector Sync ERROR] Employee {manv}: {ex.Message}");
                throw;
            }
        }

        public async Task EnsurePayloadIndexesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            var fields = new[]
            {
                new { name = "tag", schema = "keyword" },
                new { name = "departmentId", schema = "integer" },
                new { name = "companyId", schema = "integer" },
                new { name = "employeeId", schema = "integer" },
                new { name = "domain", schema = "keyword" },
                new { name = "documentType", schema = "keyword" },
                new { name = "visibility_profile", schema = "keyword" }
            };

            foreach (var f in fields)
            {
                var body = new { field_name = f.name, field_schema = f.schema };
                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                await _client.PutAsync($"{QdrantUrl}/collections/{COLLECTION_NAME}/index", content, cancellationToken);
            }
        }

        // Interface Fallbacks cho gọi đồng bộ nếu Interface bắt buộc
        public void Add(string text, string tag = "GENERAL") => Task.Run(() => AddAsync(text, tag)).Wait();
        public void Add(string text, string tag, int? employeeId) => Task.Run(() => AddAsync(text, tag, employeeId)).Wait();
        public List<string> Search(string query, string tag = null)
        {
            try
            {
                var task = Task.Run(() => SearchAsync(query, tag));
                if (task.Wait(TimeSpan.FromMilliseconds(2500)))
                {
                    return task.Result ?? new List<string>();
                }
                return new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
        public void RemoveByEmployeeId(int manv) => Task.Run(() => RemoveByEmployeeIdAsync(manv)).Wait();
        public void SyncEmployeeData(int manv) => Task.Run(() => SyncEmployeeDataAsync(manv)).Wait();
        public void Clear()
        {
            // Bảo vệ an toàn: Không xóa collection nền tảng đang chạy
            System.Diagnostics.Debug.WriteLine("[QDRANT] Clear() được gọi nhưng đã được bảo vệ để không xóa collection.");
        }
    }
}
