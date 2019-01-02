using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
// ReSharper disable InvokeAsExtensionMethod

namespace Tests
{
    [TestFixture]
    public class ServiceCollectionExtensionsTests
    {
        private Action<INybusConfigurator> configuratorDelegate;

        [SetUp]
        public void Initialize()
        {
            configuratorDelegate = Mock.Of<Action<INybusConfigurator>>();
        }

        [Test, AutoMoqData]
        public void ServiceCollection_is_returned(IServiceCollection services)
        {
            var result = ServiceCollectionExtensions.AddNybus(services, configuratorDelegate);

            Assert.That(result, Is.SameAs(services));
        }

        [Test, AutoMoqData]
        public void AddNybus_invokes_configuratorDelegate(IServiceCollection services)
        {
            ServiceCollectionExtensions.AddNybus(services, configuratorDelegate);

            Mock.Get(configuratorDelegate).Verify(p => p(It.IsAny<INybusConfigurator>()));
        }

        [Test]
        [InlineAutoMoqData(typeof(NybusHostBuilder))]
        [InlineAutoMoqData(typeof(NybusHostOptions))]
        [InlineAutoMoqData(typeof(NybusHost))]
        [InlineAutoMoqData(typeof(IBusHost))]
        [InlineAutoMoqData(typeof(IBus))]
        public void AddNybus_registers_services(Type serviceType, IServiceCollection services)
        {
            ServiceCollectionExtensions.AddNybus(services, configuratorDelegate);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == serviceType && sd.ImplementationFactory != null)));
        }
    }
}