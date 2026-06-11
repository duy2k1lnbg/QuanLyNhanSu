using System;
using System.Collections.Generic;
using System.Linq;
using Bu.Services.AI_Services.Interfaces;

namespace Bu.Services.AI_Services
{
    public class VectorItem
    {
        public string Text { get; set; }
        public float[] Vec { get; set; }
        public string Tag { get; set; }
        public int? EmployeeId { get; set; }
    }

    public class VectorService : IVectorService
    {
        private readonly List<VectorItem> _data = new List<VectorItem>();
        private readonly ILlmService _llm;

        private const int DIM = 128;
        private const float THRESHOLD = 0.75f;
        private const float FALLBACK_THRESHOLD = 0.55f;
        private const int TOPK = 5;

        public VectorService(ILlmService llm)
        {
            _llm = llm;

            // Subscribe to dynamic syncing events
            Bu.Services.AI_Services.Vector.AiDataSyncHub.EmployeeChanged += SyncEmployeeData;
            Bu.Services.AI_Services.Vector.AiDataSyncHub.EmployeeDeleted += RemoveByEmployeeId;
        }

        private static bool _isOllamaActive = true;
        private static DateTime _lastOllamaFailureTime = DateTime.MinValue;
        private static readonly object _circuitLock = new object();
        private static readonly Dictionary<string, float[]> _embedCache = new Dictionary<string, float[]>();

        private float[] Embed(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new float[DIM];

            // 1. Kiểm tra cache
            lock (_embedCache)
            {
                if (_embedCache.TryGetValue(text, out var cachedVec))
                {
                    return cachedVec;
                }
            }

            float[] vec = null;

            // Kiểm tra Circuit Breaker (Bộ ngắt mạch) để tránh dồn ứ request khi Ollama lỗi
            bool checkOllama = true;
            lock (_circuitLock)
            {
                if (!_isOllamaActive)
                {
                    // Tự động khôi phục thử nghiệm lại sau 45 giây
                    if ((DateTime.UtcNow - _lastOllamaFailureTime).TotalSeconds > 45)
                    {
                        _isOllamaActive = true;
                        System.Diagnostics.Debug.WriteLine("[VectorDB Circuit Breaker]: Resetting circuit breaker, trying Ollama again.");
                    }
                    else
                    {
                        checkOllama = false;
                    }
                }
            }

            if (checkOllama)
            {
                try
                {
                    using (var cts = new System.Threading.CancellationTokenSource(1200))
                    {
                        var task = _llm.GetEmbedding(text, cts.Token);
                        // Đợi tối đa 1.2s kết hợp cancellation token
                        if (task.Wait(1200, cts.Token))
                        {
                            vec = task.Result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Embed Ollama Timeout/Error]: {ex.Message}");
                }

                if (vec != null)
                {
                    lock (_embedCache)
                    {
                        _embedCache[text] = vec;
                    }
                    return vec;
                }

                // Nếu đi đến đây tức là bị lỗi kết nối hoặc timeout (1.2s)
                lock (_circuitLock)
                {
                    _isOllamaActive = false;
                    _lastOllamaFailureTime = DateTime.UtcNow;
                    System.Diagnostics.Debug.WriteLine("[VectorDB Circuit Breaker]: Ollama offline/timeout, tripping circuit breaker to use Hashing fallback.");
                }
            }

            vec = EmbedFeatureHashing(text);
            lock (_embedCache)
            {
                _embedCache[text] = vec;
            }
            return vec;
        }

        private float[] EmbedFeatureHashing(string text)
        {
            var vec = new float[DIM];
            if (string.IsNullOrWhiteSpace(text)) return vec;

            text = text.ToLower();

            for (int i = 0; i < text.Length - 1; i++)
            {
                int charPairHash = (text[i] << 5) ^ text[i + 1];
                int index = Math.Abs(charPairHash) % DIM;
                vec[index] += 1.0f;
            }

            return Normalize(vec);
        }

        private float[] Normalize(float[] v)
        {
            float norm = (float)Math.Sqrt(v.Sum(x => x * x)) + 1e-6f;
            for (int i = 0; i < v.Length; i++) v[i] /= norm;
            return v;
        }

        private float Cosine(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0f;
            float dot = 0;
            for (int i = 0; i < a.Length; i++) dot += a[i] * b[i];
            return dot;
        }



        public void Add(string text, string tag = "GENERAL")
        {
            Add(text, tag, null);
        }

        public void Add(string text, string tag, int? employeeId)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            var vec = Embed(text);
            lock (_data)
            {
                _data.Add(new VectorItem 
                { 
                    Text = text, 
                    Vec = vec, 
                    Tag = tag,
                    EmployeeId = employeeId
                });
            }
        }

