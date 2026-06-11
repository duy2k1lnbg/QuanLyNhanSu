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
            return (from hd in db.TB_HOPDONG
                    where hd.SOHD == sohd
                    join nv in db.TB_NHANVIEN on hd.MANV equals nv.MANV into nvGroup
                    from nv in nvGroup.DefaultIfEmpty()
                    join td in db.TB_TRINHDO on (nv != null ? nv.IDTD : null) equals td.IDTD into tdGroup
                    from td in tdGroup.DefaultIfEmpty()
                    join qt in db.TB_QUOCTICH on (nv != null ? nv.IDQT : null) equals qt.IDQT into qtGroup
                    from qt in qtGroup.DefaultIfEmpty()
                    join bp in db.TB_BOPHAN on (nv != null ? nv.IDBP : null) equals bp.IDBP into bpGroup
                    from bp in bpGroup.DefaultIfEmpty()
                    join cv in db.TB_CHUCVU on (nv != null ? nv.IDCV : null) equals cv.IDCV into cvGroup
                    from cv in cvGroup.DefaultIfEmpty()
                    join cty in db.TB_CONGTY on (nv != null ? nv.IDCTY : null) equals cty.IDCTY into ctyGroup
                    from cty in ctyGroup.DefaultIfEmpty()
                    select new
                    {
                        hd,
                        nv,
                        TENTD = td != null ? td.TENTD : null,
                        TENQT = qt != null ? qt.TENQT : null,
                        TENBP = bp != null ? bp.TENBP : null,
                        TENCV = cv != null ? cv.TENCV : null,
                        TENCTY = cty != null ? cty.TENCTY : null,
                        DAIDIEN = cty != null ? cty.DAIDIEN : null,
                        DIENTHOAICTY = cty != null ? cty.DIENTHOAICTY : null,
                        MASOTHUECTY = cty != null ? cty.MASOTHUECTY : null,
                        DIACHICTY = cty != null ? cty.DIACHICTY : null
                    })
                    .ToList()
                    .Select(x => new HOPDONG_DTO
                    {
                        SOHD = x.hd.SOHD,
                        NGAYBATDAU = x.hd.NGAYBATDAU.HasValue ? "Từ ngày " + x.hd.NGAYBATDAU.Value.ToString("dd/MM/yyyy").Substring(0, 2) + " tháng " + x.hd.NGAYBATDAU.Value.ToString("dd/MM/yyyy").Substring(3, 2) + " năm " + x.hd.NGAYBATDAU.Value.ToString("dd/MM/yyyy").Substring(6) : "",
                        NGAYKETTHUC = x.hd.NGAYKETTHUC.HasValue ? " ngày " + x.hd.NGAYKETTHUC.Value.ToString("dd/MM/yyyy").Substring(0, 2) + " tháng " + x.hd.NGAYKETTHUC.Value.ToString("dd/MM/yyyy").Substring(3, 2) + " năm " + x.hd.NGAYKETTHUC.Value.ToString("dd/MM/yyyy").Substring(6) : "Chưa xác định / Vô thời hạn",
                        NGAYKY = x.hd.NGAYKY.HasValue ? " ngày " + x.hd.NGAYKY.Value.ToString("dd/MM/yyyy").Substring(0, 2) + " tháng " + x.hd.NGAYKY.Value.ToString("dd/MM/yyyy").Substring(3, 2) + " năm " + x.hd.NGAYKY.Value.ToString("dd/MM/yyyy").Substring(6) : "",
                        LANKY = x.hd.LANKY,
                        HESOLUONG = x.hd.HESOLUONG,
                        LUONG_THOA_THUAN = x.hd.LUONG_THOA_THUAN,
                        THOIHAN = x.hd.THOIHAN,
                        NOIDUNG = x.hd.NOIDUNG,
                        MANV = x.hd.MANV,
                        HOTEN = x.nv != null ? x.nv.HOTEN : null,
                        CCCD = x.nv != null ? x.nv.CCCD : null,
                        DIACHI = x.nv != null ? x.nv.DIACHI : null,
                        NGAYSINH = x.nv != null && x.nv.NGAYSINH.HasValue ? x.nv.NGAYSINH.Value.ToString("dd/MM/yyyy") : "",
                        IDTD = x.nv != null ? x.nv.IDTD : null,
                        TENTD = x.TENTD,
                        IDQT = x.nv != null ? x.nv.IDQT : null,
                        TENQT = x.TENQT,
                        IDBP = x.nv != null ? x.nv.IDBP : null,
                        TENBP = x.TENBP,
                        IDCV = x.nv != null ? x.nv.IDCV : null,
                        TENCV = x.TENCV,
                        IDCTY = x.nv != null ? x.nv.IDCTY : null,
                        TENCTY = x.TENCTY,
                        DAIDIEN = x.DAIDIEN,
                        DIENTHOAICTY = x.DIENTHOAICTY,
                        MASOTHUECTY = x.MASOTHUECTY,
                        DIACHICTY = x.DIACHICTY,
                        CREATED_BY = x.hd.CREATED_BY,
                        CREATED_DATE = x.hd.CREATED_DATE,
                        UPDATE_BY = x.hd.UPDATE_BY,
                        UPDATE_DATE = x.hd.UPDATE_DATE,
                        DEL_BY = x.hd.DEL_BY,
                        DEL_DATE = x.hd.DEL_DATE
                    }).ToList();
        }
        public List<TB_HOPDONG> getList()
        {
            return db.TB_HOPDONG.ToList();
        }


        public List<HOPDONG_DTO> getlistFull_DTO() 
        { 
            return (from hd in db.TB_HOPDONG
                    join nv in db.TB_NHANVIEN on hd.MANV equals nv.MANV into nvGroup
                    from nv in nvGroup.DefaultIfEmpty()
                    join td in db.TB_TRINHDO on (nv != null ? nv.IDTD : null) equals td.IDTD into tdGroup
                    from td in tdGroup.DefaultIfEmpty()
                    join qt in db.TB_QUOCTICH on (nv != null ? nv.IDQT : null) equals qt.IDQT into qtGroup
                    from qt in qtGroup.DefaultIfEmpty()
                    select new
                    {
                        hd,
                        nv,
                        TENTD = td != null ? td.TENTD : null,
                        TENQT = qt != null ? qt.TENQT : null
                    })
                    .ToList()
                    .Select(x => new HOPDONG_DTO
                    {
                        SOHD = x.hd.SOHD,
                        NGAYBATDAU = x.hd.NGAYBATDAU.HasValue ? x.hd.NGAYBATDAU.Value.ToString("dd/MM/yyyy") : "",
                        NGAYKETTHUC = x.hd.NGAYKETTHUC.HasValue ? x.hd.NGAYKETTHUC.Value.ToString("dd/MM/yyyy") : "",
                        NGAYKY = x.hd.NGAYKY.HasValue ? x.hd.NGAYKY.Value.ToString("dd/MM/yyyy") : "",
                        LANKY = x.hd.LANKY,
                        HESOLUONG = x.hd.HESOLUONG,
                        LUONG_THOA_THUAN = x.hd.LUONG_THOA_THUAN,
                        THOIHAN = x.hd.THOIHAN,
                        NOIDUNG = x.hd.NOIDUNG,
                        MANV = x.hd.MANV,
                        HOTEN = x.nv != null ? x.nv.HOTEN : null,
                        CCCD = x.nv != null ? x.nv.CCCD : null,
                        DIACHI = x.nv != null ? x.nv.DIACHI : null,
                        NGAYSINH = x.nv != null && x.nv.NGAYSINH.HasValue ? x.nv.NGAYSINH.Value.ToString("dd/MM/yyyy") : "",
                        IDTD = x.nv != null ? x.nv.IDTD : null,
                        TENTD = x.TENTD,
                        IDQT = x.nv != null ? x.nv.IDQT : null,
                        TENQT = x.TENQT,
                        CREATED_BY = x.hd.CREATED_BY,
                        CREATED_DATE = x.hd.CREATED_DATE,
                        UPDATE_BY = x.hd.UPDATE_BY,
                        UPDATE_DATE = x.hd.UPDATE_DATE,
                        DEL_BY = x.hd.DEL_BY,
                        DEL_DATE = x.hd.DEL_DATE,
                        IDCTY = x.hd.IDCTY
                    }).ToList();
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
                _hd.LUONG_THOA_THUAN = hd.LUONG_THOA_THUAN;
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

        public List<HOPDONG_DTO> getLenLuong()
        {
            var now = DateTime.Now;
            return (from hd in db.TB_HOPDONG
                    join nv in db.TB_NHANVIEN on hd.MANV equals nv.MANV into nvGroup
                    from nv in nvGroup.DefaultIfEmpty()
                    join td in db.TB_TRINHDO on (nv != null ? nv.IDTD : null) equals td.IDTD into tdGroup
                    from td in tdGroup.DefaultIfEmpty()
                    join qt in db.TB_QUOCTICH on (nv != null ? nv.IDQT : null) equals qt.IDQT into qtGroup
                    from qt in qtGroup.DefaultIfEmpty()
                    select new
                    {
                        hd,
                        nv,
                        TENTD = td != null ? td.TENTD : null,
                        TENQT = qt != null ? qt.TENQT : null
                    })
                    .ToList()
                    .Where(x => x.hd.NGAYBATDAU.HasValue 
                             && x.hd.NGAYBATDAU.Value.Month == now.Month 
                             && (now.Year - x.hd.NGAYBATDAU.Value.Year) == 2)
                    .Select(x => new HOPDONG_DTO
                    {
                        SOHD = x.hd.SOHD,
                        NGAYBATDAU = x.hd.NGAYBATDAU.HasValue ? x.hd.NGAYBATDAU.Value.ToString("dd/MM/yyyy") : "",
                        NGAYKETTHUC = x.hd.NGAYKETTHUC.HasValue ? x.hd.NGAYKETTHUC.Value.ToString("dd/MM/yyyy") : "",
                        NGAYKY = x.hd.NGAYKY.HasValue ? x.hd.NGAYKY.Value.ToString("dd/MM/yyyy") : "",
                        LANKY = x.hd.LANKY,
                        HESOLUONG = x.hd.HESOLUONG,
                        LUONG_THOA_THUAN = x.hd.LUONG_THOA_THUAN,
                        THOIHAN = x.hd.THOIHAN,
                        NOIDUNG = x.hd.NOIDUNG,
                        MANV = x.hd.MANV,
                        HOTEN = x.nv != null ? x.nv.HOTEN : null,
                        CCCD = x.nv != null ? x.nv.CCCD : null,
                        DIACHI = x.nv != null ? x.nv.DIACHI : null,
                        NGAYSINH = x.nv != null && x.nv.NGAYSINH.HasValue ? x.nv.NGAYSINH.Value.ToString("dd/MM/yyyy") : "",
                        IDTD = x.nv != null ? x.nv.IDTD : null,
                        TENTD = x.TENTD,
                        IDQT = x.nv != null ? x.nv.IDQT : null,
                        TENQT = x.TENQT,
                        CREATED_BY = x.hd.CREATED_BY,
                        CREATED_DATE = x.hd.CREATED_DATE,
                        UPDATE_BY = x.hd.UPDATE_BY,
                        UPDATE_DATE = x.hd.UPDATE_DATE,
                        DEL_BY = x.hd.DEL_BY,
                        DEL_DATE = x.hd.DEL_DATE,
                        IDCTY = x.hd.IDCTY
                    }).ToList();
        }
    }
}
