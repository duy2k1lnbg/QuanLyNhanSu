using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bu.Services
{
    public class OllamaService
    {
        #region
        private readonly string _url = "http://localhost:11434/api/chat";

        public async Task<string> SendChatRequest(List<ChatMessage> history)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(3);
                var payload = new OllamaRequest { messages = history };
                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(_url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var resString = await response.Content.ReadAsStringAsync();
                        dynamic resJson = JsonConvert.DeserializeObject(resString);
                        return resJson.message.content;
                    }
                    return "Lỗi: Không thể nhận phản hồi từ AI.";
                }
                catch (Exception ex) { return "Lỗi kết nối: " + ex.Message; }
            }
        }
        #endregion
    }
}