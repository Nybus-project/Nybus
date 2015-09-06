using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using MassTransit.BusConfigurators;
using Messages;
using Nybus;
using Nybus.Configuration;
using Nybus.Container;

namespace Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var container = CreateContainer())
            {
                var bus = CreateBus(container, "rabbitmq://ec2-52-17-96-97.eu-west-1.compute.amazonaws.com/test-1/");

                var handle = bus.Start();

                for (int i = 0; i < 1000; i++)
                {
                    InvokeCommand(bus, i);
                }

                Task.WaitAll(handle.Stop());
            }
        }

        private static void InvokeCommand(IBus bus, int i)
        {
            var command = new ProduceItem
            {
                ItemId = Guid.NewGuid(),
                Quantity = i
            };

            bus.InvokeCommand(command);

            Console.WriteLine($"Invoked production of item with ID {command.ItemId}");
        }

        private static IBus CreateBus(IWindsorContainer container, string hostName)
        {
            Uri host = new Uri(hostName);

            var connectionDescriptor = new MassTransitBusConnectionDescriptor(host, "test-1", "test");
            var builder = new MassTransitBusBuilder(connectionDescriptor);

            IBus bus = builder.Build(c =>
            {
                c.SetContainer(container);
                c.SubscribeToEvent<ItemProduced>(async ctx => Console.WriteLine($"{ctx.Message.Quantity} of {ctx.Message.ItemId} produced."));
            });

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
