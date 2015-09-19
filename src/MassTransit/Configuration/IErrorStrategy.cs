using System;
using System.Threading.Tasks;
using MassTransit;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface IErrorStrategy
    {
        Task<bool> HandleError<T>(IConsumeContext<T> context, Exception exception) where T : class;
    }

    public class RetryErrorStrategy : IErrorStrategy
    {
        private readonly int _retries;

        public RetryErrorStrategy(int retries)
        {
            _retries = retries;
        }

        public Task<bool> HandleError<T>(IConsumeContext<T> context, Exception exception) where T : class
        {
            if (context.RetryCount < _retries)
            {
                context.RetryLater();

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}