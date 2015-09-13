using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Nybus.Utils;

namespace Nybus.Logging
{
    public abstract class LoggerBase : ILogger
    {
        public Task LogAsync(LogLevel level, string message, object data = null, [CallerMemberName] string callerMemberName = null)
        {
            return LogEvent(level, message, data, callerMemberName);
        }

        public void Log(LogLevel level, string message, object data = null, [CallerMemberName] string callerMemberName = null)
        {
            LogEvent(level, message, data, callerMemberName).WaitAndUnwrapException();
        }

        protected abstract Task LogEvent(LogLevel level, string message, object data, string callerMemberName);
    }
}