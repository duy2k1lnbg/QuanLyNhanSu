using Bu.Services.AI_Services.Core;
using Bu.DTO;
using Bu.Services.AI_Services.Memory;
using DA;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Bu.Services.AI_Services
{
    public class HybridRagService
    {
        private readonly SqlGeneratorService _sqlGenerator = new SqlGeneratorService();
        private readonly AiRouterService _router = new AiRouterService();
        private readonly OllamaService _ollama = new OllamaService();
        private readonly AiChatHistory _history = new AiChatHistory();

        public async Task<string> Ask(string question)
        {
            try
            {
                // 1. Nhận diện ý định và lấy lịch sử chat
                string intent = await _router.DetectIntent(question);
                string currentHistory = _history.GetHistoryString();

                // 2. Xử lý trường hợp Giao tiếp chung (Greeting/General)
                if (intent == "GENERAL")
                {
                    string botResponse = await _ollama.AskChat("", question, currentHistory);
                    UpdateHistory(question, botResponse);
                    return botResponse;
                }

                // 3. Xử lý nghiệp vụ: Sinh SQL và Truy vấn dữ liệu
                string sql = await _sqlGenerator.GenerateRawSql(question);
                string dataContext = "Không tìm thấy dữ liệu liên quan trong hệ thống.";

                if (sql != "NOT_SQL")
                {
                    var rawData = ExecuteSql(sql);
                    if (rawData.Any())
                    {
                        // Chuyển đổi dữ liệu sang JSON để AI dễ đọc (Sử dụng DTO ngầm định)
                        dataContext = JsonConvert.SerializeObject(rawData, Formatting.Indented);
                    }
                }

                // 4. AI Tổng hợp câu trả lời dựa trên Dữ liệu (RAG) và Lịch sử
                string finalResponse = await _ollama.AskChat(dataContext, question, currentHistory);

                // 5. Cập nhật trí nhớ cho AI
                UpdateHistory(question, finalResponse);

                return finalResponse;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RAG ERROR]: {ex.Message}");
                return "Xin lỗi, tôi gặp chút trục trặc khi kết nối dữ liệu. Bạn thử hỏi lại nhé!";
            }
        }

        private void UpdateHistory(string q, string a)
        {
            _history.AddMessage("User", q);
            _history.AddMessage("AI", a);
        }

        // ================= TRUY VẤN DATABASE =================
        private List<Dictionary<string, object>> ExecuteSql(string sql)
        {
            var result = new List<Dictionary<string, object>>();

            using (var db = new AiEntities())
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string colName = reader.GetName(i);
                                object value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                row[colName] = value;
                            }
                            result.Add(row);
                        }
                    }
                }
            }
            return result;
        }

        // ================= TÍNH NĂNG MỚI: CLEAR SESSION =================
        public void ResetConversation()
        {
            _history.Clear();
            // Có thể clear cache nếu muốn dữ liệu luôn mới nhất
            // _cache.Clear(); 
        }
    }
}