using DA;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_CHAMCONG
{
    public class BANGCONG_NV_CHITIET
    {
        MyEntities db = new MyEntities();

        public TB_BANGCONG_CHITIET getItem(int makycong, int manv, int ngay)
        {
            return db.TB_BANGCONG_CHITIET.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv && x.NGAY.Value.Day == ngay);
        }

        public List<TB_BANGCONG_CHITIET> getBangCongCT(int makycong, int manv)
        {
            return db.TB_BANGCONG_CHITIET.Where(x => x.MAKYCONG == makycong && x.MANV == manv).OrderBy(x => x.NGAY).ToList();
        }

        public TB_BANGCONG_CHITIET Add(TB_BANGCONG_CHITIET bcct)
        {
            try
            {
                db.TB_BANGCONG_CHITIET.Add(bcct);
                db.SaveChanges();
                return bcct;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
            
        }

        public void AddRange(List<TB_BANGCONG_CHITIET> lstBcct)
        {
            try
            {
                db.TB_BANGCONG_CHITIET.AddRange(lstBcct);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi AddRange data " + ex.Message);
            }
        }

        public void PhatSinhBangCongChiTiet(int makycong, int nam, int thang, int iduser, Action<int, int, string> progress = null)
        {
            try
            {
                progress?.Invoke(50, 100, "Đang phát sinh chi tiết từng ngày công...");

                string startDateStr = $"{nam:D4}-{thang:D2}-01";
                int daysInMonth = DateTime.DaysInMonth(nam, thang);
                string endDateStr = $"{nam:D4}-{thang:D2}-{daysInMonth:D2}";

                // Đảm bảo không bị trùng lặp dữ liệu: Xóa các bản ghi cũ của kỳ công này nếu đã có trước khi sinh mới
                var pDelMkc = new Oracle.ManagedDataAccess.Client.OracleParameter("p_del_makycong", makycong);
                db.Database.ExecuteSqlCommand("DELETE FROM TB_BANGCONG_CHITIET WHERE MAKYCONG = :p_del_makycong", pDelMkc);

                string sql = @"
INSERT INTO TB_BANGCONG_CHITIET (
    MAKYCONG, IDCTY, MANV, HOTEN, NGAY, THU, GIOVAO, GIORA, 
    NGAYPHEP, CONGNGAYLE, CONGCHUNHAT, KYHIEU, NGAYCONG, CREATED_BY, CREATED_DATE
)
SELECT 
    :p_makycong,
    nv.IDCTY,
    nv.MANV,
    nv.HOTEN,
    d.ngay,
    (CASE TO_CHAR(d.ngay, 'DY', 'NLS_DATE_LANGUAGE=AMERICAN')
        WHEN 'MON' THEN N'Thứ hai'
        WHEN 'TUE' THEN N'Thứ ba'
        WHEN 'WED' THEN N'Thứ tư'
        WHEN 'THU' THEN N'Thứ năm'
        WHEN 'FRI' THEN N'Thứ sáu'
        WHEN 'SAT' THEN N'Thứ bảy'
        ELSE N'Chủ nhật'
    END),
    '08:00',
    '17:00',
    0,
    (CASE WHEN EXISTS (SELECT 1 FROM TB_NGAYLE nl WHERE TRUNC(nl.NGAY) = TRUNC(d.ngay) AND nl.DELETED_BY IS NULL) THEN 1 ELSE 0 END),
    (CASE WHEN NOT EXISTS (SELECT 1 FROM TB_NGAYLE nl WHERE TRUNC(nl.NGAY) = TRUNC(d.ngay) AND nl.DELETED_BY IS NULL) AND TO_CHAR(d.ngay, 'DY', 'NLS_DATE_LANGUAGE=AMERICAN') = 'SUN' THEN 1 ELSE 0 END),
    (CASE 
        WHEN EXISTS (SELECT 1 FROM TB_NGAYLE nl WHERE TRUNC(nl.NGAY) = TRUNC(d.ngay) AND nl.DELETED_BY IS NULL) THEN 'L'
        WHEN TO_CHAR(d.ngay, 'DY', 'NLS_DATE_LANGUAGE=AMERICAN') = 'SUN' THEN 'CN'
        ELSE 'X'
    END),
    (CASE 
        WHEN EXISTS (SELECT 1 FROM TB_NGAYLE nl WHERE TRUNC(nl.NGAY) = TRUNC(d.ngay) AND nl.DELETED_BY IS NULL) THEN 1
        WHEN TO_CHAR(d.ngay, 'DY', 'NLS_DATE_LANGUAGE=AMERICAN') = 'SUN' THEN 0
        ELSE 1
    END),
    :p_iduser,
    SYSDATE
FROM (
    SELECT TO_DATE(:p_start_date, 'YYYY-MM-DD') + LEVEL - 1 as ngay
    FROM DUAL
    CONNECT BY LEVEL <= :p_days
) d
CROSS JOIN (
    SELECT nv.MANV, nv.IDCTY, nv.HOTEN
    FROM TB_NHANVIEN nv
    WHERE (nv.DATHOIVIEC IS NULL OR nv.DATHOIVIEC = 0)
      AND nv.MANV != 3207
      AND EXISTS (
          SELECT 1 FROM (
              SELECT hd.MANV, hd.NGAYBATDAU, hd.NGAYKETTHUC,
                     ROW_NUMBER() OVER (PARTITION BY hd.MANV ORDER BY hd.NGAYBATDAU DESC, hd.SOHD DESC) as rn
              FROM TB_HOPDONG hd
          ) hd_latest
          WHERE hd_latest.MANV = nv.MANV 
            AND hd_latest.rn = 1
            AND hd_latest.NGAYBATDAU <= TO_DATE(:p_end_date, 'YYYY-MM-DD')
            AND (hd_latest.NGAYKETTHUC IS NULL OR hd_latest.NGAYKETTHUC >= TO_DATE(:p_start_date, 'YYYY-MM-DD'))
      )
) nv";

                var pMaKyCong = new Oracle.ManagedDataAccess.Client.OracleParameter("p_makycong", makycong);
                var pIdUser = new Oracle.ManagedDataAccess.Client.OracleParameter("p_iduser", iduser);
                var pStartDate1 = new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date", startDateStr);
                var pDays = new Oracle.ManagedDataAccess.Client.OracleParameter("p_days", daysInMonth);
                var pEndDate = new Oracle.ManagedDataAccess.Client.OracleParameter("p_end_date", endDateStr);
                var pStartDate2 = new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date", startDateStr);

                db.Database.ExecuteSqlCommand(sql, pMaKyCong, pIdUser, pStartDate1, pDays, pEndDate, pStartDate2);

                progress?.Invoke(90, 100, "Đã hoàn tất phát sinh bảng công chi tiết.");
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi phát sinh bảng công chi tiết: " + ex.Message);
            }
        }

        public TB_BANGCONG_CHITIET Update(TB_BANGCONG_CHITIET bcct)
        {
            try
            {
                TB_BANGCONG_CHITIET bcnv = db.TB_BANGCONG_CHITIET.FirstOrDefault(x => x.MAKYCONG == bcct.MAKYCONG && x.MANV == bcct.MANV && x.NGAY == bcct.NGAY);
                bcnv.KYHIEU = bcct.KYHIEU;
                bcnv.GIOVAO = bcct.GIOVAO;
                bcnv.GIORA = bcct.GIORA;
                bcnv.NGAYPHEP = bcct.NGAYPHEP;
                bcnv.NGAYCONG = bcct.NGAYCONG;
                bcnv.GHICHU = bcct.GHICHU;
                bcnv.CONGCHUNHAT = bcct.CONGCHUNHAT;
                bcnv.CONGNGAYLE = bcct.CONGNGAYLE;
                bcnv.UPDATED_BY = bcct.UPDATED_BY;
                bcnv.UPDATED_DATE = bcct.UPDATED_DATE;
                db.SaveChanges();
                return bcct;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Update data " + ex.Message);
            }
        }

        public decimal tongNgayPhep(int makycong, int manv)
        {
            return db.TB_BANGCONG_CHITIET.Where(x => x.MAKYCONG == makycong && x.MANV == manv && x.NGAYPHEP != null).Sum(p => p.NGAYPHEP.Value);
        }

        public decimal tongNgayCong(int makycong, int manv)
        {
            return db.TB_BANGCONG_CHITIET.Where(x => x.MAKYCONG == makycong && x.MANV == manv && x.NGAYCONG != null).Sum(p => p.NGAYCONG.Value);
        }
    }
}
