using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class InMemoryBusEngineTests
    {
        private IFixture fixture;
        private int commandInvocations = 0;
        private int eventInvocations = 0;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            commandInvocations = 0;
            eventInvocations = 0;
        }

        private InMemoryBusEngine CreateSystemUnderTest()
        {
            return new InMemoryBusEngine();
        }

        [Test]
        public async Task Start_sets_status_to_Started()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            Assert.That(sut.Status, Is.EqualTo(InMemoryBusEngine.EngineStatus.Started));
        }

        [Test]
        public async Task Stopping_a_started_engine_sets_status_to_Stop()
        {
            var sut = CreateSystemUnderTest();

            await sut.Start();

            Assert.That(sut.Status, Is.EqualTo(InMemoryBusEngine.EngineStatus.Started));

            await sut.Stop();

            Assert.That(sut.Status, Is.EqualTo(InMemoryBusEngine.EngineStatus.Stopped));
        }

        [Test]
        public async Task Sending_a_message_adds_it_to_SentMessages_list()
        {
            var sut = CreateSystemUnderTest();

            var message = Mock.Of<Message>();

            await sut.SendMessage(message);

            Assert.That(sut.SentMessages, Contains.Item(message));
        }

        [Test]
        public void Subscribing_a_command_makes_it_handled()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToCommand<TestCommand>(message => Task.CompletedTask);

            Assert.That(sut.IsCommandHandled<TestCommand>());
        }

        [Test]
        public void Subscribing_an_event_makes_it_handled()
        {
            var sut = CreateSystemUnderTest();

            sut.SubscribeToEvent<TestEvent>(message => Task.CompletedTask);

            Assert.That(sut.IsEventHandeld<TestEvent>());
        }

        [Test]
        public async Task HandleEvent_executes_delegate_if_event_is_registered()
        {
            var sut = CreateSystemUnderTest();
            sut.SubscribeToEvent<TestEvent>(HandleEventMessageDelegate);

            var message = fixture.Create<EventMessage<TestEvent>>();

            await sut.HandleEvent(message);

            Assert.That(eventInvocations, Is.EqualTo(1));
        }

        [Test]
        public async Task HandleEvent_discards_invocation_if_no_registration()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<EventMessage<TestEvent>>();

            await sut.HandleEvent(message);

            Assert.That(eventInvocations, Is.EqualTo(0));
        }

        [Test]
        public async Task HandleCommand_executes_delegate_if_event_is_registered()
        {
            var sut = CreateSystemUnderTest();
            sut.SubscribeToCommand<TestCommand>(HandleCommandMessageDelegate);

            var message = fixture.Create<CommandMessage<TestCommand>>();

            await sut.HandleCommand(message);

            Assert.That(commandInvocations, Is.EqualTo(1));
        }

        [Test]
        public async Task HandleCommand_discards_invocation_if_no_registration()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<CommandMessage<TestCommand>>();

            await sut.HandleCommand(message);

            Assert.That(commandInvocations, Is.EqualTo(0));
        }

        private Task HandleEventMessageDelegate(EventMessage<TestEvent> message)
        {
            eventInvocations++;
            return Task.CompletedTask;
        }

        private Task HandleCommandMessageDelegate(CommandMessage<TestCommand> message)
        {
            commandInvocations++;
            return Task.CompletedTask;
        }

    }
}