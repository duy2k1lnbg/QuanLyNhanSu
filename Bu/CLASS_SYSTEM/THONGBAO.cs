using DA;
using Bu.DTO;
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

        public List<THONGBAO_DTO> getListFull_DTO()
        {
            try
            {
                var lstTB = db.Database.SqlQuery<TB_THONGBAO>(
                    "SELECT ID, TIEUDE, NOIDUNG, NGUOIDANG, NGAYDANG, LOAI_TB, IS_PINNED, TRANGTHAI, NGAY_HETHAN, FILE_DINHKEM, MACTY, MAPB FROM TB_THONGBAO ORDER BY IS_PINNED DESC, NGAYDANG DESC"
                ).ToList();

                var lstCT = db.TB_CONGTY.ToList();
                var lstPB = db.TB_PHONGBAN.ToList();

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
                        IS_PINNED = item.IS_PINNED,
                        TRANGTHAI = item.TRANGTHAI,
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
                db.Database.ExecuteSqlCommand(
                    @"INSERT INTO TB_THONGBAO (TIEUDE, NOIDUNG, NGUOIDANG, NGAYDANG, LOAI_TB, IS_PINNED, TRANGTHAI, NGAY_HETHAN, FILE_DINHKEM, MACTY, MAPB) 
                      VALUES (:TIEUDE, :NOIDUNG, :NGUOIDANG, :NGAYDANG, :LOAI_TB, :IS_PINNED, :TRANGTHAI, :NGAY_HETHAN, :FILE_DINHKEM, :MACTY, :MAPB)",
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
                    new OracleParameter("MAPB", tb.MAPB ?? (object)DBNull.Value)
                );
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
                        MAPB = :MAPB 
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
