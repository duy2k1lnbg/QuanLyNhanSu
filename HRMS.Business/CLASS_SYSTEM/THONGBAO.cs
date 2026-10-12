using DA;
using Bu.DTO;
using Bu.CLASS_SYSTEM;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu
{
    public class THONGBAO
    {
        private MyEntities db = new MyEntities();

        public TB_THONGBAO getItem(decimal id)
        {
            try
            {
                return db.Database.SqlQuery<TB_THONGBAO>(
                    "SELECT ID, TIEUDE, NOIDUNG, NGUOIDANG, NGAYDANG, LOAI_TB, IS_PINNED, TRANGTHAI, NGAY_HETHAN, FILE_DINHKEM, MACTY, MAPB FROM TB_THONGBAO WHERE ID = :ID",
                    new OracleParameter("ID", id)
                ).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy chi tiết thông báo: " + ex.Message);
            }
        }

        public List<TB_THONGBAO> getList()
        {
            try
            {
                return db.Database.SqlQuery<TB_THONGBAO>(
                    "SELECT ID, TIEUDE, NOIDUNG, NGUOIDANG, NGAYDANG, LOAI_TB, IS_PINNED, TRANGTHAI, NGAY_HETHAN, FILE_DINHKEM, MACTY, MAPB FROM TB_THONGBAO ORDER BY IS_PINNED DESC, NGAYDANG DESC"
                ).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách thông báo: " + ex.Message);
            }
        }

        public List<THONGBAO_DTO> getListFull_DTO(string langCode = "vi")
        {
            try
            {
                var lstTB = db.Database.SqlQuery<TB_THONGBAO>(
                    "SELECT ID, TIEUDE, NOIDUNG, NGUOIDANG, NGAYDANG, LOAI_TB, IS_PINNED, TRANGTHAI, NGAY_HETHAN, FILE_DINHKEM, MACTY, MAPB FROM TB_THONGBAO ORDER BY IS_PINNED DESC, NGAYDANG DESC"
                ).ToList();

                var lstCT = db.TB_CONGTY.ToList();
                var lstPB = new PHONGBAN().getListDTO(langCode);

                var lstDTO = new List<THONGBAO_DTO>();
                foreach (var item in lstTB)
                {
                    var dto = new THONGBAO_DTO
                    {
                        ID = item.ID,
                        TIEUDE = item.TIEUDE,
                        NOIDUNG = item.NOIDUNG,
                        NGUOIDANG = item.NGUOIDANG,
                        NGAYDANG = item.NGAYDANG,
                        LOAI_TB = item.LOAI_TB,
                        IS_PINNED = item.IS_PINNED.HasValue ? (item.IS_PINNED.Value ? 1 : 0) : (decimal?)null,
                        TRANGTHAI = item.TRANGTHAI.HasValue ? (item.TRANGTHAI.Value ? 1 : 0) : (decimal?)null,
                        NGAY_HETHAN = item.NGAY_HETHAN,
                        FILE_DINHKEM = item.FILE_DINHKEM,
                        MACTY = item.MACTY,
                        MAPB = item.MAPB
                    };

                    if (!string.IsNullOrEmpty(item.MACTY))
                    {
                        var cty = lstCT.FirstOrDefault(x => x.IDCTY.ToString() == item.MACTY);
                        if (cty != null)
                            dto.TENCTY = cty.TENCTY;
                    }
                    else
                    {
                        dto.TENCTY = "Tất cả";
                    }

                    if (!string.IsNullOrEmpty(item.MAPB))
                    {
                        var pb = lstPB.FirstOrDefault(x => x.IDPB.ToString() == item.MAPB);
                        if (pb != null)
                            dto.TENPB = pb.TENPB;
                    }
                    else
                    {
                        dto.TENPB = "Tất cả";
                    }

                    lstDTO.Add(dto);
                }

                return lstDTO;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách DTO: " + ex.Message);
            }
        }

        public void Add(TB_THONGBAO tb)
        {
            try
            {
                string targetType = "ALL";
                if (!string.IsNullOrWhiteSpace(tb.MAPB))
                {
                    targetType = "DEPARTMENT";
                }
                else if (!string.IsNullOrWhiteSpace(tb.MACTY))
                {
                    targetType = "COMPANY";
                }

                string connStr = Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    NotificationService.CreateNotification(
                        conn,
                        null,
                        tb.TIEUDE,
                        tb.NOIDUNG,
                        tb.LOAI_TB ?? "THONG_BAO_CHUNG",
                        tb.NGUOIDANG ?? "Hệ thống",
                        targetType: targetType,
                        targetManv: null,
                        mapb: tb.MAPB,
                        macty: tb.MACTY,
                        eventType: "ANNOUNCEMENT",
                        discretePushBody: "HRMS: Công ty vừa phát hành thông báo mới. Mở ứng dụng để xem.",
                        isPinned: tb.IS_PINNED == true,
                        attachedFile: tb.FILE_DINHKEM,
                        expiresAt: tb.NGAY_HETHAN,
                        isActive: tb.TRANGTHAI != false
                    );
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm thông báo mới: " + ex.Message);
            }
        }

        public void Update(TB_THONGBAO tb)
        {
            try
            {
                string targetType = "ALL";
                if (!string.IsNullOrWhiteSpace(tb.MAPB))
                {
                    targetType = "DEPARTMENT";
                }
                else if (!string.IsNullOrWhiteSpace(tb.MACTY))
                {
                    targetType = "COMPANY";
                }

                db.Database.ExecuteSqlCommand(
                    @"UPDATE TB_THONGBAO SET 
                        TIEUDE = :TIEUDE, 
                        NOIDUNG = :NOIDUNG, 
                        NGUOIDANG = :NGUOIDANG, 
                        NGAYDANG = :NGAYDANG, 
                        LOAI_TB = :LOAI_TB, 
                        IS_PINNED = :IS_PINNED, 
                        TRANGTHAI = :TRANGTHAI, 
                        NGAY_HETHAN = :NGAY_HETHAN, 
                        FILE_DINHKEM = :FILE_DINHKEM, 
                        MACTY = :MACTY, 
                        MAPB = :MAPB,
                        TARGET_TYPE = :TARGET_TYPE
                      WHERE ID = :ID",
                    new OracleParameter("TIEUDE", tb.TIEUDE ?? (object)DBNull.Value),
                    new OracleParameter("NOIDUNG", tb.NOIDUNG ?? (object)DBNull.Value),
                    new OracleParameter("NGUOIDANG", tb.NGUOIDANG ?? (object)DBNull.Value),
                    new OracleParameter("NGAYDANG", tb.NGAYDANG),
                    new OracleParameter("LOAI_TB", tb.LOAI_TB ?? (object)DBNull.Value),
                    new OracleParameter("IS_PINNED", tb.IS_PINNED ?? (object)DBNull.Value),
                    new OracleParameter("TRANGTHAI", tb.TRANGTHAI ?? (object)DBNull.Value),
                    new OracleParameter("NGAY_HETHAN", tb.NGAY_HETHAN ?? (object)DBNull.Value),
                    new OracleParameter("FILE_DINHKEM", tb.FILE_DINHKEM ?? (object)DBNull.Value),
                    new OracleParameter("MACTY", tb.MACTY ?? (object)DBNull.Value),
                    new OracleParameter("MAPB", tb.MAPB ?? (object)DBNull.Value),
                    new OracleParameter("TARGET_TYPE", targetType),
                    new OracleParameter("ID", tb.ID)
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật thông báo: " + ex.Message);
            }
        }

        public void Delete(decimal id)
        {
            try
            {
                db.Database.ExecuteSqlCommand("DELETE FROM TB_THONGBAO WHERE ID = :ID", new OracleParameter("ID", id));
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa thông báo: " + ex.Message);
            }
        }
    }
}
