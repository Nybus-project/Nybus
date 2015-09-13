using System;
using NUnit.Framework;
using Nybus.Configuration;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class DefaultEventMessageFactoryTests
    {
        private IFixture fixture;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();
        }

        private DefaultEventMessageFactory CreateSystemUnderTest()
        {
            return new DefaultEventMessageFactory();
        }

        [Test]
        public void CreateMessage_returns_the_command()
        {
            var sut = CreateSystemUnderTest();

            var testEvent = fixture.Create<TestEvent>();
            var correlationId = fixture.Create<Guid>();

            var message = sut.CreateMessage(testEvent, correlationId);

            Assert.That(message.Event, Is.EqualTo(testEvent));
        }

        [Test]
        public void CreateMessage_returns_the_correlationId()
        {
            var sut = CreateSystemUnderTest();

            var testEvent = fixture.Create<TestEvent>();
            var correlationId = fixture.Create<Guid>();

            var message = sut.CreateMessage(testEvent, correlationId);

            Assert.That(message.CorrelationId, Is.EqualTo(correlationId));
        }

    }
}