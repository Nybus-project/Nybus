using System;
using System.Threading.Tasks;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public class NybusBusBuilder : IBusBuilder
    {
        private readonly IBusEngine _busEngine;
        private readonly NybusOptions _options;

        public NybusBusBuilder(IBusEngine busEngine, NybusOptions options)
        {
            if (busEngine == null) throw new ArgumentNullException(nameof(busEngine));
            if (options == null) throw new ArgumentNullException(nameof(options));
            _busEngine = busEngine;
            _options = options;
        }

        public NybusBusBuilder(IBusEngine busEngine) : this(busEngine, new NybusOptions()) { }

        #region SubscribeToEvent

        public void SubscribeToEvent<TEvent>() 
            where TEvent : class, IEvent
        {
            _options.Logger.Log(LogLevel.Trace, "Subscribing to event", new { eventType = typeof(TEvent).FullName });
            _busEngine.SubscribeToEvent((EventReceived<TEvent>)ResolveHandlerAndHandle<IEventHandler<TEvent>, TEvent>);
        }

        public void SubscribeToEvent<TEventHandler, TEvent>() 
            where TEventHandler : IEventHandler<TEvent>
            where TEvent : class, IEvent
        {
            _options.Logger.Log(LogLevel.Trace, "Subscribing to event", new {eventType =typeof(TEvent).FullName, handlerType = typeof(TEventHandler).FullName });
            _busEngine.SubscribeToEvent((EventReceived<TEvent>)ResolveHandlerAndHandle<TEventHandler, TEvent>);
        }

        public void SubscribeToEvent<TEventHandler, TEvent>(TEventHandler handler)
            where TEventHandler : IEventHandler<TEvent> where TEvent : class, IEvent
        {
            _options.Logger.Log(LogLevel.Trace, "Subscribing to event with instance", new { eventType = typeof(TEvent).FullName, handlerType = typeof(TEventHandler).FullName });
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
            var handler = _options.Container.Resolve<TEventHandler>();
            await HandleEventMessage(handler, message);
            _options.Container.Release(handler);
        }

        private async Task HandleEventMessage<TEventHandler, TEvent>(TEventHandler handler, EventMessage<TEvent> message)
            where TEventHandler : IEventHandler<TEvent> 
            where TEvent : class, IEvent
        {
            await _options.Logger.LogAsync(LogLevel.Trace, "Handling event", new {eventType = typeof(TEvent).FullName, handlerType = typeof(TEventHandler).FullName, correlationId = message.CorrelationId});
            var context = _options.EventContextFactory.CreateContext(message);
            await handler.Handle(context);
        }

        #endregion

        #region SubscribeToCommand

        public void SubscribeToCommand<TCommand>() where TCommand : class, ICommand
        {
            _options.Logger.Log(LogLevel.Trace, "Subscribing to command", new { commandType = typeof(TCommand).FullName });
            _busEngine.SubscribeToCommand((CommandReceived<TCommand>)ResolveHandlerAndHandle<ICommandHandler<TCommand>, TCommand>);
        }

        public void SubscribeToCommand<TCommandHandler, TCommand>() 
            where TCommandHandler : ICommandHandler<TCommand>
            where TCommand : class, ICommand
        {
            _options.Logger.Log(LogLevel.Trace, "Subscribing to command", new { commandType = typeof(TCommand).FullName, handlerType = typeof(TCommandHandler).FullName });
            _busEngine.SubscribeToCommand((CommandReceived<TCommand>)ResolveHandlerAndHandle<TCommandHandler, TCommand>);
        }

        public void SubscribeToCommand<TCommandHandler, TCommand>(TCommandHandler handler)
            where TCommandHandler : ICommandHandler<TCommand>
            where TCommand : class, ICommand
        {
            _options.Logger.Log(LogLevel.Trace, "Subscribing to command with instance", new { commandType = typeof(TCommand).FullName, handlerType = typeof(TCommandHandler).FullName });
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
            var handler = _options.Container.Resolve<TCommandHandler>();
            await HandleCommandMessage(handler, message);
            _options.Container.Release(handler);
        }

        private async Task HandleCommandMessage<TCommandHandler, TCommand>(TCommandHandler handler, CommandMessage<TCommand> message)
            where TCommandHandler : ICommandHandler<TCommand> where TCommand : class, ICommand
        {
            await _options.Logger.LogAsync(LogLevel.Trace, "Handling command", new { commandType = typeof(TCommand).FullName, handlerType = typeof(TCommandHandler).FullName, correlationId = message.CorrelationId });
            var context = _options.CommandContextFactory.CreateContext(message);
            await handler.Handle(context);
        }

        #endregion

        public IBus Build()
        {
            _options.Logger.Log(LogLevel.Trace, "Building Bus");
            return new Nybus(_busEngine, _options);
        }

    }
}
