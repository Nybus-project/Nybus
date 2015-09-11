using System;
using System.Threading.Tasks;
using Nybus.Container;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public class NybusOptions
    {
        public IContainer Container { get; set; } = new ActivatorContainer();
    }

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

        public void SubscribeToEvent<TEvent>() where TEvent : class, IEvent
        {
            _busEngine.SubscribeToEvent((EventReceived<TEvent>)ResolveHandlerAndHandle<IEventHandler<TEvent>, TEvent>);
        }

        public void SubscribeToEvent<TEventHandler, TEvent>() where TEventHandler : IEventHandler<TEvent>
            where TEvent : class, IEvent
        {
            _busEngine.SubscribeToEvent((EventReceived<TEvent>)ResolveHandlerAndHandle<TEventHandler, TEvent>);
        }

        public void SubscribeToEvent<TEventHandler, TEvent>(TEventHandler handler)
            where TEventHandler : IEventHandler<TEvent> where TEvent : class, IEvent
        {
            _busEngine.SubscribeToEvent((EventMessage<TEvent> message) => HandleEventMessage(handler, message));
        }

        public void SubscribeToEvent<TEvent>(Func<EventContext<TEvent>, Task> handler) where TEvent : class, IEvent
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
            where TEventHandler : IEventHandler<TEvent> where TEvent : class, IEvent
        {
            await handler.Handle(CreateContext(message));
        }

        private EventContext<TEvent> CreateContext<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent
        {
            return new EventContext<TEvent>(message.Event, Clock.Default.Now);
        }

        #endregion

        #region SubscribeToCommand

        public void SubscribeToCommand<TCommand>() where TCommand : class, ICommand
        {
            _busEngine.SubscribeToCommand((CommandReceived<TCommand>)ResolveHandlerAndHandle<ICommandHandler<TCommand>, TCommand>);
        }

        public void SubscribeToCommand<TCommandHandler, TCommand>() where TCommandHandler : ICommandHandler<TCommand>
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
            var handler = _options.Container.Resolve<TCommandHandler>();
            await HandleCommandMessage(handler, message);
            _options.Container.Release(handler);
        }

        private async Task HandleCommandMessage<TCommandHandler, TCommand>(TCommandHandler handler, CommandMessage<TCommand> message)
            where TCommandHandler : ICommandHandler<TCommand> where TCommand : class, ICommand
        {
            await handler.Handle(CreateContext(message));
        }

        private CommandContext<TCommand> CreateContext<TCommand>(CommandMessage<TCommand> message)
            where TCommand : class, ICommand
        {
            return new CommandContext<TCommand>(message.Command, Clock.Default.Now);
        }

        #endregion

        public IBus Build()
        {
            return new Nybus(_busEngine);
        }

    }
}
