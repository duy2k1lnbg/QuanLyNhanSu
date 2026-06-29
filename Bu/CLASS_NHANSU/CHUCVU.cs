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
    public class CHUCVU
    {
        public static Func<string, string> TranslateDelegate { get; set; }
        MyEntities db = new MyEntities();

        public List<Bu.DTO.ChucVuDTO> getListDTO(string langCode = "vi")
        {
            var list = new List<Bu.DTO.ChucVuDTO>();
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            cv.idcv, 
                            cv.tencv as tencv_vi,
                            COALESCE(t.value, cv.tencv) as tencv,
                            COALESCE(t.description, '') as description
                        FROM TB_CHUCVU cv
                        LEFT JOIN TB_TRANSLATIONS t 
                            ON t.table_name = 'TB_CHUCVU' 
                            AND t.record_id = TO_CHAR(cv.idcv) 
                            AND t.column_name = 'TENCV' 
                            AND LOWER(t.language_code) = :langCode
                        ORDER BY cv.idcv ASC";

                    var pLang = cmd.CreateParameter();
                    pLang.ParameterName = "langCode";
                    pLang.Value = string.IsNullOrEmpty(langCode) ? "vi" : langCode.Trim().ToLower();
                    cmd.Parameters.Add(pLang);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Bu.DTO.ChucVuDTO
                            {
                                IDCV = reader.GetDecimal(0),
                                TENCV_VI = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                TENCV = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                DESCRIPTION = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var rawList = getList();
                list.Clear();
                foreach (var item in rawList)
                {
                    list.Add(new Bu.DTO.ChucVuDTO
                    {
                        IDCV = item.IDCV,
                        TENCV_VI = item.TENCV,
                        TENCV = item.TENCV,
                        DESCRIPTION = string.Empty
                    });
                }
            }
            if (!string.IsNullOrEmpty(langCode) && langCode.ToLower() != "vi" && TranslateDelegate != null)
            {
                foreach (var item in list)
                {
                    if (item.TENCV == item.TENCV_VI)
                    {
                        item.TENCV = TranslateDelegate(item.TENCV);
                    }
                }
            }
            return list;
        }

        public Bu.DTO.ChucVuDTO getItemDTO(int id, string langCode = "vi")
        {
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            cv.idcv, 
                            cv.tencv as tencv_vi,
                            COALESCE(t.value, cv.tencv) as tencv,
                            COALESCE(t.description, '') as description
                        FROM TB_CHUCVU cv
                        LEFT JOIN TB_TRANSLATIONS t 
                            ON t.table_name = 'TB_CHUCVU' 
                            AND t.record_id = TO_CHAR(cv.idcv) 
                            AND t.column_name = 'TENCV' 
                            AND LOWER(t.language_code) = :langCode
                        WHERE cv.idcv = :id";

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
                            return new Bu.DTO.ChucVuDTO
                            {
                                IDCV = reader.GetDecimal(0),
                                TENCV_VI = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                TENCV = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                DESCRIPTION = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
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
                    return new Bu.DTO.ChucVuDTO
                    {
                        IDCV = rawItem.IDCV,
                        TENCV_VI = rawItem.TENCV,
                        TENCV = rawItem.TENCV,
                        DESCRIPTION = string.Empty
                    };
                }
            }
            return null;
        }

        public TB_CHUCVU getItem(int id)
        {
            return db.TB_CHUCVU.FirstOrDefault(x => x.IDCV == id);
        }
        public List<TB_CHUCVU> getList()
        {
            return db.TB_CHUCVU.ToList();
        }

        public TB_CHUCVU Add(TB_CHUCVU cv)
        {
            try
            {
                db.TB_CHUCVU.Add(cv);
                db.SaveChanges();
                return cv;
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

        public TB_CHUCVU Update(TB_CHUCVU tg)
        {
            try
            {
                var _cv = db.TB_CHUCVU.FirstOrDefault(x => x.IDCV == tg.IDCV);
                _cv.TENCV = tg.TENCV;
                db.SaveChanges();
                return tg;
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
                var _cv = db.TB_CHUCVU.FirstOrDefault(x => x.IDCV == id);
                db.TB_CHUCVU.Remove(_cv);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }
    }
}
