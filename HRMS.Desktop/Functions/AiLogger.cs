using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace QLyNSu.Functions
{
    public enum AiLogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }

    public class AiLogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public AiLogLevel Level { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string ExceptionMessage { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level.ToString().ToUpperInvariant()}] [{Category}] {Message}");
            if (!string.IsNullOrEmpty(Details))
            {
                sb.AppendLine();
                sb.Append($"   Chi tiết: {Details}");
            }
            if (!string.IsNullOrEmpty(ExceptionMessage))
            {
                sb.AppendLine();
                sb.Append($"   Lỗi ngoại lệ: {ExceptionMessage}");
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Hệ thống ghi nhật ký (Logger) tập trung cho Trợ lý AI và API Client trên Desktop.
    /// Hỗ trợ in trực tiếp ra Visual Studio Output (Debug), file vật lý và in-memory buffer cho UI.
    /// </summary>
    public static class AiLogger
    {
        private static readonly object _syncLock = new object();
        private static readonly List<AiLogEntry> _logs = new List<AiLogEntry>();
        private const int MaxMemoryLogEntries = 1000;
        private static readonly string _logDirectory;
        private static readonly string _logFilePath;

        public static event Action<AiLogEntry> LogAppended;

        static AiLogger()
        {
            try
            {
                _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }
                _logFilePath = Path.Combine(_logDirectory, "ai_copilot.log");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AiLogger INIT ERROR]: {ex.Message}");
            }
        }

        public static string LogFilePath => _logFilePath;

        public static void Log(AiLogLevel level, string category, string message, string details = null, Exception ex = null)
        {
            var entry = new AiLogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Category = category ?? "AI",
                Message = message ?? string.Empty,
                Details = details,
                ExceptionMessage = ex != null ? $"{ex.GetType().Name}: {ex.Message}\nStackTrace:\n{ex.StackTrace}" : null
            };

            // 1. In ra Visual Studio Debug / Output Window
            string logLine = $">>> [AI LOG] [{entry.Timestamp:HH:mm:ss.fff}] [{entry.Level.ToString().ToUpperInvariant()}] [{entry.Category}] {entry.Message}";
            // Debug and Trace share listeners on .NET Framework; write one complete entry.
            var output = new StringBuilder(logLine);
            if (!string.IsNullOrEmpty(entry.Details)) output.AppendLine().Append("    -> Details: ").Append(entry.Details);
            if (!string.IsNullOrEmpty(entry.ExceptionMessage)) output.AppendLine().Append("    -> Exception: ").Append(entry.ExceptionMessage);
            try { Trace.WriteLine(output.ToString()); } catch { }

            // 2. Ghi vào In-Memory Buffer
            lock (_syncLock)
            {
                if (_logs.Count >= MaxMemoryLogEntries)
                {
                    _logs.RemoveRange(0, 100); // Drop 100 cũ nhất
                }
                _logs.Add(entry);
            }

            // 3. Ghi vào File Vật Lý
            AppendToFile(entry);

            // 4. Kích hoạt Event thông báo cho UI
            try
            {
                LogAppended?.Invoke(entry);
            }
            catch { }
        }

        public static void DebugLog(string category, string message, string details = null)
        {
            Log(AiLogLevel.Debug, category, message, details);
        }

        public static void Info(string category, string message, string details = null)
        {
            Log(AiLogLevel.Info, category, message, details);
        }

        public static void Warn(string category, string message, string details = null)
        {
            Log(AiLogLevel.Warn, category, message, details);
        }

        public static void Error(string category, string message, Exception ex = null, string details = null)
        {
            Log(AiLogLevel.Error, category, message, details, ex);
        }

        public static List<AiLogEntry> GetAllLogs()
        {
            lock (_syncLock)
            {
                return _logs.ToList();
            }
        }

        public static void Clear()
        {
            lock (_syncLock)
            {
                _logs.Clear();
            }
        }

        private static void AppendToFile(AiLogEntry entry)
        {
            if (string.IsNullOrEmpty(_logFilePath)) return;
            try
            {
                lock (_syncLock)
                {
                    File.AppendAllText(_logFilePath, entry.ToString() + Environment.NewLine + new string('-', 80) + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch { }
        }
    }
}
