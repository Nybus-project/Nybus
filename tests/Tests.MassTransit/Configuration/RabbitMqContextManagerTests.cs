using System;
using MassTransit;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Nybus.Utils;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    public class RabbitMqContextManagerTests
    {
        private IFixture fixture;

        private Mock<IConsumeContext<TestEvent>> mockEventConsumeContext;

        private Mock<IPublishContext<TestEvent>> mockEventPublishContext;

        private Mock<IConsumeContext<TestCommand>> mockCommandConsumeContext;

        private Mock<IPublishContext<TestCommand>> mockCommandPublishContext;

        private DateTimeOffset now;

        private Mock<IClock> mockClock;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            mockEventConsumeContext = new Mock<IConsumeContext<TestEvent>>();

            mockEventPublishContext = new Mock<IPublishContext<TestEvent>>();

            mockCommandConsumeContext = new Mock<IConsumeContext<TestCommand>>();

            mockCommandPublishContext = new Mock<IPublishContext<TestCommand>>();

            now = fixture.Create<DateTimeOffset>();

            mockClock = new Mock<IClock>();
            mockClock.SetupGet(p => p.Now).Returns(now);

            Clock.Default = mockClock.Object;
        }

        private RabbitMqContextManager CreateSystemUnderTest()
        {
            return new RabbitMqContextManager();
        }

        [Test]
        public void CreateEventMessage_retrieves_CorrelationId_from_context()
        {
            var sut = CreateSystemUnderTest();

            var body = fixture.Create<TestEvent>();

            var correlationId = fixture.Create<Guid>();

            mockEventConsumeContext.SetupGet(p => p.Message).Returns(body);
            mockEventConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.CorrelationIdKey]).Returns(correlationId.ToString());
            mockEventConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.MessageSentKey]).Returns(now.ToString("O"));

            var message = sut.CreateEventMessage(mockEventConsumeContext.Object);

            Assert.That(message.CorrelationId, Is.EqualTo(correlationId));
        }

        [Test]
        public void CreateEventMessage_retrieves_Event_from_context()
        {
            var sut = CreateSystemUnderTest();

            TestEvent body = fixture.Create<TestEvent>();

            Guid correlationId = fixture.Create<Guid>();

            mockEventConsumeContext.SetupGet(p => p.Message).Returns(body);
            mockEventConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.CorrelationIdKey]).Returns(correlationId.ToString());
            mockEventConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.MessageSentKey]).Returns(now.ToString("O"));

            var message = sut.CreateEventMessage(mockEventConsumeContext.Object);

            Assert.That(message.Event, Is.SameAs(body));
        }

        [Test]
        public void CreateEventMessage_returns_Now_if_SentOn_is_missing()
        {
            var sut = CreateSystemUnderTest();

            TestEvent body = fixture.Create<TestEvent>();

            Guid correlationId = fixture.Create<Guid>();

            mockEventConsumeContext.SetupGet(p => p.Message).Returns(body);
            mockEventConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.CorrelationIdKey]).Returns(correlationId.ToString());

            var message = sut.CreateEventMessage(mockEventConsumeContext.Object);

            Assert.That(message.SentOn, Is.EqualTo(now));
        }

        [Test]
        public void CreateEventMessage_returns_NewGuid_if_CorrelationId_is_missing()
        {
            var sut = CreateSystemUnderTest();

            TestEvent body = fixture.Create<TestEvent>();

            mockEventConsumeContext.SetupGet(p => p.Message).Returns(body);
            mockEventConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.MessageSentKey]).Returns(now.ToString("O"));

            var message = sut.CreateEventMessage(mockEventConsumeContext.Object);

            Assert.That(message.CorrelationId, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void SetEventMessageHeaders_sets_correlationId_to_context()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<EventMessage<TestEvent>>();

            sut.SetEventMessageHeaders(message, mockEventPublishContext.Object);

            mockEventPublishContext.Verify(p => p.SetHeader(RabbitMqContextManager.CorrelationIdKey, message.CorrelationId.ToString("D")));
            mockEventPublishContext.Verify(p => p.SetHeader(RabbitMqContextManager.MessageSentKey, message.SentOn.ToString("O")));
        }

        [Test]
        public void CreateCommandMessage_retrieves_CorrelationId_from_context()
        {
            var sut = CreateSystemUnderTest();

            var body = fixture.Create<TestCommand>();

            var correlationId = fixture.Create<Guid>();

            mockCommandConsumeContext.SetupGet(p => p.Message).Returns(body);
            mockCommandConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.CorrelationIdKey]).Returns(correlationId.ToString());
            mockCommandConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.MessageSentKey]).Returns(now.ToString("O"));

            var message = sut.CreateCommandMessage(mockCommandConsumeContext.Object);

            Assert.That(message.CorrelationId, Is.EqualTo(correlationId));
            Assert.That(message.SentOn, Is.EqualTo(now));
        }

        [Test]
        public void CreateCommandMessage_retrieves_Command_from_context()
        {
            var sut = CreateSystemUnderTest();

            var body = fixture.Create<TestCommand>();

            var correlationId = fixture.Create<Guid>();

            mockCommandConsumeContext.SetupGet(p => p.Message).Returns(body);
            mockCommandConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.CorrelationIdKey]).Returns(correlationId.ToString());
            mockCommandConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.MessageSentKey]).Returns(now.ToString("O"));

            var message = sut.CreateCommandMessage(mockCommandConsumeContext.Object);

            Assert.That(message.Command, Is.SameAs(body));
            Assert.That(message.SentOn, Is.EqualTo(now));
        }

        [Test]
        public void CreateCommandMessage_returns_Now_if_SentOn_is_missing()
        {
            var sut = CreateSystemUnderTest();

            var body = fixture.Create<TestCommand>();

            var correlationId = fixture.Create<Guid>();

            mockCommandConsumeContext.SetupGet(p => p.Message).Returns(body);
            mockCommandConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.CorrelationIdKey]).Returns(correlationId.ToString());

            var message = sut.CreateCommandMessage(mockCommandConsumeContext.Object);

            Assert.That(message.SentOn, Is.EqualTo(now));
        }

        [Test]
        public void CreateCommandMessage_returns_NewGuid_if_CorrelationId_is_missing()
        {
            var sut = CreateSystemUnderTest();

            var body = fixture.Create<TestCommand>();

            mockCommandConsumeContext.SetupGet(p => p.Message).Returns(body);
            mockCommandConsumeContext.Setup(p => p.Headers[RabbitMqContextManager.MessageSentKey]).Returns(now.ToString("O"));

            var message = sut.CreateCommandMessage(mockCommandConsumeContext.Object);

            Assert.That(message.CorrelationId, Is.Not.EqualTo(Guid.Empty));
        }


        [Test]
        public void SetCommandMessageHeaders_sets_correlationId_to_context()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            sut.SetCommandMessageHeaders(message, mockCommandPublishContext.Object);

            mockCommandPublishContext.Verify(p => p.SetHeader(RabbitMqContextManager.CorrelationIdKey, message.CorrelationId.ToString("D")));
            mockCommandPublishContext.Verify(p => p.SetHeader(RabbitMqContextManager.MessageSentKey, message.SentOn.ToString("O")));
        }


    }
}