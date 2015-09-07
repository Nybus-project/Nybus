using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Mvc;
using Messages;
using Nybus;
using Nybus.Configuration;
using Nybus.Container;

namespace WebProducer
{
    public class Global : System.Web.HttpApplication
    {
        private const string ContainerKey = "WindsorContainer";
        private const string ServiceBusHandleKey = "ServiceBus";

        protected void Application_Start()
        {
            ConfigureContainer();

            ConfigureControllerFactory(ControllerBuilder.Current);
            ConfigureRoutes(RouteTable.Routes);
            ConfigureFilters(GlobalFilters.Filters);
            //ConfigureBus();
        }

        private void ConfigureBus()
        {
            var container = (IWindsorContainer)Application[ContainerKey];

            var connectionDescriptor = CreateConnectionDescriptor();

            var builder = new MassTransitBusBuilder(connectionDescriptor);

            var bus = builder.Build(c =>
            {
                c.SetContainer(container);
                c.SubscribeToEvent<StringReversedEvent>();
            });

            container.Register(Component.For<IBus>().Instance(bus).LifestyleSingleton());

            var handle = bus.Start();

            Application[ServiceBusHandleKey] = handle;
        }

        private MassTransitBusConnectionDescriptor CreateConnectionDescriptor()
        {
            Uri host = new Uri(ConfigurationManager.AppSettings["ServiceBusHost"]);
            string userName = ConfigurationManager.AppSettings["ServiceBusUserName"];
            string password = ConfigurationManager.AppSettings["ServiceBusPassword"];

            var connectionDescriptor = new MassTransitBusConnectionDescriptor(host, userName, password);

            return connectionDescriptor;
        }

        private void ConfigureControllerFactory(ControllerBuilder current)
        {
            var container = (IWindsorContainer) Application[ContainerKey];
            current.SetControllerFactory(new WindsorControllerFactory(container.Kernel));
        }

        private void ConfigureContainer()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(new DefaultHandlerInstaller());

            Application[ContainerKey] = container;
        }

        private void ConfigureFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        private void ConfigureRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("HomePage", "", new {controller = "Home", action = "Index"});

            routes.MapMvcAttributeRoutes();
        }

        protected void Application_End()
        {
            //var handle = (IHandle) Application[ServiceBusHandleKey];
            //Task.WaitAll(handle.Stop());
            
            var container = (IWindsorContainer)Application[ContainerKey];
            Application[ContainerKey] = null;

            container.Dispose();
        }
    }

    public class StringReversedEventHandler : IEventHandler<StringReversedEvent>
    {
        public Task Handle(EventContext<StringReversedEvent> eventMessage)
        {
            return Task.FromResult(0);
        }
    }
}