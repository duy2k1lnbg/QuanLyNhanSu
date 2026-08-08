using Bu.Services.AI_Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bu.Services.AI_Services.Core
{
    public class AiRouterService
    {
        private readonly ILlmService _ollama;

        public AiRouterService(ILlmService ollama)
        {
            _ollama = ollama;
        }

        public async Task<string> DetectIntent(string q)
        {
            if (string.IsNullOrWhiteSpace(q)) return "GENERAL";

            q = q.ToLower().Trim();

            string[] helloWords = { "hi", "hello", "xin chào", "chào", "hey", "chào bạn", "có ai ở đó không" };
            if (helloWords.Any(w => q == w || q.StartsWith(w + " ")))
            {
                Debug.WriteLine($">>> RULE BASED: Greeting detected");
                return "GENERAL";
            }

            // Rule-based: Nhận diện nếu người dùng chỉ gõ một cái tên (ví dụ: "Trần Thanh Tâm")
            var wordCount = q.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount >= 2 && wordCount <= 6 && System.Text.RegularExpressions.Regex.IsMatch(q, @"^[\p{L}\s]+$"))
            {
                Debug.WriteLine($">>> RULE BASED: Name detected, routed to EMPLOYEE");
                return "EMPLOYEE";
            }

            return await AskAiToRoute(q);
        }

        private async Task<string> AskAiToRoute(string q)
        {
            string prompt = $@"
Phân loại câu hỏi sau vào ĐÚNG 1 NHÃN duy nhất: EMPLOYEE, ATTENDANCE, OVERTIME, INSURANCE, ADVANCE, ALLOWANCE, GENERAL.
Chỉ in ra tên nhãn, không giải thích.

Câu hỏi: ""{q}""
Nhãn:";

            Debug.WriteLine($">>> AI ROUTER PROMPT:\n{prompt}");

            var result = await _ollama.AskIntent(prompt);
            string intent = result.ToUpper().Trim().Replace(".", "");

            Debug.WriteLine($">>> AI ROUTER RESULT: {intent}");

            string[] validIntents = { "EMPLOYEE", "ATTENDANCE", "OVERTIME", "INSURANCE", "ADVANCE", "ALLOWANCE" };
            return validIntents.Contains(intent) ? intent : "GENERAL";
        }
    }
}