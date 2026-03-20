using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bu.Services.AI_Servies
{
    public class InsuranceRepository
    {
        public List<string> Search(string keyword)
        {
            using (var db = new AIEntities())
            {
                keyword = keyword.ToLower();

                var query = from bh in db.V_AI_BAOHIEM
                            join emp in db.V_AI_EMP_WITH_DEPT
                                on bh.MANV equals emp.MANV
                            where emp.HOTEN.ToLower().Contains(keyword)
                            select new
                            {
                                emp.HOTEN,
                                bh.SOBH
                            };

                var result = query
                    .Take(5)
                    .ToList()
                    .Select(x => x.HOTEN + " - BH: " + x.SOBH)
                    .ToList();

                return result;
            }
        }

        // ================= SCORE =================
        private int Score(dynamic x, string keyword, string keywordNoSign)
        {
            int score = 0;

            string name = (x.HOTEN ?? "").ToLower();
            string sobh = (x.SOBH ?? "").ToLower();
            string noicap = (x.NOICAP ?? "").ToLower();
            string noikham = (x.NOIKHAMBENH ?? "").ToLower();

            string nameNoSign = RemoveVietnameseTone(name);

            // ===== NAME =====
            if (name.Contains(keyword)) score += 100;
            if (nameNoSign.Contains(keywordNoSign)) score += 80;

            // ===== SOBH =====
            if (sobh.Contains(keyword)) score += 120;

            // ===== MANV =====
            if (keyword.All(char.IsDigit) && x.MANV.ToString().Contains(keyword))
                score += 110;

            // ===== NOICAP =====
            if (noicap.Contains(keyword)) score += 70;

            // ===== NOIKHAM =====
            if (noikham.Contains(keyword)) score += 70;

            // ===== DATE =====
            if (keyword.Contains("ngày") || keyword.Contains("date"))
            {
                if (x.NGAYCAP != null && x.NGAYCAP.ToString().Contains(keyword))
                    score += 60;
            }

            // ===== FUZZY =====
            if (Fuzzy(nameNoSign, keywordNoSign))
                score += 40;

            return score;
        }

        // ================= FORMAT =================
        private string Format(dynamic x)
        {
            return "Mã NV: " + x.MANV +
                   " | Tên: " + x.HOTEN +
                   " | Số BH: " + x.SOBH +
                   " | Nơi cấp: " + x.NOICAP +
                   " | Nơi khám: " + x.NOIKHAMBENH;
        }

        // ================= REMOVE TONE =================
        private string RemoveVietnameseTone(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            string[] arr1 = new string[] {
                "á","à","ả","ã","ạ","ă","ắ","ằ","ẳ","ẵ","ặ","â","ấ","ầ","ẩ","ẫ","ậ",
                "đ",
                "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                "í","ì","ỉ","ĩ","ị",
                "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                "ý","ỳ","ỷ","ỹ","ỵ"
            };

            string[] arr2 = new string[] {
                "a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","a",
                "d",
                "e","e","e","e","e","e","e","e","e","e","e",
                "i","i","i","i","i",
                "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                "u","u","u","u","u","u","u","u","u","u","u",
                "y","y","y","y","y"
            };

            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
            }

            return text;
        }

        // ================= FUZZY =================
        private bool Fuzzy(string source, string keyword)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(keyword))
                return false;

            int match = 0;
            foreach (char c in keyword)
            {
                if (source.Contains(c)) match++;
            }

            return match >= keyword.Length / 2;
        }
    }
}