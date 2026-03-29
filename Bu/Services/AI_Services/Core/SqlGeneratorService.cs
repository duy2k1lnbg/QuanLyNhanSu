using Bu.Services.AI_Services.Memory;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bu.Services.AI_Services.Core
{
    public class SqlGeneratorService
    {
        private readonly OllamaService _ollama = new OllamaService();
        private readonly AiSchemaService _schema = new AiSchemaService();
        private readonly AiCacheService _cache = new AiCacheService();

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

            // Cải tiến Regex: Bắt tên nhân viên chính xác hơn (kể cả tên có dấu)
            // Ví dụ: "thông tin nhân viên Nguyễn Thọ Duy" -> lấy được "Nguyễn Thọ Duy"
            var nameMatch = Regex.Match(q, @"(?:nhân viên|tên|là|tìm|về)\s+([\p{L}\s]+)$");
            if (nameMatch.Success)
            {
                string name = nameMatch.Groups[1].Value.Trim();
                if (name.Length > 1)
                    return $"SELECT * FROM V_AI_EMPLOYEE WHERE UPPER(HOTEN) LIKE UPPER('%{name}%')";
            }

            // Mẫu tìm theo mã số
            var idMatch = Regex.Match(q, @"(?:mã|manv)\s*[:=]?\s*(\d+)");
            if (idMatch.Success)
            {
                return $"SELECT * FROM V_AI_EMPLOYEE WHERE MANV = {idMatch.Groups[1].Value}";
            }

            return null;
        }

        private string BuildSqlPrompt(string question)
        {
            // Sử dụng GetFullSchema() để AI biết rõ cột nào là NUMBER, cột nào là TEXT
            return $@"
{_schema.GetFullSchema()}

[NHIỆM VỤ]
Chuyển câu hỏi người dùng thành 1 câu lệnh SQL Oracle duy nhất.
Câu hỏi: ""{question}""

[VÍ DỤ MẪU]
- User: Ai ở phòng kế toán?
- SQL: SELECT * FROM V_AI_EMPLOYEE WHERE UPPER(TEN_PHONGBAN) LIKE UPPER('%KẾ TOÁN%')

Lệnh SQL:";
        }

        private string CleanSql(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "NOT_SQL";

            // Loại bỏ Markdown
            string clean = Regex.Replace(raw, @"```sql|```", "", RegexOptions.IgnoreCase).Trim();

            // Loại bỏ các tiền tố rác
            clean = Regex.Replace(clean, @"^(SQL:|Lệnh SQL:|Output:)\s*", "", RegexOptions.IgnoreCase).Trim();

            // Xử lý xuống dòng (AI đôi khi xuống dòng giữa câu SELECT)
            clean = clean.Replace("\r", " ").Replace("\n", " ");
            while (clean.Contains("  ")) clean = clean.Replace("  ", " ");

            // Cắt dấu chấm phẩy
            if (clean.EndsWith(";")) clean = clean.Substring(0, clean.Length - 1).Trim();

            string upper = clean.ToUpper();

            // Kiểm tra tính hợp lệ và an toàn
            if (!upper.StartsWith("SELECT")) return "NOT_SQL";

            string[] forbidden = { "DELETE", "UPDATE", "DROP", "TRUNCATE", "INSERT", "ALTER" };
            foreach (var word in forbidden)
            {
                if (upper.Contains(word)) return "NOT_SQL";
            }

            return clean;
        }
    }
}