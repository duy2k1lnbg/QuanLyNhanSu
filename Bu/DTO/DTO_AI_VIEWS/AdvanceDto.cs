using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.DTO.DTO_AI_VIEWS
{
    public class AdvanceDto : BaseAiDto
    {
        public decimal? NGAY { get; set; }
        public decimal? THANG { get; set; }
        public decimal? NAM { get; set; }
        public decimal? SOTIEN { get; set; }
        public string NgayHienThi => $"{NGAY}/{THANG}/{NAM}";
    }

    // 2. Phụ cấp
    public class AllowanceDto : BaseAiDto
    {
        public string TENPC { get; set; }
        public decimal? SOTIEN { get; set; }
        public decimal KYCONG { get; set; }
    }

    // 3. Chấm công
    public class AttendanceDto : BaseAiDto
    {
        public string TEN_PHONGBAN { get; set; }
        public decimal? NGAY { get; set; }
        public decimal? THANG { get; set; }
        public decimal? NAM { get; set; }
        public string TIME_IN { get; set; }
        public string TIME_OUT { get; set; }
        public string NgayHienThi => $"{NGAY}/{THANG}/{NAM}";
    }

    // 4. Thông tin nhân viên (Chủ chốt)
    public class EmployeeDto : BaseAiDto
    {
        public DateTime? NGAYSINH { get; set; }
        public string DIENTHOAI { get; set; }
        public string DIACHI { get; set; }
        public string TEN_PHONGBAN { get; set; }
        public string TEN_BOPHAN { get; set; }
        public string TEN_CHUCVU { get; set; }
    }

    // 5. Bảo hiểm
    public class InsuranceDto : BaseAiDto
    {
        public string SOBH { get; set; }
        public DateTime? NGAYCAP { get; set; }
        public string NOICAP { get; set; }
        public string NOIKHAMBENH { get; set; }
    }

    // 6. Tăng ca
    public class OvertimeDto : BaseAiDto
    {
        public string TEN_PHONGBAN { get; set; }
        public decimal? NGAY { get; set; }
        public decimal? THANG { get; set; }
        public decimal? NAM { get; set; }
        public decimal? SOGIO { get; set; }
        public string NgayHienThi => $"{NGAY}/{THANG}/{NAM}";
    }
}
