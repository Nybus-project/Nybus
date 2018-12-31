using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using RabbitMQ.Client;
using Tests.Configuration;
// ReSharper disable InvokeAsExtensionMethod

namespace Tests
{
    [TestFixture]
    public class RabbitMqConfiguratorExtensionsTests
    {
        [Test, AutoMoqData]
        public void UseRabbitMqBusEngine_registers_RabbitMqBusEngine(TestNybusConfigurator configurator, IServiceCollection services)
        {
            RabbitMqConfiguratorExtensions.UseRabbitMqBusEngine(configurator);

            configurator.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBusEngine) && sd.ImplementationType == typeof(RabbitMqBusEngine))), Times.Once);
        }

        [Test, AutoMoqData]
        public void UseRabbitMqBusEngine_registers_ConfigurationFactory(TestNybusConfigurator configurator, IServiceCollection services)
        {
            RabbitMqConfiguratorExtensions.UseRabbitMqBusEngine(configurator);

            configurator.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IConfigurationFactory) && sd.ImplementationType == typeof(ConfigurationFactory))), Times.Once);

        }

        [Test, AutoMoqData]
        public void UseRabbitMqBusEngine_registers_ConnectionFactoryProviders(TestNybusConfigurator configurator, IServiceCollection services)
        {
            RabbitMqConfiguratorExtensions.UseRabbitMqBusEngine(configurator);

            configurator.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IConnectionFactoryProviders) && sd.ImplementationType == typeof(ConnectionFactoryProviders))), Times.Once);
        }

        [Test, AutoMoqData]
        public void UseRabbitMqBusEngine_registers_TemporaryQueueFactoryProvider(TestNybusConfigurator configurator, IServiceCollection services)
        {
            RabbitMqConfiguratorExtensions.UseRabbitMqBusEngine(configurator);

            configurator.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IQueueFactoryProvider) && sd.ImplementationType == typeof(TemporaryQueueFactoryProvider))), Times.Once);
        }

        [Test, AutoMoqData]
        public void UseRabbitMqBusEngine_registers_StaticQueueFactoryProvider(TestNybusConfigurator configurator, IServiceCollection services)
        {
            RabbitMqConfiguratorExtensions.UseRabbitMqBusEngine(configurator);

            configurator.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IQueueFactoryProvider) && sd.ImplementationType == typeof(StaticQueueFactoryProvider))), Times.Once);
        }

        [Test, AutoMoqData]
        public void UseRabbitMqBusEngine_invokes_configuration_delegate(TestNybusConfigurator configurator, IServiceCollection services)
        {
            Mock<Action<IRabbitMqConfigurator>> configurationDelegate = new Mock<Action<IRabbitMqConfigurator>>();

            RabbitMqConfiguratorExtensions.UseRabbitMqBusEngine(configurator, configurationDelegate.Object);

            configurationDelegate.Verify(p => p(It.IsAny<IRabbitMqConfigurator>()), Times.Once);
        }
    }
}
