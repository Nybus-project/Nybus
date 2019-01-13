using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nybus;
using Nybus.Configuration;

namespace Tests
{
    public static class TestUtils
    {
        public static IBusHost CreateNybusHost(Action<INybusConfigurator> configurator, Action<IServiceCollection> serviceRegistration = null)
        {
            var services = new ServiceCollection().AddLogging();

            serviceRegistration?.Invoke(services);

            services.AddNybus(configurator);

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            return host;
        }
    }
}