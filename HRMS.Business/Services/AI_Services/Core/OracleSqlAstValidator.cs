using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bu.Services.AI_Services.Core
{
    /// <summary>
    /// Bộ kiểm duyệt cú pháp và cấu trúc ngữ pháp (AST/Tokenizer) cho các câu lệnh SQL sinh bởi AI.
    /// Đảm bảo tính an toàn tuyệt đối trước khi gửi câu lệnh đến Oracle Database.
    /// </summary>
    public static class OracleSqlAstValidator
    {
        // Danh sách Whitelist duy nhất các View an toàn được phép đọc
        private static readonly HashSet<string> AllowedViews = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "V_AI_EMPLOYEE",
            "V_AI_ATTENDANCE",
            "V_AI_OVERTIME",
            "V_AI_INSURANCE",
            "V_AI_ADVANCE",
            "V_AI_ALLOWANCE"
        };

        // Danh sách từ khóa cấm tuyệt đối (DDL, DML, Administrative)
        private static readonly HashSet<string> ForbiddenKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "TRUNCATE", 
            "CREATE", "GRANT", "REVOKE", "MERGE", "EXEC", "EXECUTE", 
            "CALL", "DECLARE", "BEGIN", "END", "INTO", "OUTFILE", "DUMPFILE"
        };

        // Danh sách tiền tố và gói hệ thống Oracle nguy hiểm
        private static readonly string[] DangerousOraclePatterns = new[]
        {
            "DBMS_", "UTL_", "SYS.", "SYSTEM.", "ALL_", "USER_", "DBA_", "V$", "GV$"
        };

        public class SqlValidationResult
        {
            public bool IsValid { get; set; }
            public string CleanedSql { get; set; }
            public string RejectionReason { get; set; }
            public List<string> ReferencedTables { get; set; } = new List<string>();
        }

        /// <summary>
        /// Phân tích và kiểm duyệt câu lệnh SQL sinh ra từ AI.
        /// </summary>
        public static SqlValidationResult Validate(string rawSql)
        {
            var result = new SqlValidationResult { IsValid = false };

            if (string.IsNullOrWhiteSpace(rawSql))
            {
                result.RejectionReason = "Câu lệnh SQL rỗng.";
                return result;
            }

            // 1. Loại bỏ các khối code block markdown và tiền tố rác
            string cleaned = Regex.Replace(rawSql, @"```sql|```", "", RegexOptions.IgnoreCase).Trim();
            cleaned = Regex.Replace(cleaned, @"^(SQL:|Lệnh SQL:|Output:|Query:)\s*", "", RegexOptions.IgnoreCase).Trim();

            // 2. Tách và loại bỏ comments (-- comment hoặc /* comment */)
            cleaned = Regex.Replace(cleaned, @"/\*.*?\*/", " ", RegexOptions.Singleline);
            cleaned = Regex.Replace(cleaned, @"--.*?(\r?\n|$)", " ");

            // 3. Chuẩn hóa khoảng trắng
            cleaned = cleaned.Replace("\r", " ").Replace("\n", " ");
            while (cleaned.Contains("  "))
            {
                cleaned = cleaned.Replace("  ", " ");
            }
            cleaned = cleaned.Trim();

            // 4. Cắt dấu chấm phẩy cuối dòng
            if (cleaned.EndsWith(";"))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - 1).Trim();
            }

            // Chặn multi-statement SQL Injection (phát hiện dấu chấm phẩy ở giữa câu lệnh)
            if (ContainsUnquotedSemicolon(cleaned))
            {
                result.RejectionReason = "Phát hiện nhiều câu lệnh phân tách bởi dấu chấm phẩy (Multi-statement blocked).";
                return result;
            }

            // 5. Token hóa (Lexical Analysis)
            var tokens = Tokenize(cleaned);
            if (tokens.Count == 0)
            {
                result.RejectionReason = "Không có tokens cú pháp hợp lệ.";
                return result;
            }

            // 6. Kiểm tra câu lệnh phải bắt đầu bằng SELECT hoặc WITH (CTE)
            string firstToken = tokens[0].ToUpperInvariant();
            if (firstToken != "SELECT" && firstToken != "WITH")
            {
                result.RejectionReason = $"Câu lệnh không phải là truy vấn đọc (Bắt đầu bởi [{firstToken}] thay vì SELECT/WITH).";
                return result;
            }

            // 7. Kiểm tra từ khóa cấm và các gói nguy hiểm
            foreach (var token in tokens)
            {
                string upperToken = token.ToUpperInvariant();

                if (ForbiddenKeywords.Contains(upperToken))
                {
                    result.RejectionReason = $"Phát hiện từ khóa bị cấm: [{token}].";
                    return result;
                }

                foreach (var pattern in DangerousOraclePatterns)
                {
                    if (upperToken.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        result.RejectionReason = $"Phát hiện truy cập đối tượng hoặc gói hệ thống Oracle bị cấm: [{token}].";
                        return result;
                    }
                }
            }

            // 8. Trích xuất đối tượng bảng / view từ các mệnh đề FROM và JOIN
            var tables = ExtractTablesFromTokens(tokens);
            result.ReferencedTables = tables;

            if (tables.Count == 0)
            {
                result.RejectionReason = "Không tìm thấy bảng hoặc view nào được truy vấn trong mệnh đề FROM/JOIN.";
                return result;
            }

            // 9. Kiểm duyệt 100% đối tượng tham chiếu phải nằm trong Whitelist
            foreach (var table in tables)
            {
                if (!AllowedViews.Contains(table))
                {
                    result.RejectionReason = $"Bảng/View [{table}] không nằm trong danh sách View AI được cấp phép.";
                    return result;
                }
            }

            // Hợp lệ tuyệt đối
            result.IsValid = true;
            result.CleanedSql = cleaned;
            return result;
        }

        private static bool ContainsUnquotedSemicolon(string sql)
        {
            bool inString = false;
            for (int i = 0; i < sql.Length; i++)
            {
                char c = sql[i];
                if (c == '\'')
                {
                    inString = !inString;
                }
                else if (c == ';' && !inString)
                {
                    return true;
                }
            }
            return false;
        }

        private static List<string> Tokenize(string sql)
        {
            var tokens = new List<string>();
            var pattern = @"'([^']|'')*'|[a-zA-Z0-9_$.]+|[(),;=<>!+*\/]";
            var matches = Regex.Matches(sql, pattern);

            foreach (Match m in matches)
            {
                tokens.Add(m.Value);
            }
            return tokens;
        }

        private static List<string> ExtractTablesFromTokens(List<string> tokens)
        {
            var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < tokens.Count - 1; i++)
            {
                string token = tokens[i].ToUpperInvariant();
                if (token == "FROM" || token == "JOIN")
                {
                    // Lấy token kế tiếp
                    int nextIdx = i + 1;
                    if (nextIdx < tokens.Count)
                    {
                        string target = tokens[nextIdx];

                        // Nếu sau FROM là dấu mở ngoặc (subquery), bỏ qua
                        if (target == "(")
                        {
                            continue;
                        }

                        // Làm sạch schema prefix (ví dụ: HR.V_AI_EMPLOYEE -> V_AI_EMPLOYEE)
                        if (target.Contains("."))
                        {
                            var parts = target.Split('.');
                            target = parts[parts.Length - 1];
                        }

                        target = target.Trim('"', '\'');
                        if (!string.IsNullOrWhiteSpace(target))
                        {
                            tables.Add(target);
                        }
                    }
                }
            }

            return tables.ToList();
        }
    }
}
