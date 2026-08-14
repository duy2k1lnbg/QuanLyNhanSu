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
                // [NEW] Kiểm tra phản hồi nhanh (Hardcode) trước tiên để giảm tải AI
                var fastResponse = FastResponseService.GetFastResponse(question);
                if (!string.IsNullOrEmpty(fastResponse))
                {
                    result.Answer = fastResponse;
                    UpdateHistory(question, fastResponse);
                    
                    if (onTokenReceived != null)
                    {
                        // Giả lập AI đang suy nghĩ trong 1.5 giây (hiện chữ "đang suy nghĩ")
                        await Task.Delay(1500);
                        
                        // Giả lập gõ từng từ (Streaming) để UI có hiệu ứng mượt mà giống như đang dùng Ollama
                        var words = fastResponse.Split(' ');
                        for (int i = 0; i < words.Length; i++)
                        {
                            onTokenReceived(words[i] + (i < words.Length - 1 ? " " : ""));
                            await Task.Delay(50); // Độ trễ 50ms giữa mỗi từ
                        }
                        onTokenReceived(""); // Gửi token rỗng để kích hoạt chốt sổ trên UI
                    }
                    
                    return result;
                }

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
                    // [ĐÃ BẬT SQL]: Chế độ Hybrid RAG.
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
                
                // Kết hợp cả Data Context (từ SQL) và Vector Context (từ Qdrant) để tạo Hybrid RAG
                if (!string.IsNullOrEmpty(dataContext) || !string.IsNullOrEmpty(vectorContext))
                {
                    if (!string.IsNullOrEmpty(dataContext)) combinedContext += "Dữ liệu cấu trúc (SQL):\n" + dataContext + "\n";
                    if (!string.IsNullOrEmpty(vectorContext)) combinedContext += vectorContext + "\n";
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

            // Xử lý đặc biệt cho các câu truy vấn thống kê (COUNT, SUM, MAX...) trả về đúng 1 ô dữ liệu
            if (dt.Rows.Count == 1 && dt.Columns.Count == 1)
            {
                var col = dt.Columns[0];
                var val = dt.Rows[0][col];
                sb.AppendLine("[KẾT QUẢ TRUY VẤN CƠ SỞ DỮ LIỆU]");
                
                string displayVal = val.ToString();
                if (val is decimal decVal && (col.ColumnName.Contains("SOTIEN") || col.ColumnName.Contains("LUONG")))
                {
                    displayVal = decVal.ToString("N0") + " VNĐ";
                }
                
                sb.AppendLine($"- Giá trị ({col.ColumnName}): {displayVal}");
                sb.AppendLine($"(Đây chính là kết quả thống kê tương ứng cho câu hỏi của người dùng)");
                return sb.ToString();
            }

            int limit = 12;
            if (dt.Columns.Count <= 3) limit = 50; // Tăng giới hạn cho các truy vấn thống kê (GROUP BY) như đếm số phòng ban

            int maxRows = Math.Min(dt.Rows.Count, limit);
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
                int hiddenCount = dt.Rows.Count - maxRows;
                sb.AppendLine($"\n[LƯU Ý QUAN TRỌNG DÀNH CHO AI]: Cơ sở dữ liệu thực tế tìm thấy {dt.Rows.Count} kết quả, nhưng để phản hồi nhanh, hệ thống chỉ cấp cho bạn {maxRows} bản ghi. BẠN BẮT BUỘC PHẢI thêm 1 dòng ở cuối cùng câu trả lời của bạn với nội dung chính xác như sau: \"(...Danh sách còn {hiddenCount} kết quả nữa bị ẩn để tăng tốc độ. Vui lòng thêm điều kiện tìm kiếm cụ thể hơn...)\"");
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