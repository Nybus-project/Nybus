using System.Threading.Tasks;

namespace Nybus.Logging
{
    public class NopLogger : LoggerBase
    {
        protected override Task LogEvent(LogLevel level, string message, object data, string callerMemberName)
        {
            return Task.FromResult(0);
        }
    }
}