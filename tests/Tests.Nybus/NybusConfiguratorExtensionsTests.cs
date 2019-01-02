using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Nybus.Policies;
using NybusConfiguratorExtensions = Nybus.NybusConfiguratorExtensions;

// ReSharper disable InvokeAsExtensionMethod

namespace Tests
{
    [TestFixture]
    public class NybusConfiguratorExtensionsTests
    {
        [Test, AutoMoqData]
        public void UseInMemoryBusEngine_registers_InMemory_bus_engine(TestNybusConfigurator nybus, IServiceCollection services)
        {
            NybusConfiguratorExtensions.UseInMemoryBusEngine(nybus);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IBusEngine) && sd.ImplementationType == typeof(InMemoryBusEngine))));
        }

        [Test, AutoMoqData]
        public void SubscribeToCommand_registers_handler_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            NybusConfiguratorExtensions.SubscribeToCommand<FirstTestCommand, FirstTestCommandHandler>(nybus);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToCommand<FirstTestCommand>(typeof(FirstTestCommandHandler)));
        }

        [Test, AutoMoqData]
        public void SubscribeToCommand_registers_handler_type(TestNybusConfigurator nybus, IServiceCollection services)
        {
            NybusConfiguratorExtensions.SubscribeToCommand<FirstTestCommand, FirstTestCommandHandler>(nybus);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(FirstTestCommandHandler))));
        }

        [Test, AutoMoqData]
        public void SubscribeToCommand_registers_delegate_handler_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            var testHandler = Mock.Of<CommandReceived<FirstTestCommand>>();

            NybusConfiguratorExtensions.SubscribeToCommand(nybus, testHandler);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToCommand<FirstTestCommand>(typeof(DelegateWrapperCommandHandler<FirstTestCommand>)));
        }

        [Test, AutoMoqData]
        public void SubscribeToCommand_registers_delegate_handler_type(TestNybusConfigurator nybus, IServiceCollection services)
        {
            var testHandler = Mock.Of<CommandReceived<FirstTestCommand>>();

            NybusConfiguratorExtensions.SubscribeToCommand(nybus, testHandler);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(DelegateWrapperCommandHandler<FirstTestCommand>))));
        }

        [Test, AutoMoqData]
        public void SubscribeToCommand_subscribes_to_command_type(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            NybusConfiguratorExtensions.SubscribeToCommand<FirstTestCommand>(nybus);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToCommand<FirstTestCommand>(typeof(ICommandHandler<FirstTestCommand>)));
        }

        [Test, AutoMoqData]
        public void SubscribeToCommand_registers_handler_instance_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder, FirstTestCommandHandler handler)
        {
            NybusConfiguratorExtensions.SubscribeToCommand<FirstTestCommand, FirstTestCommandHandler>(nybus, handler);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToCommand<FirstTestCommand>(handler.GetType()));
        }

        [Test, AutoMoqData]
        public void SubscribeToCommand_registers_handler_instance(TestNybusConfigurator nybus, IServiceCollection services, FirstTestCommandHandler handler)
        {
            NybusConfiguratorExtensions.SubscribeToCommand<FirstTestCommand, FirstTestCommandHandler>(nybus, handler);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == handler.GetType())));
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_registers_handler_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            NybusConfiguratorExtensions.SubscribeToEvent<FirstTestEvent, FirstTestEventHandler>(nybus);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToEvent<FirstTestEvent>(typeof(FirstTestEventHandler)));
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_registers_handler_type(TestNybusConfigurator nybus, IServiceCollection services)
        {
            NybusConfiguratorExtensions.SubscribeToEvent<FirstTestEvent, FirstTestEventHandler>(nybus);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(FirstTestEventHandler))));
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_registers_delegate_handler_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            var testHandler = Mock.Of<EventReceived<FirstTestEvent>>();

            NybusConfiguratorExtensions.SubscribeToEvent(nybus, testHandler);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToEvent<FirstTestEvent>(typeof(DelegateWrapperEventHandler<FirstTestEvent>)));
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_registers_delegate_handler_type(TestNybusConfigurator nybus, IServiceCollection services)
        {
            var testHandler = Mock.Of<EventReceived<FirstTestEvent>>();

            NybusConfiguratorExtensions.SubscribeToEvent(nybus, testHandler);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(DelegateWrapperEventHandler<FirstTestEvent>))));
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_subscribes_to_command_type(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder)
        {
            NybusConfiguratorExtensions.SubscribeToEvent<FirstTestEvent>(nybus);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToEvent<FirstTestEvent>(typeof(IEventHandler<FirstTestEvent>)));
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_registers_handler_instance_for_command(TestNybusConfigurator nybus, ISubscriptionBuilder subscriptionBuilder, FirstTestEventHandler handler)
        {
            NybusConfiguratorExtensions.SubscribeToEvent<FirstTestEvent, FirstTestEventHandler>(nybus, handler);

            nybus.ApplySubscriptions(subscriptionBuilder);

            Mock.Get(subscriptionBuilder).Verify(p => p.SubscribeToEvent<FirstTestEvent>(handler.GetType()));
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_registers_handler_instance(TestNybusConfigurator nybus, IServiceCollection services, FirstTestEventHandler handler)
        {
            NybusConfiguratorExtensions.SubscribeToEvent<FirstTestEvent, FirstTestEventHandler>(nybus, handler);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == handler.GetType())));
        }

        [Test, AutoMoqData]
        public void RegisterErrorPolicyProvider_adds_provider_with_default_setup(TestNybusConfigurator nybus, IServiceCollection services)
        {
            NybusConfiguratorExtensions.RegisterErrorPolicyProvider<TestErrorPolicyProvider>(nybus);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IErrorPolicyProvider) && sd.ImplementationType == typeof(TestErrorPolicyProvider))));
        }

        [Test, AutoMoqData]
        public void RegisterErrorPolicyProvider_adds_provider_with_custom_setup(TestNybusConfigurator nybus, IServiceCollection services)
        {
            var providerFactory = Mock.Of<Func<IServiceProvider, IErrorPolicyProvider>>();

            NybusConfiguratorExtensions.RegisterErrorPolicyProvider<TestErrorPolicyProvider>(nybus, providerFactory);

            nybus.ApplyServiceConfigurations(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == typeof(IErrorPolicyProvider) && sd.ImplementationFactory == providerFactory)));
        }

    }

    public class TestErrorPolicyProvider : IErrorPolicyProvider
    {
        public string ProviderName { get; }
        public IErrorPolicy CreatePolicy(IConfigurationSection configuration)
        {
            throw new System.NotImplementedException();
        }
    }
}
