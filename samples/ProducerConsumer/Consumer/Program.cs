using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Nybus;
using Nybus.Container;
using Nybus.Utils;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = CreateContainer())
            {
                var bus = container.Resolve<IBus>();

                bus.Start().WaitAndUnwrapException();

                Console.WriteLine("Press ENTER to stop the execution.");

                Console.ReadLine();

                bus.Stop().WaitAndUnwrapException();

                Console.WriteLine("Bye");
            }
        }

        static IWindsorContainer CreateContainer()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(new DefaultHandlerInstaller());
            container.Install(FromAssembly.InThisApplication());

            return container;
        }
    }
}
