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
        public const string MessageKey = "message";

        public static void Log<T>(this ILogger logger, LogLevel level, T state, Exception error, Func<T, Exception, string> messageFormatter)
        {
            var dictionary = ObjectToDictionary.Convert(state);

            if (messageFormatter != null)
            {
                var formattedMessage = messageFormatter(state, error);

                dictionary[MessageKey] = formattedMessage;
            }

            logger.Log(level, dictionary, error);
        }

        public static void Log(this ILogger logger, LogLevel level, string message)
        {
            var state = new { message };

            Log(logger, level, state, null, null);
        }

        public static void Log(this ILogger logger, LogLevel level, Exception error)
        {
            var state = new {};

            Log(logger, level, state, error, (a, e) => e.ToString());
        }

        public static void Log<T>(this ILogger logger, LogLevel level, T state, Func<T, string> messageFormatter)
        {
            if (messageFormatter == null)
            {
                Log(logger, level, state, null, null);
            }
            else
            {
                Log(logger, level, state, null, (arg, exception) => messageFormatter(arg));
            }
        }

        #region Verbose

        public static void LogVerbose(this ILogger logger, string message)
        {
            Log(logger, LogLevel.Verbose, message);
        }

        public static void LogVerbose<T>(this ILogger logger, T state, Func<T, string> messageFormatter)
        {
            Log(logger, LogLevel.Verbose, state, messageFormatter);
        }

        public static void LogVerbose(this ILogger logger, Exception error)
        {
            Log(logger, LogLevel.Verbose, error);
        }

        public static void LogVerbose<T>(this ILogger logger, T state, Exception error, Func<T, Exception, string> messageFormatter)
        {
            Log(logger, LogLevel.Verbose, state, error, messageFormatter);
        }

        public static void LogVerbose(this ILogger logger, string message, Exception error)
        {
            Log(logger, LogLevel.Verbose, new {message}, error, (a, e) => a.message);
        }

        #endregion

        #region Debug

        public static void LogDebug(this ILogger logger, string message)
        {
            Log(logger, LogLevel.Debug, message);
        }

        public static void LogDebug<T>(this ILogger logger, T state, Func<T, string> messageFormatter)
        {
            Log(logger, LogLevel.Debug, state, messageFormatter);
        }

        public static void LogDebug(this ILogger logger, Exception error)
        {
            Log(logger, LogLevel.Debug, error);
        }

        public static void LogDebug<T>(this ILogger logger, T state, Exception error, Func<T, Exception, string> messageFormatter)
        {
            Log(logger, LogLevel.Debug, state, error, messageFormatter);
        }

        public static void LogDebug(this ILogger logger, string message, Exception error)
        {
            Log(logger, LogLevel.Debug, new { message }, error, (a, e) => a.message);
        }

        #endregion

        #region Information

        public static void LogInformation(this ILogger logger, string message)
        {
            Log(logger, LogLevel.Information, message);
        }

        public static void LogInformation<T>(this ILogger logger, T state, Func<T, string> messageFormatter = null)
        {
            Log(logger, LogLevel.Information, state, messageFormatter);
        }

        public static void LogInformation(this ILogger logger, Exception error)
        {
            Log(logger, LogLevel.Information, error);
        }

        public static void LogInformation<T>(this ILogger logger, T state, Exception error, Func<T, Exception, string> messageFormatter)
        {
            Log(logger, LogLevel.Information, state, error, messageFormatter);
        }

        public static void LogInformation(this ILogger logger, string message, Exception error)
        {
            Log(logger, LogLevel.Information, new { message }, error, (a, e) => a.message);
        }

        #endregion

        #region Warning

        public static void LogWarning(this ILogger logger, string message)
        {
            Log(logger, LogLevel.Warning, message);
        }

        public static void LogWarning<T>(this ILogger logger, T state, Func<T, string> messageFormatter = null)
        {
            Log(logger, LogLevel.Warning, state, messageFormatter);
        }

        public static void LogWarning(this ILogger logger, Exception error)
        {
            Log(logger, LogLevel.Warning, error);
        }

        public static void LogWarning<T>(this ILogger logger, T state, Exception error, Func<T, Exception, string> messageFormatter)
        {
            Log(logger, LogLevel.Warning, state, error, messageFormatter);
        }

        public static void LogWarning(this ILogger logger, string message, Exception error)
        {
            Log(logger, LogLevel.Warning, new { message }, error, (a, e) => a.message);
        }

        #endregion

        #region Error

        public static void LogError(this ILogger logger, string message)
        {
            Log(logger, LogLevel.Error, message);
        }

        public static void LogError<T>(this ILogger logger, T state, Func<T, string> messageFormatter = null)
        {
            Log(logger, LogLevel.Error, state, messageFormatter);
        }

        public static void LogError(this ILogger logger, Exception error)
        {
            Log(logger, LogLevel.Error, error);
        }

        public static void LogError<T>(this ILogger logger, T state, Exception error, Func<T, Exception, string> messageFormatter)
        {
            Log(logger, LogLevel.Error, state, error, messageFormatter);
        }

        public static void LogError(this ILogger logger, string message, Exception error)
        {
            Log(logger, LogLevel.Error, new { message }, error, (a, e) => a.message);
        }

        #endregion

        #region Critical

        public static void LogCritical(this ILogger logger, string message)
        {
            Log(logger, LogLevel.Critical, message);
        }

        public static void LogCritical<T>(this ILogger logger, T state, Func<T, string> messageFormatter = null)
        {
            Log(logger, LogLevel.Critical, state, messageFormatter);
        }

        public static void LogCritical(this ILogger logger, Exception error)
        {
            Log(logger, LogLevel.Critical, error);
        }

        public static void LogCritical<T>(this ILogger logger, T state, Exception error, Func<T, Exception, string> messageFormatter)
        {
            Log(logger, LogLevel.Critical, state, error, messageFormatter);
        }

        public static void LogCritical(this ILogger logger, string message, Exception error)
        {
            Log(logger, LogLevel.Critical, new { message }, error, (a, e) => a.message);
        }

        #endregion
    }
}