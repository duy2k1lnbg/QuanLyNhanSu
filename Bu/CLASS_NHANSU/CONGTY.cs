using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class CONGTY
    {
        MyEntities db = new MyEntities();

        public TB_CONGTY getItem(int id)
        {
            return db.TB_CONGTY.FirstOrDefault(x => x.IDCTY == id);
        }
        public List<TB_CONGTY> getList()
        {
            return db.TB_CONGTY.ToList();
        }

        public TB_CONGTY Add(TB_CONGTY ct)
        {
            try
            {
                db.TB_CONGTY.Add(ct);
                db.SaveChanges();
                return ct;
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

        public TB_CONGTY Update(TB_CONGTY ct)
        {
            try
            {
                var _ct = db.TB_CONGTY.FirstOrDefault(x => x.IDCTY == ct.IDCTY);
                _ct.TENCTY = ct.TENCTY;
                _ct.DIENTHOAICTY = ct.DIENTHOAICTY;
                _ct.EMAILCTY = ct.EMAILCTY;
                _ct.DIACHICTY = ct.DIACHICTY;
                _ct.DAIDIEN = ct.DAIDIEN;
                _ct.MASOTHUECTY = ct.MASOTHUECTY;
                db.SaveChanges();
                return ct;
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
                var _ct = db.TB_CONGTY.FirstOrDefault(x => x.IDCTY == id);
                db.TB_CONGTY.Remove(_ct);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }
    }
}
