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
        private readonly AiRouterService _aiRouter;
        private readonly AiChatHistory _history = new AiChatHistory();

        public HybridRagService()
        {
            _sqlGenerator = AiServiceLocator.GetService<ISqlGenerator>();
            _ollama = AiServiceLocator.GetService<ILlmService>();
            _vectorService = AiServiceLocator.GetService<IVectorService>();
            _aiRouter = AiServiceLocator.GetService<AiRouterService>();
        }

        private bool _isVectorDbInitialized = false;
        private readonly object _vectorDbLock = new object();




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


                string currentHistory = _history.GetHistoryString();
                string vectorContext = "";
                string sql = "";
                string dataContext = "";

                // 1. Phân loại luồng câu hỏi bằng AiRouterService
                string intent = await _aiRouter.DetectIntent(question);
                bool isGeneral = intent == "GENERAL";
                bool likelyDbQuery = !isGeneral;

                if (likelyDbQuery)
                {
                    // [ĐÃ TẮT SQL]: Chế độ Thuần Vector Search. Bỏ comment đoạn này nếu muốn bật lại Hybrid RAG.
                    /*
                    // 2. Generate SQL
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
                    */
                }

                // 3. Search vector DB for additional context
                string searchTag = likelyDbQuery ? intent : null;
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


                string combinedContext = "";
                
                // Fallback to vector context (Pure Vector Search)
                if (!string.IsNullOrEmpty(vectorContext))
                {
                    combinedContext = vectorContext;
                }
                else
                {
                    combinedContext = "Không tìm thấy dữ liệu liên quan trong hệ thống.";
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