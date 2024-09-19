using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_CHAMCONG
{
    public class LOAICONG
    {
        MyEntities db = new MyEntities();

        public TB_LOAICONG getItem(int idloaicong)
        {
            return db.TB_LOAICONG.FirstOrDefault(x => x.IDLOAICONG == idloaicong);
        }

        public List<TB_LOAICONG> getList()
        {
            return db.TB_LOAICONG.ToList();
        }

        public TB_LOAICONG Add(TB_LOAICONG lc)
        {
            try
            {
                db.TB_LOAICONG.Add(lc);
                db.SaveChanges();
                return lc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
        }

        public TB_LOAICONG Update(TB_LOAICONG lc)
        {
            try
            {
                var _lc = db.TB_LOAICONG.FirstOrDefault(x => x.IDLOAICONG == lc.IDLOAICONG);
                _lc.TENLC = lc.TENLC;
                _lc.HESOLOAICONG = lc.HESOLOAICONG;
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

        public void Delete(int idloaicong, int iduser)
        {
            var _lc = db.TB_LOAICONG.FirstOrDefault(x => x.IDLOAICONG == idloaicong);
            _lc.DELETED_BY = iduser;
            _lc.DELETED_DATE = DateTime.Now;

            db.SaveChanges();
        }
    }
}
