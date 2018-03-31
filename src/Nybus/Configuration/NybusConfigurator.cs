using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Nybus.Configuration
{
    public class NybusConfigurator : INybusConfigurator
    {
        private readonly IList<Action<IServiceCollection>> _serviceConfigurations  = new List<Action<IServiceCollection>>();
        private readonly IList<Action<NybusHostBuilder>> _hostBuilderConfigurations  = new List<Action<NybusHostBuilder>>();
        private readonly IList<Action<IServiceProvider, NybusHostOptions>> _optionsConfigurations = new List<Action<IServiceProvider, NybusHostOptions>>();

        public void ConfigureServices(IServiceCollection services)
        {
            foreach (var cfg in _serviceConfigurations)
                cfg(services);
        }

        public void ConfigureBuilder(NybusHostBuilder builder)
        {
            foreach (var cfg in _hostBuilderConfigurations)
                cfg(builder);
        }

        public void ConfigureOptions(IServiceProvider serviceProvider, NybusHostOptions options)
        {
            foreach (var cfg in _optionsConfigurations)
                cfg(serviceProvider, options);
        }

        public IConfiguration Configuration { get; private set; }

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

        public void AddHostBuilderConfiguration(Action<NybusHostBuilder> configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            _hostBuilderConfigurations.Add(configurator);
        }

        public void AddOptionsConfiguration(Action<IServiceProvider, NybusHostOptions> configurator)
        {
            if (configurator == null)
            {
                throw new ArgumentNullException(nameof(configurator));
            }

            _optionsConfigurations.Add(configurator);
        }
    }
}
