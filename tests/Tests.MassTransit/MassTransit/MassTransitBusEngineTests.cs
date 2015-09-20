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
        private Mock<IContextManager> mockContextManager;

        private string commandQueueName;
        private string eventQueueName;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            commandQueueName = fixture.Create<string>();
            eventQueueName = fixture.Create<string>();

            mockLogger = new Mock<ILogger>();

            mockContextManager = new Mock<IContextManager>();

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
                ServiceBusFactory = mockServiceBusFactory.Object,
                ContextManager = mockContextManager.Object
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
        public async Task SendCommand_throws_if_engine_isnt_started()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            await sut.SendCommand(message);
        }

        [Test]
        public async Task Command_Message_is_sent_by_Event_bus_when_no_command_is_registered()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            mockEventServiceBus
                .Setup(p => p.Publish(It.IsAny<TestCommand>(), It.IsAny<Action<IPublishContext<TestCommand>>>()))
                .Callback((TestCommand cmd, Action<IPublishContext<TestCommand>> pc) => mockContextManager.Object.SetCommandMessageHeaders(message, It.IsAny<IPublishContext<TestCommand>>()));

            await sut.SendCommand(message);

            mockEventServiceBus.Verify(p => p.Publish(message.Command, It.IsAny<Action<IPublishContext<TestCommand>>>()), Times.Once);
            mockCommandServiceBus.Verify(p => p.Publish(message.Command, It.IsAny<Action<IPublishContext<TestCommand>>>()), Times.Never);
            mockContextManager.Verify(p => p.SetCommandMessageHeaders(message, It.IsAny<IPublishContext<TestCommand>>()), Times.Once);
        }

        [Test]
        public async Task Command_Message_is_sent_by_Command_bus_when_a_command_is_registered()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>(msg => Task.CompletedTask);

            await sut.Start();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            mockCommandServiceBus
                .Setup(p => p.Publish(It.IsAny<TestCommand>(), It.IsAny<Action<IPublishContext<TestCommand>>>()))
                .Callback((TestCommand cmd, Action<IPublishContext<TestCommand>> pc) => mockContextManager.Object.SetCommandMessageHeaders(message, It.IsAny<IPublishContext<TestCommand>>()));

            await sut.SendCommand(message);

            mockEventServiceBus.Verify(p => p.Publish(message.Command, It.IsAny<Action<IPublishContext<TestCommand>>>()), Times.Never);
            mockCommandServiceBus.Verify(p => p.Publish(message.Command, It.IsAny<Action<IPublishContext<TestCommand>>>()), Times.Once);
            mockContextManager.Verify(p => p.SetCommandMessageHeaders(message, It.IsAny<IPublishContext<TestCommand>>()), Times.Once);
        }
        
        [Test]
        [ExpectedException]
        public async Task SendEvent_throws_if_engine_isnt_started()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<EventMessage<TestEvent>>();

            await sut.SendEvent(message);
        }

        [Test]
        public async Task Event_Message_is_sent_by_Event_bus()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            var message = fixture.Create<EventMessage<TestEvent>>();

            mockEventServiceBus
                .Setup(p => p.Publish(It.IsAny<TestEvent>(), It.IsAny<Action<IPublishContext<TestEvent>>>()))
                .Callback((TestEvent body, Action<IPublishContext<TestEvent>> pc) => mockContextManager.Object.SetEventMessageHeaders(message, It.IsAny<IPublishContext<TestEvent>>()));

            await sut.SendEvent(message);

            mockEventServiceBus.Verify(p => p.Publish(message.Event, It.IsAny<Action<IPublishContext<TestEvent>>>()), Times.Once);
            mockCommandServiceBus.Verify(p => p.Publish(message.Event, It.IsAny<Action<IPublishContext<TestEvent>>>()), Times.Never);
            mockContextManager.Verify(p => p.SetEventMessageHeaders(message, It.IsAny<IPublishContext<TestEvent>>()), Times.Once);
        }

        [Test]
        public async Task Event_Message_is_sent_by_Event_bus_when_a_command_is_registered()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>(msg => Task.CompletedTask);

            await sut.Start();

            var message = fixture.Create<EventMessage<TestEvent>>();

            mockEventServiceBus
                .Setup(p => p.Publish(It.IsAny<TestEvent>(), It.IsAny<Action<IPublishContext<TestEvent>>>()))
                .Callback((TestEvent body, Action<IPublishContext<TestEvent>> pc) => mockContextManager.Object.SetEventMessageHeaders(message, It.IsAny<IPublishContext<TestEvent>>()));

            await sut.SendEvent(message);

            mockEventServiceBus.Verify(p => p.Publish(message.Event, It.IsAny<Action<IPublishContext<TestEvent>>>()), Times.Once);
            mockCommandServiceBus.Verify(p => p.Publish(message.Event, It.IsAny<Action<IPublishContext<TestEvent>>>()), Times.Never);
            mockContextManager.Verify(p => p.SetEventMessageHeaders(message, It.IsAny<IPublishContext<TestEvent>>()), Times.Once);
        }

        [Test]
        [ExpectedException]
        public void SubscribeToEvent_cant_receive_a_null_handler()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToEvent<TestEvent>(null);
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

            mockContextManager.Setup(p => p.CreateEventMessage(It.IsAny<IConsumeContext<TestEvent>>())).Returns(message);

            await sut.Start();

            await sut.SendEvent(message);

            are.WaitOne(TimeSpan.FromSeconds(3));

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

            mockEventErrorStrategy.Setup(p => p.HandleError(It.IsAny<IConsumeContext<TestEvent>>(), It.IsAny<Exception>())).ReturnsAsync(true);

            sut.SubscribeToEvent<TestEvent>(msg =>
            {
                try
                {
                    throw exception;
                }
                finally
                {
                    are.Set();
                }
            });

            var message = fixture.Create<EventMessage<TestEvent>>();

            mockContextManager.Setup(p => p.CreateEventMessage(It.IsAny<IConsumeContext<TestEvent>>())).Returns(message);

            await sut.Start();

            await sut.SendEvent(message);

            are.WaitOne(TimeSpan.FromSeconds(3));

            await Task.Delay(TimeSpan.FromSeconds(2));

            mockEventErrorStrategy.Verify(p => p.HandleError(It.IsAny<IConsumeContext<TestEvent>>(), exception), Times.Once);
        }

        [Test]
        [ExpectedException]
        public async Task TEvent_throws_when_ErrorStrategy_returns_false()
        {
            AutoResetEvent are = new AutoResetEvent(false);

            options.ServiceBusFactory = new LoopbackServiceBusFactory();

            var sut = CreateSystemUnderTest();

            var exception = Mock.Of<Exception>();

            mockEventErrorStrategy.Setup(p => p.HandleError(It.IsAny<IConsumeContext<TestEvent>>(), It.IsAny<Exception>())).ReturnsAsync(false);

            sut.SubscribeToEvent<TestEvent>(msg =>
            {
                try
                {
                    throw exception;
                }
                finally
                {
                    are.Set();
                }
            });

            var message = fixture.Create<EventMessage<TestEvent>>();

            mockContextManager.Setup(p => p.CreateEventMessage(It.IsAny<IConsumeContext<TestEvent>>())).Returns(message);

            await sut.Start();

            await sut.SendEvent(message);

            are.WaitOne(TimeSpan.FromSeconds(3));

            await Task.Delay(TimeSpan.FromSeconds(2));

            mockEventErrorStrategy.Verify(p => p.HandleError(It.IsAny<IConsumeContext<TestEvent>>(), exception), Times.Once);
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

            mockContextManager.Setup(p => p.CreateCommandMessage(It.IsAny<IConsumeContext<TestCommand>>())).Returns(message);

            await sut.Start();

            await sut.SendCommand(message);

            are.WaitOne(TimeSpan.FromSeconds(3));

            Assert.That(receivedMessage, Is.Not.Null);
            Assert.That(message.CorrelationId, Is.EqualTo(receivedMessage.CorrelationId));
            Assert.That(receivedMessage.Command, Is.Not.Null);
            Assert.That(receivedMessage.Command.Flag, Is.EqualTo(message.Command.Flag));
            Assert.That(receivedMessage.Command.Text, Is.EqualTo(message.Command.Text));
            Assert.That(receivedMessage.Command.Time, Is.EqualTo(message.Command.Time));
            Assert.That(receivedMessage.Command.Value, Is.EqualTo(message.Command.Value));
        }

        [Test]
        [ExpectedException]
        public void SubscribeToCommand_cant_receive_a_null_handler()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>(null);
        }

        [Test]
        public async Task TCommand_ErroryStrategy_is_invoked_when_handler_throws()
        {
            AutoResetEvent are = new AutoResetEvent(false);

            options.ServiceBusFactory = new LoopbackServiceBusFactory();

            var sut = CreateSystemUnderTest();

            var exception = Mock.Of<Exception>();

            mockCommandErrorStrategy.Setup(p => p.HandleError(It.IsAny<IConsumeContext<TestCommand>>(), It.IsAny<Exception>())).ReturnsAsync(true);

            sut.SubscribeToCommand<TestCommand>(msg =>
            {
                try
                {
                    throw exception;
                }
                finally
                {
                    are.Set();
                }
            });

            var message = fixture.Create<CommandMessage<TestCommand>>();

            mockContextManager.Setup(p => p.CreateCommandMessage(It.IsAny<IConsumeContext<TestCommand>>())).Returns(message);

            await sut.Start();

            await sut.SendCommand(message);
            
            are.WaitOne(TimeSpan.FromSeconds(3));

            await Task.Delay(TimeSpan.FromSeconds(2));

            mockCommandErrorStrategy.Verify(p => p.HandleError(It.IsAny<IConsumeContext<TestCommand>>(), exception), Times.Once);
        }

        [Test]
        [ExpectedException]
        public async Task TCommand_throws_when_ErrorStrategy_returns_false()
        {
            AutoResetEvent are = new AutoResetEvent(false);

            options.ServiceBusFactory = new LoopbackServiceBusFactory();

            var sut = CreateSystemUnderTest();

            var exception = Mock.Of<Exception>();

            mockCommandErrorStrategy.Setup(p => p.HandleError(It.IsAny<IConsumeContext<TestCommand>>(), It.IsAny<Exception>())).ReturnsAsync(false);

            sut.SubscribeToCommand<TestCommand>(msg =>
            {
                try
                {
                    throw exception;
                }
                finally
                {
                    are.Set();
                }
            });

            var message = fixture.Create<CommandMessage<TestCommand>>();

            mockContextManager.Setup(p => p.CreateCommandMessage(It.IsAny<IConsumeContext<TestCommand>>())).Returns(message);

            await sut.Start();

            await sut.SendCommand(message);

            are.WaitOne(TimeSpan.FromSeconds(3));

            await Task.Delay(TimeSpan.FromSeconds(2));

            mockCommandErrorStrategy.Verify(p => p.HandleError(It.IsAny<IConsumeContext<TestCommand>>(), exception), Times.Once);
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
        public async Task CommandServiceBus_is_EventServiceBus_if_no_command_is_subscribed()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            Assert.That(sut.CommandServiceBus, Is.SameAs(sut.EventServiceBus));
        }

        [Test]
        public async Task CommandServiceBus_is_not_EventServiceBus_if_least_one_command_is_subscribed()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>(msg => Task.CompletedTask);

            await sut.Start();

            Assert.That(sut.CommandServiceBus, Is.Not.SameAs(sut.EventServiceBus));
        }

    }
}