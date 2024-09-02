using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class PHONGBAN
    {
        MyEntities db = new MyEntities();

        public TB_PHONGBAN getItem(int id)
        {
            return db.TB_PHONGBAN.FirstOrDefault(x => x.IDPB == id);
        }
        public List<TB_PHONGBAN> getList()
        {
            return db.TB_PHONGBAN.ToList();
        }

        public TB_PHONGBAN Add(TB_PHONGBAN pb)
        {
            try
            {
                db.TB_PHONGBAN.Add(pb);
                db.SaveChanges();
                return pb;
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

        public TB_PHONGBAN Update(TB_PHONGBAN pb)
        {
            try
            {
                var _pb = db.TB_PHONGBAN.FirstOrDefault(x => x.IDPB == pb.IDPB);
                _pb.TENPB = pb.TENPB;
                db.SaveChanges();
                return pb;
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
                var _pb = db.TB_PHONGBAN.FirstOrDefault(x => x.IDPB == id);
                db.TB_PHONGBAN.Remove(_pb);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }
    }
}
