using System;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class StaticQueueFactoryProviderTests
    {
        [Test, AutoMoqData]
        public void ProviderName_returns_static(StaticQueueFactoryProvider sut)
        {
            Assert.That(sut.ProviderName, Is.EqualTo("static"));
        }

        [Test, AutoMoqData]
        public void CreateFactory_returns_StaticQueueFactory(StaticQueueFactoryProvider sut, IConfigurationSection configuration, string queueName)
        {
            Mock.Get(configuration.GetSection("QueueName")).SetupGet(p => p.Value).Returns(queueName);

            var factory = sut.CreateFactory(configuration);

            Assert.That(factory, Is.InstanceOf<StaticQueueFactory>());
        }

        [Test, AutoMoqData]
        public void CreateFactory_returns_StaticQueueFactory_with_given_queueName(StaticQueueFactoryProvider sut, IConfigurationSection configuration, string queueName)
        {
            Mock.Get(configuration.GetSection("QueueName")).SetupGet(p => p.Value).Returns(queueName);

            var factory = sut.CreateFactory(configuration) as StaticQueueFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.QueueName, Is.EqualTo(queueName));
        }

        [Test, AutoMoqData]
        public void CreateFactory_throws_if_no_queueName_is_specified(StaticQueueFactoryProvider sut, IConfigurationSection configuration)
        {
            Mock.Get(configuration.GetSection("QueueName")).SetupGet(p => p.Value).Returns(null as string);

            Assert.Throws<ArgumentNullException>(() => sut.CreateFactory(configuration));
        }
    }
}