using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bu.CLASS_CHAMCONG
{
    public class PHUCAP
    {

        MyEntities db = new MyEntities();
        public TB_NHANVIEN_PHUCAP getItem(int manv, int id)
        {
            return db.TB_NHANVIEN_PHUCAP.FirstOrDefault(x => x.MANV == manv && x.IDPC == id);
        }

        public List<TB_NHANVIEN_PHUCAP> getList()
        {
            return db.TB_NHANVIEN_PHUCAP.ToList();
        }

        public List<NHANVIEN_PHUCAP_DTO> GetNhanVienSortedByIDPC()
        {
            var result = (from np in db.TB_NHANVIEN_PHUCAP
                          join pc in db.TB_PHUCAP on np.IDPC equals pc.IDPC
                          join nv in db.TB_NHANVIEN on np.MANV equals nv.MANV // Thêm join để lấy HOTEN
                          select new
                          {
                              np.MANV,
                              nv.HOTEN, // Lấy HOTEN từ bảng NHANVIEN
                              np.IDPC,
                              np.SOTIEN
                          }).ToList();

            // Gộp dữ liệu theo nhân viên
            var groupedData = result
                .GroupBy(x => new { x.MANV, x.HOTEN })
                .Select(g => new NHANVIEN_PHUCAP_DTO
                {
                    MANV = g.Key.MANV,
                    HOTEN = g.Key.HOTEN,
                    SOTIEN_IDPC1 = g.FirstOrDefault(x => x.IDPC == 1)?.SOTIEN,
                    SOTIEN_IDPC2 = g.FirstOrDefault(x => x.IDPC == 2)?.SOTIEN,
                    SOTIEN_IDPC3 = g.FirstOrDefault(x => x.IDPC == 3)?.SOTIEN,
                    SOTIEN_IDPC4 = g.FirstOrDefault(x => x.IDPC == 4)?.SOTIEN,
                    SOTIEN_IDPC5 = g.FirstOrDefault(x => x.IDPC == 5)?.SOTIEN,
                    SOTIEN_IDPC6 = g.FirstOrDefault(x => x.IDPC == 6)?.SOTIEN,
                    SOTIEN_IDPC7 = g.FirstOrDefault(x => x.IDPC == 7)?.SOTIEN
                }).OrderBy(a => a.MANV).ToList();

            return groupedData;
        }



        public TB_NHANVIEN_PHUCAP Add(TB_NHANVIEN_PHUCAP pc)
        {
            try
            {
                db.TB_NHANVIEN_PHUCAP.Add(pc);
                db.SaveChanges();
                return pc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
        }

        public TB_NHANVIEN_PHUCAP Update(TB_NHANVIEN_PHUCAP pc)
        {
            try
            {
                var _pc = db.TB_NHANVIEN_PHUCAP.FirstOrDefault(x => x.MANV == pc.MANV && x.IDPC == pc.IDPC);
                _pc.NGAY = pc.NGAY;
                _pc.SOTIEN = pc.SOTIEN;
                _pc.UPDATED_BY = pc.UPDATED_BY;
                _pc.UPDATED_DATE = pc.UPDATED_DATE;

                db.SaveChanges();
                return pc;
            }
            catch (Exception ex)
            {

                throw new Exception("Lỗi Add data " + ex.Message);
            }
        }

        public void Delete(int manv, int id, int iduser)
        {
            var _lc = db.TB_NHANVIEN_PHUCAP.FirstOrDefault(x => x.MANV == manv && x.IDPC ==id);
            _lc.DELETED_BY = iduser;
            _lc.DELETED_DATE = DateTime.Now;

            db.SaveChanges();
        }

        public TB_PHUCAP getItemPC(int id)
        {
            return db.TB_PHUCAP.FirstOrDefault(x=>x.IDPC == id);
        }

        public void UpdatePhucap(int manv, int idpc, decimal sotien)
        {
            var phucap = db.TB_NHANVIEN_PHUCAP.FirstOrDefault(np => np.MANV == manv && np.IDPC == idpc);

            if (phucap != null)
            {
                phucap.SOTIEN = sotien;
            }
            else
            {
                db.TB_NHANVIEN_PHUCAP.Add(new TB_NHANVIEN_PHUCAP
                {
                    MANV = manv,
                    IDPC = idpc,
                    SOTIEN = sotien,
                    NGAY = DateTime.Now,
                    GHICHU = ""
                });
            }
            db.SaveChanges();
        }
    }
}
