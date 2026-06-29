using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class QUOCTICH
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
                            tb.idqt, 
                            tb.tenqt as ten_vi,
                            COALESCE(t.value, tb.tenqt) as ten
                        FROM TB_QUOCTICH tb
                        LEFT JOIN TB_TRANSLATIONS t 
                            ON t.table_name = 'TB_QUOCTICH' 
                            AND t.record_id = TO_CHAR(tb.idqt) 
                            AND t.column_name = 'TENQT' 
                            AND LOWER(t.language_code) = :langCode
                        ORDER BY tb.idqt ASC";

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
                                ID = Convert.ToInt32(reader["idqt"]),
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
                    var idProp = item.GetType().GetProperty("IDQT");
                    var tenProp = item.GetType().GetProperty("TENQT");
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
        }public TB_QUOCTICH getItem(int id)
        {
            return db.TB_QUOCTICH.FirstOrDefault(x => x.IDQT == id);
        }
        public List<TB_QUOCTICH> getList()
        {
            return db.TB_QUOCTICH.ToList();
        }

        public TB_QUOCTICH Add(TB_QUOCTICH qt)
        {
            try
            {
                db.TB_QUOCTICH.Add(qt);
                db.SaveChanges();
                return qt;
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

        public TB_QUOCTICH Update(TB_QUOCTICH qt)
        {
            try
            {
                var _qt = db.TB_QUOCTICH.FirstOrDefault(x => x.IDQT == qt.IDQT);
                _qt.TENQT = qt.TENQT;
                db.SaveChanges();
                return qt;
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
                var _qt = db.TB_QUOCTICH.FirstOrDefault(x => x.IDQT == id);
                db.TB_QUOCTICH.Remove(_qt);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }
    }
}


