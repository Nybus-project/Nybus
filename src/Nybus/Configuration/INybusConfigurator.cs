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

        void SubscribeToEvent<TEvent, TEventHandler>()
            where TEvent : class, IEvent
            where TEventHandler : class, IEventHandler<TEvent>;
    }
}
