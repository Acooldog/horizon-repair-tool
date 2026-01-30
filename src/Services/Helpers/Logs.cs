using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace test.src.Services.Helpers
{
    public class Logs
    {
        public static void SetupTraceListener()
        {
            // 创建日志目录
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Console.WriteLine(logDir);
            Directory.CreateDirectory(logDir);

            string logFile = Path.Combine(logDir,
                                         $"app_{DateTime.Now:yyyyMMdd}.log");

            // 添加文件监听器
            Trace.Listeners.Add(new TextWriterTraceListener(logFile));

            // 同时输出到控制台
            Trace.Listeners.Add(new ConsoleTraceListener());

            // 自动刷新
            Trace.AutoFlush = true;

            Trace.WriteLine("=".PadRight(50, '='));
            Trace.WriteLine($"应用程序启动 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Trace.WriteLine("=".PadRight(50, '='));
        }

        public static void LogInfo(string message)
        {
            Trace.TraceInformation($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
        }

        public static void LogWarning(string message)
        {
            Trace.TraceWarning($"[WARN] {DateTime.Now:HH:mm:ss} - {message}");
        }

        public static void LogError(string message, Exception? ex = null)
        {
            if (ex != null)
            {
                Trace.TraceError($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}\n" +
                               $"异常: {ex.GetType().Name}: {ex.Message}\n" +
                               $"堆栈: {ex.StackTrace}");
            }
            else
            {
                Trace.TraceError($"[ERROR] {DateTime.Now:HH:mm:ss} - {message}");
            }
        }
    }
}
