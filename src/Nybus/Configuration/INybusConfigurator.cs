using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nybus.Configuration {
    public interface INybusConfigurator
    {
        void UseConfiguration(IConfiguration configuration, string sectionName = "Nybus");

        void AddServiceConfiguration(Action<IServiceCollection> configurator);

        void AddHostBuilderConfiguration(Action<NybusHostBuilder> configurator);

        void AddOptionsConfiguration(Action<IServiceProvider, NybusHostOptions> configurator);

        IConfiguration Configuration { get; }
    }
}