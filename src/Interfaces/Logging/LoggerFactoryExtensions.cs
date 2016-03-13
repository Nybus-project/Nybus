using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nybus.Logging
{
    public static class LoggerFactoryExtensions
    {
        /// <summary>
        /// Gets the logger with the name of the current class. 
        /// </summary>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. Make sure you're not doing this in a loop.</remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ILogger CreateCurrentClassLogger(this ILoggerFactory loggerFactory)
        {
            var frame = new StackFrame(1, false);

            string loggerName = frame.GetMethod().DeclaringType.FullName;

            return loggerFactory.CreateLogger(loggerName);
        }
    }
}