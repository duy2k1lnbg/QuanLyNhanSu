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
    public class RawTimekeepingItemDto
    {
        public decimal MANV { get; set; }
        public int NAM { get; set; }
        public int THANG { get; set; }
        public int NGAY { get; set; }
        public int? GIOVAO { get; set; }
        public int? PHUTVAO { get; set; }
        public int? GIORA { get; set; }
        public int? PHUTRA { get; set; }
        public int? IDLOAICONG { get; set; }
    }

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

        /// <summary>
        /// Tự động phát sinh dữ liệu quẹt thẻ chuẩn vào TB_BANGCONG cho các ngày làm việc hành chính
        /// (Giờ vào 08:00, giờ ra 17:00, loại trừ Chủ nhật, ngày lễ và ngày nghỉ phép đã duyệt).
        /// </summary>
        public int PhatSinhBangCongRaw(int nam, int thang, int iduser, MyEntities dbInstance = null)
        {
            bool isLocalContext = (dbInstance == null);
            var activeDb = dbInstance ?? new MyEntities();
            try
            {
                string startDateStr = $"{nam:D4}-{thang:D2}-01";
                int daysInMonth = DateTime.DaysInMonth(nam, thang);
                string endDateStr = $"{nam:D4}-{thang:D2}-{daysInMonth:D2}";

                // 1. Xóa các bản ghi cũ của tháng này trong TB_BANGCONG nếu có
                activeDb.Database.ExecuteSqlCommand(
                    "DELETE FROM TB_BANGCONG WHERE NAM = :p_nam AND THANG = :p_thang",
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_nam", nam),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_thang", thang)
                );

                // 2. Chèn các lượt chấm công chuẩn vào TB_BANGCONG
                string sql = @"
INSERT INTO TB_BANGCONG (
    NAM, THANG, NGAY, GIOVAO, PHUTVAO, GIORA, PHUTRA, MANV, IDLOAICONG
)
SELECT 
    :p_nam,
    :p_thang,
    EXTRACT(DAY FROM d.ngay) as NGAY,
    (CASE 
        WHEN ct.ID IS NOT NULL AND ct.GIO_VAO IS NOT NULL AND INSTR(TO_CHAR(ct.GIO_VAO), ':') > 0 
        THEN TO_NUMBER(SUBSTR(TO_CHAR(ct.GIO_VAO), 1, INSTR(TO_CHAR(ct.GIO_VAO), ':') - 1))
        ELSE 8 
    END) as GIOVAO,
    (CASE 
        WHEN ct.ID IS NOT NULL AND ct.GIO_VAO IS NOT NULL AND INSTR(TO_CHAR(ct.GIO_VAO), ':') > 0 
        THEN TO_NUMBER(SUBSTR(TO_CHAR(ct.GIO_VAO), INSTR(TO_CHAR(ct.GIO_VAO), ':') + 1, 2))
        ELSE 0 
    END) as PHUTVAO,
    (CASE 
        WHEN ct.ID IS NOT NULL AND ct.GIO_RA IS NOT NULL AND INSTR(TO_CHAR(ct.GIO_RA), ':') > 0 
        THEN TO_NUMBER(SUBSTR(TO_CHAR(ct.GIO_RA), 1, INSTR(TO_CHAR(ct.GIO_RA), ':') - 1))
        ELSE 17 
    END) as GIORA,
    (CASE 
        WHEN ct.ID IS NOT NULL AND ct.GIO_RA IS NOT NULL AND INSTR(TO_CHAR(ct.GIO_RA), ':') > 0 
        THEN TO_NUMBER(SUBSTR(TO_CHAR(ct.GIO_RA), INSTR(TO_CHAR(ct.GIO_RA), ':') + 1, 2))
        ELSE 0 
    END) as PHUTRA,
    nv.MANV,
    1 as IDLOAICONG
FROM (
    SELECT TO_DATE(:p_start_date, 'YYYY-MM-DD') + LEVEL - 1 as ngay
    FROM DUAL
    CONNECT BY LEVEL <= :p_days
) d
CROSS JOIN (
    SELECT nv.MANV
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
            AND (hd_latest.NGAYKETTHUC IS NULL OR hd_latest.NGAYKETTHUC >= TO_DATE(:p_start_date2, 'YYYY-MM-DD'))
      )
) nv
LEFT JOIN TB_NGAYLE nl ON TRUNC(nl.NGAY) = TRUNC(d.ngay) AND nl.DELETED_BY IS NULL
LEFT JOIN TB_YEUCAU_NGHIPHEP phep ON phep.MANV = nv.MANV 
                                 AND TRUNC(d.ngay) BETWEEN TRUNC(phep.TUNGAY) AND TRUNC(phep.DENNGAY)
                                 AND phep.TRANGTHAI = 'APPROVED'
