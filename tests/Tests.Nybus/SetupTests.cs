using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;

namespace Tests
{
    [TestFixture]
    public class SetupTests
    {
        [Test, AutoMoqData]
        public void UseBusEngine_is_required()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            services.AddNybus(nybus =>
            {

            });

            var serviceProvider = services.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IBusHost>());
        }

        [Test, AutoMoqData]
        public void Logging_is_required()
        {
            var services = new ServiceCollection();

            services.AddNybus(nybus =>
            {
                nybus.UseBusEngine<TestBusEngine>();
            });

            var serviceProvider = services.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IBusHost>());
        }

        [Test, AutoMoqData]
        public void Configuration_delegate_is_invoked_when_assembling_the_host(Action<INybusConfiguration> configurationDelegate)
        {
            var services = new ServiceCollection();
            services.AddLogging();

            services.AddNybus(nybus =>
            {
                nybus.UseBusEngine<TestBusEngine>();

                nybus.Configure(configurationDelegate);
            });

            var serviceProvider = services.BuildServiceProvider();

            var host = serviceProvider.GetRequiredService<IBusHost>();

            Mock.Get(configurationDelegate).Verify(p => p(It.IsAny<INybusConfiguration>()));
        }
    }
}
