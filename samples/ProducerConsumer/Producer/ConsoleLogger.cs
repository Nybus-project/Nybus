using System;
using System.Threading.Tasks;
using Nybus.Logging;

namespace Producer
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