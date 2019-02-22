using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class RabbitMqOptionsBindingTests
    {
        private static IConfiguration CreateConfiguration(IDictionary<string, string> settings)
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(settings);

            return builder.Build();
        }

        [Test]
        public void CommandExchange_is_correctly_bound([Values] bool autoDelete, [Values] bool durable)
        {
            var settings = new Dictionary<string, string>
            {
                [$"{nameof(RabbitMqOptions.CommandExchange)}:{nameof(ExchangeOptions.IsAutoDelete)}"] = autoDelete.ToString(),
                [$"{nameof(RabbitMqOptions.CommandExchange)}:{nameof(ExchangeOptions.IsDurable)}"] = durable.ToString(),
            };

            var configuration = CreateConfiguration(settings);

            var sut = new RabbitMqOptions();

            configuration.Bind(sut);

            Assert.That(sut.CommandExchange.IsAutoDelete, Is.EqualTo(autoDelete));
            Assert.That(sut.CommandExchange.IsDurable, Is.EqualTo(durable));
        }

        [Test, AutoMoqData]
        public void CommandExchange_Properties_is_correctly_bound(string key, string value)
        {
            var settings = new Dictionary<string, string>
            {
                [$"{nameof(RabbitMqOptions.CommandExchange)}:{nameof(ExchangeOptions.Properties)}:{key}"] = value
            };

            var configuration = CreateConfiguration(settings);

            var sut = new RabbitMqOptions();

            configuration.Bind(sut);

            Assert.That(sut.CommandExchange.Properties[key], Is.EqualTo(value));
        }

        [Test]
        public void EventExchange_is_correctly_bound([Values] bool autoDelete, [Values] bool durable)
        {
            var settings = new Dictionary<string, string>
            {
                [$"{nameof(RabbitMqOptions.EventExchange)}:{nameof(ExchangeOptions.IsAutoDelete)}"] = autoDelete.ToString(),
                [$"{nameof(RabbitMqOptions.EventExchange)}:{nameof(ExchangeOptions.IsDurable)}"] = durable.ToString(),
            };

            var configuration = CreateConfiguration(settings);

            var sut = new RabbitMqOptions();

            configuration.Bind(sut);

            Assert.That(sut.EventExchange.IsAutoDelete, Is.EqualTo(autoDelete));
            Assert.That(sut.EventExchange.IsDurable, Is.EqualTo(durable));
        }

        [Test, AutoMoqData]
        public void EventExchange_Properties_is_correctly_bound(string key, string value)
        {
            var settings = new Dictionary<string, string>
            {
                [$"{nameof(RabbitMqOptions.EventExchange)}:{nameof(ExchangeOptions.Properties)}:{key}"] = value
            };

            var configuration = CreateConfiguration(settings);

            var sut = new RabbitMqOptions();

            configuration.Bind(sut);

            Assert.That(sut.EventExchange.Properties[key], Is.EqualTo(value));
        }

        [Test, CustomAutoMoqData]
        public void OutboundEncoding_is_correctly_bound(Encoding encoding)
        {
            var settings = new Dictionary<string, string>
            {
                [$"{nameof(RabbitMqOptions.OutboundEncoding)}"] = encoding.WebName
            };

            var configuration = CreateConfiguration(settings);

            var sut = new RabbitMqOptions();

            configuration.Bind(sut);

            Assert.That(sut.OutboundEncoding, Is.EqualTo(encoding.WebName));
        }

        [Test, AutoMoqData]
        public void ConnectionString_is_correctly_bound(string connectionString)
        {
            var settings = new Dictionary<string, string>
            {
                [$"{nameof(RabbitMqOptions.ConnectionString)}"] = connectionString
            };

            var configuration = CreateConfiguration(settings);

            var sut = new RabbitMqOptions();

            configuration.Bind(sut);

            Assert.That(sut.ConnectionString.Value, Is.EqualTo(connectionString));
        }

        [Test, AutoMoqData]
        public void ConnectionNode_is_correctly_bound(string userName, string password, string hostName, string vhost)
        {
            var settings = new Dictionary<string, string>
            {
                [$"{nameof(RabbitMqOptions.Connection)}:UserName"] = userName,
                [$"{nameof(RabbitMqOptions.Connection)}:Password"] = password,
                [$"{nameof(RabbitMqOptions.Connection)}:HostName"] = hostName,
                [$"{nameof(RabbitMqOptions.Connection)}:VirtualHost"] = vhost,
            };

            var configuration = CreateConfiguration(settings);

            var sut = new RabbitMqOptions();

            configuration.Bind(sut);

            Assert.That(sut.Connection["UserName"], Is.EqualTo(userName));
            Assert.That(sut.Connection["Password"], Is.EqualTo(password));
            Assert.That(sut.Connection["HostName"], Is.EqualTo(hostName));
            Assert.That(sut.Connection["VirtualHost"], Is.EqualTo(vhost));
        }
    }
}