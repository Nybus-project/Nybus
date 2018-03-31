using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Nybus.Policies;

namespace Nybus.Configuration
{
    public interface INybusConfigurator
    {
        void AddServiceConfiguration(Action<IServiceCollection> configurator);

        void AddHostBuilderConfiguration(Action<NybusHostBuilder> configurator);
    }

    public class NybusConfigurator : INybusConfigurator
    {
        private readonly IList<Action<IServiceCollection>> _serviceConfigurations  = new List<Action<IServiceCollection>>();
        private readonly IList<Action<NybusHostBuilder>> _hostBuilderConfigurations  = new List<Action<NybusHostBuilder>>();

        public void ConfigureServices(IServiceCollection services)
        {
            foreach (var cfg in _serviceConfigurations)
                cfg(services);
        }

        public void ConfigureBuilder(NybusHostBuilder builder)
        {
            foreach (var cfg in _hostBuilderConfigurations)
                cfg(builder);
        }

        public void ConfigureOptions(NybusBusOptionsBuilder optionsBuilder)
        {
            _configureOptions?.Invoke(optionsBuilder);
        }

        private Action<NybusBusOptionsBuilder> _configureOptions;

        public void CustomizeOptions(Action<NybusBusOptionsBuilder> configureOptions)
        {
            _configureOptions = configureOptions ?? throw new ArgumentNullException(nameof(configureOptions));
        }

        public void AddServiceConfiguration(Action<IServiceCollection> configurator)
        {
            _serviceConfigurations.Add(configurator);
        }

        public void AddHostBuilderConfiguration(Action<NybusHostBuilder> configurator)
        {
            _hostBuilderConfigurations.Add(configurator);
        }
    }

    public static class NybusConfiguratorExtensions
    {
        public static void UseBusEngine<TEngine>(this INybusConfigurator configurator, Action<IServiceCollection> configureServices = null)
            where TEngine : class, IBusEngine
        {
            configurator.AddServiceConfiguration(svcs => svcs.AddSingleton<IBusEngine, TEngine>());

            if (configureServices != null)
                configurator.AddServiceConfiguration(configureServices);
        }

        public static void UseInMemoryBusEngine(this INybusConfigurator configurator)
        {
            configurator.UseBusEngine<InMemoryBusEngine>();
        }

        public static void RegisterPolicy<TPolicy>(this INybusConfigurator configurator) where TPolicy : class, IPolicy
        {

        }

        public static void SubscribeToCommand<TCommand, TCommandHandler>(this INybusConfigurator configurator)
            where TCommand : class, ICommand
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToCommand<TCommand>(typeof(TCommandHandler)));

            configurator.AddServiceConfiguration(services => services.AddTransient<TCommandHandler>());
        }

        public static void SubscribeToCommand<TCommand>(this INybusConfigurator configurator, CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand
        {
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToCommand(commandReceived));
        }

        public static void SubscribeToCommand<TCommand>(this INybusConfigurator configurator)
            where TCommand : class, ICommand
        {
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToCommand<TCommand>(typeof(ICommandHandler<TCommand>)));
        }

        public static void SubscribeToEvent<TEvent, TEventHandler>(this INybusConfigurator configurator)
            where TEvent : class, IEvent
            where TEventHandler : class, IEventHandler<TEvent>
        {
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToEvent<TEvent>(typeof(TEventHandler)));

            configurator.AddServiceConfiguration(services => services.AddTransient<TEventHandler>());
        }

        public static void SubscribeToEvent<TEvent>(this INybusConfigurator configurator, EventReceived<TEvent> eventReceived) where TEvent : class, IEvent
        {
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToEvent(eventReceived));
        }

        public static void SubscribeToEvent<TEvent>(this INybusConfigurator configurator)
            where TEvent : class, IEvent
        {
            configurator.AddHostBuilderConfiguration(builder => builder.SubscribeToEvent<TEvent>(typeof(IEventHandler<TEvent>)));
        }
    }
}
