using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Nybus.Container;
using Nybus.Logging;
using Nybus.Utils;
using Ploeh.AutoFixture;

namespace Tests
{
    public class TestCommand : ICommand
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public bool BoolValue { get; set; }

        public DateTimeOffset DateTimeOffsetValue { get; set; }
    }

    public class TestCommandHandler : ICommandHandler<TestCommand> {
        public Task Handle(CommandContext<TestCommand> commandMessage) => Task.CompletedTask;
    }

    public class TestEvent : IEvent
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public bool BoolValue { get; set; }

        public DateTimeOffset DateTimeOffsetValue { get; set; }
    }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public Task Handle(EventContext<TestEvent> eventMessage) => Task.CompletedTask;
    }

    [TestFixture]
    public class NybusTests
    {
        private IFixture fixture;

        private Mock<IBusEngine> mockBusEngine;
        private Mock<ILogger> mockLogger;
        private Mock<ICommandContextFactory> mockCommandContextFactory;
        private Mock<IEventContextFactory> mockEventContextFactory;
        private Mock<ICommandMessageFactory> mockCommandMessageFactory;
        private Mock<IEventMessageFactory> mockEventMessageFactory;
        private Mock<IContainer> mockContainer;
        private Mock<ICorrelationIdGenerator> mockCorrelationIdGenerator;
        private NybusOptions options;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            mockBusEngine = new Mock<IBusEngine>();

            mockLogger = new Mock<ILogger>();
            mockCommandContextFactory = new Mock<ICommandContextFactory>();
            mockCommandMessageFactory = new Mock<ICommandMessageFactory>();
            mockEventContextFactory = new Mock<IEventContextFactory>();
            mockEventMessageFactory = new Mock<IEventMessageFactory>();
            mockContainer = new Mock<IContainer>();
            mockCorrelationIdGenerator = new Mock<ICorrelationIdGenerator>();

            options = new NybusOptions
            {
                Logger = mockLogger.Object,
                CommandContextFactory = mockCommandContextFactory.Object,
                EventContextFactory = mockEventContextFactory.Object,
                CommandMessageFactory = mockCommandMessageFactory.Object,
                EventMessageFactory = mockEventMessageFactory.Object,
                Container = mockContainer.Object,
                CorrelationIdGenerator = mockCorrelationIdGenerator.Object
            };
        }

        private Nybus.NybusBus CreateSystemUnderTest()
        {
            return new Nybus.NybusBus(mockBusEngine.Object, options);
        }

        [Test]
        [ExpectedException]
        public void BusEngine_is_required()
        {
            new Nybus.NybusBus(null, options);
        }

        [Test]
        [ExpectedException]
        public void Options_is_required()
        {
            new Nybus.NybusBus(mockBusEngine.Object, null);
        }

        private bool LogDataContains(object data, string key, object expectedValue)
        {
            var dictionary = ObjectToDictionary.Convert(data);

            if (!dictionary.ContainsKey(key))
                return false;

            var value = dictionary[key];

            Assert.That(value,Is.EqualTo(expectedValue));

            return true;
        }


        #region InvokeCommand

        [Test]
        public async Task InvokeCommand_generates_a_correlationId_when_no_is_provided()
        {
            var sut = CreateSystemUnderTest();

            var command = fixture.Create<TestCommand>();
            var correlationId = fixture.Create<Guid>();
            var commandMessage = fixture.Create<CommandMessage<TestCommand>>();

            mockCorrelationIdGenerator.Setup(p => p.Generate()).Returns(correlationId);
            mockCommandMessageFactory.Setup(p => p.CreateMessage(It.IsAny<TestCommand>(), It.IsAny<Guid>()))
                .Returns(commandMessage);

            await sut.InvokeCommand(command);

            mockCorrelationIdGenerator.Verify(p => p.Generate(), Times.Once);
            mockCommandMessageFactory.Verify(p => p.CreateMessage(command, correlationId), Times.Once);
        }

        [Test]
        public async Task InvokeCommand_does_not_generate_a_correlationId_when_is_provided()
        {
            var sut = CreateSystemUnderTest();

            var command = fixture.Create<TestCommand>();
            var correlationId = fixture.Create<Guid>();
            var commandMessage = fixture.Create<CommandMessage<TestCommand>>();

            mockCommandMessageFactory.Setup(p => p.CreateMessage(It.IsAny<TestCommand>(), It.IsAny<Guid>()))
                .Returns(commandMessage);

            await sut.InvokeCommand(command, correlationId);

            mockCorrelationIdGenerator.Verify(p => p.Generate(), Times.Never);
            mockCommandMessageFactory.Verify(p => p.CreateMessage(command, correlationId), Times.Once);
        }

        [Test]
        public async Task InvokeCommand_outputs_log_with_correlationId()
        {
            var sut = CreateSystemUnderTest();

            var command = fixture.Create<TestCommand>();
            var correlationId = fixture.Create<Guid>();
            var commandMessage = fixture.Create<CommandMessage<TestCommand>>();

            mockCommandMessageFactory.Setup(p => p.CreateMessage(It.IsAny<TestCommand>(), It.IsAny<Guid>()))
                .Returns(commandMessage);

            await sut.InvokeCommand(command, correlationId);

            mockLogger.Verify(p => p.LogAsync(LogLevel.Info, It.IsAny<string>(), It.Is<object>(o => LogDataContains(o, "correlationId", commandMessage.CorrelationId)), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task InvokeCommand_outputs_log_with_command_type()
        {
            var sut = CreateSystemUnderTest();

            var command = fixture.Create<TestCommand>();
            var correlationId = fixture.Create<Guid>();
            var commandMessage = fixture.Create<CommandMessage<TestCommand>>();

            mockCommandMessageFactory.Setup(p => p.CreateMessage(It.IsAny<TestCommand>(), It.IsAny<Guid>()))
                .Returns(commandMessage);

            await sut.InvokeCommand(command, correlationId);

            mockLogger.Verify(p => p.LogAsync(LogLevel.Info, It.IsAny<string>(), It.Is<object>(o => LogDataContains(o, "type", command.GetType().FullName)), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task InvokeCommand_sends_a_message()
        {
            var sut = CreateSystemUnderTest();

            var command = fixture.Create<TestCommand>();
            var correlationId = fixture.Create<Guid>();
            var commandMessage = fixture.Create<CommandMessage<TestCommand>>();

            mockCommandMessageFactory.Setup(p => p.CreateMessage(It.IsAny<TestCommand>(), It.IsAny<Guid>()))
                .Returns(commandMessage);

            await sut.InvokeCommand(command, correlationId);

            mockBusEngine.Verify(p => p.SendCommand(commandMessage),Times.Once);
        }

        #endregion

        #region RaiseEvent

        [Test]
        public async Task RaiseEvent_generates_a_correlationId_when_no_is_provided()
        {
            var sut = CreateSystemUnderTest();

            var ev = fixture.Create<TestEvent>();
            var correlationId = fixture.Create<Guid>();
            var eventMessage = fixture.Create<EventMessage<TestEvent>>();

            mockCorrelationIdGenerator.Setup(p => p.Generate()).Returns(correlationId);
            mockEventMessageFactory.Setup(p => p.CreateMessage(It.IsAny<TestEvent>(), It.IsAny<Guid>()))
                .Returns(eventMessage);

            await sut.RaiseEvent(ev);

            mockCorrelationIdGenerator.Verify(p => p.Generate(), Times.Once);
            mockEventMessageFactory.Verify(p => p.CreateMessage(ev, correlationId), Times.Once);
        }

        [Test]
        public async Task RaiseEvent_does_not_generate_a_correlationId_when_is_provided()
        {
            var sut = CreateSystemUnderTest();

            var ev = fixture.Create<TestEvent>();
            var correlationId = fixture.Create<Guid>();
            var eventMessage = fixture.Create<EventMessage<TestEvent>>();

            mockEventMessageFactory.Setup(p => p.CreateMessage(It.IsAny<TestEvent>(), It.IsAny<Guid>()))
                .Returns(eventMessage);

            await sut.RaiseEvent(ev, correlationId);

            mockCorrelationIdGenerator.Verify(p => p.Generate(), Times.Never);
            mockEventMessageFactory.Verify(p => p.CreateMessage(ev, correlationId), Times.Once);
        }

        [Test]
        public async Task RaiseEvent_outputs_log_with_correlationId()
        {
            var sut = CreateSystemUnderTest();

            var ev = fixture.Create<TestEvent>();
            var correlationId = fixture.Create<Guid>();
            var eventMessage = fixture.Create<EventMessage<TestEvent>>();

            mockEventMessageFactory.Setup(p => p.CreateMessage(It.IsAny<TestEvent>(), It.IsAny<Guid>()))
                .Returns(eventMessage);

            await sut.RaiseEvent(ev, correlationId);

            mockLogger.Verify(p => p.LogAsync(LogLevel.Info, It.IsAny<string>(), It.Is<object>(o => LogDataContains(o, "correlationId", eventMessage.CorrelationId)), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task RaiseEvent_outputs_log_with_command_type()
        {
            var sut = CreateSystemUnderTest();

            var ev = fixture.Create<TestEvent>();
            var correlationId = fixture.Create<Guid>();
            var eventMessage = fixture.Create<EventMessage<TestEvent>>();

            mockEventMessageFactory.Setup(p => p.CreateMessage(It.IsAny<TestEvent>(), It.IsAny<Guid>()))
                .Returns(eventMessage);

            await sut.RaiseEvent(ev, correlationId);

            mockLogger.Verify(p => p.LogAsync(LogLevel.Info, It.IsAny<string>(), It.Is<object>(o => LogDataContains(o, "type", ev.GetType().FullName)), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task RaiseEvent_sends_a_message()
        {
            var sut = CreateSystemUnderTest();

            var ev = fixture.Create<TestEvent>();
            var correlationId = fixture.Create<Guid>();
            var eventMessage = fixture.Create<EventMessage<TestEvent>>();

            mockEventMessageFactory.Setup(p => p.CreateMessage(It.IsAny<TestEvent>(), It.IsAny<Guid>()))
                .Returns(eventMessage);

            await sut.RaiseEvent(ev, correlationId);

            mockBusEngine.Verify(p => p.SendEvent(eventMessage), Times.Once);
        }

        #endregion

        #region Start

        [Test]
        public async Task Start_starts_the_engine()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            mockBusEngine.Verify(p => p.Start(), Times.Once);
        }

        [Test]
        public async Task Start_logs_when_starting()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            mockLogger.Setup(p => p.LogAsync(LogLevel.Info, It.Is<string>(s => s.Contains("starting")), null, It.IsAny<string>()));
        }

        [Test]
        public async Task Start_logs_when_started()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            mockLogger.Setup(p => p.LogAsync(LogLevel.Info, It.Is<string>(s => s.Contains("started")), null, It.IsAny<string>()));
        }

        #endregion

        #region Stop

        [Test]
        public async Task Start_stops_the_engine()
        {
            var sut = CreateSystemUnderTest();

            await sut.Stop();

            mockBusEngine.Verify(p => p.Stop(), Times.Once);
        }

        [Test]
        public async Task Stop_logs_when_stopping()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            mockLogger.Setup(p => p.LogAsync(LogLevel.Info, It.Is<string>(s => s.Contains("stopping")), null, It.IsAny<string>()));
        }

        [Test]
        public async Task Stop_logs_when_stopped()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            mockLogger.Setup(p => p.LogAsync(LogLevel.Info, It.Is<string>(s => s.Contains("stopped")), null, It.IsAny<string>()));
        }

        #endregion
    }
}
