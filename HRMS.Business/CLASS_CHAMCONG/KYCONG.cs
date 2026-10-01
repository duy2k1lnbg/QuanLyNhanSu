using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_CHAMCONG
{
    public class KYCONG
    {
        MyEntities db = new MyEntities();

        public TB_KYCONG getItem(int id)
        {
            return db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == id);
        }

        public List<TB_KYCONG> getList()
        {
            return db.TB_KYCONG.OrderBy(x => x.MAKYCONG).ToList();
        }

        public TB_KYCONG Add(TB_KYCONG kc)
        {
            try
            {
                db.TB_KYCONG.Add(kc);
                db.SaveChanges();
                return kc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
        }

        public TB_KYCONG Update(TB_KYCONG kc)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                db.Database.SqlQuery<decimal>("SELECT NVL(KHOA,0) FROM TB_KYCONG WHERE MAKYCONG=:p0 FOR UPDATE",
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p0",kc.MAKYCONG)).Single();
                if ((kc.KHOA ?? 0) == 1)
                {
                    var counts = db.Database.SqlQuery<LockReadiness>(@"
                        SELECT COUNT(*) TOTAL,
                          NVL(SUM(CASE WHEN b.LANTINH_ID_HIENHANH IS NULL OR NVL(b.DU_DIEUKIEN_CHOT,0)<>1
                            OR r.INPUT_REV<>k.CONG_INPUT_REV THEN 1 ELSE 0 END),0) BLOCKED
                        FROM TB_BANGCONG_CHITIET b JOIN TB_KYCONG k ON k.MAKYCONG=b.MAKYCONG
                        LEFT JOIN TB_CHAMCONG_LANTINH r ON r.IDLANTINH=b.LANTINH_ID_HIENHANH
                        WHERE b.MAKYCONG=:p0",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0",kc.MAKYCONG)).Single();
                    if (counts.TOTAL == 0 || counts.BLOCKED > 0)
                        throw new InvalidOperationException("Chưa thể khóa kỳ: thiếu công, chưa qua công bố TimeSegmentationEngine hoặc dữ liệu đầu vào đã thay đổi.");

                    int y = (int)kc.MAKYCONG / 100;
                    int m = (int)kc.MAKYCONG % 100;
                    var fromDate = new DateTime(y, m, 1);
                    var toDate = fromDate.AddMonths(1).AddDays(-1);

                    var blockingAnomalies = db.Database.SqlQuery<decimal>(@"
                        SELECT COUNT(*) FROM TB_CHAMCONG_BATTHUONG
                        WHERE NGAY >= :p_from AND NGAY <= :p_to
                          AND CHAN_CHOT = 1 AND TRANG_THAI = 'CHO_XU_LY'",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_from", fromDate),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_to", toDate)).Single();

                    if (blockingAnomalies > 0)
                        throw new InvalidOperationException($"Chưa thể khóa kỳ: còn {blockingAnomalies} bất thường chấm công chặn chốt đang chờ xử lý.");
                }
                var current=db.TB_KYCONG.First(x=>x.MAKYCONG==kc.MAKYCONG);
                current.NAM=kc.NAM;current.THANG=kc.THANG;current.KHOA=kc.KHOA;
                current.NGAYTINHCONG=kc.NGAYTINHCONG;current.NGAYCONGTRONGTHANG=kc.NGAYCONGTRONGTHANG;
                current.TRANGTHAI=kc.TRANGTHAI;current.UPDATED_BY=kc.UPDATED_BY;current.UPDATED_DATE=kc.UPDATED_DATE;
                db.SaveChanges();tx.Commit();return current;
            }
        }

        public void LockKyCong(int makycong, int iduser)
        {
            var kc = getItem(makycong);
            if (kc == null) throw new InvalidOperationException($"Không tìm thấy kỳ công {makycong}.");
            if ((kc.KHOA ?? 0) == 1) throw new InvalidOperationException($"Kỳ công {makycong} hiện đã ở trạng thái khóa.");

            kc.KHOA = 1;
            kc.TRANGTHAI = 1;
            kc.NGAYTINHCONG = DateTime.Now;
            kc.UPDATED_BY = iduser;
            kc.UPDATED_DATE = DateTime.Now;

            Update(kc);
        }

        public void UnlockKyCong(int makycong, int iduser, string lyDo)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                db.Database.SqlQuery<decimal>("SELECT NVL(KHOA,0) FROM TB_KYCONG WHERE MAKYCONG=:p0 FOR UPDATE",
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p0", makycong)).Single();

                var current = db.TB_KYCONG.First(x => x.MAKYCONG == makycong);
                current.KHOA = 0;
                current.UPDATED_BY = iduser;
                current.UPDATED_DATE = DateTime.Now;
                db.SaveChanges();
                tx.Commit();
            }
        }

        private class LockReadiness
        {
            public decimal TOTAL {get;set;}
            public decimal BLOCKED {get;set;}
        }

        public void Delete(int makycong, int iduser)
        {
            try
            {
                var _kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                _kc.DELETED_BY = iduser;
                _kc.DELETED_DATE = DateTime.Now;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi del data " + ex.Message);
            }
        }

        public int KiemTraPhatSinhKyCong(int makycong)
        {
            var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
            if (kc == null)
            {
                return 0;
            }
            if (kc.TRANGTHAI == 1)
            {
                return 1;
            }
            // Fallback phòng vệ: nếu cờ TRANGTHAI chưa kịp bật nhưng đã có dữ liệu kỳ công chi tiết
            if (db.TB_KYCONGCHITIET.Any(x => x.MAKYCONG == makycong))
            {
                return 1;
            }
            return 0;
        }

        public int KiemTraMaKyCong(int makycong)
        {
            var id = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
            if (id == null)
            {
                return 0; 
            }
            else
            {
                return 1;
            }
        }
    }
}
