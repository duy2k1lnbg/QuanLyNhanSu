using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu
{
    public class KHENTHUONG_KYLUAT
    {
        MyEntities db = new MyEntities();
        public TB_KHENTHUONG_KYLUAT getItem(string soQD)
        {
            return db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(x => x.SOQUYETDINH == soQD);
        }

        public List<TB_KHENTHUONG_KYLUAT> getList(int loai) 
        {
            return db.TB_KHENTHUONG_KYLUAT.Where(x => x.LOAI == loai).ToList();
        }

        public List<KHENTHUONG_KYLUAT_DTO> getItem_FULL(int loai, string soQD)
        {
            return (from kt in db.TB_KHENTHUONG_KYLUAT
                    where kt.LOAI == loai && kt.SOQUYETDINH == soQD
                    join nv in db.TB_NHANVIEN on kt.MANV equals nv.MANV into nvGroup
                    from nv in nvGroup.DefaultIfEmpty()
                    join bp in db.TB_BOPHAN on (nv != null ? nv.IDBP : null) equals bp.IDBP into bpGroup
                    from bp in bpGroup.DefaultIfEmpty()
                    join cv in db.TB_CHUCVU on (nv != null ? nv.IDCV : null) equals cv.IDCV into cvGroup
                    from cv in cvGroup.DefaultIfEmpty()
                    join cty in db.TB_CONGTY on (nv != null ? nv.IDCTY : null) equals cty.IDCTY into ctyGroup
                    from cty in ctyGroup.DefaultIfEmpty()
                    select new
                    {
                        kt,
                        HOTEN = nv != null ? nv.HOTEN : null,
                        IDBP = nv != null ? nv.IDBP : null,
                        TENBP = bp != null ? bp.TENBP : null,
                        IDCV = nv != null ? nv.IDCV : null,
                        TENCV = cv != null ? cv.TENCV : null,
                        IDCTY = nv != null ? nv.IDCTY : null,
                        TENCTY = cty != null ? cty.TENCTY : null
                    })
                    .ToList()
                    .Select(x => new KHENTHUONG_KYLUAT_DTO
                    {
                        SOQUYETDINH = x.kt.SOQUYETDINH,
                        TUNGAY = x.kt.TUNGAY.HasValue ? x.kt.TUNGAY.Value.ToString("dd/MM/yyyy") : null,
                        DENNGAY = x.kt.DENNGAY.HasValue ? x.kt.DENNGAY.Value.ToString("dd/MM/yyyy") : null,
                        NOIDUNG = x.kt.NOIDUNG,
                        LYDO = x.kt.LYDO,
                        NGAY = x.kt.NGAY.HasValue ? "Bắc Giang, ngày " + x.kt.NGAY.Value.ToString("dd/MM/yyyy").Substring(0, 2) + " tháng " + x.kt.NGAY.Value.ToString("dd/MM/yyyy").Substring(3, 2) + " năm " + x.kt.NGAY.Value.ToString("dd/MM/yyyy").Substring(6) : null,
                        LOAI = x.kt.LOAI,
                        MANV = x.kt.MANV,
                        HOTEN = x.HOTEN,
                        IDBP = x.IDBP,
                        TENBP = x.TENBP,
                        IDCV = x.IDCV,
                        TENCV = x.TENCV,
                        IDCTY = x.IDCTY,
                        TENCTY = x.TENCTY,
                        CREATED_BY = x.kt.CREATED_BY,
                        CREATED_DATE = x.kt.CREATED_DATE,
                        UPDATED_BY = x.kt.UPDATED_BY,
                        UPDATED_DATE = x.kt.UPDATED_DATE,
                        DELETED_BY = x.kt.DELETED_BY,
                        DELETED_DATE = x.kt.DELETED_DATE
                    }).ToList();
        }

        public List<KHENTHUONG_KYLUAT_DTO> getListFull(int loai)
        {
            return (from kt in db.TB_KHENTHUONG_KYLUAT
                    where kt.LOAI == loai
                    join nv in db.TB_NHANVIEN on kt.MANV equals nv.MANV into nvGroup
                    from nv in nvGroup.DefaultIfEmpty()
                    select new
                    {
                        kt,
                        HOTEN = nv != null ? nv.HOTEN : null
                    })
                    .ToList()
                    .Select(x => new KHENTHUONG_KYLUAT_DTO
                    {
                        SOQUYETDINH = x.kt.SOQUYETDINH,
                        TUNGAY = x.kt.TUNGAY.HasValue ? x.kt.TUNGAY.Value.ToString("dd/MM/yyyy") : null,
                        DENNGAY = x.kt.DENNGAY.HasValue ? x.kt.DENNGAY.Value.ToString("dd/MM/yyyy") : null,
                        NOIDUNG = x.kt.NOIDUNG,
                        LYDO = x.kt.LYDO,
                        NGAY = x.kt.NGAY.HasValue ? x.kt.NGAY.Value.ToString("dd/MM/yyyy") : null,
                        LOAI = x.kt.LOAI,
                        MANV = x.kt.MANV,
                        HOTEN = x.HOTEN,
                        CREATED_BY = x.kt.CREATED_BY,
                        CREATED_DATE = x.kt.CREATED_DATE,
                        UPDATED_BY = x.kt.UPDATED_BY,
                        UPDATED_DATE = x.kt.UPDATED_DATE,
                        DELETED_BY = x.kt.DELETED_BY,
                        DELETED_DATE = x.kt.DELETED_DATE
                    }).ToList();
        }

        public TB_KHENTHUONG_KYLUAT Add(TB_KHENTHUONG_KYLUAT kt)
        {
            try
            {
                db.TB_KHENTHUONG_KYLUAT.Add(kt);
                db.SaveChanges();
                return kt;
            }
            catch (Exception ex )
            {

                throw new Exception("Lỗi thêm data" + ex.Message);
            }
        }

        public TB_KHENTHUONG_KYLUAT Update(TB_KHENTHUONG_KYLUAT kt)
        {
            try
            {
                TB_KHENTHUONG_KYLUAT _kt = db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(x => x.SOQUYETDINH == kt.SOQUYETDINH);
                _kt.LOAI = kt.LOAI;
                _kt.NGAY = kt.NGAY;
                _kt.TUNGAY = kt.TUNGAY;
                _kt.DENNGAY = kt.DENNGAY;
                _kt.LYDO = kt.LYDO;
                _kt.MANV = kt.MANV;
                _kt.UPDATED_BY = kt.UPDATED_BY;
                _kt.UPDATED_DATE = kt.UPDATED_DATE;
                _kt.NOIDUNG = kt.NOIDUNG;
                db.SaveChanges();
                return kt;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi sửa data" + ex.Message);
            }
        }

        public void Delete(string soQD, int iduser)
        {
            try
            {
                TB_KHENTHUONG_KYLUAT _kt = db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(x => x.SOQUYETDINH == soQD);
                _kt.DELETED_BY = iduser;
                _kt.DELETED_DATE = DateTime.Now;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi xoá data" + ex.Message);
            }
        }
        public string MaxSoQuyetDinh(int loai)
        {
            var _hd = db.TB_KHENTHUONG_KYLUAT.Where(x => x.LOAI == loai).OrderByDescending(x => x.CREATED_DATE).FirstOrDefault();
            if (_hd != null)
            {
                return _hd.SOQUYETDINH;
            }
            else
            {
                return "00000";
            }
        }
    }
}
