using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class NANGLUONG_NHANVIEN
    {
        MyEntities db = new MyEntities();

        public TB_NANGLUONG_NHANVIEN getItem(string soqd)
        {
            return db.TB_NANGLUONG_NHANVIEN.FirstOrDefault(x => x.SOQDNL == soqd);
        }

        public List<TB_NANGLUONG_NHANVIEN> getList()
        {
            return db.TB_NANGLUONG_NHANVIEN.ToList();
        }

        public List<NANGLUONG_NHANVIEN_DTO> getListFull()
        {
            var lstNL = db.TB_NANGLUONG_NHANVIEN.ToList();
            List<NANGLUONG_NHANVIEN_DTO> lstDTO = new List<NANGLUONG_NHANVIEN_DTO>();
            NANGLUONG_NHANVIEN_DTO nlDTO;
            foreach (var item in lstNL)
            {
                nlDTO = new NANGLUONG_NHANVIEN_DTO();
                nlDTO.SOQDNL = item.SOQDNL;
                nlDTO.SOHD = item.SOHD;
                nlDTO.HESOLUONG_NOW = item.HESOLUONG_NOW;
                nlDTO.HESOLUONG_NEW = item.HESOLUONG_NEW;
                nlDTO.GHICHUNL = item.GHICHUNL;
                nlDTO.NGAYKYNL = item.NGAYKYNL;
                nlDTO.NGAYLENLUONG = item.NGAYLENLUONG;

                nlDTO.MANV = item.MANV;
                var nv = db.TB_NHANVIEN.FirstOrDefault(a => a.MANV == item.MANV);
                nlDTO.HOTEN = nv.HOTEN;

                nlDTO.CREATED_BY = item.CREATED_BY;
                nlDTO.CREATED_DATE = item.CREATED_DATE;
                nlDTO.UPDATED_BY = item.UPDATED_BY;
                nlDTO.CREATED_DATE = item.CREATED_DATE;
                nlDTO.DELETED_BY = item.DELETED_BY;
                nlDTO.DELETED_DATE = item.DELETED_DATE;
                lstDTO.Add(nlDTO);
            }
            return lstDTO;
        }

        public TB_NANGLUONG_NHANVIEN Add(TB_NANGLUONG_NHANVIEN nl)
        {
            try
            {
                db.TB_NANGLUONG_NHANVIEN.Add(nl);
                db.SaveChanges();
                return nl;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data" + ex.Message);
            }
        }

        public TB_NANGLUONG_NHANVIEN Update(TB_NANGLUONG_NHANVIEN nl)
        {
            try
            {
                var _nl = db.TB_NANGLUONG_NHANVIEN.FirstOrDefault(x => x.SOQDNL == nl.SOQDNL);
                _nl.SOHD = nl.SOHD;
                _nl.MANV = nl.MANV;
                _nl.HESOLUONG_NOW = nl.HESOLUONG_NOW;
                _nl.HESOLUONG_NEW = nl.HESOLUONG_NEW;
                _nl.NGAYKYNL = nl.NGAYKYNL;
                _nl.NGAYLENLUONG = nl.NGAYLENLUONG;
                _nl.GHICHUNL = nl.GHICHUNL;
                _nl.UPDATED_BY = nl.UPDATED_BY;
                _nl.UPDATED_DATE = nl.UPDATED_DATE;
                db.SaveChanges();
                return nl;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi update" + ex.Message);
            }
        }

        public void Delete(string soqd, int iduser)
        {
            try
            {
                var _nl = db.TB_NANGLUONG_NHANVIEN.FirstOrDefault(x => x.SOQDNL == soqd);
                _nl.DELETED_BY = iduser;
                _nl.DELETED_DATE = DateTime.Now;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi delete" + ex.Message);
            }
        }

        public string MaxSoQuyetDinh()
        {
            var _hd = db.TB_NANGLUONG_NHANVIEN.OrderByDescending(x => x.CREATED_DATE).FirstOrDefault();
            if (_hd != null)
            {
                return _hd.SOQDNL;
            }
            else
            {
                return "00000";
            }
        }
    }
}
