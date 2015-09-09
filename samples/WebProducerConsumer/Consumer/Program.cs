using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Messages;
using Nybus;
using Nybus.Configuration;
using Nybus.Container;
using Topshelf;

namespace Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            string rabbitmqHost = ConfigurationManager.AppSettings["ServiceBusHost"] ?? "localhost";

            using (var container = CreateContainer(rabbitmqHost))
            {
                Console.WriteLine("Initializing the bus");

                var service = HostFactory.New(c =>
                {
                    c.Service<ServiceHost>(svc =>
                    {
                        svc.ConstructUsing(() => container.Resolve<ServiceHost>());
                        svc.WhenStarted(host => Task.WaitAll(host.Start()));
                        svc.WhenStopped(host => Task.WaitAll(host.Stop()));
                    });

                    c.RunAsLocalSystem();

                    c.SetDisplayName("Consumer test");

                    c.SetServiceName("Consumer-test");
                });

                service.Run();
                
                Console.WriteLine("Bye");

            }

        }

        static IWindsorContainer CreateContainer(string hostName)
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(new DefaultHandlerInstaller());
            container.Register(Component.For<ServiceHost>());
            container.Register(Component.For<IBus>().Instance(CreateBus(container, hostName)));

            return container;
        }

        static IBus CreateBus(IWindsorContainer container, string hostName)
        {
            Uri host = new Uri(hostName);

            var connectionDescriptor = new MassTransitBusConnectionDescriptor(host, "test-1", "test");
            var builder = new MassTransitBusBuilder(connectionDescriptor);

            IBus bus = builder.Build(c =>
            {
                c.SetContainer(container);
                c.SubscribeToCommand<ReverseStringCommand>();
            });

            return bus;
        }

    }

    public class ServiceHost
    {
        private readonly IBus _bus;

        private IHandle _handle;

        public ServiceHost(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            _bus = bus;
        }

        public Task Start()
        {
            _handle = _bus.Start();

            return Task.FromResult(0);
        }

        public async Task Stop()
        {
            await _handle.Stop();
        }
    }
}
