using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Nybus.Policies
{
    public class RetryErrorPolicy : IErrorPolicy
    {
        private readonly ILogger<RetryErrorPolicy> _logger;
        private readonly RetryErrorPolicyOptions _options;

        public RetryErrorPolicy(IOptions<RetryErrorPolicyOptions> options, ILogger<RetryErrorPolicy> logger)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _options = options.Value;

            if (_options.MaxRetries < 0) throw new ArgumentOutOfRangeException(nameof(_options.MaxRetries), "maxRetries must be greater or equal than 0");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleError<TCommand>(IBusEngine engine, Exception exception, CommandMessage<TCommand> message)
            where TCommand : class, ICommand
        {
            var retryCount = message.Headers.TryGetValue(Headers.RetryCount, out string str) && int.TryParse(str, out int i) ? i : 0;

            retryCount++;

            if (retryCount < _options.MaxRetries)
            {
                _logger.LogTrace($"Error {retryCount}/{_options.MaxRetries}: will retry");

                message.Headers[Headers.RetryCount] = retryCount.ToString();

                await engine.SendCommandAsync(message).ConfigureAwait(false);
            }
            else
            {
                _logger.LogTrace($"Error {retryCount}/{_options.MaxRetries}: will not retry");

                await engine.NotifyFail(message).ConfigureAwait(false);
            }

        }

        public async Task HandleError<TEvent>(IBusEngine engine, Exception exception, EventMessage<TEvent> message)
            where TEvent : class, IEvent
        {
            var retryCount = message.Headers.TryGetValue(Headers.RetryCount, out string str) && int.TryParse(str, out int i) ? i : 0;

            retryCount++;

            if (retryCount < _options.MaxRetries)
            {
                message.Headers[Headers.RetryCount] = retryCount.ToString();

                await engine.SendEventAsync(message).ConfigureAwait(false);
            }
            else
            {
                await engine.NotifyFail(message).ConfigureAwait(false);
            }
        }
    }

    public class RetryErrorPolicyOptions
    {
        public int MaxRetries { get; set; }
    }
}
