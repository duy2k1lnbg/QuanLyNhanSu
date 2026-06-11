using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_CHAMCONG
{
    public class TANGCA
    {
        MyEntities db = new MyEntities();

        public TB_TANGCA getItem(int id)
        {
            return db.TB_TANGCA.FirstOrDefault(x => x.IDTCA == id);
        }

        public List<TB_TANGCA> getList()
        {
            return db.TB_TANGCA.ToList();
        }

        public List<TANGCA_DTO> getListFull()
        {
            var query = from tc in db.TB_TANGCA
                        join nv in db.TB_NHANVIEN on tc.MANV equals nv.MANV into nvGroup
                        from nv in nvGroup.DefaultIfEmpty()
                        join lc in db.TB_LOAICA on tc.IDLOAICA equals lc.IDLOAICA into lcGroup
                        from lc in lcGroup.DefaultIfEmpty()
                        select new TANGCA_DTO
                        {
                            IDTCA = tc.IDTCA,
                            NAM = tc.NAM,
                            THANG = tc.THANG,
                            NGAY = tc.NGAY,
                            SOGIO = tc.SOGIO,
                            GHICHU = tc.GHICHU,
                            MANV = tc.MANV,
                            HOTEN = nv != null ? nv.HOTEN : null,
                            IDLOAICA = tc.IDLOAICA,
                            TENLOAICA = lc != null ? lc.TENLOAICA : null,
                            HESOLOAICA = lc != null ? lc.HESOLOAICA : null,
                            SOTIENTC = tc.SOTIENTC,
                            CREATED_BY = tc.CREATED_BY,
                            CREATED_DATE = tc.CREATED_DATE,
                            UPDATED_BY = tc.UPDATED_BY,
                            UPDATED_DATE = tc.UPDATED_DATE,
                            DELETED_BY = tc.DELETED_BY,
                            DELETED_DATE = tc.DELETED_DATE
                        };
            return query.ToList();
        }

        public TB_TANGCA Add(TB_TANGCA lc)
        {
            try
            {
                db.TB_TANGCA.Add(lc);
                db.SaveChanges();
                return lc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
        }

        public TB_TANGCA Update(TB_TANGCA tc)
        {
            try
            {
                var _tc = db.TB_TANGCA.FirstOrDefault(x => x.IDTCA == tc.IDTCA);
                _tc.IDLOAICA = tc.IDLOAICA;
                _tc.NAM = tc.NAM;
                _tc.THANG = tc.THANG;
                _tc.NGAY = tc.NGAY;
                _tc.SOGIO = tc.SOGIO;
                _tc.MANV = tc.MANV;
                _tc.GHICHU = tc.GHICHU;
                _tc.SOTIENTC = tc.SOTIENTC;
                _tc.UPDATED_BY = tc.UPDATED_BY;
                _tc.UPDATED_DATE = tc.UPDATED_DATE;

                db.SaveChanges();
                return tc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Update data " + ex.Message);
            }
        }

        public void Delete(int idtc, int iduser)
        {
            var _lc = db.TB_TANGCA.FirstOrDefault(x => x.IDTCA == idtc);
            _lc.DELETED_BY = iduser;
            _lc.DELETED_DATE = DateTime.Now;

            db.SaveChanges();
        }
    }
}
