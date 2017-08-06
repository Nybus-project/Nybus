using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Nybus.Policies
{
    public class RetryEventErrorPolicy : IEventErrorPolicy
    {
        private readonly ILogger<RetryEventErrorPolicy> _logger;
        private readonly RetryEventErrorPolicyOptions _options;

        public RetryEventErrorPolicy(IOptions<RetryEventErrorPolicyOptions> options, ILogger<RetryEventErrorPolicy> logger)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _options = options.Value;

            if (_options.MaxRetries < 0) throw new ArgumentOutOfRangeException(nameof(_options.MaxRetries), "maxRetries must be greater or equal than 0");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleError<TEvent>(IBusEngine engine, Exception exception, EventMessage<TEvent> message) where TEvent : class, IEvent
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

    public class RetryEventErrorPolicyOptions
    {
        public int MaxRetries { get; set; }
    }
}
