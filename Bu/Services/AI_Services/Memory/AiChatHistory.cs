using System.Collections.Generic;
using System.Text;

namespace Bu.Services.AI_Services.Memory
{
    public class ChatMessage
    {
        public string Role { get; set; } // "User" hoặc "AI"
        public string Content { get; set; }
    }

    public class AiChatHistory
    {
        private List<ChatMessage> _messages = new List<ChatMessage>();
        private readonly int _maxHistory = 10;

        public void AddMessage(string role, string content)
        {
            _messages.Add(new ChatMessage { Role = role, Content = content });
            if (_messages.Count > _maxHistory) _messages.RemoveAt(0);
        }

        public string GetHistoryString()
        {
            var sb = new StringBuilder();
            foreach (var msg in _messages)
                sb.AppendLine($"{msg.Role}: {msg.Content}");
            return sb.ToString();
        }

        public void Clear() => _messages.Clear();
    }
}