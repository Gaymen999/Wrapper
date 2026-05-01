using System;
using System.IO;

namespace GameLauncherApp.Core
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_execution.log");
        private static readonly object LockObject = new object();

        public static void LogInfo(string message) => WriteLog("INFO", message);
        public static void LogWarning(string message) => WriteLog("WARN", message);
        public static void LogError(string message, Exception ex = null) 
        {
            string fullMessage = ex != null ? $"{message} | Exception: {ex.Message}\nStacktrace: {ex.StackTrace}" : message;
            WriteLog("ERROR", fullMessage);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                lock (LockObject)
                {
                    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(LogFilePath, logEntry);
                }
            }
            catch
            {
                // Fallback to console if file logging fails to avoid crashing
                Console.WriteLine($"Logging failed: {message}");
            }
        }
    }
}
