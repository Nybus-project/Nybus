using System;
using System.Threading.Tasks;
using Nybus.Container;

namespace Nybus.Configuration
{
    public interface IBusBuilder
    {
        void SubscribeToEvent<TEvent>()
            where TEvent : class, IEvent;

        void SubscribeToEvent<TEventHandler, TEvent>()
            where TEventHandler : IEventHandler<TEvent>
            where TEvent : class, IEvent;

        void SubscribeToEvent<TEventHandler, TEvent>(TEventHandler handler)
            where TEventHandler : IEventHandler<TEvent>
            where TEvent : class, IEvent;

        void SubscribeToEvent<TEvent>(Func<EventContext<TEvent>, Task> handler)
            where TEvent : class, IEvent;

        void SubscribeToCommand<TCommand>()
            where TCommand : class, ICommand;

        void SubscribeToCommand<TCommandHandler, TCommand>()
            where TCommandHandler : ICommandHandler<TCommand>
            where TCommand : class, ICommand;

        void SubscribeToCommand<TCommandHandler, TCommand>(TCommandHandler handler)
            where TCommandHandler : ICommandHandler<TCommand>
            where TCommand : class, ICommand;

        void SubscribeToCommand<TCommand>(Func<CommandContext<TCommand>, Task> handler)
            where TCommand : class, ICommand;

        IBus Build();
    }
}