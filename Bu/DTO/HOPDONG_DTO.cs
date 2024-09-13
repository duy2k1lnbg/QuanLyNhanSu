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
        public string NGAYBATDAU { get; set; }
        public string NGAYKETTHUC { get; set; }
        public string NGAYKY { get; set; }
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
        public string DIENTHOAI { get; set; }
        public string CCCD { get; set; }
        public string DIACHI { get; set; }
        public string NGAYSINH { get; set; }
        public Nullable<decimal> IDTD { get; set; }
        public string TENTD { get; set; }
        public Nullable<decimal> IDQT { get; set; }
        public string TENQT { get; set; }
        public Nullable<decimal> IDBP { get; set; }
        public string TENBP { get; set; }
        public Nullable<decimal> IDCV { get; set; }
        public string TENCV { get; set; }
        public Nullable<decimal> IDPB { get; set; }
        public string TENPB { get; set; }
        public string TENCTY { get; set; }
        public string DAIDIEN { get; set; }
        public string MASOTHUECTY { get; set; }
        public string DIACHICTY { get; set; }
        public string DIENTHOAICTY { get; set; }

    }
}
