using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nybus.Policies;
using Nybus.Utils;

namespace Nybus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNybus(this IServiceCollection services, Action<INybusConfigurator> configure)
        {
            var configurator = new NybusConfigurator();

            configurator.RegisterErrorPolicyProvider<RetryErrorPolicyProvider>();
            configurator.RegisterErrorPolicyProvider<NoopErrorPolicyProvider>();
            
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
                var configuration = sp.GetRequiredService<INybusConfiguration>();

                configurator.ConfigureBuilder(builder);

                return builder.BuildHost(engine, sp, configuration);
            });

            services.AddSingleton<IBusHost>(sp => sp.GetRequiredService<NybusHost>());

            services.AddSingleton<IBus>(sp => sp.GetRequiredService<NybusHost>());

            return services;
        }
    }
}
