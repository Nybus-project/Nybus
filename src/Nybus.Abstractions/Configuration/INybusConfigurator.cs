using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nybus.Filters;
using Nybus.Policies;

namespace Nybus.Configuration
{
    public interface INybusConfigurator
    {
        void UseConfiguration(IConfiguration configuration, string sectionName = "Nybus");

        void AddServiceConfiguration(Action<IServiceCollection> configurator);

        void AddSubscription(Action<ISubscriptionBuilder> configurator);

        IConfiguration Configuration { get; }

        void Configure(Action<INybusConfiguration> configuration);
    }

    public interface INybusConfiguration
    {
        IErrorPolicy ErrorPolicy { get; set; }

        IReadOnlyList<IErrorFilter> CommandErrorFilters { get; set; }

        IReadOnlyList<IErrorFilter> EventErrorFilters { get; set; }
    }

    public static class NybusConfiguratorExtensions
    {
        public static void UseBusEngine<TEngine>(this INybusConfigurator configurator, Action<IServiceCollection> configureServices = null)
            where TEngine : class, IBusEngine
        {
            configurator.AddServiceConfiguration(services => services.AddSingleton<IBusEngine, TEngine>());

            if (configureServices != null)
            {
                configurator.AddServiceConfiguration(configureServices);
            }
        }
    }
}