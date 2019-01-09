using Microsoft.Extensions.DependencyInjection;
using Nybus.Configuration;
using Nybus.InMemory;

namespace Nybus
{
    public static class InMemoryConfiguratorExtensions
    {
        public static void UseInMemoryBusEngine(this INybusConfigurator configurator)
        {
            configurator.AddServiceConfiguration(svc => svc.AddSingleton<ISerializer, JsonSerializer>());

            configurator.AddServiceConfiguration(svc => svc.AddSingleton<IEnvelopeService, EnvelopeService>());

            configurator.UseBusEngine<InMemoryBusEngine>();
        }
    }
}
