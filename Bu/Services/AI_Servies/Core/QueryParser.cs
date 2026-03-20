using System;
using System.Text.RegularExpressions;

namespace Bu.Services.AI_Servies.Core
{
    public class QueryParser
    {
        // ================= NAME =================
        public string ExtractName(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return null;

            // tên sau "tên ..."
            var match = Regex.Match(q, @"tên\s+([^\?]+)", RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value.Trim();

            // tên kiểu "Nguyễn Văn A"
            match = Regex.Match(q, @"[A-ZÀ-Ỹ][a-zà-ỹ]+(\s[A-ZÀ-Ỹ][a-zà-ỹ]+)+");
            if (match.Success)
                return match.Value;

            return null;
        }

        // ================= MANV =================
        public decimal? ExtractManv(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return null;

            var match = Regex.Match(q, @"\b\d{1,6}\b");

            if (match.Success)
            {
                decimal manv;
                if (decimal.TryParse(match.Value, out manv))
                    return manv;
            }

            return null;
        }

        // ================= ACTION =================
        public bool IsCount(string q)
        {
            q = q.ToLower();
            return q.Contains("bao nhiêu")
                || q.Contains("how many")
                || q.Contains("tổng");
        }

        public bool IsList(string q)
        {
            q = q.ToLower();
            return q.Contains("liệt kê")
                || q.Contains("list")
                || q.Contains("danh sách");
        }

        public bool IsDetail(string q)
        {
            q = q.ToLower();
            return q.Contains("thông tin")
                || q.Contains("chi tiết");
        }

        // ================= DOMAIN =================
        public bool IsInsurance(string q)
        {
            q = q.ToLower();
            return q.Contains("bảo hiểm")
                || q.Contains("insurance")
                || q.Contains("bh");
        }

        public bool IsAttendance(string q)
        {
            q = q.ToLower();
            return q.Contains("chấm công")
                || q.Contains("attendance");
        }
        public string ExtractDept(string q)
        {
            q = q.ToLower();
            var match = Regex.Match(q, @"phòng\s+([^\?]+)");
            if (match.Success)
                return match.Groups[1].Value.Trim();
            return null;
        }
    }
}