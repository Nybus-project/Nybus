using Microsoft.Extensions.DependencyInjection;
using System;

namespace Nybus.Configuration
{
    public interface INybusConfigurator
    {
        void UseBusEngine<TEngine>(Action<IServiceCollection> configureServices = null) where TEngine : class, IBusEngine;

        void SubscribeToCommand<TCommand, TCommandHandler>()
            where TCommand : class, ICommand
            where TCommandHandler : class, ICommandHandler<TCommand>;

        void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent, TEventHandler>()
            where TEvent : class, IEvent
            where TEventHandler : class, IEventHandler<TEvent>;

        void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent;
    }
}
