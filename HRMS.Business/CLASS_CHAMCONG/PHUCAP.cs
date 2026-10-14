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
using Bu.CLASS_SYSTEM;
using Bu.CLASS_PAYROLL;
using Oracle.ManagedDataAccess.Client;

namespace Bu.CLASS_CHAMCONG
{
    public class PHUCAP
    {

        MyEntities db = new MyEntities();
        private MyEntities Context => db ?? (db = new MyEntities());

        public TB_NHANVIEN_PHUCAP getItem(int manv, int id)
        {
            using (var dbLocal = new MyEntities())
            {
                return dbLocal.TB_NHANVIEN_PHUCAP.AsNoTracking().FirstOrDefault(x => x.MANV == manv && x.IDPC == id);
            }
        }

        public List<TB_NHANVIEN_PHUCAP> getList()
        {
            using (var dbLocal = new MyEntities())
            {
                return dbLocal.TB_NHANVIEN_PHUCAP.AsNoTracking().ToList();
            }
        }

        public List<NHANVIEN_PHUCAP_DTO> GetNhanVienSortedByIDPC()
        {
            using (var dbLocal = new MyEntities())
            {
                var result = (from np in dbLocal.TB_NHANVIEN_PHUCAP.AsNoTracking()
                              join pc in dbLocal.TB_PHUCAP.AsNoTracking() on np.IDPC equals pc.IDPC
                              join nv in dbLocal.TB_NHANVIEN.AsNoTracking() on np.MANV equals nv.MANV 
                              select new
                              {
                                  np.MANV,
                                  nv.HOTEN, 
                                  np.IDPC,
                                  np.SOTIEN
                              }).ToList();

                var groupedData = result
                    .GroupBy(x => new { x.MANV, x.HOTEN })
                    .Select(g => new NHANVIEN_PHUCAP_DTO
                    {
                        MANV = g.Key.MANV,
                        HOTEN = g.Key.HOTEN,
                        MAKYCONG = 0, 
                        SOTIEN_IDPC1 = g.FirstOrDefault(x => x.IDPC == 1)?.SOTIEN,
                        SOTIEN_IDPC2 = g.FirstOrDefault(x => x.IDPC == 2)?.SOTIEN,
                        SOTIEN_IDPC3 = g.FirstOrDefault(x => x.IDPC == 3)?.SOTIEN,
                        SOTIEN_IDPC4 = g.FirstOrDefault(x => x.IDPC == 4)?.SOTIEN,
                        SOTIEN_IDPC5 = g.FirstOrDefault(x => x.IDPC == 5)?.SOTIEN,
                        SOTIEN_IDPC6 = g.FirstOrDefault(x => x.IDPC == 6)?.SOTIEN,
                        SOTIEN_IDPC7 = g.FirstOrDefault(x => x.IDPC == 7)?.SOTIEN,
                        SOTIEN_IDPC8 = g.FirstOrDefault(x => x.IDPC == 8)?.SOTIEN,
                        SOTIEN_IDPC9 = g.FirstOrDefault(x => x.IDPC == 9)?.SOTIEN,
                        SOTIEN_IDPC10 = g.FirstOrDefault(x => x.IDPC == 10)?.SOTIEN,
                        SOTIEN_IDPC11 = g.FirstOrDefault(x => x.IDPC == 11)?.SOTIEN,
                        SOTIEN_IDPC12 = g.FirstOrDefault(x => x.IDPC == 12)?.SOTIEN,
                        SOTIEN_IDPC13 = g.FirstOrDefault(x => x.IDPC == 13)?.SOTIEN
                    })
                    .OrderBy(a => a.MANV)
                    .ToList();

                return groupedData;
            }
        }

