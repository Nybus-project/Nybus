using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Windsor.Mvc;
using Messages;
using Nybus;
using Nybus.Configuration;
using Nybus.Container;
using Nybus.Utils;

namespace WebProducer
{
    public class Global : System.Web.HttpApplication
    {
        private const string ContainerKey = "WindsorContainer";
        private const string ServiceBusHandleKey = "ServiceBus";

        protected void Application_Start()
        {
            ConfigureContainer();

            ConfigureControllerFactory();
            ConfigureRoutes(RouteTable.Routes);
            ConfigureFilters(GlobalFilters.Filters);
            ConfigureBus();
        }

        private void ConfigureBus()
        {
            var container = (IWindsorContainer)Application[ContainerKey];

            var bus = container.Resolve<IBus>();

            bus.Start().WaitAndUnwrapException();

            Application[ServiceBusHandleKey] = bus;
        }

        private void ConfigureControllerFactory()
        {
            var container = (IWindsorContainer) Application[ContainerKey];
            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container.Kernel));
        }

        private void ConfigureContainer()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(FromAssembly.InThisApplication());

            Application[ContainerKey] = container;
        }

        private void ConfigureFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        private void ConfigureRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();
        }

        protected void Application_End()
        {
            var bus = (IBus)Application[ServiceBusHandleKey];
            bus.Stop().WaitAndUnwrapException();

            var container = (IWindsorContainer)Application[ContainerKey];
            Application[ContainerKey] = null;

            container.Release(bus);
            container.Dispose();
        }
    }
}