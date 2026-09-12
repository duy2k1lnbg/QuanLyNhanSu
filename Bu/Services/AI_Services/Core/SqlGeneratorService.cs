using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Interfaces;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bu.Services.AI_Services.Core
{
    public class SqlGeneratorService : ISqlGenerator
    {
        private readonly ILlmService _ollama;
        private readonly IPromptManager _promptManager;
        private readonly AiCacheService _cache = new AiCacheService();

        public SqlGeneratorService(ILlmService ollama, IPromptManager promptManager)
        {
            _ollama = ollama;
            _promptManager = promptManager;
        }

        public async Task<string> GenerateRawSql(string question)
        {
            if (string.IsNullOrWhiteSpace(question)) return "NOT_SQL";

            // 1. Kiểm tra Cache
            string cachedSql = _cache.Get(question);
            if (cachedSql != null) return cachedSql;

            // 2. Xử lý nhanh (Hardcode) - Ưu tiên vì cực nhanh và chính xác 100%
            var fastSql = TryHardCode(question);
            if (fastSql != null)
            {
                _cache.Set(question, fastSql);
                return fastSql;
            }

            // 3. Gọi AI với Schema đầy đủ
            string prompt = BuildSqlPrompt(question);
            var rawResponse = await _ollama.AskSql(prompt);

            // 4. Làm sạch SQL
            var cleanSql = CleanSql(rawResponse);

            // 5. Lưu Cache
            if (cleanSql != "NOT_SQL")
            {
                _cache.Set(question, cleanSql);
            }

            return cleanSql;
        }

        private string TryHardCode(string q)
        {
            q = q.ToLower().Trim();

            // 1. Tìm theo mã nhân viên: "nhân viên mã 10", "manv: 12", "mã nv 18"
            var idMatch = Regex.Match(q, @"(?:mã|manv|mã nv|id)\s*[:=]?\s*(\d+)");
            if (idMatch.Success)
            {
                return $"SELECT * FROM V_AI_EMPLOYEE WHERE MANV = {idMatch.Groups[1].Value}";
            }

            // 2. Tìm theo tên nhân viên:
            // "cho tôi thông tin nhân viên tên Duy", "thông tin nhân viên Nguyễn Thọ Duy", "nhân viên tên Duy"
            var nameMatch = Regex.Match(q, @"(?:thông tin nhân viên tên là|thông tin nhân viên tên|thông tin nhân viên|nhân viên tên là|nhân viên tên|tìm nhân viên tên|tìm nhân viên|nhân sự tên là|nhân sự tên|thông tin của|thông tin|tìm|về)\s+([\p{L}\s]+)$");
            if (nameMatch.Success)
            {
                string name = nameMatch.Groups[1].Value.Trim();
                if (name.Length >= 2)
                {
                    string safeName = name.Replace("'", "''");
                    return $"SELECT * FROM V_AI_EMPLOYEE WHERE UPPER(HOTEN) LIKE UPPER('%{safeName}%')";
                }
            }

            // 3. Người dùng chỉ gõ tên riêng (2 - 5 từ): "Nguyễn Thọ Duy", "Trần Thanh Tâm"
            var words = q.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length >= 2 && words.Length <= 5 && Regex.IsMatch(q, @"^[\p{L}\s]+$"))
            {
                // Loại trừ các câu chào hoặc câu hỏi chung
                string[] nonNameKeywords = { "xin chào", "chào bạn", "bạn là ai", "hôm nay", "công ty", "quy chế", "chính sách" };
                if (!nonNameKeywords.Any(k => q.Contains(k)))
                {
                    string safeName = q.Replace("'", "''");
                    return $"SELECT * FROM V_AI_EMPLOYEE WHERE UPPER(HOTEN) LIKE UPPER('%{safeName}%')";
                }
            }

            // 4. Danh sách nhân viên theo phòng ban: "phòng kế toán", "phòng nhân sự", "ai ở phòng IT?"
            string qClean = q.Replace("?", "").Replace(".", "").Replace("!", "").Trim();
            var pbMatch = Regex.Match(qClean, @"(?:phòng ban|phòng|bộ phận)\s+([\p{L}\s0-9]+)$");
            if (pbMatch.Success)
            {
                string pb = pbMatch.Groups[1].Value.Trim();
                if (pb.Length >= 2)
                {
                    string safePb = pb.Replace("'", "''");
                    return $"SELECT * FROM V_AI_EMPLOYEE WHERE UPPER(TEN_PHONGBAN) LIKE UPPER('%{safePb}%')";
                }
            }

            // 5. Thống kê phụ cấp trên X triệu:
            // "phụ cấp ... trên 1 triệu"
            if (q.Contains("phụ cấp") && q.Contains("triệu"))
            {
                var numMatch = Regex.Match(q, @"(\d+)\s*(?:triệu|tr)");
                if (numMatch.Success && decimal.TryParse(numMatch.Groups[1].Value, out decimal trieu))
                {
                    decimal sotien = trieu * 1000000;
                    return $"SELECT * FROM V_AI_ALLOWANCE WHERE SOTIEN >= {sotien}";
                }
            }

            return null;
        }

        private string BuildSqlPrompt(string question)
        {
            string schemaText = _promptManager.GetSchema();
            string template = _promptManager.GetPrompt("SqlPromptTemplate");
            if (string.IsNullOrEmpty(template))
            {
                // Fallback nếu không tải được template
                return $@"
{schemaText}

[NHIỆM VỤ]
Chuyển câu hỏi người dùng thành 1 câu lệnh SQL Oracle duy nhất.
Câu hỏi: ""{question}""

[VÍ DỤ MẪU]
- User: Ai ở phòng kế toán?
- SQL: SELECT * FROM V_AI_EMPLOYEE WHERE UPPER(TEN_PHONGBAN) LIKE UPPER('%KẾ TOÁN%')

Lệnh SQL:";
            }
            return template.Replace("{Schema}", schemaText)
                           .Replace("{Question}", question);
        }

        private string CleanSql(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "NOT_SQL";

            var validation = OracleSqlAstValidator.Validate(raw);
            if (!validation.IsValid)
            {
                System.Diagnostics.Debug.WriteLine($"[SQL VALIDATION REJECTED]: {validation.RejectionReason} (Raw: {raw})");
                return "NOT_SQL";
            }

            return validation.CleanedSql;
        }
    }
}