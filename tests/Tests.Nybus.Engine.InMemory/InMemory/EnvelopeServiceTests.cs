using System;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Nybus.InMemory;

namespace Tests.InMemory
{
    [TestFixture]
    public class EnvelopeServiceTests
    {
        [Test, CustomAutoMoqData]
        public void Create_from_CommandMessage_requires_valid_message(EnvelopeService sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.CreateEnvelope((CommandMessage<FirstTestCommand>)null));
        }

        [Test, CustomAutoMoqData]
        public void Create_from_EventMessage_requires_valid_message(EnvelopeService sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.CreateEnvelope((EventMessage<FirstTestEvent>)null));
        }

        [Test, CustomAutoMoqData]
        public void Create_from_CommandMessage_can_create_envelope(EnvelopeService sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var envelope = sut.CreateEnvelope(testMessage);

            Assert.That(envelope, Is.Not.Null);
            Assert.That(envelope.Type, Is.EqualTo(testMessage.Type));
            Assert.That(envelope.Headers, Is.SameAs(testMessage.Headers));
            Assert.That(envelope.MessageId, Is.EqualTo(testMessage.MessageId));
            Assert.That(envelope.MessageType, Is.EqualTo(testMessage.MessageType));
        }

        [Test, CustomAutoMoqData]
        public void Create_from_EventMessage_can_create_envelope(EnvelopeService sut, EventMessage<FirstTestEvent> testMessage)
        {
            var envelope = sut.CreateEnvelope(testMessage);

            Assert.That(envelope, Is.Not.Null);
            Assert.That(envelope.Type, Is.EqualTo(testMessage.Type));
            Assert.That(envelope.Headers, Is.SameAs(testMessage.Headers));
            Assert.That(envelope.MessageId, Is.EqualTo(testMessage.MessageId));
            Assert.That(envelope.MessageType, Is.EqualTo(testMessage.MessageType));
        }

        [Test, CustomAutoMoqData]
        public void Create_from_CommandMessage_uses_serializer([Frozen] ISerializer serializer, EnvelopeService sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var envelope = sut.CreateEnvelope(testMessage);

            Assert.That(envelope, Is.Not.Null);

            Mock.Get(serializer).Verify(p => p.SerializeObject(testMessage.Command), Times.Once);
        }

        [Test, CustomAutoMoqData]
        public void Create_from_EventMessage_uses_serializer([Frozen] ISerializer serializer, EnvelopeService sut, EventMessage<FirstTestEvent> testMessage)
        {
            var envelope = sut.CreateEnvelope(testMessage);

            Assert.That(envelope, Is.Not.Null);

            Mock.Get(serializer).Verify(p => p.SerializeObject(testMessage.Event), Times.Once);
        }

        [Test, CustomAutoMoqData]
        public void CreateCommandMessage_requires_valid_envelope(EnvelopeService sut, Type type)
        {
            Assert.Throws<ArgumentNullException>(() => sut.CreateCommandMessage(null, type));
        }

        [Test, CustomAutoMoqData]
        public void CreateCommandMessage_requires_valid_commandType(EnvelopeService sut, Envelope envelope)
        {
            Assert.Throws<ArgumentNullException>(() => sut.CreateCommandMessage(envelope, null));
        }

        [Test, CustomAutoMoqData]
        public void CreateCommandMessage_returns_message_from_Envelope([Frozen] ISerializer serializer, EnvelopeService sut, Envelope envelope, FirstTestCommand testCommand)
        {
            envelope.MessageType = MessageType.Command;

            Mock.Get(serializer).Setup(p => p.DeserializeObject(It.IsAny<string>(), typeof(FirstTestCommand))).Returns(testCommand);

            var commandMessage = sut.CreateCommandMessage(envelope, typeof(FirstTestCommand)) as CommandMessage<FirstTestCommand>;

            Assert.That(commandMessage, Is.Not.Null);
            Assert.That(commandMessage.Command, Is.SameAs(testCommand));
            Assert.That(commandMessage.Headers, Is.SameAs(envelope.Headers));
            Assert.That(commandMessage.MessageId, Is.EqualTo(envelope.MessageId));
            Assert.That(commandMessage.MessageType, Is.EqualTo(envelope.MessageType));
        }

        [Test, CustomAutoMoqData]
        public void CreateEventMessage_requires_valid_envelope(EnvelopeService sut, Type type)
        {
            Assert.Throws<ArgumentNullException>(() => sut.CreateEventMessage(null, type));
        }

        [Test, CustomAutoMoqData]
        public void CreateEventMessage_requires_valid_eventType(EnvelopeService sut, Envelope envelope)
        {
            Assert.Throws<ArgumentNullException>(() => sut.CreateEventMessage(envelope, null));
        }

        [Test, CustomAutoMoqData]
        public void CreateEventMessage_returns_message_from_Envelope([Frozen] ISerializer serializer, EnvelopeService sut, Envelope envelope, FirstTestEvent testEvent)
        {
            envelope.MessageType = MessageType.Event;

            Mock.Get(serializer).Setup(p => p.DeserializeObject(It.IsAny<string>(), typeof(FirstTestEvent))).Returns(testEvent);

            var eventMessage = sut.CreateEventMessage(envelope, typeof(FirstTestEvent)) as EventMessage<FirstTestEvent>;

            Assert.That(eventMessage, Is.Not.Null);
            Assert.That(eventMessage.Event, Is.SameAs(testEvent));
            Assert.That(eventMessage.Headers, Is.SameAs(envelope.Headers));
            Assert.That(eventMessage.MessageId, Is.EqualTo(envelope.MessageId));
            Assert.That(eventMessage.MessageType, Is.EqualTo(envelope.MessageType));
        }
    }
}
