using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nybus.Utils;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using Nybus.Filters;

namespace Nybus
{
    public class NybusHost : IBusHost, IBus, IBusExecutionEnvironment
    {
        private readonly IBusEngine _engine;
        private readonly IServiceProvider _serviceProvider;
        private readonly INybusConfiguration _configuration;
        private readonly ILogger<NybusHost> _logger;

        public NybusHost(IBusEngine busEngine, INybusConfiguration configuration, IServiceProvider serviceProvider, ILogger<NybusHost> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _engine = busEngine ?? throw new ArgumentNullException(nameof(busEngine));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IBus Bus => this;

        public Task InvokeCommandAsync<TCommand>(TCommand command, Guid correlationId)
            where TCommand : class, ICommand
        {
            var message = new CommandMessage<TCommand>
            {
                MessageId = Guid.NewGuid().Stringfy(),
                Headers = new HeaderBag
                {
                    CorrelationId = correlationId,
                    SentOn = Clock.Default.Now
                },
                Command = command
            };

            _logger.LogTrace(new { type = typeof(TCommand).FullName, correlationId = correlationId, command }, arg => $"Invoking command of type {arg.type} with correlationId {arg.correlationId}. Command: {arg.command.ToString()}");
            return _engine.SendCommandAsync(message);
        }

        public Task RaiseEventAsync<TEvent>(TEvent @event, Guid correlationId)
            where TEvent : class, IEvent
        {
            var message = new EventMessage<TEvent>
            {
                MessageId = Guid.NewGuid().Stringfy(),
                Headers = new HeaderBag
                {
                    CorrelationId = correlationId,
                    SentOn = Clock.Default.Now
                },
                Event = @event
            };

            _logger.LogTrace(new { type = typeof(TEvent).FullName, correlationId = correlationId, @event }, arg => $"Raising event of type {arg.type} with correlationId {arg.correlationId}. Event: {arg.@event.ToString()}");
            return _engine.SendEventAsync(message);
        }

        private bool _isStarted;
        private IDisposable _disposable;

        public async Task StartAsync()
        {
            _logger.LogTrace("Bus starting");

            var incomingMessages = await _engine.StartAsync().ConfigureAwait(false);

            var observable = from message in incomingMessages
                             where message != null
                             from pipeline in _messagePipelines
                             from execution in pipeline(message).ToObservable()
                             select Unit.Default;

            _disposable = observable.Subscribe();

            _logger.LogTrace("Bus started");

            _isStarted = true;
        }

        public async Task StopAsync()
        {
            if (_isStarted)
            {
                _logger.LogTrace("Bus stopping");

                await _engine.StopAsync().ConfigureAwait(false);
                _disposable.Dispose();

                _logger.LogTrace("Bus stopped");
            }
        }

        private readonly IList<MessagePipeline> _messagePipelines = new List<MessagePipeline>();

        public void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived)
            where TCommand : class, ICommand
        {
            _engine.SubscribeToCommand<TCommand>();

            _errorHandlers.AddOrUpdate(typeof(TCommand), CreateCommandErrorDelegate<TCommand>(), (key, item) => item);

            _messagePipelines.Add(async message =>
            {
                if (message is CommandMessage<TCommand> commandMessage)
                {
                    var dispatcher = new NybusDispatcher(this, commandMessage);
                    var context = new NybusCommandContext<TCommand>(commandMessage);
                    await commandReceived(dispatcher, context).ConfigureAwait(false);
                }
            });
        }

        public void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived)
            where TEvent : class, IEvent
        {
            _engine.SubscribeToEvent<TEvent>();

            _errorHandlers.AddOrUpdate(typeof(TEvent), CreateEventErrorDelegate<TEvent>(), (key, item) => item);

            _messagePipelines.Add(async message =>
            {
                if (message is EventMessage<TEvent> eventMessage)
                {
                    var dispatcher = new NybusDispatcher(this, eventMessage);
                    var context = new NybusEventContext<TEvent>(eventMessage);
                    await eventReceived(dispatcher, context).ConfigureAwait(false);
                }
            });
        }

        private delegate Task MessagePipeline(Message message);

        public IBusExecutionEnvironment ExecutionEnvironment => this;

