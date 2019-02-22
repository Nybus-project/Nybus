using System.Collections.Generic;
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
        public void CommandExchange_IsAutoDelete_is_correctly_bound([Values] bool value)
        {
            var settings = new Dictionary<string, string>
            {
                [$"{nameof(RabbitMqOptions.CommandExchange)}:{nameof(ExchangeOptions.IsAutoDelete)}"] = value.ToString()
            };

            var configuration = CreateConfiguration(settings);

            var sut = new RabbitMqOptions();

            configuration.Bind(sut);

            Assert.That(sut.CommandExchange.IsAutoDelete, Is.EqualTo(value));
        }

        [Test]
        public void CommandExchange_IsDurable_is_correctly_bound([Values] bool value)
        {
            var settings = new Dictionary<string, string>
            {
                [$"{nameof(RabbitMqOptions.CommandExchange)}:{nameof(ExchangeOptions.IsDurable)}"] = value.ToString()
            };

            var configuration = CreateConfiguration(settings);

            var sut = new RabbitMqOptions();

            configuration.Bind(sut);

            Assert.That(sut.CommandExchange.IsDurable, Is.EqualTo(value));
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
        public void EventExchange_IsAutoDelete_is_correctly_bound([Values] bool value)
        {
            var settings = new Dictionary<string, string>
            {
                [$"{nameof(RabbitMqOptions.EventExchange)}:{nameof(ExchangeOptions.IsAutoDelete)}"] = value.ToString()
            };

            var configuration = CreateConfiguration(settings);

            var sut = new RabbitMqOptions();

            configuration.Bind(sut);

            Assert.That(sut.EventExchange.IsAutoDelete, Is.EqualTo(value));
        }

        [Test]
        public void EventExchange_IsDurable_is_correctly_bound([Values] bool value)
        {
            var settings = new Dictionary<string, string>
            {
                [$"{nameof(RabbitMqOptions.EventExchange)}:{nameof(ExchangeOptions.IsDurable)}"] = value.ToString()
            };

            var configuration = CreateConfiguration(settings);

            var sut = new RabbitMqOptions();

            configuration.Bind(sut);

            Assert.That(sut.EventExchange.IsDurable, Is.EqualTo(value));
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
    }
}