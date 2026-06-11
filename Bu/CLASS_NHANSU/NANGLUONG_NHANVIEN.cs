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
            return (from nl in db.TB_NANGLUONG_NHANVIEN
                    join nv in db.TB_NHANVIEN on nl.MANV equals nv.MANV into nvGroup
                    from nv in nvGroup.DefaultIfEmpty()
                    select new NANGLUONG_NHANVIEN_DTO
                    {
                        SOQDNL = nl.SOQDNL,
                        SOHD = nl.SOHD,
                        HESOLUONG_NOW = nl.HESOLUONG_NOW,
                        HESOLUONG_NEW = nl.HESOLUONG_NEW,
                        GHICHUNL = nl.GHICHUNL,
                        NGAYKYNL = nl.NGAYKYNL,
                        NGAYLENLUONG = nl.NGAYLENLUONG,
                        MANV = nl.MANV,
                        HOTEN = nv != null ? nv.HOTEN : null,
                        CREATED_BY = nl.CREATED_BY,
                        CREATED_DATE = nl.CREATED_DATE,
                        UPDATED_BY = nl.UPDATED_BY,
                        UPDATED_DATE = nl.UPDATED_DATE,
                        DELETED_BY = nl.DELETED_BY,
                        DELETED_DATE = nl.DELETED_DATE
                    }).ToList();
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
