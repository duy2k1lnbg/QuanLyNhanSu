using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.DTO
{
    public class NHANVIEN_PHUCAP_DTO
    {
        public decimal MANV { get; set; }
        public string HOTEN { get; set; }
        public decimal IDPC { get; set; }
        public string TENPC { get; set; }
        public decimal MAKYCONG { get; set; }
        public string GHICHU { get; set; }
        public Nullable<decimal> SOTIEN { get; set; }
        public Nullable<decimal> UPDATED_BY { get; set; }
        public Nullable<System.DateTime> UPDATED_DATE { get; set; }
        public Nullable<decimal> CREATED_BY { get; set; }
        public Nullable<System.DateTime> CREATED_DATE { get; set; }
        public Nullable<decimal> DELETED_BY { get; set; }
        public Nullable<System.DateTime> DELETED_DATE { get; set; }

        public decimal? SOTIEN_IDPC1 { get; set; }  // Phụ cấp nhà ở
        public decimal? SOTIEN_IDPC2 { get; set; }  // Phụ cấp đi lại
        public decimal? SOTIEN_IDPC3 { get; set; }  // Phụ cấp gia đình
        public decimal? SOTIEN_IDPC4 { get; set; }  // Phụ cấp người phụ thuộc
        public decimal? SOTIEN_IDPC5 { get; set; }  // Phụ cấp chức vụ
        public decimal? SOTIEN_IDPC6 { get; set; }  // Phụ cấp chứng chỉ
        public decimal? SOTIEN_IDPC7 { get; set; }  // Phụ cấp kỹ năng
        public decimal? SOTIEN_IDPC8 { get; set; }  // Phụ cấp khu vực
        public decimal? SOTIEN_IDPC9 { get; set; }  // Phụ cấp chuyên cần
        public decimal? SOTIEN_IDPC10 { get; set; } // Phụ cấp thâm niên
        public decimal? SOTIEN_IDPC11 { get; set; } // Phụ cấp làm việc tại nhà
        public decimal? SOTIEN_IDPC12 { get; set; } // Phụ cấp đặc biệt
        public decimal? SOTIEN_IDPC13 { get; set; } // Phụ cấp khác

        public decimal TONG_PHUCAP => (SOTIEN_IDPC1 ?? 0) + (SOTIEN_IDPC2 ?? 0) + (SOTIEN_IDPC3 ?? 0) +
                                      (SOTIEN_IDPC4 ?? 0) + (SOTIEN_IDPC5 ?? 0) + (SOTIEN_IDPC6 ?? 0) +
                                      (SOTIEN_IDPC7 ?? 0) + (SOTIEN_IDPC8 ?? 0) + (SOTIEN_IDPC9 ?? 0) +
                                      (SOTIEN_IDPC10 ?? 0) + (SOTIEN_IDPC11 ?? 0) + (SOTIEN_IDPC12 ?? 0) +
                                      (SOTIEN_IDPC13 ?? 0);
    }
}
