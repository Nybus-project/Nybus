using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Topshelf;
using Topshelf.HostConfigurators;
using Topshelf.ServiceConfigurators;

namespace Nybus
{
    public static class HostConfiguratorExtensions
    {
        public static HostConfigurator UseStartup<TStartup>(this HostConfigurator configurator)
            where TStartup : Startup, new()
        {
            return configurator.Service<IBusHost>(UseStartup<TStartup>);
        }

        private static void UseStartup<TStartup>(ServiceConfigurator<IBusHost> configurator)
            where TStartup : Startup, new()
        {
            var startup = new TStartup();

            configurator.ConstructUsing(settings =>
            {
                var configurationBuilder = new ConfigurationBuilder();

                startup.ConfigureAppConfiguration(configurationBuilder);

                var configuration = configurationBuilder.Build();

                var context = new StartupContext(settings, configuration);

                var services = new ServiceCollection();
                services.AddSingleton(configuration);

                startup.ConfigureServices(context, services);

                services.AddLogging(builder => startup.ConfigureLogging(context, builder));

                var serviceProvider = services.BuildServiceProvider();

                return startup.ConstructService(context, serviceProvider);
            });

            configurator.WhenStarted(startup.OnStart);

            configurator.WhenStopped(startup.OnStop);
        }
    }
}