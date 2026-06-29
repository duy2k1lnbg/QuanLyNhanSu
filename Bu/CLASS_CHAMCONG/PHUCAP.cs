using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bu.CLASS_CHAMCONG
{
    public class PHUCAP
    {

        MyEntities db = new MyEntities();
        public TB_NHANVIEN_PHUCAP getItem(int manv, int id)
        {
            return db.TB_NHANVIEN_PHUCAP.FirstOrDefault(x => x.MANV == manv && x.IDPC == id);
        }

        public List<TB_NHANVIEN_PHUCAP> getList()
        {
            return db.TB_NHANVIEN_PHUCAP.ToList();
        }

        public List<NHANVIEN_PHUCAP_DTO> GetNhanVienSortedByIDPC()
        {
            var result = (from np in db.TB_NHANVIEN_PHUCAP
                          join pc in db.TB_PHUCAP on np.IDPC equals pc.IDPC
                          join nv in db.TB_NHANVIEN on np.MANV equals nv.MANV 
                          select new
                          {
                              np.MANV,
                              nv.HOTEN, 
                              np.IDPC,
                              np.SOTIEN,
                              np.MAKYCONG 
                          }).ToList();

            var groupedData = result
                .GroupBy(x => new { x.MANV, x.HOTEN, x.MAKYCONG })
                .Select(g => new NHANVIEN_PHUCAP_DTO
                {
                    MANV = g.Key.MANV,
                    HOTEN = g.Key.HOTEN,
                    MAKYCONG = g.Key.MAKYCONG, 
                    SOTIEN_IDPC1 = g.FirstOrDefault(x => x.IDPC == 1)?.SOTIEN,
                    SOTIEN_IDPC2 = g.FirstOrDefault(x => x.IDPC == 2)?.SOTIEN,
                    SOTIEN_IDPC3 = g.FirstOrDefault(x => x.IDPC == 3)?.SOTIEN,
                    SOTIEN_IDPC4 = g.FirstOrDefault(x => x.IDPC == 4)?.SOTIEN,
                    SOTIEN_IDPC5 = g.FirstOrDefault(x => x.IDPC == 5)?.SOTIEN,
                    SOTIEN_IDPC6 = g.FirstOrDefault(x => x.IDPC == 6)?.SOTIEN,
                    SOTIEN_IDPC7 = g.FirstOrDefault(x => x.IDPC == 7)?.SOTIEN
                })
                .OrderBy(a => a.MANV)
                .ThenBy(a => a.MAKYCONG) 
                .ToList();

            return groupedData;
        }


        //public List<NHANVIEN_PHUCAP_DTO> GetNhanVienSortedByIDPC()
        //{
        //    var result = (from np in db.TB_NHANVIEN_PHUCAP
        //                  join pc in db.TB_PHUCAP on np.IDPC equals pc.IDPC
        //                  join nv in db.TB_NHANVIEN on np.MANV equals nv.MANV // Thêm join để lấy HOTEN
        //                  select new
        //                  {
        //                      np.MANV,
        //                      nv.HOTEN, // Lấy HOTEN từ bảng NHANVIEN
        //                      np.IDPC,
        //                      np.SOTIEN
        //                  }).ToList();

        //    // Gộp dữ liệu theo nhân viên
        //    var groupedData = result
        //        .GroupBy(x => new { x.MANV, x.HOTEN })
        //        .Select(g => new NHANVIEN_PHUCAP_DTO
        //        {
        //            MANV = g.Key.MANV,
        //            HOTEN = g.Key.HOTEN,
        //            SOTIEN_IDPC1 = g.FirstOrDefault(x => x.IDPC == 1)?.SOTIEN,
        //            SOTIEN_IDPC2 = g.FirstOrDefault(x => x.IDPC == 2)?.SOTIEN,
        //            SOTIEN_IDPC3 = g.FirstOrDefault(x => x.IDPC == 3)?.SOTIEN,
        //            SOTIEN_IDPC4 = g.FirstOrDefault(x => x.IDPC == 4)?.SOTIEN,
        //            SOTIEN_IDPC5 = g.FirstOrDefault(x => x.IDPC == 5)?.SOTIEN,
        //            SOTIEN_IDPC6 = g.FirstOrDefault(x => x.IDPC == 6)?.SOTIEN,
        //            SOTIEN_IDPC7 = g.FirstOrDefault(x => x.IDPC == 7)?.SOTIEN
        //        }).OrderBy(a => a.MANV).ToList();

        //    return groupedData;
        //}



        public TB_NHANVIEN_PHUCAP Add(TB_NHANVIEN_PHUCAP pc)
        {
            try
            {
                db.TB_NHANVIEN_PHUCAP.Add(pc);
                db.SaveChanges();
                return pc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
        }

        public TB_NHANVIEN_PHUCAP Update(TB_NHANVIEN_PHUCAP pc)
        {
            try
            {
                var _pc = db.TB_NHANVIEN_PHUCAP.FirstOrDefault(x => x.MANV == pc.MANV && x.IDPC == pc.IDPC && x.MAKYCONG == pc.MAKYCONG && x.MAKYCONG == pc.MAKYCONG);
                _pc.SOTIEN = pc.SOTIEN;
                _pc.UPDATED_BY = pc.UPDATED_BY;
                _pc.UPDATED_DATE = pc.UPDATED_DATE;

                db.SaveChanges();
                return pc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
        }

        public void Delete(int manv, int id, int iduser)
        {
            var _lc = db.TB_NHANVIEN_PHUCAP.FirstOrDefault(x => x.MANV == manv && x.IDPC ==id);
            _lc.DELETED_BY = iduser;
            _lc.DELETED_DATE = DateTime.Now;

            db.SaveChanges();
        }

        public TB_PHUCAP getItemPC(int id)
        {
            return db.TB_PHUCAP.FirstOrDefault(x=>x.IDPC == id);
        }

        public List<Bu.DTO.PhuCapDTO> getListPC_DTO(string langCode = "vi")
        {
            var list = new List<Bu.DTO.PhuCapDTO>();
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            pc.idpc, 
                            pc.tenpc as tenpc_vi,
                            COALESCE(t.value, pc.tenpc) as tenpc,
                            COALESCE(t.description, '') as description
                        FROM TB_PHUCAP pc
                        LEFT JOIN TB_TRANSLATIONS t 
                            ON t.table_name = 'TB_PHUCAP' 
                            AND t.record_id = TO_CHAR(pc.idpc) 
                            AND t.column_name = 'TENPC' 
                            AND LOWER(t.language_code) = :langCode
                        ORDER BY pc.idpc ASC";

                    var pLang = cmd.CreateParameter();
                    pLang.ParameterName = "langCode";
                    pLang.Value = string.IsNullOrEmpty(langCode) ? "vi" : langCode.Trim().ToLower();
                    cmd.Parameters.Add(pLang);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Bu.DTO.PhuCapDTO
                            {
                                IDPC = reader.GetDecimal(0),
                                TENPC_VI = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                TENPC = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                DESCRIPTION = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var rawList = db.TB_PHUCAP.ToList();
                list.Clear();
                foreach (var item in rawList)
                {
                    list.Add(new Bu.DTO.PhuCapDTO
                    {
                        IDPC = item.IDPC,
                        TENPC_VI = item.TENPC,
                        TENPC = item.TENPC,
                        DESCRIPTION = string.Empty
                    });
                }
            }
            return list;
        }

        public Bu.DTO.PhuCapDTO getItemPC_DTO(int id, string langCode = "vi")
        {
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            pc.idpc, 
                            pc.tenpc as tenpc_vi,
                            COALESCE(t.value, pc.tenpc) as tenpc,
                            COALESCE(t.description, '') as description
                        FROM TB_PHUCAP pc
                        LEFT JOIN TB_TRANSLATIONS t 
                            ON t.table_name = 'TB_PHUCAP' 
                            AND t.record_id = TO_CHAR(pc.idpc) 
                            AND t.column_name = 'TENPC' 
                            AND LOWER(t.language_code) = :langCode
                        WHERE pc.idpc = :id";

                    var pLang = cmd.CreateParameter();
                    pLang.ParameterName = "langCode";
                    pLang.Value = string.IsNullOrEmpty(langCode) ? "vi" : langCode.Trim().ToLower();
                    cmd.Parameters.Add(pLang);

                    var pId = cmd.CreateParameter();
                    pId.ParameterName = "id";
                    pId.Value = id;
                    cmd.Parameters.Add(pId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Bu.DTO.PhuCapDTO
                            {
                                IDPC = reader.GetDecimal(0),
                                TENPC_VI = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                TENPC = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                DESCRIPTION = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var rawItem = getItemPC(id);
                if (rawItem != null)
                {
                    return new Bu.DTO.PhuCapDTO
                    {
                        IDPC = rawItem.IDPC,
                        TENPC_VI = rawItem.TENPC,
                        TENPC = rawItem.TENPC,
                        DESCRIPTION = string.Empty
                    };
                }
            }
            return null;
        }

        //public void UpdatePhucap(int manv, int idpc, decimal sotien)
        //{
        //    var phucap = db.TB_NHANVIEN_PHUCAP.FirstOrDefault(np => np.MANV == manv && np.IDPC == idpc);

        //    if (phucap != null)
        //    {
        //        phucap.SOTIEN = sotien;
        //    }
        //    else
        //    {
        //        db.TB_NHANVIEN_PHUCAP.Add(new TB_NHANVIEN_PHUCAP
        //        {
        //            MANV = manv,
        //            IDPC = idpc,
        //            SOTIEN = sotien,
        //            GHICHU = ""
        //        });
        //    }
        //    db.SaveChanges();
        //}

        public void UpdatePhucap(int manv, int idpc, decimal sotien, int makycong)
        {
            var phucap = db.TB_NHANVIEN_PHUCAP.FirstOrDefault(np => np.MANV == manv && np.IDPC == idpc && np.MAKYCONG == makycong);

            if (phucap != null)
            {
                phucap.SOTIEN = sotien;
            }
            else
            {
                db.TB_NHANVIEN_PHUCAP.Add(new TB_NHANVIEN_PHUCAP
                {
                    MANV = manv,
                    IDPC = idpc,
                    MAKYCONG = makycong, 
                    SOTIEN = sotien,
                    GHICHU = ""
                });
            }
            db.SaveChanges();
        }
    }
}
