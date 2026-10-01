using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bu.DTO;

namespace Bu.CLASS_CHAMCONG
{
    public class BANGLUONG
    {
        MyEntities db = new MyEntities();

        public List<BANGLUONG_DTO> getList(int makycong)
        {
            var query = from bl in db.TB_BANGLUONG
                        where bl.MAKYCONG == makycong
                        join nv in db.TB_NHANVIEN on bl.MANV equals nv.MANV into nvGroup
                        from nv in nvGroup.DefaultIfEmpty()
                        select new BANGLUONG_DTO
                        {
                            IDBL = bl.IDBL,
                            MANV = bl.MANV,
                            HOTEN = nv != null ? nv.HOTEN : "",
                            MAKYCONG = bl.MAKYCONG,
                            THANG = bl.THANG,
                            NAM = bl.NAM,
                            CONG_CHUAN = bl.CONG_CHUAN,
                            CONG_THUCTE = bl.CONG_THUCTE,
                            CONG_LAMNGAY = bl.CONG_LAMNGAY,
                            CONG_LAMDEM = bl.CONG_LAMDEM,
                            DAILY_RATE = bl.DAILY_RATE,
                            DAILY_ALLOWANCE = bl.DAILY_ALLOWANCE,
                            LUONG_CONG_THUCTE = bl.LUONG_CONG_THUCTE,
                            PHUCAP_CONG_THUCTE = bl.PHUCAP_CONG_THUCTE,
                            TIEN_TANGCA = bl.TIEN_TANGCA,
                            TIEN_CHUYENCAN = bl.TIEN_CHUYENCAN,
                            TIEN_AN_CA = bl.TIEN_AN_CA,
                            KHOAN_CONG_KHAC = bl.KHOAN_CONG_KHAC,
                            TONG_CONG = bl.TONG_CONG,
                            LUONG_BHXH = bl.LUONG_BHXH,
                            TIEN_BHXH = bl.TIEN_BHXH,
                            TIEN_BHYT = bl.TIEN_BHYT,
                            TIEN_BHTN = bl.TIEN_BHTN,
                            TIEN_BHXH_TRICH = bl.TIEN_BHXH_TRICH,
                            TIEN_CONG_DOAN = bl.TIEN_CONG_DOAN,
                            TIEN_TAMUNG = bl.TIEN_TAMUNG,
                            THUE_TNCN = bl.THUE_TNCN,
                            KHOAN_TRU_KHAC = bl.KHOAN_TRU_KHAC,
                            HOAN_THUE = bl.HOAN_THUE,
                            THUC_LINH = bl.THUC_LINH,
                            LUONG_CA_NGAY = (bl.DAILY_RATE != null && bl.CONG_LAMNGAY != null) ? (bl.DAILY_RATE * bl.CONG_LAMNGAY) : 0,
                            LUONG_CA_DEM = (bl.DAILY_RATE != null && bl.CONG_LAMDEM != null) ? (bl.DAILY_RATE * bl.CONG_LAMDEM * 1.30m) : 0
                        };
            var list = query.ToList();

            if (makycong >= 202601 && list.Count > 0)
            {
                try
                {
                    var sql = @"SELECT IDBL, IS_LEGACY, TRANG_THAI, VUNG_LUONG, LUONG_TOI_THIEU_VUNG, MUC_THAM_CHIEU_BH,
                                       LUONG_DONG_BHXH, TIEN_BHXH_NSDLD, TIEN_BHYT_NSDLD, TIEN_BHTN_NSDLD, TIEN_TNLD_BNN_NSDLD,
                                       TIEN_DOAN_PHI_NLD, TIEN_KINH_PHI_CD_NSDLD, SO_NGUOI_PHU_THUOC, GIAM_TRU_BAN_THAN,
                                       GIAM_TRU_PHU_THUOC, GIAM_TRU_BAO_HIEM, TONG_THU_NHAP_CHIU_THUE, THU_NHAP_TINH_THUE,
                                       TONG_CHI_PHI_NSDLD
                                FROM TB_BANGLUONG WHERE MAKYCONG = :p0";
                    var snaps = db.Database.SqlQuery<ModernPayrollSnapshotDto>(
                        sql,
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", makycong)
                    ).ToDictionary(x => x.IDBL);

                    foreach (var item in list)
                    {
                        if (snaps.TryGetValue(item.IDBL, out var snap))
                        {
                            item.IS_LEGACY = snap.IS_LEGACY;
                            item.TRANG_THAI = snap.TRANG_THAI;
                            item.VUNG_LUONG = snap.VUNG_LUONG;
                            item.LUONG_TOI_THIEU_VUNG = snap.LUONG_TOI_THIEU_VUNG;
                            item.MUC_THAM_CHIEU_BH = snap.MUC_THAM_CHIEU_BH;
                            item.LUONG_DONG_BHXH = snap.LUONG_DONG_BHXH;
                            item.TIEN_BHXH_NSDLD = snap.TIEN_BHXH_NSDLD;
                            item.TIEN_BHYT_NSDLD = snap.TIEN_BHYT_NSDLD;
                            item.TIEN_BHTN_NSDLD = snap.TIEN_BHTN_NSDLD;
                            item.TIEN_TNLD_BNN_NSDLD = snap.TIEN_TNLD_BNN_NSDLD;
                            item.TIEN_DOAN_PHI_NLD = snap.TIEN_DOAN_PHI_NLD;
                            item.TIEN_KINH_PHI_CD_NSDLD = snap.TIEN_KINH_PHI_CD_NSDLD;
                            item.SO_NGUOI_PHU_THUOC = snap.SO_NGUOI_PHU_THUOC;
                            item.GIAM_TRU_BAN_THAN = snap.GIAM_TRU_BAN_THAN;
                            item.GIAM_TRU_PHU_THUOC = snap.GIAM_TRU_PHU_THUOC;
                            item.GIAM_TRU_BAO_HIEM = snap.GIAM_TRU_BAO_HIEM;
                            item.TONG_THU_NHAP_CHIU_THUE = snap.TONG_THU_NHAP_CHIU_THUE;
                            item.THU_NHAP_TINH_THUE = snap.THU_NHAP_TINH_THUE;
                            item.TONG_CHI_PHI_NSDLD = snap.TONG_CHI_PHI_NSDLD;
                        }
                        else
                        {
                            item.IS_LEGACY = 0;
                        }
                    }
                }
                catch
                {
                    // Fallback gracefully if modern columns are not yet present in some schemas
                }
            }

            return list;
        }

