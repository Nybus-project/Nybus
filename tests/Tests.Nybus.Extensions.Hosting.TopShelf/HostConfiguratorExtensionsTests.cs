using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Topshelf.HostConfigurators;
using Topshelf.Runtime;
// ReSharper disable InvokeAsExtensionMethod

namespace Tests
{
    [TestFixture]
    public class HostConfiguratorExtensionsTests
    {
        [Test, AutoMoqData]
        public void UseStartup_sets_up_service_builder(HostConfigurator configurator)
        {
            HostConfiguratorExtensions.UseStartup<TestStartup>(configurator);

            Mock.Get(configurator).Verify(p => p.UseServiceBuilder(It.IsAny<ServiceBuilderFactory>()));
        }

        [Test, AutoMoqData]
        public void UseStartup_sets_up_service_start(HostConfigurator configurator)
        {
            HostConfiguratorExtensions.UseStartup<TestStartup>(configurator);
            
            Assert.Inconclusive("The test is not able to assert whether the desired action happened");
        }

        [Test, AutoMoqData]
        public void UseStartup_sets_up_service_stop(HostConfigurator configurator)
        {
            HostConfiguratorExtensions.UseStartup<TestStartup>(configurator);

            Assert.Inconclusive("The test is not able to assert whether the desired action happened");
        }

        [Test, AutoMoqData]
        public void BuildHost_uses_ConfigureAppConfiguration(Mock<Startup> mockStartup, HostSettings settings)
        {
            HostConfiguratorExtensions.BuildHost(mockStartup.Object, settings);

            mockStartup.Verify(p => p.ConfigureAppConfiguration(It.IsAny<IConfigurationBuilder>()));
        }

        [Test, AutoMoqData]
        public void BuildHost_uses_ConstructService(Mock<Startup> mockStartup, HostSettings settings)
        {
            HostConfiguratorExtensions.BuildHost(mockStartup.Object, settings);

            mockStartup.Verify(p => p.ConstructService(It.IsAny<StartupContext>(), It.IsAny<IServiceProvider>()));
        }

        [Test, AutoMoqData]
        public void BuildHost_uses_ConfigureServices(Mock<Startup> mockStartup, HostSettings settings)
        {
            HostConfiguratorExtensions.BuildHost(mockStartup.Object, settings);

            mockStartup.Verify(p => p.ConfigureServices(It.IsAny<StartupContext>(), It.IsAny<IServiceCollection>()));
        }

        [Test, AutoMoqData]
        public void BuildHost_uses_ConfigureLogging(Mock<Startup> mockStartup, HostSettings settings)
        {
            HostConfiguratorExtensions.BuildHost(mockStartup.Object, settings);

            mockStartup.Verify(p => p.ConfigureLogging(It.IsAny<StartupContext>(), It.IsAny<ILoggingBuilder>()));
        }
    }

    public class TestStartup : Startup
    {
        public override IBusHost ConstructService(StartupContext context, IServiceProvider serviceProvider)
        {
            return Mock.Of<IBusHost>();
        }
    }
}