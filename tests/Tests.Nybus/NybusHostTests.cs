using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{ 
    [TestFixture]
    public class NybusHostTests
    {
        [Test, AutoMoqData]
        public void BusEngine_is_required(NybusHostOptions options)
        {
            Assert.Throws<ArgumentNullException>(() => new NybusHost(null, options, Mock.Of<ILogger<NybusHost>>()));
        }

        [Test, AutoMoqData]
        public void Options_is_required(IBusEngine engine)
        {
            Assert.Throws<ArgumentNullException>(() => new NybusHost(engine, null, Mock.Of<ILogger<NybusHost>>()));
        }

        [Test, AutoMoqData]
        public void Logger_is_required(IBusEngine engine, NybusHostOptions options)
        {
            Assert.Throws<ArgumentNullException>(() => new NybusHost(engine, options, null));
        }

        [Test, AutoMoqData]
        public async Task InvokeCommandAsync_forwards_message_to_engine(NybusHost sut, FirstTestCommand testCommand, Guid correlationId)
        {
            await sut.InvokeCommandAsync(testCommand, correlationId);

            Mock.Get(sut.Engine).Verify(p => p.SendCommandAsync(It.Is<CommandMessage<FirstTestCommand>>(m => ReferenceEquals(m.Command, testCommand) && m.Headers.CorrelationId == correlationId)), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task RaiseEventAsync_forwards_message_to_engine(NybusHost sut, FirstTestEvent testEvent, Guid correlationId)
        {
            await sut.RaiseEventAsync(testEvent, correlationId);

            Mock.Get(sut.Engine).Verify(p => p.SendEventAsync(It.Is<EventMessage<FirstTestEvent>>(m => ReferenceEquals(m.Event, testEvent) && m.Headers.CorrelationId == correlationId)), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task StartAsync_starts_the_engine(NybusHost sut)
        {
            await sut.StartAsync();

            Mock.Get(sut.Engine).Verify(p => p.Start(), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task StopAsync_is_ignored_if_not_started(NybusHost sut)
        {
            await sut.StopAsync();

            Mock.Get(sut.Engine).Verify(p => p.Stop(), Times.Never);

        }

        [Test, AutoMoqData]
        public async Task StopAsync_stops_the_engine_if_started(NybusHost sut)
        {
            await sut.StartAsync();

            await sut.StopAsync();

            Mock.Get(sut.Engine).Verify(p => p.Stop(), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Handler_is_executed_when_commandMessages_are_processed(NybusHost sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(sut.Engine).Setup(p => p.Start()).Returns(subject);

            var receivedMessage = Mock.Of<CommandReceived<FirstTestCommand>>();

            sut.SubscribeToCommand(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(receivedMessage).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Engine_is_notified_when_commandMessages_are_successfully_processed(NybusHost sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(sut.Engine).Setup(p => p.Start()).Returns(subject);

            var receivedMessage = Mock.Of<CommandReceived<FirstTestCommand>>();

            sut.SubscribeToCommand(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(sut.Engine).Verify(p => p.NotifySuccess(testMessage), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Error_policy_is_executed_when_commandMessages_are_processed_with_errors(NybusHost sut, CommandMessage<FirstTestCommand> testMessage, Exception error)
        {
            var subject = new Subject<Message>();

            Mock.Get(sut.Engine).Setup(p => p.Start()).Returns(subject);

            var receivedMessage = Mock.Of<CommandReceived<FirstTestCommand>>();
            Mock.Get(receivedMessage).Setup(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>())).Throws(error);

            sut.SubscribeToCommand(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(sut.Options.ErrorPolicy).Verify(p => p.HandleError(sut.Engine, error, testMessage), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Null_messages_delivered_from_engine_are_discarded(NybusHost sut, CommandMessage<FirstTestCommand> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(sut.Engine).Setup(p => p.Start()).Returns(subject);

            var receivedMessage = Mock.Of<CommandReceived<FirstTestCommand>>();

            sut.SubscribeToCommand(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(null);

            await sut.StopAsync();

            Mock.Get(receivedMessage).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>()), Times.Never);
        }

        [Test, AutoMoqData]
        public async Task Handler_is_executed_when_eventMessages_are_processed(NybusHost sut, EventMessage<FirstTestEvent> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(sut.Engine).Setup(p => p.Start()).Returns(subject);

            var receivedMessage = Mock.Of<EventReceived<FirstTestEvent>>();

            sut.SubscribeToEvent(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(receivedMessage).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Engine_is_notified_when_eventMessages_are_successfully_processed(NybusHost sut, EventMessage<FirstTestEvent> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(sut.Engine).Setup(p => p.Start()).Returns(subject);

            var receivedMessage = Mock.Of<EventReceived<FirstTestEvent>>();

            sut.SubscribeToEvent(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(sut.Engine).Verify(p => p.NotifySuccess(testMessage), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Error_policy_is_executed_when_eventMessages_are_processed_with_errors(NybusHost sut, EventMessage<FirstTestEvent> testMessage, Exception error)
        {
            var subject = new Subject<Message>();

            Mock.Get(sut.Engine).Setup(p => p.Start()).Returns(subject);

            var receivedMessage = Mock.Of<EventReceived<FirstTestEvent>>();
            Mock.Get(receivedMessage).Setup(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>())).Throws(error);

            sut.SubscribeToEvent(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(testMessage);

            await sut.StopAsync();

            Mock.Get(sut.Options.ErrorPolicy).Verify(p => p.HandleError(sut.Engine, error, testMessage), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Null_messages_delivered_from_engine_are_discarded(NybusHost sut, EventMessage<FirstTestEvent> testMessage)
        {
            var subject = new Subject<Message>();

            Mock.Get(sut.Engine).Setup(p => p.Start()).Returns(subject);

            var receivedMessage = Mock.Of<EventReceived<FirstTestEvent>>();

            sut.SubscribeToEvent(receivedMessage);

            await sut.StartAsync();

            subject.OnNext(null);

            await sut.StopAsync();

            Mock.Get(receivedMessage).Verify(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>()), Times.Never);
        }
    }
}