using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.InMemory;

namespace Tests.InMemory
{
    [TestFixture]
    public class InMemoryBusEngineTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(InMemoryBusEngine).GetConstructors());
        }

        [Test, AutoMoqData]
        public void SubscribeToCommand_adds_type_to_AcceptedTypes_list(InMemoryBusEngine sut)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            Assert.That(sut.IsTypeAccepted(typeof(FirstTestCommand)));
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_adds_type_AcceptedTypes_list(InMemoryBusEngine sut)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            Assert.That(sut.IsTypeAccepted(typeof(FirstTestEvent)));
        }

        [Test, AutoMoqData]
        public async Task Sent_commands_are_received([Frozen] IEnvelopeService envelopeService, InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage, IFixture fixture)
        {
            fixture.Customize<Envelope>(c => c
                                             .With(p => p.Type, testMessage.Type)
                                             .With(p => p.Headers, testMessage.Headers)
                                             .With(p => p.Content)
                                             .With(p => p.MessageId, testMessage.MessageId)
                                             .With(p => p.MessageType, testMessage.MessageType)
            );

            Mock.Get(envelopeService).Setup(p => p.CreateEnvelope(It.IsAny<CommandMessage<FirstTestCommand>>())).ReturnsUsingFixture(fixture);
            Mock.Get(envelopeService).Setup(p => p.CreateCommandMessage(It.IsAny<Envelope>(), It.IsAny<Type>())).Returns(testMessage);

            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = await sut.StartAsync().ConfigureAwait(false);

            var items = sequence.DumpInList();

            await sut.SendCommandAsync(testMessage);

            Assert.That(items.First(), Is.EqualTo(testMessage).Using<CommandMessage<FirstTestCommand>>((x, y) => x.MessageId == y.MessageId));
        }

        [Test, AutoMoqData]
        public async Task Sent_events_are_received([Frozen] IEnvelopeService envelopeService, InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage, IFixture fixture)
        {
            fixture.Customize<Envelope>(c => c
                                             .With(p => p.Type, testMessage.Type)
                                             .With(p => p.Headers, testMessage.Headers)
                                             .With(p => p.Content)
                                             .With(p => p.MessageId, testMessage.MessageId)
                                             .With(p => p.MessageType, testMessage.MessageType)
            );

            Mock.Get(envelopeService).Setup(p => p.CreateEnvelope(It.IsAny<EventMessage<FirstTestEvent>>())).ReturnsUsingFixture(fixture);
            Mock.Get(envelopeService).Setup(p => p.CreateEventMessage(It.IsAny<Envelope>(), It.IsAny<Type>())).Returns(testMessage);

            sut.SubscribeToEvent<FirstTestEvent>();

            var sequence = await sut.StartAsync().ConfigureAwait(false);

            var items = sequence.DumpInList();

            await sut.SendEventAsync(testMessage);

            Assert.That(items.First(), Is.EqualTo(testMessage).Using<EventMessage<FirstTestEvent>>((x, y) => x.MessageId == y.MessageId));
        }

        [Test, AutoMoqData]
        public void Stop_completes_the_sequence_if_started(InMemoryBusEngine sut)
        {
            var sequence = sut.StartAsync().Result;

            var isCompleted = false;

            sequence.Subscribe(
                onNext: _ => { },
                onError: _ => { },
                onCompleted: () => isCompleted = true
            );

            sut.StopAsync().Wait();

            Assert.That(isCompleted, Is.True);
        }

        [Test, AutoMoqData]
        public void Stop_is_ignored_if_not_started(InMemoryBusEngine sut)
        {
            sut.StopAsync().Wait();
        }

        [Test, AutoMoqData]
        public void NotifySuccess_returns_completed_task(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            Assert.That(sut.NotifySuccessAsync(testMessage), Is.SameAs(Task.CompletedTask));
        }

        [Test, AutoMoqData]
        public void NotifySuccess_raises_event(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var handler = Mock.Of <EventHandler<MessageEventArgs>>();
            sut.OnMessageNotifySuccess += handler;

            sut.NotifySuccessAsync(testMessage);

            sut.OnMessageNotifySuccess -= handler;

            Mock.Get(handler).Verify(p => p(sut, It.Is<MessageEventArgs>(m => ReferenceEquals(m.Message, testMessage))));
        }

        [Test, AutoMoqData]
        public void NotifySuccess_raises_event(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            var handler = Mock.Of<EventHandler<MessageEventArgs>>();
            sut.OnMessageNotifySuccess += handler;

            sut.NotifySuccessAsync(testMessage);

            sut.OnMessageNotifySuccess -= handler;

            Mock.Get(handler).Verify(p => p(sut, It.Is<MessageEventArgs>(m => ReferenceEquals(m.Message, testMessage))));
        }

        [Test, AutoMoqData]
        public void NotifySuccess_returns_completed_task(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            Assert.That(sut.NotifySuccessAsync(testMessage), Is.SameAs(Task.CompletedTask));
        }

        [Test, AutoMoqData]
        public void NotifyFail_returns_completed_task(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            Assert.That(sut.NotifyFailAsync(testMessage), Is.SameAs(Task.CompletedTask));
        }

        [Test, AutoMoqData]
        public void NotifyFail_returns_completed_task(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            Assert.That(sut.NotifyFailAsync(testMessage), Is.SameAs(Task.CompletedTask));
        }

        [Test, AutoMoqData]
        public void NotifyFail_raises_event(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var handler = Mock.Of<EventHandler<MessageEventArgs>>();
            sut.OnMessageNotifyFail += handler;

            sut.NotifyFailAsync(testMessage);

            sut.OnMessageNotifyFail -= handler;

            Mock.Get(handler).Verify(p => p(sut, It.Is<MessageEventArgs>(m => ReferenceEquals(m.Message, testMessage))));
        }

        [Test, AutoMoqData]
        public void NotifyFail_raises_event(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            var handler = Mock.Of<EventHandler<MessageEventArgs>>();
            sut.OnMessageNotifyFail += handler;

            sut.NotifyFailAsync(testMessage);

            sut.OnMessageNotifyFail -= handler;

            Mock.Get(handler).Verify(p => p(sut, It.Is<MessageEventArgs>(m => ReferenceEquals(m.Message, testMessage))));
        }
    }

    [TestFixture]
    public class MessageEventArgsTests
    {
        [Test]
        public void Message_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new MessageEventArgs(null));
        }

        [Test, AutoMoqData]
        public void Message_is_attached([Frozen] Message message, MessageEventArgs sut)
        {
            Assert.That(sut.Message, Is.SameAs(message));
        }
    }
}
