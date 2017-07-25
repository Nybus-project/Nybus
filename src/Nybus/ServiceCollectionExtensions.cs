using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using System;
using System.Text;

namespace Nybus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNybus(this IServiceCollection services, Action<INybusConfigurator> configuration)
        {
            var configurator = new NybusConfigurator();

            configuration(configurator);

            services.AddSingleton<NybusBusBuilder>();

            configurator.ConfigureServices(services);

            services.AddSingleton(sp => 
            {
                var engine = sp.GetRequiredService<IBusEngine>();
                var builder = sp.GetRequiredService<NybusBusBuilder>();

                configurator.ConfigureBuilder(builder);

                return builder.Build(engine);
            });

            return services;
        }

        public static void UseInMemoryBusEngine(this INybusConfigurator configurator)
        {
            configurator.UseBusEngine<InMemoryBusEngine>();
        }
    }
}
