using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nybus.Utils;
using System.Linq;
using System.Reactive;

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

        private IDisposable _disposable;

        public Task StartAsync()
        {
            _logger.LogInformation("Bus starting");

            IObservable<Message> fromEngine = _engine.Start();

            var sequence = processes.ToObservable().Select(p => p(fromEngine)).SelectMany(p => p);

            _disposable = sequence.Subscribe();

            _logger.LogInformation("Bus started");

            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Bus stopping");
            await _engine.Stop().ConfigureAwait(false);
            _disposable.Dispose();

            _logger.LogInformation("Bus stopped");
        }

        private List<ProcessMessage> processes = new List<ProcessMessage>();

        public void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand
        {
            _engine.SubscribeToCommand<TCommand>();

            ProcessMessage process = messages => from message in messages
                                                 where message is CommandMessage<TCommand>
                                                 let commandMessage = (CommandMessage<TCommand>)message
                                                 let context = new NybusCommandContext<TCommand>(commandMessage)
                                                 from task in Observable.FromAsync(() => commandReceived(context))
                                                 select task;

            processes.Add(process);
        }

        public void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent
        {
            _engine.SubscribeToEvent<TEvent>();

            ProcessMessage process = messages => from message in messages
                                                 where message is EventMessage<TEvent>
                                                 let eventMessage = (EventMessage<TEvent>)message
                                                 let context = new NybusEventContext<TEvent>(eventMessage)
                                                 from task in Observable.FromAsync(() => eventReceived(context))
                                                 select task;

            processes.Add(process);

        }

        private delegate IObservable<Unit> ProcessMessage(IObservable<Message> message);

    }
}
