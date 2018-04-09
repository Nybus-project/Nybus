using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using RabbitMQ.Client;

namespace Nybus
{
    public static class RabbitMqConfiguratorExtensions
    {
        public static void UseRabbitMqBusEngine(this INybusConfigurator configurator, Action<RabbitMqConfigurator> configure = null)
        {
            configurator.UseBusEngine<RabbitMqBusEngine>(svc => svc.AddSingleton(new RabbitMqBusEngineOptions { CommandQueueName = "test-queue" }));
        }
    }

    public class RabbitMqConfigurator
    {
        public void Connection(Action<ConnectionFactory> configurator)
        {

        }

        public void Connection(string sectionName = "RabbitMq")
        {

        }
    }
}
