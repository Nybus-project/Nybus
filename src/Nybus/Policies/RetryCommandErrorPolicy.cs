using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nybus.Policies
{
    public class RetryCommandErrorPolicy : ICommandErrorPolicy
    {
        private readonly ILogger<RetryCommandErrorPolicy> _logger;
        private readonly RetryCommandErrorPolicyOptions _options;

        public RetryCommandErrorPolicy(IOptions<RetryCommandErrorPolicyOptions> options, ILogger<RetryCommandErrorPolicy> logger)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _options = options.Value;

            if (_options.MaxRetries < 0) throw new ArgumentOutOfRangeException(nameof(_options.MaxRetries), "maxRetries must be greater or equal than 0");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleError<TCommand>(IBusEngine engine, Exception exception, CommandMessage<TCommand> message) where TCommand : class, ICommand
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
    }

    public class RetryCommandErrorPolicyOptions
    {
        public int MaxRetries { get; set; }
    }
}
