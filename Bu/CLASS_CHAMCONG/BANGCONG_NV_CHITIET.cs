using DA;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_CHAMCONG
{
    public class BANGCONG_NV_CHITIET
    {
        MyEntities db = new MyEntities();

        public TB_BANGCONG_CHITIET getItem(int makycong, int manv, int ngay)
        {
            return db.TB_BANGCONG_CHITIET.FirstOrDefault(x => x.MAKYCONG == makycong && x.MANV == manv && x.NGAY.Value.Day == ngay);
        }

        public List<TB_BANGCONG_CHITIET> getBangCongCT(int makycong, int manv)
        {
            return db.TB_BANGCONG_CHITIET.Where(x => x.MAKYCONG == makycong && x.MANV == manv).OrderBy(x => x.NGAY).ToList();
        }

        public TB_BANGCONG_CHITIET Add(TB_BANGCONG_CHITIET bcct)
        {
            try
            {
                db.TB_BANGCONG_CHITIET.Add(bcct);
                db.SaveChanges();
                return bcct;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
            
        }

        public TB_BANGCONG_CHITIET Update(TB_BANGCONG_CHITIET bcct)
        {
            try
            {
                TB_BANGCONG_CHITIET bcnv = db.TB_BANGCONG_CHITIET.FirstOrDefault(x => x.MAKYCONG == bcct.MAKYCONG && x.MANV == bcct.MANV && x.NGAY == bcct.NGAY);
                bcnv.KYHIEU = bcct.KYHIEU;
                bcnv.GIOVAO = bcct.GIOVAO;
                bcnv.GIORA = bcct.GIORA;
                bcnv.NGAYPHEP = bcct.NGAYPHEP;
                bcnv.NGAYCONG = bcct.NGAYCONG;
                bcnv.GHICHU = bcct.GHICHU;
                bcnv.CONGCHUNHAT = bcct.CONGCHUNHAT;
                bcnv.CONGNGAYLE = bcct.CONGNGAYLE;
                bcnv.UPDATED_BY = bcct.UPDATED_BY;
                bcnv.UPDATED_DATE = bcct.UPDATED_DATE;
                db.SaveChanges();
                return bcct;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Update data " + ex.Message);
            }
        }

        public decimal tongNgayPhep(int makycong, int manv)
        {
            return db.TB_BANGCONG_CHITIET.Where(x => x.MAKYCONG == makycong && x.MANV == manv && x.NGAYPHEP != null).Sum(p => p.NGAYPHEP.Value);
        }

        public decimal tongNgayCong(int makycong, int manv)
        {
            return db.TB_BANGCONG_CHITIET.Where(x => x.MAKYCONG == makycong && x.MANV == manv && x.NGAYCONG != null).Sum(p => p.NGAYCONG.Value);
        }
    }
}