        public List<string> Search(string query, string tag = null)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<string>();

            var q = Embed(query);
            List<VectorItem> localCopy;
            lock (_data)
            {
                localCopy = new List<VectorItem>(_data);
            }

            var results = localCopy
                .Where(x => tag == null || x.Tag == tag)
                .Select(x => new { x.Text, Score = Cosine(q, x.Vec) })
                .Where(x => x.Score >= THRESHOLD)
                .OrderByDescending(x => x.Score)
                .Take(TOPK)
                .Select(x => x.Text)
                .ToList();

            // Fallback if no high similarity match found
            if (results.Count == 0)
            {
                results = localCopy
                    .Where(x => tag == null || x.Tag == tag)
                    .Select(x => new { x.Text, Score = Cosine(q, x.Vec) })
                    .Where(x => x.Score >= FALLBACK_THRESHOLD)
                    .OrderByDescending(x => x.Score)
                    .Take(TOPK)
                    .Select(x => x.Text)
                    .ToList();
            }

            return results;
        }

        public void Clear()
        {
            lock (_data)
            {
                _data.Clear();
            }
        }

        public void RemoveByEmployeeId(int manv)
        {
            lock (_data)
            {
                _data.RemoveAll(x => x.EmployeeId == manv);
            }
        }

        public void SyncEmployeeData(int manv)
        {
            RemoveByEmployeeId(manv);
            try
            {
                using (var db = new DA.AiEntities())
                {
                    // 1. Employee Info
                    var emp = db.V_AI_EMPLOYEE.FirstOrDefault(x => x.MANV == manv);
                    if (emp != null)
                    {
                        string text = $"Nhân viên {emp.HOTEN} (Mã NV: {emp.MANV}), sinh ngày {emp.NGAYSINH:dd/MM/yyyy}, " +
                                      $"thuộc phòng ban {emp.TEN_PHONGBAN}, chức vụ {emp.TEN_CHUCVU}, bộ phận {emp.TEN_BOPHAN}, " +
                                      $"số điện thoại {emp.DIENTHOAI}, địa chỉ {emp.DIACHI}.";
                        Add(text, "EMPLOYEE", manv);
                    }

                    // 2. Insurance
                    var ins = db.V_AI_INSURANCE.FirstOrDefault(x => x.MANV == manv);
                    if (ins != null)
                    {
                        string text = $"Nhân viên {ins.HOTEN} (Mã NV: {ins.MANV}) có số bảo hiểm là {ins.SOBH}, " +
                                      $"cấp ngày {ins.NGAYCAP:dd/MM/yyyy} tại {ins.NOICAP}, đăng ký khám tại {ins.NOIKHAMBENH}.";
                        Add(text, "INSURANCE", manv);
                    }

                    // 3. Allowances
                    var allowances = db.V_AI_ALLOWANCE.Where(x => x.MANV == manv).ToList();
                    foreach (var al in allowances)
                    {
                        string text = $"Nhân viên {al.HOTEN} (Mã NV: {al.MANV}) nhận phụ cấp {al.TENPC} với số tiền {al.SOTIEN:N0} VNĐ cho kỳ công {al.KYCONG}.";
                        Add(text, "ALLOWANCE", manv);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Vector Sync ERROR] Employee {manv}: {ex.Message}");
            }
        }
    }
}