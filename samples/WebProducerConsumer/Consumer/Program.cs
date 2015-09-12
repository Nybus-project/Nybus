using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Nybus.Container;
using Nybus.Utils;
using Topshelf;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = CreateContainer())
            {
                var service = HostFactory.New(c =>
                {
                    c.Service<ServiceHost>(svc =>
                    {
                        svc.ConstructUsing(() => container.Resolve<ServiceHost>());
                        svc.WhenStarted(host => host.Start().WaitAndUnwrapException());
                        svc.WhenStopped(host =>
                        {
                            host.Stop().WaitAndUnwrapException();
                            container.Release(host);
                        });
                    });

                    c.RunAsLocalSystem();

                    c.SetDisplayName("Consumer test");

                    c.SetServiceName("Consumer-test");
                });

                service.Run();

                Console.WriteLine("Bye");
            }
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
