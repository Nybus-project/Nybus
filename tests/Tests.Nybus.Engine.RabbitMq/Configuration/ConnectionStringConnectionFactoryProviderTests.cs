using System;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;
using RabbitMQ.Client;

namespace Tests.Configuration
{
    [TestFixture]
    public class ConnectionStringConnectionFactoryProviderTests
    {
        [Test, AutoMoqData]
        public void ConnectionString_value_is_required(ConnectionStringConnectionFactoryProvider sut, IConfigurationSection section)
        {
            Mock.Get(section).SetupGet(p => p.Value).Returns(null as string);

            Assert.Throws<ArgumentNullException>(() => sut.CreateFactory(section));
        }

        [Test, AutoMoqData]
        public void CreateFactory_creates_a_ConnectionFactory_from_connectionString(ConnectionStringConnectionFactoryProvider sut, IConfigurationSection section, string hostname, string username, string password, string virtualHost)
        {
            var connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder.Add("HostName", hostname);
            connectionStringBuilder.Add("Username", username);
            connectionStringBuilder.Add("Password", password);
            connectionStringBuilder.Add("VirtualHost", virtualHost);

            Mock.Get(section).SetupGet(p => p.Value).Returns(connectionStringBuilder.ConnectionString);

            var factory = sut.CreateFactory(section) as ConnectionFactory;

            Assert.That(factory, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public void CreateFactory_uses_given_hostName(ConnectionStringConnectionFactoryProvider sut, IConfigurationSection section, string value)
        {
            var connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder.Add("HostName", value);

            Mock.Get(section).SetupGet(p => p.Value).Returns(connectionStringBuilder.ConnectionString);

            var factory = sut.CreateFactory(section) as ConnectionFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.HostName, Is.EqualTo(value));
        }

        [Test, AutoMoqData]
        public void CreateFactory_uses_given_username(ConnectionStringConnectionFactoryProvider sut, IConfigurationSection section, string value)
        {
            var connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder.Add("Username", value);

            Mock.Get(section).SetupGet(p => p.Value).Returns(connectionStringBuilder.ConnectionString);

            var factory = sut.CreateFactory(section) as ConnectionFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.UserName, Is.EqualTo(value));
        }

        [Test, AutoMoqData]
        public void CreateFactory_uses_given_password(ConnectionStringConnectionFactoryProvider sut, IConfigurationSection section, string value)
        {
            var connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder.Add("Password", value);

            Mock.Get(section).SetupGet(p => p.Value).Returns(connectionStringBuilder.ConnectionString);

            var factory = sut.CreateFactory(section) as ConnectionFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.Password, Is.EqualTo(value));
        }

        [Test, AutoMoqData]
        public void CreateFactory_uses_given_virtualHost(ConnectionStringConnectionFactoryProvider sut, IConfigurationSection section, string value)
        {
            var connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder.Add("VirtualHost", value);

            Mock.Get(section).SetupGet(p => p.Value).Returns(connectionStringBuilder.ConnectionString);

            var factory = sut.CreateFactory(section) as ConnectionFactory;

            Assert.That(factory, Is.Not.Null);
            Assert.That(factory.VirtualHost, Is.EqualTo(value));
        }
    }
}