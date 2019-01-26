using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class NybusConfiguratorTests
    {
        [Test, CustomAutoMoqData]
        public void AddServiceConfiguration_configures_given_service(NybusConfigurator sut, Type serviceType, IServiceCollection services)
        {
            sut.AddServiceConfiguration(svc => svc.AddSingleton(serviceType));

            sut.ConfigureServices(services);

            Mock.Get(services).Verify(p => p.Add(It.Is<ServiceDescriptor>(sd => sd.ServiceType == serviceType)));
        }

        [Test, CustomAutoMoqData]
        public void AddServiceConfiguration_invokes_configuration_delegate(NybusConfigurator sut, IServiceCollection services, Action<IServiceCollection> configurationDelegate)
        {
            sut.AddServiceConfiguration(configurationDelegate);

            sut.ConfigureServices(services);

            Mock.Get(configurationDelegate).Verify(p => p(services), Times.Once);
        }

        [Test, CustomAutoMoqData]
        public void AddServiceConfiguration_requires_non_null_configuration_delegate(NybusConfigurator sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.AddServiceConfiguration(null));
        }

        [Test, CustomAutoMoqData]
        public void AddSubscription_configures(NybusConfigurator sut, ISubscriptionBuilder subscriptionBuilder, Action<ISubscriptionBuilder> subscriptionDelegate)
        {
            sut.AddSubscription(subscriptionDelegate);

            sut.ConfigureBuilder(subscriptionBuilder);

            Mock.Get(subscriptionDelegate).Verify(p => p(subscriptionBuilder), Times.Once);
        }

        [Test, CustomAutoMoqData]
        public void AddSubscription_requires_non_null_subscription_delegate(NybusConfigurator sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.AddSubscription(null));
        }

        [Test, CustomAutoMoqData]
        public void UseConfiguration_uses_specified_section(NybusConfigurator sut, IConfigurationSection configuration, string sectionName)
        {
            sut.UseConfiguration(configuration, sectionName);

            Mock.Get(configuration).Verify(p => p.GetSection(sectionName), Times.Once);

            Assert.That(sut.Configuration, Is.SameAs(configuration.GetSection(sectionName)));
        }

        [Test, CustomAutoMoqData]
        public void UseConfiguration_requires_configuration(NybusConfigurator sut, string sectionName)
        {
            Assert.Throws<ArgumentNullException>(() => sut.UseConfiguration(null, sectionName));
        }

        [Test, CustomAutoMoqData]
        public void UseConfiguration_requires_sectionName(NybusConfigurator sut, IConfigurationSection configuration)
        {
            Assert.Throws<ArgumentNullException>(() => sut.UseConfiguration(configuration, null));
        }

        [Test, CustomAutoMoqData]
        public void UseConfiguration_uses_default_sectionName(NybusConfigurator sut, IConfigurationSection configuration)
        {
            sut.UseConfiguration(configuration);

            Mock.Get(configuration).Verify(p => p.GetSection("Nybus"), Times.Once);
        }

        [Test, CustomAutoMoqData]
        public void Configure_(NybusConfigurator sut, IServiceProvider serviceProvider, INybusConfiguration configuration, Action<INybusConfiguration> configurationDelegate)
        {
            sut.Configure(configurationDelegate);

            sut.CustomizeConfiguration(serviceProvider, configuration);

            Mock.Get(configurationDelegate).Verify(p => p(configuration));
        }
    }
}
