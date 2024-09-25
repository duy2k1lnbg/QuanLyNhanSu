using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.DTO
{
    public class NHANVIEN_PHUCAP_DTO
    {
        public decimal IDPC { get; set; }
        public string TENPC { get; set; }
        public Nullable<decimal> SOTIEN { get; set; }
        public decimal ID { get; set; }
        public Nullable<decimal> MANV { get; set; }
        public string HOTEN { get; set; }
        public Nullable<System.DateTime> NGAY { get; set; }
        public string GHICHU { get; set; }
        public Nullable<decimal> TONGPHUCAP { get; set; }
        public Nullable<decimal> UPDATED_BY { get; set; }
        public Nullable<System.DateTime> UPDATED_DATE { get; set; }
        public Nullable<decimal> CREATED_BY { get; set; }
        public Nullable<System.DateTime> CREATED_DATE { get; set; }
        public Nullable<decimal> DELETED_BY { get; set; }
        public Nullable<System.DateTime> DELETED_DATE { get; set; }
        public List<string> PHUCAP { get; set; }
        public decimal? PhuCap1 { get; set; } // Số tiền phụ cấp 1
        public decimal? PhuCap2 { get; set; } // Số tiền phụ cấp 2
        public decimal? PhuCap3 { get; set; } // Số tiền phụ cấp 3
        public decimal? PhuCap4 { get; set; } // Số tiền phụ cấp 4
        public decimal? PhuCap5 { get; set; } // Số tiền phụ cấp 5
        public decimal? PhuCap6 { get; set; } // Số tiền phụ cấp 6
        public decimal? PhuCap7 { get; set; } // Số tiền phụ cấp 7
    }
}
