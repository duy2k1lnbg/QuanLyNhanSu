using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class CHUCVU
    {
        MyEntities db = new MyEntities();

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
