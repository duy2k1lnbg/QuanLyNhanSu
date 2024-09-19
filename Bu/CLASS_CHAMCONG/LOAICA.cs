using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_CHAMCONG
{
    public class LOAICA
    {
        MyEntities db = new MyEntities();

        public TB_LOAICA getItem(int idloaica )
        {
            return db.TB_LOAICA.FirstOrDefault(x => x.IDLOAICA == idloaica);
        }

        public List<TB_LOAICA> getList()
        {
            return db.TB_LOAICA.ToList();
        }

        public TB_LOAICA Add(TB_LOAICA lc)
        {
            try
            {
                db.TB_LOAICA.Add(lc);
                db.SaveChanges();
                return lc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
        }

        public TB_LOAICA Update(TB_LOAICA lc)
        {
            try
            {
                var _lc = db.TB_LOAICA.FirstOrDefault(x => x.IDLOAICA == lc.IDLOAICA);
                _lc.TENLOAICA = lc.TENLOAICA;
                _lc.HESO = lc.HESO;
                _lc.UPDATED_BY = lc.UPDATED_BY;
                _lc.UPDATED_DATE = lc.UPDATED_DATE;

                db.SaveChanges();
                return lc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
        }

        public void Delete(int idloaica, int iduser)
        {
            var _lc = db.TB_LOAICA.FirstOrDefault(x => x.IDLOAICA == idloaica);
            _lc.DELETED_BY = iduser;
            _lc.DELETED_DATE = DateTime.Now;

            db.SaveChanges();
        }
    }
}
