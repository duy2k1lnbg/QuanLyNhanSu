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
        private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) }; // Fail fast (5s) to allow IPv6/IPv4 fallback
        private readonly string _qdrantUrl;
        public string QdrantUrl => _qdrantUrl;
        public const string DEFAULT_COLLECTION_NAME = "hrms_vectors_v2";
        private string CollectionName
        {
            get
            {
                try
                {
                    string configuredCollection = System.Configuration.ConfigurationManager.AppSettings["Qdrant:Collection"];
                    return string.IsNullOrWhiteSpace(configuredCollection) ? DEFAULT_COLLECTION_NAME : configuredCollection.Trim();
                }
                catch
                {
                    return DEFAULT_COLLECTION_NAME;
                }
            }
        }
        private string COLLECTION_NAME => CollectionName;
        private const float THRESHOLD = 0.6f; // Giảm threshold để dễ match hơn
        private const int TOPK = 10;

        private bool _isInitialized = false;
        private bool _isOffline = false;

        public static string GeneratePointId(string sourceCode, string entityKey, string visibilityProfile = "PUBLIC", string chunkKey = "0")
        {
            string input = $"{sourceCode}:{entityKey}:{visibilityProfile}:{chunkKey}";
            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes("HRMS_QDRANT_NAMESPACE:" + input));
                byte[] guidBytes = new byte[16];
                Array.Copy(hash, 0, guidBytes, 0, 16);
                guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | (5 << 4)); // version 5
                guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80); // RFC 4122
                return new Guid(guidBytes).ToString("D");
            }
        }

        public QdrantService(ILlmService llm = null)
        {
            _llm = llm;
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

        public async Task AddAsync(string text, string tag = "GENERAL", int? employeeId = null)
        {
            if (_isOffline || string.IsNullOrWhiteSpace(text)) return;
            await CheckInitAsync();

            if (_isOffline) return; // double check after init

            var vec = await _llm.GetEmbedding(text);
            if (vec == null) return;

            try
            {
                // Dùng UUID v5 xác định theo namespace chuẩn của HRMS
                var pointId = GeneratePointId(tag, employeeId.HasValue ? employeeId.Value.ToString() : text.GetHashCode().ToString());
                var payload = new Dictionary<string, object>
                {
                    { "text", text },
                    { "tag", tag }
                };

                if (employeeId.HasValue) payload["employeeId"] = employeeId.Value;

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

        public async Task<VectorSearchResult> SearchScopedAsync(string query, VectorBusinessFilter businessFilter, VectorSecurityFilter securityFilter, float[] precomputedEmbedding = null, System.Threading.CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Fail closed: Missing or invalid security filter returns empty/fail
            if (securityFilter == null)
            {
                return new VectorSearchResult { SecurityFilterApplied = "fail_closed: missing valid security context" };
            }

            // Explicit deny check
            if (securityFilter.DenyCodes != null && securityFilter.DenyCodes.Count > 0)
            {
                return new VectorSearchResult { SecurityFilterApplied = "fail_closed: explicit deny applied" };
            }

            // Non-admin without any valid scope (no departments, no caller ID, no company) fails closed
            if (!securityFilter.IsAdmin && (securityFilter.AllowedDepartmentIds == null || securityFilter.AllowedDepartmentIds.Count == 0) && !securityFilter.CallerEmployeeId.HasValue && string.IsNullOrWhiteSpace(securityFilter.AllowedCompanyId))
            {
                return new VectorSearchResult { SecurityFilterApplied = "fail_closed: missing valid security scope" };
            }

            // Target Employee ID outside caller's scope for non-admin without dept access:
            if (!securityFilter.IsAdmin && securityFilter.TargetEmployeeId.HasValue && securityFilter.CallerEmployeeId.HasValue &&
                securityFilter.TargetEmployeeId.Value != securityFilter.CallerEmployeeId.Value &&
                (securityFilter.AllowedDepartmentIds == null || securityFilter.AllowedDepartmentIds.Count == 0))
            {
                return new VectorSearchResult { SecurityFilterApplied = "fail_closed: target outside caller scope" };
            }

            if (_llm == null)
            {
                return new VectorSearchResult { SecurityFilterApplied = "disabled: vector search is not configured" };
            }

            if (_isOffline || string.IsNullOrWhiteSpace(query)) return new VectorSearchResult();

            // Reader path: verify collection exists with GET, never call PUT to create collection!
            try
            {
                var checkRes = await _client.GetAsync($"{QdrantUrl}/collections/{COLLECTION_NAME}", cancellationToken);
                if (!checkRes.IsSuccessStatusCode)
                {
                    return new VectorSearchResult { SecurityFilterApplied = $"unavailable: collection '{COLLECTION_NAME}' does not exist or is not ready ({checkRes.StatusCode})" };
                }
            }
            catch (Exception ex)
            {
                return new VectorSearchResult { SecurityFilterApplied = $"unavailable: error contacting Qdrant ({ex.Message})" };
            }

            var qVec = precomputedEmbedding ?? await _llm.GetEmbedding(query, cancellationToken);
            if (qVec == null) return new VectorSearchResult();

            try
            {
                var mustConditions = new List<object>();
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

                // Company scope
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

                // Scope-based enforcement: EffectiveScope takes absolute precedence over role name!
                if (string.Equals(securityFilter.EffectiveScope, "SELF", StringComparison.OrdinalIgnoreCase))
                {
                    if (securityFilter.CallerEmployeeId.HasValue)
                    {
                        mustConditions.Add(new { key = "employeeId", match = new { value = securityFilter.CallerEmployeeId.Value } });
                    }
                    else
                    {
                        return new VectorSearchResult { SecurityFilterApplied = "fail_closed: SELF scope requires caller employee id" };
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
                        return new VectorSearchResult { SecurityFilterApplied = "fail_closed: DEPARTMENT scope requires valid positive department ids" };
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

                var body = new
                {
                    vector = qVec,
                    limit = TOPK,
                    filter = mustConditions.Count > 0 ? new { must = mustConditions } : null,
                    with_payload = true,
                    score_threshold = THRESHOLD
                };

                var content = new StringContent(JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json");
                var res = await _client.PostAsync($"{QdrantUrl}/collections/{COLLECTION_NAME}/points/search", content, cancellationToken);

                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized || res.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException($"Qdrant vector store access denied: {res.StatusCode}");
                }
                if ((int)res.StatusCode >= 500)
                {
                    throw new InvalidOperationException($"Qdrant vector store server error: {res.StatusCode}");
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
                                IsAuthorized = isHitAuthorized
                            });
                        }
                    }
                    result.TotalFound = result.Hits.Count(h => h.IsAuthorized);
                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QDRANT SCOPED SEARCH ERROR]: {ex.Message}");
                throw;
            }
            return new VectorSearchResult();
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
                using (var db = new DA.AiEntities())
                {
                    var emp = db.V_AI_EMPLOYEE.FirstOrDefault(x => x.MANV == manv);
                    if (emp != null)
                    {
                        string text = $"Nhân viên {emp.HOTEN} (Mã NV: {emp.MANV}), " +
                                      $"thuộc phòng ban {emp.TEN_PHONGBAN}, chức vụ {emp.TEN_CHUCVU}, bộ phận {emp.TEN_BOPHAN}.";
                        await AddAsync(text, "EMPLOYEE", manv);
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