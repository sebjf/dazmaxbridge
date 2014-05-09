using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Autodesk.Max;
using System.Runtime.InteropServices;

namespace MaxManagedBridge
{
    public enum LogLevel : int
    {
        All = 0,
        Debug = 1,
        Information = 2,
        Error = 3
    }

    public class Log
    {
        private const uint SYSLOG_ERROR = 0x00000001;
        private const uint SYSLOG_WARN = 0x00000002;
        private const uint SYSLOG_INFO = 0x00000004;
        private const uint SYSLOG_DEBUG = 0x00000008;
        private const uint SYSLOG_BROADCAST = 0x00010000;
        private const uint SYSLOG_MR = 0x00020000;

        public static bool EnableLog { get; set; }
        public static ILogSys MaxLogger { get; set; }

        public static LogLevel LogLevel { get { return logLevel; } set { logLevel = value; } }
        private static LogLevel logLevel = MaxManagedBridge.LogLevel.Information;

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        public static void Add(string message, LogLevel level)
        {
            if (EnableLog && (MaxLogger != null) && level >= LogLevel)
            {
                MaxLogger.LogEntry(SYSLOG_INFO, false, "DazMaxBridge", message + "( " + GetElapsedTime() + "s)\n");
            }
        }


        private static long lastTime = 0;
        private static long pcFrequency = 0;

        private static float GetElapsedTime()
        {
            if(pcFrequency == 0)
            {
                QueryPerformanceFrequency(out pcFrequency);
            }

            long currentTime = 0;
            QueryPerformanceCounter(out currentTime);

            float elapsed = 0;
            if (lastTime == 0)
            {
                elapsed = 0;
            }
            else
            {
                elapsed = currentTime - lastTime;
                elapsed = elapsed / pcFrequency;
            }

            lastTime = currentTime;

            return elapsed;
        }
    }
}
