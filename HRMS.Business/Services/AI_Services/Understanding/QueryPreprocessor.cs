using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Bu.Services.AI_Services.Core
{
    public static class QueryPreprocessor
    {
        private static readonly Dictionary<string, string> KeywordReplacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "nhan vien", "nhân viên" },
            { "phong ban", "phòng ban" },
            { "ke toan", "kế toán" },
            { "nhan su", "nhân sự" },
            { "sinh nhat", "sinh nhật" },
            { "tang luong", "tăng lương" },
            { "len luong", "lên lương" },
            { "chuc vu", "chức vụ" },
            { "tang ca", "tăng ca" },
            { "ung luong", "ứng lương" },
            { "thoi viec", "thôi việc" },
            { "khen thuong", "khen thưởng" },
            { "ky luat", "kỷ luật" },
            { "bao hiem", "bảo hiểm" },
            { "hop dong", "hợp đồng" },
            { "he so luong", "hệ số lương" },
            { "tai vu", "tài vụ" },
            { "kinh doanh", "kinh doanh" },
            { "ky thuat", "kỹ thuật" },
            { "giam doc", "giám đốc" },
            { "phu cap", "phụ cấp" },
            { "bao cao", "báo cáo" },
            { "gioi tinh", "giới tính" }
        };

        public static string Preprocess(string question)
        {
            if (string.IsNullOrWhiteSpace(question)) return question;

            // 1. Normalize spelling & restore diacritics on key terms
            string processed = NormalizeKeywords(question);

            // 2. Inject context/hints based on detected intent
            string hint = GetContextHint(processed);

            if (!string.IsNullOrEmpty(hint))
            {
                processed = $"{processed} {hint}";
            }

            System.Diagnostics.Debug.WriteLine($"[Preprocessor Input]: {question}");
            System.Diagnostics.Debug.WriteLine($"[Preprocessor Output]: {processed}");

            return processed;
        }

        private static string NormalizeKeywords(string text)
        {
            string normalized = text;
            foreach (var kvp in KeywordReplacements)
            {
                // Use regex with word boundaries to replace exactly the matched terms
                string pattern = @"\b" + Regex.Escape(kvp.Key) + @"\b";
                normalized = Regex.Replace(normalized, pattern, kvp.Value, RegexOptions.IgnoreCase);
            }
            return normalized;
        }

        private static string GetContextHint(string text)
        {
            string lower = text.ToLower();
            int currentMonth = DateTime.Now.Month;

            // Intent 1: Sinh nhật (Birthday)
            if (lower.Contains("sinh nhật") || lower.Contains("ngày sinh") || lower.Contains("năm sinh"))
            {
                return $"[Gợi ý CSDL: Để lọc sinh nhật trong tháng hiện tại (ví dụ: tháng {currentMonth}), sử dụng cột NGAYSINH trong bảng V_AI_EMPLOYEE hoặc TB_NHANVIEN. Dùng hàm Oracle: EXTRACT(MONTH FROM NGAYSINH) = {currentMonth} hoặc TO_CHAR(NGAYSINH, 'MM') = '{currentMonth:D2}'].";
            }

            // Intent 2: Tăng lương / Hệ số lương (Salary/Raise)
            if (lower.Contains("tăng lương") || lower.Contains("lên lương") || lower.Contains("hệ số lương") || lower.Contains("mức lương"))
            {
                return "[Gợi ý CSDL: Thông tin về lương và hệ số lương nằm ở cột HESOLUONG trong bảng TB_HOPDONG. Cần lấy thông tin nhân viên qua liên kết bảng TB_NHANVIEN (MANV) hoặc bảng hợp đồng].";
            }

            // Intent 3: Tăng ca / Làm thêm (Overtime)
            if (lower.Contains("tăng ca") || lower.Contains("làm thêm") || lower.Contains("overtime"))
            {
                return "[Gợi ý CSDL: Thông tin tăng ca nằm ở bảng TB_TANGCA và V_AI_OVERTIME, chứa cột SOGIO (số giờ tăng ca) và liên kết MANV].";
            }

            // Intent 4: Ứng lương / Tạm ứng (Advance)
            if (lower.Contains("ứng lương") || lower.Contains("tạm ứng") || lower.Contains("ứng tiền"))
            {
                return "[Gợi ý CSDL: Thông tin tạm ứng nằm ở bảng TB_UNGLUONG và V_AI_ADVANCE, chứa cột SOTIEN (số tiền ứng) và liên kết MANV].";
            }

            // Intent 5: Phụ cấp / Trợ cấp (Allowance)
            if (lower.Contains("phụ cấp") || lower.Contains("trợ cấp"))
            {
                return "[Gợi ý CSDL: Thông tin phụ cấp nằm ở bảng TB_NHANVIEN_PHUCAP và V_AI_ALLOWANCE, chứa cột SOTIEN (số tiền phụ cấp) và liên kết MANV].";
            }

            // Intent 6: Bảo hiểm (Insurance)
            if (lower.Contains("bảo hiểm") || lower.Contains("bhyt") || lower.Contains("bhxh"))
            {
                return "[Gợi ý CSDL: Thông tin bảo hiểm của nhân viên nằm ở bảng TB_BAOHIEM và V_AI_INSURANCE, chứa cột SOBH (số bảo hiểm) và liên kết MANV].";
            }

            // Intent 7: Phòng ban / Bộ phận (Department)
            if (lower.Contains("phòng ban") || lower.Contains("bộ phận") || lower.Contains("kế toán") || lower.Contains("nhân sự") || lower.Contains("tài vụ") || lower.Contains("kinh doanh"))
            {
                return "[Gợi ý CSDL: Sử dụng bảng V_AI_EMPLOYEE hoặc liên kết TB_NHANVIEN với TB_PHONGBAN. Cột tên phòng ban là TEN_PHONGBAN hoặc TENPB].";
            }

            return string.Empty;
        }
    }
}
