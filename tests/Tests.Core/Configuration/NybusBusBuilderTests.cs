using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Nybus.Container;
using Nybus.Logging;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class NybusBusBuilderTests
    {
        private IFixture fixture;

        private InMemoryBusEngine testBusEngine;
        private Mock<ILogger> mockLogger;
        private Mock<ICommandContextFactory> mockCommandContextFactory;
        private Mock<IEventContextFactory> mockEventContextFactory;
        private Mock<ICommandMessageFactory> mockCommandMessageFactory;
        private Mock<IEventMessageFactory> mockEventMessageFactory;
        private Mock<IContainer> mockContainer;
        private Mock<ICorrelationIdGenerator> mockCorrelationIdGenerator;
        private NybusOptions options;

        private Mock<ICommandHandler<TestCommand>> mockCommandHandler;
        private Mock<IEventHandler<TestEvent>> mockEventHandler;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            testBusEngine = new InMemoryBusEngine();

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

            mockCommandHandler = new Mock<ICommandHandler<TestCommand>>();
            mockEventHandler = new Mock<IEventHandler<TestEvent>>();
        }

        private NybusBusBuilder CreateSystemUnderTest()
        {
            return new NybusBusBuilder(testBusEngine, options);
        }

        [Test, ExpectedException]
        public void BusEngine_is_required()
        {
            new NybusBusBuilder(null, options);
        }

        [Test, ExpectedException]
        public void Options_cannot_be_null()
        {
            new NybusBusBuilder(testBusEngine, null);
        }

        [Test]
        public void Options_is_optional()
        {
            var bus = new NybusBusBuilder(testBusEngine);

            Assert.That(bus, Is.Not.Null);
        }

        #region Build

        [Test]
        public void Build_returns_a_Bus()
        {
            var sut = CreateSystemUnderTest();

            var bus = sut.Build();

            Assert.That(bus, Is.Not.Null);
        }

        [Test]
        public void Build_emits_log()
        {
            var sut = CreateSystemUnderTest();

            var bus = sut.Build();

            mockLogger.Verify(p => p.Log(It.IsAny<LogLevel>(), It.Is<string>(s => s.Contains("Building")), It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region SubscribeToEvent

        [Test]
        public void SubscribeToEvent_TEvent_invokes_bus_engine()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToEvent<TestEvent>();

            Assert.That(testBusEngine.IsEventHandeld<TestEvent>());
        }

        [Test]
        public void SubscribeToEvent_TEvent_TEventHandler_invokes_bus_engine()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToEvent<TestEventHandler, TestEvent>();

            Assert.That(testBusEngine.IsEventHandeld<TestEvent>());
        }

        [Test]
        public void SubscribeToEvent_Func_invokes_bus_engine()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToEvent<TestEvent>(te => Task.CompletedTask);

            Assert.That(testBusEngine.IsEventHandeld<TestEvent>());
        }

        [Test]
        public void SubscribeToEvent_TEvent_TEventHandler_instance_invokes_bus_engine()
        {
            var sut = CreateSystemUnderTest();

            var handler = Mock.Of<IEventHandler<TestEvent>>();

            sut.SubscribeToEvent<IEventHandler<TestEvent>, TestEvent>(handler);

            Assert.That(testBusEngine.IsEventHandeld<TestEvent>());
        }

        [Test]
        public async Task SubscribeToEvent_TEvent_is_invoked_when_message_is_received()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToEvent<TestEvent>();

            var message = fixture.Create<EventMessage<TestEvent>>();

            var context = fixture.Create<EventContext<TestEvent>>();

            mockContainer.Setup(p => p.Resolve<IEventHandler<TestEvent>>()).Returns(mockEventHandler.Object);
            mockEventContextFactory.Setup(p => p.CreateContext(It.IsAny<EventMessage<TestEvent>>())).Returns(context);
            mockEventHandler.Setup(p => p.Handle(It.IsAny<EventContext<TestEvent>>())).Returns(Task.CompletedTask);

            await testBusEngine.HandleEvent(message);

            mockContainer.Verify(p => p.Resolve<IEventHandler<TestEvent>>(), Times.Once);
            mockEventContextFactory.Verify(p => p.CreateContext(message), Times.Once);
            mockEventHandler.Verify(p => p.Handle(context), Times.Once);
        }

        [Test]
        public async Task SubscribeToEvent_TEvent_TEventHandler_instance_is_invoked_when_message_is_received()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToEvent<IEventHandler<TestEvent>, TestEvent>(mockEventHandler.Object);

            var message = fixture.Create<EventMessage<TestEvent>>();

            var context = fixture.Create<EventContext<TestEvent>>();

            mockEventContextFactory.Setup(p => p.CreateContext(It.IsAny<EventMessage<TestEvent>>())).Returns(context);
            mockEventHandler.Setup(p => p.Handle(It.IsAny<EventContext<TestEvent>>())).Returns(Task.CompletedTask);

            await testBusEngine.HandleEvent(message);

            mockEventContextFactory.Verify(p => p.CreateContext(message), Times.Once);
            mockEventHandler.Verify(p => p.Handle(context), Times.Once);
        }


        #endregion

        #region SubscribeToCommand

        [Test]
        public void SubscribeToCommand_TCommand_invokes_bus_engine()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>();

            Assert.That(testBusEngine.IsCommandHandled<TestCommand>());
        }

        [Test]
        public void SubscribeToCommand_TCommand_TCommandHandler_invokes_bus_engine()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommandHandler, TestCommand>();

            Assert.That(testBusEngine.IsCommandHandled<TestCommand>());
        }

        [Test]
        public void SubscribeToCommand_Func_invokes_bus_engine()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>(te => Task.CompletedTask);

            Assert.That(testBusEngine.IsCommandHandled<TestCommand>());
        }

        [Test]
        public void SubscribeToCommand_TCommand_TCommandHandler_instance_invokes_bus_engine()
        {
            var sut = CreateSystemUnderTest();

            var handler = Mock.Of<ICommandHandler<TestCommand>>();

            sut.SubscribeToCommand<ICommandHandler<TestCommand>, TestCommand>(handler);

            Assert.That(testBusEngine.IsCommandHandled<TestCommand>());
        }

        [Test]
        public async Task SubscribeToCommand_TCommand_is_invoked_when_message_is_received()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            var context = fixture.Create<CommandContext<TestCommand>>();

            mockContainer.Setup(p => p.Resolve<ICommandHandler<TestCommand>>()).Returns(mockCommandHandler.Object);
            mockCommandContextFactory.Setup(p => p.CreateContext(It.IsAny<CommandMessage<TestCommand>>())).Returns(context);
            mockCommandHandler.Setup(p => p.Handle(It.IsAny<CommandContext<TestCommand>>())).Returns(Task.CompletedTask);

            await testBusEngine.HandleCommand(message);

            mockContainer.Verify(p => p.Resolve<ICommandHandler<TestCommand>>(), Times.Once);
            mockCommandContextFactory.Verify(p => p.CreateContext(message), Times.Once);
            mockCommandHandler.Verify(p => p.Handle(context), Times.Once);
        }

        [Test]
        public async Task SubscribeToCommand_TCommand_TCommandHandler_is_invoked_when_message_is_received()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<ICommandHandler<TestCommand>, TestCommand>();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            var context = fixture.Create<CommandContext<TestCommand>>();

            mockContainer.Setup(p => p.Resolve<ICommandHandler<TestCommand>>()).Returns(mockCommandHandler.Object);
            mockCommandContextFactory.Setup(p => p.CreateContext(It.IsAny<CommandMessage<TestCommand>>())).Returns(context);
            mockCommandHandler.Setup(p => p.Handle(It.IsAny<CommandContext<TestCommand>>())).Returns(Task.CompletedTask);

            await testBusEngine.HandleCommand(message);

            mockContainer.Verify(p => p.Resolve<ICommandHandler<TestCommand>>(), Times.Once);
            mockCommandContextFactory.Verify(p => p.CreateContext(message), Times.Once);
            mockCommandHandler.Verify(p => p.Handle(context), Times.Once);
        }

        [Test]
        public async Task SubscribeToCommand_TCommand_TCommandHandler_instance_is_invoked_when_message_is_received()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<ICommandHandler<TestCommand>, TestCommand>(mockCommandHandler.Object);

            var message = fixture.Create<CommandMessage<TestCommand>>();
            var context = fixture.Create<CommandContext<TestCommand>>();

            mockCommandContextFactory.Setup(p => p.CreateContext(It.IsAny<CommandMessage<TestCommand>>())).Returns(context);
            mockCommandHandler.Setup(p => p.Handle(It.IsAny<CommandContext<TestCommand>>())).Returns(Task.CompletedTask);

            await testBusEngine.HandleCommand(message);

            mockCommandContextFactory.Verify(p => p.CreateContext(message), Times.Once);
            mockCommandHandler.Verify(p => p.Handle(context), Times.Once);
        }

        [Test]
        public async Task SubscribeToCommand_Func_is_invoked_when_message_is_received()
        {
            int invocations = 0;

            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>(async ctx => invocations++);

            var message = fixture.Create<CommandMessage<TestCommand>>();
            var context = fixture.Create<CommandContext<TestCommand>>();

            mockCommandContextFactory.Setup(p => p.CreateContext(It.IsAny<CommandMessage<TestCommand>>())).Returns(context);
            mockCommandHandler.Setup(p => p.Handle(It.IsAny<CommandContext<TestCommand>>())).Returns(Task.CompletedTask);

            await testBusEngine.HandleCommand(message);

            mockCommandContextFactory.Verify(p => p.CreateContext(message), Times.Once);

            Assert.That(invocations, Is.GreaterThan(0));
        }

        #endregion
    }
}