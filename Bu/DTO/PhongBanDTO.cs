using System;

namespace Bu.DTO
{
    public class PhongBanDTO
    {
        public decimal IDPB { get; set; }
        public string TENPB_VI { get; set; }  // Tên gốc tiếng Việt
        public string TENPB { get; set; }     // Tên đã dịch (nếu có, hoặc fallback về tiếng Việt)
        public string DESCRIPTION { get; set; }
    }
}
