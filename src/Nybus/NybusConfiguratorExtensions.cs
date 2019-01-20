using System;
using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using Nybus.Filters;

namespace Nybus
{
    public static class NybusConfiguratorExtensions
    {
        #region Subscribe to Command

        public static void SubscribeToCommand<TCommand, TCommandHandler>(this INybusConfigurator configurator)
            where TCommand : class, ICommand
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            configurator.AddSubscription(builder => builder.SubscribeToCommand<TCommand>(typeof(TCommandHandler)));

            configurator.AddServiceConfiguration(services => services.AddTransient<TCommandHandler>());
        }

        public static void SubscribeToCommand<TCommand>(this INybusConfigurator configurator, CommandReceived<TCommand> commandReceived)
            where TCommand : class, ICommand
        {
            var handler = new DelegateWrapperCommandHandler<TCommand>(commandReceived);
            SubscribeToCommand<TCommand, DelegateWrapperCommandHandler<TCommand>>(configurator, handler);
        }

        public static void SubscribeToCommand<TCommand>(this INybusConfigurator configurator)
            where TCommand : class, ICommand
        {
            configurator.AddSubscription(builder => builder.SubscribeToCommand<TCommand>(typeof(ICommandHandler<TCommand>)));
        }

        public static void SubscribeToCommand<TCommand, TCommandHandler>(this INybusConfigurator configurator, TCommandHandler handler)
            where TCommand : class, ICommand
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            configurator.AddSubscription(builder => builder.SubscribeToCommand<TCommand>(typeof(TCommandHandler)));

            configurator.AddServiceConfiguration(services => services.AddSingleton(handler));
        }

        #endregion

        #region Subscribe to Event

        public static void SubscribeToEvent<TEvent, TEventHandler>(this INybusConfigurator configurator)
            where TEvent : class, IEvent
            where TEventHandler : class, IEventHandler<TEvent>
        {
            configurator.AddSubscription(builder => builder.SubscribeToEvent<TEvent>(typeof(TEventHandler)));

            configurator.AddServiceConfiguration(services => services.AddTransient<TEventHandler>());
        }

        public static void SubscribeToEvent<TEvent>(this INybusConfigurator configurator, EventReceived<TEvent> eventReceived)
            where TEvent : class, IEvent
        {
            var handler = new DelegateWrapperEventHandler<TEvent>(eventReceived);
            SubscribeToEvent<TEvent, DelegateWrapperEventHandler<TEvent>>(configurator, handler);
        }

        public static void SubscribeToEvent<TEvent>(this INybusConfigurator configurator)
            where TEvent : class, IEvent
        {
            configurator.AddSubscription(builder => builder.SubscribeToEvent<TEvent>(typeof(IEventHandler<TEvent>)));
        }

        public static void SubscribeToEvent<TEvent, TEventHandler>(this INybusConfigurator configurator, TEventHandler handler)
            where TEvent : class, IEvent
            where TEventHandler : class, IEventHandler<TEvent>
        {
            configurator.AddSubscription(builder => builder.SubscribeToEvent<TEvent>(typeof(TEventHandler)));

            configurator.AddServiceConfiguration(services => services.AddSingleton(handler));
        }

        public static void RegisterErrorFilterProvider<TProvider>(this INybusConfigurator configurator, Func<IServiceProvider, IErrorFilterProvider> factory = null) where TProvider : class, IErrorFilterProvider
        {
            if (factory == null)
            {
                configurator.AddServiceConfiguration(sc => sc.AddSingleton<IErrorFilterProvider, TProvider>());
            }
            else
            {
                configurator.AddServiceConfiguration(sc => sc.AddSingleton(factory));
            }
        }
        
        #endregion
    }
}