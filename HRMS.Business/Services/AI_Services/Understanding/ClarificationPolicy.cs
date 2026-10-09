using System;
using System.Linq;
using System.Globalization;

namespace Bu.Services.AI_Services.Core
{
    public class ClarificationPolicy
    {
        private static ClarificationOption Option(string label, string value) => new ClarificationOption { Token = Guid.NewGuid().ToString("N"), Label = label, Value = value };
        public bool EvaluateClarificationNeeded(QueryUnderstandingResult r)
        {
            if (r == null || r.SupportStatus == QuerySupportStatus.Unsupported || r.SupportStatus == QuerySupportStatus.Forbidden) return false;
            if (r.Clarification != null && r.SupportStatus == QuerySupportStatus.NeedsClarification) return true;
            var entity = r.Entities.FirstOrDefault(e => e.Status == "AMBIGUOUS" && e.Candidates.Count > 1);
            if (entity != null)
            {
                r.Clarification = new ClarificationPrompt { Field = entity.EntityType == "DEPARTMENT" ? "DEPARTMENT" : "MANV", Question = "Có nhiều kết quả phù hợp. Bạn muốn tra cứu đối tượng nào?" };
                foreach (var c in entity.Candidates.Take(5)) r.Clarification.Options.Add(Option(c.Name + " (#" + c.Id + " • " + c.DepartmentName + " • " + c.PositionName + ")", c.Id.ToString(CultureInfo.InvariantCulture)));
            }
            else if (r.Domain == "PAYROLL" && r.Metric == "PAYROLL_UNSPECIFIED")
            {
                r.Clarification = new ClarificationPrompt { Field = "METRIC", Question = "Bạn muốn xem khoản lương nào?" };
                r.Clarification.Options.Add(Option("Thực lĩnh kỳ công", "THUCLANH"));
                r.Clarification.Options.Add(Option("Tiền lương theo ngày công thực tế", "LUONG_CONG_THUCTE"));
            }
            else if (r.Domain == "OVERTIME" && string.IsNullOrEmpty(r.Operation))
            {
                r.Clarification = new ClarificationPrompt { Field = "OPERATION", Question = "Bạn muốn xem chi tiết hay tổng số giờ tăng ca?" };
                r.Clarification.Options.Add(Option("Danh sách chi tiết", "VIEW")); r.Clarification.Options.Add(Option("Tổng số giờ", "SUM"));
            }
            else if (new[] { "OVERTIME", "ATTENDANCE", "ADVANCE", "PAYROLL" }.Contains(r.Domain) && (!r.Time.Month.HasValue || !r.Time.Year.HasValue) && r.Time.AnchorKind != "INVALID")
            {
                r.Clarification = new ClarificationPrompt { Field = "TIME_PERIOD", Question = r.Time.Month.HasValue ? "Bạn muốn tra cứu tháng " + r.Time.Month + " của năm nào? (Ví dụ: tháng 9 năm 2026)" : "Bạn muốn tra cứu kỳ nào? (Ví dụ: tháng này hoặc tháng 9 năm 2026)" };
            }
            else if (r.Metric == "BIRTHDAY" && !r.Time.BirthdayMonth.HasValue && r.Time.AnchorKind != "INVALID")
                r.Clarification = new ClarificationPrompt { Field = "BIRTHDAY_MONTH", Question = "Bạn muốn tra cứu sinh nhật tháng mấy?" };
            if (r.Clarification == null) return false;
            r.SupportStatus = QuerySupportStatus.NeedsClarification; return true;
        }
        public bool ApplyOptionSelection(QueryUnderstandingResult r, string token, string field)
        {
            if (r?.Clarification == null || r.Clarification.Field != field) return false;
            var option = r.Clarification.Options.SingleOrDefault(o => string.Equals(o.Token, token, StringComparison.Ordinal));
            if (option == null) return false;
            if (field == "MANV" || field == "DEPARTMENT")
            {
                if (!int.TryParse(option.Value, out var id)) return false;
                var e = r.Entities.FirstOrDefault(x => x.EntityType == (field == "MANV" ? "EMPLOYEE" : "DEPARTMENT") && x.Status == "AMBIGUOUS");
                var candidate = e?.Candidates.SingleOrDefault(c => c.Id == id);
                if (candidate == null) return false;
                e.ResolvedId = id; e.ResolvedName = candidate.Name; e.DepartmentId = candidate.DepartmentId; e.CompanyCode = candidate.CompanyCode; e.Status = "RESOLVED";
            }
            else if (field == "OPERATION" && (option.Value == "VIEW" || option.Value == "SUM")) r.Operation = option.Value;
            else if (field == "METRIC" && (option.Value == "THUCLANH" || option.Value == "LUONG_CONG_THUCTE")) r.Metric = option.Value;
            else return false;
            r.SupportStatus = QuerySupportStatus.Supported; r.Clarification = null; return true;
        }
    }
}