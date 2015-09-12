using System;
using System.Threading.Tasks;
using Nybus.Configuration;

namespace Producer
{
    public class ConsoleLogger : ILogger
    {
        public Task LogAsync(LogLevel level, string message, object data = null)
        {
            Log(level, message, data);
            return Task.FromResult(0);
        }

        public void Log(LogLevel level, string message, object data = null)
        {
            Console.WriteLine($"\t{level:G} {message}");
        }
    }
}