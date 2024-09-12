using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.DTO
{
    public class HOPDONG_DTO
    {
        public Nullable<System.DateTime> NGAYBATDAU { get; set; }
        public Nullable<System.DateTime> NGAYKETTHUC { get; set; }
        public Nullable<System.DateTime> NGAYKY { get; set; }
        public Nullable<decimal> LANKY { get; set; }
        public string THOIHAN { get; set; }
        public Nullable<decimal> HESOLUONG { get; set; }
        public Nullable<decimal> MANV { get; set; }
        public string SOHD { get; set; }
        public Nullable<decimal> IDCTY { get; set; }
        public Nullable<decimal> DEL_BY { get; set; }
        public Nullable<System.DateTime> DEL_DATE { get; set; }
        public Nullable<decimal> UPDATE_BY { get; set; }
        public Nullable<System.DateTime> UPDATE_DATE { get; set; }
        public Nullable<decimal> CREATED_BY { get; set; }
        public Nullable<System.DateTime> CREATED_DATE { get; set; }
        public string NOIDUNG { get; set; }

        public virtual TB_NHANVIEN TB_NHANVIEN { get; set; }
        public string HOTEN { get; set; }
    }
}
