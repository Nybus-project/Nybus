using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class ConfigurationFactoryTests
    {
        [Test]
        public void QueryFactoryProviders_are_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationFactory(null, Mock.Of<IConnectionFactoryProviders>(), Mock.Of<ILogger<ConfigurationFactory>>()));
        }

        [Test, AutoMoqData]
        public void Logger_is_required(IQueueFactoryProvider[] providers, IConnectionFactoryProviders connectionFactoryProviders)
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationFactory(providers, connectionFactoryProviders, null));
        }

        [Test, AutoMoqData]
        public void QueryFactoryProviders_with_same_name_are_processed_correctly(IQueueFactoryProvider[] providers, IConnectionFactoryProviders connectionFactoryProviders, string providerName, ILogger<ConfigurationFactory> logger)
        {
            foreach (var provider in providers)
            {
                Mock.Get(provider).SetupGet(p => p.ProviderName).Returns(providerName);
            }

            var factory = new ConfigurationFactory(providers, connectionFactoryProviders, logger);

            Assert.That(factory.QueueFactoryProviders[providerName], Is.SameAs(providers[0]));
        }

        [Test, AutoMoqData]
        public void Create_requires_valid_encoding(ConfigurationFactory sut, RabbitMqOptions options, string invalidEncoding)
        {
            options.OutboundEncoding = invalidEncoding;

            Assert.Throws<ConfigurationException>(() => sut.Create(options));
        }

        [Test, AutoMoqData]
        public void Create_returns_utf8_if_no_encoding_is_specified(ConfigurationFactory sut, RabbitMqOptions options)
        {
            options.OutboundEncoding = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.OutboundEncoding, Is.SameAs(Encoding.UTF8));
        }

        [Test, AutoMoqData]
        public void Create_returns_selected_encoding_if_valid(ConfigurationFactory sut, RabbitMqOptions options)
        {
            var configuration = sut.Create(options);

            Assert.That(configuration.OutboundEncoding.WebName, Is.EqualTo(options.OutboundEncoding));
        }

        [Test, AutoMoqData]
        public void Create_uses_valid_commandQueue(ConfigurationFactory sut, RabbitMqOptions options, IQueueFactory queueFactory)
        {
            Mock.Get(sut.QueueFactoryProviders.First().Value).Setup(p => p.CreateFactory(It.IsAny<IConfigurationSection>())).Returns(queueFactory);
            Mock.Get(options.CommandQueue.GetSection("ProviderName")).SetupGet(p => p.Value).Returns(sut.QueueFactoryProviders.First().Value.ProviderName);

            options.ConnectionString = null;
            options.Connection = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.CommandQueueFactory, Is.SameAs(queueFactory));
        }

        [Test, AutoMoqData]
        public void Create_uses_TemporaryQueueFactory_for_commands_if_provider_is_unknown(ConfigurationFactory sut, RabbitMqOptions options, string unknownProviderName)
        {
            Mock.Get(options.CommandQueue.GetSection("ProviderName")).SetupGet(p => p.Value).Returns(unknownProviderName);

            options.ConnectionString = null;
            options.Connection = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.CommandQueueFactory, Is.SameAs(TemporaryQueueFactory.Instance));
        }

        [Test, AutoMoqData]
        public void Create_uses_valid_eventQueue(ConfigurationFactory sut, RabbitMqOptions options, IQueueFactory queueFactory)
        {
            Mock.Get(sut.QueueFactoryProviders.First().Value).Setup(p => p.CreateFactory(It.IsAny<IConfigurationSection>())).Returns(queueFactory);
            Mock.Get(options.EventQueue.GetSection("ProviderName")).SetupGet(p => p.Value).Returns(sut.QueueFactoryProviders.First().Value.ProviderName);

            options.ConnectionString = null;
            options.Connection = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.EventQueueFactory, Is.SameAs(queueFactory));
        }

        [Test, AutoMoqData]
        public void Create_uses_TemporaryQueueFactory_for_events_if_provider_is_unknown(ConfigurationFactory sut, RabbitMqOptions options, string unknownProviderName)
        {
            Mock.Get(options.EventQueue.GetSection("ProviderName")).SetupGet(p => p.Value).Returns(unknownProviderName);

            options.ConnectionString = null;
            options.Connection = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.EventQueueFactory, Is.SameAs(TemporaryQueueFactory.Instance));
        }

        [Test, AutoMoqData]
        public void Create_uses_ConnectionStringConnectionFactory_when_connectionString_is_provided(ConfigurationFactory sut, RabbitMqOptions options, string connectionString)
        {
            Mock.Get(options.ConnectionString).SetupGet(p => p.Value).Returns(connectionString);

            options.Connection = null;

            var configuration = sut.Create(options);

            Mock.Get(sut.ConnectionFactoryProviders.ConnectionString).Verify(p => p.CreateFactory(options.ConnectionString), Times.Once);
        }

        [Test, AutoMoqData]
        public void Create_uses_ConnectionNodeConnectionFactory_when_connection_settings_are_provided(ConfigurationFactory sut, RabbitMqOptions options, string connectionString)
        {
            options.ConnectionString = null;

            var configuration = sut.Create(options);

            Mock.Get(sut.ConnectionFactoryProviders.ConnectionNode).Verify(p => p.CreateFactory(options.Connection), Times.Once);
        }
    }
}
