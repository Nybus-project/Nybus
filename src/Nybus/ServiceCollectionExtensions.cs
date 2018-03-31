using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using Nybus.Policies;
using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Nybus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNybus(this IServiceCollection services, Action<INybusConfigurator> configuration)
        {
            var configurator = new NybusConfigurator();
            configurator.UseErrorPolicy<NoopErrorPolicy>();

            configuration(configurator);

            services.AddSingleton(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var builder = new NybusHostBuilder(sp, loggerFactory);

                return builder;
            });

            services.AddSingleton(sp =>
            {
                var options = new NybusHostOptions();
                configurator.ConfigureOptions(sp, options);

                return options;
            });

            configurator.ConfigureServices(services);

            services.AddSingleton(sp => 
            {
                var engine = sp.GetRequiredService<IBusEngine>();
                var builder = sp.GetRequiredService<NybusHostBuilder>();
                var options = sp.GetRequiredService<NybusHostOptions>();

                configurator.ConfigureBuilder(builder);

                return builder.BuildHost(engine, options);
            });

            services.AddSingleton<IBusHost>(sp => sp.GetRequiredService<NybusHost>());

            services.AddSingleton<IBus>(sp => sp.GetRequiredService<NybusHost>());

            return services;
        }
    }
}
