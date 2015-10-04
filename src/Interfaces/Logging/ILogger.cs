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
        //Task LogAsync(LogLevel level, object state, Exception exception, Func<object, Exception, string> formatter);

        void Log(LogLevel level, IReadOnlyDictionary<string, object> state, Exception exception, MessageFormatter formatter);

        bool IsEnabled(LogLevel level);
    }

    public delegate string MessageFormatter(IReadOnlyDictionary<string, object> state, Exception exception);
}