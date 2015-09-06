using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.SubscriptionConfigurators;
using Nybus.Container;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public class MassTransitRabbitMqBusConfiguration : IBusConfiguration
    {
        public MassTransitRabbitMqBusConfiguration()
        {
            _eventSubscriptions = new List<Action<SubscriptionBusServiceConfigurator>>();
            _commandSubscriptions = new List<Action<SubscriptionBusServiceConfigurator>>();
            _sharedQueueName = Assembly.GetEntryAssembly().FullName.Hash();
        }

        public void SetSharedQueueName(string queueName)
        {
            _sharedQueueName = queueName;
        }

        private string _sharedQueueName;

        public string SharedQueueName
        {
            get { return _sharedQueueName; }
        }

        private IContainer _container;

        public IContainer Container
        {
            get { return _container; }
        }

        public void SetContainer(IContainer container)
        {
            _container = container;
        }

        private readonly List<Action<SubscriptionBusServiceConfigurator>> _eventSubscriptions;

        internal IReadOnlyList<Action<SubscriptionBusServiceConfigurator>> EventSubscriptions
        {
            get { return _eventSubscriptions; }
        }

        private readonly List<Action<SubscriptionBusServiceConfigurator>> _commandSubscriptions;

        internal IReadOnlyList<Action<SubscriptionBusServiceConfigurator>> CommandSubscriptions
        {
            get { return _commandSubscriptions; }
        }

        public void SubscribeToEvent<TEvent>() where TEvent : class, IEvent
        {
            if (_container == null)
            {
                throw new InvalidOperationException("Set the container before you subscribe to events");
            }

            _eventSubscriptions.Add(configurator => configurator.Handler<TEvent>((ctx, e) =>
            {
                IEventHandler<TEvent> handler = _container.Resolve<IEventHandler<TEvent>>();

                Task.WaitAll(HandleEvent(handler, e, ctx));

                _container.Release(handler);
            }));
        }

        public void SubscribeToEvent<TEvent, TEventHandler>() where TEvent : class, IEvent where TEventHandler : IEventHandler<TEvent>
        {
            if (_container == null)
            {
                throw new InvalidOperationException("Set the container before you subscribe to events");
            }

            _eventSubscriptions.Add(configurator => configurator.Handler<TEvent>((ctx, e) =>
            {
                IEventHandler<TEvent> handler = _container.Resolve<TEventHandler>();

                Task.WaitAll(HandleEvent(handler, e, ctx));

                _container.Release(handler);
            }));
        }

        public void SubscribeToEvent<TEvent, TEventHandler>(TEventHandler handler) where TEvent : class, IEvent where TEventHandler : IEventHandler<TEvent>
        {
            _eventSubscriptions.Add(configurator => configurator.Handler<TEvent>((ctx, e) =>
            {
                Task.WaitAll(HandleEvent(handler, e, ctx));
            }));
        }

        public void SubscribeToEvent<TEvent>(Func<EventContext<TEvent>, Task> handler) where TEvent : class, IEvent
        {
            var eventHandler = new DelegateEventHandler<TEvent>(handler);
            SubscribeToEvent<TEvent, DelegateEventHandler<TEvent>>(eventHandler);
        }

        private async Task HandleEvent<TEvent>(IEventHandler<TEvent> handler, TEvent eventMessage, IConsumeContext<TEvent> context) where TEvent : class, IEvent
        {
            try
            {
                var eventContext = new EventContext<TEvent>(eventMessage, Clock.Default.Now);
                await handler.Handle(eventContext);
            }
            catch (Exception ex)
            {
                context.RetryLater();
            }
        }

        public void SubscribeToCommand<TCommand>() where TCommand : class, ICommand
        {
            if (_container == null)
            {
                throw new InvalidOperationException("Set the container before you subscribe to commands");
            }

            _commandSubscriptions.Add(configurator => configurator.Handler<TCommand>((ctx, cmd) =>
            {
                ICommandHandler<TCommand> handler = _container.Resolve<ICommandHandler<TCommand>>();
                Task.WaitAll(HandleCommand(handler, cmd, ctx));
                _container.Release(handler);
            }));
        }

        public void SubscribeToCommand<TCommand, TCommandHandler>() where TCommand : class, ICommand where TCommandHandler : ICommandHandler<TCommand>
        {
            if (_container == null)
            {
                throw new InvalidOperationException("Set the container before you subscribe to commands");
            }

            _commandSubscriptions.Add(configurator => configurator.Handler<TCommand>((ctx, cmd) =>
            {
                ICommandHandler<TCommand> handler = _container.Resolve<TCommandHandler>();
                Task.WaitAll(HandleCommand(handler, cmd, ctx));
                _container.Release(handler);
            }));
        }

        public void SubscribeToCommand<TCommand, TCommandHandler>(TCommandHandler handler) where TCommand : class, ICommand where TCommandHandler : ICommandHandler<TCommand>
        {
            _commandSubscriptions.Add(configurator => configurator.Handler<TCommand>((ctx, cmd) =>
            {
                Task.WaitAll(HandleCommand(handler, cmd, ctx));
            }));
        }

        public void SubscribeToCommand<TCommand>(Func<CommandContext<TCommand>, Task> handler) where TCommand : class, ICommand
        {
            var commandHandler = new DelegateCommandHandler<TCommand>(handler);
            SubscribeToCommand<TCommand, DelegateCommandHandler<TCommand>>(commandHandler);
        }

        private async Task HandleCommand<TCommand>(ICommandHandler<TCommand> handler, TCommand commandMessage, IConsumeContext<TCommand> context) where TCommand : class, ICommand
        {
            try
            {
                var commandContext = new CommandContext<TCommand>(commandMessage, Clock.Default.Now);
                await handler.Handle(commandContext);
            }
            catch (Exception ex)
            {
                context.RetryLater();
            }
        }


    }
}