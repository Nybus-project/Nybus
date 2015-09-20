using System;
using System.Threading.Tasks;
using Nybus.Configuration;
using Nybus.Logging;

namespace Consumer
{
    public class ConsoleLogger : LoggerBase
    {
        protected override Task LogEvent(LogLevel level, string message, object data, string callerMemberName)
        {
            Console.WriteLine($"\t{level:G} {message}");

            return Task.CompletedTask;
        }
    }
}