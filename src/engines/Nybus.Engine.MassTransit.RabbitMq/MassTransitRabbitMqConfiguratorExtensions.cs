using System;
using Nybus.Configuration;
using Nybus.MassTransit.RabbitMq;

namespace Nybus
{
    public static class MassTransitRabbitMqConfiguratorExtensions
    {
        public static void UseMassTransitWithRabbitMq(this INybusConfigurator nybus, Action<IMassTransitRabbitMqConfigurator> configure = null)
        {
            var configurator = new MassTransitRabbitMqConfigurator();

            configure?.Invoke(configurator);

            configurator.Apply(nybus);

            nybus.UseBusEngine<MassTransitRabbitMqBusEngine>();
        }
        
    }
}