        public TB_NHANVIEN_PHUCAP Add(TB_NHANVIEN_PHUCAP pc)
        {
            try
            {
                using (var dbLocal = new MyEntities())
                {
                    dbLocal.TB_NHANVIEN_PHUCAP.Add(pc);
                    dbLocal.SaveChanges();
                    return pc;
                }
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
                using (var dbLocal = new MyEntities())
                {
                    var _pc = dbLocal.TB_NHANVIEN_PHUCAP.FirstOrDefault(x => x.MANV == pc.MANV && x.IDPC == pc.IDPC);
                    if (_pc != null)
                    {
                        _pc.SOTIEN = pc.SOTIEN;
                        _pc.UPDATED_BY = pc.UPDATED_BY;
                        _pc.UPDATED_DATE = pc.UPDATED_DATE;
                        dbLocal.SaveChanges();
                    }
                    return pc;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi Update data " + ex.Message);
            }
        }

        public void Delete(int manv, int id, int iduser)
        {
            using (var dbLocal = new MyEntities())
            {
                var _lc = dbLocal.TB_NHANVIEN_PHUCAP.FirstOrDefault(x => x.MANV == manv && x.IDPC == id);
                if (_lc != null)
                {
                    _lc.DELETED_BY = iduser;
                    _lc.DELETED_DATE = DateTime.Now;
                    dbLocal.SaveChanges();
                }
            }
        }

        public TB_PHUCAP getItemPC(int id)
        {
            using (var dbLocal = new MyEntities())
            {
                return dbLocal.TB_PHUCAP.AsNoTracking().FirstOrDefault(x => x.IDPC == id);
            }
        }

        public List<Bu.DTO.PhuCapDTO> getListPC_DTO(string langCode = "vi")
        {
            var list = new List<Bu.DTO.PhuCapDTO>();
            try
            {
                using (var dbLocal = new MyEntities())
                {
                    var conn = dbLocal.Database.Connection;
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
            }
            catch (Exception)
            {
                using (var dbFallback = new MyEntities())
                {
                    var rawList = dbFallback.TB_PHUCAP.AsNoTracking().ToList();
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
            }
            return list;
        }

        public Bu.DTO.PhuCapDTO getItemPC_DTO(int id, string langCode = "vi")
        {
            try
            {
                using (var dbLocal = new MyEntities())
                {
                    var conn = dbLocal.Database.Connection;
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
            }
            catch (Exception)
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

        private class PhuCapRow
        {
            public decimal IDPC { get; set; }
            public decimal? SOTIEN { get; set; }
        }

        public Dictionary<int, decimal> GetPhuCapByNhanVien(int manv)
        {
            var dict = new Dictionary<int, decimal>();
            for (int i = 1; i <= 13; i++)
            {
                dict[i] = 0m;
            }

            try
            {
                using (var dbLocal = new MyEntities())
                {
                    var list = dbLocal.TB_NHANVIEN_PHUCAP
                                      .AsNoTracking()
                                      .Where(x => x.MANV == manv)
                                      .ToList();

                    foreach (var item in list)
                    {
                        int idpc = Convert.ToInt32(item.IDPC);
                        if (idpc >= 1 && idpc <= 13)
                        {
                            dict[idpc] = item.SOTIEN ?? 0m;
                        }
                    }
                }
            }
            catch (Exception)
            {
                try
                {
                    using (var dbFallback = new MyEntities())
                    {
                        var rows = dbFallback.Database.SqlQuery<PhuCapRow>(
                            "SELECT IDPC, SOTIEN FROM TB_NHANVIEN_PHUCAP WHERE MANV = :p0",
                            new OracleParameter("p0", (decimal)manv)).ToList();
                        foreach (var r in rows)
                        {
                            int idpc = Convert.ToInt32(r.IDPC);
                            if (idpc >= 1 && idpc <= 13)
                            {
                                dict[idpc] = r.SOTIEN ?? 0m;
                            }
                        }
                    }
                }
                catch
                {
                    // return default initialized zeros
                }
            }
            return dict;
        }

        public void SavePhuCapHopDong(int manv, Dictionary<int, decimal> allowances, int iduser = 1, DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            if (allowances == null) return;

            using (var dbLocal = new MyEntities())
            {
                foreach (var kvp in allowances)
                {
                    int idpc = kvp.Key;
                    decimal sotien = kvp.Value;

                    var existing = dbLocal.TB_NHANVIEN_PHUCAP.FirstOrDefault(x => x.MANV == manv && x.IDPC == idpc);
                    if (existing != null)
                    {
                        existing.SOTIEN = sotien;
                        existing.UPDATED_BY = iduser;
                        existing.UPDATED_DATE = DateTime.Now;
                        if (tuNgay.HasValue) existing.TU_NGAY = tuNgay.Value;
                        if (denNgay.HasValue) existing.DEN_NGAY = denNgay;
                    }
                    else
                    {
                        dbLocal.TB_NHANVIEN_PHUCAP.Add(new TB_NHANVIEN_PHUCAP
                        {
                            MANV = manv,
                            IDPC = idpc,
                            SOTIEN = sotien,
                            GHICHU = "Phụ cấp theo hợp đồng",
                            CREATED_BY = iduser,
                            CREATED_DATE = DateTime.Now,
                            TU_NGAY = tuNgay,
                            DEN_NGAY = denNgay,
                            CACH_TINH = "CO_DINH_THANG"
                        });
                    }
                }
                dbLocal.SaveChanges();
            }
        }

        public void UpdatePhucap(int manv, int idpc, decimal sotien, int makycong = 0)
        {
            using (var dbLocal = new MyEntities())
            {
                var phucap = dbLocal.TB_NHANVIEN_PHUCAP.FirstOrDefault(np => np.MANV == manv && np.IDPC == idpc);

                if (phucap != null)
                {
                    phucap.SOTIEN = sotien;
                    phucap.UPDATED_DATE = DateTime.Now;
                }
                else
                {
                    dbLocal.TB_NHANVIEN_PHUCAP.Add(new TB_NHANVIEN_PHUCAP
                    {
                        MANV = manv,
                        IDPC = idpc,
                        SOTIEN = sotien,
                        GHICHU = "Phụ cấp theo hợp đồng",
                        CREATED_DATE = DateTime.Now
                    });
                }
                dbLocal.SaveChanges();
            }
        }

        public void UpdateCatalogItem(int idpc, string newTenPc, int userId)
        {
            if (UserSession.CurrentUser == null || UserSession.CurrentUser.IDUSER <= 0)
                throw new BusinessException("UNAUTHENTICATED", "Người dùng chưa đăng nhập hoặc phiên làm việc đã hết hạn.");

            if (!UserSession.IsAdmin && !UserSession.CanEdit("F_CC_BANGLUONG"))
                throw new BusinessException("PERMISSION_DENIED", "Bạn không có quyền chỉnh sửa danh mục phụ cấp lương.");

            if (string.IsNullOrWhiteSpace(newTenPc))
                throw new BusinessException("VALIDATION_ERROR", "Tên phụ cấp không được để trống.", "TENPC");

            using (var dbLocal = new MyEntities())
            using (var tx = dbLocal.Database.BeginTransaction())
            {
                try
                {
                    var pc = dbLocal.TB_PHUCAP.FirstOrDefault(x => x.IDPC == idpc);
                    if (pc == null)
                        throw new BusinessException("NOT_FOUND", $"Không tìm thấy phụ cấp mã [{idpc}].");

                    string oldName = pc.TENPC;
                    pc.TENPC = newTenPc.Trim();
                    dbLocal.SaveChanges();

                    string auditUser = UserSession.CurrentUser.FULLNAME ?? ("User " + userId);
                    dbLocal.Database.ExecuteSqlCommand(@"
                        INSERT INTO TB_SYS_LOG (
                            MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI,
                            DU_LIEU_CU, DU_LIEU_MOI, THOIGIAN, MODULE_NAME, CHANGED_FIELDS
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4,
                            :p5, :p6, CURRENT_TIMESTAMP, :p7, :p8
                        )",
                        new OracleParameter("p0", userId),
                        new OracleParameter("p1", auditUser),
                        new OracleParameter("p2", "CAP_NHAT_PHUCAP"),
                        new OracleParameter("p3", "TB_PHUCAP"),
                        new OracleParameter("p4", idpc.ToString()),
                        new OracleParameter("p5", oldName ?? ""),
                        new OracleParameter("p6", pc.TENPC),
                        new OracleParameter("p7", "CAUHINH_LUONG"),
                        new OracleParameter("p8", $"TENPC: {oldName} -> {pc.TENPC}")
                    );

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}
