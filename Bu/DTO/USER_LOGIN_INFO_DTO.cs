using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.DTO
{
    public class USER_LOGIN_INFO_DTO
    {
        public decimal IDUSER { get; set; }
        public string USERNAME { get; set; }
        public string FULLNAME { get; set; }
        
        /// <summary>
        /// "Bị khóa" nếu DISABLED = 1, ngược lại "Đang hoạt động"
        /// </summary>
        public string TRANGTHAI_HOATDONG { get; set; } 
        
        public string IP_ADDRESS { get; set; }
        public string MAC_ADDRESS { get; set; }
        public string TEN_MAY_TINH { get; set; }
        
        /// <summary>
        /// Thời gian đăng nhập cuối cùng
        /// </summary>
        public DateTime? THOIGIAN_DANGNHAP { get; set; }
        public DateTime? THOIGIAN_DANGXUAT { get; set; }
        
        public string TRANGTHAI_DANGNHAP { get; set; }
        
        /// <summary>
        /// Đang Online / Offline dựa trên THOIGIAN_DANGXUAT
        /// </summary>
        public string TRANGTHAI_ONLINE { get; set; }
    }
}
