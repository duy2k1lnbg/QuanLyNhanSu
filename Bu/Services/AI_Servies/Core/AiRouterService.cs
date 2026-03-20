using System.Threading.Tasks;

namespace Bu.Services.AI_Servies
{
    public class AiRouterService
    {
        public Task<string> DetectIntent(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return Task.FromResult("GENERAL");

            question = question.ToLower();

            // ===== EMPLOYEE =====
            if (ContainsAny(question, new[]
            {
                "nhân viên", "nv", "staff", "employee", "tên", "họ tên"
            }))
            {
                return Task.FromResult("EMPLOYEE");
            }

            // ===== ATTENDANCE =====
            if (ContainsAny(question, new[]
            {
                "chấm công", "attendance", "đi làm", "giờ vào", "giờ ra",
                "đi trễ", "muộn", "về sớm", "ngày công"
            }))
            {
                return Task.FromResult("ATTENDANCE");
            }

            // ===== INSURANCE =====
            if (ContainsAny(question, new[]
            {
                "bảo hiểm", "bh", "insurance", "số bh", "bhxh"
            }))
            {
                return Task.FromResult("INSURANCE");
            }

            // ===== DEPARTMENT =====
            if (ContainsAny(question, new[]
            {
                "phòng", "bộ phận", "department", "dept"
            }))
            {
                return Task.FromResult("DEPARTMENT");
            }

            // ===== COUNT (ưu tiên cao) =====
            if (ContainsAny(question, new[]
            {
                "bao nhiêu", "how many", "tổng", "count"
            }))
            {
                return Task.FromResult("COUNT");
            }

            // ===== LIST =====
            if (ContainsAny(question, new[]
            {
                "liệt kê", "list", "danh sách", "show"
            }))
            {
                return Task.FromResult("LIST");
            }

            // ===== FALLBACK =====
            return Task.FromResult("GENERAL");
        }

        // ================= HELPER =================
        private bool ContainsAny(string text, string[] keywords)
        {
            foreach (var k in keywords)
            {
                if (text.Contains(k))
                    return true;
            }
            return false;
        }
    }
}