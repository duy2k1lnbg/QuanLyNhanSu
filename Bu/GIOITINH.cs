using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class GIOITINH
    {
        MyEntities db = new MyEntities();

        public TB_GIOITINH getItem(int id)
        {
            return db.TB_GIOITINH.FirstOrDefault(x => x.IDGT == id);
        }
        public List<TB_GIOITINH> getList()
        {
            return db.TB_GIOITINH.ToList();
        }

        public TB_GIOITINH Add(TB_GIOITINH gt)
        {
            try
            {
                db.TB_GIOITINH.Add(gt);
                db.SaveChanges();
                return gt;
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

        public TB_GIOITINH Update(TB_GIOITINH gt)
        {
            try
            {
                var _gt = db.TB_GIOITINH.FirstOrDefault(x => x.IDGT == gt.IDGT);
                _gt.TENGT = gt.TENGT;
                db.SaveChanges();
                return gt;
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
                var _gt = db.TB_GIOITINH.FirstOrDefault(x => x.IDGT == id);
                db.TB_GIOITINH.Remove(_gt);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }
    }
}
