using System.Collections.Generic;
using System.Linq;

namespace Nybus.Logging
{
    public class LoggerFactory : ILoggerFactory
    {
        public static readonly ILoggerFactory Default = new LoggerFactory();

        private readonly Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>();
        private ILoggerProvider[] _providers = new ILoggerProvider[0];
        private readonly object _sync = new object();
        private bool _disposed;

        public ILogger CraeteLogger(string categoryName)
        {
            Logger logger;

            lock (_sync)
            {
                if (!_loggers.TryGetValue(categoryName, out logger))
                {
                    logger = new Logger(this, categoryName);
                    _loggers[categoryName] = logger;
                }
            }

            return logger;
        }

        public void AddProvider(ILoggerProvider provider)
        {
            lock (_sync)
            {
                _providers = _providers.Concat(new[] {provider}).ToArray();
                foreach (var logger in _loggers)
                {
                    logger.Value.AddProvider(provider);
                }
            }
        }

        public IReadOnlyList<ILoggerProvider> GetProviders() => _providers;

        public LogLevel MinimumLevel { get; set; } = LogLevel.Verbose;

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var provider in _providers)
                {
                    try
                    {
                        provider.Dispose();
                    }
                    catch
                    {
                        
                    }
                }

                _disposed = true;
            }
        }
    }
}