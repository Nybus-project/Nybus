using System.Text;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class RabbitMqConfigurationTests
    {
        [Test]
        public void JsonSerializer_is_default_serializer()
        {
            var sut = new RabbitMqConfiguration();

            Assert.That(sut.Serializer, Is.InstanceOf<JsonSerializer>());
        }

        [Test]
        public void UTF8_is_default_outbound_encoding()
        {
            var sut = new RabbitMqConfiguration();

            Assert.That(sut.OutboundEncoding, Is.EqualTo(Encoding.UTF8));
        }

        [Test]
        public void Default_event_queue_strategy_is_temporary()
        {
            var sut = new RabbitMqConfiguration();

            Assert.That(sut.EventQueueFactory, Is.InstanceOf<TemporaryQueueFactory>());
        }

        [Test]
        public void Default_command_queue_strategy_is_temporary()
        {
            var sut = new RabbitMqConfiguration();

            Assert.That(sut.EventQueueFactory, Is.InstanceOf<TemporaryQueueFactory>());
        }
    }
}