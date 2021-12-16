using System;

namespace Tests
{
    public class TestException : Exception
    {
        public TestException(string message, string stackTrace) : base(message)
        {
            StackTrace = stackTrace;
        }

        public override string StackTrace { get; }
    }
}