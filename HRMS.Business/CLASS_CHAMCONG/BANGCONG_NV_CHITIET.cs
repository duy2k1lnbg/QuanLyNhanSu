using DA;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
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

                // 1. Chèn các lượt chấm công chuẩn vào TB_BANGCONG (chỉ chèn những ngày/nhân viên chưa có lượt chấm nào)
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
    SELECT TO_DATE(:p_start_date1, 'YYYY-MM-DD') + LEVEL - 1 as ngay
    FROM DUAL
    CONNECT BY LEVEL <= :p_days
) d
CROSS JOIN (
    SELECT nv.MANV
    FROM TB_NHANVIEN nv
    WHERE (nv.DELETED_DATE IS NULL OR nv.DELETED_DATE >= TO_DATE(:p_start_date_del, 'YYYY-MM-DD'))
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
  AND phep.ID IS NULL
  AND NOT EXISTS (
      SELECT 1 FROM TB_BANGCONG ex 
      WHERE ex.MANV = nv.MANV 
        AND ex.NAM = :p_ex_nam 
        AND ex.THANG = :p_ex_thang 
        AND ex.NGAY = EXTRACT(DAY FROM d.ngay)
  )";

                int inserted = activeDb.Database.ExecuteSqlCommand(sql,
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_nam", nam),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_thang", thang),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date1", startDateStr),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_days", daysInMonth),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date_del", startDateStr),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_end_date", endDateStr),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date2", startDateStr),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_ex_nam", nam),
                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_ex_thang", thang)
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

        public void PhatSinhBangCongChiTiet(int makycong, int nam, int thang, int iduser, Action<int, int, string> progress = null, MyEntities dbInstance = null, bool tuPhatSinhBangCong = true, bool runPublishing = true)
        {
            bool isLocalContext = (dbInstance == null);
            var activeDb = dbInstance ?? new MyEntities();
            try
            {
                var kcCheck = activeDb.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                if (kcCheck != null && (kcCheck.KHOA ?? 0) == 1)
                {
                    throw new InvalidOperationException($"Kỳ công {makycong} đã bị khóa (KHOA = 1), không thể phát sinh lại bảng công chi tiết.");
                }

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

                // 2. Không xóa dữ liệu cũ (non-destructive): Dùng MERGE INTO TB_BANGCONG_CHITIET để bảo toàn IDBANGCONGCT và lịch sử
                string sql;
                if (hasRawPunches)
                {
                    // CHẾ ĐỘ 1: Đồng bộ trực tiếp từ TB_BANGCONG (quẹt thẻ thực tế) kết hợp Đơn phép, Đơn công tác, Ngày lễ
                    sql = @"
MERGE INTO TB_BANGCONG_CHITIET dest
USING (
    SELECT 
        :p_makycong as MAKYCONG,
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
        END) as THU,
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
        END) AS NGAYCONG
    FROM (
        SELECT TO_DATE(:p_start_date1, 'YYYY-MM-DD') + LEVEL - 1 as ngay
        FROM DUAL
        CONNECT BY LEVEL <= :p_days
    ) d
    CROSS JOIN (
        SELECT nv.MANV, nv.IDCTY, nv.HOTEN
        FROM TB_NHANVIEN nv
        WHERE (nv.DELETED_DATE IS NULL OR nv.DELETED_DATE >= TO_DATE(:p_start_date_del, 'YYYY-MM-DD'))
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
                                        AND ct.TRANGTHAI = 'APPROVED'
) src
ON (dest.MAKYCONG = src.MAKYCONG AND dest.MANV = src.MANV AND TRUNC(dest.NGAY) = TRUNC(src.ngay))
WHEN MATCHED THEN
UPDATE SET
    dest.IDCTY = src.IDCTY,
    dest.HOTEN = src.HOTEN,
    dest.THU = src.THU,
    dest.GIOVAO = src.GIO_VAO,
    dest.GIORA = src.GIO_RA,
    dest.NGAYPHEP = src.NGAYPHEP,
    dest.CONGNGAYLE = src.CONGNGAYLE,
    dest.CONGCHUNHAT = src.CONGCHUNHAT,
    dest.KYHIEU = src.KYHIEU,
    dest.NGAYCONG = src.NGAYCONG,
    dest.UPDATED_BY = :p_upd_user,
    dest.UPDATED_DATE = SYSDATE
WHEN NOT MATCHED THEN
INSERT (
    MAKYCONG, IDCTY, MANV, HOTEN, NGAY, THU, GIOVAO, GIORA, 
    NGAYPHEP, CONGNGAYLE, CONGCHUNHAT, KYHIEU, NGAYCONG, CREATED_BY, CREATED_DATE
) VALUES (
    src.MAKYCONG, src.IDCTY, src.MANV, src.HOTEN, src.ngay, src.THU, src.GIO_VAO, src.GIO_RA,
    src.NGAYPHEP, src.CONGNGAYLE, src.CONGCHUNHAT, src.KYHIEU, src.NGAYCONG, :p_ins_user, SYSDATE
)";
                }
                else
                {
                    // CHẾ ĐỘ 2: Chưa có máy chấm công -> Phát sinh lịch chuẩn hành chính 8h-17h, tự động tích hợp Đơn phép, Đơn công tác, Ngày lễ
                    sql = @"
MERGE INTO TB_BANGCONG_CHITIET dest
USING (
    SELECT 
        :p_makycong as MAKYCONG,
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
        END) as THU,
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
        END) AS NGAYCONG
    FROM (
        SELECT TO_DATE(:p_start_date1, 'YYYY-MM-DD') + LEVEL - 1 as ngay
        FROM DUAL
        CONNECT BY LEVEL <= :p_days
    ) d
    CROSS JOIN (
        SELECT nv.MANV, nv.IDCTY, nv.HOTEN
        FROM TB_NHANVIEN nv
        WHERE (nv.DELETED_DATE IS NULL OR nv.DELETED_DATE >= TO_DATE(:p_start_date_del, 'YYYY-MM-DD'))
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
) src
ON (dest.MAKYCONG = src.MAKYCONG AND dest.MANV = src.MANV AND TRUNC(dest.NGAY) = TRUNC(src.ngay))
WHEN MATCHED THEN
UPDATE SET
    dest.IDCTY = src.IDCTY,
    dest.HOTEN = src.HOTEN,
    dest.THU = src.THU,
    dest.GIOVAO = src.GIOVAO,
    dest.GIORA = src.GIORA,
    dest.NGAYPHEP = src.NGAYPHEP,
    dest.CONGNGAYLE = src.CONGNGAYLE,
    dest.CONGCHUNHAT = src.CONGCHUNHAT,
    dest.KYHIEU = src.KYHIEU,
    dest.NGAYCONG = src.NGAYCONG,
    dest.UPDATED_BY = :p_upd_user,
    dest.UPDATED_DATE = SYSDATE
WHEN NOT MATCHED THEN
INSERT (
    MAKYCONG, IDCTY, MANV, HOTEN, NGAY, THU, GIOVAO, GIORA, 
    NGAYPHEP, CONGNGAYLE, CONGCHUNHAT, KYHIEU, NGAYCONG, CREATED_BY, CREATED_DATE
) VALUES (
    src.MAKYCONG, src.IDCTY, src.MANV, src.HOTEN, src.ngay, src.THU, src.GIOVAO, src.GIORA,
    src.NGAYPHEP, src.CONGNGAYLE, src.CONGCHUNHAT, src.KYHIEU, src.NGAYCONG, :p_ins_user, SYSDATE
)";
                }

                if (hasRawPunches)
                {
                    activeDb.Database.ExecuteSqlCommand(sql,
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_makycong", makycong),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date1", startDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_days", daysInMonth),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date_del", startDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_end_date", endDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date2", startDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_nam", nam),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_thang", thang),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_upd_user", iduser),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_ins_user", iduser)
                    );
                }
                else
                {
                    activeDb.Database.ExecuteSqlCommand(sql,
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_makycong", makycong),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date1", startDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_days", daysInMonth),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date_del", startDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_end_date", endDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date2", startDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_upd_user", iduser),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_ins_user", iduser)
                    );
                }

                progress?.Invoke(80, 100, "Đang đồng bộ ngược lại ma trận TB_KYCONGCHITIET...");
                DongBoSangKyCongChiTiet(makycong, nam, thang, activeDb);

                // 3. Chạy TimeSegmentationEngine và công bố chính thức bảng công V1.18 (Bước B: tách biệt với Bước A)
                if (runPublishing)
                {
                    progress?.Invoke(85, 100, "Đang phân đoạn thời gian và công bố bảng công V1.18...");
                    var publishingService = new AttendancePublishingService();
                    var pubResult = publishingService.PublishAttendance(new AttendancePublishRequest
                    {
                        MaKyCong = makycong,
                        NguoiThucHien = iduser,
                        GhiChu = "Phát sinh và công bố công tự động V1.18",
                        ForceRecalculate = true
                    });
                    if (!pubResult.Success)
                    {
                        throw new InvalidOperationException("Đã lưu đầu vào bảng công thành công, nhưng công bố kết quả chưa hoàn tất: " + pubResult.ErrorMessage + ". Bạn có thể xử lý các vấn đề và chạy lại công bố mà không cần phát sinh lại đầu vào.");
                    }
                }

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
            var kcCheck = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
            if (kcCheck != null && (kcCheck.KHOA ?? 0) == 1)
            {
                throw new InvalidOperationException($"Kỳ công {makycong} đã bị khóa (KHOA = 1), không thể import dữ liệu chấm công.");
            }
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
                var kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == bcct.MAKYCONG);
                if (kc != null && (kc.KHOA ?? 0) == 1)
                {
                    throw new InvalidOperationException($"Kỳ công {bcct.MAKYCONG} đã bị khóa (KHOA = 1). Bảng công chi tiết chỉ có thể xem, không thể chỉnh sửa.");
                }

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

        public void CapNhatNgayCongVaBangCongRaw(int manv, int makycong, int nam, int thang, int ngay, int gioVao, int phutVao, int gioRa, int phutRa, string kyhieu, string loaiNghi, int iduser, string ghiChu = null)
        {
            decimal nc = 1.0m;
            decimal np = 0.0m;
            if (kyhieu == "P") { nc = (loaiNghi == "NN" || loaiNghi == "KHONG") ? 1m : 0.5m; np = nc; }
            else if (kyhieu == "V") { nc = (loaiNghi == "NN" || loaiNghi == "KHONG") ? 0m : 0.5m; np = 0m; }
            else if (kyhieu == "VR") { nc = (loaiNghi == "NN" || loaiNghi == "KHONG") ? 0m : 0.5m; np = (loaiNghi == "NN" || loaiNghi == "KHONG") ? 1m : 0.5m; }
            else if (kyhieu == "CT") { nc = 1.0m; np = 0m; }

            CapNhatNgayCongVaBangCongRaw(manv, makycong, nam, thang, ngay, gioVao, phutVao, gioRa, phutRa, kyhieu, loaiNghi, nc, np, iduser, ghiChu, null);
        }

        /// <summary>
        /// Cập nhật đồng thời TB_BANGCONG, TB_BANGCONG_CHITIET và TB_KYCONGCHITIET trong 1 ACID transaction
        /// Hỗ trợ giờ rỗng/null (nghỉ nguyên ngày), tính công chính xác và cập nhật ca làm việc
        /// </summary>
        public void CapNhatNgayCongVaBangCongRaw(
            int manv, int makycong, int nam, int thang, int ngay,
            int? gioVao, int? phutVao, int? gioRa, int? phutRa,
            string kyhieu, string loaiNghi, decimal ngayCong, decimal ngayPhep,
            int iduser, string ghiChu = null, long? idCaPhienBan = null)
        {
            var kcLock = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
            if (kcLock != null && (kcLock.KHOA ?? 0) == 1)
            {
                throw new InvalidOperationException($"Kỳ công {makycong} đã bị khóa (KHOA = 1). Bảng công chi tiết chỉ có thể xem, không thể chỉnh sửa.");
            }

            DateTime ngayDate = new DateTime(nam, thang, ngay);
            using (var trans = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
            {
                try
                {
                    var bcct = db.TB_BANGCONG_CHITIET.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv && x.NGAY == ngayDate);

                    bool giuNguyenCong = (kyhieu == "KHONG_DOI" || string.IsNullOrEmpty(kyhieu));
                    if (giuNguyenCong)
                    {
                        if (bcct != null && !string.IsNullOrEmpty(bcct.KYHIEU))
                        {
                            kyhieu = bcct.KYHIEU;
                            ngayCong = bcct.NGAYCONG ?? 1m;
                            ngayPhep = bcct.NGAYPHEP ?? 0m;
                        }
                        else
                        {
                            kyhieu = "X";
                        }
                    }

                    bool isNghi = (kyhieu == "P" || kyhieu == "V" || kyhieu == "VR");
                    bool isNghiNguyenNgay = (isNghi && loaiNghi == "NN");
                    bool hasPunch = (gioVao.HasValue && gioRa.HasValue && (!isNghiNguyenNgay || giuNguyenCong));

                    // 0. Cập nhật phân ca nếu có chỉ định idCaPhienBan
                    if (idCaPhienBan.HasValue)
                    {
                        try
                        {
                            var allShifts = AttendanceCalculationHelper.LoadAllShifts();
                            var shiftObj = allShifts.FirstOrDefault(s => s.IdCaPhienBan == idCaPhienBan.Value);
                            DateTime batDauKeHoach = ngayDate.Date.AddHours(8);
                            DateTime ketThucKeHoach = ngayDate.Date.AddHours(17);
                            if (shiftObj != null && shiftObj.WorkFrames.Count > 0)
                            {
                                var firstFrame = shiftObj.WorkFrames.First();
                                var lastFrame = shiftObj.WorkFrames.Last();
                                batDauKeHoach = ngayDate.Date.AddMinutes(firstFrame.BatDauPhut);
                                ketThucKeHoach = ngayDate.Date.AddMinutes(lastFrame.KetThucPhut);
                            }

                            int updated = db.Database.ExecuteSqlCommand(@"
                                UPDATE HR.TB_LICH_LAMVIEC 
                                SET IDCAPHIENBAN = :p_idca,
                                    TRANG_THAI_PHAN_CONG = 'LAM_VIEC',
                                    BATDAU_KEHOACH = :p_start,
                                    KETTHUC_KEHOACH = :p_end,
                                    IDQUYDINH = NVL(IDQUYDINH, 1),
                                    TRANG_THAI = 'PUBLISHED',
                                    NGUON_PHAN_CONG = 'DIEU_CHINH',
                                    TAO_BOI = :p_user
                                WHERE MANV = :p_manv AND TRUNC(NGAY) = :p_ngay",
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p_idca", idCaPhienBan.Value),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p_start", batDauKeHoach),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p_end", ketThucKeHoach),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p_user", iduser > 0 ? (decimal)iduser : 1m),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p_manv", (decimal)manv),
                                new Oracle.ManagedDataAccess.Client.OracleParameter("p_ngay", ngayDate)
                            );

                            if (updated == 0)
                            {
                                string maPhanCong = $"STD_{manv}_{ngayDate:yyyyMMdd}";
                                db.Database.ExecuteSqlCommand(@"
                                    INSERT INTO HR.TB_LICH_LAMVIEC (
                                        MANV, NGAY, MA_PHANCONG, SO_PHIENBAN, IDCAPHIENBAN, IDQUYDINH,
                                        BATDAU_KEHOACH, KETTHUC_KEHOACH, TRANG_THAI_PHAN_CONG, LOAI_NGAY,
                                        TRANG_THAI, NGUON_PHAN_CONG, TAO_BOI, LY_DO
                                    ) VALUES (
                                        :p_manv, :p_ngay, :p_mapc, 1, :p_idca, 1,
                                        :p_start, :p_end,
                                        'LAM_VIEC', 'THUONG', 'PUBLISHED', 'DIEU_CHINH', :p_user, 'Cập nhật phân ca từ chấm công'
                                    )",
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_manv", (decimal)manv),
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_ngay", ngayDate),
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_mapc", maPhanCong),
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_idca", idCaPhienBan.Value),
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_start", batDauKeHoach),
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_end", ketThucKeHoach),
                                    new Oracle.ManagedDataAccess.Client.OracleParameter("p_user", iduser > 0 ? (decimal)iduser : 1m)
                                );
                            }
                        }
                        catch (Exception exLich)
                        {
                            System.Diagnostics.Debug.WriteLine("Cập nhật TB_LICH_LAMVIEC không thành công: " + exLich.Message);
                        }
                    }

                    // 1. Cập nhật hoặc thêm mới vào TB_BANGCONG (Dữ liệu quẹt thẻ gốc)
                    var raw = db.TB_BANGCONG.FirstOrDefault(x => x.MANV == manv && x.NAM == nam && x.THANG == thang && x.NGAY == ngay);
                    decimal idLoaiCong = (kyhieu == "CD") ? 2 : (raw != null && raw.IDLOAICONG.HasValue ? raw.IDLOAICONG.Value : 1);

                    if (hasPunch)
                    {
                        if (raw != null)
                        {
                            raw.GIOVAO = gioVao.Value;
                            raw.PHUTVAO = phutVao.GetValueOrDefault();
                            raw.GIORA = gioRa.Value;
                            raw.PHUTRA = phutRa.GetValueOrDefault();
                            raw.IDLOAICONG = idLoaiCong;
                        }
                        else
                        {
                            raw = new TB_BANGCONG
                            {
                                MANV = manv,
                                NAM = nam,
                                THANG = thang,
                                NGAY = ngay,
                                GIOVAO = gioVao.Value,
                                PHUTVAO = phutVao.GetValueOrDefault(),
                                GIORA = gioRa.Value,
                                PHUTRA = phutRa.GetValueOrDefault(),
                                IDLOAICONG = idLoaiCong
                            };
                            db.TB_BANGCONG.Add(raw);
                        }
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

                    // Nghỉ nguyên ngày: không lưu đồng thời giờ làm được xác nhận (GIOVAO, GIORA phải NULL)
                    if ((isNghiNguyenNgay && !giuNguyenCong) || !hasPunch)
                    {
                        bcct.GIOVAO = null;
                        bcct.GIORA = null;
                    }
                    else
                    {
                        bcct.GIOVAO = $"{gioVao.Value:D2}:{phutVao.GetValueOrDefault():D2}";
                        bcct.GIORA = $"{gioRa.Value:D2}:{phutRa.GetValueOrDefault():D2}";
                    }

                    if (!giuNguyenCong)
                    {
                        bcct.KYHIEU = kyhieu;
                        bcct.NGAYCONG = ngayCong;
                        bcct.NGAYPHEP = ngayPhep;
                    }
                    bcct.UPDATED_BY = iduser;
                    bcct.UPDATED_DATE = DateTime.Now;
                    if (!string.IsNullOrEmpty(ghiChu))
                    {
                        bcct.GHICHU = ghiChu;
                    }

                    db.SaveChanges();

                    // 3. Cập nhật ma trận TB_KYCONGCHITIET (D1..D31, TONGNGAYCONG, NGAYPHEP)
                    var kcct = db.TB_KYCONGCHITIET.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv);
                    if (kcct != null)
                    {
                        string fieldName = "D" + ngay;
                        var prop = kcct.GetType().GetProperty(fieldName);
                        if (prop != null)
                        {
                            prop.SetValue(kcct, kyhieu);
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

                    // 4. Tăng CONG_INPUT_REV trên TB_KYCONG để đánh dấu dữ liệu đầu vào đã thay đổi
                    db.Database.ExecuteSqlCommand(
                        "UPDATE TB_KYCONG SET CONG_INPUT_REV = NVL(CONG_INPUT_REV, 0) + 1 WHERE MAKYCONG = :p_mkc",
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_mkc", makycong)
                    );

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    try { trans.Rollback(); } catch (InvalidOperationException) { }
                    throw new Exception("Lỗi khi cập nhật ngày công và giờ vào/ra: " + ex.Message, ex);
                }
            }

            // 5. Tự động tính toán lại phân đoạn thời gian và công bố lại cho ngày vừa cập nhật
            // Thực hiện hoàn toàn ngoài transaction của EF để nhả hết lock trước đó
            // Chỉ công bố khi kỳ công tồn tại và nhân viên thực tế có trong TB_NHANVIEN (thỏa mãn FK18_KQ_NV)
            if (kcLock != null && db.TB_NHANVIEN.Any(x => x.MANV == manv))
            {
                try
                {
                    var pubService = new AttendancePublishingService();
                    var published = pubService.PublishAttendance(new AttendancePublishRequest
                    {
                        MaKyCong = makycong,
                        TuNgay = ngayDate,
                        DenNgay = ngayDate.AddDays(1),
                        ManvList = new List<long> { manv },
                        NguoiThucHien = iduser,
                        GhiChu = $"Cập nhật công ngày {ngayDate:dd/MM/yyyy} từ Desktop UI"
                    });
                    if (!published.Success) throw new InvalidOperationException(published.ErrorMessage);

                    // Đảm bảo trạng thái công/phép thủ công do người dùng xác nhận không bị engine tự động ghi đè
                    using (var syncConn = new Oracle.ManagedDataAccess.Client.OracleConnection(pubService.ConnectionString))
                    {
                        syncConn.Open();
                        using (var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                            UPDATE TB_BANGCONG_CHITIET 
                            SET KYHIEU = :p_kh, NGAYCONG = :p_nc, NGAYPHEP = :p_np 
                            WHERE MAKYCONG = :p_mkc AND MANV = :p_manv AND TRUNC(NGAY) = :p_ngay", syncConn))
                        {
                            cmd.BindByName = true;
                            cmd.Parameters.Add("p_kh", kyhieu);
                            cmd.Parameters.Add("p_nc", ngayCong);
                            cmd.Parameters.Add("p_np", ngayPhep);
                            cmd.Parameters.Add("p_mkc", makycong);
                            cmd.Parameters.Add("p_manv", manv);
                            cmd.Parameters.Add("p_ngay", ngayDate);
                            cmd.ExecuteNonQuery();
                        }

                        string colD = $"D{ngay}";
                        using (var cmdKcct = new Oracle.ManagedDataAccess.Client.OracleCommand($@"
                            UPDATE TB_KYCONGCHITIET 
                            SET {colD} = :p_kh,
                                TONGNGAYCONG = (SELECT NVL(SUM(NGAYCONG),0) FROM TB_BANGCONG_CHITIET WHERE MAKYCONG = :p_mkc AND MANV = :p_manv),
                                NGAYPHEP = (SELECT NVL(SUM(NGAYPHEP),0) FROM TB_BANGCONG_CHITIET WHERE MAKYCONG = :p_mkc AND MANV = :p_manv)
                            WHERE MAKYCONG = :p_mkc AND MANV = :p_manv", syncConn))
                        {
                            cmdKcct.BindByName = true;
                            cmdKcct.Parameters.Add("p_kh", kyhieu);
                            cmdKcct.Parameters.Add("p_mkc", makycong);
                            cmdKcct.Parameters.Add("p_manv", manv);
                            cmdKcct.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception exPub)
                {
                    throw new InvalidOperationException("Dữ liệu nhập đã lưu nhưng công bố lại chưa thành công. Cần xử lý trước khi chốt kỳ: " + exPub.Message, exPub);
                }
            }
        }

        /// <summary>
        /// Lấy chi tiết thông tin ngày công chuẩn V1.18 cùng trạng thái con trỏ và bất thường
        /// </summary>
        public BangCongChiTietV118Dto GetChiTietNgayCongV118(int makycong, int manv, int ngay)
        {
            int nam = makycong / 100;
            int thang = makycong % 100;
            DateTime date = new DateTime(nam, thang, ngay);

            string connStr = AttendanceCalculationHelper.GetStoreConnectionString(db);
            using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(connStr))
            {
                conn.Open();
                BangCongChiTietV118Dto dto = null;

                using (var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                    SELECT 
                        bc.IDBANGCONGCT, bc.MAKYCONG, bc.MANV, bc.HOTEN, bc.NGAY, bc.GIOVAO, bc.GIORA,
                        bc.KYHIEU, bc.NGAYCONG, bc.LANTINH_ID_HIENHANH, bc.TRANGTHAI_CONG, bc.DU_DIEUKIEN_CHOT,
                        bc.GIAY_THUC_TE, bc.GIAY_HUONG_CONG_THUONG, bc.GIAY_OT_XAC_NHAN,
                        bc.GIAY_DEM_TRONG_GIO_THUONG, bc.GIAY_DEM_OT,
                        bc.GIAY_DI_MUON_THUC_TE, bc.GIAY_VE_SOM_THUC_TE,
                        bc.GIAY_DI_MUON_VIPHAM, bc.GIAY_VE_SOM_VIPHAM,
                        kc.KHOA, kc.CONG_PUBLISH_REV
                    FROM HR.TB_BANGCONG_CHITIET bc
                    LEFT JOIN HR.TB_KYCONG kc ON kc.MAKYCONG = bc.MAKYCONG
                    WHERE bc.MAKYCONG = :p_mkc AND bc.MANV = :p_manv AND TRUNC(bc.NGAY) = :p_ngay", conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_mkc", makycong);
                    cmd.Parameters.Add("p_manv", manv);
                    cmd.Parameters.Add("p_ngay", date);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dto = new BangCongChiTietV118Dto
                            {
                                IdBangCongCt = Convert.ToDecimal(reader["IDBANGCONGCT"]),
                                MaKyCong = Convert.ToDecimal(reader["MAKYCONG"]),
                                MaNV = Convert.ToDecimal(reader["MANV"]),
                                HoTen = reader["HOTEN"]?.ToString(),
                                Ngay = Convert.ToDateTime(reader["NGAY"]),
                                GioVao = reader["GIOVAO"]?.ToString(),
                                GioRa = reader["GIORA"]?.ToString(),
                                KyHieu = reader["KYHIEU"]?.ToString(),
                                NgayCong = reader["NGAYCONG"] != DBNull.Value ? Convert.ToDecimal(reader["NGAYCONG"]) : (decimal?)null,
                                LanTinhIdHienHanh = reader["LANTINH_ID_HIENHANH"] != DBNull.Value ? Convert.ToInt64(reader["LANTINH_ID_HIENHANH"]) : (long?)null,
                                TrangThaiCong = reader["TRANGTHAI_CONG"]?.ToString(),
                                DuDieuKienChot = reader["DU_DIEUKIEN_CHOT"] != DBNull.Value ? Convert.ToInt32(reader["DU_DIEUKIEN_CHOT"]) : (int?)null,
                                GiayThucTe = reader["GIAY_THUC_TE"] != DBNull.Value ? Convert.ToInt64(reader["GIAY_THUC_TE"]) : (long?)null,
                                GiayHuongCongThuong = reader["GIAY_HUONG_CONG_THUONG"] != DBNull.Value ? Convert.ToInt64(reader["GIAY_HUONG_CONG_THUONG"]) : (long?)null,
                                GiayOtXacNhan = reader["GIAY_OT_XAC_NHAN"] != DBNull.Value ? Convert.ToInt64(reader["GIAY_OT_XAC_NHAN"]) : (long?)null,
                                GiayDemTrongGioThuong = reader["GIAY_DEM_TRONG_GIO_THUONG"] != DBNull.Value ? Convert.ToInt64(reader["GIAY_DEM_TRONG_GIO_THUONG"]) : (long?)null,
                                GiayDemOt = reader["GIAY_DEM_OT"] != DBNull.Value ? Convert.ToInt64(reader["GIAY_DEM_OT"]) : (long?)null,
                                GiayDiMuonThucTe = reader["GIAY_DI_MUON_THUC_TE"] != DBNull.Value ? Convert.ToInt64(reader["GIAY_DI_MUON_THUC_TE"]) : (long?)null,
                                GiayVeSomThucTe = reader["GIAY_VE_SOM_THUC_TE"] != DBNull.Value ? Convert.ToInt64(reader["GIAY_VE_SOM_THUC_TE"]) : (long?)null,
                                GiayDiMuonViPham = reader["GIAY_DI_MUON_VIPHAM"] != DBNull.Value ? Convert.ToInt64(reader["GIAY_DI_MUON_VIPHAM"]) : (long?)null,
                                GiayVeSomViPham = reader["GIAY_VE_SOM_VIPHAM"] != DBNull.Value ? Convert.ToInt64(reader["GIAY_VE_SOM_VIPHAM"]) : (long?)null,
                                KyCongKhoa = reader["KHOA"] != DBNull.Value ? Convert.ToInt32(reader["KHOA"]) : 0
                            };
                        }
                    }
                }

                if (dto != null && dto.LanTinhIdHienHanh.HasValue)
                {
                    using (var anomCmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                        SELECT IDBATTHUONG, 
                               MA_LOI AS MA_LOAI_BATTHUONG, 
                               BATDAU_LUC, 
                               KETTHUC_LUC,
                               (CASE 
                                   WHEN BATDAU_LUC IS NOT NULL AND KETTHUC_LUC IS NOT NULL 
                                   THEN ROUND((CAST(KETTHUC_LUC AS DATE) - CAST(BATDAU_LUC AS DATE)) * 86400) 
                                   ELSE NULL 
                                END) AS THOILUONG_GIAY, 
                               CHAN_CHOT, 
                               TRANG_THAI, 
                               MO_TA
                        FROM HR.TB_CHAMCONG_BATTHUONG
                        WHERE IDLANTINH_PHATHIEN = :p_idlt 
                          AND MANV = :p_manv
                          AND (IDBANGCONGCT = :p_idbcct OR TRUNC(NGAY) = :p_ngay)", conn))
                    {
                        anomCmd.BindByName = true;
                        anomCmd.Parameters.Add("p_idlt", dto.LanTinhIdHienHanh.Value);
                        anomCmd.Parameters.Add("p_manv", manv);
                        anomCmd.Parameters.Add("p_idbcct", dto.IdBangCongCt);
                        anomCmd.Parameters.Add("p_ngay", date);

                        using (var reader = anomCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dto.BatThuongList.Add(new ChamCongBatThuongDto
                                {
                                    IdBatThuong = Convert.ToInt64(reader["IDBATTHUONG"]),
                                    MaLoaiBatThuong = reader["MA_LOAI_BATTHUONG"]?.ToString(),
                                    BatDau = reader["BATDAU_LUC"] != DBNull.Value ? Convert.ToDateTime(reader["BATDAU_LUC"]) : (DateTime?)null,
                                    KetThuc = reader["KETTHUC_LUC"] != DBNull.Value ? Convert.ToDateTime(reader["KETTHUC_LUC"]) : (DateTime?)null,
                                    ThoiLuongGiay = reader["THOILUONG_GIAY"] != DBNull.Value ? Convert.ToInt64(reader["THOILUONG_GIAY"]) : (long?)null,
                                    ChanChot = reader["CHAN_CHOT"] != DBNull.Value ? Convert.ToInt32(reader["CHAN_CHOT"]) : 0,
                                    TrangThai = reader["TRANG_THAI"]?.ToString(),
                                    MoTa = reader["MO_TA"]?.ToString()
                                });
                            }
                        }
                    }
                }

                return dto;
            }
        }
    }

    public class BangCongChiTietV118Dto
    {
        public decimal IdBangCongCt { get; set; }
        public decimal MaKyCong { get; set; }
        public decimal MaNV { get; set; }
        public string HoTen { get; set; }
        public DateTime Ngay { get; set; }
        public string GioVao { get; set; }
        public string GioRa { get; set; }
        public string KyHieu { get; set; }
        public decimal? NgayCong { get; set; }
        public long? LanTinhIdHienHanh { get; set; }
        public string TrangThaiCong { get; set; }
        public int? DuDieuKienChot { get; set; }
        public long? GiayThucTe { get; set; }
        public long? GiayHuongCongThuong { get; set; }
        public long? GiayOtXacNhan { get; set; }
        public long? GiayDemTrongGioThuong { get; set; }
        public long? GiayDemOt { get; set; }
        public long? GiayDiMuonThucTe { get; set; }
        public long? GiayVeSomThucTe { get; set; }
        public long? GiayDiMuonViPham { get; set; }
        public long? GiayVeSomViPham { get; set; }
        public int KyCongKhoa { get; set; }
        public List<ChamCongBatThuongDto> BatThuongList { get; set; } = new List<ChamCongBatThuongDto>();
    }

    public class ChamCongBatThuongDto
    {
        public long IdBatThuong { get; set; }
        public string MaLoaiBatThuong { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public long? ThoiLuongGiay { get; set; }
        public int ChanChot { get; set; }
        public string TrangThai { get; set; }
        public string MoTa { get; set; }
    }
}
