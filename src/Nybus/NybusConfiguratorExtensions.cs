using System;
using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using Nybus.Policies;

namespace Nybus
{
    public static class NybusConfiguratorExtensions
    {
        public static void UseInMemoryBusEngine(this INybusConfigurator configurator)
        {
            configurator.UseBusEngine<InMemoryBusEngine>();
        }

        public static void UseErrorPolicy<TErrorPolicy>(this INybusConfigurator configurator, Action<IServiceCollection> configureServices = null)
            where TErrorPolicy : class, IErrorPolicy
        {
            configurator.SetErrorPolicy(sp => sp.GetRequiredService<TErrorPolicy>());

            configurator.AddServiceConfiguration(services => services.AddSingleton<TErrorPolicy>());

            if (configureServices != null)
            {
                configurator.AddServiceConfiguration(configureServices);
            }
        }

        public static void UseRetryErrorPolicy(this INybusConfigurator configurator, string sectionName = "RetryPolicy")
        {
            if (sectionName == null)
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            UseErrorPolicy<RetryErrorPolicy>(configurator, services => services.Configure<RetryErrorPolicyOptions>(configurator.Configuration.GetSection(sectionName)));
        }

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

        #endregion
    }
}