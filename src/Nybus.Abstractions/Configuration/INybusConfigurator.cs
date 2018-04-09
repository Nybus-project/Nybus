using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nybus.Policies;

namespace Nybus.Configuration
{
    public interface INybusConfigurator
    {
        void UseConfiguration(IConfiguration configuration, string sectionName = "Nybus");

        void AddServiceConfiguration(Action<IServiceCollection> configurator);

        void AddSubscription(Action<ISubscriptionBuilder> configurator);

        void SetErrorPolicy(Func<IServiceProvider, IErrorPolicy> policyGenerator);

        IConfiguration Configuration { get; }
    }

    public static class NybusConfiguratorExtensions
    {
        public static void UseBusEngine<TEngine>(this INybusConfigurator configurator, Action<IServiceCollection> configureServices = null)
            where TEngine : class, IBusEngine
        {
            configurator.AddServiceConfiguration(svcs => svcs.AddSingleton<IBusEngine, TEngine>());

            if (configureServices != null)
            {
                configurator.AddServiceConfiguration(configureServices);
            }
        }
    }
}