using DA;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class PHONGBAN
    {
        public static Func<string, string> TranslateDelegate { get; set; }
        MyEntities db = new MyEntities();

        public List<Bu.DTO.PhongBanDTO> getListDTO(string langCode = "vi")
        {
            var list = new List<Bu.DTO.PhongBanDTO>();
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            pb.idpb, 
                            pb.tenpb as tenpb_vi,
                            COALESCE(t.value, pb.tenpb) as tenpb,
                            COALESCE(t.description, '') as description
                        FROM TB_PHONGBAN pb
                        LEFT JOIN TB_TRANSLATIONS t 
                            ON t.table_name = 'TB_PHONGBAN' 
                            AND t.record_id = TO_CHAR(pb.idpb) 
                            AND t.column_name = 'TENPB' 
                            AND LOWER(t.language_code) = :langCode
                        ORDER BY pb.idpb ASC";

                    var pLang = cmd.CreateParameter();
                    pLang.ParameterName = "langCode";
                    pLang.Value = string.IsNullOrEmpty(langCode) ? "vi" : langCode.Trim().ToLower();
                    cmd.Parameters.Add(pLang);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Bu.DTO.PhongBanDTO
                            {
                                IDPB = reader.GetDecimal(0),
                                TENPB_VI = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                TENPB = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                DESCRIPTION = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Dự phòng nếu lỗi kết nối / DB chưa có bảng dịch: trả về dữ liệu thô tiếng Việt
                var rawList = getList();
                list.Clear();
                foreach (var item in rawList)
                {
                    list.Add(new Bu.DTO.PhongBanDTO
                    {
                        IDPB = item.IDPB,
                        TENPB_VI = item.TENPB,
                        TENPB = item.TENPB,
                        DESCRIPTION = string.Empty
                    });
                }
            }
            if (!string.IsNullOrEmpty(langCode) && langCode.ToLower() != "vi" && TranslateDelegate != null)
            {
                foreach (var item in list)
                {
                    if (item.TENPB == item.TENPB_VI)
                    {
                        item.TENPB = TranslateDelegate(item.TENPB);
                    }
                }
            }
            return list;
        }

        public Bu.DTO.PhongBanDTO getItemDTO(int id, string langCode = "vi")
        {
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            pb.idpb, 
                            pb.tenpb as tenpb_vi,
                            COALESCE(t.value, pb.tenpb) as tenpb,
                            COALESCE(t.description, '') as description
                        FROM TB_PHONGBAN pb
                        LEFT JOIN TB_TRANSLATIONS t 
                            ON t.table_name = 'TB_PHONGBAN' 
                            AND t.record_id = TO_CHAR(pb.idpb) 
                            AND t.column_name = 'TENPB' 
                            AND LOWER(t.language_code) = :langCode
                        WHERE pb.idpb = :id";

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
                            return new Bu.DTO.PhongBanDTO
                            {
                                IDPB = Convert.ToInt32(reader["idpb"]),
                                TENPB_VI = reader["tenpb_vi"].ToString(),
                                TENPB = reader["tenpb"].ToString(),
                                DESCRIPTION = reader["description"].ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var rawItem = getItem(id);
                if (rawItem != null)
                {
                    return new Bu.DTO.PhongBanDTO
                    {
                        IDPB = rawItem.IDPB,
                        TENPB_VI = rawItem.TENPB,
                        TENPB = rawItem.TENPB,
                        DESCRIPTION = string.Empty
                    };
                }
            }
            return null;
        }

        public TB_PHONGBAN getItem(int id)
        {
            return db.TB_PHONGBAN.FirstOrDefault(x => x.IDPB == id);
        }
        public List<TB_PHONGBAN> getList()
        {
            return db.TB_PHONGBAN.ToList();
        }

        public TB_PHONGBAN Add(TB_PHONGBAN pb)
        {
            try
            {
                db.TB_PHONGBAN.Add(pb);
                db.SaveChanges();
                return pb;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);
                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);
                throw new Exception("Lỗi: " + exceptionMessage);
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? string.Empty;
                throw new Exception("Lỗi: " + ex.Message + " Inner Exception: " + innerExceptionMessage);
            }
        }

        public TB_PHONGBAN Update(TB_PHONGBAN pb)
        {
            try
            {
                var _pb = db.TB_PHONGBAN.FirstOrDefault(x => x.IDPB == pb.IDPB);
                _pb.TENPB = pb.TENPB;
                db.SaveChanges();
                return pb;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }

        public void Delete(int id)
        {
            try
            {
                var _pb = db.TB_PHONGBAN.FirstOrDefault(x => x.IDPB == id);
                db.TB_PHONGBAN.Remove(_pb);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }
    }
}
