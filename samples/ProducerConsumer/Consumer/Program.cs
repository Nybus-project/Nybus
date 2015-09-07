using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Messages;
using Nybus;
using Nybus.Configuration;
using Nybus.Container;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = CreateContainer())
            {
                var bus = CreateBus(container, "rabbitmq://localhost/test-1/");

                var handle = bus.Start();

                Console.WriteLine("Press ENTER to stop the execution.");

                Console.ReadLine();

                Console.WriteLine("Stopping");

                Task.WaitAll(handle.Stop());

                Console.WriteLine("Bye");

            }
        }

        private static IBus CreateBus(IWindsorContainer container, string hostName)
        {
            Uri host = new Uri(hostName);

            var connectionDescriptor = new MassTransitBusConnectionDescriptor(host, "test-1", "test");
            var builder = new MassTransitBusBuilder(connectionDescriptor);

            IBus bus = builder.Build(c =>
            {
                c.SetContainer(container);
                c.SubscribeToCommand<ProduceItem>();
            });

            container.Register(Component.For<IBus>().Instance(bus));

            return bus;
        }


        static IWindsorContainer CreateContainer()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(new DefaultHandlerInstaller());

            return container;
        }

    }
}
