using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface IRabbitMqConfigurator
    {
        void RegisterQueueFactoryProvider<TProvider>(Func<IServiceProvider, IQueueFactoryProvider> factory = null)
            where TProvider : class, IQueueFactoryProvider;

        void Configure(Action<IRabbitMqConfiguration> configure);

        void UseConfiguration(string sectionName = "RabbitMq");

    }

    public class RabbitMqConfigurator : IRabbitMqConfigurator
    {
        private readonly List<Action<IServiceCollection>> _queueFactoryProviderRegistrations = new List<Action<IServiceCollection>>();

        public void RegisterQueueFactoryProvider<TProvider>(Func<IServiceProvider, IQueueFactoryProvider> factory = null)
            where TProvider : class, IQueueFactoryProvider
        {
            if (factory == null)
            {
                _queueFactoryProviderRegistrations.Add(sp => sp.AddSingleton<IQueueFactoryProvider, TProvider>());
            }
            else
            {
                _queueFactoryProviderRegistrations.Add(sp => sp.AddSingleton<IQueueFactoryProvider>(factory));
            }
        }

        private Action<IRabbitMqConfiguration> _configurationAction;

        public void Configure(Action<IRabbitMqConfiguration> configure)
        {
            _configurationAction = configure ?? throw new ArgumentNullException(nameof(configure));
        }

        private string _configurationSectionName;

        public void UseConfiguration(string sectionName = "RabbitMq")
        {
            _configurationSectionName = sectionName ?? throw new ArgumentNullException(nameof(sectionName));
        }

        public void Apply(INybusConfigurator nybus)
        {
            var options = new RabbitMqOptions();

            if (_configurationSectionName != null && nybus.Configuration.TryGetSection(_configurationSectionName, out var configurationSection))
            {
                configurationSection.Bind(options);
            }

            nybus.AddServiceConfiguration(sc => sc.AddSingleton(options));

            nybus.AddServiceConfiguration(sc => sc.AddTransient<IRabbitMqConfiguration>(sp =>
            {
                var factory = sp.GetRequiredService<IConfigurationFactory>();

                var op = sp.GetRequiredService<RabbitMqOptions>();

                var configuration = factory.Create(op);

                _configurationAction?.Invoke(configuration);

                return configuration;
            }));

            foreach (var registration in _queueFactoryProviderRegistrations)
            {
                nybus.AddServiceConfiguration(registration);
            }
        }
    }
}