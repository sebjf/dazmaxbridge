using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Autodesk.Max;
using System.Runtime.InteropServices;

namespace MaxManagedBridge
{

    public class Log
    {
        private const uint SYSLOG_ERROR = 0x00000001;
        private const uint SYSLOG_WARN = 0x00000002;
        private const uint SYSLOG_INFO = 0x00000004;
        private const uint SYSLOG_DEBUG = 0x00000008;
        private const uint SYSLOG_BROADCAST = 0x00010000;
        private const uint SYSLOG_MR = 0x00020000;

        public static bool EnableLog { get; set; }
        public static ILogSys logger { get; set; }

        public static void Add(string message)
        {
            if (EnableLog && (logger != null))
            {
                logger.LogEntry(SYSLOG_INFO, false, "DazMaxBridge", message + "\n");
            }
        }
    }
}
