using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NLog;
using System.Threading.Tasks;
using Nybus.Utils;

namespace Nybus.Logging
{
    public class NLogLogger : ILogger
    {
        private readonly NLog.ILogger _logger;

        public NLogLogger(NLog.ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _logger = logger;

        }

        public void Log(LogLevel level, IDictionary<string, object> state, Exception exception)
        {
            var nlogLogLevel = GetLogLevel(level);

            string message = null;

            if (state.ContainsKey("message"))
            {
                message = (string)state["message"];
            }

            var eventInfo = CreateLogEventInfo(nlogLogLevel, message, state, exception);

            _logger.Log(eventInfo);
        }

        private LogEventInfo CreateLogEventInfo(NLog.LogLevel level, string message, IDictionary<string, object> dictionary, Exception exception)
        {
            LogEventInfo logEvent = new LogEventInfo(level, _logger.Name, message);

            foreach (var item in dictionary)
            {
                logEvent.Properties[item.Key] = item.Value;
            }

            if (exception != null)
            {
                logEvent.Properties["error-source"] = exception.Source;
                logEvent.Properties["error-class"] = exception.TargetSite.DeclaringType.FullName;
                logEvent.Properties["error-method"] = exception.TargetSite.Name;
                logEvent.Properties["error-message"] = exception.Message;

                if (exception.InnerException != null)
                {
                    logEvent.Properties["inner-error-message"] = exception.InnerException.Message;
                }
            }

            return logEvent;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(GetLogLevel(logLevel));
        }

        private global::NLog.LogLevel GetLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Verbose: return global::NLog.LogLevel.Trace;
                case LogLevel.Debug: return global::NLog.LogLevel.Debug;
                case LogLevel.Information: return global::NLog.LogLevel.Info;
                case LogLevel.Warning: return global::NLog.LogLevel.Warn;
                case LogLevel.Error: return global::NLog.LogLevel.Error;
                case LogLevel.Critical: return global::NLog.LogLevel.Fatal;
            }
            return global::NLog.LogLevel.Debug;
        }
    }
}
