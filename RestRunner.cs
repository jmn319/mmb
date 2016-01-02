using System;

namespace mmb
{
    public class RestRunner
    {
        //TODO: Add logging to file and decide whether or not to keep console logging
        //TODO: Add verbose logging to app.config that will log every call
        public static void InitRestThread()
        {
            var listeningOn = "http://*:26263/";
            var appHost = new AppHost();
            appHost.Init();
            appHost.Start(listeningOn);

            Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);
            Console.Read();
        }
    }
}
