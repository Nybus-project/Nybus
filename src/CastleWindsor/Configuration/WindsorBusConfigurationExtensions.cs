using Castle.Windsor;
using Nybus.Container;

namespace Nybus.Configuration
{
    public static class WindsorBusConfigurationExtensions
    {
        public static void SetContainer(this IBusConfiguration configuration, IWindsorContainer container)
        {
            var brokerContainer = new WindsorBusContainer(container.Kernel);
            configuration.SetContainer(brokerContainer);
        }
    }
}