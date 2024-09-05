using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class NHANVIEN
    {
        MyEntities db = new MyEntities();

        public TB_NHANVIEN getItem(int id)
        {
            return db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == id);
        }
        public List<TB_NHANVIEN> getList()
        {
            return db.TB_NHANVIEN.ToList();
        }
        public List<NHANVIEN_DTO> getListFll_DTO() 
        { 
            var lstNV = db.TB_NHANVIEN.ToList();
            List<NHANVIEN_DTO > lstNvDTO = new List<NHANVIEN_DTO>();
            NHANVIEN_DTO nvDTO;
            foreach (var item in lstNV) 
            {
                nvDTO = new NHANVIEN_DTO();
                nvDTO.MANV = item.MANV;
                nvDTO.HOTEN = item.HOTEN;
                nvDTO.IDGT = item.IDGT;
                nvDTO.CCCD = item.CCCD;
                nvDTO.NGAYSINH = item.NGAYSINH;
                nvDTO.DIENTHOAI = item.DIENTHOAI;
                nvDTO.DIACHI = item.DIACHI;
                nvDTO.HINHANH = item.HINHANH;

                nvDTO.IDTD = item.IDTD;
                var td = db.TB_TRINHDO.FirstOrDefault(a => a.IDTD == item.IDTD);
                nvDTO.TENTD = td.TENTD;

                nvDTO.IDBP = item.IDBP;
                var bp = db.TB_BOPHAN.FirstOrDefault( b=> b.IDBP == item.IDBP);
                nvDTO.TENBP = bp.TENBP;

                nvDTO.IDPB = item.IDPB;
                var pb = db.TB_PHONGBAN.FirstOrDefault(c => c.IDPB == item.IDPB);
                nvDTO.TENPB = pb.TENPB;

                nvDTO.IDDT = item.IDDT;
                var dt = db.TB_DANTOC.FirstOrDefault(e => e.IDDT == item.IDDT);
                nvDTO.TENDT = dt.TENDT;

                nvDTO.IDCTY = item.IDCTY;
                var cty = db.TB_CONGTY.FirstOrDefault(f => f.IDCTY == item.IDCTY);
                nvDTO.TENCTY = cty.TENCTY;

                nvDTO.IDCV = item.IDCV;
                var cv = db.TB_CHUCVU.FirstOrDefault(g => g.IDCV == item.IDCV);
                nvDTO.TENCV = cv.TENCV;

                nvDTO.IDTG = item.IDTG;
                var tg = db.TB_TONGIAO.FirstOrDefault(h => h.IDTG == item.IDTG);
                nvDTO.TENTG = tg.TENTG;

                nvDTO.IDGT = item.IDGT;
                var gt = db.TB_GIOITINH.FirstOrDefault(i => i.IDGT == item.IDGT);
                nvDTO.TENGT = gt.TENGT;

                lstNvDTO.Add(nvDTO);

            }
            return lstNvDTO;
        }

        public TB_NHANVIEN Add(TB_NHANVIEN nv)
        {
            try
            {
                db.TB_NHANVIEN.Add(nv);
                db.SaveChanges();
                return nv;
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

        public TB_NHANVIEN Update(TB_NHANVIEN nv)
        {
            try
            {
                var _nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == nv.MANV);
                _nv.HOTEN = nv.HOTEN;
                _nv.IDGT = nv.IDGT;
                _nv.NGAYSINH = nv.NGAYSINH;
                _nv.DIENTHOAI = nv.DIENTHOAI;
                _nv.CCCD = nv.CCCD;
                _nv.DIACHI = nv.DIACHI;
                _nv.HINHANH = nv.HINHANH;
                _nv.IDPB = nv.IDPB;
                _nv.IDBP = nv.IDBP;
                _nv.IDCV = nv.IDCV;
                _nv.IDTD = nv.IDTD;
                _nv.IDDT = nv.IDDT;
                _nv.IDTG = nv.IDTG;
                _nv.IDCTY = nv.IDCTY;
                db.SaveChanges();
                return nv;
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
                var _nv = db.TB_NHANVIEN.FirstOrDefault(x => x.MANV == id);
                db.TB_NHANVIEN.Remove(_nv);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }
    }
}
