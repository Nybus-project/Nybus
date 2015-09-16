using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Messages;
using Nybus;
using WebProducer.Handlers;

namespace WebProducer.Installers
{
    public class BusHandlerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IEventHandler<StringReversedEvent>>()
                .ImplementedBy<StringReversedEventHandler>()
                .LifestyleTransient());
        }
    }
}