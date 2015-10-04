using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Nybus.Utils;

namespace Nybus.Logging
{
    public static class LoggerExtensions
    {
        public static void Log(this ILogger logger, LogLevel level, string message)
        {
            IReadOnlyDictionary<string, object> state = new Dictionary<string, object>
            {
                ["message"] = message
            };

            logger.Log(level, state, null, ((pairs, exception) => message));
        }

        public static void Log<T>(this ILogger logger, LogLevel level, T state, Func<T, string> formatter = null)
        {
            formatter = formatter ?? DefaultFormatter;

            var dictionary = ObjectToDictionary.Convert(state);

            logger.Log(level, dictionary, null, ((pairs, exception) => formatter(state)));
        }

        public static void Log<T>(this ILogger logger, LogLevel level, T state, Exception error, Func<T, string> formatter = null)
        {
            formatter = formatter ?? DefaultFormatter;

            var dictionary = ObjectToDictionary.Convert(state);

            logger.Log(level, dictionary, error, (pairs, exception) => $"{formatter(state)}{Environment.NewLine}{error}");
        }

        #region Verbose

        public static void LogVerbose(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Verbose, message);
        }

        public static void LogVerbose<T>(this ILogger logger, T state, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Verbose, state, formatter);
        }

        public static void LogVerbose(this ILogger logger, Exception error)
        {
            logger.Log(LogLevel.Verbose, null, error, (state, exception) => exception.ToString());
        }

        public static void LogVerbose<T>(this ILogger logger, T state, Exception error, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Verbose, state, error, formatter);
        }

        public static void LogVerbose(this ILogger logger, string message, Exception error)
        {
            logger.Log(LogLevel.Verbose, new { message = message }, error);
        }

        #endregion

        #region Debug

        public static void LogDebug(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Debug, message);
        }

        public static void LogDebug<T>(this ILogger logger, T state, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Debug, state, formatter);
        }

        public static void LogDebug(this ILogger logger, Exception error)
        {
            logger.Log(LogLevel.Debug, null, error, (state, exception) => exception.ToString());
        }

        public static void LogDebug<T>(this ILogger logger, T state, Exception error, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Debug, state, error, formatter);
        }

        public static void LogDebug(this ILogger logger, string message, Exception error)
        {
            logger.Log(LogLevel.Debug, new { message = message }, error);
        }

        #endregion

        #region Information

        public static void LogInformation(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Information, message);
        }

        public static void LogInformation<T>(this ILogger logger, T state, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Information, state, formatter);
        }

        public static void LogInformation(this ILogger logger, Exception error)
        {
            logger.Log(LogLevel.Information, null, error, (state, exception) => exception.ToString());
        }

        public static void LogInformation<T>(this ILogger logger, T state, Exception error, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Information, state, error, formatter);
        }

        public static void LogInformation(this ILogger logger, string message, Exception error)
        {
            logger.Log(LogLevel.Information, new { message = message }, error);
        }

        #endregion

        #region Warning

        public static void LogWarning(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Warning, message);
        }

        public static void LogWarning<T>(this ILogger logger, T state, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Warning, state, formatter);
        }

        public static void LogWarning(this ILogger logger, Exception error)
        {
            logger.Log(LogLevel.Warning, null, error, (state, exception) => exception.ToString());
        }

        public static void LogWarning<T>(this ILogger logger, T state, Exception error, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Warning, state, error, formatter);
        }

        public static void LogWarning(this ILogger logger, string message, Exception error)
        {
            logger.Log(LogLevel.Warning, new { message = message }, error);
        }

        #endregion

        #region Error

        public static void LogError(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Error, message);
        }

        public static void LogError<T>(this ILogger logger, T state, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Error, state, formatter);
        }

        public static void LogError(this ILogger logger, Exception error)
        {
            logger.Log(LogLevel.Error, null, error, (state, exception) => exception.ToString());
        }

        public static void LogError<T>(this ILogger logger, T state, Exception error, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Error, state, error, formatter);
        }

        public static void LogError(this ILogger logger, string message, Exception error)
        {
            logger.Log(LogLevel.Error, new { message = message }, error);
        }

        #endregion

        #region Critical

        public static void LogCritical(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Critical, message);
        }

        public static void LogCritical<T>(this ILogger logger, T state, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Critical, state, formatter);
        }

        public static void LogCritical(this ILogger logger, Exception error)
        {
            logger.Log(LogLevel.Critical, null, error, (state, exception) => exception.ToString());
        }

        public static void LogCritical<T>(this ILogger logger, T state, Exception error, Func<T, string> formatter = null)
        {
            logger.Log(LogLevel.Critical, state, error, formatter);
        }

        public static void LogCritical(this ILogger logger, string message, Exception error)
        {
            logger.Log(LogLevel.Critical, new {message = message}, error);
        }

        #endregion

        private static string DefaultFormatter<T>(T state)
        {
            return DefaultFormatter(ObjectToDictionary.Convert(state), null);
        }

        private static string DefaultFormatter(IReadOnlyDictionary<string, object> state, Exception error)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in state)
            {
                builder.AppendFormat("{0}={1};", item.Key, item.Value);
            }

            return builder.ToString();
        }

        /*
        private static string MessageFormatter(object state, Exception error)
        {
            if (state == null && error == null)
            {
                throw new InvalidOperationException("No message or exception details were found to create a message for the log.");
            }

            if (state == null)
            {
                return error.ToString();
            }

            if (error == null)
            {
                return state.ToString();
            }

            return string.Format(CultureInfo.CurrentCulture, "{0}{1}{2}", state, Environment.NewLine, error);
        }
        */
    }
}