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

        public async Task<QueryResult> Ask(string question)
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
                // 1. Nhận diện ý định và lấy lịch sử chat
                string intent = await _router.DetectIntent(question);
                string currentHistory = _history.GetHistoryString();

                // 2. Xử lý trường hợp Giao tiếp chung (Greeting/General)
                if (intent == "GENERAL")
                {
                    string botResponse = await _ollama.AskChat("", question, currentHistory);
                    UpdateHistory(question, botResponse);
                    result.Answer = botResponse;
                    return result;
                }

                // 3. Xử lý nghiệp vụ: Sinh SQL và Truy vấn dữ liệu
                string sql = await _sqlGenerator.GenerateRawSql(question);
                string dataContext = "Không tìm thấy dữ liệu liên quan trong hệ thống.";
                result.SqlQuery = sql;

                if (sql != "NOT_SQL" && !string.IsNullOrEmpty(sql))
                {
                    var dt = ExecuteSqlToDataTable(sql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        result.Data = dt;
                        dataContext = JsonConvert.SerializeObject(dt, Formatting.Indented);
                    }
                }

                // 4. AI Tổng hợp câu trả lời dựa trên Dữ liệu (RAG) và Lịch sử
                string finalResponse = await _ollama.AskChat(dataContext, question, currentHistory);

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