using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Nybus.Policies;

namespace Tests.Configuration
{
    [TestFixture]
    public class RabbitMqConfiguratorTests
    {
        [Test, AutoMoqData]
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

        [Test, AutoMoqData]
        public void RegisterQueueFactoryProvider_adds_provider_with_custom_setup(RabbitMqConfigurator sut, TestNybusConfigurator configurator, TestQueueFactoryProvider factoryProvider)
        {
            var setup = new Mock<Func<IServiceProvider, IQueueFactoryProvider>>();
            setup.Setup(p => p(It.IsAny<IServiceProvider>())).Returns(factoryProvider);

            sut.RegisterQueueFactoryProvider<TestQueueFactoryProvider>(setup.Object);

            sut.Apply(configurator);

            var services = new ServiceCollection();

            configurator.ApplyServiceConfigurations(services);

            var serviceProvider = services.BuildServiceProvider();

            var provider = serviceProvider.GetService<IQueueFactoryProvider>();

            setup.Verify(s => s(It.IsAny<IServiceProvider>()), Times.Once);
            Assert.That(provider, Is.SameAs(factoryProvider));
        }

        [Test, AutoMoqData]
        public void Configure_sets_action_to_be_used(RabbitMqConfigurator sut, TestNybusConfigurator configurator, IConfigurationFactory configurationFactory, RabbitMqOptions options)
        {
            var configurationSetup = new Mock<Action<IRabbitMqConfiguration>>();
            configurationSetup.Setup(p => p(It.IsAny<IRabbitMqConfiguration>()));

            sut.Configure(configurationSetup.Object);

            sut.Apply(configurator);

            var services = new ServiceCollection();
            services.AddSingleton(configurationFactory);
            services.AddSingleton(options);

            configurator.ApplyServiceConfigurations(services);

            var serviceProvider = services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IRabbitMqConfiguration>();

            configurationSetup.Verify(p => p(configuration), Times.Once);
        }

        [Test, AutoMoqData]
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

    public class TestNybusConfigurator : INybusConfigurator
    {
        public void UseConfiguration(Microsoft.Extensions.Configuration.IConfiguration configuration, string sectionName = "Nybus")
        {
            Configuration = configuration.GetSection(sectionName);
        }

        private readonly List<Action<IServiceCollection>> _serviceConfigurations = new List<Action<IServiceCollection>>();

        public void AddServiceConfiguration(Action<IServiceCollection> configurator)
        {
            _serviceConfigurations.Add(configurator);
        }

        public void ApplyServiceConfigurations(IServiceCollection services)
        {
            foreach (var sc in _serviceConfigurations)
                sc(services);
        }

        private readonly List<Action<ISubscriptionBuilder>> _subscriptionBuilders = new List<Action<ISubscriptionBuilder>>();

        public void AddSubscription(Action<ISubscriptionBuilder> configurator)
        {
            _subscriptionBuilders.Add(configurator);
        }

        public void ApplySubscriptions(ISubscriptionBuilder builder)
        {
            foreach (var sb in _subscriptionBuilders)
                sb(builder);
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; private set; }

        public void Configure(Action<INybusConfiguration> configuration)
        {
            throw new NotImplementedException();
        }
    }
}
