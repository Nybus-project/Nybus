using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class InMemoryBusEngineTests
    {
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
        public async Task Sent_commands_are_received(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = sut.Start();

            var items = sequence.DumpInList();

            await sut.SendCommandAsync(testMessage);
            
            Assert.That(items.First(), Is.SameAs(testMessage));
        }

        [Test, AutoMoqData]
        public async Task Sent_events_are_received(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            var sequence = sut.Start();

            var items = sequence.DumpInList();

            await sut.SendEventAsync(testMessage);

            Assert.That(items.First(), Is.SameAs(testMessage));
        }

        [Test, AutoMoqData]
        public void Stop_completes_the_sequence_if_started(InMemoryBusEngine sut)
        {
            var sequence = sut.Start();

            var isCompleted = false;

            sequence.Subscribe(
                onNext: _ => { },
                onError: _ => { },
                onCompleted: () => isCompleted = true
            );

            sut.Stop();

            Assert.That(isCompleted, Is.True);
        }

        [Test, AutoMoqData]
        public void Stop_is_ignored_if_not_started(InMemoryBusEngine sut)
        {
            sut.Stop();
        }

        [Test, AutoMoqData]
        public void NotifySuccess_returns_completed_task(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            Assert.That(sut.NotifySuccess(testMessage), Is.SameAs(Task.CompletedTask));
        }

        [Test, AutoMoqData]
        public void NotifySuccess_raises_event(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var handler = Mock.Of <EventHandler<MessageEventArgs>>();
            sut.OnMessageNotifySuccess += handler;

            sut.NotifySuccess(testMessage);

            sut.OnMessageNotifySuccess -= handler;

            Mock.Get(handler).Verify(p => p(sut, It.Is<MessageEventArgs>(m => ReferenceEquals(m.Message, testMessage))));
        }

        [Test, AutoMoqData]
        public void NotifySuccess_raises_event(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            var handler = Mock.Of<EventHandler<MessageEventArgs>>();
            sut.OnMessageNotifySuccess += handler;

            sut.NotifySuccess(testMessage);

            sut.OnMessageNotifySuccess -= handler;

            Mock.Get(handler).Verify(p => p(sut, It.Is<MessageEventArgs>(m => ReferenceEquals(m.Message, testMessage))));
        }

        [Test, AutoMoqData]
        public void NotifySuccess_returns_completed_task(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            Assert.That(sut.NotifySuccess(testMessage), Is.SameAs(Task.CompletedTask));
        }

        [Test, AutoMoqData]
        public void NotifyFail_returns_completed_task(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            Assert.That(sut.NotifyFail(testMessage), Is.SameAs(Task.CompletedTask));
        }

        [Test, AutoMoqData]
        public void NotifyFail_returns_completed_task(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            Assert.That(sut.NotifyFail(testMessage), Is.SameAs(Task.CompletedTask));
        }

        [Test, AutoMoqData]
        public void NotifyFail_raises_event(InMemoryBusEngine sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var handler = Mock.Of<EventHandler<MessageEventArgs>>();
            sut.OnMessageNotifyFail += handler;

            sut.NotifyFail(testMessage);

            sut.OnMessageNotifyFail -= handler;

            Mock.Get(handler).Verify(p => p(sut, It.Is<MessageEventArgs>(m => ReferenceEquals(m.Message, testMessage))));
        }

        [Test, AutoMoqData]
        public void NotifyFail_raises_event(InMemoryBusEngine sut, EventMessage<FirstTestEvent> testMessage)
        {
            var handler = Mock.Of<EventHandler<MessageEventArgs>>();
            sut.OnMessageNotifyFail += handler;

            sut.NotifyFail(testMessage);

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
