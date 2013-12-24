using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MaxBridgeUtility.Logger
{
    public class Log
    {
        public static void Add(String msg)
        {
            Instance.messages.Add(DateTime.Now.ToString() + ": " + msg);
        }

        private static Log Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Log();
                }
                return instance;
            }
        }

        private static Log instance;

        private List<string> messages = new List<string>();

        private void Write()
        {
            using (TextWriter tw = new StreamWriter("D:\\DazMaxUtilDebug.txt"))
            {
                foreach (var s in messages)
                {
                    tw.WriteLine(s);
                }
            }
        }

    }
}
