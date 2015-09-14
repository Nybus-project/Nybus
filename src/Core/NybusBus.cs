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

        public NybusBus(IBusEngine engine, INybusOptions options)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (options == null) throw new ArgumentNullException(nameof(options));

            _engine = engine;
            _options = options;
        }

        public Task InvokeCommand<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            var correlationId = _options.CorrelationIdGenerator.Generate();
            return InvokeCommand(command, correlationId);
        }

        public async Task InvokeCommand<TCommand>(TCommand command, Guid correlationId) where TCommand : class, ICommand
        {
            var message = _options.CommandMessageFactory.CreateMessage(command, correlationId);
            await _options.Logger.LogAsync(LogLevel.Info, "Invoking command", new { type = typeof(TCommand).FullName, correlationId = message.CorrelationId });
            await _engine.SendCommand(message);
        }

        public Task RaiseEvent<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            var correlationId = _options.CorrelationIdGenerator.Generate();
            return RaiseEvent(@event, correlationId);
        }

        public async Task RaiseEvent<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent
        {
            var message = _options.EventMessageFactory.CreateMessage(@event, correlationId);
            await _options.Logger.LogAsync(LogLevel.Info, "Raising event", new {type = typeof(TEvent).FullName, correlationId = message.CorrelationId});
            await _engine.SendEvent(message);
        }

        public async Task Start()
        {
            await _options.Logger.LogAsync(LogLevel.Info, "Bus starting");
            await _engine.Start();
            await _options.Logger.LogAsync(LogLevel.Info, "Bus started");
        }

        public async Task Stop()
        {
            await _options.Logger.LogAsync(LogLevel.Info, "Bus stopping");
            await _engine.Stop();
            await _options.Logger.LogAsync(LogLevel.Info, "Bus stopped");
        }
    }
}