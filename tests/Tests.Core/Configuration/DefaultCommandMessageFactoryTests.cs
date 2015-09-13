using System;
using NUnit.Framework;
using Nybus.Configuration;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class DefaultCommandMessageFactoryTests
    {
        private IFixture fixture;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();
        }

        private DefaultCommandMessageFactory CreateSystemUnderTest()
        {
            return new DefaultCommandMessageFactory();
        }

        [Test]
        public void CreateMessage_returns_the_command()
        {
            var sut = CreateSystemUnderTest();

            var command = fixture.Create<TestCommand>();
            var correlationId = fixture.Create<Guid>();

            var message = sut.CreateMessage(command, correlationId);

            Assert.That(message.Command, Is.EqualTo(command));
        }

        [Test]
        public void CreateMessage_returns_the_correlationId()
        {
            var sut = CreateSystemUnderTest();

            var command = fixture.Create<TestCommand>();
            var correlationId = fixture.Create<Guid>();

            var message = sut.CreateMessage(command, correlationId);

            Assert.That(message.CorrelationId, Is.EqualTo(correlationId));
        }

    }
}