using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nybus.Filters;
using Nybus.Utils;

namespace Nybus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNybus(this IServiceCollection services, Action<INybusConfigurator> configure)
        {
            var configurator = new NybusConfigurator();

            configurator.RegisterErrorFilterProvider<RetryErrorFilterProvider>();
            configurator.RegisterErrorFilterProvider<DiscardErrorFilterProvider>();
            configurator.RegisterErrorFilterProvider<DeadLetterQueueErrorFilterProvider>();

            services.AddSingleton<DiscardErrorFilter>();
            
            configure(configurator);

            services.AddSingleton(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var builder = new NybusHostBuilder(loggerFactory);

                return builder;
            });

            services.AddSingleton<INybusHostConfigurationFactory, NybusHostConfigurationFactory>();

            services.AddSingleton(sp =>
            {
                var factory = sp.GetRequiredService<INybusHostConfigurationFactory>();
                var options = sp.GetRequiredService<NybusHostOptions>();

                var configuration = factory.CreateConfiguration(options);

                configurator.CustomizeConfiguration(sp, configuration);

                return configuration;
            });

            services.AddSingleton(sp =>
            {
                var options = new NybusHostOptions();

                configurator.Configuration?.Bind(options);
                
                return options;
            });

            services.AddSingleton<IMessageDescriptorStore, MessageDescriptorStore>();

            configurator.ConfigureServices(services);

            services.AddSingleton(sp => 
            {
                var engine = sp.GetRequiredService<IBusEngine>();
                var builder = sp.GetRequiredService<NybusHostBuilder>();
                var configuration = sp.GetRequiredService<NybusConfiguration>();

                configurator.ConfigureBuilder(builder);

                return builder.BuildHost(engine, sp, configuration);
            });

            services.AddSingleton<IBusHost>(sp => sp.GetRequiredService<NybusHost>());

            services.AddSingleton<IBus>(sp => sp.GetRequiredService<NybusHost>());

            return services;
        }

        public static IServiceCollection AddCommandHandler<TCommand, THandler>(this IServiceCollection services)
            where TCommand : class, ICommand
            where THandler : class, ICommandHandler<TCommand>
        {
            return services.AddTransient<ICommandHandler<TCommand>, THandler>();
        }

        public static IServiceCollection AddCommandHandler<THandler>(this IServiceCollection services)
            where THandler : class, ICommandHandler
        {
            return AddCommandHandler(services, typeof(THandler));
        }

        public static IServiceCollection AddCommandHandler(this IServiceCollection services, Type handlerType)
        {
            var interfaces = from i in handlerType.GetInterfaces()
                             where i.IsGenericType
                             let definition = i.GetGenericTypeDefinition()
                             where definition == typeof(ICommandHandler<>)
                             select i;

            foreach (var i in interfaces)
            {
                services.AddTransient(i, handlerType);
            }

            return services;
        }

        public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : class, IEvent
            where THandler : class, IEventHandler<TEvent>
        {
            return services.AddTransient<IEventHandler<TEvent>, THandler>();
        }

        public static IServiceCollection AddEventHandler<THandler>(this IServiceCollection services)
            where THandler : class, IEventHandler
        {
            return AddEventHandler(services, typeof(THandler));
        }

        public static IServiceCollection AddEventHandler(this IServiceCollection services, Type handlerType)
        {
            var interfaces = from i in handlerType.GetInterfaces()
                             where i.IsGenericType
                             let definition = i.GetGenericTypeDefinition()
                             where definition == typeof(IEventHandler<>)
                             select i;

            foreach (var i in interfaces)
            {
                services.AddTransient(i, handlerType);
            }

            return services;
        }
    }
}
