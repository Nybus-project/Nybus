using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nybus.Utils;
using System.Linq;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using Nybus.Policies;

namespace Nybus
{
    public class NybusHost : IBusHost, IBus
    {
        private readonly IBusEngine _engine;
        private readonly ILogger<NybusHost> _logger;
        private readonly NybusBusOptions _options;

        public NybusHost(IBusEngine busEngine, NybusBusOptions options, ILogger<NybusHost> logger)
        {
            _engine = busEngine ?? throw new ArgumentNullException(nameof(busEngine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
        
        public async Task InvokeCommandAsync<TCommand>(TCommand command, Guid correlationId) where TCommand : class, ICommand
        {
            CommandMessage<TCommand> message = new CommandMessage<TCommand>
            {
                Headers = new HeaderBag
                {
                    [Headers.CorrelationId] = Helpers.StringfyGuid(correlationId),
                    [Headers.SentOn] = Helpers.StringfyDateTimeOffset(Clock.Default.Now)
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
                    [Headers.CorrelationId] = Helpers.StringfyGuid(correlationId),
                    [Headers.SentOn] = Helpers.StringfyDateTimeOffset(Clock.Default.Now)
                },
                Event = @event
            };

            _logger.LogTrace(new { type = typeof(TEvent).FullName, correlationId = correlationId, @event }, arg => $"Raising event of type {arg.type} with correlationId {arg.correlationId}. Event: {arg.@event.ToString()}");
            await _engine.SendEventAsync(message).ConfigureAwait(false);
        }

        private IDisposable _disposable;

        public Task StartAsync()
        {
            _logger.LogTrace("Bus starting");

            IObservable<Message> fromEngine = _engine.Start();

            var sequence = _pipelineFactories.ToObservable().Select(factory => factory(fromEngine)).SelectMany(p => p);

            _disposable = sequence.Subscribe();

            _logger.LogTrace("Bus started");

            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            _logger.LogTrace("Bus stopping");
            _engine.Stop();
            _disposable.Dispose();

            _logger.LogTrace("Bus stopped");

            return Task.CompletedTask;
        }

        private readonly List<MessagePipelineFactory> _pipelineFactories = new List<MessagePipelineFactory>();

        public void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand
        {
            _engine.SubscribeToCommand<TCommand>();

            MessagePipelineFactory factory = messages => from message in messages
                                                         where message is CommandMessage<TCommand>
                                                         let commandMessage = (CommandMessage<TCommand>)message
                                                         let context = new NybusCommandContext<TCommand>(commandMessage)
                                                         from t1 in ExecuteHandler(context).Catch<Unit, Exception>(ex => HandleError(ex, commandMessage, context))
                                                         from t2 in NotifySuccess(commandMessage)
                                                         select Unit.Default;

            _pipelineFactories.Add(factory);

            IObservable<Unit> ExecuteHandler(ICommandContext<TCommand> context)
            {
                var dispatcher = new NybusDispatcher(this, context.CorrelationId);
                return commandReceived(dispatcher, context).ToObservable();
            }

            IObservable<Unit> NotifySuccess(CommandMessage<TCommand> message)
            {
                return _engine.NotifySuccess(message).ToObservable();
            }

            IObservable<Unit> HandleError(Exception exception, CommandMessage<TCommand> message, ICommandContext<TCommand> context)
            {
                _logger.LogError(new { CorrelationId = context.CorrelationId, MessageId = message.MessageId, CommandType = typeof(TCommand).Name, Exception = exception, Message = message }, s => $"An error occurred while handling {s.CommandType}. {s.Exception.Message}");
                return _options.ErrorPolicy.HandleError(_engine, exception, message).ToObservable();
            }
        }

        public void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent
        {
            _engine.SubscribeToEvent<TEvent>();

            MessagePipelineFactory factory = messages => from message in messages
                                                         where message is EventMessage<TEvent>
                                                         let eventMessage = (EventMessage<TEvent>)message
                                                         let context = new NybusEventContext<TEvent>(eventMessage)
                                                         from t1 in ExecuteHandler(context).Catch<Unit, Exception>(ex => HandleError(ex, eventMessage, context))
                                                         from t2 in NotifySuccess(eventMessage)
                                                         select Unit.Default;

            _pipelineFactories.Add(factory);

            IObservable<Unit> ExecuteHandler(IEventContext<TEvent> context)
            {
                var dispatcher = new NybusDispatcher(this, context.CorrelationId);
                return Observable.FromAsync(() => eventReceived(dispatcher, context));
            }

            async Task<Unit> NotifySuccess(EventMessage<TEvent> message)
            {
                await _engine.NotifySuccess(message);
                return Unit.Default;
            }

            IObservable<Unit> HandleError(Exception exception, EventMessage<TEvent> message, IEventContext<TEvent> context)
            {
                _logger.LogError(new { CorrelationId = context.CorrelationId, MessageId = message.MessageId, EventType = typeof(TEvent).Name, Exception = exception, Message = message }, s => $"An error occurred while handling {s.EventType}. {s.Exception.Message}");
                return _options.ErrorPolicy.HandleError(_engine, exception, message).ToObservable();
            }
        }

        private delegate IObservable<Unit> MessagePipelineFactory(IObservable<Message> message);

    }

    public class NybusBusOptions
    {
        public IErrorPolicy ErrorPolicy { get; set; }
    }

    public class NybusDispatcher : IDispatcher
    {
        private readonly IBus _innerBus;
        private readonly Guid _correlationId;

        public NybusDispatcher(IBus innerBus, Guid correlationId)
        {
            _innerBus = innerBus;
            _correlationId = correlationId;
        }

        public Task InvokeCommandAsync<TCommand>(TCommand command)
            where TCommand : class, ICommand
        {
            return _innerBus.InvokeCommandAsync(command, _correlationId);
        }

        public Task RaiseEventAsync<TEvent>(TEvent @event)
            where TEvent : class, IEvent
        {
            return _innerBus.RaiseEventAsync(@event, _correlationId);
        }
    }
}
