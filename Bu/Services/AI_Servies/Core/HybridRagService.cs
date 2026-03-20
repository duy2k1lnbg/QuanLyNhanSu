using Bu.Services.AI_Servies.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.Services.AI_Servies
{
    public class HybridRagService
    {
        private readonly AiRouterService _router;
        private readonly EmployeeRepository _emp;
        private readonly AttendanceRepository _att;
        private readonly InsuranceRepository _ins;
        private readonly VectorService _vec;
        private readonly OllamaService _llm;
        private readonly QueryParser _parser;

        public HybridRagService()
        {
            _router = new AiRouterService();
            _emp = new EmployeeRepository();
            _att = new AttendanceRepository();
            _ins = new InsuranceRepository();
            _vec = new VectorService();
            _llm = new OllamaService();
            _parser = new QueryParser();

            SeedVector();
        }

        private void SeedVector()
        {
            _vec.Add("Nghỉ phép tối đa 12 ngày mỗi năm");
            _vec.Add("Giờ làm việc từ 8h đến 17h");
        }

        // ================= MAIN =================
        public async Task<string> Ask(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return "Vui lòng nhập câu hỏi.";

            q = q.Trim();

            string intent = await _router.DetectIntent(q);

            // ================= PARSE =================
            var name = _parser.ExtractName(q);
            var manv = _parser.ExtractManv(q);

            bool isCount = _parser.IsCount(q);
            bool isList = _parser.IsList(q);
            bool isDetail = _parser.IsDetail(q);

            // ================= EMPLOYEE =================
            //if (intent == "EMPLOYEE")
            //{
            //    if (isCount)
            //    {
            //        int total = _emp.Count();
            //        return "Tổng số nhân viên: " + total;
            //    }

            //    if (isList)
            //    {
            //        return string.Join("\n", _emp.GetTop(5));
            //    }

            //    var data = _emp.Search(q);

            //    if (!data.Any())
            //        return "Không tìm thấy nhân viên.";

            //    return string.Join("\n", data);
            //}
            var dept = _parser.ExtractDept(q);
            if (intent == "EMPLOYEE")
            {
                if (_parser.IsCount(q))
                {
                    var count = _emp.Search(name, dept).Count;
                    return "Tổng số nhân viên: " + count;
                }

                if (_parser.IsList(q))
                {
                    var list = _emp.Search(name, dept);
                    return list.Any() ? string.Join("\n", list) : "Không có nhân viên nào phù hợp";
                }

                var data = _emp.Search(name, dept);
                return data.Any() ? string.Join("\n", data)
                                  : (!string.IsNullOrWhiteSpace(name)
                                     ? $"Không tìm thấy nhân viên tên {name}"
                                     : "Không tìm thấy nhân viên nào phù hợp");
            }

            // ================= ATTENDANCE =================
            if (intent == "ATTENDANCE")
            {
                var data = _att.Search(q);

                if (!data.Any())
                    return "Không có dữ liệu chấm công.";

                return string.Join("\n", data);
            }

            // ================= INSURANCE =================
            if (intent == "INSURANCE")
            {
                var data = _ins.Search(q);

                if (!data.Any())
                    return "Không có dữ liệu bảo hiểm.";

                return string.Join("\n", data);
            }

            // ================= GENERAL (VECTOR + AI) =================
            var vecData = _vec.Search(q);

            if (vecData.Any())
            {
                return string.Join("\n", vecData);
            }

            // fallback dùng AI
            return await _llm.Ask(BuildPrompt(q));
        }

        // ================= PROMPT =================
        private string BuildPrompt(string question)
        {
            return
                "Bạn là AI quản lý nhân sự.\n" +
                "Trả lời ngắn gọn, đúng trọng tâm, bằng tiếng Việt.\n\n" +
                "Câu hỏi: " + question + "\n\n" +
                "Trả lời:";
        }
    }
}