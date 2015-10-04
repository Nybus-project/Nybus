using System;
using NLog;

namespace Nybus.Logging
{
    public class NLogLoggerProvider : ILoggerProvider
    {
        private readonly LogFactory _logFactory;
        private bool _disposed = false;

        public NLogLoggerProvider(LogFactory logFactory)
        {
            if (logFactory == null) throw new ArgumentNullException(nameof(logFactory));
            _logFactory = logFactory;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _logFactory.Flush();
                _logFactory.Dispose();
                _disposed = true;
            }
        }

        public ILogger CreateLogger(string name)
        {
            return new NLogLogger(_logFactory.GetLogger(name));
        }
    }
}