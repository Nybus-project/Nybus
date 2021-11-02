using System;
using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using Nybus.RabbitMq;

namespace Nybus
{
    public static class RabbitMqConfiguratorExtensions
    {
        public static void UseRabbitMqBusEngine(this INybusConfigurator nybus, Action<IRabbitMqConfigurator> configure = null)
        {
            nybus.AddServiceConfiguration(svc => svc.AddSingleton<IConfigurationFactory, ConfigurationFactory>());

            nybus.AddServiceConfiguration(svc => svc.AddSingleton<IConnectionFactoryProviders, ConnectionFactoryProviders>());

            var configurator = new RabbitMqConfigurator();

            configurator.RegisterQueueFactoryProvider<StaticQueueFactoryProvider>();

            configurator.RegisterQueueFactoryProvider<TemporaryQueueFactoryProvider>();

            configurator.RegisterQueueFactoryProvider<PrefixedTemporaryQueueFactoryProvider>();

            configure?.Invoke(configurator);

            configurator.Apply(nybus);

            nybus.UseBusEngine<RabbitMqBusEngine>();
        }
    }


}
