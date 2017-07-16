using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nybus.Utils;

namespace Nybus
{
    public class NybusBus : IBus
    {
        private readonly IBusEngine _engine;
        private readonly ILogger<NybusBus> _logger;

        public NybusBus(IBusEngine busEngine, ILogger<NybusBus> logger)
        {
            _engine = busEngine ?? throw new ArgumentNullException(nameof(busEngine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        string StringfyGuid(Guid guid) => guid.ToString("N");

        string StringfyDateTimeOffset(DateTimeOffset dto) => dto.ToString("O");

        public async Task InvokeCommandAsync<TCommand>(TCommand command, Guid correlationId) where TCommand : class, ICommand
        {
            CommandMessage<TCommand> message = new CommandMessage<TCommand>
            {
                Headers = new HeaderBag
                {
                    [Headers.CorrelationId] = StringfyGuid(correlationId),
                    [Headers.SentOn] = StringfyDateTimeOffset(Clock.Default.Now)
                },
                Command = command
            };

            _logger.LogTrace(new { type = typeof(TCommand).FullName, correlationId = correlationId, command }, arg => $"Invoking command of type {arg.type} with correlationId {arg.correlationId}. Command: {arg.command.ToString()}");
            await _engine.SendCommandAsync(message).ConfigureAwait(false);
        }

        public async Task RaiseEventAsync<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent
        {
            EventMessage<TEvent> message = new EventMessage<TEvent>
            {
                Headers = new HeaderBag
                {
                    [Headers.CorrelationId] = StringfyGuid(correlationId),
                    [Headers.SentOn] = StringfyDateTimeOffset(Clock.Default.Now)
                },
                Event = @event
            };

            _logger.LogTrace(new { type = typeof(TEvent).FullName, correlationId = correlationId, @event }, arg => $"Raising event of type {arg.type} with correlationId {arg.correlationId}. Event: {arg.@event.ToString()}");
            await _engine.SendEventAsync(message).ConfigureAwait(false);
        }

        public async Task StartAsync()
        {
            _logger.LogInformation("Bus starting");
            await _engine.StartAsync().ConfigureAwait(false);
            _logger.LogInformation("Bus started");
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Bus stopping");
            await _engine.StopAsync().ConfigureAwait(false);
            _logger.LogInformation("Bus stopped");
        }
    }
}
