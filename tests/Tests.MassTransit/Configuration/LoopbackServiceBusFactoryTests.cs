using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.SubscriptionConfigurators;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;
using Nybus.MassTransit;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class LoopbackServiceBusFactoryTests
    {
        private IFixture fixture;

        private Mock<IQueueStrategy> mockQueueStrategy;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            mockQueueStrategy = new Mock<IQueueStrategy>();
        }

        private LoopbackServiceBusFactory CreateSystemUnderTest()
        {
            return new LoopbackServiceBusFactory();
        }

        private MassTransitConnectionDescriptor CreateConnectionDescriptor()
        {
            Uri loopbackHost= new Uri("loopback://localhost/test");
            return new MassTransitConnectionDescriptor(loopbackHost, string.Empty, string.Empty);
        }

        [Test]
        public void A_message_can_be_sent_through_the_loopback_bus()
        {
            var are = new AutoResetEvent(false);

            var sut = CreateSystemUnderTest();

            var testMessage = fixture.Create<TestMessage>();

            TestMessage receivedMessage = null;

            var connectionDescriptor = CreateConnectionDescriptor();
            IReadOnlyList<Action<SubscriptionBusServiceConfigurator>> subscriptions = new List<Action<SubscriptionBusServiceConfigurator>>
            {
                c => c.Handler<TestMessage>((ctx, message) =>
                {
                    receivedMessage = message;
                    are.Set();
                })
            };

            using (var bus = sut.CreateServiceBus(connectionDescriptor, mockQueueStrategy.Object, subscriptions))
            {
                bus.Publish(testMessage);

                are.WaitOne(TimeSpan.FromSeconds(2));

                Assert.That(receivedMessage, Is.Not.Null);
                Assert.That(receivedMessage.Flag, Is.EqualTo(testMessage.Flag));
                Assert.That(receivedMessage.Value, Is.EqualTo(testMessage.Value));
                Assert.That(receivedMessage.Text, Is.EqualTo(testMessage.Text));
                Assert.That(receivedMessage.Time, Is.EqualTo(testMessage.Time));
            }
        }
    }
}