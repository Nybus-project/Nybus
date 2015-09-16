using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Messages;
using Nybus;
using Nybus.Configuration;
using Nybus.Container;
using Nybus.Logging;
using Nybus.MassTransit;

namespace Consumer
{
    public class AppInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<ILogger>().ImplementedBy<ConsoleLogger>());

            container.Register(Component.For<ServiceHost>());

            container.Register(Component.For<IBus>().UsingFactory((IBusBuilder builder) => builder.Build()).LifeStyle.Singleton);

            container.Register(Component.For<IBusBuilder>().ImplementedBy<NybusBusBuilder>().OnCreate(ConfigureSubscriptions).LifeStyle.Singleton);

            container.Register(Component.For<INybusOptions>().ImplementedBy<NybusOptions>());

            container.Register(
                Component.For<IContainer>()
                    .ImplementedBy<WindsorBusContainer>()
                    .DependsOn(Dependency.OnValue<IKernel>(container.Kernel))
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<IBusEngine>()
                    .ImplementedBy<MassTransitBusEngine>()
                    .LifeStyle.Singleton);

            container.Register(
                Component.For<MassTransitConnectionDescriptor>()
                    .UsingFactoryMethod(() => MassTransitConnectionDescriptor.FromConfiguration("ServiceBus")));


            container.Register(Component.For<MassTransitOptions>().OnCreate(ConfigureMassTransitOptions));
        }

        private void ConfigureMassTransitOptions(MassTransitOptions options)
        {
            options.CommandQueueStrategy = new TemporaryQueueStrategy();

            options.EventQueueStrategy = new TemporaryQueueStrategy();
        }

        private void ConfigureSubscriptions(IBusBuilder builder)
        {
            builder.SubscribeToCommand<ReverseStringCommand>();
        }
    }
}