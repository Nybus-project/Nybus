using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nybus.Policies;

namespace Nybus.Configuration
{
    public static class NybusConfiguratorExtensions
    {
        public static void UseBusEngine<TEngine>(this INybusConfigurator configurator, Action<IServiceCollection> configureServices = null)
            where TEngine : class, IBusEngine
        {
            configurator.AddServiceConfiguration(svcs => svcs.AddSingleton<IBusEngine, TEngine>());

            if (configureServices != null)
            {
                configurator.AddServiceConfiguration(configureServices);
            }
        }

        public static void UseInMemoryBusEngine(this INybusConfigurator configurator)
        {
            configurator.UseBusEngine<InMemoryBusEngine>();
        }

        public static void UseErrorPolicy<TErrorPolicy>(this INybusConfigurator configurator, Action<IServiceCollection> configureServices = null)
            where TErrorPolicy : class, IErrorPolicy
        {
            configurator.AddOptionsConfiguration((services, options) =>
            {
                var errorPolicy = services.GetRequiredService<TErrorPolicy>();

                options.ErrorPolicy = errorPolicy;
            });

            configurator.AddServiceConfiguration(services => services.AddSingleton<TErrorPolicy>());

            if (configureServices != null)
            {
                configurator.AddServiceConfiguration(configureServices);
            }
        }

        public static void UseRetryErrorPolicy(this INybusConfigurator configurator)
        {
            UseErrorPolicy<RetryErrorPolicy>(configurator, services => services.Configure<RetryErrorPolicyOptions>(configurator.Configuration.GetSection("RetryPolicy")));
        }

        #region Subscribe to Command

        public static void SubscribeToCommand<TCommand, TCommandHandler>(this INybusConfigurator configurator)
            where TCommand : class, ICommand
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToCommand<TCommand>(typeof(TCommandHandler)));

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
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToCommand<TCommand>(typeof(ICommandHandler<TCommand>)));
        }

        public static void SubscribeToCommand<TCommand, TCommandHandler>(this INybusConfigurator configurator, TCommandHandler handler)
            where TCommand : class, ICommand
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToCommand<TCommand>(typeof(TCommandHandler)));

            configurator.AddServiceConfiguration(services => services.AddSingleton(handler));
        }

        #endregion

        #region Subscribe to Event

        public static void SubscribeToEvent<TEvent, TEventHandler>(this INybusConfigurator configurator)
            where TEvent : class, IEvent
            where TEventHandler : class, IEventHandler<TEvent>
        {
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToEvent<TEvent>(typeof(TEventHandler)));

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
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToEvent<TEvent>(typeof(IEventHandler<TEvent>)));
        }

        public static void SubscribeToEvent<TEvent, TEventHandler>(this INybusConfigurator configurator, TEventHandler handler)
            where TEvent : class, IEvent
            where TEventHandler : class, IEventHandler<TEvent>
        {
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToEvent<TEvent>(typeof(TEventHandler)));

            configurator.AddServiceConfiguration(services => services.AddSingleton(handler));
        }

        #endregion
    }

    public class DelegateWrapperCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        private readonly CommandReceived<TCommand> _handler;

        public DelegateWrapperCommandHandler(CommandReceived<TCommand> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task HandleAsync(IDispatcher dispatcher, ICommandContext<TCommand> incomingCommand)
        {
            return _handler.Invoke(dispatcher, incomingCommand);
        }
    }

    public class DelegateWrapperEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : class, IEvent
    {
        private readonly EventReceived<TEvent> _handler;

        public DelegateWrapperEventHandler(EventReceived<TEvent> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task HandleAsync(IDispatcher dispatcher, IEventContext<TEvent> incomingEvent)
        {
            return _handler.Invoke(dispatcher, incomingEvent);
        }
    }
}