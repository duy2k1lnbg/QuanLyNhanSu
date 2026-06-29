using System;

namespace Bu.DTO
{
    public class ChucVuDTO
    {
        public decimal IDCV { get; set; }
        public string TENCV_VI { get; set; }  // Tên gốc tiếng Việt
        public string TENCV { get; set; }     // Tên đã dịch (nếu có, hoặc fallback về tiếng Việt)
        public string DESCRIPTION { get; set; }
    }
}
