using System;

namespace Bu.DTO
{
    public class PhuCapDTO
    {
        public decimal IDPC { get; set; }
        public string TENPC_VI { get; set; }  // Tên gốc tiếng Việt
        public string TENPC { get; set; }     // Tên đã dịch (nếu có, hoặc fallback về tiếng Việt)
        public string DESCRIPTION { get; set; }
    }
}
