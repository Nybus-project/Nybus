using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using NUnit.Framework;
using Nybus;
using Nybus.Container;

namespace Tests.Container
{
    [TestFixture]
    public class DefaultHandlerInstallerTests
    {
        private DefaultHandlerInstaller CreateSystemUnderTest()
        {
            return new DefaultHandlerInstaller(Classes.FromAssemblyInThisApplication());
        }

        [Test]
        public void DefaultHandlerInstaller_uses_AppDomain_Current_BaseDirectory_as_default_filter()
        {
            new DefaultHandlerInstaller();
        }

        [Test]
        public void DefaultHandlerInstaller_can_accept_a_path()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            new DefaultHandlerInstaller(path);
        }

        [Test]
        public void DefaultHandlerInstaller_can_accept_a_descriptor()
        {
            var descriptor = Classes.FromThisAssembly();
            var installer = new DefaultHandlerInstaller(descriptor);

            Assert.That(installer.Descriptor, Is.SameAs(descriptor));
        }

        [Test][ExpectedException]
        public void Descriptor_is_required()
        {
            new DefaultHandlerInstaller((FromAssemblyDescriptor)null);
        }

        [Test]
        public void Event_handlers_are_automatically_registered()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(CreateSystemUnderTest());

            var handler = container.Kernel.GetHandler(typeof (IEventHandler<TestEvent>));

            Assert.That(handler.ComponentModel.ComponentName.Name, Is.StringEnding("(fallback)"));
            Assert.That(handler.ComponentModel.LifestyleType, Is.EqualTo(LifestyleType.Transient));
        }

        [Test]
        public void Event_handlers_automatic_registration_returns_an_instance()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(CreateSystemUnderTest());

            var component = container.Resolve<IEventHandler<TestEvent>>();

            Assert.That(component, Is.Not.Null);
        }


        [Test]
        public void Command_handlers_are_automatically_registered()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(CreateSystemUnderTest());

            var handler = container.Kernel.GetHandler(typeof(ICommandHandler<TestCommand>));

            Assert.That(handler.ComponentModel.ComponentName.Name, Is.StringEnding("(fallback)"));
            Assert.That(handler.ComponentModel.LifestyleType, Is.EqualTo(LifestyleType.Transient));
        }

        [Test]
        public void Command_handlers_automatic_registration_returns_an_instance()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Install(CreateSystemUnderTest());

            var component = container.Resolve<ICommandHandler<TestCommand>>();

            Assert.That(component, Is.Not.Null);
        }

    }
}
