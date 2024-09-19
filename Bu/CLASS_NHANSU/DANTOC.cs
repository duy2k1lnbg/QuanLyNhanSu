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

        public TB_DANTOC getItem(int id)
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
