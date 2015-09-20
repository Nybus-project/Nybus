using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Nybus;
using Nybus.Container;
using Nybus.Utils;

namespace NLogSampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = CreateContainer())
            {
                var bus = container.Resolve<IBus>();
                var app = container.Resolve<App>();

                Run(bus, app).WaitAndUnwrapException();
            }

            Console.WriteLine("Completed. Press ENTER to exit");
            Console.ReadLine();
        }

        static async Task Run(IBus bus, App app)
        {
            await bus.Start();
            await app.Execute();

            Console.WriteLine("Completed. Press ENTER to exit");
            Console.ReadLine();

            await bus.Stop();
        }

        static IWindsorContainer CreateContainer()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(FromAssembly.InThisApplication());
            container.Install(new DefaultHandlerInstaller());
            container.Register(Component.For<App>());

            return container;
        }
    }
}
