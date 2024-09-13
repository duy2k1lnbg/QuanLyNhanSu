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

        public List<KHENTHUONG_KYLUAT_DTO> getListFull(int loai)
        {
            List<TB_KHENTHUONG_KYLUAT> lstKT = db.TB_KHENTHUONG_KYLUAT.Where(x => x.LOAI == loai).ToList();
            List<KHENTHUONG_KYLUAT_DTO> lst_DTO = new List<KHENTHUONG_KYLUAT_DTO>();
            KHENTHUONG_KYLUAT_DTO kt_DTO;
            foreach (var item in lstKT)
            {
                kt_DTO = new KHENTHUONG_KYLUAT_DTO();
                kt_DTO.SOQUYETDINH = item.SOQUYETDINH;
                kt_DTO.TUNGAY = item.TUNGAY;
                kt_DTO.DENNGAY = item.DENNGAY;           
                kt_DTO.NOIDUNG = item.NOIDUNG;
                kt_DTO.LYDO = item.LYDO;
                kt_DTO.NGAY = item.NGAY;
                kt_DTO.LOAI = item.LOAI;

                kt_DTO.MANV = item.MANV;
                var nv = db.TB_NHANVIEN.FirstOrDefault(n => n.MANV == item.MANV);
                kt_DTO.HOTEN = nv.HOTEN;

                //kt_DTO.IDCTY = nv.IDCTY;
                //var cty = db.TB_CONGTY.FirstOrDefault(e => e.IDCTY == nv.IDCTY);
                //kt_DTO.TENCTY = cty.TENCTY;

                kt_DTO.CREATED_BY = item.CREATED_BY;
                kt_DTO.CREATED_DATE = item.CREATED_DATE;
                kt_DTO.UPDATED_BY = item.UPDATED_BY;
                kt_DTO.UPDATED_DATE = item.UPDATED_DATE;
                kt_DTO.DELETED_BY = item.DELETED_BY;
                kt_DTO.DELETED_DATE = item.DELETED_DATE;
                //kt_DTO.IDCTY = item.IDCTY;
                lst_DTO.Add(kt_DTO);
            }
            return lst_DTO;
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

        public void Delete(string soQD)
        {
            try
            {
                TB_KHENTHUONG_KYLUAT _kt = db.TB_KHENTHUONG_KYLUAT.FirstOrDefault(x => x.SOQUYETDINH == soQD);
                _kt.DELETED_BY = _kt.DELETED_BY;
                _kt.DELETED_DATE = _kt.DELETED_DATE;
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
