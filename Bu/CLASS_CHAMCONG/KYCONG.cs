using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_CHAMCONG
{
    public class KYCONG
    {
        MyEntities db = new MyEntities();

        public TB_KYCONG getItem(int id)
        {
            return db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == id);
        }

        public List<TB_KYCONG> getList()
        {
            return db.TB_KYCONG.ToList();
        }

        public TB_KYCONG Add(TB_KYCONG kc)
        {
            try
            {
                db.TB_KYCONG.Add(kc);
                db.SaveChanges();
                return kc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
        }

        public TB_KYCONG Update(TB_KYCONG kc)
        {
            try
            {
                var _kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == kc.MAKYCONG);
                _kc.MAKYCONG = kc.MAKYCONG;
                _kc.NAM = kc.NAM;
                _kc.THANG = kc.THANG;
                _kc.KHOA = kc.KHOA;
                _kc.NGAYTINHCONG = kc.NGAYTINHCONG;
                _kc.NGAYCONGTRONGTHANG = kc.NGAYCONGTRONGTHANG;
                _kc.TRANGTHAI = kc.TRANGTHAI;
                _kc.UPDATED_BY = kc.UPDATED_BY;
                _kc.UPDATED_DATE = kc.UPDATED_DATE;

                db.SaveChanges();
                return kc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi update data " + ex.Message);
            }
        }

        public void Delete(int makycong, int iduser)
        {
            try
            {
                var _kc = db.TB_KYCONG.FirstOrDefault(x => x.MAKYCONG == makycong);
                _kc.DELETED_BY = iduser;
                _kc.DELETED_DATE = DateTime.Now;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi del data " + ex.Message);
            }
        }
    }
}
