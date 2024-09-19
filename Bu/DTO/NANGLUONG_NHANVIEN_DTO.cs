using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.DTO
{
    public class NANGLUONG_NHANVIEN_DTO
    {
        public string SOQDNL { get; set; }
        public string SOHD { get; set; }
        public Nullable<decimal> MANV { get; set; }
        public string HOTEN { get; set; }
        public Nullable<decimal> HESOLUONG_NOW { get; set; }
        public Nullable<decimal> HESOLUONG_NEW { get; set; }
        public Nullable<System.DateTime> NGAYLENLUONG { get; set; }
        public Nullable<System.DateTime> NGAYKYNL { get; set; }
        public string GHICHUNL { get; set; }
        public Nullable<decimal> CREATED_BY { get; set; }
        public Nullable<System.DateTime> CREATED_DATE { get; set; }
        public Nullable<decimal> UPDATED_BY { get; set; }
        public Nullable<System.DateTime> UPDATED_DATE { get; set; }
        public Nullable<decimal> DELETED_BY { get; set; }
        public Nullable<System.DateTime> DELETED_DATE { get; set; }
    }
}
