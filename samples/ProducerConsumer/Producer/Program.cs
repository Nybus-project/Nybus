using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Castle.Windsor.Installer;
using MassTransit.BusConfigurators;
using Nybus;
using Nybus.Container;
using Nybus.Utils;

namespace Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = CreateContainer())
            {
                var bus = container.Resolve<IBus>();
                var app = container.Resolve<ProductionManager>();

                Run(bus, app).WaitAndUnwrapException();
            }

            Console.ReadLine();
        }

        static async Task Run(IBus bus, ProductionManager app)
        {
            await bus.Start();
            await app.Execute(100);
            await bus.Stop();
        }

        static IWindsorContainer CreateContainer()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(FromAssembly.InThisApplication());
            container.Install(new DefaultHandlerInstaller());

            return container;
        }
    }
}
