using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
        private const string COLLECTION_NAME = "hrms_vectors";
        private const float THRESHOLD = 0.6f; // Giảm threshold để dễ match hơn
        private const int TOPK = 5;

        private bool _isInitialized = false;
        private bool _isOffline = false;

        public QdrantService(ILlmService llm)
        {
            _llm = llm;
            // Lấy URL cấu hình từ Oracle/SystemConfig, mặc định 127.0.0.1 để tránh trễ phân giải IPv6 (localhost)
            string configUrl = new Bu.CLASS_CHAMCONG.SYS_CONFIG().getValue("QdrantUrl", "http://127.0.0.1:6333").TrimEnd('/');
            // Nếu người dùng lỡ config "localhost", chuyển luôn sang "127.0.0.1" để an toàn
            _qdrantUrl = configUrl.Replace("localhost", "127.0.0.1");

            // Đăng ký sự kiện đồng bộ tự động từ Oracle HRMS
            Bu.Services.AI_Services.Vector.AiDataSyncHub.EmployeeChanged += (manv) => Task.Run(() => SyncEmployeeDataAsync(manv));
            Bu.Services.AI_Services.Vector.AiDataSyncHub.EmployeeDeleted += (manv) => Task.Run(() => RemoveByEmployeeIdAsync(manv));

            // Khởi tạo Collection ngầm không làm tắc nghẽn Constructor
            Task.Run(EnsureCollectionExistsAsync);
        }

        private async Task EnsureCollectionExistsAsync()
        {
            if (_isOffline) return;

            try
            {
                var res = await _client.GetAsync($"{_qdrantUrl}/collections/{COLLECTION_NAME}");
                if (res.IsSuccessStatusCode)
                {
                    _isInitialized = true;
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
                var createRes = await _client.PutAsync($"{_qdrantUrl}/collections/{COLLECTION_NAME}", content);

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
                // Dùng Guid dạng UUID chuẩn cho Qdrant
                var pointId = Guid.NewGuid().ToString("D");
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
                await _client.PutAsync($"{_qdrantUrl}/collections/{COLLECTION_NAME}/points?wait=true", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[QDRANT ADD ERROR]: {ex.Message}");
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
                var res = await _client.PostAsync($"{_qdrantUrl}/collections/{COLLECTION_NAME}/points/search", content);

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QDRANT SEARCH ERROR]: {ex.Message}");
            }

            return new List<string>();
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
                await _client.PostAsync($"{_qdrantUrl}/collections/{COLLECTION_NAME}/points/delete", content);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QDRANT REMOVE ERROR]: {ex.Message}");
            }
        }

        public async Task SyncEmployeeDataAsync(int manv)
        {
            await RemoveByEmployeeIdAsync(manv);
            try
            {
                using (var db = new DA.AiEntities())
                {
                    // 1. Thông tin nhân viên từ View Oracle
                    var emp = db.V_AI_EMPLOYEE.FirstOrDefault(x => x.MANV == manv);
                    if (emp != null)
                    {
                        string text = $"Nhân viên {emp.HOTEN} (Mã NV: {emp.MANV}), sinh ngày {emp.NGAYSINH:dd/MM/yyyy}, " +
                                      $"thuộc phòng ban {emp.TEN_PHONGBAN}, chức vụ {emp.TEN_CHUCVU}, bộ phận {emp.TEN_BOPHAN}, " +
                                      $"số điện thoại {emp.DIENTHOAI}, địa chỉ {emp.DIACHI}.";
                        await AddAsync(text, "EMPLOYEE", manv);
                    }


                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Vector Sync ERROR] Employee {manv}: {ex.Message}");
            }
        }

        // Interface Fallbacks cho gọi đồng bộ nếu Interface bắt buộc
        public void Add(string text, string tag = "GENERAL") => Task.Run(() => AddAsync(text, tag)).Wait();
        public void Add(string text, string tag, int? employeeId) => Task.Run(() => AddAsync(text, tag, employeeId)).Wait();
        public List<string> Search(string query, string tag = null) => Task.Run(() => SearchAsync(query, tag)).GetAwaiter().GetResult();
        public void RemoveByEmployeeId(int manv) => Task.Run(() => RemoveByEmployeeIdAsync(manv)).Wait();
        public void SyncEmployeeData(int manv) => Task.Run(() => SyncEmployeeDataAsync(manv)).Wait();
        public void Clear()
        {
            try
            {
                _client.DeleteAsync($"{_qdrantUrl}/collections/{COLLECTION_NAME}").GetAwaiter().GetResult();
                _isInitialized = false;
                EnsureCollectionExistsAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex) { Console.WriteLine($"[CLEAR ERROR]: {ex.Message}"); }
        }
    }
}