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
        public List<HOPDONG_DTO> getItem_FULL(string sohd)
        {
            List<TB_HOPDONG> lstHD = db.TB_HOPDONG.Where( x => x.SOHD == sohd).ToList();
            List<HOPDONG_DTO> lstHD_DTO = new List<HOPDONG_DTO>();
            HOPDONG_DTO hd_DTO;
            foreach (var item in lstHD)
            {
                hd_DTO = new HOPDONG_DTO();
                hd_DTO.SOHD = item.SOHD;
                hd_DTO.NGAYBATDAU = "Từ ngày " + item.NGAYBATDAU.Value.ToString("dd/MM/yyyy").Substring(0, 2) + " tháng " + item.NGAYBATDAU.Value.ToString("dd/MM/yyyy").Substring(3, 2) + " năm " + item.NGAYBATDAU.Value.ToString("dd/MM/yyyy").Substring(6);
                hd_DTO.NGAYKETTHUC = " ngày " + item.NGAYKETTHUC.Value.ToString("dd/MM/yyyy").Substring(0, 2) + " tháng " + item.NGAYKETTHUC.Value.ToString("dd/MM/yyyy").Substring(3, 2) + " năm " + item.NGAYKETTHUC.Value.ToString("dd/MM/yyyy").Substring(6);
                //hd_DTO.NGAYBATDAU = item.NGAYBATDAU.Value.ToString("dd/MM/yyyy");
                //hd_DTO.NGAYKETTHUC = item.NGAYKETTHUC.Value.ToString("dd/MM/yyyy");
                hd_DTO.NGAYKY = item.NGAYKY.Value.ToString("dd/MM/yyyy");
                hd_DTO.LANKY = item.LANKY;
                hd_DTO.HESOLUONG = item.HESOLUONG;
                hd_DTO.THOIHAN = item.THOIHAN;
                hd_DTO.NOIDUNG = item.NOIDUNG;

                hd_DTO.MANV = item.MANV;
                var nv = db.TB_NHANVIEN.FirstOrDefault(n => n.MANV == item.MANV);
                hd_DTO.HOTEN = nv.HOTEN;
                hd_DTO.CCCD = nv.CCCD;
                hd_DTO.DIACHI = nv.DIACHI;
                hd_DTO.NGAYSINH = nv.NGAYSINH.Value.ToString("dd/MM/yyyy");

                hd_DTO.IDTD = nv.IDTD;
                var td = db.TB_TRINHDO.FirstOrDefault(a => a.IDTD == nv.IDTD);
                hd_DTO.TENTD = td.TENTD;

                hd_DTO.IDQT = nv.IDQT;
                var qt = db.TB_QUOCTICH.FirstOrDefault(b => b.IDQT == nv.IDQT);
                hd_DTO.TENQT = qt.TENQT;

                hd_DTO.IDBP = nv.IDBP;
                var bp = db.TB_BOPHAN.FirstOrDefault(c => c.IDBP == nv.IDBP);
                hd_DTO.TENBP = bp.TENBP;

                hd_DTO.IDCV = nv.IDCV;
                var cv = db.TB_CHUCVU.FirstOrDefault(d => d.IDCV == nv.IDCV);
                hd_DTO.TENCV = cv.TENCV;

                hd_DTO.IDCTY = nv.IDCTY;
                var cty = db.TB_CONGTY.FirstOrDefault(e => e.IDCTY == nv.IDCTY);
                hd_DTO.TENCTY = cty.TENCTY;
                hd_DTO.DAIDIEN = cty.DAIDIEN;
                hd_DTO.DIENTHOAICTY = cty.DIENTHOAICTY;
                hd_DTO.MASOTHUECTY = cty.MASOTHUECTY;
                hd_DTO.DIACHICTY = cty.DIACHICTY;

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
                hd_DTO.NGAYBATDAU = item.NGAYBATDAU.Value.ToString("dd/MM/yyyy");
                hd_DTO.NGAYKETTHUC = item.NGAYKETTHUC.Value.ToString("dd/MM/yyyy");
                //hd_DTO.NGAYBATDAU = "Từ ngày " + item.NGAYBATDAU.Value.ToString("dd/MM/yyyy").Substring(0,2) + " tháng " + item.NGAYBATDAU.Value.ToString("dd/MM/yyyy").Substring(3, 2) + " năm " + item.NGAYBATDAU.Value.ToString("dd/MM/yyyy").Substring(6);
                //hd_DTO.NGAYKETTHUC = "Từ ngày " + item.NGAYKETTHUC.Value.ToString("dd/MM/yyyy").Substring(0, 2) + " tháng " + item.NGAYKETTHUC.Value.ToString("dd/MM/yyyy").Substring(3, 2) + " năm " + item.NGAYKETTHUC.Value.ToString("dd/MM/yyyy").Substring(6);
                hd_DTO.NGAYKY = item.NGAYKY.Value.ToString("dd/MM/yyyy");
                hd_DTO.LANKY = item.LANKY;
                hd_DTO.HESOLUONG = item.HESOLUONG;
                hd_DTO.THOIHAN = item.THOIHAN;
                hd_DTO.NOIDUNG = item.NOIDUNG;

                hd_DTO.MANV = item.MANV;
                var nv = db.TB_NHANVIEN.FirstOrDefault(n => n.MANV == item.MANV);
                hd_DTO.HOTEN = nv.HOTEN;
                hd_DTO.CCCD = nv.CCCD;
                hd_DTO.DIACHI = nv.DIACHI;
                hd_DTO.NGAYSINH = nv.NGAYSINH.Value.ToString("dd/MM/yyyy");

                hd_DTO.IDTD = nv.IDTD;
                var td = db.TB_TRINHDO.FirstOrDefault(a => a.IDTD == nv.IDTD);
                hd_DTO.TENTD = td.TENTD;

                hd_DTO.IDQT = nv.IDQT;
                var qt = db.TB_QUOCTICH.FirstOrDefault(b => b.IDQT == nv.IDQT);
                hd_DTO.TENQT = qt.TENQT;

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
