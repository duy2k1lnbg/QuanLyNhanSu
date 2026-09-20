using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class DIEUCHUYEN_NHANVIEN
    {
        MyEntities db = new MyEntities();

        public TB_DIEUCHUYEN_NHANVIEN getItem(string soQD)
        {
            return db.TB_DIEUCHUYEN_NHANVIEN.FirstOrDefault(x => x.SOQDDIEUCHUYEN == soQD);
        }

        public List<TB_DIEUCHUYEN_NHANVIEN> getList()
        { 
            return db.TB_DIEUCHUYEN_NHANVIEN.ToList();
        }

        public List<DIEUCHUYEN_NHANVIEN_DTO> getListFull()
        {
            return (from dc in db.TB_DIEUCHUYEN_NHANVIEN
                    join nv in db.TB_NHANVIEN on dc.MANV equals nv.MANV into nvGroup
                    from nv in nvGroup.DefaultIfEmpty()
                    join pb1 in db.TB_PHONGBAN on dc.MAPB equals pb1.IDPB into pb1Group
                    from pb1 in pb1Group.DefaultIfEmpty()
                    join pb2 in db.TB_PHONGBAN on dc.MAPB2 equals pb2.IDPB into pb2Group
                    from pb2 in pb2Group.DefaultIfEmpty()
                    select new DIEUCHUYEN_NHANVIEN_DTO
                    {
                        SOQDDIEUCHUYEN = dc.SOQDDIEUCHUYEN,
                        NGAYDC = dc.NGAYDC,
                        MANV = dc.MANV,
                        MAPB = dc.MAPB,
                        MAPB2 = dc.MAPB2,
                        LYDODC = dc.LYDODC,
                        GHICHU = dc.GHICHU,
                        CREATED_BY = dc.CREATED_BY,
                        CREATED_DATE = dc.CREATED_DATE,
                        UPDATED_BY = dc.UPDATED_BY,
                        UPDATED_DATE = dc.UPDATED_DATE,
                        DELETED_BY = dc.DELETED_BY,
                        DELETED_DATE = dc.DELETED_DATE,
                        HOTEN = nv != null ? nv.HOTEN : null,
                        TENPB = pb1 != null ? pb1.TENPB : null,
                        TENPB2 = pb2 != null ? pb2.TENPB : null
                    }).ToList();
        }

        public TB_DIEUCHUYEN_NHANVIEN Add(TB_DIEUCHUYEN_NHANVIEN dc)
        {
            try
            {
                db.TB_DIEUCHUYEN_NHANVIEN.Add(dc);
                db.SaveChanges();
                return dc;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi điều chuyển" + ex.Message);
            }
        }

        public TB_DIEUCHUYEN_NHANVIEN Update(TB_DIEUCHUYEN_NHANVIEN dc)
        {
            try
            {
                var _dc = db.TB_DIEUCHUYEN_NHANVIEN.FirstOrDefault(x => x.SOQDDIEUCHUYEN == dc.SOQDDIEUCHUYEN);
                _dc.MAPB2 = dc.MAPB2;
                _dc.NGAYDC = dc.NGAYDC;
                _dc.LYDODC = dc.LYDODC;
                _dc.GHICHU = dc.GHICHU;
                _dc.MANV = dc.MANV;
                _dc.UPDATED_BY = dc.UPDATED_BY;
                _dc.UPDATED_DATE = dc.UPDATED_DATE;
                db.SaveChanges();
                return dc;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi sửa thông tin" + ex.Message);
            }
        }

        public void Delete(string soQD)
        {
            try
            {
                var _dc = db.TB_DIEUCHUYEN_NHANVIEN.FirstOrDefault(x => x.SOQDDIEUCHUYEN == soQD);
                //_dc.DELETED_BY = iduser;
                _dc.DELETED_BY = 1;
                _dc.DELETED_DATE = DateTime.Now;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xoá" + ex.Message);
            }
        }

        public string MaxSoQuyetDinh()
        {
            var _hd = db.TB_DIEUCHUYEN_NHANVIEN.OrderByDescending(x => x.CREATED_DATE).FirstOrDefault();
            if (_hd != null)
            {
                return _hd.SOQDDIEUCHUYEN;
            }
            else
            {
                return "00000";
            }
        }
    }
}
