using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.InMemory;

// ReSharper disable InvokeAsExtensionMethod

namespace Tests
{
    [TestFixture]
    public class InMemoryConfiguratorExtensionsTests
    {
        [Test, CustomAutoMoqData]
        public void UseInMemoryBusEngine_registers_InMemory_bus_engine(TestNybusConfigurator nybus, IServiceCollection services)
        {
            InMemoryConfiguratorExtensions.UseInMemoryBusEngine(nybus);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBusEngine) && sd.ImplementationType == typeof(InMemoryBusEngine))));
        }
    }
}
