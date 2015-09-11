using System;
using System.Runtime.ExceptionServices;

namespace Nybus.Utils
{
    public static class ExceptionManager
    {
        public static Exception PrepareForRethrow(Exception exception)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
            return exception;
        }
    }
}