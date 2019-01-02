using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Nybus.Configuration
{
    public class NybusConfigurator : INybusConfigurator
    {
        private readonly IList<Action<IServiceCollection>> _serviceConfigurations  = new List<Action<IServiceCollection>>();
        private readonly IList<Action<ISubscriptionBuilder>> _hostBuilderConfigurations  = new List<Action<ISubscriptionBuilder>>();
        private readonly IList<Action<IServiceProvider, INybusConfiguration>> _configurationCustomizations = new List<Action<IServiceProvider, INybusConfiguration>>();

        public void ConfigureServices(IServiceCollection services)
        {
            foreach (var serviceConfiguration in _serviceConfigurations)
            {
                serviceConfiguration(services);
            }
        }

        public void ConfigureBuilder(ISubscriptionBuilder builder)
        {
            foreach (var subscriptionConfiguration in _hostBuilderConfigurations)
            {
                subscriptionConfiguration(builder);
            }
        }

        public IConfiguration Configuration { get; private set; }

        public void Configure(Action<INybusConfiguration> configuration)
        {
            _configurationCustomizations.Add((provider, config) => configuration(config));
        }

        public void UseConfiguration(IConfiguration configuration, string sectionName = "Nybus")
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (sectionName == null)
            {
                throw new ArgumentNullException(nameof(sectionName));
            }

            Configuration = configuration.GetSection(sectionName);
        }

        public void AddServiceConfiguration(Action<IServiceCollection> configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            _serviceConfigurations.Add(configurator);
        }

        public void AddSubscription(Action<ISubscriptionBuilder> configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            _hostBuilderConfigurations.Add(configurator);
        }

        public void CustomizeConfiguration(IServiceProvider serviceProvider, INybusConfiguration configuration)
        {
            foreach (var customization in _configurationCustomizations)
            {
                customization(serviceProvider, configuration);
            }
        }
    }
}
