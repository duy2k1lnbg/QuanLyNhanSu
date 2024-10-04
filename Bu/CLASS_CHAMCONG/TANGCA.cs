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
            var lstTangcCa = db.TB_TANGCA.ToList();
            List<TANGCA_DTO> lstDTO = new List<TANGCA_DTO>();
            TANGCA_DTO tc;
            foreach(var item in lstTangcCa)
            {
                tc = new TANGCA_DTO();
                tc.IDTCA = item.IDTCA;
                tc.NAM = item.NAM;
                tc.THANG = item.THANG;
                tc.NGAY = item.NGAY;
                tc.SOGIO = item.SOGIO;
                tc.GHICHU = item.GHICHU;

                tc.MANV = item.MANV;
                var nv = db.TB_NHANVIEN.FirstOrDefault(a => a.MANV == item.MANV);
                tc.HOTEN = nv.HOTEN;
                
                tc.IDLOAICA = item.IDLOAICA;
                var lc = db.TB_LOAICA.FirstOrDefault(b => b.IDLOAICA == item.IDLOAICA);
                tc.TENLOAICA = lc.TENLOAICA;
                tc.HESOLOAICA = lc.HESOLOAICA;

                tc.SOTIENTC = item.SOTIENTC;
                tc.CREATED_BY = item.CREATED_BY;
                tc.CREATED_DATE = item.CREATED_DATE;
                tc.UPDATED_BY = item.UPDATED_BY;
                tc.UPDATED_DATE = item.UPDATED_DATE;
                tc.DELETED_BY = item.DELETED_BY;
                tc.DELETED_DATE = item.DELETED_DATE;
                lstDTO.Add(tc);
            }    
            return lstDTO;
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
