using System.Threading.Tasks;

namespace Nybus.Logging
{
    public class NopLogger : LoggerBase
    {
        public override Task LogAsync(LogLevel level, string message, object data = null)
        {
            return Task.FromResult(0);
        }
    }
}