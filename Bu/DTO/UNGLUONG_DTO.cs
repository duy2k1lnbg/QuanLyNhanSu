using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.DTO
{
    public class UNGLUONG_DTO
    {
        public decimal IDUL { get; set; }
        public Nullable<decimal> NAM { get; set; }
        public Nullable<decimal> THANG { get; set; }
        public Nullable<decimal> NGAY { get; set; }
        public Nullable<decimal> SOTIENUNG { get; set; }
        public Nullable<decimal> MANV { get; set; }
        public string HOTEN { get; set; }
        public string GHICHU { get; set; }
        public Nullable<decimal> CREATED_BY { get; set; }
        public Nullable<System.DateTime> CREATED_DATE { get; set; }
        public Nullable<decimal> UPDATED_BY { get; set; }
        public Nullable<System.DateTime> UPDATED_DATE { get; set; }
        public Nullable<decimal> DELETED_BY { get; set; }
        public Nullable<System.DateTime> DELETED_DATE { get; set; }
    }
}
