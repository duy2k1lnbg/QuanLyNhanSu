using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class QUOCTICH
    {
        MyEntities db = new MyEntities();

        public TB_QUOCTICH getItem(int id)
        {
            return db.TB_QUOCTICH.FirstOrDefault(x => x.IDQT == id);
        }
        public List<TB_QUOCTICH> getList()
        {
            return db.TB_QUOCTICH.ToList();
        }

        public TB_QUOCTICH Add(TB_QUOCTICH qt)
        {
            try
            {
                db.TB_QUOCTICH.Add(qt);
                db.SaveChanges();
                return qt;
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

        public TB_QUOCTICH Update(TB_QUOCTICH qt)
        {
            try
            {
                var _qt = db.TB_QUOCTICH.FirstOrDefault(x => x.IDQT == qt.IDQT);
                _qt.TENQT = qt.TENQT;
                db.SaveChanges();
                return qt;
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
                var _qt = db.TB_QUOCTICH.FirstOrDefault(x => x.IDQT == id);
                db.TB_QUOCTICH.Remove(_qt);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }
    }
}
