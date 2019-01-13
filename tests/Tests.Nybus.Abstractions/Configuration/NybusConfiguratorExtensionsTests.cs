using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
// ReSharper disable InvokeAsExtensionMethod

namespace Tests.Configuration
{
    [TestFixture]
    public class NybusConfiguratorExtensionsTests
    {
        [Test, CustomAutoMoqData]
        public void UseBusEngine_registers_BusEngine(INybusConfigurator configurator, TestBusEngine engine)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            Mock.Get(configurator).Setup(p => p.AddServiceConfiguration(It.IsAny<Action<IServiceCollection>>())).Callback((Action<IServiceCollection> dlg) => dlg(serviceCollection));

            NybusConfiguratorExtensions.UseBusEngine<TestBusEngine>(configurator);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            Assert.That(serviceProvider.GetRequiredService<IBusEngine>(), Is.InstanceOf<TestBusEngine>());
        }

        [Test, CustomAutoMoqData]
        public void ServiceConfigurator_delegate_is_registered(INybusConfigurator configurator, TestBusEngine engine)
        {
            var serviceConfigurator = Mock.Of<Action<IServiceCollection>>();

            NybusConfiguratorExtensions.UseBusEngine<TestBusEngine>(configurator, serviceConfigurator);

            Mock.Get(configurator).Verify(p => p.AddServiceConfiguration(serviceConfigurator));
        }
    }

}
