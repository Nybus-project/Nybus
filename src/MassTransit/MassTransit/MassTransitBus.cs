using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Nybus.Configuration;

namespace Nybus.MassTransit
{
    public class MassTransitBus : IBus
    {
        private readonly MassTransitBusConnectionDescriptor _connectionDescriptor;
        private readonly MassTransitRabbitMqBusConfiguration _configuration;

        private readonly List<IServiceBus> _serviceBusses = new List<IServiceBus>();
        private BusStatus _status = BusStatus.New;

        public MassTransitBus(MassTransitBusConnectionDescriptor connectionDescriptor, MassTransitRabbitMqBusConfiguration configuration)
        {
            if (connectionDescriptor == null) throw new ArgumentNullException(nameof(connectionDescriptor));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            _connectionDescriptor = connectionDescriptor;
            _configuration = configuration;
        }

        private void EnsureBrokerIsRunning()
        {
            if (_status == BusStatus.New)
            {
                throw new InvalidOperationException("Broker not started");
            }
        }


        public Task InvokeCommand<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            EnsureBrokerIsRunning();
            _serviceBusses[0].Publish(command);
            return Task.FromResult(0);
        }

        public Task RaiseEvent<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            EnsureBrokerIsRunning();
            _serviceBusses[0].Publish(@event);
            return Task.FromResult(0);
        }

        public IHandle Start()
        {
            try
            {
                _serviceBusses.Add(ServiceBusFactory.New(pbc =>
                {
                    var receiveUri = new Uri(_connectionDescriptor.Host, "*?temporary=true");

                    pbc.UseRabbitMq(r =>
                    {
                        r.ConfigureHost(receiveUri, h =>
                        {
                            h.SetUsername(_connectionDescriptor.UserName);
                            h.SetPassword(_connectionDescriptor.Password);
                        });
                    });

                    pbc.ReceiveFrom(receiveUri);
                    pbc.UseJsonSerializer();

                    foreach (var subscription in _configuration.EventSubscriptions)
                        pbc.Subscribe(subscription);

                    pbc.Validate();
                }));

                if (_configuration.CommandSubscriptions.Any())
                {
                    _serviceBusses.Add(ServiceBusFactory.New(sbc =>
                    {
                        var receiveUri = new Uri(_connectionDescriptor.Host, _configuration.SharedQueueName);

                        sbc.UseRabbitMq(r => r.ConfigureHost(receiveUri, h =>
                        {
                            h.SetUsername(_connectionDescriptor.UserName);
                            h.SetPassword(_connectionDescriptor.Password);
                        }));

                        sbc.ReceiveFrom(receiveUri);
                        sbc.UseJsonSerializer();

                        foreach (var subscription in _configuration.CommandSubscriptions)
                            sbc.Subscribe(subscription);

                        sbc.Validate();
                    }));
                }

                _status = BusStatus.Running;

                return new MassTransitHandle(_serviceBusses);
            }
            catch (Exception ex)
            {
                throw new Exception("Bus failed to start", ex);
            }

        }

        public enum BusStatus
        {
            New,
            Running
        }
    }
}
