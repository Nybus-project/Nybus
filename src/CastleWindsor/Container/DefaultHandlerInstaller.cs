using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Installer;

namespace Nybus.Container
{
    public class DefaultHandlerInstaller : IWindsorInstaller
    {
        private readonly FromAssemblyDescriptor _descriptor;

        public DefaultHandlerInstaller() : this(AppDomain.CurrentDomain.BaseDirectory)
        {
        }

        public DefaultHandlerInstaller(string directoryToSearch) : this(Classes.FromAssemblyInDirectory(new AssemblyFilter(directoryToSearch)))
        {
            
        }

        public DefaultHandlerInstaller(FromAssemblyDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            _descriptor = descriptor;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            RegisterCommandHandlers(container, _descriptor);

            RegisterEventHandlers(container, _descriptor);

        }

        private static void RegisterEventHandlers(IWindsorContainer container, FromAssemblyDescriptor descriptor)
        {
            container.Register(descriptor
                .BasedOn(typeof (IEventHandler<>))
                .WithServiceAllInterfaces()
                .WithServiceSelf()
                .Configure(c =>
                {
                    c.Named($"{c.Implementation.FullName} (fallback)");
                    c.IsFallback();
                }).LifestyleTransient());
        }

        private void RegisterCommandHandlers(IWindsorContainer container, FromAssemblyDescriptor descriptor)
        {
            container.Register(descriptor
                .BasedOn(typeof (ICommandHandler<>))
                .WithServiceAllInterfaces()
                .WithServiceSelf()
                .Configure(c =>
                {
                    c.Named($"{c.Implementation.FullName} (fallback)");
                    c.IsFallback();
                }).LifestyleTransient());
        }
    }
}