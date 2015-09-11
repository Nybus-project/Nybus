using System;
using MassTransit;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface IErrorStrategy
    {
        void HandleError<T>(IConsumeContext<T> context, Exception exception) where T : class;
    }

    public class RetryErrorStrategy : IErrorStrategy
    {
        private readonly int _retries;

        public RetryErrorStrategy(int retries)
        {
            _retries = retries;
        }

        public void HandleError<T>(IConsumeContext<T> context, Exception exception) where T : class
        {
            if (context.RetryCount < _retries)
            {
                context.RetryLater();
            }
            else
            {
                throw ExceptionManager.PrepareForRethrow(exception);
            }
        }
    }
}