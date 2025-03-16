using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailSenderH.DataAccess;

namespace MailSenderH.Logging
{
    public static class FileLogger
    {
        private static readonly string BaseLogPath = ConfigurationManager.AppSettings["LogBasePath"]; // 기본 로그 경로
        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public static void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        private static void WriteLog(string logType, string message)
        {
            try
            {
                DateTime time = DateTime.Now;

                string yearMonth = time.ToString("yyyyMM");
                string logFileName = time.ToString("yyyyMMdd") + ".txt";

                string logDirectory = Path.Combine(BaseLogPath, yearMonth);
                string logFilePath = Path.Combine(logDirectory, logFileName);

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string logMessage = $"[{time:yyyy-MM-dd HH:mm:ss}] [{logType}] {message}";
                // 파일에 로그 기록
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"로그 기록 중 오류 발생: {ex.Message}");
            }
        }
    }
}
