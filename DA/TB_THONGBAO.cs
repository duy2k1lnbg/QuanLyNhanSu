namespace DA
{
    using System;
    using System.Collections.Generic;

    public partial class TB_THONGBAO
    {
        public decimal ID { get; set; }
        public string TIEUDE { get; set; }
        public string NOIDUNG { get; set; }
        public string NGUOIDANG { get; set; }
        public DateTime NGAYDANG { get; set; }
        public string LOAI_TB { get; set; }
        public Nullable<decimal> IS_PINNED { get; set; }
        public Nullable<decimal> TRANGTHAI { get; set; }
        public Nullable<DateTime> NGAY_HETHAN { get; set; }
        public string FILE_DINHKEM { get; set; }
        public string MACTY { get; set; }
        public string MAPB { get; set; }
    }
}
