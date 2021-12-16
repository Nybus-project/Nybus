using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
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

        [Test, CustomAutoMoqData]
        public void Logger_is_required(IQueueFactoryProvider[] providers, IConnectionFactoryProviders connectionFactoryProviders)
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationFactory(providers, connectionFactoryProviders, null));
        }

        [Test, CustomAutoMoqData]
        public void QueryFactoryProviders_with_same_name_are_processed_correctly(IQueueFactoryProvider[] providers, IConnectionFactoryProviders connectionFactoryProviders, string providerName, ILogger<ConfigurationFactory> logger)
        {
            foreach (var provider in providers)
            {
                Mock.Get(provider).SetupGet(p => p.ProviderName).Returns(providerName);
            }

            var factory = new ConfigurationFactory(providers, connectionFactoryProviders, logger);
        }

        [Test, CustomAutoMoqData]
        public void Create_requires_valid_encoding(ConfigurationFactory sut, RabbitMqOptions options, string invalidEncoding)
        {
            options.OutboundEncoding = invalidEncoding;

            Assert.Throws<ConfigurationException>(() => sut.Create(options));
        }

        [Test, CustomAutoMoqData]
        public void Create_returns_utf8_if_no_encoding_is_specified(ConfigurationFactory sut, RabbitMqOptions options)
        {
            options.OutboundEncoding = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.OutboundEncoding, Is.SameAs(Encoding.UTF8));
        }

        [Test, CustomAutoMoqData]
        public void Create_returns_selected_encoding_if_valid(ConfigurationFactory sut, RabbitMqOptions options)
        {
            var configuration = sut.Create(options);

            Assert.That(configuration.OutboundEncoding.WebName, Is.EqualTo(options.OutboundEncoding));
        }

        [Test, CustomAutoMoqData]
        public void Create_uses_valid_commandQueue([Frozen] IEnumerable<IQueueFactoryProvider> providers, ConfigurationFactory sut, RabbitMqOptions options, IQueueFactory queueFactory)
        {
            Mock.Get(providers.First()).Setup(p => p.CreateFactory(It.IsAny<IConfigurationSection>())).Returns(queueFactory);
            Mock.Get(options.CommandQueue.GetSection("ProviderName")).SetupGet(p => p.Value).Returns(providers.First().ProviderName);

            options.ConnectionString = null;
            options.Connection = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.CommandQueueFactory, Is.SameAs(queueFactory));
        }

        [Test, CustomAutoMoqData]
        public void Create_uses_TemporaryQueueFactory_for_commands_if_provider_is_unknown(ConfigurationFactory sut, RabbitMqOptions options, string unknownProviderName)
        {
            Mock.Get(options.CommandQueue.GetSection("ProviderName")).SetupGet(p => p.Value).Returns(unknownProviderName);

            options.ConnectionString = null;
            options.Connection = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.CommandQueueFactory, Is.SameAs(TemporaryQueueFactory.Instance));
        }

        [Test, CustomAutoMoqData]
        public void Create_uses_valid_eventQueue([Frozen] IEnumerable<IQueueFactoryProvider> providers, ConfigurationFactory sut, RabbitMqOptions options, IQueueFactory queueFactory)
        {
            Mock.Get(providers.First()).Setup(p => p.CreateFactory(It.IsAny<IConfigurationSection>())).Returns(queueFactory);
            Mock.Get(options.EventQueue.GetSection("ProviderName")).SetupGet(p => p.Value).Returns(providers.First().ProviderName);

            options.ConnectionString = null;
            options.Connection = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.EventQueueFactory, Is.SameAs(queueFactory));
        }

        [Test, CustomAutoMoqData]
        public void Create_uses_TemporaryQueueFactory_for_events_if_provider_is_unknown(ConfigurationFactory sut, RabbitMqOptions options, string unknownProviderName)
        {
            Mock.Get(options.EventQueue.GetSection("ProviderName")).SetupGet(p => p.Value).Returns(unknownProviderName);

            options.ConnectionString = null;
            options.Connection = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.EventQueueFactory, Is.SameAs(TemporaryQueueFactory.Instance));
        }

        [Test, CustomAutoMoqData]
        public void Create_uses_valid_errorQueue([Frozen] IEnumerable<IQueueFactoryProvider> providers, ConfigurationFactory sut, RabbitMqOptions options, IQueueFactory queueFactory)
        {
            Mock.Get(providers.First()).Setup(p => p.CreateFactory(It.IsAny<IConfigurationSection>())).Returns(queueFactory);
            Mock.Get(options.ErrorQueue.GetSection("ProviderName")).SetupGet(p => p.Value).Returns(providers.First().ProviderName);

            options.ConnectionString = null;
            options.Connection = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.ErrorQueueFactory, Is.SameAs(queueFactory));
        }

        [Test, CustomAutoMoqData]
        public void Create_uses_TemporaryQueueFactory_for_errors_if_provider_is_unknown(ConfigurationFactory sut, RabbitMqOptions options, string unknownProviderName)
        {
            Mock.Get(options.ErrorQueue.GetSection("ProviderName")).SetupGet(p => p.Value).Returns(unknownProviderName);

            options.ConnectionString = null;
            options.Connection = null;

            var configuration = sut.Create(options);

            Assert.That(configuration.ErrorQueueFactory, Is.SameAs(TemporaryQueueFactory.Instance));
        }

        [Test, CustomAutoMoqData]
        public void Create_uses_ConnectionStringConnectionFactory_when_connectionString_is_provided([Frozen] IConnectionFactoryProviders providers, ConfigurationFactory sut, RabbitMqOptions options, string connectionString)
        {
            Mock.Get(options.ConnectionString).SetupGet(p => p.Value).Returns(connectionString);

            options.Connection = null;

            var configuration = sut.Create(options);

            Mock.Get(providers.ConnectionString).Verify(p => p.CreateFactory(options.ConnectionString), Times.Once);
        }

        [Test, CustomAutoMoqData]
        public void Create_uses_ConnectionNodeConnectionFactory_when_connection_settings_are_provided([Frozen] IConnectionFactoryProviders providers, ConfigurationFactory sut, RabbitMqOptions options, string connectionString)
        {
            options.ConnectionString = null;

            var configuration = sut.Create(options);

            Mock.Get(providers.ConnectionNode).Verify(p => p.CreateFactory(options.Connection), Times.Once);
        }
    }
}
