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
            var lstUngLuong = db.TB_UNGLUONG.ToList();
            List<UNGLUONG_DTO> lstDTO = new List<UNGLUONG_DTO>();
            UNGLUONG_DTO ul;
            foreach (var item in lstUngLuong)
            {
                ul = new UNGLUONG_DTO();
                ul.IDUL = item.IDUL;
                ul.NAM = item.NAM;
                ul.THANG = item.THANG;
                ul.NGAY = item.NGAY;
                ul.GHICHU = item.GHICHU;

                ul.MANV = item.MANV;
                var nv = db.TB_NHANVIEN.FirstOrDefault(a => a.MANV == item.MANV);
                ul.HOTEN = nv.HOTEN;

                ul.SOTIENUNG = item.SOTIENUNG;
                ul.CREATED_BY = item.CREATED_BY;
                ul.CREATED_DATE = item.CREATED_DATE;
                ul.UPDATED_BY = item.UPDATED_BY;
                ul.UPDATED_DATE = item.UPDATED_DATE;
                ul.DELETED_BY = item.DELETED_BY;
                ul.DELETED_DATE = item.DELETED_DATE;
                lstDTO.Add(ul);
            }
            return lstDTO;
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
