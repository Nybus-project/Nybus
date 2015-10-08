using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nybus.Logging;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public class NybusBusBuilder : IBusBuilder
    {
        private readonly IBusEngine _busEngine;
        private readonly INybusOptions _options;
        private readonly ILogger _logger;

        public NybusBusBuilder(IBusEngine busEngine, INybusOptions options)
        {
            if (busEngine == null) throw new ArgumentNullException(nameof(busEngine));
            if (options == null) throw new ArgumentNullException(nameof(options));
            _busEngine = busEngine;
            _options = options;

            _logger = _options.LoggerFactory.CreateLogger(nameof(NybusBusBuilder));
        }

        public NybusBusBuilder(IBusEngine busEngine) : this(busEngine, new NybusOptions()) { }

        #region SubscribeToEvent

        public void SubscribeToEvent<TEvent>() 
            where TEvent : class, IEvent
        {
            _busEngine.SubscribeToEvent((EventReceived<TEvent>)ResolveHandlerAndHandle<IEventHandler<TEvent>, TEvent>);
        }

        public void SubscribeToEvent<TEventHandler, TEvent>() 
            where TEventHandler : IEventHandler<TEvent>
            where TEvent : class, IEvent
        {
            _busEngine.SubscribeToEvent((EventReceived<TEvent>)ResolveHandlerAndHandle<TEventHandler, TEvent>);
        }

        public void SubscribeToEvent<TEventHandler, TEvent>(TEventHandler handler)
            where TEventHandler : IEventHandler<TEvent> where TEvent : class, IEvent
        {
            _busEngine.SubscribeToEvent((EventMessage<TEvent> message) => HandleEventMessage(handler, message));
        }

        public void SubscribeToEvent<TEvent>(Func<EventContext<TEvent>, Task> handler) 
            where TEvent : class, IEvent
        {
            var eventHandler = new DelegateEventHandler<TEvent>(handler);
            SubscribeToEvent<DelegateEventHandler<TEvent>, TEvent>(eventHandler);
        }

        private async Task ResolveHandlerAndHandle<TEventHandler, TEvent>(EventMessage<TEvent> message)
            where TEventHandler : IEventHandler<TEvent>
            where TEvent : class, IEvent
        {
            using (var scope = _options.Container.BeginScope())
            {
                var handler = scope.Resolve<TEventHandler>();

                if (handler != null)
                {
                    await HandleEventMessage(handler, message).ConfigureAwait(false);
                    scope.Release(handler);
                }
                else
                {
                    _logger.LogError(new { message = "Handler not found", eventType = typeof(TEvent).FullName, handlerType = typeof(TEventHandler).FullName, correlationId = message.CorrelationId, containerType = _options.Container.GetType().FullName });
                }
            }
        }

        private async Task HandleEventMessage<TEventHandler, TEvent>(TEventHandler handler, EventMessage<TEvent> message)
            where TEventHandler : IEventHandler<TEvent> 
            where TEvent : class, IEvent
        {
            _logger.LogVerbose(new { eventType = typeof(TEvent).FullName, handlerType = typeof(TEventHandler).FullName, correlationId = message.CorrelationId, message = "Handling event" });

            var context = _options.EventContextFactory.CreateContext(message, _options);
            await handler.Handle(context).ConfigureAwait(false);
        }

        #endregion

        #region SubscribeToCommand

        public void SubscribeToCommand<TCommand>() where TCommand : class, ICommand
        {
            _busEngine.SubscribeToCommand((CommandReceived<TCommand>)ResolveHandlerAndHandle<ICommandHandler<TCommand>, TCommand>);
        }

        public void SubscribeToCommand<TCommandHandler, TCommand>() 
            where TCommandHandler : ICommandHandler<TCommand>
            where TCommand : class, ICommand
        {
            _busEngine.SubscribeToCommand((CommandReceived<TCommand>)ResolveHandlerAndHandle<TCommandHandler, TCommand>);
        }

        public void SubscribeToCommand<TCommandHandler, TCommand>(TCommandHandler handler)
            where TCommandHandler : ICommandHandler<TCommand>
            where TCommand : class, ICommand
        {
            _busEngine.SubscribeToCommand((CommandMessage<TCommand> message) => HandleCommandMessage(handler, message));
        }

        public void SubscribeToCommand<TCommand>(Func<CommandContext<TCommand>, Task> handler)
            where TCommand : class, ICommand
        {
            var commandHandler = new DelegateCommandHandler<TCommand>(handler);
            SubscribeToCommand<DelegateCommandHandler<TCommand>, TCommand>(commandHandler);
        }

        private async Task ResolveHandlerAndHandle<TCommandHandler, TCommand>(CommandMessage<TCommand> message)
            where TCommandHandler : ICommandHandler<TCommand>
            where TCommand : class, ICommand
        {
            using (var scope = _options.Container.BeginScope())
            {
                var handler = scope.Resolve<TCommandHandler>();

                if (handler != null)
                {
                    await HandleCommandMessage(handler, message).ConfigureAwait(false);
                    scope.Release(handler);
                }
                else
                {
                    _logger.LogError(new {message = "Handler not found", commandType = typeof(TCommand).FullName, handlerType = typeof(TCommandHandler).FullName, correlationId = message.CorrelationId, containerType = _options.Container.GetType().FullName });
                }
            }
        }

        private async Task HandleCommandMessage<TCommandHandler, TCommand>(TCommandHandler handler, CommandMessage<TCommand> message)
            where TCommandHandler : ICommandHandler<TCommand> where TCommand : class, ICommand
        {
            _logger.LogVerbose(new { eventType = typeof(TCommand).FullName, handlerType = typeof(TCommandHandler).FullName, correlationId = message.CorrelationId, message = "Handling command" });

            var context = _options.CommandContextFactory.CreateContext(message, _options);
            await handler.Handle(context).ConfigureAwait(false);
        }

        #endregion

        public IBus Build()
        {
            _logger.LogVerbose("Building bus");

            return new NybusBus(_busEngine, _options);
        }

    }
}
