using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class NHANVIEN_THOIVIEC
    {
        MyEntities db = new MyEntities();
        public TB_NHANVIEN_THOIVIEC getItem(string soqd)
        {
            return db.TB_NHANVIEN_THOIVIEC.FirstOrDefault(x => x.SOQDTV == soqd);
        }
        public List<TB_NHANVIEN_THOIVIEC> getList()
        {
            return db.TB_NHANVIEN_THOIVIEC.ToList();
        }

        public List<NHANVIEN_THOIVIEC_DTO> getListFull()
        {
            var lstTV = db.TB_NHANVIEN_THOIVIEC.ToList();
            List<NHANVIEN_THOIVIEC_DTO> lstDTO = new List<NHANVIEN_THOIVIEC_DTO>();
            NHANVIEN_THOIVIEC_DTO nvDTO;
            foreach (var item in lstTV)
            {
                nvDTO = new NHANVIEN_THOIVIEC_DTO();
                nvDTO.SOQDTV = item.SOQDTV;

                nvDTO.MANV = item.MANV;
                var nv = db.TB_NHANVIEN.FirstOrDefault(a => a.MANV == item.MANV);
                nvDTO.HOTEN = nv.HOTEN;

                nvDTO.NGAYNOPDON = item.NGAYNOPDON;
                nvDTO.NGAYNGHIVIEC = item.NGAYNGHIVIEC;
                nvDTO.LYDOTV = item.LYDOTV;
                nvDTO.GHICHUTV = item.GHICHUTV;
                nvDTO.CREATED_BY = item.CREATED_BY;
                nvDTO.CREATED_DATE = item.CREATED_DATE;
                nvDTO.UPDATED_BY = item.UPDATED_BY;
                nvDTO.CREATED_DATE = item.CREATED_DATE;
                nvDTO.DELETED_BY = item.DELETED_BY;
                nvDTO.DELETED_DATE = item.DELETED_DATE;
                lstDTO.Add(nvDTO);
            }
            return lstDTO;
        }
        public TB_NHANVIEN_THOIVIEC Add(TB_NHANVIEN_THOIVIEC tv)
        {
            try
            {
                db.TB_NHANVIEN_THOIVIEC.Add(tv);
                db.SaveChanges();
                return tv;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi add NV thôi việc" + ex.Message);
            }
        }

        public TB_NHANVIEN_THOIVIEC Update(TB_NHANVIEN_THOIVIEC tv)
        {
            try
            {
                var _tv = db.TB_NHANVIEN_THOIVIEC.FirstOrDefault(x => x.SOQDTV == tv.SOQDTV);
                _tv.NGAYNOPDON = tv.NGAYNOPDON;
                _tv.MANV = tv.MANV;
                _tv.NGAYNGHIVIEC = tv.NGAYNGHIVIEC;
                _tv.LYDOTV = tv.LYDOTV;
                _tv.UPDATED_BY = tv.UPDATED_BY;
                _tv.UPDATED_DATE = tv.UPDATED_DATE;
                db.SaveChanges();
                return tv;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi update NV thôi việc" + ex.Message);
            }
        }

        public void Delete(string soqd, int iduser)
        {
            try
            {
                var _tv = db.TB_NHANVIEN_THOIVIEC.FirstOrDefault(x => x.SOQDTV == soqd);
                _tv.DELETED_BY = iduser;
                _tv.DELETED_DATE = DateTime.Now;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi delete NV thôi việc" + ex.Message);
            }
        }

        public string MaxSoQuyetDinh()
        {
            var _hd = db.TB_NHANVIEN_THOIVIEC.OrderByDescending(x => x.CREATED_DATE).FirstOrDefault();
            if (_hd != null)
            {
                return _hd.SOQDTV;
            }
            else
            {
                return "00000";
            }
        }
    }
}
