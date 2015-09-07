using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Messages;
using Nybus;
using WebProducer.Handlers;

namespace WebProducer.Installers
{
    public class ControllerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Classes.FromAssemblyInThisApplication()
                                        .BasedOn<Controller>()
                                        .WithServiceSelf()
                                        .Configure(c => c.IsFallback())
                                        .LifestyleTransient());
        }
    }

    public class BusHandlerInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IEventHandler<StringReversedEvent>>()
                                        .ImplementedBy<StringReversedEventHandler>()
                                        .LifestyleTransient());
        }
    }
}