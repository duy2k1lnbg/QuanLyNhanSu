using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.Services
{
    #region
    public class ChatMessage
    {
        public string role { get; set; } // system, user, assistant
        public string content { get; set; }
    }
    public class OllamaRequest
    {
        public string model { get; set; } = "llama3:8b-instruct-q4_1";
        public List<ChatMessage> messages { get; set; }
        public bool stream { get; set; } = false;
    }

    public class QueryIntent
    {
        public string Intent { get; set; }
        public string TargetName { get; set; }
        public bool IsDatabaseQuery { get; set; }
    }
    #endregion
}
