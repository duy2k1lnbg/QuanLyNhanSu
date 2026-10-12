using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Services.AI_Services.Retrieval.Sql
{
    public class AiViewColumn
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Description { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsMaskedByDefault { get; set; }
        public string[] AllowedOperations { get; set; } = new[] { "SELECT", "FILTER" };
    }

    public class AiViewDescriptor
    {
        public string ViewName { get; set; }
        public string Domain { get; set; }
        public string Description { get; set; }
        public string Grain { get; set; }
        public string[] PrimaryKeys { get; set; }
        public string RequiredCapability { get; set; }
        public List<AiViewColumn> Columns { get; set; } = new List<AiViewColumn>();
        public string[] AllowedJoinPaths { get; set; } = new string[0];
    }

    /// <summary>
    /// Schema Catalog chuẩn có kiểu mạnh (Strongly-Typed) mô tả toàn bộ 14 Views AI_OWNER (V1_25).
    /// Cung cấp thông tin chi tiết về Grain, Cột nhạy cảm, Phân quyền Capability và Join Paths được duyệt.
    /// </summary>
    public static class AiSchemaCatalog
    {
        private static readonly Dictionary<string, AiViewDescriptor> _views = new Dictionary<string, AiViewDescriptor>(StringComparer.OrdinalIgnoreCase);

        static AiSchemaCatalog()
        {
            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_EMPLOYEE",
                Domain = "EMPLOYEE",
                Description = "Hồ sơ nhân viên đầy đủ (yêu cầu EMPLOYEE_PROFILE)",
                Grain = "Mỗi dòng là 1 nhân viên hiện tại",
                PrimaryKeys = new[] { "MANV" },
                RequiredCapability = "EMPLOYEE_PROFILE",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân sự", IsPrimaryKey = true },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ và tên nhân viên" },
                    new AiViewColumn { Name = "NGAYSINH", DataType = "DATE", Description = "Ngày tháng năm sinh", IsMaskedByDefault = true },
                    new AiViewColumn { Name = "DIENTHOAI", DataType = "VARCHAR2", Description = "Số điện thoại liên hệ", IsMaskedByDefault = true },
                    new AiViewColumn { Name = "DIACHI", DataType = "VARCHAR2", Description = "Địa chỉ thường trú", IsMaskedByDefault = true },
                    new AiViewColumn { Name = "TEN_PHONGBAN", DataType = "VARCHAR2", Description = "Tên phòng ban trực thuộc" },
                    new AiViewColumn { Name = "TEN_BOPHAN", DataType = "VARCHAR2", Description = "Tên bộ phận làm việc" },
                    new AiViewColumn { Name = "TEN_CHUCVU", DataType = "VARCHAR2", Description = "Tên chức vụ" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_EMPLOYEE_LOOKUP",
                Domain = "EMPLOYEE",
                Description = "Tra cứu nhanh danh sách nhân viên công khai (không lộ PII)",
                Grain = "Mỗi dòng là 1 nhân viên",
                PrimaryKeys = new[] { "MANV" },
                RequiredCapability = "EMPLOYEE_LOOKUP",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân viên", IsPrimaryKey = true },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ tên" },
                    new AiViewColumn { Name = "TEN_PHONGBAN", DataType = "VARCHAR2", Description = "Phòng ban" },
                    new AiViewColumn { Name = "TEN_CHUCVU", DataType = "VARCHAR2", Description = "Chức vụ" },
                    new AiViewColumn { Name = "DATHOIVIEC", DataType = "NUMBER", Description = "0: đang làm việc, 1: đã thôi việc" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_EMPLOYEE_COUNT",
                Domain = "EMPLOYEE",
                Description = "Tổng hợp số lượng nhân sự theo phòng ban hoặc toàn công ty",
                Grain = "Tổng hợp nhóm theo phòng ban hoặc công ty (không danh tính cá nhân)",
                PrimaryKeys = new[] { "IDPB" },
                RequiredCapability = "EMPLOYEE_COUNT",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "IDPB", DataType = "NUMBER", Description = "Mã phòng ban", IsPrimaryKey = true },
                    new AiViewColumn { Name = "TEN_PHONGBAN", DataType = "VARCHAR2", Description = "Tên phòng ban" },
                    new AiViewColumn { Name = "TOTAL_COUNT", DataType = "NUMBER", Description = "Số lượng nhân viên" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_ATTENDANCE",
                Domain = "ATTENDANCE",
                Description = "Chi tiết lượt chấm công hàng ngày",
                Grain = "Mỗi dòng là 1 lượt quẹt thẻ / ngày của nhân viên",
                PrimaryKeys = new[] { "MANV", "NGAY", "THANG", "NAM" },
                RequiredCapability = "ATTENDANCE_DETAIL",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân sự", IsPrimaryKey = true },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ tên" },
                    new AiViewColumn { Name = "TEN_PHONGBAN", DataType = "VARCHAR2", Description = "Phòng ban" },
                    new AiViewColumn { Name = "NGAY", DataType = "NUMBER", Description = "Ngày" },
                    new AiViewColumn { Name = "THANG", DataType = "NUMBER", Description = "Tháng" },
                    new AiViewColumn { Name = "NAM", DataType = "NUMBER", Description = "Năm" },
                    new AiViewColumn { Name = "TIME_IN", DataType = "VARCHAR2", Description = "Giờ vào (HH:MM)" },
                    new AiViewColumn { Name = "TIME_OUT", DataType = "VARCHAR2", Description = "Giờ ra (HH:MM)" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_ATTENDANCE_SUMMARY",
                Domain = "ATTENDANCE",
                Description = "Tổng hợp công, ngày phép, nghỉ không phép theo kỳ",
                Grain = "Mỗi dòng là 1 kỳ công của 1 nhân viên",
                PrimaryKeys = new[] { "MAKYCONG", "MANV" },
                RequiredCapability = "ATTENDANCE_SUMMARY",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "MAKYCONG", DataType = "NUMBER", Description = "Mã kỳ công (VD: 202408)" },
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân viên", IsPrimaryKey = true },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ tên" },
                    new AiViewColumn { Name = "TEN_PHONGBAN", DataType = "VARCHAR2", Description = "Phòng ban" },
                    new AiViewColumn { Name = "THANG", DataType = "NUMBER", Description = "Tháng" },
                    new AiViewColumn { Name = "NAM", DataType = "NUMBER", Description = "Năm" },
                    new AiViewColumn { Name = "TONGNGAYCONG", DataType = "NUMBER", Description = "Tổng ngày công thực tế" },
                    new AiViewColumn { Name = "NGAYPHEP", DataType = "NUMBER", Description = "Số ngày nghỉ phép có lương" },
                    new AiViewColumn { Name = "NGHIKHONGPHEP", DataType = "NUMBER", Description = "Số ngày nghỉ không phép" },
                    new AiViewColumn { Name = "CONGNGAYLE", DataType = "NUMBER", Description = "Công ngày lễ" },
                    new AiViewColumn { Name = "CONGCHUNHAT", DataType = "NUMBER", Description = "Công chủ nhật" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_OVERTIME",
                Domain = "OVERTIME",
                Description = "Chi tiết các ca tăng ca của nhân viên",
                Grain = "Mỗi dòng là 1 lượt tăng ca",
                PrimaryKeys = new[] { "IDTCA" },
                RequiredCapability = "OVERTIME_VIEW",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân sự" },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ tên" },
                    new AiViewColumn { Name = "TEN_PHONGBAN", DataType = "VARCHAR2", Description = "Phòng ban" },
                    new AiViewColumn { Name = "NGAY", DataType = "NUMBER", Description = "Ngày" },
                    new AiViewColumn { Name = "THANG", DataType = "NUMBER", Description = "Tháng" },
                    new AiViewColumn { Name = "NAM", DataType = "NUMBER", Description = "Năm" },
                    new AiViewColumn { Name = "SOGIO", DataType = "NUMBER", Description = "Số giờ làm thêm" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_OVERTIME_SUMMARY",
                Domain = "OVERTIME",
                Description = "Tổng hợp số giờ tăng ca của nhân viên theo kỳ",
                Grain = "Mỗi dòng là tổng số giờ tăng ca của nhân viên trong kỳ",
                PrimaryKeys = new[] { "MANV", "THANG", "NAM" },
                RequiredCapability = "OVERTIME_SUM",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân viên", IsPrimaryKey = true },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ tên" },
                    new AiViewColumn { Name = "TEN_PHONGBAN", DataType = "VARCHAR2", Description = "Phòng ban" },
                    new AiViewColumn { Name = "THANG", DataType = "NUMBER", Description = "Tháng" },
                    new AiViewColumn { Name = "NAM", DataType = "NUMBER", Description = "Năm" },
                    new AiViewColumn { Name = "TONG_SOGIO", DataType = "NUMBER", Description = "Tổng số giờ tăng ca" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_PAYROLL",
                Domain = "PAYROLL",
                Description = "Chi tiết bảng lương tháng của nhân viên",
                Grain = "Mỗi dòng là 1 phiếu lương tháng của nhân viên",
                PrimaryKeys = new[] { "MAKYCONG", "MANV" },
                RequiredCapability = "PAYROLL_VIEW",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "MAKYCONG", DataType = "NUMBER", Description = "Mã kỳ công" },
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân viên", IsPrimaryKey = true },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ tên" },
                    new AiViewColumn { Name = "TEN_PHONGBAN", DataType = "VARCHAR2", Description = "Phòng ban" },
                    new AiViewColumn { Name = "THANG", DataType = "NUMBER", Description = "Tháng" },
                    new AiViewColumn { Name = "NAM", DataType = "NUMBER", Description = "Năm" },
                    new AiViewColumn { Name = "LUONG_CONG_THUCTE", DataType = "NUMBER", Description = "Lương theo ngày công thực tế" },
                    new AiViewColumn { Name = "THUCLANH", DataType = "NUMBER", Description = "Số tiền thực lĩnh", IsMaskedByDefault = true },
                    new AiViewColumn { Name = "NGAYCONG_THUCTE", DataType = "NUMBER", Description = "Ngày công tính lương" },
                    new AiViewColumn { Name = "TRANGTHAI_CHITRA", DataType = "VARCHAR2", Description = "Trạng thái chi trả" },
                    new AiViewColumn { Name = "IS_LOCKED", DataType = "NUMBER", Description = "Khóa bảng lương" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_PAYROLL_SUMMARY",
                Domain = "PAYROLL",
                Description = "Quỹ lương tổng hợp theo kỳ",
                Grain = "Tổng hợp toàn bộ quỹ lương hoặc theo phòng ban theo kỳ",
                PrimaryKeys = new[] { "THANG", "NAM" },
                RequiredCapability = "PAYROLL_SUMMARY",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "THANG", DataType = "NUMBER", Description = "Tháng" },
                    new AiViewColumn { Name = "NAM", DataType = "NUMBER", Description = "Năm" },
                    new AiViewColumn { Name = "TONG_THUCLANH", DataType = "NUMBER", Description = "Tổng quỹ lương thực lĩnh" },
                    new AiViewColumn { Name = "TONG_LUONG_CONG_THUCTE", DataType = "NUMBER", Description = "Tổng quỹ lương công thực tế" },
                    new AiViewColumn { Name = "SO_NHANVIEN", DataType = "NUMBER", Description = "Số nhân viên được tính lương" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_ALLOWANCE",
                Domain = "ALLOWANCE",
                Description = "Danh sách phụ cấp của nhân viên",
                Grain = "Mỗi dòng là 1 khoản phụ cấp áp dụng cho nhân viên",
                PrimaryKeys = new[] { "MANV", "IDPC" },
                RequiredCapability = "ALLOWANCE_VIEW",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân sự", IsPrimaryKey = true },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ tên" },
                    new AiViewColumn { Name = "TEN_PHONGBAN", DataType = "VARCHAR2", Description = "Phòng ban" },
                    new AiViewColumn { Name = "TENPC", DataType = "VARCHAR2", Description = "Tên loại phụ cấp" },
                    new AiViewColumn { Name = "SOTIEN", DataType = "NUMBER", Description = "Số tiền phụ cấp" },
                    new AiViewColumn { Name = "TU_NGAY", DataType = "DATE", Description = "Ngày bắt đầu áp dụng" },
                    new AiViewColumn { Name = "DEN_NGAY", DataType = "DATE", Description = "Ngày kết thúc áp dụng" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_ADVANCE",
                Domain = "ADVANCE",
                Description = "Các khoản tạm ứng lương",
                Grain = "Mỗi dòng là 1 phiếu tạm ứng",
                PrimaryKeys = new[] { "IDUL" },
                RequiredCapability = "ADVANCE_VIEW",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân sự" },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ tên" },
                    new AiViewColumn { Name = "NGAY", DataType = "NUMBER", Description = "Ngày" },
                    new AiViewColumn { Name = "THANG", DataType = "NUMBER", Description = "Tháng" },
                    new AiViewColumn { Name = "NAM", DataType = "NUMBER", Description = "Năm" },
                    new AiViewColumn { Name = "SOTIEN", DataType = "NUMBER", Description = "Số tiền tạm ứng" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_INSURANCE",
                Domain = "INSURANCE",
                Description = "Thông tin sổ bảo hiểm xã hội / y tế",
                Grain = "Mỗi dòng là 1 hồ sơ bảo hiểm của nhân viên",
                PrimaryKeys = new[] { "MANV", "IDBH" },
                RequiredCapability = "INSURANCE_VIEW",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân sự", IsPrimaryKey = true },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ tên" },
                    new AiViewColumn { Name = "SOBH", DataType = "VARCHAR2", Description = "Số sổ bảo hiểm" },
                    new AiViewColumn { Name = "NGAYCAP", DataType = "DATE", Description = "Ngày cấp" },
                    new AiViewColumn { Name = "NOICAP", DataType = "VARCHAR2", Description = "Nơi cấp" },
                    new AiViewColumn { Name = "NOIKHAMBENH", DataType = "VARCHAR2", Description = "Nơi đăng ký khám chữa bệnh ban đầu" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_CONTRACT",
                Domain = "CONTRACT",
                Description = "Hợp đồng lao động và thời hạn hợp đồng",
                Grain = "Mỗi dòng là 1 hợp đồng lao động đã ký",
                PrimaryKeys = new[] { "SOHD" },
                RequiredCapability = "CONTRACT_VIEW",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "SOHD", DataType = "VARCHAR2", Description = "Số hợp đồng", IsPrimaryKey = true },
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân sự" },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ tên" },
                    new AiViewColumn { Name = "TEN_PHONGBAN", DataType = "VARCHAR2", Description = "Phòng ban" },
                    new AiViewColumn { Name = "NGAYBATDAU", DataType = "DATE", Description = "Ngày bắt đầu" },
                    new AiViewColumn { Name = "NGAYKETTHUC", DataType = "DATE", Description = "Ngày kết thúc" },
                    new AiViewColumn { Name = "NGAYKY", DataType = "DATE", Description = "Ngày ký" },
                    new AiViewColumn { Name = "LANKY", DataType = "NUMBER", Description = "Lần ký hợp đồng" },
                    new AiViewColumn { Name = "HESOLUONG", DataType = "NUMBER", Description = "Hệ số lương" }
                }
            });

            Register(new AiViewDescriptor
            {
                ViewName = "V_AI_SALARY_CHANGE",
                Domain = "SALARY_CHANGE",
                Description = "Lịch sử quyết định nâng lương / điều chỉnh hệ số lương",
                Grain = "Mỗi dòng là 1 quyết định thay đổi lương",
                PrimaryKeys = new[] { "SOQD" },
                RequiredCapability = "SALARY_CHANGE_VIEW",
                Columns = new List<AiViewColumn>
                {
                    new AiViewColumn { Name = "SOQD", DataType = "VARCHAR2", Description = "Số quyết định", IsPrimaryKey = true },
                    new AiViewColumn { Name = "SOHD", DataType = "VARCHAR2", Description = "Số hợp đồng liên quan" },
                    new AiViewColumn { Name = "MANV", DataType = "NUMBER", Description = "Mã nhân viên" },
                    new AiViewColumn { Name = "HOTEN", DataType = "VARCHAR2", Description = "Họ tên" },
                    new AiViewColumn { Name = "TEN_PHONGBAN", DataType = "VARCHAR2", Description = "Phòng ban" },
                    new AiViewColumn { Name = "NGAYKY", DataType = "DATE", Description = "Ngày ký" },
                    new AiViewColumn { Name = "NGAYLENLUONG", DataType = "DATE", Description = "Ngày có hiệu lực tăng lương" },
                    new AiViewColumn { Name = "HESOLUONGHIENTAI", DataType = "NUMBER", Description = "Hệ số lương cũ" },
                    new AiViewColumn { Name = "HESOLUONGMOI", DataType = "NUMBER", Description = "Hệ số lương mới" }
                }
            });
        }

        private static void Register(AiViewDescriptor descriptor)
        {
            _views[descriptor.ViewName] = descriptor;
        }

        public static IEnumerable<AiViewDescriptor> GetAllViews() => _views.Values;

        public static AiViewDescriptor GetView(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName)) return null;
            return _views.TryGetValue(viewName.Trim(), out var desc) ? desc : null;
        }

        public static string GenerateFullSchemaDocumentation()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[AI DATABASE - READ ONLY ENTERPRISE SCHEMA CATALOG V1_25]");
            sb.AppendLine("Chỉ sử dụng các VIEW dưới đây với tiền tố AI_OWNER. Tuyệt đối không query trực tiếp table.");
            sb.AppendLine("==================================================");

            int idx = 1;
            foreach (var v in _views.Values)
            {
                sb.AppendLine($"{idx++}. AI_OWNER.{v.ViewName} ({v.Description})");
                sb.AppendLine($"   Hạt dữ liệu (Grain): {v.Grain}");
                sb.AppendLine($"   Quyền yêu cầu: {v.RequiredCapability}");
                sb.AppendLine("   -----------------------------------------------");
                foreach (var c in v.Columns)
                {
                    string pk = c.IsPrimaryKey ? " [PK]" : "";
                    string mask = c.IsMaskedByDefault ? " [MASKED]" : "";
                    sb.AppendLine($"   - {c.Name} ({c.DataType}){pk}{mask}: {c.Description}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
