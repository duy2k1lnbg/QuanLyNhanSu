using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DA;

namespace Bu
{
    public class DANTOC
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
                            tb.iddt, 
                            tb.tendt as ten_vi,
                            COALESCE(t.value, tb.tendt) as ten
                        FROM TB_DANTOC tb
                        LEFT JOIN TB_TRANSLATIONS t 
                            ON t.table_name = 'TB_DANTOC' 
                            AND t.record_id = TO_CHAR(tb.iddt) 
                            AND t.column_name = 'TENDT' 
                            AND LOWER(t.language_code) = :langCode
                        ORDER BY tb.iddt ASC";

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
                                ID = Convert.ToInt32(reader["iddt"]),
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
                    var idProp = item.GetType().GetProperty("IDDT");
                    var tenProp = item.GetType().GetProperty("TENDT");
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
        }public TB_DANTOC getItem(int id)
        {
            return db.TB_DANTOC.FirstOrDefault(x=>x.IDDT == id);
        }
        public List<TB_DANTOC> getList() 
        { 
            return db.TB_DANTOC.ToList();
        }

        public TB_DANTOC Add(TB_DANTOC dt)
        {
            try
            {
                db.TB_DANTOC.Add(dt);
                db.SaveChanges();
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi" + ex.Message);
            }
        }

        public TB_DANTOC Update(TB_DANTOC dt)
        {
            try
            {
                var _dt = db.TB_DANTOC.FirstOrDefault(x => x.IDDT == dt.IDDT);
                _dt.TENDT = dt.TENDT;
                db.SaveChanges();
                return dt;
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
                var _dt = db.TB_DANTOC.FirstOrDefault(x => x.IDDT == id);
                db.TB_DANTOC.Remove(_dt);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }

    }
}


