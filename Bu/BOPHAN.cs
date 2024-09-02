using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class BOPHAN
    {
        MyEntities db = new MyEntities();

        public TB_BOPHAN getItem(int id)
        {
            return db.TB_BOPHAN.FirstOrDefault(x => x.IDBP == id);
        }
        public List<TB_BOPHAN> getList()
        {
            return db.TB_BOPHAN.ToList();
        }

        public TB_BOPHAN Add(TB_BOPHAN bp)
        {
            try
            {
                db.TB_BOPHAN.Add(bp);
                db.SaveChanges();
                return bp;
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

        public TB_BOPHAN Update(TB_BOPHAN bp)
        {
            try
            {
                var _bp = db.TB_BOPHAN.FirstOrDefault(x => x.IDBP == bp.IDBP);
                _bp.TENBP = bp.TENBP;
                db.SaveChanges();
                return bp;
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
                var _bp = db.TB_BOPHAN.FirstOrDefault(x => x.IDBP == id);
                db.TB_BOPHAN.Remove(_bp);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }

    }
}
