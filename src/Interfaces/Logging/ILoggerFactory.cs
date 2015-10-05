using System;
using System.Collections.Generic;

namespace Nybus.Logging
{
    public interface ILoggerFactory : IDisposable
    {
        ILogger CreateLogger(string categoryName);

        void AddProvider(ILoggerProvider provider);

        IReadOnlyList<ILoggerProvider> GetProviders();

        LogLevel MinimumLevel { get; set; }
    }
}