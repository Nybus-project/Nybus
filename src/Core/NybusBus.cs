using System;
using System.Threading.Tasks;
using Nybus.Configuration;
using Nybus.Logging;

namespace Nybus
{
    public class NybusBus : IBus
    {
        private readonly IBusEngine _engine;
        private readonly INybusOptions _options;
        private readonly ILogger _logger;

        public NybusBus(IBusEngine engine, INybusOptions options)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (options == null) throw new ArgumentNullException(nameof(options));

            _engine = engine;
            _options = options;
            _logger = _options.LoggerFactory.CraeteLogger(nameof(NybusBus));
        }

        public Task InvokeCommand<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            var correlationId = _options.CorrelationIdGenerator.Generate();
            return InvokeCommand(command, correlationId);
        }

        public async Task InvokeCommand<TCommand>(TCommand command, Guid correlationId) where TCommand : class, ICommand
        {
            var message = _options.CommandMessageFactory.CreateMessage(command, correlationId);
            _logger.LogInformation(new { type = typeof(TCommand).FullName, correlationId = message.CorrelationId, message = "Invoking command" });
            await _engine.SendCommand(message).ConfigureAwait(false);
        }

        public Task RaiseEvent<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            var correlationId = _options.CorrelationIdGenerator.Generate();
            return RaiseEvent(@event, correlationId);
        }

        public async Task RaiseEvent<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent
        {
            var message = _options.EventMessageFactory.CreateMessage(@event, correlationId);
            _logger.LogInformation(new { type = typeof(TEvent).FullName, correlationId = message.CorrelationId, message = "Raising event" });
            await _engine.SendEvent(message).ConfigureAwait(false);
        }

        public async Task Start()
        {
            _logger.LogInformation("Bus starting");
            await _engine.Start().ConfigureAwait(false);
            _logger.LogInformation("Bus started");
        }

        public async Task Stop()
        {
            _logger.LogInformation("Bus stopping");
            await _engine.Stop().ConfigureAwait(false);
            _logger.LogInformation("Bus stopped");
        }
    }
}