using System;
using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using Nybus.MassTransit;
using Nybus.MassTransit.RabbitMq;

namespace Nybus
{
    public static class MassTransitRabbitMqConfiguratorExtensions
    {
        public static void UseMassTransitWithRabbitMq(this INybusConfigurator nybus, Action<IMassTransitRabbitMqConfigurator> configure = null)
        {
            nybus.AddServiceConfiguration(svc => svc.AddSingleton<IMassTransitRabbitMqBusBuilder, MassTransitRabbitMqBusBuilder>());

            nybus.UseBusEngine<MassTransitRabbitMqBusEngine>();
        }
        
    }
}