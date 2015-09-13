using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.SubscriptionConfigurators;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Nybus.Logging;
using Nybus.MassTransit;
using Ploeh.AutoFixture;

namespace Tests.MassTransit
{
    [TestFixture]
    public class MassTransitBusEngineTests
    {
        private IFixture fixture;

        private MassTransitOptions options;
        private MassTransitConnectionDescriptor connectionDescriptor;

        private Mock<ILogger> mockLogger;
        private Mock<IErrorStrategy> mockCommandErrorStrategy;
        private Mock<IQueueStrategy> mockCommandQueueStrategy;
        private Mock<IServiceBus> mockCommandServiceBus;
        private Mock<IErrorStrategy> mockEventErrorStrategy;
        private Mock<IQueueStrategy> mockEventQueueStrategy;
        private Mock<IServiceBus> mockEventServiceBus;
        private Mock<IServiceBusFactory> mockServiceBusFactory;

        private string commandQueueName;
        private string eventQueueName;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            commandQueueName = fixture.Create<string>();
            eventQueueName = fixture.Create<string>();

            mockLogger = new Mock<ILogger>();

            mockCommandErrorStrategy = new Mock<IErrorStrategy>();
            mockCommandQueueStrategy = new Mock<IQueueStrategy>();
            mockCommandServiceBus = new Mock<IServiceBus>();

            mockEventErrorStrategy = new Mock<IErrorStrategy>();
            mockEventQueueStrategy = new Mock<IQueueStrategy>();
            mockEventServiceBus = new Mock<IServiceBus>();
            
            mockServiceBusFactory = new Mock<IServiceBusFactory>();
            mockServiceBusFactory.Setup(p => p.CreateServiceBus(It.IsAny<MassTransitConnectionDescriptor>(), mockCommandQueueStrategy.Object, It.IsAny<IReadOnlyList<Action<SubscriptionBusServiceConfigurator>>>())).Returns(mockCommandServiceBus.Object);
            mockServiceBusFactory.Setup(p => p.CreateServiceBus(It.IsAny<MassTransitConnectionDescriptor>(), mockEventQueueStrategy.Object, It.IsAny<IReadOnlyList<Action<SubscriptionBusServiceConfigurator>>>())).Returns(mockEventServiceBus.Object);

            mockCommandQueueStrategy.Setup(p => p.GetQueueName()).Returns(commandQueueName);
            mockEventQueueStrategy.Setup(p => p.GetQueueName()).Returns(eventQueueName);

            options = new MassTransitOptions
            {
                Logger = mockLogger.Object,
                CommandErrorStrategy = mockCommandErrorStrategy.Object,
                CommandQueueStrategy = mockCommandQueueStrategy.Object,
                EventErrorStrategy = mockEventErrorStrategy.Object,
                EventQueueStrategy = mockEventQueueStrategy.Object,
                ServiceBusFactory = mockServiceBusFactory.Object
            };

            var host = fixture.Create<Uri>();
            var userName = fixture.Create<string>();
            var password = fixture.Create<string>();

            connectionDescriptor = new MassTransitConnectionDescriptor(host, userName, password);
        }

        [Test]
        [ExpectedException]
        public void ConnectionDescriptor_is_required()
        {
            new MassTransitBusEngine(null, options);
        }

        [Test]
        [ExpectedException]
        public void Options_cant_be_null()
        {
            new MassTransitBusEngine(connectionDescriptor, null);
        }

        [Test]
        public void Options_is_optional()
        {
            new MassTransitBusEngine(connectionDescriptor);
        }

        [Test]
        [ExpectedException]
        public void ConnectionDescriptor_is_required_with_default_options()
        {
            new MassTransitBusEngine(null);
        }

        private MassTransitBusEngine CreateSystemUnderTest()
        {
            return new MassTransitBusEngine(connectionDescriptor, options);
        }

        [Test]
        public async Task A_bus_is_created_when_starting_an_empty_engine()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            mockServiceBusFactory.Verify(p => p.CreateServiceBus(connectionDescriptor, mockEventQueueStrategy.Object, It.IsAny<IReadOnlyList<Action<SubscriptionBusServiceConfigurator>>>()), Times.Once);

