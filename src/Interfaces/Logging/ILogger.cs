using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Nybus.Logging
{
    public interface ILoggerProvider : IDisposable
    {
        ILogger CreateLogger(string name);
    }

    public interface ILogger
    {
        void Log(LogLevel level, IDictionary<string, object> state, Exception exception);

        bool IsEnabled(LogLevel level);
    }
}