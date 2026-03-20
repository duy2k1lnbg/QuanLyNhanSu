using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bu.Services.AI_Servies
{
    public class AttendanceRepository
    {
        public List<string> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<string>();

            keyword = keyword.ToLower().Trim();

            using (var db = new AIEntities())
            {
                var query = db.V_AI_CHAMCONG.AsQueryable();

                // ================= 1. FILTER THEO MANV =================
                decimal manv;
                if (decimal.TryParse(ExtractNumber(keyword), out manv))
                {
                    query = query.Where(x => x.MANV == manv);
                }

                // ================= 2. FILTER THEO NĂM =================
                int year;
                if (TryExtractYear(keyword, out year))
                {
                    query = query.Where(x => x.NAM == year);
                }

                // ================= 3. FILTER THEO THÁNG =================
                int month;
                if (TryExtractMonth(keyword, out month))
                {
                    query = query.Where(x => x.THANG == month);
                }

                // ================= 4. FILTER THEO NGÀY =================
                int day;
                if (TryExtractDay(keyword, out day))
                {
                    query = query.Where(x => x.NGAY == day);
                }

                // ================= 5. ĐI TRỄ =================
                if (keyword.Contains("trễ") || keyword.Contains("muộn"))
                {
                    query = query.Where(x => x.GIOVAO > 8 || (x.GIOVAO == 8 && x.PHUTVAO > 0));
                }

                // ================= 6. VỀ SỚM =================
                if (keyword.Contains("sớm"))
                {
                    query = query.Where(x => x.GIORA < 17);
                }

                // ================= 7. LÀM ĐỦ GIỜ =================
                if (keyword.Contains("đủ giờ"))
                {
                    query = query.Where(x => x.GIOVAO <= 8 && x.GIORA >= 17);
                }

                // ================= 8. KHOẢNG THỜI GIAN =================
                if (keyword.Contains("từ") && keyword.Contains("đến"))
                {
                    int fromMonth, toMonth;
                    if (TryExtractRange(keyword, out fromMonth, out toMonth))
                    {
                        query = query.Where(x => x.THANG >= fromMonth && x.THANG <= toMonth);
                    }
                }

                // ================= 9. SORT =================
                var result = query
                    .OrderByDescending(x => x.NAM)
                    .ThenByDescending(x => x.THANG)
                    .ThenByDescending(x => x.NGAY)
                    .Take(10)
                    .ToList();

                // ================= 10. FORMAT =================
                return result.Select(x => Format(x)).ToList();
            }
        }

        // ================= FORMAT =================
        private string Format(dynamic x)
        {
            return "NV: " + x.MANV +
                   " | Ngày: " + x.NGAY + "/" + x.THANG + "/" + x.NAM +
                   " | Vào: " + x.GIOVAO + ":" + x.PHUTVAO +
                   " | Ra: " + x.GIORA + ":" + x.PHUTRA;
        }

        // ================= HELPER =================

        private string ExtractNumber(string text)
        {
            var match = Regex.Match(text, @"\d+");
            return match.Success ? match.Value : "";
        }

        private bool TryExtractYear(string text, out int year)
        {
            var match = Regex.Match(text, @"20\d{2}");
            if (match.Success)
            {
                year = int.Parse(match.Value);
                return true;
            }
            year = 0;
            return false;
        }

        private bool TryExtractMonth(string text, out int month)
        {
            var match = Regex.Match(text, @"tháng\s*(\d{1,2})");
            if (match.Success)
            {
                month = int.Parse(match.Groups[1].Value);
                return true;
            }
            month = 0;
            return false;
        }

        private bool TryExtractDay(string text, out int day)
        {
            var match = Regex.Match(text, @"ngày\s*(\d{1,2})");
            if (match.Success)
            {
                day = int.Parse(match.Groups[1].Value);
                return true;
            }
            day = 0;
            return false;
        }

        private bool TryExtractRange(string text, out int from, out int to)
        {
            var match = Regex.Match(text, @"từ\s*(\d{1,2}).*đến\s*(\d{1,2})");
            if (match.Success)
            {
                from = int.Parse(match.Groups[1].Value);
                to = int.Parse(match.Groups[2].Value);
                return true;
            }
            from = to = 0;
            return false;
        }
    }
}