            mockServiceBusFactory.Verify(p => p.CreateServiceBus(connectionDescriptor, mockCommandQueueStrategy.Object, It.IsAny<IReadOnlyList<Action<SubscriptionBusServiceConfigurator>>>()), Times.Never);
        }

        [Test]
        public async Task No_extra_bus_is_created_when_starting_only_events_are_subscribed()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToEvent<TestEvent>(message => Task.CompletedTask);

            await sut.Start();

            mockServiceBusFactory.Verify(p => p.CreateServiceBus(connectionDescriptor, mockEventQueueStrategy.Object, It.IsAny<IReadOnlyList<Action<SubscriptionBusServiceConfigurator>>>()), Times.Once);

            mockServiceBusFactory.Verify(p => p.CreateServiceBus(connectionDescriptor, mockCommandQueueStrategy.Object, It.IsAny<IReadOnlyList<Action<SubscriptionBusServiceConfigurator>>>()), Times.Never);
        }


        [Test]
        public async Task A_bus_for_command_is_created_when_a_command_is_subscribed()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>(message => Task.CompletedTask);

            await sut.Start();

            mockServiceBusFactory.Verify(p => p.CreateServiceBus(connectionDescriptor, mockEventQueueStrategy.Object, It.IsAny<IReadOnlyList<Action<SubscriptionBusServiceConfigurator>>>()), Times.Once);

            mockServiceBusFactory.Verify(p => p.CreateServiceBus(connectionDescriptor, mockCommandQueueStrategy.Object, It.IsAny<IReadOnlyList<Action<SubscriptionBusServiceConfigurator>>>()), Times.Once);
        }

        [Test, ExpectedException]
        public async Task An_exception_is_thrown_when_engine_cant_start()
        {
            var sut = CreateSystemUnderTest();

            mockServiceBusFactory.Setup(p => p.CreateServiceBus(It.IsAny<MassTransitConnectionDescriptor>(), mockEventQueueStrategy.Object, It.IsAny<IReadOnlyList<Action<SubscriptionBusServiceConfigurator>>>())).Throws(Mock.Of<Exception>());

            await sut.Start();
        }

        [Test]
        public async Task Stop_disposes_all_started_bus()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>(message => Task.CompletedTask);

            sut.SubscribeToEvent<TestEvent>(message => Task.CompletedTask);

            await sut.Start();

            await sut.Stop();

            mockCommandServiceBus.Verify(p => p.Dispose(), Times.Once);
            mockEventServiceBus.Verify(p => p.Dispose(), Times.Once);
        }

        [Test]
        [ExpectedException]
        public async Task SendMessage_TCommand_throws_if_engine_isnt_started()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            await sut.SendMessage(message);
        }

        [Test]
        public async Task TCommand_Message_is_sent_by_Event_bus()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            await sut.SendMessage(message);

            mockEventServiceBus.Verify(p => p.Publish(message), Times.Once);
            mockCommandServiceBus.Verify(p => p.Publish(message), Times.Never);
        }

        [Test]
        [ExpectedException]
        public async Task SendMessage_TEvent_throws_if_engine_isnt_started()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<EventMessage<TestEvent>>();

            await sut.SendMessage(message);
        }

        [Test]
        public async Task TEvent_Message_is_sent_by_Event_bus()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            var message = fixture.Create<EventMessage<TestEvent>>();

            await sut.SendMessage(message);

            mockEventServiceBus.Verify(p => p.Publish(message), Times.Once);
            mockCommandServiceBus.Verify(p => p.Publish(message), Times.Never);
        }

        [Test]
        public async Task TEvent_subscribed_delegate_is_invoked_when_message_is_received()
        {
            AutoResetEvent are = new AutoResetEvent(false);

            options.ServiceBusFactory = new LoopbackServiceBusFactory();

            var sut = CreateSystemUnderTest();

            EventMessage<TestEvent> receivedMessage = null;

            sut.SubscribeToEvent<TestEvent>(msg =>
            {
                receivedMessage = msg;
                are.Set();
                return Task.CompletedTask;
            });

            var message = fixture.Create<EventMessage<TestEvent>>();

            await sut.Start();

            sut.EventServiceBus.Publish(message);

            are.WaitOne(TimeSpan.FromSeconds(2));

            Assert.That(receivedMessage, Is.Not.Null);
            Assert.That(message.CorrelationId, Is.EqualTo(receivedMessage.CorrelationId));
            Assert.That(receivedMessage.Event, Is.Not.Null);
            Assert.That(receivedMessage.Event.Flag, Is.EqualTo(message.Event.Flag));
            Assert.That(receivedMessage.Event.Text, Is.EqualTo(message.Event.Text));
            Assert.That(receivedMessage.Event.Time, Is.EqualTo(message.Event.Time));
            Assert.That(receivedMessage.Event.Value, Is.EqualTo(message.Event.Value));
        }

        [Test]
        public async Task TEvent_ErroryStrategy_is_invoked_when_handler_throws()
        {
            AutoResetEvent are = new AutoResetEvent(false);

            options.ServiceBusFactory = new LoopbackServiceBusFactory();

            var sut = CreateSystemUnderTest();
            
            var exception = Mock.Of<Exception>();

            sut.SubscribeToEvent<TestEvent>(msg =>
            {
                are.Set();
                throw exception;
            });

            var message = fixture.Create<EventMessage<TestEvent>>();

            await sut.Start();

            sut.EventServiceBus.Publish(message);

            are.WaitOne(TimeSpan.FromSeconds(2));

            mockEventErrorStrategy.Verify(p => p.HandleError(It.IsAny<IConsumeContext<EventMessage<TestEvent>>>(), exception), Times.AtLeastOnce);
        }


        [Test]
        public async Task TCommand_subscribed_delegate_is_invoked_when_message_is_received()
        {
            AutoResetEvent are = new AutoResetEvent(false);

            options.ServiceBusFactory = new LoopbackServiceBusFactory();

            var sut = CreateSystemUnderTest();

            CommandMessage<TestCommand> receivedMessage = null;

            sut.SubscribeToCommand<TestCommand>(msg =>
            {
                receivedMessage = msg;
                are.Set();
                return Task.CompletedTask;
            });

            var message = fixture.Create<CommandMessage<TestCommand>>();

            await sut.Start();

            sut.CommandServiceBus.Publish(message);

            are.WaitOne(TimeSpan.FromSeconds(2));

            Assert.That(receivedMessage, Is.Not.Null);
            Assert.That(message.CorrelationId, Is.EqualTo(receivedMessage.CorrelationId));
            Assert.That(receivedMessage.Command, Is.Not.Null);
            Assert.That(receivedMessage.Command.Flag, Is.EqualTo(message.Command.Flag));
            Assert.That(receivedMessage.Command.Text, Is.EqualTo(message.Command.Text));
            Assert.That(receivedMessage.Command.Time, Is.EqualTo(message.Command.Time));
            Assert.That(receivedMessage.Command.Value, Is.EqualTo(message.Command.Value));
        }

        [Test]
        public async Task TCommand_ErroryStrategy_is_invoked_when_handler_throws()
        {
            AutoResetEvent are = new AutoResetEvent(false);

            options.ServiceBusFactory = new LoopbackServiceBusFactory();

            var sut = CreateSystemUnderTest();

            var exception = Mock.Of<Exception>();

            sut.SubscribeToCommand<TestCommand>(msg =>
            {
                are.Set();
                throw exception;
            });

            var message = fixture.Create<CommandMessage<TestCommand>>();

            await sut.Start();

            sut.CommandServiceBus.Publish(message);

            are.WaitOne(TimeSpan.FromSeconds(2));

            mockCommandErrorStrategy.Verify(p => p.HandleError(It.IsAny<IConsumeContext<CommandMessage<TestCommand>>>(), exception), Times.AtLeastOnce);
        }


        [Test]
        [ExpectedException]
        public void EventServiceBus_throws_if_engine_isnt_started()
        {
            var sut = CreateSystemUnderTest();

            var bus = sut.EventServiceBus;
        }

        [Test]
        public async Task EventServiceBus_is_not_null_when_service_is_started()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            Assert.That(sut.EventServiceBus, Is.Not.Null);
        }

        [Test]
        [ExpectedException]
        public void CommandServiceBus_throws_if_engine_isnt_started()
        {
            var sut = CreateSystemUnderTest();

            var bus = sut.CommandServiceBus;
        }

        [Test]
        public async Task CommandServiceBus_is_null_if_no_command_is_subscribed()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            Assert.That(sut.CommandServiceBus, Is.Null);
        }

        [Test]
        public async Task CommandServiceBus_is_not_null_if_least_one_command_is_subscribed()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>(msg => Task.CompletedTask);

            await sut.Start();

            Assert.That(sut.CommandServiceBus, Is.Not.Null);
        }

    }
}