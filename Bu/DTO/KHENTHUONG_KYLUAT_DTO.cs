using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.DTO
{
    public class KHENTHUONG_KYLUAT_DTO
    {
        public string NOIDUNG { get; set; }
        public Nullable<System.DateTime> NGAY { get; set; }
        public Nullable<decimal> MANV { get; set; }
        public string HOTEN { get; set; }
        public Nullable<decimal> LOAI { get; set; }
        public string LYDO { get; set; }
        public Nullable<System.DateTime> TUNGAY { get; set; }
        public Nullable<System.DateTime> DENNGAY { get; set; }
        public Nullable<decimal> CREATED_BY { get; set; }
        public Nullable<System.DateTime> CREATED_DATE { get; set; }
        public Nullable<decimal> UPDATED_BY { get; set; }
        public Nullable<System.DateTime> UPDATED_DATE { get; set; }
        public Nullable<decimal> DELETED_BY { get; set; }
        public Nullable<System.DateTime> DELETED_DATE { get; set; }
        public string SOQUYETDINH { get; set; }
    }
}
