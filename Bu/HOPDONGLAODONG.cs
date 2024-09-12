using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class HOPDONGLAODONG
    {
        MyEntities db = new MyEntities();

        public TB_HOPDONG getItem(string sohd)
        {
            return db.TB_HOPDONG.FirstOrDefault(x => x.SOHD == sohd);
        }
        public List<TB_HOPDONG> getList()
        {
            return db.TB_HOPDONG.ToList();
        }


        public List<HOPDONG_DTO> getlistFull_DTO() 
        { 
            List<TB_HOPDONG> lstHD = db.TB_HOPDONG.ToList();
            List<HOPDONG_DTO> lstHD_DTO = new List<HOPDONG_DTO>();
            HOPDONG_DTO hd_DTO;
            foreach (var item in lstHD)
            {
                hd_DTO = new HOPDONG_DTO();
                hd_DTO.SOHD = item.SOHD;
                hd_DTO.NGAYBATDAU = item.NGAYBATDAU;
                hd_DTO.NGAYKETTHUC = item.NGAYKETTHUC;
                hd_DTO.NGAYKY = item.NGAYKY;
                hd_DTO.LANKY = item.LANKY;
                hd_DTO.HESOLUONG = item.HESOLUONG;
                hd_DTO.THOIHAN = item.THOIHAN;
                hd_DTO.NOIDUNG = item.NOIDUNG;
                hd_DTO.MANV = item.MANV;
                var nv = db.TB_NHANVIEN.FirstOrDefault(n => n.MANV == item.MANV);
                hd_DTO.HOTEN = nv.HOTEN;
                hd_DTO.CREATED_BY = item.CREATED_BY;
                hd_DTO.CREATED_DATE = item.CREATED_DATE;
                hd_DTO.UPDATE_BY = item.UPDATE_BY;
                hd_DTO.UPDATE_DATE = item.UPDATE_DATE;
                hd_DTO.DEL_BY = item.DEL_BY;
                hd_DTO.DEL_DATE = item.DEL_DATE;
                hd_DTO.IDCTY = item.IDCTY;
                lstHD_DTO.Add(hd_DTO);
            }
            return lstHD_DTO;
        }
        public TB_HOPDONG Add(TB_HOPDONG hd)
        {
            try
            {
                db.TB_HOPDONG.Add(hd);
                db.SaveChanges();
                return hd;
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

        public TB_HOPDONG Update(TB_HOPDONG hd)
        {
            try
            {
                var _hd = db.TB_HOPDONG.FirstOrDefault(x => x.SOHD == hd.SOHD);
                _hd.NGAYBATDAU = hd.NGAYBATDAU;
                _hd.NGAYKETTHUC = hd.NGAYKETTHUC;
                _hd.MANV = hd.MANV;
                _hd.NGAYKY = hd.NGAYKY;
                _hd.LANKY = hd.LANKY;
                _hd.HESOLUONG = hd.HESOLUONG;
                _hd.NOIDUNG = hd.NOIDUNG;
                _hd.THOIHAN = hd.THOIHAN;
                _hd.SOHD = hd.SOHD;
                _hd.IDCTY = hd.IDCTY;
                _hd.UPDATE_BY = hd.UPDATE_BY;
                _hd.UPDATE_DATE = hd.UPDATE_DATE;
                db.SaveChanges();
                return hd;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }

        public void Delete(string sohd, int manv)
        {
            try
            {
                var _hd = db.TB_HOPDONG.FirstOrDefault(x => x.SOHD == sohd);
                _hd.DEL_BY = manv;
                _hd.DEL_DATE = DateTime.Now;
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi" + ex.Message);
            }
        }

        public string MaxSoHopDong()
        {
            var _hd = db.TB_HOPDONG.OrderByDescending(x => x.CREATED_DATE).FirstOrDefault();
            if (_hd != null)
            {
                return _hd.SOHD;
            }
            else
            {
                return "00000";
            }
        }
    }
}
