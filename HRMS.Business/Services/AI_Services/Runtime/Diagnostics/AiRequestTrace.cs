using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bu.Services.AI_Services.Core
{
    public sealed class AiTraceEvent
    {
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public string Stage { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }

    // A request-local diagnostic scope. Public events contain metadata only.
    // SQL and authorized retrieval excerpts are written only to the server log.
    public sealed class AiRequestTrace : IDisposable
    {
        private static readonly AsyncLocal<AiRequestTrace> Active = new AsyncLocal<AiRequestTrace>();
        private static readonly object FileLock = new object();
        private readonly AiRequestTrace _previous;
        private readonly object _sync = new object();
        private bool _disposed;
        private static string _fallbackDirectory;
        public Action<List<AiTraceEvent>> OnCompleted { get; set; }
        public string RequestId { get; private set; }
        public List<AiTraceEvent> Events { get; } = new List<AiTraceEvent>();
        public static string LogFilePath => Path.Combine(Environment.GetEnvironmentVariable("HRMS_AI_LOG_DIRECTORY") ?? _fallbackDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "ai_logs"), "ai_pipeline.jsonl");

        private AiRequestTrace(string requestId) { RequestId = requestId; _previous = Active.Value; Active.Value = this; }
        public static AiRequestTrace Begin(string requestId) => new AiRequestTrace(requestId);

        public static void Record(string stage, string message, object details = null, object serverDetails = null)
        {
            var scope = Active.Value;
            if (scope == null) return;
            var entry = new AiTraceEvent {
                Stage = stage, Message = message,
                Details = details == null ? null : JsonConvert.SerializeObject(RedactObject(details))
            };
            lock (scope._sync) { if (!scope._disposed && scope.Events.Count < 64) scope.Events.Add(entry); }
            string line = JsonConvert.SerializeObject(new {
                requestId = scope.RequestId, entry.TimestampUtc, entry.Stage, entry.Message,
                details = details == null ? null : RedactObject(details),
                serverDetails = serverDetails == null ? null : RedactObject(serverDetails)
            });
            try { Trace.WriteLine("[AI_PIPELINE] " + line); } catch { }
            try {
                lock (FileLock) {
                    string path = LogFilePath;
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    if (File.Exists(path) && new FileInfo(path).Length > 5 * 1024 * 1024) {
                        string previous = path + ".1";
                        if (File.Exists(previous)) File.Delete(previous);
                        File.Move(path, previous);
                    }
                    File.AppendAllText(path, line + Environment.NewLine, new UTF8Encoding(false));
                }
            } catch (Exception ex) {
                try {
                    if (Environment.GetEnvironmentVariable("HRMS_AI_LOG_DIRECTORY") == null) {
                        _fallbackDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HRMS", "AiLogs");
                        lock (FileLock) {
                            Directory.CreateDirectory(_fallbackDirectory);
                            File.AppendAllText(LogFilePath, line + Environment.NewLine, new UTF8Encoding(false));
                        }
                    } else Trace.WriteLine("[AI_LOG_WRITE_FAILED] " + ex.GetType().Name);
                } catch { }
            }
        }

        public static JToken RedactObject(object value)
        {
            var token = value is JToken supplied ? supplied.DeepClone() : JToken.FromObject(value);
            Redact(token);
            return token;
        }
        private static void Redact(JToken token)
        {
            if (token is JObject obj) foreach (var prop in obj.Properties().ToArray()) {
                if (Regex.IsMatch(prop.Name, "password|matkhau|token|authorization|cookie|secret|proof|connectionstring", RegexOptions.IgnoreCase))
                    prop.Value = "[REDACTED]";
                else Redact(prop.Value);
            }
            else if (token is JArray arr) foreach (var item in arr) Redact(item);
            else if (token is JValue val && val.Type == JTokenType.String) val.Value = RedactText((string)val.Value);
        }
        public static string RedactText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            text = Regex.Replace(text, @"(?i)(bearer\s+)[\w.\-]+", "$1[REDACTED]");
            text = Regex.Replace(text, @"(?i)((?:password|mật khẩu|matkhau|token|secret|proof|connectionstring)\s*[:=]\s*)[^\s,;]+", "$1[REDACTED]");
            text = Regex.Replace(text, @"\b[\w.+\-]+@[\w.\-]+\.[A-Za-z]{2,}\b", "[EMAIL]");
            text = Regex.Replace(text, @"(?<!\d)(?:\+84|0)\d{8,10}(?!\d)", "[PHONE]");
            return text.Length > 8000 ? text.Substring(0, 8000) + " [TRUNCATED]" : text;
        }
        public static string PromptStructure(string prompt)
        {
            if (string.IsNullOrEmpty(prompt)) return "";
            var parts = Regex.Split(prompt, @"(?m)(^\[[^\]\r\n]+\]\s*$)");
            return string.Join("\n", parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p =>
                Regex.IsMatch(p.Trim(), @"^\[[^\]]+\]$") ? p.Trim() : "[CONTENT REDACTED; chars=" + p.Length + "]"));
        }
        public void Dispose() {
            List<AiTraceEvent> snapshot;
            lock (_sync) { _disposed = true; snapshot = Events.ToList(); }
            OnCompleted?.Invoke(snapshot);
            if (ReferenceEquals(Active.Value, this)) Active.Value = _previous;
        }
    }
}

