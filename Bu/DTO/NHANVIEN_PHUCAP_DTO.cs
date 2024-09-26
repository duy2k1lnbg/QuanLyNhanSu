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
        public Nullable<System.DateTime> NGAY { get; set; }
        public string GHICHU { get; set; }
        public Nullable<decimal> SOTIEN { get; set; }
        public Nullable<decimal> UPDATED_BY { get; set; }
        public Nullable<System.DateTime> UPDATED_DATE { get; set; }
        public Nullable<decimal> CREATED_BY { get; set; }
        public Nullable<System.DateTime> CREATED_DATE { get; set; }
        public Nullable<decimal> DELETED_BY { get; set; }
        public Nullable<System.DateTime> DELETED_DATE { get; set; }

        public decimal? SOTIEN_IDPC1 { get; set; }
        public decimal? SOTIEN_IDPC2 { get; set; }
        public decimal? SOTIEN_IDPC3 { get; set; }
        public decimal? SOTIEN_IDPC4 { get; set; }
        public decimal? SOTIEN_IDPC5 { get; set; }
        public decimal? SOTIEN_IDPC6 { get; set; }
        public decimal? SOTIEN_IDPC7 { get; set; }


    }
}
