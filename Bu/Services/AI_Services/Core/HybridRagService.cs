using Bu.Services.AI_Services.Core;
using DA;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.Services.AI_Services
{
    public class HybridRagService
    {
        private readonly SqlGeneratorService _sqlService = new SqlGeneratorService();

        public async Task<string> Ask(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "Vui lòng nhập câu hỏi.";

            // 1. SQL
            string sql = await _sqlService.GenerateRawSql(question);

            System.Diagnostics.Debug.WriteLine("SQL: " + sql);

            if (sql == "NOT_SQL")
                return "Không hiểu câu hỏi.";

            // 2. DB
            var data = ExecuteSql(sql);

            System.Diagnostics.Debug.WriteLine("DATA COUNT: " + data.Count);

            if (!data.Any())
                return "Không có dữ liệu.";

            // 3. FORMAT
            return FormatResult(data);
        }

        // ================= EXECUTE =================
        private List<Dictionary<string, object>> ExecuteSql(string sql)
        {
            var result = new List<Dictionary<string, object>>();

            using (var db = new AiEntities())
            {
                var conn = db.Database.Connection;

                if (conn.State != ConnectionState.Open)
                    conn.Open();

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
                                row[reader.GetName(i)] =
                                    reader.IsDBNull(i) ? null : reader.GetValue(i);
                            }

                            result.Add(row);
                        }
                    }
                }
            }

            return result;
        }

        // ================= FORMAT =================
        private string FormatResult(List<Dictionary<string, object>> rows)
        {
            var first = rows.First();

            if (first.ContainsKey("TEN_CHUCVU"))
                return FormatEmployee(rows);

            if (first.ContainsKey("GIOVAO"))
                return FormatAttendance(rows);

            if (first.ContainsKey("SOBH"))
                return FormatInsurance(rows);

            if (first.ContainsKey("SOTIENUNG"))
                return FormatAdvance(rows);

            if (first.ContainsKey("TENPC"))
                return FormatAllowance(rows);

            return FormatGeneric(rows);
        }

        private string FormatEmployee(List<Dictionary<string, object>> rows)
        {
            var sb = new StringBuilder();

            foreach (var r in rows)
            {
                sb.AppendLine($"👤 {r["HOTEN"]}");
                sb.AppendLine($"- Mã NV: {r["MANV"]}");
                sb.AppendLine($"- Phòng: {r["TEN_PHONGBAN"]}");
                sb.AppendLine($"- Bộ phận: {r["TEN_BOPHAN"]}");
                sb.AppendLine($"- Chức vụ: {r["TEN_CHUCVU"]}");
                sb.AppendLine("------------------");
            }

            return sb.ToString();
        }

        private string FormatAttendance(List<Dictionary<string, object>> rows)
        {
            var sb = new StringBuilder();

            foreach (var r in rows)
            {
                sb.AppendLine($"👤 {r["HOTEN"]}");
                sb.AppendLine($"- Ngày: {r["NGAY"]}/{r["THANG"]}/{r["NAM"]}");
                sb.AppendLine($"- Vào: {r["GIOVAO"]}:{r["PHUTVAO"]}");
                sb.AppendLine($"- Ra: {r["GIORA"]}:{r["PHUTRA"]}");
                sb.AppendLine("------------------");
            }

            return sb.ToString();
        }

        private string FormatInsurance(List<Dictionary<string, object>> rows)
        {
            var sb = new StringBuilder();

            foreach (var r in rows)
            {
                sb.AppendLine($"👤 {r["HOTEN"]}");
                sb.AppendLine($"- Số BH: {r["SOBH"]}");
                sb.AppendLine($"- Nơi khám: {r["NOIKHAMBENH"]}");
                sb.AppendLine("------------------");
            }

            return sb.ToString();
        }

        private string FormatAdvance(List<Dictionary<string, object>> rows)
        {
            var sb = new StringBuilder();

            foreach (var r in rows)
            {
                sb.AppendLine($"👤 {r["HOTEN"]}");
                sb.AppendLine($"- Ngày: {r["NGAY"]}/{r["THANG"]}/{r["NAM"]}");
                sb.AppendLine($"- Tiền ứng: {r["SOTIENUNG"]}");
                sb.AppendLine("------------------");
            }

            return sb.ToString();
        }

        private string FormatAllowance(List<Dictionary<string, object>> rows)
        {
            var sb = new StringBuilder();

            foreach (var r in rows)
            {
                sb.AppendLine($"👤 {r["HOTEN"]}");
                sb.AppendLine($"- Phụ cấp: {r["TENPC"]}");
                sb.AppendLine($"- Số tiền: {r["SOTIEN"]}");
                sb.AppendLine("------------------");
            }

            return sb.ToString();
        }

        private string FormatGeneric(List<Dictionary<string, object>> rows)
        {
            var sb = new StringBuilder();

            foreach (var r in rows.Take(5))
            {
                foreach (var col in r)
                {
                    sb.Append($"{col.Key}: {col.Value} | ");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}