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
    public class DefaultCommandContextFactoryTests
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

        private DefaultCommandContextFactory CreateSystemUnderTest()
        {
            return new DefaultCommandContextFactory(mockClock.Object);
        }

        [Test]
        [ExpectedException]
        public void Clock_is_required()
        {
            new DefaultCommandContextFactory(null);
        }

        [Test]
        public void CreateContext_contains_received_message_body()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            var context = sut.CreateContext(message);

            Assert.That(context.Message, Is.EqualTo(message.Command));
        }

        [Test]
        public void CreateContext_contains_received_correlationId()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            var context = sut.CreateContext(message);

            Assert.That(context.CorrelationId, Is.EqualTo(message.CorrelationId));
        }

        [Test]
        public void CreateContext_contains_current_time()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            var context = sut.CreateContext(message);

            Assert.That(context.ReceivedOn, Is.EqualTo(now));
        }

    }
}