using System;
using Nybus.MassTransit;

namespace Nybus.Configuration
{
    public class MassTransitBusBuilder : IBusBuilder<MassTransitRabbitMqBusConfiguration>
    {
        private readonly MassTransitBusConnectionDescriptor _connectionDescriptor;

        public MassTransitBusBuilder(MassTransitBusConnectionDescriptor connectionDescriptor)
        {
            if (connectionDescriptor == null)
            {
                throw new ArgumentNullException(nameof(connectionDescriptor));
            }
            _connectionDescriptor = connectionDescriptor;
        }

        public IBus Build(Action<MassTransitRabbitMqBusConfiguration> configurator = null)
        {
            var configuration = new MassTransitRabbitMqBusConfiguration();

            configurator?.Invoke(configuration);

            return new MassTransitBus(_connectionDescriptor, configuration);
        }
    }
}