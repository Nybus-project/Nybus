using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nybus.Filters
{
    public class DeadLetterQueueErrorFilterProvider : IErrorFilterProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DeadLetterQueueErrorFilterProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public string ProviderName { get; } = "dead-letter-queue";

        public IErrorFilter CreateErrorFilter(IConfigurationSection settings)
        {
            var engine = _serviceProvider.GetRequiredService<IBusEngine>();
            var logger = _serviceProvider.GetRequiredService<ILogger<DeadLetterQueueErrorFilter>>();

            return new DeadLetterQueueErrorFilter(logger, engine);
        }
    }

    public class DeadLetterQueueErrorFilter : IErrorFilter
    {
        private readonly ILogger<DeadLetterQueueErrorFilter> _logger;
        private readonly IBusEngine _engine;

        public DeadLetterQueueErrorFilter(ILogger<DeadLetterQueueErrorFilter> logger, IBusEngine engine)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public async Task HandleErrorAsync<TCommand>(ICommandContext<TCommand> context, Exception exception, CommandErrorDelegate<TCommand> next) where TCommand : class, ICommand
        {
            try
            {
                await SendToErrorQueue(context.Message, exception);
            }
            catch (Exception dlqException)
            {
                _logger.LogError(new { }, dlqException, (s, ex) => $"Unable to send message to DLQ: {ex.Message}");
                await next(context, exception).ConfigureAwait(false);
            }
        }

        public async Task HandleErrorAsync<TEvent>(IEventContext<TEvent> context, Exception exception, EventErrorDelegate<TEvent> next) where TEvent : class, IEvent
        {
            try
            {
                await SendToErrorQueue(context.Message, exception);
            }
            catch (Exception dlqException)
            {
                _logger.LogError(new { }, dlqException, (s, ex) => $"Unable to send message to DLQ: {ex.Message}");
                await next(context, exception).ConfigureAwait(false);
            }
        }

        private async Task SendToErrorQueue(Message message, Exception exception)
        {
            if (exception != null)
            {
                message.Headers["DLQ-Fault-Message"] = exception.Message;
                message.Headers["DLQ-Fault-StackTrace"] = exception.StackTrace;
            }

            message.Headers["DLQ-Error-Host"] = Environment.MachineName;
            message.Headers["DLQ-Error-Process"] = Process.GetCurrentProcess().ProcessName;

            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                message.Headers["DLQ-Error-Assembly"] = entryAssembly.GetName().Name;
            }

            await _engine.NotifySuccessAsync(message).ConfigureAwait(false);

            await _engine.SendMessageToErrorQueueAsync(message).ConfigureAwait(false);
        }
    }
}