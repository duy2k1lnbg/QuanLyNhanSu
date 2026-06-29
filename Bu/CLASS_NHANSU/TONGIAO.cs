using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DA;

namespace Bu
{
    public class TONGIAO
    {
        MyEntities db = new MyEntities();

        
        public static Func<string, string> TranslateDelegate { get; set; }

        public List<Bu.DTO.ComboDTO> getListDTO(string langCode = "vi")
        {
            var list = new List<Bu.DTO.ComboDTO>();
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT 
                            tb.idtg, 
                            tb.tentg as ten_vi,
                            COALESCE(t.value, tb.tentg) as ten
                        FROM TB_TONGIAO tb
                        LEFT JOIN TB_TRANSLATIONS t 
                            ON t.table_name = 'TB_TONGIAO' 
                            AND t.record_id = TO_CHAR(tb.idtg) 
                            AND t.column_name = 'TENTG' 
                            AND LOWER(t.language_code) = :langCode
                        ORDER BY tb.idtg ASC";

                    var pLang = cmd.CreateParameter();
                    pLang.ParameterName = "langCode";
                    pLang.Value = string.IsNullOrEmpty(langCode) ? "vi" : langCode.Trim().ToLower();
                    cmd.Parameters.Add(pLang);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Bu.DTO.ComboDTO
                            {
                                ID = Convert.ToInt32(reader["idtg"]),
                                TEN_VI = reader["ten_vi"].ToString(),
                                TEN = reader["ten"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                var efList = getList();
                foreach (var item in efList)
                {
                    var idProp = item.GetType().GetProperty("IDTG");
                    var tenProp = item.GetType().GetProperty("TENTG");
                    list.Add(new Bu.DTO.ComboDTO 
                    { 
                        ID = Convert.ToInt32(idProp.GetValue(item)), 
                        TEN_VI = tenProp.GetValue(item)?.ToString(), 
                        TEN = tenProp.GetValue(item)?.ToString() 
                    });
                }
            }
            
            if (!string.IsNullOrEmpty(langCode) && langCode.ToLower() != "vi" && TranslateDelegate != null)
            {
                foreach (var item in list)
                {
                    if (item.TEN == item.TEN_VI)
                    {
                        item.TEN = TranslateDelegate(item.TEN);
                    }
                }
            }
            return list;
        }public TB_TONGIAO getItem(int id)
        {
            return db.TB_TONGIAO.FirstOrDefault(x => x.IDTG == id);
        }
        public List<TB_TONGIAO> getList()
        {
            return db.TB_TONGIAO.ToList();
        }

        public TB_TONGIAO Add(TB_TONGIAO tg)
        {
            try
            {
                db.TB_TONGIAO.Add(tg);
                db.SaveChanges();
                return tg;
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

        //public TB_TONGIAO Add(TB_TONGIAO tg)
        //{
        //    try
        //    {
        //        db.TB_TONGIAO.Add(tg);
        //        db.SaveChanges();
        //        return tg;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Lỗi" + ex.Message);
        //    }
        //}

        public TB_TONGIAO Update(TB_TONGIAO tg)
        {
            try
            {
                var _tg = db.TB_TONGIAO.FirstOrDefault(x => x.IDTG == tg.IDTG);
                _tg.TENTG = tg.TENTG;
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
                var _tg = db.TB_TONGIAO.FirstOrDefault(x => x.IDTG == id);
                db.TB_TONGIAO.Remove(_tg);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }

    }
}


