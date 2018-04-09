using System;

namespace Nybus.Configuration
{
    public interface ISubscriptionBuilder
    {
        void SubscribeToCommand<TCommand>(Type commandHandlerType)
            where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent>(Type eventHandlerType)
            where TEvent : class, IEvent;
    }
}