using System;
using System.Collections.Generic;
using MassTransit;
using MassTransit.SubscriptionConfigurators;
using Nybus.MassTransit;

namespace Nybus.Configuration
{
    public interface IServiceBusFactory
    {
        IServiceBus CreateServiceBus(MassTransitConnectionDescriptor connectionDescriptor, IQueueStrategy queueStrategy, IReadOnlyList<Action<SubscriptionBusServiceConfigurator>> subscriptions);
    }

    public class DefaultServiceBusFactory : IServiceBusFactory
    {
        public IServiceBus CreateServiceBus(MassTransitConnectionDescriptor connectionDescriptor, IQueueStrategy queueStrategy, IReadOnlyList<Action<SubscriptionBusServiceConfigurator>> subscriptions)
        {
            return ServiceBusFactory.New(bus =>
            {
                var receiveUri = new Uri(connectionDescriptor.Host, queueStrategy.GetQueueName());

                bus.UseRabbitMq(r =>
                {
                    r.ConfigureHost(receiveUri, h =>
                    {
                        h.SetUsername(connectionDescriptor.UserName);
                        h.SetPassword(connectionDescriptor.Password);
                    });
                });

                bus.ReceiveFrom(receiveUri);
                bus.UseJsonSerializer();

                foreach (var subscription in subscriptions)
                    bus.Subscribe(subscription);

                bus.Validate();
            });
        }
    }
}