LEFT JOIN TB_YEUCAU_DIEUCHINHCONG ct ON ct.MANV = nv.MANV 
                                    AND TRUNC(d.ngay) = TRUNC(ct.NGAY)
                                    AND ct.TRANGTHAI = 'APPROVED'
WHERE TO_CHAR(d.ngay, 'DY', 'NLS_DATE_LANGUAGE=AMERICAN') != 'SUN'
  AND nl.NGAY IS NULL
  AND phep.ID IS NULL";

                int inserted = activeDb.Database.ExecuteSqlCommand(sql,
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_nam", nam),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_thang", thang),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date", startDateStr),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_days", daysInMonth),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_end_date", endDateStr),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date2", startDateStr)
                );

                return inserted;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi tự động phát sinh TB_BANGCONG: " + ex.Message, ex);
            }
            finally
            {
                if (isLocalContext)
                {
                    activeDb.Dispose();
                    this.db = new MyEntities();
                }
            }
        }

        public void PhatSinhBangCongChiTiet(int makycong, int nam, int thang, int iduser, Action<int, int, string> progress = null, MyEntities dbInstance = null, bool tuPhatSinhBangCong = true)
        {
            bool isLocalContext = (dbInstance == null);
            var activeDb = dbInstance ?? new MyEntities();
            try
            {
                progress?.Invoke(20, 100, "Đang kiểm tra nguồn dữ liệu chấm công...");

                string startDateStr = $"{nam:D4}-{thang:D2}-01";
                int daysInMonth = DateTime.DaysInMonth(nam, thang);
                string endDateStr = $"{nam:D4}-{thang:D2}-{daysInMonth:D2}";

                // 1. Kiểm tra xem TB_BANGCONG có dữ liệu quẹt thẻ thực tế trong tháng này hay không
                int rawCount = 0;
                try
                {
                    rawCount = activeDb.TB_BANGCONG.Count(b => b.NAM == nam && b.THANG == thang);
                }
                catch { }

                if (rawCount == 0 && tuPhatSinhBangCong)
                {
                    progress?.Invoke(30, 100, "Chưa có máy chấm công. Đang tự động phát sinh bảng chấm công gốc (TB_BANGCONG)...");
                    rawCount = PhatSinhBangCongRaw(nam, thang, iduser, activeDb);
                    progress?.Invoke(40, 100, $"Đã phát sinh {rawCount} lượt chấm công chuẩn vào TB_BANGCONG.");
                }

                bool hasRawPunches = rawCount > 0;
                if (hasRawPunches)
                {
                    progress?.Invoke(45, 100, $"Phát hiện {rawCount} lượt quẹt thẻ trong TB_BANGCONG. Đang đồng bộ dữ liệu thực tế...");
                }
                else
                {
                    progress?.Invoke(45, 100, "Đang phát sinh lịch công chuẩn hành chính và đơn từ...");
                }

                // 2. Xóa các bản ghi cũ của kỳ công này nếu đã có trước khi sinh mới
                var pDelMkc = new Oracle.ManagedDataAccess.Client.OracleParameter("p_del_makycong", makycong);
                activeDb.Database.ExecuteSqlCommand("DELETE FROM TB_BANGCONG_CHITIET WHERE MAKYCONG = :p_del_makycong", pDelMkc);

                string sql;
                if (hasRawPunches)
                {
                    // CHẾ ĐỘ 1: Đồng bộ trực tiếp từ TB_BANGCONG (quẹt thẻ thực tế) kết hợp Đơn phép, Đơn công tác, Ngày lễ
                    sql = @"
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
        WHEN 'MON' THEN 'Thứ hai'
        WHEN 'TUE' THEN 'Thứ ba'
        WHEN 'WED' THEN 'Thứ tư'
        WHEN 'THU' THEN 'Thứ năm'
        WHEN 'FRI' THEN 'Thứ sáu'
        WHEN 'SAT' THEN 'Thứ bảy'
        ELSE 'Chủ nhật'
    END),
    raw_bc.GIO_VAO,
    raw_bc.GIO_RA,
    (CASE WHEN phep.ID IS NOT NULL THEN 1 ELSE 0 END) AS NGAYPHEP,
    (CASE WHEN nl.NGAY IS NOT NULL THEN 1 ELSE 0 END) AS CONGNGAYLE,
    (CASE WHEN nl.NGAY IS NULL AND TO_CHAR(d.ngay, 'DY', 'NLS_DATE_LANGUAGE=AMERICAN') = 'SUN' THEN 1 ELSE 0 END) AS CONGCHUNHAT,
    (CASE 
        WHEN nl.NGAY IS NOT NULL THEN 'L'
        WHEN phep.ID IS NOT NULL THEN 'P'
        WHEN ct.ID IS NOT NULL THEN 'CT'
        WHEN TO_CHAR(d.ngay, 'DY', 'NLS_DATE_LANGUAGE=AMERICAN') = 'SUN' THEN 'CN'
        WHEN raw_bc.MANV IS NOT NULL AND raw_bc.IS_DEM = 1 THEN 'CD'
        WHEN raw_bc.MANV IS NOT NULL THEN 'X'
        ELSE 'V'
    END) AS KYHIEU,
    (CASE 
        WHEN nl.NGAY IS NOT NULL THEN 1
        WHEN phep.ID IS NOT NULL THEN 1
        WHEN ct.ID IS NOT NULL THEN 1
        WHEN TO_CHAR(d.ngay, 'DY', 'NLS_DATE_LANGUAGE=AMERICAN') = 'SUN' THEN 0
        WHEN raw_bc.MANV IS NOT NULL THEN 1
        ELSE 0
    END) AS NGAYCONG,
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
) nv
LEFT JOIN (
    SELECT 
        bc.MANV,
        bc.NGAY,
        LPAD(TO_CHAR(MIN(bc.GIOVAO)), 2, '0') || ':' || LPAD(TO_CHAR(NVL(MIN(bc.PHUTVAO), 0)), 2, '0') AS GIO_VAO,
        LPAD(TO_CHAR(MAX(bc.GIORA)), 2, '0') || ':' || LPAD(TO_CHAR(NVL(MAX(bc.PHUTRA), 0)), 2, '0') AS GIO_RA,
        CASE WHEN MIN(bc.GIOVAO) >= 18 OR MIN(bc.GIOVAO) < 6 THEN 1 ELSE 0 END AS IS_DEM
    FROM TB_BANGCONG bc
    WHERE bc.NAM = :p_nam AND bc.THANG = :p_thang
    GROUP BY bc.MANV, bc.NGAY
) raw_bc ON raw_bc.MANV = nv.MANV AND raw_bc.NGAY = EXTRACT(DAY FROM d.ngay)
LEFT JOIN TB_NGAYLE nl ON TRUNC(nl.NGAY) = TRUNC(d.ngay) AND nl.DELETED_BY IS NULL
LEFT JOIN TB_YEUCAU_NGHIPHEP phep ON phep.MANV = nv.MANV 
                                 AND TRUNC(d.ngay) BETWEEN TRUNC(phep.TUNGAY) AND TRUNC(phep.DENNGAY)
                                 AND phep.TRANGTHAI = 'APPROVED'
LEFT JOIN TB_YEUCAU_DIEUCHINHCONG ct ON ct.MANV = nv.MANV 
                                    AND TRUNC(d.ngay) = TRUNC(ct.NGAY)
                                    AND ct.TRANGTHAI = 'APPROVED'";
                }
                else
                {
                    // CHẾ ĐỘ 2: Chưa có máy chấm công -> Phát sinh lịch chuẩn hành chính 8h-17h, tự động tích hợp Đơn phép, Đơn công tác, Ngày lễ
                    sql = @"
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
        WHEN 'MON' THEN 'Thứ hai'
        WHEN 'TUE' THEN 'Thứ ba'
        WHEN 'WED' THEN 'Thứ tư'
        WHEN 'THU' THEN 'Thứ năm'
        WHEN 'FRI' THEN 'Thứ sáu'
        WHEN 'SAT' THEN 'Thứ bảy'
        ELSE 'Chủ nhật'
    END),
    (CASE WHEN ct.ID IS NOT NULL AND ct.GIO_VAO IS NOT NULL THEN TO_CHAR(ct.GIO_VAO) ELSE '08:00' END) AS GIOVAO,
    (CASE WHEN ct.ID IS NOT NULL AND ct.GIO_RA IS NOT NULL THEN TO_CHAR(ct.GIO_RA) ELSE '17:00' END) AS GIORA,
    (CASE WHEN phep.ID IS NOT NULL THEN 1 ELSE 0 END) AS NGAYPHEP,
    (CASE WHEN nl.NGAY IS NOT NULL THEN 1 ELSE 0 END) AS CONGNGAYLE,
    (CASE WHEN nl.NGAY IS NULL AND TO_CHAR(d.ngay, 'DY', 'NLS_DATE_LANGUAGE=AMERICAN') = 'SUN' THEN 1 ELSE 0 END) AS CONGCHUNHAT,
    (CASE 
        WHEN nl.NGAY IS NOT NULL THEN 'L'
        WHEN phep.ID IS NOT NULL THEN 'P'
        WHEN ct.ID IS NOT NULL THEN 'CT'
        WHEN TO_CHAR(d.ngay, 'DY', 'NLS_DATE_LANGUAGE=AMERICAN') = 'SUN' THEN 'CN'
        ELSE 'X'
    END) AS KYHIEU,
    (CASE 
        WHEN nl.NGAY IS NOT NULL THEN 1
        WHEN phep.ID IS NOT NULL THEN 1
        WHEN ct.ID IS NOT NULL THEN 1
        WHEN TO_CHAR(d.ngay, 'DY', 'NLS_DATE_LANGUAGE=AMERICAN') = 'SUN' THEN 0
        ELSE 1
    END) AS NGAYCONG,
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
) nv
LEFT JOIN TB_NGAYLE nl ON TRUNC(nl.NGAY) = TRUNC(d.ngay) AND nl.DELETED_BY IS NULL
LEFT JOIN TB_YEUCAU_NGHIPHEP phep ON phep.MANV = nv.MANV 
                                 AND TRUNC(d.ngay) BETWEEN TRUNC(phep.TUNGAY) AND TRUNC(phep.DENNGAY)
                                 AND phep.TRANGTHAI = 'APPROVED'
