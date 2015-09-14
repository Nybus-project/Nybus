using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Nybus;
using Nybus.Configuration;
using Nybus.Container;
using Nybus.Logging;
using Nybus.MassTransit;

namespace NLogSampleApp
{
    public class AppInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<NLog.ILogger>().UsingFactoryMethod(() => NLog.LogManager.GetLogger(NLogLogger.LoggerName)));

            container.Register(Component.For<ILogger>().ImplementedBy<NLogLogger>());

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
                    .DependsOn(
                        Dependency.OnValue<Uri>(new Uri("loopback://localhost/test")),
                        Dependency.OnValue("username", string.Empty),
                        Dependency.OnValue("password", string.Empty)
                    ));

            container.Register(Component.For<MassTransitOptions>().OnCreate(ConfigureMassTransit));
        }

        private void ConfigureSubscriptions(IBusBuilder builder)
        {
            builder.SubscribeToEvent<MessageReceived>();

            builder.SubscribeToCommand<SendMessage>();
        }

        private void ConfigureMassTransit(MassTransitOptions options)
        {
            options.ServiceBusFactory = new LoopbackServiceBusFactory();
        }
    }
}