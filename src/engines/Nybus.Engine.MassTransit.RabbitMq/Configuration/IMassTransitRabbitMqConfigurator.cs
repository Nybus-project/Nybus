using System;
using MassTransit.RabbitMqTransport;

namespace Nybus.Configuration
{
    public interface IMassTransitRabbitMqConfigurator
    {
        void Configure(Action<IRabbitMqBusFactoryConfigurator> configureBus);
    }
}