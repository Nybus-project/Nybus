using System;
using System.Collections;
using System.Collections.Generic;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;
using Nybus.MassTransit.RabbitMq;

namespace Nybus.Configuration
{
    public interface IMassTransitRabbitMqConfigurator
    {
        void ConfigureMassTransit(Action<IRabbitMqBusFactoryConfigurator> configureMassTransit);

        //void UseConfiguration(string sectionName = "MassTransit");


    }

    public class MassTransitRabbitMqConfigurator : IMassTransitRabbitMqConfigurator
    {
        private readonly IList<Action<IRabbitMqBusFactoryConfigurator>> _configurationActions = new List<Action<IRabbitMqBusFactoryConfigurator>>();

        public void ConfigureMassTransit(Action<IRabbitMqBusFactoryConfigurator> configureMassTransit)
        {
            if (configureMassTransit == null)
            {
                throw new ArgumentNullException(nameof(configureMassTransit));
            }

            _configurationActions.Add(configureMassTransit);
        }

        private string _configurationSectionName;

        public void UseConfiguration(string sectionName = "MassTransit")
        {
            _configurationSectionName = sectionName ?? throw new ArgumentNullException(nameof(sectionName));
        }

        public void Apply(INybusConfigurator nybus)
        {
            var busBuilder = new MassTransitRabbitMqBusBuilder();

            foreach (var action in _configurationActions)
            {
                busBuilder.AddConfiguration(action);
            }

            nybus.AddServiceConfiguration(svc => svc.AddSingleton<IMassTransitRabbitMqBusBuilder>(busBuilder));
        }
    }
}