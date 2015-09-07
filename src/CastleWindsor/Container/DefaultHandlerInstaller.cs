using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Nybus.Container
{
    public class DefaultHandlerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var filter = new AssemblyFilter(currentDirectory);

            container.Register(Classes.FromAssemblyInDirectory(filter)
                .BasedOn(typeof(ICommandHandler<>))
                .WithServiceAllInterfaces()
                .WithServiceSelf()
                .Configure(c =>
                {
                    c.Named($"{c.Implementation.FullName} (fallback)");
                    c.IsFallback();
                }).LifestyleTransient());

            container.Register(Classes.FromAssemblyInDirectory(filter)
                .BasedOn(typeof(IEventHandler<>))
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