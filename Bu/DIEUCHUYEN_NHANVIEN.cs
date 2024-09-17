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
            var lstDC = db.TB_DIEUCHUYEN_NHANVIEN.ToList();
            List<DIEUCHUYEN_NHANVIEN_DTO> lstDTO = new List<DIEUCHUYEN_NHANVIEN_DTO>();
            DIEUCHUYEN_NHANVIEN_DTO nvDTO;
            foreach (var item in lstDC)
            { 
                nvDTO = new DIEUCHUYEN_NHANVIEN_DTO();
                nvDTO.SOQDDIEUCHUYEN = item.SOQDDIEUCHUYEN;

                nvDTO.MANV = item.MANV;
                var nv = db.TB_NHANVIEN.FirstOrDefault(a => a.MANV == item.MANV);
                nvDTO.HOTEN = nv.HOTEN;

                nvDTO.MAPB = item.MAPB;
                var pb = db.TB_PHONGBAN.FirstOrDefault(b => b.IDPB == item.MAPB);
                nvDTO.TENPB = pb.TENPB;

                nvDTO.MAPB2 = item.MAPB2;
                var pb2 = db.TB_PHONGBAN.FirstOrDefault(c => c.IDPB == item.MAPB2);
                nvDTO.TENPB2 = pb2.TENPB;

                nvDTO.NGAYDC = item.NGAYDC;
                nvDTO.LYDODC = item.LYDODC;
                nvDTO.GHICHU = item.GHICHU;
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
