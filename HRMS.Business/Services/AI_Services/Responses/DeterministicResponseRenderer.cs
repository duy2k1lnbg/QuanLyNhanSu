using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Bu.Services.AI_Services.Interfaces;
namespace Bu.Services.AI_Services.Core
{
    public class RenderedResponse
    {
        public string Status { get; set; }
        public string Answer { get; set; }
        public string SourceProvenance { get; set; }
        public int? TotalRecords { get; set; }
        public bool HasMore { get; set; }
        public bool BypassedLlm { get; set; } = true;
        public DateTime? AsOf { get; set; }
        public int HttpStatus { get; set; } = 500;
        public DataTable Data { get; set; }
    }
    public static class DeterministicResponseRenderer
    {
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("vi-VN");
        private static readonly Dictionary<string,string> Labels = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase) {
            {"TOTAL_COUNT","Số nhân viên"},{"MANV","Mã nhân viên"},{"HOTEN","Họ tên"},{"TEN_PHONGBAN","Phòng ban"},{"TEN_CHUCVU","Chức vụ"},{"TENPC","Phụ cấp"},
            {"SOTIEN","Số tiền (VNĐ)"},{"SOGIO","Số giờ"},{"TONG_SOGIO","Tổng số giờ"},{"NGAY","Ngày trong tháng"},{"THANG","Tháng"},{"NAM","Năm"},
            {"NGAYSINH","Ngày sinh"},{"BIRTHDAY_DAY","Ngày sinh nhật"},{"BIRTHDAY_MONTH","Tháng sinh nhật"},{"DIENTHOAI","Điện thoại"},{"DIACHI","Địa chỉ"},
            {"THUCLANH","Thực lĩnh (VNĐ)"},{"LUONG_CONG_THUCTE","Tiền lương theo công thực tế (VNĐ)"},{"NGAYCONG_THUCTE","Ngày công thực tế"},
            {"TRANGTHAI_CHITRA","Trạng thái chi trả"},{"IS_LOCKED","Khóa sổ"},{"MAKYCONG","Mã kỳ công"},
            {"TONGNGAYCONG","Tổng ngày công"},{"NGAYPHEP","Ngày phép"},{"NGHIKHONGPHEP","Nghỉ không phép"},{"CONGNGAYLE","Công ngày lễ"},{"CONGCHUNHAT","Công Chủ nhật"},
            {"SOHD","Số hợp đồng"},{"NGAYBATDAU","Ngày bắt đầu"},{"NGAYKETTHUC","Ngày kết thúc"},{"NGAYKY","Ngày ký"},{"LANKY","Lần ký"},{"HESOLUONG","Hệ số lương"},
            {"SOQD","Số quyết định"},{"NGAYLENLUONG","Ngày nâng lương"},{"HESOLUONGHIENTAI","Hệ số hiện tại"},{"HESOLUONGMOI","Hệ số mới"},
            {"SOBH","Số bảo hiểm"},{"NGAYCAP","Ngày cấp"},{"NOICAP","Nơi cấp"},{"NOIKHAMBENH","Nơi khám bệnh"},{"TU_NGAY","Áp dụng từ"},{"DEN_NGAY","Áp dụng đến"}
        };
        public static RenderedResponse Render(QueryExecutionPlan p, SqlExecutionResult sql)
        {
            if (p == null) return Error("Không có kế hoạch truy vấn.",500);
            if (p.Strategy == ExecutionStrategy.NeedsClarification) return new RenderedResponse { Status="needs_clarification", Answer=p.Clarification?.Question ?? "Cần làm rõ yêu cầu.", HttpStatus=200 };
            if (p.Strategy == ExecutionStrategy.Forbidden) return new RenderedResponse { Status="forbidden", Answer=p.DenialOrUnsupportedReason ?? "Bạn chưa được cấp quyền truy cập.", HttpStatus=403 };
            if (p.Strategy == ExecutionStrategy.Unsupported || p.Strategy == ExecutionStrategy.VectorSearch) return new RenderedResponse { Status="unsupported", Answer=p.DenialOrUnsupportedReason ?? "Nguồn dữ liệu cho tính năng này chưa sẵn sàng.", HttpStatus=200 };
            if (sql == null) return Error("Không nhận được kết quả truy vấn.",500);
            if (sql.Status == SqlExecutionStatus.AuthorizationDenied) return new RenderedResponse { Status="forbidden", Answer="Truy vấn bị từ chối theo chính sách phân quyền dữ liệu AI.", HttpStatus=403 };
            if (sql.Status == SqlExecutionStatus.SourceUnavailable || sql.Status == SqlExecutionStatus.ConnectionError) return Error("Nguồn dữ liệu AI chưa sẵn sàng. Vui lòng liên hệ quản trị viên.",503);
            if (sql.Status == SqlExecutionStatus.Timeout) return Error("Truy vấn đã quá thời gian chờ. Vui lòng thu hẹp phạm vi.",504);
            if (sql.Status != SqlExecutionStatus.SuccessWithData && sql.Status != SqlExecutionStatus.SuccessEmpty) return Error("Có lỗi khi truy vấn dữ liệu. Vui lòng thử lại sau.",500);
            string source = PublicSource(p.Domain);
            string provenance = "Nguồn: " + source + " • Phạm vi: " + (p.EffectiveScopeDisplay ?? "Được cấp") + (p.EffectivePeriodDisplay != null ? " • Kỳ: " + p.EffectivePeriodDisplay : "");
            if (sql.Data == null || sql.Data.Rows.Count == 0) return new RenderedResponse { Status="no_data", Answer=(p.Domain == "PAYROLL" ? "Không có bảng lương đã khóa sổ với đợt tính thành công trong kỳ và phạm vi được phép." : "Không có dữ liệu phù hợp trong phạm vi được phép.") + (p.SelectedEntityDisplay != null ? " Đối tượng: " + p.SelectedEntityDisplay + "." : ""), SourceProvenance=provenance, TotalRecords=0, HttpStatus=200 };
            var table = sql.Data; var sb = new StringBuilder();
            if (p.IsScalar)
            {
                string measure = p.Operation == "COUNT" ? "TOTAL_COUNT" : p.Domain == "OVERTIME" ? "TONG_SOGIO" : p.Metric == "LUONG_CONG_THUCTE" ? "TONG_LUONG_CONG_THUCTE" : "TONG_THUCLANH";
                if (!table.Columns.Contains(measure)) return Error("Kết quả không khớp hợp đồng dữ liệu AI.",503);
                decimal value = table.Rows.Cast<DataRow>().Sum(r => r[measure] == DBNull.Value ? 0 : Convert.ToDecimal(r[measure]));
                sb.Append(p.Domain == "PAYROLL" ? (measure == "TONG_THUCLANH" ? "Tổng thực lĩnh kỳ công: " : "Tổng tiền lương theo công thực tế: ") : p.Domain == "OVERTIME" ? "Tổng giờ tăng ca: " : "Tổng nhân sự: ");
                if (!string.IsNullOrWhiteSpace(p.SelectedEntityDisplay)) sb.Append("\nĐối tượng: ").Append(p.SelectedEntityDisplay).Append("\nGiá trị: ");
                sb.Append(value.ToString("0.##",Culture)).Append(p.Domain == "PAYROLL" ? " VNĐ" : " " + p.ScalarUnit);
                if (p.Domain == "PAYROLL" && table.Columns.Contains("SO_NHANVIEN")) sb.Append("\nSố nhân sự trong các kỳ: ").Append(table.Rows.Cast<DataRow>().Sum(r => r["SO_NHANVIEN"] == DBNull.Value ? 0 : Convert.ToDecimal(r["SO_NHANVIEN"])).ToString("0",Culture));
            }
            else
            {
                sb.AppendLine(p.Domain == "CONTRACT" ? (p.Operation == "EXPIRING" ? "Hợp đồng hết hạn trong khoảng đã xác định:" : "Danh sách hợp đồng:") : p.Domain == "SALARY_CHANGE" ? "Quyết định nâng lương (hệ số không phải số tiền):" : "Kết quả tra cứu:");
                int n = 0;
                foreach (DataRow row in table.Rows.Cast<DataRow>().Take(p.RowLimit))
                {
                    sb.Append(++n).Append(". "); var parts = new List<string>();
                    foreach (DataColumn column in table.Columns)
                    {
                        string field = column.ColumnName;
                        if (!Labels.TryGetValue(field,out var label)) continue;
                        string mode = p.FieldModes.TryGetValue(field,out var access) ? access : (p.MaskedFields.Contains(field) ? "MASK" : "FULL");
                        if (mode == "DENY") continue;
                        var value = row[column];
                        string text = mode == "MASK" ? "***" : value == DBNull.Value ? "Chưa có" : value is DateTime date ? date.ToString("dd/MM/yyyy") : field == "TRANGTHAI_CHITRA" ? PaymentStatus(value.ToString()) : field == "IS_LOCKED" ? (Convert.ToInt32(value) == 1 ? "Đã khóa sổ" : "Chưa khóa sổ") : value is IFormattable formattable ? formattable.ToString("0.##",Culture) : value.ToString();
                        parts.Add(label + ": " + text.Replace("\r"," ").Replace("\n"," "));
                    }
                    sb.AppendLine(string.Join(" • ",parts));
                }
                if (sql.HasMore || table.Rows.Count > p.RowLimit) sb.AppendLine("Đang hiển thị " + p.RowLimit + " kết quả đầu tiên. Hãy thu hẹp đối tượng hoặc phạm vi để xem tiếp.");
            }
            return new RenderedResponse { Status="answered", Answer=sb.ToString().TrimEnd()+"\n\n"+provenance, SourceProvenance=source, TotalRecords=sql.TotalRecords, HasMore=sql.HasMore || (!p.IsScalar && table.Rows.Count > p.RowLimit), Data = p.IsScalar ? null : table, HttpStatus=200 };
        }
        private static RenderedResponse Error(string message,int status) => new RenderedResponse { Status="error", Answer=message, HttpStatus=status };
        private static string PaymentStatus(string raw)
        {
            switch (raw) { case "DA_CHI_TRA": return "Đã chi trả"; case "DANG_CHI_TRA": return "Đang chi trả"; case "CHUA_CHI_TRA": return "Chưa chi trả"; case "HUY_CHI_TRA": return "Hủy chi trả"; case "LOI_CHI_TRA": return "Lỗi chi trả"; case "APPROVED": return "Đã duyệt; chưa xác nhận chi trả"; default: return "Chưa xác định"; }
        }
        private static string PublicSource(string domain)
        {
            switch (domain) { case "EMPLOYEE": return "Hồ sơ nhân sự"; case "PAYROLL": return "Bảng lương kỳ công đã khóa sổ"; case "OVERTIME": return "Đăng ký tăng ca"; case "ATTENDANCE": return "Ngày công đã công bố"; case "ALLOWANCE": return "Phân bổ phụ cấp"; case "INSURANCE": return "Hồ sơ bảo hiểm"; case "ADVANCE": return "Tạm ứng lương"; case "CONTRACT": return "Hợp đồng lao động"; case "SALARY_CHANGE": return "Quyết định nâng lương"; default: return "Dữ liệu nhân sự"; }
        }
    }
}