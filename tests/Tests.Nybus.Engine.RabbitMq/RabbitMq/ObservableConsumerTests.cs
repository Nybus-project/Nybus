using System;
using System.Collections.Generic;
using System.Reactive;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Nybus.RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tests.RabbitMq
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

        [Test, CustomAutoMoqData]
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

        [Test, CustomAutoMoqData]
        public void Messages_are__not_lost_if_not_subscribed(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            
            var sut = CreateSystemUnderTest();

            sut.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            var messages = sut.DumpInList();

            Assert.That(messages, Has.Exactly(1).InstanceOf<BasicDeliverEventArgs>());
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

        [Test, CustomAutoMoqData]
        public void IsRunning_returns_true_if_consumer_registered(ObservableConsumer sut, string consumerTag)
        {
            sut.HandleBasicConsumeOk(consumerTag);

            Assert.That(sut.IsRunning, Is.True);
        }

        [Test, CustomAutoMoqData]
        public void HandleBasicConsumeOk_accepts_incoming_consumerTag(ObservableConsumer sut, string consumerTag)
        {
            sut.HandleBasicConsumeOk(consumerTag);
        }

        [Test, CustomAutoMoqData]
        public void HandleBasicCancel_accepts_departing_consumerTag(ObservableConsumer sut, string consumerTag)
        {
            sut.HandleBasicConsumeOk(consumerTag);

            sut.HandleBasicCancel(consumerTag);
        }

        [Test, CustomAutoMoqData]
        public void HandleBasicCancel_raise_ConsumerCancelled_event(ObservableConsumer sut, string consumerTag, EventHandler<ConsumerEventArgs> eventHandler)
        {
            sut.HandleBasicConsumeOk(consumerTag);

            sut.ConsumerCancelled += eventHandler;

            sut.HandleBasicCancel(consumerTag);

            sut.ConsumerCancelled -= eventHandler;

            Mock.Get(eventHandler).Verify(p => p(sut, It.Is<ConsumerEventArgs>(c => c.ConsumerTag == consumerTag)));
        }

        [Test, CustomAutoMoqData]
        public void HandleBasicCancelOk_accepts_departing_consumerTag(ObservableConsumer sut, string consumerTag)
        {
            sut.HandleBasicConsumeOk(consumerTag);

            sut.HandleBasicCancelOk(consumerTag);
        }

        [Test, CustomAutoMoqData]
        public void HandleBasicCancelOk_raise_ConsumerCancelled_event(ObservableConsumer sut, string consumerTag, EventHandler<ConsumerEventArgs> eventHandler)
        {
            sut.HandleBasicConsumeOk(consumerTag);

            sut.ConsumerCancelled += eventHandler;

            sut.HandleBasicCancelOk(consumerTag);

            sut.ConsumerCancelled -= eventHandler;

            Mock.Get(eventHandler).Verify(p => p(sut, It.Is<ConsumerEventArgs>(c => c.ConsumerTag == consumerTag)));
        }

        [Test, CustomAutoMoqData]
        public void Handle_([Frozen] IModel model, ObservableConsumer sut, ShutdownEventArgs shutdownEventArgs)
        {
            sut.HandleModelShutdown(model, shutdownEventArgs);
        }
    }
}