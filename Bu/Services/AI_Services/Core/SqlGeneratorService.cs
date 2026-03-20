using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bu.Services.AI_Services.Core
{
    public class SqlGeneratorService
    {
        private readonly OllamaService _ollama = new OllamaService();

        // ⚡ CACHE
        private static Dictionary<string, string> _cache = new Dictionary<string, string>();

        public async Task<string> GenerateRawSql(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "NOT_SQL";

            question = question.Trim();

            // 🚀 CACHE HIT
            if (_cache.ContainsKey(question))
                return _cache[question];

            // 🚀 HARD CODE
            var fast = TryHardCode(question);
            if (!string.IsNullOrEmpty(fast))
            {
                _cache[question] = fast;
                return fast;
            }

            // 🚀 AI fallback
            string prompt = BuildPrompt(question);

            var raw = await _ollama.AskSql(prompt);

            var clean = CleanSql(raw);

            _cache[question] = clean;

            return clean;
        }

        // ================= HARD CODE =================
        private string TryHardCode(string q)
        {
            var match = Regex.Match(q, @"\b\d+\b");

            // tìm theo mã
            if (q.ToLower().Contains("mã") && match.Success)
            {
                return $"SELECT * FROM V_AI_EMPLOYEE WHERE MANV = {match.Value}";
            }

            // tìm theo tên
            if (q.ToLower().Contains("nhân viên"))
            {
                var name = ExtractName(q);
                if (!string.IsNullOrEmpty(name))
                {
                    return $"SELECT * FROM V_AI_EMPLOYEE WHERE UPPER(HOTEN) LIKE UPPER('%{name}%')";
                }
            }

            return null;
        }

        private string ExtractName(string q)
        {
            var match = Regex.Match(q, @"nhân viên (.+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : "";
        }

        // ================= PROMPT =================
        private string BuildPrompt(string question)
        {
            return $@"
Bạn là AI viết SQL Oracle.

CHỈ dùng các VIEW:
- V_AI_EMPLOYEE(MANV, HOTEN, TEN_PHONGBAN, TEN_BOPHAN, TEN_CHUCVU)
- V_AI_ATTENDANCE
- V_AI_OVERTIME
- V_AI_INSURANCE
- V_AI_ADVANCE
- V_AI_ALLOWANCE

QUY TẮC:
1. CHỈ trả về SELECT
2. KHÔNG giải thích
3. KHÔNG dùng dấu ```
4. MANV không có dấu '
5. Tên dùng LIKE

Câu hỏi: {question}
SQL:";
        }

        // ================= CLEAN =================
        private string CleanSql(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "NOT_SQL";

            var clean = Regex.Replace(raw, "```sql|```", "").Trim();

            if (clean.EndsWith(";"))
                clean = clean.Substring(0, clean.Length - 1);

            var upper = clean.ToUpper();

            if (upper.Contains("DELETE") ||
                upper.Contains("UPDATE") ||
                upper.Contains("INSERT") ||
                upper.Contains("DROP"))
                return "NOT_SQL";

            return clean;
        }
    }
}