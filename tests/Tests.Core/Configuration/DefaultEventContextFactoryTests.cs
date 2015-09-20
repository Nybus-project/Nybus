using System;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Nybus.Utils;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class DefaultEventContextFactoryTests
    {
        private IFixture fixture;
        private Mock<IClock> mockClock;

        private DateTimeOffset now;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            now = fixture.Create<DateTimeOffset>();

            mockClock = new Mock<IClock>();
            mockClock.SetupGet(p => p.Now).Returns(now);

        }

        private DefaultEventContextFactory CreateSystemUnderTest()
        {
            return new DefaultEventContextFactory(mockClock.Object);
        }

        [Test]
        [ExpectedException]
        public void Clock_is_required()
        {
            new DefaultEventContextFactory(null);
        }

        [Test]
        public void CreateContext_contains_received_message_body()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<EventMessage<TestEvent>>();

            var context = sut.CreateContext(message, Mock.Of<INybusOptions>());

            Assert.That(context.Message, Is.EqualTo(message.Event));
        }

        [Test]
        public void CreateContext_contains_received_correlationId()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<EventMessage<TestEvent>>();

            var context = sut.CreateContext(message, Mock.Of<INybusOptions>());

            Assert.That(context.CorrelationId, Is.EqualTo(message.CorrelationId));
        }

        [Test]
        public void CreateContext_contains_current_time()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<EventMessage<TestEvent>>();

            var context = sut.CreateContext(message, Mock.Of<INybusOptions>());

            Assert.That(context.ReceivedOn, Is.EqualTo(now));
        }

    }
}