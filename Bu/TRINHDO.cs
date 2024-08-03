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

        public TB_TRINHDO getItem(int id)
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
