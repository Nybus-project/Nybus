using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Nybus.Container
{
    public class DefaultHandlerInstaller : IWindsorInstaller
    {
        private readonly string _directory;

        /// <summary>
        /// A Windsor Installer that will automatically register all the handlers in the AppDomain's base directory
        /// </summary>
        public DefaultHandlerInstaller() : this(AppDomain.CurrentDomain.BaseDirectory) { }

        /// <summary>
        /// A Windsor Installer that will automatically register all the handlers in the <param name="directory"></param> directory
        /// </summary>
        /// <param name="directory">The directory to scan for handlers to be registered</param>
        public DefaultHandlerInstaller(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            _directory = directory;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var filter = new AssemblyFilter(_directory);

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