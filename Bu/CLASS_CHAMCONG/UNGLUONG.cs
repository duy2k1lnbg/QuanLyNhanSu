using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_CHAMCONG
{
    public class UNGLUONG
    {
        MyEntities db = new MyEntities();

        public TB_UNGLUONG getItem(int id)
        {
            return db.TB_UNGLUONG.FirstOrDefault(x => x.IDUL == id);
        }

        public List<TB_UNGLUONG> getList()
        {
            return db.TB_UNGLUONG.ToList();
        }

        public List<UNGLUONG_DTO> getListFull()
        {
            var query = from ul in db.TB_UNGLUONG
                        join nv in db.TB_NHANVIEN on ul.MANV equals nv.MANV into nvGroup
                        from nv in nvGroup.DefaultIfEmpty()
                        select new UNGLUONG_DTO
                        {
                            IDUL = ul.IDUL,
                            NAM = ul.NAM,
                            THANG = ul.THANG,
                            NGAY = ul.NGAY,
                            GHICHU = ul.GHICHU,
                            MANV = ul.MANV,
                            HOTEN = nv != null ? nv.HOTEN : null,
                            SOTIENUNG = ul.SOTIENUNG,
                            CREATED_BY = ul.CREATED_BY,
                            CREATED_DATE = ul.CREATED_DATE,
                            UPDATED_BY = ul.UPDATED_BY,
                            UPDATED_DATE = ul.UPDATED_DATE,
                            DELETED_BY = ul.DELETED_BY,
                            DELETED_DATE = ul.DELETED_DATE
                        };
            return query.ToList();
        }

        public TB_UNGLUONG Add(TB_UNGLUONG ul)
        {
            try
            {
                db.TB_UNGLUONG.Add(ul);
                db.SaveChanges();
                return ul;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
        }

        public TB_UNGLUONG Update(TB_UNGLUONG ul)
        {
            try
            {
                var _ul = db.TB_UNGLUONG.FirstOrDefault(x => x.IDUL == ul.IDUL);
                _ul.NAM = ul.NAM;
                _ul.THANG = ul.THANG;
                _ul.NGAY = ul.NGAY;
                _ul.MANV = ul.MANV;
                _ul.SOTIENUNG = ul.SOTIENUNG;
                _ul.GHICHU = ul.GHICHU;
                _ul.UPDATED_BY = ul.UPDATED_BY;
                _ul.UPDATED_DATE = ul.UPDATED_DATE;

                db.SaveChanges();
                return ul;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Update data " + ex.Message);
            }
        }

        public void Delete(int idul, int iduser)
        {
            var _ul = db.TB_UNGLUONG.FirstOrDefault(x => x.IDUL == idul);
            _ul.DELETED_BY = iduser;
            _ul.DELETED_DATE = DateTime.Now;

            db.SaveChanges();
        }
    }
}
