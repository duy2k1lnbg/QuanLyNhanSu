using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DA;

namespace Bu
{
    public class TRINHDO
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
                            tb.idtd, 
                            tb.tentd as ten_vi,
                            COALESCE(t.value, tb.tentd) as ten
                        FROM TB_TRINHDO tb
                        LEFT JOIN TB_TRANSLATIONS t 
                            ON t.table_name = 'TB_TRINHDO' 
                            AND t.record_id = TO_CHAR(tb.idtd) 
                            AND t.column_name = 'TENTD' 
                            AND LOWER(t.language_code) = :langCode
                        ORDER BY tb.idtd ASC";

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
                                ID = Convert.ToInt32(reader["idtd"]),
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
                    var idProp = item.GetType().GetProperty("IDTD");
                    var tenProp = item.GetType().GetProperty("TENTD");
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
        }public TB_TRINHDO getItem(int id)
        {
            return db.TB_TRINHDO.FirstOrDefault(x => x.IDTD == id);
        }
        public List<TB_TRINHDO> getList()
        {
            return db.TB_TRINHDO.ToList();
        }

        public TB_TRINHDO Add(TB_TRINHDO td)
        {
            try
            {
                db.TB_TRINHDO.Add(td);
                db.SaveChanges();
                return td;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi" + ex.Message);
            }
        }

        public TB_TRINHDO Update(TB_TRINHDO td)
        {
            try
            {
                var _td = db.TB_TRINHDO.FirstOrDefault(x => x.IDTD == td.IDTD);
                _td.TENTD = td.TENTD;
                db.SaveChanges();
                return td;
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
                var _td = db.TB_TRINHDO.FirstOrDefault(x => x.IDTD == id);
                db.TB_TRINHDO.Remove(_td);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }

    }
}


