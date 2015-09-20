using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Ploeh.AutoFixture;

namespace Tests
{
    public abstract class ReactiveTest
    {
        public Recorded<Notification<T>> OnNext<T>(long ticks, T value)
        {
            return new Recorded<Notification<T>>(ticks, Notification.CreateOnNext(value));
        }

        public Recorded<Notification<T>> OnError<T>(long ticks, Exception exception)
        {
            return new Recorded<Notification<T>>(ticks, Notification.CreateOnError<T>(exception));
        }

        public Recorded<Notification<T>> OnComplete<T>(long ticks)
        {
            return new Recorded<Notification<T>>(ticks, Notification.CreateOnCompleted<T>());
        }
    }

    [TestFixture]
    public class ReactiveExtensionsTests : ReactiveTest
    {
        private IFixture fixture;
        private TestScheduler scheduler;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            scheduler = new TestScheduler();
        }

        [Test]
        public void Command_should_return_the_command_of_the_incoming_messages()
        {
            var testContext = fixture.Create<CommandContext<TestCommand>>();
            
            var observable = scheduler.CreateHotObservable(OnNext(300, testContext));

            var results = scheduler.Start(() => observable.Command());

            Assert.That(results.Messages, Has.Exactly(1).InstanceOf<Recorded<Notification<TestCommand>>>());
            Assert.That(results.Messages[0].Value.Value, Is.SameAs(testContext.Message));
        }

        [Test]
        public void Event_should_return_the_event_of_the_incoming_messages()
        {
            var testContext = fixture.Create<EventContext<TestEvent>>();

            var observable = scheduler.CreateHotObservable(OnNext(300, testContext));

            var results = scheduler.Start(() => observable.Event());

            Assert.That(results.Messages, Has.Exactly(1).InstanceOf<Recorded<Notification<TestEvent>>>());
            Assert.That(results.Messages[0].Value.Value, Is.SameAs(testContext.Message));
        }

        [Test]
        public void ObserveCommand_should_register_a_delegate_command_handler()
        {
            Mock<IBusBuilder> mockBusBuilder = new Mock<IBusBuilder>();

            var observable = ReactiveExtensions.ObserveCommand<TestCommand>(mockBusBuilder.Object);

            mockBusBuilder.Verify(p => p.SubscribeToCommand(It.IsAny<Func<CommandContext<TestCommand>, Task>>()), Times.Once);
        }

        [Test]
        public void InMemoryBusEngine_IsCommandHandled_is_true_when_ObserveCommand()
        {
            var busEngine = new InMemoryBusEngine();
            var builder = new NybusBusBuilder(busEngine);

            var observable = builder.ObserveCommand<TestCommand>();

            busEngine.IsCommandHandled<TestCommand>();
        }


        [Test]
        public async Task Commands_are_pushed_into_observable()
        {
            var busEngine = new InMemoryBusEngine();
            var builder = new NybusBusBuilder(busEngine);

            var list = new List<TestCommand>();

            var disposable = builder.ObserveCommand<TestCommand>().Select(c => c.Message).Subscribe(list.Add);

            var observable = scheduler.CreateHotObservable(
                OnNext(500, fixture.Create<CommandMessage<TestCommand>>()),
                OnNext(600, fixture.Create<CommandMessage<TestCommand>>()),
                OnNext(1000, fixture.Create<CommandMessage<TestCommand>>()),
                OnNext(1200, fixture.Create<CommandMessage<TestCommand>>()),
                OnNext(1500, fixture.Create<CommandMessage<TestCommand>>())
                );

            var results = scheduler.Start(() => observable.Do(msg => Task.WaitAll(busEngine.HandleCommand(msg))).Select(c => c.Command), 100, 150, 3000);
            
            disposable.Dispose();

            for (int i = 0; i < list.Count; i++)
            {
                Assert.That(list[i], Is.SameAs(results.Messages[i].Value.Value));
            }

        }

        [Test]
        public void ObserveEvent_should_register_a_delegate_event_handler()
        {
            Mock<IBusBuilder> mockBusBuilder = new Mock<IBusBuilder>();

            var observable = ReactiveExtensions.ObserveEvent<TestEvent>(mockBusBuilder.Object);

            mockBusBuilder.Verify(p => p.SubscribeToEvent(It.IsAny<Func<EventContext<TestEvent>, Task>>()), Times.Once);
        }

        [Test]
        public void InMemoryBusEngine_IsEventHandled_is_true_when_ObserveEvent()
        {
            var busEngine = new InMemoryBusEngine();
            var builder = new NybusBusBuilder(busEngine);

            var observable = builder.ObserveEvent<TestEvent>();

            busEngine.IsEventHandeld<TestEvent>();
        }

        [Test]
        public async Task Events_are_pushed_into_observable()
        {
            var busEngine = new InMemoryBusEngine();
            var builder = new NybusBusBuilder(busEngine);

            var list = new List<TestEvent>();

            var disposable = builder.ObserveEvent<TestEvent>().Select(c => c.Message).Subscribe(list.Add);

            var observable = scheduler.CreateHotObservable(
                OnNext(500, fixture.Create<EventMessage<TestEvent>>()),
                OnNext(600, fixture.Create<EventMessage<TestEvent>>()),
                OnNext(1000, fixture.Create<EventMessage<TestEvent>>()),
                OnNext(1200, fixture.Create<EventMessage<TestEvent>>()),
                OnNext(1500, fixture.Create<EventMessage<TestEvent>>())
                );

            var results = scheduler.Start(() => observable.Do(msg => Task.WaitAll(busEngine.HandleEvent(msg))).Select(c => c.Event), 100, 150, 3000);

            disposable.Dispose();

            for (int i = 0; i < list.Count; i++)
            {
                Assert.That(list[i], Is.SameAs(results.Messages[i].Value.Value));
            }

        }

    }
}
