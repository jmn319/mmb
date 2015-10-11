using System;
using System.IO;

namespace mmb
{
    class Log
    {
        public static void AppendToLog(string logMessage, string fileLocation)
        {
            using (StreamWriter w = File.AppendText(fileLocation))
            {
                w.WriteLine("\r\n{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
                w.WriteLine("  :{0}", logMessage);
            }
        }
    }
}
