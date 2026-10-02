using System;

namespace Bu.DTO
{
    public class LuongHieuLucDTO
    {
        public decimal ID { get; set; }
        public decimal MANV { get; set; }
        public string SOHD { get; set; }
        public DateTime TU_NGAY { get; set; }
        public DateTime? DEN_NGAY { get; set; }
        public decimal LUONG_THANG { get; set; }
        public string CAN_CU { get; set; }
        public string HOTEN { get; set; }
    }
}
