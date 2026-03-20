namespace Bu.Services.AI_Services.Core
{
    public class AiRouterService
    {
        public string DetectIntent(string q)
        {
            q = q.ToLower();

            if (q.Contains("nhân viên") || q.Contains("tên") || q.Contains("mã"))
                return "EMPLOYEE";

            if (q.Contains("chấm công"))
                return "ATTENDANCE";

            if (q.Contains("tăng ca"))
                return "OVERTIME";

            if (q.Contains("bảo hiểm"))
                return "INSURANCE";

            if (q.Contains("ứng lương"))
                return "ADVANCE";

            if (q.Contains("phụ cấp"))
                return "ALLOWANCE";

            return "GENERAL";
        }
    }
}