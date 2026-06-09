using Bu.Services.AI_Services.Core;
using Bu.DTO;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Interfaces;
using DA;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace Bu.Services.AI_Services
{
    public class HybridRagService
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly ILlmService _ollama;
        private readonly IVectorService _vectorService;
        private readonly AiChatHistory _history = new AiChatHistory();

        public HybridRagService()
        {
            _sqlGenerator = AiServiceLocator.GetService<ISqlGenerator>();
            _ollama = AiServiceLocator.GetService<ILlmService>();
            _vectorService = AiServiceLocator.GetService<IVectorService>();
        }

        private bool _isVectorDbInitialized = false;
        private readonly object _vectorDbLock = new object();

        private async Task EnsureVectorDbInitialized()
        {
            if (_isVectorDbInitialized) return;

            await Task.Run(() =>
            {
                lock (_vectorDbLock)
                {
                    if (_isVectorDbInitialized) return;

                    try
                    {
                        using (var db = new AiEntities())
                        {
                            // 1. Load Employees
                            var employees = db.V_AI_EMPLOYEE.ToList();
                            foreach (var emp in employees)
                            {
                                string text = $"Nhân viên {emp.HOTEN} (Mã NV: {emp.MANV}), sinh ngày {emp.NGAYSINH:dd/MM/yyyy}, " +
                                              $"thuộc phòng ban {emp.TEN_PHONGBAN}, chức vụ {emp.TEN_CHUCVU}, bộ phận {emp.TEN_BOPHAN}, " +
                                              $"số điện thoại {emp.DIENTHOAI}, địa chỉ {emp.DIACHI}.";
                                _vectorService.Add(text, "EMPLOYEE");
                            }

                            // 2. Load Insurances
                            var insurances = db.V_AI_INSURANCE.ToList();
                            foreach (var ins in insurances)
                            {
                                string text = $"Nhân viên {ins.HOTEN} (Mã NV: {ins.MANV}) có số bảo hiểm là {ins.SOBH}, " +
                                              $"cấp ngày {ins.NGAYCAP:dd/MM/yyyy} tại {ins.NOICAP}, đăng ký khám tại {ins.NOIKHAMBENH}.";
                                _vectorService.Add(text, "INSURANCE");
                            }

                            // 3. Load Allowances
                            var allowances = db.V_AI_ALLOWANCE.ToList();
                            foreach (var al in allowances)
                            {
                                string text = $"Nhân viên {al.HOTEN} (Mã NV: {al.MANV}) nhận phụ cấp {al.TENPC} với số tiền {al.SOTIEN:N0} VNĐ cho kỳ công {al.KYCONG}.";
                                _vectorService.Add(text, "ALLOWANCE");
                            }
                        }
                        _isVectorDbInitialized = true;
                        System.Diagnostics.Debug.WriteLine("[VectorDB] Loaded vector data successfully.");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[VectorDB ERROR]: {ex.Message}");
                    }
                }
            });
        }

        private bool IsLikelyDbQuery(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return false;
            q = q.ToLower().Trim();

            // Business keywords related to our database views
            string[] keywords = { 
                "nhân viên", "nv", "phòng", "ban", "lương", "thu nhập", "tăng ca", "overtime", "ot",
                "bảo hiểm", "bh", "ứng", "phụ cấp", "trợ cấp", "sinh nhật", "sn", "ngày sinh", "chấm công", 
                "giờ vào", "giờ ra", "time in", "time out", "mã", "manv", "tên", "hoten", "họ tên", 
                "danh sách", "ds", "thống kê", "báo cáo", "tìm", "ai là", "thông tin", "hợp đồng", "hdld",
                "chức vụ", "bộ phận", "địa chỉ", "điện thoại"
            };

            return keywords.Any(k => q.Contains(k));
        }

        private string GetTagFromSql(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return null;
            string upper = sql.ToUpper();
            if (upper.Contains("V_AI_EMPLOYEE")) return "EMPLOYEE";
            if (upper.Contains("V_AI_ATTENDANCE")) return "ATTENDANCE";
            if (upper.Contains("V_AI_OVERTIME")) return "OVERTIME";
            if (upper.Contains("V_AI_INSURANCE")) return "INSURANCE";
            if (upper.Contains("V_AI_ADVANCE")) return "ADVANCE";
            if (upper.Contains("V_AI_ALLOWANCE")) return "ALLOWANCE";
            return null;
        }

        public async Task<QueryResult> Ask(string question, Action<string> onTokenReceived = null)
        {
            // 0. Preprocess user query to restore diacritics and inject schema hints based on intent
            question = QueryPreprocessor.Preprocess(question);

            var result = new QueryResult
            {
                Answer = "Xin lỗi, tôi gặp chút trục trặc khi kết nối dữ liệu. Bạn thử hỏi lại nhé!",
                SqlQuery = "",
                Data = null
            };

            try
            {
                // Ensure vector DB is initialized (non-blocking after first load)
                await EnsureVectorDbInitialized();

                string currentHistory = _history.GetHistoryString();
                string vectorContext = "";
                string sql = "";
                string dataContext = "";

                bool likelyDbQuery = IsLikelyDbQuery(question);

                if (likelyDbQuery)
                {
                    // 1. Generate SQL
                    sql = await _sqlGenerator.GenerateRawSql(question);
                    result.SqlQuery = sql;

                    if (sql != "NOT_SQL" && !string.IsNullOrEmpty(sql))
                    {
                        var dt = ExecuteSqlToDataTable(sql);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            result.Data = dt;
                            dataContext = FormatDataTableToTextContext(dt);
                        }
                    }
                }

                // 2. Search vector DB for additional context
                string searchTag = likelyDbQuery ? GetTagFromSql(sql) : null;
                if (_isVectorDbInitialized)
                {
                    var vectorMatches = _vectorService.Search(question, searchTag);
                    
                    // Fallback to all tags if intent search returned nothing
                    if (vectorMatches.Count == 0 && searchTag != null)
                    {
                        vectorMatches = _vectorService.Search(question, null);
                    }

                    if (vectorMatches.Count > 0)
                    {
                        vectorContext = "Dữ liệu tìm kiếm tương đồng (Vector Search):\n" +
                                        string.Join("\n", vectorMatches.Select(m => $"- {m}"));
                    }
                }

                // 3. Combine SQL context and Vector context in a clean, non-conflicting way
                string combinedContext = "";
                if (!string.IsNullOrEmpty(dataContext))
                {
                    // If we have precise SQL results, use them exclusively to avoid vector search noise
                    combinedContext = "Dữ liệu cấu trúc truy vấn (SQL Database):\n" + dataContext;
                }
                else
                {
                    // Fallback to vector context
                    if (!string.IsNullOrEmpty(vectorContext))
                    {
                        combinedContext = vectorContext;
                    }
                    else
                    {
                        combinedContext = "Không tìm thấy dữ liệu liên quan trong hệ thống.";
                    }
                }

                // 4. AI Tổng hợp câu trả lời dựa trên Dữ liệu (RAG) và Lịch sử
                string finalResponse = await _ollama.AskChat(combinedContext, question, currentHistory, onTokenReceived);

                // 5. Cập nhật trí nhớ cho AI
                UpdateHistory(question, finalResponse);

                result.Answer = finalResponse;
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RAG ERROR]: {ex.Message}");
                return result;
            }
        }

        private string FormatDataTableToTextContext(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return "";

            var sb = new StringBuilder();
            var columnLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "MANV", "Mã nhân viên" },
                { "HOTEN", "Họ tên" },
                { "NGAYSINH", "Ngày sinh" },
                { "DIENTHOAI", "Điện thoại" },
                { "DIACHI", "Địa chỉ" },
                { "TEN_PHONGBAN", "Phòng ban" },
                { "TEN_BOPHAN", "Bộ phận" },
                { "TEN_CHUCVU", "Chức vụ" },
                { "NGAY", "Ngày" },
                { "THANG", "Tháng" },
                { "NAM", "Năm" },
                { "GIOVAO", "Giờ vào" },
                { "PHUTVAO", "Phút vào" },
                { "GIORA", "Giờ ra" },
                { "PHUTRA", "Phút ra" },
                { "TIME_IN", "Giờ vào làm" },
                { "TIME_OUT", "Giờ ra làm" },
                { "SOGIO", "Số giờ tăng ca" },
                { "SOBH", "Số bảo hiểm" },
                { "NGAYCAP", "Ngày cấp" },
                { "NOICAP", "Nơi cấp" },
                { "NOIKHAMBENH", "Nơi đăng ký khám chữa bệnh" },
                { "SOTIEN", "Số tiền" },
                { "TENPC", "Tên phụ cấp" },
                { "KYCONG", "Kỳ công" }
            };

            int maxRows = Math.Min(dt.Rows.Count, 12);
            for (int r = 0; r < maxRows; r++)
            {
                sb.AppendLine($"--- Bản ghi #{r + 1} ---");
                foreach (DataColumn col in dt.Columns)
                {
                    var val = dt.Rows[r][col];
                    if (val == DBNull.Value || val == null) continue;

                    string friendlyName = columnLabels.ContainsKey(col.ColumnName) 
                        ? columnLabels[col.ColumnName] 
                        : col.ColumnName;

                    string displayVal = val.ToString();
                    if (val is DateTime dtVal)
                    {
                        displayVal = dtVal.ToString("dd/MM/yyyy");
                    }
                    else if (val is decimal decVal && (col.ColumnName.Contains("SOTIEN") || col.ColumnName.Contains("LUONG")))
                    {
                        displayVal = decVal.ToString("N0") + " VNĐ";
                    }

                    sb.AppendLine($"- {friendlyName} ({col.ColumnName}): {displayVal}");
                }
            }

            if (dt.Rows.Count > maxRows)
            {
                sb.AppendLine($"--- LƯỢC BỚT {dt.Rows.Count - maxRows} BẢN GHI ĐỂ TĂNG TỐC ĐỘ PHẢN HỒI ---");
            }

            return sb.ToString();
        }

        private void UpdateHistory(string q, string a)
        {
            _history.AddMessage("User", q);
            _history.AddMessage("AI", a);
        }

        // ================= TRUY VẤN DATABASE SANG DATATABLE =================
        private DataTable ExecuteSqlToDataTable(string sql)
        {
            var dt = new DataTable();
            try
            {
                using (var db = new AiEntities())
                {
                    var conn = db.Database.Connection;
                    if (conn.State != ConnectionState.Open) conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB SQL ERROR]: {ex.Message}");
            }
            return dt;
        }

        public List<Bu.Services.AI_Services.Memory.ChatMessage> GetMessages()
        {
            return _history.GetMessages();
        }

        // ================= TÍNH NĂNG MỚI: CLEAR SESSION =================
        public void ResetConversation()
        {
            _history.Clear();
        }
    }

    public class QueryResult
    {
        public string Answer { get; set; }
        public string SqlQuery { get; set; }
        public DataTable Data { get; set; }
    }
}