        public async Task ExecuteCommandHandlerAsync<TCommand>(IDispatcher dispatcher, ICommandContext<TCommand> context, Type handlerType)
            where TCommand : class, ICommand
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    if (scope.ServiceProvider.GetService(handlerType) is ICommandHandler<TCommand> handler)
                    {
                        await handler.HandleAsync(dispatcher, context).ConfigureAwait(false);
                        await _engine.NotifySuccessAsync(context.Message).ConfigureAwait(false);
                    }
                    else
                    {
                        throw new MissingHandlerException(handlerType, $"No valid registration for {handlerType.FullName}");
                    }
                }
                catch (MissingHandlerException ex)
                {
                    _logger.LogError(new { eventType = typeof(TCommand), ex.HandlerType }, ex, (s, e) => $"No valid registration for {s.HandlerType.FullName}");
                    throw;
                }
                catch (Exception ex)
                {
                    var message = context.Message as CommandMessage<TCommand>;
                    _logger.LogError(new { CorrelationId = context.CorrelationId, MessageId = context.Message.MessageId, EventType = typeof(TCommand).Name, Message = message }, ex, (s, e) => $"An error occurred while handling {s.EventType}. {e.Message}");

                    await HandleCommandErrorAsync(context, ex).ConfigureAwait(false);
                }
            }
        }

        public async Task ExecuteEventHandlerAsync<TEvent>(IDispatcher dispatcher, IEventContext<TEvent> context, Type handlerType)
            where TEvent : class, IEvent
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    if (scope.ServiceProvider.GetService(handlerType) is IEventHandler<TEvent> handler)
                    {
                        await handler.HandleAsync(dispatcher, context).ConfigureAwait(false);
                        await _engine.NotifySuccessAsync(context.Message).ConfigureAwait(false);
                    }
                    else
                    {
                        throw new MissingHandlerException(handlerType, $"No valid registration for {handlerType.FullName}");
                    }
                }
                catch (MissingHandlerException ex)
                {
                    _logger.LogError(new { eventType = typeof(TEvent), ex.HandlerType }, ex, (s, e) => $"No valid registration for {s.HandlerType.FullName}");
                    throw;
                }
                catch (Exception ex)
                {
                    var message = context.Message as EventMessage<TEvent>;
                    _logger.LogError(new { CorrelationId = context.CorrelationId, MessageId = context.Message.MessageId, EventType = typeof(TEvent).Name, Message = message }, ex, (s,e) => $"An error occurred while handling {s.EventType}. {e.Message}");

                    await HandleEventErrorAsync(context, ex).ConfigureAwait(false);
                }
            }
        }

        private readonly ConcurrentDictionary<Type, Func<IContext, Exception, Task>> _errorHandlers = new ConcurrentDictionary<Type, Func<IContext, Exception, Task>>();

        private Task HandleCommandErrorAsync<TCommand>(ICommandContext<TCommand> context, Exception error)
            where TCommand : class, ICommand
        {
            var handler = _errorHandlers.GetOrAdd(typeof(TCommand), CreateCommandErrorDelegate<TCommand>());
            return handler(context, error);
        }

        private Func<IContext, Exception, Task> CreateCommandErrorDelegate<TCommand>()
            where TCommand : class, ICommand
        {
            var chain = new List<CommandErrorDelegate<TCommand>>
            {
                (c, ex) => _configuration.FallbackErrorFilter.HandleErrorAsync(c, ex, null)
            };

            foreach (var filter in _configuration.CommandErrorFilters.Reverse())
            {
                var latest = chain.Last();
                chain.Add((c, ex) => filter.HandleErrorAsync(c, ex, latest));
            }

            return (ctx, exception) =>
            {
                if (ctx is ICommandContext<TCommand> context)
                {
                    return chain.Last().Invoke(context, exception);
                }

                return Task.FromException(exception);
            };
        }

        private Task HandleEventErrorAsync<TEvent>(IEventContext<TEvent> context, Exception error)
            where TEvent : class, IEvent
        {
            var handler = _errorHandlers.GetOrAdd(typeof(TEvent), CreateEventErrorDelegate<TEvent>());
            return handler(context, error);
        }

        private Func<IContext, Exception, Task> CreateEventErrorDelegate<TEvent>()
            where TEvent : class, IEvent
        {
            var chain = new List<EventErrorDelegate<TEvent>>
            {
                (c, ex) => _configuration.FallbackErrorFilter.HandleErrorAsync(c, ex, null)
            };

            foreach (var filter in _configuration.EventErrorFilters.Reverse())
            {
                var latest = chain.Last();
                chain.Add((c, ex) => filter.HandleErrorAsync(c, ex, latest));
            }

            return (ctx, exception) =>
            {
                if (ctx is IEventContext<TEvent> context)
                {
                    return chain.Last().Invoke(context, exception);
                }

                return Task.FromException(exception);
            };
        }

    }
}