        public Bu.CLASS_PAYROLL.PayrollRunDto TinhLuongKyCong(int makycong, int iduser)
        {
            return TinhLuongKyCong(makycong, iduser, null);
        }

        public Bu.CLASS_PAYROLL.PayrollRunDto TinhLuongKyCong(int makycong, int iduser, Action<int, int, string> progress)
        {
            using (var dbCheck = new MyEntities())
            {
                var kc = dbCheck.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == (decimal)makycong);
                if (kc != null && ((kc.KHOA ?? 0) == 1 || (kc.TRANGTHAI ?? 0) == 1))
                {
                    throw new InvalidOperationException($"Kỳ công {makycong} đã bị khóa sổ. Khi đã khóa bảng công thì không cho phép tính lại lương.");
                }
            }

            int nam = makycong / 100;
            int thang = makycong % 100;

            progress?.Invoke(10, 100, "Đang khởi tạo engine tính lương theo chính sách...");
            var engine = new Bu.CLASS_PAYROLL.PayrollEngine();
            progress?.Invoke(40, 100, "Đang tính toán lương, bảo hiểm, công đoàn và thuế TNCN theo chính sách...");
            var result = engine.ExecuteFullPayrollRecalculation(nam, thang, "USER_" + iduser);
            progress?.Invoke(100, 100, $"Hoàn tất tính toán bảng lương kỳ {makycong} ({result.SUCCESS_COUNT}/{result.TOTAL_EMPLOYEES} nhân sự).");
            return result;
        }
    }
}
