using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nybus.Filters
{
    public class DiscardErrorFilterProvider : IErrorFilterProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DiscardErrorFilterProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public string ProviderName { get; } = "discard";

        public IErrorFilter CreateErrorFilter(IConfigurationSection settings)
        {
            var engine = _serviceProvider.GetRequiredService<IBusEngine>();
            var logger = _serviceProvider.GetRequiredService<ILogger<DiscardErrorFilter>>();

            return new DiscardErrorFilter(engine, logger);
        }
    }

    public class DiscardErrorFilter : IErrorFilter
    {
        private readonly IBusEngine _engine;
        private readonly ILogger<DiscardErrorFilter> _logger;

        public DiscardErrorFilter(IBusEngine engine, ILogger<DiscardErrorFilter> logger)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleErrorAsync<TCommand>(ICommandContext<TCommand> context, Exception exception, CommandErrorDelegate<TCommand> next)
            where TCommand : class, ICommand
        {
            try
            {
                await _engine.NotifyFailAsync(context.Message).ConfigureAwait(false);
            }
            catch (Exception discardException)
            {
                _logger.LogError(discardException, ex => $"Unable to discard message: {ex.Message}");
                await next(context, exception).ConfigureAwait(false);
            }
        }

        public async Task HandleErrorAsync<TEvent>(IEventContext<TEvent> context, Exception exception, EventErrorDelegate<TEvent> next)
            where TEvent : class, IEvent
        {
            try
            {
                await _engine.NotifyFailAsync(context.Message).ConfigureAwait(false);
            }
            catch (Exception discardException)
            {
                _logger.LogError(discardException, ex => $"Unable to discard message: {ex.Message}");
                await next(context, exception).ConfigureAwait(false);
            }
        }
    }
}
