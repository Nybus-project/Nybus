using System;
using AutoFixture.NUnit3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Nybus;
// ReSharper disable InvokeAsExtensionMethod

namespace Tests
{
    [TestFixture]
    public class HostBuilderExtensionsTests
    {
        [Test]
        public void UseHostedService_requires_hostBuilder()
        {
            Assert.Throws<ArgumentNullException>(() => HostBuilderExtensions.UseHostedService<NybusHostedService>(null));
        }

        [Test, AutoMoqData]
        public void UseHostedService_configure_services([Frozen] IServiceCollection services, IHostBuilder builder)
        {
            HostBuilderExtensions.UseHostedService<NybusHostedService>(builder);

            Mock.Get(builder).Verify(p => p.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()));
        }
    }
}