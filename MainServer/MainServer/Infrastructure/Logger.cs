using System;

namespace MainServer.Infrastructure
{
    class Logger
    {
        public static void AddEvent(string theEvent)
        {
            Console.WriteLine(theEvent);
        }
    }
}
