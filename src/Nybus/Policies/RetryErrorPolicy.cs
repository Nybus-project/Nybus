using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Nybus.Policies
{
    public class RetryErrorPolicyProvider : IErrorPolicyProvider
    {
        private readonly ILoggerFactory _loggerFactory;

        public RetryErrorPolicyProvider(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public string ProviderName => "retry";

        public IErrorPolicy CreatePolicy(IConfigurationSection configuration)
        {
            var options = new RetryErrorPolicyOptions();
            configuration.Bind(options);

            var logger = _loggerFactory.CreateLogger<RetryErrorPolicy>();
            return new RetryErrorPolicy(options, logger);
        }
    }


    public class RetryErrorPolicy : IErrorPolicy
    {
        private readonly ILogger<RetryErrorPolicy> _logger;
        private readonly RetryErrorPolicyOptions _options;

        public RetryErrorPolicy(RetryErrorPolicyOptions options, ILogger<RetryErrorPolicy> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (_options.MaxRetries < 0) throw new ArgumentOutOfRangeException(nameof(_options.MaxRetries), "maxRetries must be greater or equal than 0");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleError<TCommand>(IBusEngine engine, Exception exception, CommandMessage<TCommand> message)
            where TCommand : class, ICommand
        {
            var retryCount = message.Headers.TryGetValue(Headers.RetryCount, out var str) && int.TryParse(str, out var i) ? i : 0;

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

        public int MaxRetries => _options.MaxRetries;
    }

    public class RetryErrorPolicyOptions
    {
        public int MaxRetries { get; set; }
    }
}
