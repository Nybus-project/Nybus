using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;
using RabbitMQ.Client;

namespace Tests.Configuration
{
    [TestFixture]
    public class ConnectionNodeConnectionFactoryProviderTests
    {
        [Test, CustomAutoMoqData]
        public void CreateFactory_returns_a_ConnectionFactory(ConnectionNodeConnectionFactoryProvider sut, IConfigurationSection configuration)
        {
            var factory = sut.CreateFactory(configuration);

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory, Is.InstanceOf<ConnectionFactory>());
        }

        [Test, CustomAutoMoqData]
        public void CreateFactory_adds_hostName(ConnectionNodeConnectionFactoryProvider sut, IConfigurationSection configuration, IConfigurationSection valueSection, string value)
        {
            Mock.Get(valueSection).SetupGet(p => p.Value).Returns(value);

            Mock.Get(configuration).Setup(p => p.GetSection("HostName")).Returns(valueSection);

            var factory = sut.CreateFactory(configuration) as ConnectionFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.HostName, Is.EqualTo(value));
        }

        [Test, CustomAutoMoqData]
        public void CreateFactory_adds_username(ConnectionNodeConnectionFactoryProvider sut, IConfigurationSection configuration, IConfigurationSection valueSection, string value)
        {
            Mock.Get(valueSection).SetupGet(p => p.Value).Returns(value);

            Mock.Get(configuration).Setup(p => p.GetSection("UserName")).Returns(valueSection);

            var factory = sut.CreateFactory(configuration) as ConnectionFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.UserName, Is.EqualTo(value));
        }

        [Test, CustomAutoMoqData]
        public void CreateFactory_adds_password(ConnectionNodeConnectionFactoryProvider sut, IConfigurationSection configuration, IConfigurationSection valueSection, string value)
        {
            Mock.Get(valueSection).SetupGet(p => p.Value).Returns(value);

            Mock.Get(configuration).Setup(p => p.GetSection("Password")).Returns(valueSection);

            var factory = sut.CreateFactory(configuration) as ConnectionFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.Password, Is.EqualTo(value));
        }

        [Test, CustomAutoMoqData]
        public void CreateFactory_adds_virtualHost(ConnectionNodeConnectionFactoryProvider sut, IConfigurationSection configuration, IConfigurationSection valueSection, string value)
        {
            Mock.Get(valueSection).SetupGet(p => p.Value).Returns(value);

            Mock.Get(configuration).Setup(p => p.GetSection("VirtualHost")).Returns(valueSection);

            var factory = sut.CreateFactory(configuration) as ConnectionFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.VirtualHost, Is.EqualTo(value));
        }
    }
}
