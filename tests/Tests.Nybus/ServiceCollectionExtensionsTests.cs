using System;
using Microsoft.Extensions.Configuration;
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
        [Test, CustomAutoMoqData]
        public void ServiceCollection_is_returned(IServiceCollection services, Action<INybusConfigurator> configuratorDelegate)
        {
            var result = ServiceCollectionExtensions.AddNybus(services, configuratorDelegate);

            Assert.That(result, Is.SameAs(services));
        }

        [Test, CustomAutoMoqData]
        public void AddNybus_invokes_configuratorDelegate(IServiceCollection services, Action<INybusConfigurator> configuratorDelegate)
        {
            ServiceCollectionExtensions.AddNybus(services, configuratorDelegate);

            Mock.Get(configuratorDelegate).Verify(p => p(It.IsAny<INybusConfigurator>()));
        }

        [Test]
        [InlineAutoMoqData(typeof(NybusHostBuilder))]
        [InlineAutoMoqData(typeof(NybusConfiguration))]
        [InlineAutoMoqData(typeof(NybusHost))]
        [InlineAutoMoqData(typeof(IBusHost))]
        [InlineAutoMoqData(typeof(IBus))]
        public void AddNybus_registers_services(Type serviceType, IServiceCollection services, Action<INybusConfigurator> configuratorDelegate)
        {
            ServiceCollectionExtensions.AddNybus(services, configuratorDelegate);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == serviceType && sd.ImplementationFactory != null)));
        }

        [Test, CustomAutoMoqData]
        public void AddNybus_registers_NybusHostOptions(IServiceCollection services, IConfigurationSection configuration, Action<INybusConfigurator> configuratorDelegate)
        {
            ServiceCollectionExtensions.AddNybus(services, configuratorDelegate);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(NybusHostOptions))));
        }
    }
}