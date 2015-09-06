using System;
using System.Threading.Tasks;
using Nybus.Container;

namespace Nybus.Configuration
{
    public interface IBusConfiguration
    {
        void SetSharedQueueName(string queueName);

        void SetContainer(IContainer container);

        void SubscribeToEvent<TEvent>() where TEvent : class, IEvent;

        void SubscribeToEvent<TEvent, TEventHandler>() where TEvent : class, IEvent where TEventHandler : IEventHandler<TEvent>;

        void SubscribeToEvent<TEvent, TEventHandler>(TEventHandler handler) where TEvent : class, IEvent where TEventHandler : IEventHandler<TEvent>;

        void SubscribeToEvent<TEvent>(Func<EventContext<TEvent>, Task> handler) where TEvent : class, IEvent;

        void SubscribeToCommand<TCommand>() where TCommand : class, ICommand;

        void SubscribeToCommand<TCommand, TCommandHandler>() where TCommand : class, ICommand where TCommandHandler : ICommandHandler<TCommand>;

        void SubscribeToCommand<TCommand, TCommandHandler>(TCommandHandler handler) where TCommand : class, ICommand where TCommandHandler : ICommandHandler<TCommand>;

        void SubscribeToCommand<TCommand>(Func<CommandContext<TCommand>, Task> handler) where TCommand : class, ICommand;
    }
}