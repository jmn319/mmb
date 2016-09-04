using System;
using System.IO;

namespace mmb
{
    class Log
    {
        public static void AppendToLog(string logMessage, string fileLocation, int memLeakPrevent = 0)
        {
            //Arbitrarily set a bound so that a memory leak is not created
            if (memLeakPrevent < 20)
            {
                try
                {
                    using (StreamWriter w = File.AppendText(fileLocation))
                    {
                        w.WriteLine("\r\n{0} {1}", DateTime.Now.ToLongTimeString(),
                            DateTime.Now.ToLongDateString());
                        w.WriteLine("  : {0}", logMessage);
                    }
                }
                catch (Exception e)
                { AppendToLog(logMessage, fileLocation, memLeakPrevent++); }
            } 
        }
    }
}
