using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class GIOITINH
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
                            tb.idgt, 
                            tb.tengt as ten_vi,
                            COALESCE(t.value, tb.tengt) as ten
                        FROM TB_GIOITINH tb
                        LEFT JOIN TB_TRANSLATIONS t 
                            ON t.table_name = 'TB_GIOITINH' 
                            AND t.record_id = TO_CHAR(tb.idgt) 
                            AND t.column_name = 'TENGT' 
                            AND LOWER(t.language_code) = :langCode
                        ORDER BY tb.idgt ASC";

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
                                ID = Convert.ToInt32(reader["idgt"]),
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
                    var idProp = item.GetType().GetProperty("IDGT");
                    var tenProp = item.GetType().GetProperty("TENGT");
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
        }public TB_GIOITINH getItem(int id)
        {
            return db.TB_GIOITINH.FirstOrDefault(x => x.IDGT == id);
        }
        public List<TB_GIOITINH> getList()
        {
            return db.TB_GIOITINH.ToList();
        }

        public TB_GIOITINH Add(TB_GIOITINH gt)
        {
            try
            {
                db.TB_GIOITINH.Add(gt);
                db.SaveChanges();
                return gt;
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

        public TB_GIOITINH Update(TB_GIOITINH gt)
        {
            try
            {
                var _gt = db.TB_GIOITINH.FirstOrDefault(x => x.IDGT == gt.IDGT);
                _gt.TENGT = gt.TENGT;
                db.SaveChanges();
                return gt;
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
                var _gt = db.TB_GIOITINH.FirstOrDefault(x => x.IDGT == id);
                db.TB_GIOITINH.Remove(_gt);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }
    }
}


