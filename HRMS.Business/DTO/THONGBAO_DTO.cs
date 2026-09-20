using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.DTO
{
    public class THONGBAO_DTO
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
        public string TENCTY { get; set; }
        public string TENPB { get; set; }

        public string DisplayText
        {
            get
            {
                string prefix = IS_PINNED == 1 ? "📌 [GHIM] " : "";
                string loai = !string.IsNullOrEmpty(LOAI_TB) ? $"[{LOAI_TB}] " : "";
                return $"{prefix}{loai}{TIEUDE} ({NGAYDANG:dd/MM/yyyy})";
            }
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
