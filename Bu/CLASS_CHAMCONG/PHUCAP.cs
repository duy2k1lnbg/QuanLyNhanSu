using Bu.DTO;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_CHAMCONG
{
    public class PHUCAP
    {

        MyEntities db = new MyEntities();

        public TB_PHUCAP getItem_PC(int id)
        {
            return db.TB_PHUCAP.FirstOrDefault(x => x.IDPC == id);
        }

        public List<TB_PHUCAP> getList_PC()
        {
            return db.TB_PHUCAP.ToList();
        }

        public List<NHANVIEN_PHUCAP_DTO> GetNhanVienPhuCap()
        {
            using (var db = new MyEntities())
            {
                var nhanVienList = db.TB_NHANVIEN.ToList();
                var viewModelList = new List<NHANVIEN_PHUCAP_DTO>();
                foreach (var nv in nhanVienList)
                {
                    var model = new NHANVIEN_PHUCAP_DTO
                    {
                        MANV = nv.MANV,
                        HOTEN = nv.HOTEN,
                        PhuCap1 = 0,
                        PhuCap2 = 0,
                        PhuCap3 = 0,
                        PhuCap4 = 0,
                        PhuCap5 = 0,
                        PhuCap6 = 0,
                        PhuCap7 = 0
                    };

                    var phuCapList = db.TB_NHANVIEN_PHUCAP
                        .Where(nvp => nvp.MANV == nv.MANV)
                        .Select(nvp => new
                        {
                            nvp.IDPC,
                            nvp.NGAY,
                            nvp.GHICHU,
                            SOTIEN = db.TB_PHUCAP
                                       .Where(pc => pc.IDPC == nvp.IDPC)
                                       .Select(pc => pc.SOTIEN)
                                       .FirstOrDefault()
                        })
                        .ToList();

                    foreach (var pc in phuCapList)
                    {
                        switch (pc.IDPC)
                        {
                            case 1:
                                model.PhuCap1 = pc.SOTIEN ?? 0; 
                                break;
                            case 2:
                                model.PhuCap2 = pc.SOTIEN ?? 0;
                                break;
                            case 3:
                                model.PhuCap3 = pc.SOTIEN ?? 0;
                                break;
                            case 4:
                                model.PhuCap4 = pc.SOTIEN ?? 0;
                                break;
                            case 5:
                                model.PhuCap5 = pc.SOTIEN ?? 0;
                                break;
                            case 6:
                                model.PhuCap6 = pc.SOTIEN ?? 0;
                                break;
                            case 7:
                                model.PhuCap7 = pc.SOTIEN ?? 0;
                                break;
                        }
                    }

                    viewModelList.Add(model);
                }

                return viewModelList;  
            }
        }

        public List<NHANVIEN_PHUCAP_DTO> GetNV_PC(int manv)
        {
            using (var db = new MyEntities())
            {
                var nhanVienList = db.TB_NHANVIEN.Where(x => x.MANV == manv).ToList();
                var viewModelList = new List<NHANVIEN_PHUCAP_DTO>();
                foreach (var nv in nhanVienList)
                {
                    var model = new NHANVIEN_PHUCAP_DTO
                    {
                        MANV = nv.MANV,
                        HOTEN = nv.HOTEN,
                        PhuCap1 = 0,
                        PhuCap2 = 0,
                        PhuCap3 = 0,
                        PhuCap4 = 0,
                        PhuCap5 = 0,
                        PhuCap6 = 0,
                        PhuCap7 = 0
                    };

                    var phuCapList = db.TB_NHANVIEN_PHUCAP
                        .Where(nvp => nvp.MANV == nv.MANV)
                        .Select(nvp => new
                        {
                            nvp.IDPC,
                            nvp.NGAY,
                            nvp.GHICHU,
                            SOTIEN = db.TB_PHUCAP
                                       .Where(pc => pc.IDPC == nvp.IDPC)
                                       .Select(pc => pc.SOTIEN)
                                       .FirstOrDefault()
                        })
                        .ToList();

                    foreach (var pc in phuCapList)
                    {
                        switch (pc.IDPC)
                        {
                            case 1:
                                model.PhuCap1 = pc.SOTIEN ?? 0;
                                break;
                            case 2:
                                model.PhuCap2 = pc.SOTIEN ?? 0;
                                break;
                            case 3:
                                model.PhuCap3 = pc.SOTIEN ?? 0;
                                break;
                            case 4:
                                model.PhuCap4 = pc.SOTIEN ?? 0;
                                break;
                            case 5:
                                model.PhuCap5 = pc.SOTIEN ?? 0;
                                break;
                            case 6:
                                model.PhuCap6 = pc.SOTIEN ?? 0;
                                break;
                            case 7:
                                model.PhuCap7 = pc.SOTIEN ?? 0;
                                break;
                        }
                    }

                    viewModelList.Add(model);
                }

                return viewModelList;
            }
        }
    }
}
