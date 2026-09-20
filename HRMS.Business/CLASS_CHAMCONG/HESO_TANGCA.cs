using DA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.CLASS_CHAMCONG
{
    public class HESO_TANGCA
    {
        private MyEntities db = new MyEntities();

        public List<TB_HESO_TANGCA> GetList()
        {
            try
            {
                return db.Database.SqlQuery<TB_HESO_TANGCA>(
                    "SELECT ID, TEN_QUYDINH, LOAICONG, IDLOAICONG, LOAICA, IDLOAICA, GIO_BATDAU, GIO_KETTHUC, HESO_CHINHTHUC, HESO_THUVIEC, THUTU_UT, GHICHU FROM TB_HESO_TANGCA ORDER BY THUTU_UT, ID"
                ).ToList();
            }
            catch
            {
                return new List<TB_HESO_TANGCA>();
            }
        }

        /// <summary>
        /// Xác định quy định và hệ số tăng ca khớp nhất dựa trên loại công, loại ca, giờ bắt đầu và kết thúc
        /// </summary>
        public TB_HESO_TANGCA TimQuyDinh(int idLoaiCong, int idLoaiCa, string strGioBatDau, string strGioKetThuc)
        {
            var list = GetList();
            if (list == null || list.Count == 0) return null;

            if (!TimeSpan.TryParse(strGioBatDau, out var tStart) || !TimeSpan.TryParse(strGioKetThuc, out var tEnd))
            {
                return list.FirstOrDefault(x => x.IDLOAICONG == idLoaiCong && (x.IDLOAICA == idLoaiCa || x.IDLOAICA == null)) 
                    ?? list.FirstOrDefault();
            }

            // Lọc danh sách ứng viên theo loại công (LOAICONG) và loại ca (nếu có chỉ định)
            var candidates = list.Where(x => x.IDLOAICONG == idLoaiCong).ToList();
            if (candidates.Count == 0) candidates = list;

            // Lọc tiếp theo IDLOAICA nếu ứng viên có phân biệt
            var caMatches = candidates.Where(x => x.IDLOAICA == idLoaiCa || x.IDLOAICA == null).ToList();
            if (caMatches.Count > 0) candidates = caMatches;

            TB_HESO_TANGCA bestMatch = null;
            double maxOverlapMinutes = -1;

            foreach (var rule in candidates)
            {
                if (!TimeSpan.TryParse(rule.GIO_BATDAU, out var rStart) || !TimeSpan.TryParse(rule.GIO_KETTHUC, out var rEnd))
                {
                    continue;
                }

                double overlap = CalculateOverlap(tStart, tEnd, rStart, rEnd);
                if (overlap > maxOverlapMinutes)
                {
                    maxOverlapMinutes = overlap;
                    bestMatch = rule;
                }
            }

            return bestMatch ?? candidates.FirstOrDefault() ?? list.FirstOrDefault();
        }

        private double CalculateOverlap(TimeSpan s1, TimeSpan e1, TimeSpan s2, TimeSpan e2)
        {
            // Chuẩn hóa thành các khoảng thời gian tính theo phút từ 0 đến 2880 (hỗ trợ qua đêm)
            List<Tuple<int, int>> intervals1 = NormalizeInterval(s1, e1);
            List<Tuple<int, int>> intervals2 = NormalizeInterval(s2, e2);

            double totalOverlap = 0;
            foreach (var i1 in intervals1)
            {
                foreach (var i2 in intervals2)
                {
                    int startMax = Math.Max(i1.Item1, i2.Item1);
                    int endMin = Math.Min(i1.Item2, i2.Item2);
                    if (endMin > startMax)
                    {
                        totalOverlap += (endMin - startMax);
                    }
                }
            }

            return totalOverlap;
        }

        private List<Tuple<int, int>> NormalizeInterval(TimeSpan start, TimeSpan end)
        {
            var list = new List<Tuple<int, int>>();
            int startMin = (int)start.TotalMinutes;
            int endMin = (int)end.TotalMinutes;

            if (endMin >= startMin)
            {
                list.Add(new Tuple<int, int>(startMin, endMin));
                // Nếu là khung qua ngày (ví dụ 00:00 -> 06:00), ta cũng add phần đối ứng trong ngày hôm sau để khớp ca qua ngày
                if (endMin <= 360) // trước 06:00 sáng
                {
                    list.Add(new Tuple<int, int>(startMin + 1440, endMin + 1440));
                }
            }
            else
            {
                // Khoảng qua đêm: startMin -> 1440 và 0 -> endMin
                list.Add(new Tuple<int, int>(startMin, 1440 + endMin));
            }

            return list;
        }
    }
}