LEFT JOIN TB_YEUCAU_DIEUCHINHCONG ct ON ct.MANV = nv.MANV 
                                    AND TRUNC(d.ngay) = TRUNC(ct.NGAY)
                                    AND ct.TRANGTHAI = 'APPROVED'";
                }

                if (hasRawPunches)
                {
                    activeDb.Database.ExecuteSqlCommand(sql,
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_makycong", makycong),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_iduser", iduser),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date", startDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_days", daysInMonth),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_end_date", endDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date2", startDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_nam", nam),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_thang", thang)
                    );
                }
                else
                {
                    activeDb.Database.ExecuteSqlCommand(sql,
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_makycong", makycong),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_iduser", iduser),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date", startDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_days", daysInMonth),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_end_date", endDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date2", startDateStr)
                    );
                }

                progress?.Invoke(80, 100, "Đang đồng bộ ngược lại ma trận TB_KYCONGCHITIET...");
                DongBoSangKyCongChiTiet(makycong, nam, thang, activeDb);

                progress?.Invoke(100, 100, "Đã hoàn tất phát sinh và đồng bộ bảng công chi tiết.");
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi phát sinh bảng công chi tiết: " + ex.Message, ex);
            }
            finally
            {
                if (isLocalContext)
                {
                    activeDb.Dispose();
                    this.db = new MyEntities();
                }
            }
        }

        /// <summary>
        /// Đồng bộ ngược từ TB_BANGCONG_CHITIET sang ma trận TB_KYCONGCHITIET (D1..D31, TONGNGAYCONG, NGAYPHEP...)
        /// </summary>
        public void DongBoSangKyCongChiTiet(int makycong, int nam, int thang, MyEntities dbInstance = null)
        {
            bool isLocalContext = (dbInstance == null);
            var activeDb = dbInstance ?? new MyEntities();
            try
            {
                // 1. Đồng bộ các chỉ số tổng hợp (TONGNGAYCONG, NGAYPHEP, CONGNGAYLE, CONGCHUNHAT, NGHIKHONGPHEP) trực tiếp bằng MERGE (quét 1 lần duy nhất)
                string updateTotalsSql = @"
MERGE INTO TB_KYCONGCHITIET kc
USING (
    SELECT 
        bc.MAKYCONG,
        bc.MANV,
        SUM(bc.NGAYCONG) as tong_cong,
        SUM(bc.NGAYPHEP) as tong_phep,
        SUM(bc.CONGNGAYLE) as tong_le,
        SUM(bc.CONGCHUNHAT) as tong_cn,
        COUNT(CASE WHEN bc.KYHIEU = 'V' THEN 1 END) as tong_v
    FROM TB_BANGCONG_CHITIET bc
    WHERE bc.MAKYCONG = :p_makycong
    GROUP BY bc.MAKYCONG, bc.MANV
) src
ON (kc.MAKYCONG = src.MAKYCONG AND kc.MANV = src.MANV)
WHEN MATCHED THEN
UPDATE SET
    kc.TONGNGAYCONG = NVL(src.tong_cong, 0),
    kc.NGAYPHEP = NVL(src.tong_phep, 0),
    kc.CONGNGAYLE = NVL(src.tong_le, 0),
    kc.CONGCHUNHAT = NVL(src.tong_cn, 0),
    kc.NGHIKHONGPHEP = NVL(src.tong_v, 0)";

                activeDb.Database.ExecuteSqlCommand(updateTotalsSql,
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_makycong", makycong)
                );

                // 2. Cập nhật ký hiệu các ngày đặc biệt (P, CT, CD, V) vào các cột D1..D31
                string updateSpecialDaysSql = @"
DECLARE
    v_mkc NUMBER := :p_makycong;
BEGIN
    FOR r IN (
        SELECT bc.MANV, EXTRACT(DAY FROM bc.NGAY) as day_num, bc.KYHIEU 
        FROM TB_BANGCONG_CHITIET bc 
        WHERE bc.MAKYCONG = v_mkc 
          AND bc.KYHIEU IN ('P', 'CT', 'CD', 'V')
    ) LOOP
        EXECUTE IMMEDIATE 'UPDATE TB_KYCONGCHITIET SET D' || r.day_num || ' = :1 WHERE MAKYCONG = :2 AND MANV = :3'
        USING r.KYHIEU, v_mkc, r.MANV;
    END LOOP;
END;";

                activeDb.Database.ExecuteSqlCommand(updateSpecialDaysSql,
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_makycong", makycong)
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi đồng bộ sang TB_KYCONGCHITIET: " + ex.Message);
                throw;
            }
            finally
            {
                if (isLocalContext)
                {
                    activeDb.Dispose();
                    this.db = new MyEntities();
                }
            }
        }

        /// <summary>
        /// Import dữ liệu quẹt thẻ thô từ file/máy chấm công vào TB_BANGCONG, sau đó tự động kích hoạt đồng bộ
        /// </summary>
        public int ImportBangCongRaw(List<RawTimekeepingItemDto> punches, int makycong, int nam, int thang, int iduser)
        {
            if (punches == null || punches.Count == 0) return 0;
            int count = 0;

            foreach (var p in punches)
            {
                var bc = new TB_BANGCONG
                {
                    MANV = p.MANV,
                    NAM = p.NAM > 0 ? p.NAM : nam,
                    THANG = p.THANG > 0 ? p.THANG : thang,
                    NGAY = p.NGAY,
                    GIOVAO = p.GIOVAO,
                    PHUTVAO = p.PHUTVAO ?? 0,
                    GIORA = p.GIORA,
                    PHUTRA = p.PHUTRA ?? 0,
                    IDLOAICONG = p.IDLOAICONG ?? 1
                };
                db.TB_BANGCONG.Add(bc);
                count++;
            }
            db.SaveChanges();

            // Tự động phát sinh lại Bảng công chi tiết & đồng bộ sang Kỳ công chi tiết
            PhatSinhBangCongChiTiet(makycong, nam, thang, iduser, null);

            return count;
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

        /// <summary>
        /// Lấy bản ghi chấm công quẹt thẻ gốc từ TB_BANGCONG theo nhân viên và ngày cụ thể
        /// </summary>
        public TB_BANGCONG GetBangCongRaw(int manv, int nam, int thang, int ngay)
        {
            return db.TB_BANGCONG.FirstOrDefault(x => x.MANV == manv && x.NAM == nam && x.THANG == thang && x.NGAY == ngay);
        }

        /// <summary>
        /// Cập nhật đồng thời TB_BANGCONG (giờ vào, ra gốc), TB_BANGCONG_CHITIET và TB_KYCONGCHITIET trong 1 ACID transaction
        /// </summary>
        public void CapNhatNgayCongVaBangCongRaw(int manv, int makycong, int nam, int thang, int ngay, int gioVao, int phutVao, int gioRa, int phutRa, string kyhieu, string loaiNghi, int iduser, string ghiChu = null)
        {
            using (var trans = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                try
                {
                    DateTime ngayDate = new DateTime(nam, thang, ngay);
                    var bcct = db.TB_BANGCONG_CHITIET.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv && x.NGAY == ngayDate);

                    bool giuNguyenCong = (kyhieu == "KHONG_DOI" || string.IsNullOrEmpty(kyhieu));
                    if (giuNguyenCong)
                    {
                        if (bcct != null && !string.IsNullOrEmpty(bcct.KYHIEU))
                        {
                            kyhieu = bcct.KYHIEU;
                        }
                        else
                        {
                            kyhieu = "X";
                        }
                    }

                    // 1. Cập nhật hoặc thêm mới vào TB_BANGCONG (Dữ liệu quẹt thẻ gốc)
                    var raw = db.TB_BANGCONG.FirstOrDefault(x => x.MANV == manv && x.NAM == nam && x.THANG == thang && x.NGAY == ngay);
                    decimal idLoaiCong = (kyhieu == "CD") ? 2 : (raw != null && raw.IDLOAICONG.HasValue ? raw.IDLOAICONG.Value : 1);
                    if (raw != null)
                    {
                        raw.GIOVAO = gioVao;
                        raw.PHUTVAO = phutVao;
                        raw.GIORA = gioRa;
                        raw.PHUTRA = phutRa;
                        if (!giuNguyenCong)
                        {
                            raw.IDLOAICONG = idLoaiCong;
                        }
                    }
                    else
                    {
                        raw = new TB_BANGCONG
                        {
                            MANV = manv,
                            NAM = nam,
                            THANG = thang,
                            NGAY = ngay,
                            GIOVAO = gioVao,
                            PHUTVAO = phutVao,
                            GIORA = gioRa,
                            PHUTRA = phutRa,
                            IDLOAICONG = idLoaiCong
                        };
                        db.TB_BANGCONG.Add(raw);
                    }
                    db.SaveChanges();

                    // 2. Cập nhật hoặc thêm mới vào TB_BANGCONG_CHITIET
                    if (bcct == null)
                    {
                        var nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == manv);
                        bcct = new TB_BANGCONG_CHITIET
                        {
                            MAKYCONG = makycong,
                            MANV = manv,
                            HOTEN = nv != null ? nv.HOTEN : "",
                            IDCTY = nv != null ? nv.IDCTY : 1,
                            NGAY = ngayDate,
                            THU = ngayDate.DayOfWeek == DayOfWeek.Sunday ? "Chủ nhật" : ("Thứ " + ((int)ngayDate.DayOfWeek + 1)),
                            CREATED_BY = iduser,
                            CREATED_DATE = DateTime.Now
                        };
                        db.TB_BANGCONG_CHITIET.Add(bcct);
                    }

                    bcct.GIOVAO = $"{gioVao:D2}:{phutVao:D2}";
                    bcct.GIORA = $"{gioRa:D2}:{phutRa:D2}";
                    bcct.UPDATED_BY = iduser;
                    bcct.UPDATED_DATE = DateTime.Now;
                    if (!string.IsNullOrEmpty(ghiChu))
                    {
                        bcct.GHICHU = ghiChu;
                    }

                    if (!giuNguyenCong)
                    {
                        bcct.KYHIEU = kyhieu;

                        // Nếu loaiNghi là KHONG_DOI thì giữ nguyên loại nghỉ hiện tại
                        if (loaiNghi == "KHONG_DOI")
                        {
                            loaiNghi = (bcct.NGAYPHEP == 0.5m || bcct.NGAYCONG == 0.5m) ? "S" : "NN";
                        }

                        // Tính công dựa theo ký hiệu và loại nghỉ
                        switch (kyhieu)
                        {
                            case "X":
                                bcct.NGAYCONG = 1;
                                bcct.NGAYPHEP = 0;
                                break;
                            case "CD":
                                bcct.NGAYCONG = 1;
                                bcct.NGAYPHEP = 0;
                                break;
                            case "P":
                                if (loaiNghi == "NN" || loaiNghi == "KHONG")
                                {
                                    bcct.NGAYPHEP = 1;
                                    bcct.NGAYCONG = 1;
                                }
                                else
                                {
                                    bcct.NGAYPHEP = 0.5m;
                                    bcct.NGAYCONG = 0.5m;
                                }
                                break;
                            case "CT":
                                if (loaiNghi == "NN" || loaiNghi == "KHONG")
                                {
                                    bcct.NGAYCONG = 1;
                                    bcct.NGAYPHEP = 0;
                                }
                                else
                                {
                                    bcct.NGAYCONG = 0.5m;
                                    bcct.NGAYPHEP = 0.5m;
                                }
                                break;
                            case "V":
                                if (loaiNghi == "NN" || loaiNghi == "KHONG")
                                {
                                    bcct.NGAYCONG = 0;
                                    bcct.NGAYPHEP = 0;
                                }
                                else
                                {
                                    bcct.NGAYCONG = 0.5m;
                                    bcct.NGAYPHEP = 0;
                                }
                                break;
                            case "VR":
                                if (loaiNghi == "NN" || loaiNghi == "KHONG")
                                {
                                    bcct.NGAYCONG = 0;
                                    bcct.NGAYPHEP = 1;
                                }
                                else
                                {
                                    bcct.NGAYCONG = 0.5m;
                                    bcct.NGAYPHEP = 0.5m;
                                }
                                break;
                            default:
                                bcct.NGAYCONG = 1;
                                bcct.NGAYPHEP = 0;
                                break;
                        }
                    }
                    db.SaveChanges();

                    // 3. Cập nhật ma trận TB_KYCONGCHITIET (D1..D31, TONGNGAYCONG, NGAYPHEP)
                    var kcct = db.TB_KYCONGCHITIET.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv);
                    if (kcct != null)
                    {
                        if (!giuNguyenCong)
                        {
                            string fieldName = "D" + ngay;
                            var prop = kcct.GetType().GetProperty(fieldName);
                            if (prop != null)
                            {
                                prop.SetValue(kcct, kyhieu);
                            }
                        }

                        decimal tongCong = db.TB_BANGCONG_CHITIET
                            .Where(x => x.MAKYCONG == makycong && x.MANV == manv && x.NGAYCONG != null)
                            .Sum(p => p.NGAYCONG.Value);
                        decimal tongPhep = db.TB_BANGCONG_CHITIET
                            .Where(x => x.MAKYCONG == makycong && x.MANV == manv && x.NGAYPHEP != null)
                            .Sum(p => p.NGAYPHEP.Value);

                        kcct.TONGNGAYCONG = tongCong;
                        kcct.NGAYPHEP = tongPhep;
                        db.SaveChanges();
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception("Lỗi khi cập nhật ngày công và giờ vào/ra: " + ex.Message, ex);
                }
            }
        }
    }
}
