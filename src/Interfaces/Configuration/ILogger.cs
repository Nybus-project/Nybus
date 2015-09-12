using System.Threading.Tasks;

namespace Nybus.Configuration
{
    public interface ILogger
    {
        Task LogAsync(LogLevel level, string message, object data = null);

        void Log(LogLevel level, string message, object data = null);
    }

    public enum LogLevel
    {
        Trace = 1,
        Debug = 2,
        Info = 3,
        Warn = 4,
        Error = 5,
        Fatal = 6
    }

    public class NopLogger : ILogger
    {
        public Task LogAsync(LogLevel level, string message, object data = null)
        {
            return Task.FromResult(0);
        }

        public void Log(LogLevel level, string message, object data = null)
        {
            return;
        }
    }
}