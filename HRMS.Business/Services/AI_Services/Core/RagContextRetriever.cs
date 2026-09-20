using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Interfaces;

namespace Bu.Services.AI_Services.Core
{
    public class RagContextRetriever : IRagContextRetriever
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly ISafeSqlExecutor _sqlExecutor;
        private readonly IVectorService _vectorService;

        private static readonly Dictionary<string, string> ColumnLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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

        public RagContextRetriever(ISqlGenerator sqlGenerator, ISafeSqlExecutor sqlExecutor, IVectorService vectorService)
        {
            _sqlGenerator = sqlGenerator;
            _sqlExecutor = sqlExecutor;
            _vectorService = vectorService;
        }

        public async Task<RetrievedRagContext> RetrieveContextAsync(string question, string intent)
        {
            var result = new RetrievedRagContext();
            bool isGeneral = string.Equals(intent, "GENERAL", StringComparison.OrdinalIgnoreCase);
            bool likelyDbQuery = !isGeneral;

            // 1. Nếu có thể là câu hỏi về cơ sở dữ liệu, thử sinh và chạy SQL
            if (likelyDbQuery)
            {
                string sql = await _sqlGenerator.GenerateRawSql(question);
                result.SqlQuery = sql;

                if (!string.IsNullOrWhiteSpace(sql) && sql != "NOT_SQL")
                {
                    var dt = _sqlExecutor.ExecuteSafeQuery(sql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        result.SqlDataTable = dt;
                        result.DataContext = FormatDataTableToTextContext(dt);
                    }
                }
            }

            // 2. Tìm kiếm ngữ nghĩa từ Vector Database (Qdrant)
            string searchTag = likelyDbQuery ? intent : null;
            var vectorMatches = _vectorService.Search(question, searchTag);

            // Fallback: nếu tìm theo tag không ra, tìm toàn bộ collection
            if ((vectorMatches == null || vectorMatches.Count == 0) && searchTag != null)
            {
                vectorMatches = _vectorService.Search(question, null);
            }

            if (vectorMatches != null && vectorMatches.Count > 0)
            {
                result.VectorContext = "Dữ liệu tìm kiếm tương đồng (Vector Search):\n" +
                                      string.Join("\n", vectorMatches.Select(m => $"- {m}"));
            }

            // 3. Kết hợp context
            var combinedBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(result.DataContext))
            {
                combinedBuilder.AppendLine("Dữ liệu cấu trúc (SQL):");
                combinedBuilder.AppendLine(result.DataContext);
            }
            if (!string.IsNullOrEmpty(result.VectorContext))
            {
                combinedBuilder.AppendLine(result.VectorContext);
            }

            if (combinedBuilder.Length == 0)
            {
                result.CombinedContext = "Không tìm thấy dữ liệu liên quan trong hệ thống.";
            }
            else
            {
                result.CombinedContext = combinedBuilder.ToString().TrimEnd();
            }

            return result;
        }

        public string FormatDataTableToTextContext(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return string.Empty;

            var sb = new StringBuilder();

            // Xử lý đặc biệt cho truy vấn thống kê (1 ô dữ liệu: COUNT, SUM, MAX...)
            if (dt.Rows.Count == 1 && dt.Columns.Count == 1)
            {
                var col = dt.Columns[0];
                var val = dt.Rows[0][col];
                sb.AppendLine("[KẾT QUẢ TRUY VẤN CƠ SỞ DỮ LIỆU]");

                string displayVal = val?.ToString() ?? string.Empty;
                if (val is decimal decVal && (col.ColumnName.IndexOf("SOTIEN", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                              col.ColumnName.IndexOf("LUONG", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    displayVal = decVal.ToString("N0") + " VNĐ";
                }

                sb.AppendLine($"- Giá trị ({col.ColumnName}): {displayVal}");
                sb.AppendLine("(Đây chính là kết quả thống kê tương ứng cho câu hỏi của người dùng)");
                return sb.ToString();
            }

            int limit = dt.Columns.Count <= 3 ? 50 : 12;
            int maxRows = Math.Min(dt.Rows.Count, limit);

            for (int r = 0; r < maxRows; r++)
            {
                sb.AppendLine($"--- Bản ghi #{r + 1} ---");
                foreach (DataColumn col in dt.Columns)
                {
                    var val = dt.Rows[r][col];
                    if (val == DBNull.Value || val == null) continue;

                    string friendlyName = ColumnLabels.TryGetValue(col.ColumnName, out var name)
                        ? name
                        : col.ColumnName;

                    string displayVal = val.ToString();
                    if (val is DateTime dtVal)
                    {
                        displayVal = dtVal.ToString("dd/MM/yyyy");
                    }
                    else if (val is decimal decVal && (col.ColumnName.IndexOf("SOTIEN", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                                       col.ColumnName.IndexOf("LUONG", StringComparison.OrdinalIgnoreCase) >= 0))
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
    }
}
