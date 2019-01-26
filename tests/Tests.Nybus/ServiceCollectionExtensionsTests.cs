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

        [Test, AutoMoqData]
        public void AddCommandHandler_registers_handler_generic_syntax(IServiceCollection services)
        {
            ServiceCollectionExtensions.AddCommandHandler<FirstTestCommand, FirstTestCommandHandler>(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(ICommandHandler<FirstTestCommand>) && sd.ImplementationType == typeof(FirstTestCommandHandler))));
        }

        [Test, AutoMoqData]
        public void AddCommandHandler_registers_handler_by_its_type(IServiceCollection services)
        {
            ServiceCollectionExtensions.AddCommandHandler(services, typeof(FirstTestCommandHandler));

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(ICommandHandler<FirstTestCommand>) && sd.ImplementationType == typeof(FirstTestCommandHandler))));
        }

        [Test, AutoMoqData]
        public void AddCommandHandler_can_handle_type_handling_multiple_commands(IServiceCollection services)
        {
            ServiceCollectionExtensions.AddCommandHandler(services, typeof(MixedTestCommandHandler));

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(ICommandHandler<FirstTestCommand>) && sd.ImplementationType == typeof(MixedTestCommandHandler))));

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(ICommandHandler<SecondTestCommand>) && sd.ImplementationType == typeof(MixedTestCommandHandler))));
        }

        [Test, AutoMoqData]
        public void AddCommandHandler_registers_handler_by_its_type_generic_syntax(IServiceCollection services)
        {
            ServiceCollectionExtensions.AddCommandHandler<FirstTestCommandHandler>(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(ICommandHandler<FirstTestCommand>) && sd.ImplementationType == typeof(FirstTestCommandHandler))));
        }

        [Test, AutoMoqData]
        public void AddEventHandler_registers_handler_generic_syntax(IServiceCollection services)
        {
            ServiceCollectionExtensions.AddEventHandler<FirstTestEvent, FirstTestEventHandler>(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IEventHandler<FirstTestEvent>) && sd.ImplementationType == typeof(FirstTestEventHandler))));
        }

        [Test, AutoMoqData]
        public void AddEventHandler_registers_handler_by_its_type(IServiceCollection services)
        {
            ServiceCollectionExtensions.AddEventHandler(services, typeof(FirstTestEventHandler));

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IEventHandler<FirstTestEvent>) && sd.ImplementationType == typeof(FirstTestEventHandler))));
        }

        [Test, AutoMoqData]
        public void AddEventHandler_can_handle_type_handling_multiple_Events(IServiceCollection services)
        {
            ServiceCollectionExtensions.AddEventHandler(services, typeof(MixedTestEventHandler));

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IEventHandler<FirstTestEvent>) && sd.ImplementationType == typeof(MixedTestEventHandler))));

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IEventHandler<SecondTestEvent>) && sd.ImplementationType == typeof(MixedTestEventHandler))));
        }

        [Test, AutoMoqData]
        public void AddEventHandler_registers_handler_by_its_type_generic_syntax(IServiceCollection services)
        {
            ServiceCollectionExtensions.AddEventHandler<FirstTestEventHandler>(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IEventHandler<FirstTestEvent>) && sd.ImplementationType == typeof(FirstTestEventHandler))));
        }
    }
}