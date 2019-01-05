using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;

namespace Tests
{
    public class TestNybusConfigurator : INybusConfigurator
    {
        public void UseConfiguration(Microsoft.Extensions.Configuration.IConfiguration configuration, string sectionName = "Nybus")
        {
            Configuration = configuration.GetSection(sectionName);
        }

        private readonly List<Action<IServiceCollection>> _serviceConfigurations = new List<Action<IServiceCollection>>();

        public void AddServiceConfiguration(Action<IServiceCollection> configurator)
        {
            _serviceConfigurations.Add(configurator);
        }

        public void ApplyServiceConfigurations(IServiceCollection services)
        {
            foreach (var sc in _serviceConfigurations)
                sc(services);
        }

        private readonly List<Action<ISubscriptionBuilder>> _subscriptionBuilders = new List<Action<ISubscriptionBuilder>>();

        public void AddSubscription(Action<ISubscriptionBuilder> configurator)
        {
            _subscriptionBuilders.Add(configurator);
        }

        public void ApplySubscriptions(ISubscriptionBuilder builder)
        {
            foreach (var sb in _subscriptionBuilders)
                sb(builder);
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; private set; }

        public void Configure(Action<INybusConfiguration> configuration)
        {
            throw new NotImplementedException();
        }
    }
}
