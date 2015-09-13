using System.Threading.Tasks;

namespace Nybus.Logging
{
    public interface ILogger
    {
        Task LogAsync(LogLevel level, string message, object data = null);

        void Log(LogLevel level, string message, object data = null);
    }
}