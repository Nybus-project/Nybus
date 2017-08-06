using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using Nybus.Policies;
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

            services.AddSingleton<NybusBusOptionsBuilder>();

            services.AddSingleton(sp => 
            {
                var optionBuilder = sp.GetRequiredService<NybusBusOptionsBuilder>();
                configurator.ConfigureOptions(optionBuilder);
                return optionBuilder.Build();
            });

            configurator.ConfigureServices(services);

            services.AddSingleton(sp => 
            {
                var engine = sp.GetRequiredService<IBusEngine>();
                var builder = sp.GetRequiredService<NybusBusBuilder>();
                var options = sp.GetRequiredService<NybusBusOptions>();

                configurator.ConfigureBuilder(builder);

                return builder.Build(engine, options);
            });

            return services;
        }

        public static void UseInMemoryBusEngine(this INybusConfigurator configurator)
        {
            configurator.UseBusEngine<InMemoryBusEngine>();
        }

        public static void RegisterPolicy<TPolicy>(this INybusConfigurator configurator) where TPolicy : class, IPolicy
        {

        }
    }
}
