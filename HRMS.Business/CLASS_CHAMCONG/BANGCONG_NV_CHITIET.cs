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

        public void PhatSinhBangCongChiTiet(int makycong, int nam, int thang, int iduser, Action<int, int, string> progress = null)
        {
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
                    rawCount = db.TB_BANGCONG.Count(b => b.NAM == nam && b.THANG == thang);
                }
                catch { }

                bool hasRawPunches = rawCount > 0;
                if (hasRawPunches)
                {
                    progress?.Invoke(40, 100, $"Phát hiện {rawCount} lượt quẹt thẻ trong TB_BANGCONG. Đang đồng bộ dữ liệu thực tế...");
                }
                else
                {
                    progress?.Invoke(40, 100, "Chưa có máy chấm công quẹt thẻ. Đang phát sinh lịch công chuẩn hành chính và đơn từ...");
                }

                // 2. Xóa các bản ghi cũ của kỳ công này nếu đã có trước khi sinh mới
                var pDelMkc = new Oracle.ManagedDataAccess.Client.OracleParameter("p_del_makycong", makycong);
                db.Database.ExecuteSqlCommand("DELETE FROM TB_BANGCONG_CHITIET WHERE MAKYCONG = :p_del_makycong", pDelMkc);

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
        WHEN 'MON' THEN N'Thứ hai'
        WHEN 'TUE' THEN N'Thứ ba'
        WHEN 'WED' THEN N'Thứ tư'
        WHEN 'THU' THEN N'Thứ năm'
        WHEN 'FRI' THEN N'Thứ sáu'
        WHEN 'SAT' THEN N'Thứ bảy'
        ELSE N'Chủ nhật'
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
        WHEN 'MON' THEN N'Thứ hai'
        WHEN 'TUE' THEN N'Thứ ba'
        WHEN 'WED' THEN N'Thứ tư'
        WHEN 'THU' THEN N'Thứ năm'
        WHEN 'FRI' THEN N'Thứ sáu'
        WHEN 'SAT' THEN N'Thứ bảy'
        ELSE N'Chủ nhật'
    END),
    (CASE WHEN ct.ID IS NOT NULL AND ct.GIO_VAO IS NOT NULL THEN ct.GIO_VAO ELSE '08:00' END) AS GIOVAO,
    (CASE WHEN ct.ID IS NOT NULL AND ct.GIO_RA IS NOT NULL THEN ct.GIO_RA ELSE '17:00' END) AS GIORA,
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
                    db.Database.ExecuteSqlCommand(sql,
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
                    db.Database.ExecuteSqlCommand(sql,
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_makycong", makycong),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_iduser", iduser),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date", startDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_days", daysInMonth),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_end_date", endDateStr),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p_start_date2", startDateStr)
                    );
                }

                progress?.Invoke(80, 100, "Đang đồng bộ ngược lại ma trận TB_KYCONGCHITIET...");
                DongBoSangKyCongChiTiet(makycong, nam, thang);

                progress?.Invoke(100, 100, "Đã hoàn tất phát sinh và đồng bộ bảng công chi tiết.");
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi phát sinh bảng công chi tiết: " + ex.Message);
            }
        }

        /// <summary>
        /// Đồng bộ ngược từ TB_BANGCONG_CHITIET sang ma trận TB_KYCONGCHITIET (D1..D31, TONGNGAYCONG, NGAYPHEP...)
        /// </summary>
        public void DongBoSangKyCongChiTiet(int makycong, int nam, int thang)
        {
            try
            {
                var listBcct = db.TB_BANGCONG_CHITIET
                    .Where(x => x.MAKYCONG == makycong)
                    .ToList()
                    .GroupBy(x => Convert.ToInt32(x.MANV))
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.NGAY).ToList());

                var listKcct = db.TB_KYCONGCHITIET.Where(x => x.MAKYCONG == makycong).ToList();

                foreach (var kc in listKcct)
                {
                    int manv = Convert.ToInt32(kc.MANV);
                    if (!listBcct.TryGetValue(manv, out var days) || days == null) continue;

                    decimal tongCong = 0;
                    decimal tongPhep = 0;
                    decimal tongLe = 0;
                    decimal tongCN = 0;
                    decimal tongVang = 0;

                    foreach (var d in days)
                    {
                        if (!d.NGAY.HasValue) continue;
                        int dayNum = d.NGAY.Value.Day;
                        string kyhieu = d.KYHIEU ?? "X";

                        var prop = kc.GetType().GetProperty("D" + dayNum);
                        if (prop != null)
                        {
                            prop.SetValue(kc, kyhieu);
                        }

                        if (d.NGAYCONG.HasValue) tongCong += d.NGAYCONG.Value;
                        if (d.NGAYPHEP.HasValue && d.NGAYPHEP.Value > 0) tongPhep += d.NGAYPHEP.Value;
                        if (d.CONGNGAYLE.HasValue && d.CONGNGAYLE.Value > 0) tongLe += d.CONGNGAYLE.Value;
                        if (d.CONGCHUNHAT.HasValue && d.CONGCHUNHAT.Value > 0) tongCN += d.CONGCHUNHAT.Value;
                        if (kyhieu == "V") tongVang += 1;
                    }

                    kc.TONGNGAYCONG = tongCong;
                    kc.NGAYPHEP = tongPhep;
                    kc.CONGNGAYLE = tongLe;
                    kc.CONGCHUNHAT = tongCN;
                    kc.NGHIKHONGPHEP = tongVang;
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Lỗi đồng bộ sang TB_KYCONGCHITIET: " + ex.Message);
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
    }
}
