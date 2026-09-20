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
            return (from tv in db.TB_NHANVIEN_THOIVIEC
                    join nv in db.TB_NHANVIEN on tv.MANV equals nv.MANV into nvGroup
                    from nv in nvGroup.DefaultIfEmpty()
                    select new NHANVIEN_THOIVIEC_DTO
                    {
                        SOQDTV = tv.SOQDTV,
                        MANV = tv.MANV,
                        HOTEN = nv != null ? nv.HOTEN : null,
                        NGAYNOPDON = tv.NGAYNOPDON,
                        NGAYNGHIVIEC = tv.NGAYNGHIVIEC,
                        LYDOTV = tv.LYDOTV,
                        GHICHUTV = tv.GHICHUTV,
                        CREATED_BY = tv.CREATED_BY,
                        CREATED_DATE = tv.CREATED_DATE,
                        UPDATED_BY = tv.UPDATED_BY,
                        UPDATED_DATE = tv.UPDATED_DATE,
                        DELETED_BY = tv.DELETED_BY,
                        DELETED_DATE = tv.DELETED_DATE
                    }).ToList();
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
