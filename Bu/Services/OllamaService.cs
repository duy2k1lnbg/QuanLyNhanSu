using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class OllamaService
{
    private readonly HttpClient _client = new HttpClient();

    public async Task<string> GenerateSqlAsync(string question)
    {
        _client.Timeout = TimeSpan.FromSeconds(120); // 2 phút
        string prompt = $@"
Bạn là chuyên gia Oracle.
Chỉ tạo câu SELECT.
Không dùng INSERT, UPDATE, DELETE.
Chỉ dùng các VIEW:
V_EMPLOYEE, V_DEPARTMENT, V_SALARY.
Luôn thêm FETCH FIRST 50 ROWS ONLY.

Câu hỏi:
{question}
";

        var body = new
        {
            model = "llama3-8b-4q",
            prompt = prompt,
            stream = false
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(body),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync(
            "http://localhost:11434/api/generate",
            content);

        var json = await response.Content.ReadAsStringAsync();

        dynamic result = JsonConvert.DeserializeObject(json);

        return result.response.ToString();
    }
}