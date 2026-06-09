using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Bu.Services.AI_Services.Interfaces;

namespace Bu.Services.AI_Services.Core
{
    public class JsonPromptManager : IPromptManager
    {
        private readonly Dictionary<string, string> _prompts;

        public JsonPromptManager()
        {
            _prompts = new Dictionary<string, string>();
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ai_prompts.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    _prompts = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) 
                               ?? new Dictionary<string, string>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PromptManager ERROR]: {ex.Message}");
            }
        }

        public string GetPrompt(string key)
        {
            if (_prompts.TryGetValue(key, out string val))
            {
                return val;
            }
            return string.Empty;
        }

        public string GetSchema()
        {
            return GetPrompt("Schema");
        }
    }
}
