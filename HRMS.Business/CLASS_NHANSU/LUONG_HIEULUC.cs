using Bu.DTO;
using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu
{
    public class LUONG_HIEULUC
    {
        private readonly MyEntities _db;

        public LUONG_HIEULUC()
        {
            _db = new MyEntities();
        }

        public LUONG_HIEULUC(MyEntities db)
        {
            _db = db;
        }

        public List<LuongHieuLucDTO> GetListByNhanVien(decimal manv)
        {
            try
            {
                return _db.Database.SqlQuery<LuongHieuLucDTO>(@"
                    SELECT lh.ID, lh.MANV, lh.SOHD, lh.TU_NGAY, lh.DEN_NGAY, lh.LUONG_THANG, lh.CAN_CU, nv.HOTEN
                    FROM TB_LUONG_HIEULUC lh
                    JOIN TB_NHANVIEN nv ON nv.MANV = lh.MANV
                    WHERE lh.MANV = :p0
                    ORDER BY lh.TU_NGAY DESC, lh.ID DESC",
                    new OracleParameter("p0", manv)
                ).ToList();
            }
            catch
            {
                return new List<LuongHieuLucDTO>();
            }
        }

        public LuongHieuLucDTO GetLuongHieuLucTaiNgay(decimal manv, DateTime atDate)
        {
            try
            {
                return _db.Database.SqlQuery<LuongHieuLucDTO>(@"
                    SELECT ID, MANV, SOHD, TU_NGAY, DEN_NGAY, LUONG_THANG, CAN_CU
                    FROM (
                        SELECT ID, MANV, SOHD, TU_NGAY, DEN_NGAY, LUONG_THANG, CAN_CU
                        FROM TB_LUONG_HIEULUC
                        WHERE MANV = :p0
                          AND TU_NGAY <= :p1
                          AND (DEN_NGAY IS NULL OR DEN_NGAY >= :p1)
                        ORDER BY TU_NGAY DESC, ID DESC
                    ) WHERE ROWNUM = 1",
                    new OracleParameter("p0", manv),
                    new OracleParameter("p1", atDate)
                ).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public void UpsertLuongHieuLucHopDong(decimal manv, string sohd, DateTime tuNgay, DateTime? denNgay, decimal luongThang, string canCu)
        {
            if (luongThang <= 0) return;

            try
            {
                // Kiểm tra xem đã có bản ghi mức hưởng theo hợp đồng này chưa
                var existing = _db.Database.SqlQuery<decimal?>(@"
                    SELECT ID FROM TB_LUONG_HIEULUC
                    WHERE MANV = :p0 AND SOHD = :p1 AND TU_NGAY = :p2",
                    new OracleParameter("p0", manv),
                    new OracleParameter("p1", sohd ?? string.Empty),
                    new OracleParameter("p2", tuNgay)
                ).FirstOrDefault();

                if (existing.HasValue)
                {
                    _db.Database.ExecuteSqlCommand(@"
                        UPDATE TB_LUONG_HIEULUC
                        SET DEN_NGAY = :p0, LUONG_THANG = :p1, CAN_CU = :p2
                        WHERE ID = :p3",
                        new OracleParameter("p0", (object)denNgay ?? DBNull.Value),
                        new OracleParameter("p1", luongThang),
                        new OracleParameter("p2", canCu ?? "Hợp đồng lao động"),
                        new OracleParameter("p3", existing.Value)
                    );
                }
                else
                {
                    // Đóng kỳ hiệu lực trước đó nếu đang mở vô hạn
                    _db.Database.ExecuteSqlCommand(@"
                        UPDATE TB_LUONG_HIEULUC
                        SET DEN_NGAY = :p0
                        WHERE MANV = :p1 AND (DEN_NGAY IS NULL OR DEN_NGAY > :p0) AND TU_NGAY < :p0",
                        new OracleParameter("p0", tuNgay.AddDays(-1)),
                        new OracleParameter("p1", manv)
                    );

                    // Thêm bản ghi mới
                    _db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_LUONG_HIEULUC (MANV, SOHD, TU_NGAY, DEN_NGAY, LUONG_THANG, CAN_CU)
                        VALUES (:p0, :p1, :p2, :p3, :p4, :p5)",
                        new OracleParameter("p0", manv),
                        new OracleParameter("p1", sohd ?? string.Empty),
                        new OracleParameter("p2", tuNgay),
                        new OracleParameter("p3", (object)denNgay ?? DBNull.Value),
                        new OracleParameter("p4", luongThang),
                        new OracleParameter("p5", canCu ?? ("Hợp đồng " + sohd))
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LUONG_HIEULUC] Upsert failed: {ex.Message}");
            }
        }

        public void RecordNangLuong(decimal manv, string sohd, DateTime ngayHieuLuc, decimal luongMoi, string soQuyetDinh)
        {
            if (luongMoi <= 0) return;

            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    // Chốt ngày kết thúc của mức lương cũ
                    _db.Database.ExecuteSqlCommand(@"
                        UPDATE TB_LUONG_HIEULUC
                        SET DEN_NGAY = :p0
                        WHERE MANV = :p1 AND (DEN_NGAY IS NULL OR DEN_NGAY >= :p2) AND TU_NGAY < :p2",
                        new OracleParameter("p0", ngayHieuLuc.AddDays(-1)),
                        new OracleParameter("p1", manv),
                        new OracleParameter("p2", ngayHieuLuc)
                    );

                    // Thêm bản ghi mức lương mới theo quyết định nâng lương
                    _db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_LUONG_HIEULUC (MANV, SOHD, TU_NGAY, DEN_NGAY, LUONG_THANG, CAN_CU)
                        VALUES (:p0, :p1, :p2, NULL, :p3, :p4)",
                        new OracleParameter("p0", manv),
                        new OracleParameter("p1", sohd ?? string.Empty),
                        new OracleParameter("p2", ngayHieuLuc),
                        new OracleParameter("p3", luongMoi),
                        new OracleParameter("p4", "Quyết định nâng lương " + soQuyetDinh)
                    );

                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    System.Diagnostics.Debug.WriteLine($"[LUONG_HIEULUC] RecordNangLuong failed: {ex.Message}");
                    throw;
                }
            }
        }

        public void RecordPhuLuc(
            decimal manv,
            string sohd,
            string soPhuLuc,
            DateTime ngayHieuLuc,
            DateTime? denNgay,
            decimal luongMoi,
            string noiDung,
            string canCu,
            Dictionary<int, decimal> phuCaps = null,
            int userId = 1)
        {
            if (luongMoi <= 0)
                throw new CLASS_SYSTEM.BusinessException("VALIDATION_FAILED", "Mức lương mới trong phụ lục phải lớn hơn 0.");

            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Chốt ngày kết thúc của mức lương cũ trước ngày hiệu lực
                    _db.Database.ExecuteSqlCommand(@"
                        UPDATE TB_LUONG_HIEULUC
                        SET DEN_NGAY = :p0
                        WHERE MANV = :p1 AND (DEN_NGAY IS NULL OR DEN_NGAY >= :p2) AND TU_NGAY < :p2",
                        new OracleParameter("p0", ngayHieuLuc.AddDays(-1)),
                        new OracleParameter("p1", manv),
                        new OracleParameter("p2", ngayHieuLuc)
                    );

                    // 2. Thêm bản ghi mức lương mới theo phụ lục hợp đồng
                    _db.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_LUONG_HIEULUC (MANV, SOHD, TU_NGAY, DEN_NGAY, LUONG_THANG, CAN_CU)
                        VALUES (:p0, :p1, :p2, :p3, :p4, :p5)",
                        new OracleParameter("p0", manv),
                        new OracleParameter("p1", sohd ?? string.Empty),
                        new OracleParameter("p2", ngayHieuLuc),
                        new OracleParameter("p3", (object)denNgay ?? DBNull.Value),
                        new OracleParameter("p4", luongMoi),
                        new OracleParameter("p5", !string.IsNullOrEmpty(canCu) ? canCu : ("Phụ lục HĐ số " + soPhuLuc + (!string.IsNullOrEmpty(noiDung) ? " - " + noiDung : "")))
                    );

                    // 3. Cập nhật phụ cấp đi kèm phụ lục nếu có
                    if (phuCaps != null && phuCaps.Count > 0)
                    {
                        foreach (var kvp in phuCaps)
                        {
                            int idpc = kvp.Key;
                            decimal sotien = kvp.Value;

                            var existing = _db.TB_NHANVIEN_PHUCAP.FirstOrDefault(x => x.MANV == manv && x.IDPC == idpc);
                            if (existing != null)
                            {
                                existing.SOTIEN = sotien;
                                existing.UPDATED_BY = userId;
                                existing.UPDATED_DATE = DateTime.Now;
                                existing.TU_NGAY = ngayHieuLuc;
                                existing.DEN_NGAY = denNgay;
                            }
                            else if (sotien > 0)
                            {
                                _db.TB_NHANVIEN_PHUCAP.Add(new TB_NHANVIEN_PHUCAP
                                {
                                    MANV = manv,
                                    IDPC = idpc,
                                    SOTIEN = sotien,
                                    GHICHU = "Phụ cấp theo phụ lục " + soPhuLuc,
                                    CREATED_BY = userId,
                                    CREATED_DATE = DateTime.Now,
                                    TU_NGAY = ngayHieuLuc,
                                    DEN_NGAY = denNgay,
                                    CACH_TINH = "CO_DINH_THANG"
                                });
                            }
                        }
                        _db.SaveChanges();
                    }

                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    System.Diagnostics.Debug.WriteLine($"[LUONG_HIEULUC] RecordPhuLuc failed: {ex.Message}");
                    throw new CLASS_SYSTEM.BusinessException("ADDENDUM_SAVE_FAILED", "Không thể lưu phụ lục hợp đồng và mức hưởng mới. Giao dịch đã được khôi phục nguyên trạng.", ex);
                }
            }
        }
    }
}
