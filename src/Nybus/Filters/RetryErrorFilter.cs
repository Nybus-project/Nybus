using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nybus.Utils;

namespace Nybus.Filters
{
    public class RetryErrorFilterProvider : IErrorFilterProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public RetryErrorFilterProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public string ProviderName { get; } = "retry";

        public IErrorFilter CreateErrorFilter(IConfigurationSection settings)
        {
            var engine = _serviceProvider.GetRequiredService<IBusEngine>();
            var logger = _serviceProvider.GetRequiredService<ILogger<RetryErrorFilter>>();

            var options = new RetryErrorFilterOptions();
            settings.Bind(options);

            return new RetryErrorFilter(engine, options, logger);
        }
    }

    public class RetryErrorFilter : IErrorFilter
    {
        private readonly IBusEngine _engine;
        private readonly RetryErrorFilterOptions _options;
        private readonly ILogger<RetryErrorFilter> _logger;

        public RetryErrorFilter(IBusEngine engine, RetryErrorFilterOptions options, ILogger<RetryErrorFilter> logger)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (_options.MaxRetries < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.MaxRetries), "maxRetries must be greater or equal than 0");
            }
        }
        
        private static int RetryCount(Message message) => message.Headers.TryGetValue(Headers.RetryCount, out var str) && int.TryParse(str, out var i) ? i : 0;

        public async Task HandleErrorAsync<TCommand>(ICommandContext<TCommand> context, Exception exception, CommandErrorDelegate<TCommand> next)
            where TCommand : class, ICommand
        {
            if (context.Message is CommandMessage<TCommand> message)
            {
                var retryCount = RetryCount(message) + 1;

                if (retryCount < _options.MaxRetries)
                {
                    _logger.LogTrace($"Error {retryCount}/{_options.MaxRetries}: will retry");

                    message.Headers[Headers.RetryCount] = retryCount.Stringfy();

                    await _engine.SendCommandAsync(message).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogTrace($"Error {retryCount}/{_options.MaxRetries}: will not retry");

                    await _engine.NotifyFailAsync(message).ConfigureAwait(false);

                    await next(context, exception).ConfigureAwait(false);
                }
            }
        }

        public async Task HandleErrorAsync<TEvent>(IEventContext<TEvent> context, Exception exception, EventErrorDelegate<TEvent> next)
            where TEvent : class, IEvent
        {
            if (context.Message is EventMessage<TEvent> message)
            {
                var retryCount = RetryCount(message) + 1;

                if (retryCount < _options.MaxRetries)
                {
                    _logger.LogTrace($"Error {retryCount}/{_options.MaxRetries}: will retry");

                    message.Headers[Headers.RetryCount] = retryCount.Stringfy();

                    await _engine.SendEventAsync(message).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogTrace($"Error {retryCount}/{_options.MaxRetries}: will not retry");

                    await _engine.NotifyFailAsync(message).ConfigureAwait(false);

                    await next(context, exception).ConfigureAwait(false);
                }
            }
        }
    }

    public class RetryErrorFilterOptions
    {
        public int MaxRetries { get; set; }
    }
}
