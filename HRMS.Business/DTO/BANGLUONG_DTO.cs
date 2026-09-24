using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.DTO
{
    public class BANGLUONG_DTO
    {
        public decimal IDBL { get; set; }
        public decimal MANV { get; set; }
        public string HOTEN { get; set; }
        public string TENPB { get; set; }
        public Nullable<decimal> IDPB { get; set; }
        public Nullable<decimal> DATHOIVIEC { get; set; }
        public Nullable<decimal> KHOA { get; set; }
        public string TRANGTHAI_CHITRA { get; set; }
        public Nullable<decimal> SOGIO_TANGCA { get; set; }
        public decimal MAKYCONG { get; set; }
        public byte THANG { get; set; }
        public short NAM { get; set; }
        public Nullable<decimal> CONG_CHUAN { get; set; }
        public Nullable<decimal> CONG_THUCTE { get; set; }
        public Nullable<decimal> CONG_LAMDEM { get; set; }
        public Nullable<decimal> DAILY_RATE { get; set; }
        public Nullable<decimal> DAILY_ALLOWANCE { get; set; }
        public Nullable<decimal> LUONG_CONG_THUCTE { get; set; }
        public Nullable<decimal> PHUCAP_CONG_THUCTE { get; set; }
        public Nullable<decimal> TIEN_TANGCA { get; set; }
        public Nullable<decimal> TIEN_CHUYENCAN { get; set; }
        public Nullable<decimal> TIEN_AN_CA { get; set; }
        public Nullable<decimal> KHOAN_CONG_KHAC { get; set; }
        public Nullable<decimal> TIEN_BHXH_TRICH { get; set; }
        public Nullable<decimal> TIEN_TAMUNG { get; set; }
        public Nullable<decimal> KHOAN_TRU_KHAC { get; set; }
        public Nullable<decimal> THUC_LINH { get; set; }
        public Nullable<decimal> CONG_LAMNGAY { get; set; }
        public Nullable<decimal> TONG_CONG { get; set; }
        public Nullable<decimal> LUONG_BHXH { get; set; }
        public Nullable<decimal> TIEN_BHXH { get; set; }
        public Nullable<decimal> TIEN_BHYT { get; set; }
        public Nullable<decimal> TIEN_BHTN { get; set; }
        public Nullable<decimal> TIEN_CONG_DOAN { get; set; }
        public Nullable<decimal> THUE_TNCN { get; set; }
        public Nullable<decimal> HOAN_THUE { get; set; }
        public Nullable<decimal> LUONG_CA_NGAY { get; set; }
        public Nullable<decimal> LUONG_CA_DEM { get; set; }

        // Modern 2026 Snapshot & Trace Properties
        public Nullable<int> IS_LEGACY { get; set; }
        public string TRANG_THAI { get; set; }
        public Nullable<decimal> VUNG_LUONG { get; set; }
        public Nullable<decimal> LUONG_TOI_THIEU_VUNG { get; set; }
        public Nullable<decimal> MUC_THAM_CHIEU_BH { get; set; }
        public Nullable<decimal> LUONG_DONG_BHXH { get; set; }
        public Nullable<decimal> TIEN_BHXH_NSDLD { get; set; }
        public Nullable<decimal> TIEN_BHYT_NSDLD { get; set; }
        public Nullable<decimal> TIEN_BHTN_NSDLD { get; set; }
        public Nullable<decimal> TIEN_TNLD_BNN_NSDLD { get; set; }
        public Nullable<decimal> TIEN_DOAN_PHI_NLD { get; set; }
        public Nullable<decimal> TIEN_KINH_PHI_CD_NSDLD { get; set; }
        public Nullable<int> SO_NGUOI_PHU_THUOC { get; set; }
        public Nullable<decimal> GIAM_TRU_BAN_THAN { get; set; }
        public Nullable<decimal> GIAM_TRU_PHU_THUOC { get; set; }
        public Nullable<decimal> GIAM_TRU_BAO_HIEM { get; set; }
        public Nullable<decimal> TONG_THU_NHAP_CHIU_THUE { get; set; }
        public Nullable<decimal> THU_NHAP_TINH_THUE { get; set; }
        public Nullable<decimal> TONG_CHI_PHI_NSDLD { get; set; }
    }

    public class ModernPayrollSnapshotDto
    {
        public decimal IDBL { get; set; }
        public Nullable<int> IS_LEGACY { get; set; }
        public string TRANG_THAI { get; set; }
        public Nullable<decimal> VUNG_LUONG { get; set; }
        public Nullable<decimal> LUONG_TOI_THIEU_VUNG { get; set; }
        public Nullable<decimal> MUC_THAM_CHIEU_BH { get; set; }
        public Nullable<decimal> LUONG_DONG_BHXH { get; set; }
        public Nullable<decimal> TIEN_BHXH_NSDLD { get; set; }
        public Nullable<decimal> TIEN_BHYT_NSDLD { get; set; }
        public Nullable<decimal> TIEN_BHTN_NSDLD { get; set; }
        public Nullable<decimal> TIEN_TNLD_BNN_NSDLD { get; set; }
        public Nullable<decimal> TIEN_DOAN_PHI_NLD { get; set; }
        public Nullable<decimal> TIEN_KINH_PHI_CD_NSDLD { get; set; }
        public Nullable<int> SO_NGUOI_PHU_THUOC { get; set; }
        public Nullable<decimal> GIAM_TRU_BAN_THAN { get; set; }
        public Nullable<decimal> GIAM_TRU_PHU_THUOC { get; set; }
        public Nullable<decimal> GIAM_TRU_BAO_HIEM { get; set; }
        public Nullable<decimal> TONG_THU_NHAP_CHIU_THUE { get; set; }
        public Nullable<decimal> THU_NHAP_TINH_THUE { get; set; }
        public Nullable<decimal> TONG_CHI_PHI_NSDLD { get; set; }
    }
}
