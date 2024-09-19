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

        public TB_TONGIAO getItem(int id)
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
