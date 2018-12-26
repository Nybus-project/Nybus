using System;
using System.Collections.Generic;
using System.Reactive;
using NUnit.Framework;
using AutoFixture.NUnit3;
using Moq;
using Nybus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tests
{
    [TestFixture]
    public class ObservableConsumerTests
    {
        private Mock<IModel> _mockModel;

        [SetUp]
        public void Initialize()
        {
            _mockModel = new Mock<IModel>();
        }

        private ObservableConsumer CreateSystemUnderTest()
        {
            return new ObservableConsumer(_mockModel.Object);
        }

        [Test]
        public void Model_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservableConsumer(null));
        }

        [Test, AutoMoqData]
        public void Can_subscribe_to_incoming_messages(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            var messages = new List<BasicDeliverEventArgs>();

            var sut = CreateSystemUnderTest();
            var subscription = sut.Subscribe(Observer.Create<BasicDeliverEventArgs>(item => messages.Add(item)));

            sut.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            Assert.That(messages, Has.Exactly(1).InstanceOf<BasicDeliverEventArgs>());
            Assert.That(messages[0].ConsumerTag, Is.EqualTo(consumerTag));
            Assert.That(messages[0].DeliveryTag, Is.EqualTo(deliveryTag));
            Assert.That(messages[0].Redelivered, Is.EqualTo(redelivered));
            Assert.That(messages[0].Exchange, Is.EqualTo(exchange));
            Assert.That(messages[0].RoutingKey, Is.EqualTo(routingKey));
            Assert.That(messages[0].BasicProperties, Is.SameAs(properties));
            CollectionAssert.AreEqual(messages[0].Body, body);

            subscription.Dispose();
        }

        [Test, AutoMoqData]
        public void Messages_are_lost_if_not_subscribed(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            var messages = new List<BasicDeliverEventArgs>();

            var sut = CreateSystemUnderTest();

            sut.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            var subscription = sut.Subscribe(Observer.Create<BasicDeliverEventArgs>(item => messages.Add(item)));

            Assert.That(messages, Is.Empty);

            subscription.Dispose();
        }

        [Test, AutoData]
        public void ConsumeFrom_registers_a_queue_with_no_autoAck(string queueName)
        {
            var sut = CreateSystemUnderTest();

            sut.ConsumeFrom(queueName);

            const bool autoAck = false;

            _mockModel.Verify(p => p.BasicConsume(queueName, autoAck, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(),  It.IsAny<IBasicConsumer>()));
        }

        [Test]
        public void ConsumeFrom_requires_a_valid_queueName()
        {
            var sut = CreateSystemUnderTest();

            Assert.Throws<ArgumentNullException>(() => sut.ConsumeFrom(null));
        }
    }
}