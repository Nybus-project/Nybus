using System.Threading.Tasks;
using Nybus.Utils;

namespace Nybus.Logging
{
    public abstract class LoggerBase : ILogger
    {
        public abstract Task LogAsync(LogLevel level, string message, object data = null);

        public void Log(LogLevel level, string message, object data = null)
        {
            LogAsync(level, message, data).WaitAndUnwrapException();
        }
    }
}