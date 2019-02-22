using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class RabbitMqConfiguratorTests
    {
        [Test, CustomAutoMoqData]
        public void RegisterQueueFactoryProvider_adds_provider_with_default_setup(RabbitMqConfigurator sut, TestNybusConfigurator configurator)
        {
            sut.RegisterQueueFactoryProvider<TestQueueFactoryProvider>();

            sut.Apply(configurator);

            var services = new ServiceCollection();

            configurator.ApplyServiceConfigurations(services);

            var serviceProvider = services.BuildServiceProvider();

            var provider = serviceProvider.GetService<IQueueFactoryProvider>();

            Assert.That(provider, Is.InstanceOf<TestQueueFactoryProvider>());
        }

        [Test, CustomAutoMoqData]
        public void RegisterQueueFactoryProvider_adds_provider_with_custom_setup(RabbitMqConfigurator sut, TestNybusConfigurator configurator, TestQueueFactoryProvider factoryProvider, Func<IServiceProvider, IQueueFactoryProvider> setup)
        {
            Mock.Get(setup).Setup(p => p(It.IsAny<IServiceProvider>())).Returns(factoryProvider);

            sut.RegisterQueueFactoryProvider<TestQueueFactoryProvider>(setup);

            sut.Apply(configurator);

            var services = new ServiceCollection();

            configurator.ApplyServiceConfigurations(services);

            var serviceProvider = services.BuildServiceProvider();

            var provider = serviceProvider.GetService<IQueueFactoryProvider>();

            Mock.Get(setup).Verify(s => s(It.IsAny<IServiceProvider>()), Times.Once);
            Assert.That(provider, Is.SameAs(factoryProvider));
        }

        [Test, CustomAutoMoqData]
        public void Configure_sets_action_to_be_used(RabbitMqConfigurator sut, TestNybusConfigurator configurator, IConfigurationFactory configurationFactory, RabbitMqOptions options, Action<IRabbitMqConfiguration> configurationSetup)
        {
            sut.Configure(configurationSetup);

            sut.Apply(configurator);

            var services = new ServiceCollection();
            services.AddSingleton(configurationFactory);
            services.AddSingleton(options);

            configurator.ApplyServiceConfigurations(services);

            var serviceProvider = services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IRabbitMqConfiguration>();

            Mock.Get(configurationSetup).Verify(p => p(configuration), Times.Once);
        }

        [Test, CustomAutoMoqData]
        public void Configure_usages_are_additive(RabbitMqConfigurator sut, TestNybusConfigurator configurator, IConfigurationFactory configurationFactory, RabbitMqOptions options, Action<IRabbitMqConfiguration>[] configurationSetupActions)
        {
            foreach (var configurationSetup in configurationSetupActions)
            {
                sut.Configure(configurationSetup);
            }

            sut.Apply(configurator);

            var services = new ServiceCollection();
            services.AddSingleton(configurationFactory);
            services.AddSingleton(options);

            configurator.ApplyServiceConfigurations(services);

            var serviceProvider = services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IRabbitMqConfiguration>();

            foreach (var configurationSetup in configurationSetupActions)
            {
                Mock.Get(configurationSetup).Verify(p => p(configuration), Times.Once);
            }
        }

        [Test, CustomAutoMoqData]
        public void Configure_does_not_accept_null_delegates(RabbitMqConfigurator sut, TestNybusConfigurator configurator, IConfigurationFactory configurationFactory, RabbitMqOptions options)
        {
            Assert.Throws<ArgumentNullException>(() => sut.Configure(null));
        }

        [Test, CustomAutoMoqData]
        public void UseConfiguration_binds_values_to_options(RabbitMqConfigurator sut, TestNybusConfigurator configurator, IConfigurationFactory configurationFactory, string nybusSectionName, string rabbitMqSectionName)
        {
            var values = new Dictionary<string, string>
            {
                [$"{nybusSectionName}:{rabbitMqSectionName}:OutboundEncoding"] = "utf-8"
            };

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(values);

            var settings = configurationBuilder.Build();

            configurator.UseConfiguration(settings, nybusSectionName);

            sut.UseConfiguration(rabbitMqSectionName);

            sut.Apply(configurator);
            
            var services = new ServiceCollection();
            services.AddSingleton(configurationFactory);

            configurator.ApplyServiceConfigurations(services);

            var serviceProvider = services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IRabbitMqConfiguration>();

            Assert.That(configuration.OutboundEncoding, Is.SameAs(Encoding.UTF8));
        }
    }

    public class TestQueueFactoryProvider : IQueueFactoryProvider
    {
        public string ProviderName { get; }

        public IQueueFactory CreateFactory(IConfigurationSection settings)
        {
            throw new NotImplementedException();
        }
    }
}
