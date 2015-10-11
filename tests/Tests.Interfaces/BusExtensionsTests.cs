using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
using Ploeh.AutoFixture;
// ReSharper disable InvokeAsExtensionMethod

namespace Tests
{
    [TestFixture]
    public class BusExtensionsTests
    {
        private IFixture fixture;
        private Mock<IBus> mockBus;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            mockBus = new Mock<IBus>();
        }

        #region InvokeManyCommands

        [Test]
        public async Task InvokeManyCommands_forwards_to_IBus()
        {
            var commands = fixture.CreateMany<TestCommand>().ToArray();

            await BusExtensions.InvokeManyCommands(mockBus.Object, commands);

            mockBus.Verify(p => p.InvokeCommand(It.IsAny<TestCommand>()), Times.Exactly(commands.Length));
        }

        [Test, ExpectedException]
        public async Task InvokeManyCommands_Bus_is_required()
        {
            var commands = fixture.CreateMany<TestCommand>().ToArray();

            await BusExtensions.InvokeManyCommands(null, commands);
        }

        [Test]
        public async Task InvokeManyCommands_no_exception_if_commands_is_null()
        {
            IEnumerable<TestCommand> commands = null;

            await BusExtensions.InvokeManyCommands(mockBus.Object, commands);

            mockBus.Verify(p => p.InvokeCommand(It.IsAny<TestCommand>()), Times.Never);

        }

        #endregion

        #region InvokeManyCommands with CorrelationId

        [Test]
        public async Task InvokeManyCommands_CorrelationId_forwards_to_IBus()
        {
            var commands = fixture.CreateMany<TestCommand>().ToArray();

            var correlationId = fixture.Create<Guid>();

            await BusExtensions.InvokeManyCommands(mockBus.Object, commands, correlationId);

            mockBus.Verify(p => p.InvokeCommand(It.IsAny<TestCommand>(), correlationId), Times.Exactly(commands.Length));
        }

        [Test, ExpectedException]
        public async Task InvokeManyCommands_CorrelationId_Bus_is_required()
        {
            var commands = fixture.CreateMany<TestCommand>().ToArray();

            var correlationId = fixture.Create<Guid>();

            await BusExtensions.InvokeManyCommands(null, commands, correlationId);
        }

        [Test]
        public async Task InvokeManyCommands_CorrelationId_no_exception_if_commands_is_null()
        {
            IEnumerable<TestCommand> commands = null;

            var correlationId = fixture.Create<Guid>();

            await BusExtensions.InvokeManyCommands(mockBus.Object, commands, correlationId);

            mockBus.Verify(p => p.InvokeCommand(It.IsAny<TestCommand>()), Times.Never);

        }

        #endregion

        #region RaiseManyEvents

        [Test]
        public async Task RaiseManyEvents_forwards_to_IBus()
        {
            var events = fixture.CreateMany<TestEvent>().ToArray();

            await BusExtensions.RaiseManyEvents(mockBus.Object, events);

            mockBus.Verify(p => p.RaiseEvent(It.IsAny<TestEvent>()), Times.Exactly(events.Length));
        }

        [Test, ExpectedException]
        public async Task RaiseManyEvents_Bus_is_required()
        {
            var events = fixture.CreateMany<TestEvent>().ToArray();

            await BusExtensions.RaiseManyEvents(null, events);
        }

        [Test]
        public async Task RaiseManyEvents_no_exception_if_commands_is_null()
        {
            IEnumerable<TestEvent> events = null;

            await BusExtensions.RaiseManyEvents(mockBus.Object, events);

            mockBus.Verify(p => p.RaiseEvent(It.IsAny<TestEvent>()), Times.Never);

        }

        #endregion


        #region RaiseManyEvents with CorrelationId

        [Test]
        public async Task RaiseManyEvents_CorrelationId_forwards_to_IBus()
        {
            var events = fixture.CreateMany<TestEvent>().ToArray();

            var correlationId = fixture.Create<Guid>();

            await BusExtensions.RaiseManyEvents(mockBus.Object, events, correlationId);

            mockBus.Verify(p => p.RaiseEvent(It.IsAny<TestEvent>(), correlationId), Times.Exactly(events.Length));
        }

        [Test, ExpectedException]
        public async Task RaiseManyEvents_CorrelationId_Bus_is_required()
        {
            var events = fixture.CreateMany<TestEvent>().ToArray();

            var correlationId = fixture.Create<Guid>();

            await BusExtensions.RaiseManyEvents(null, events, correlationId);
        }

        [Test]
        public async Task RaiseManyEvents_CorrelationId_no_exception_if_commands_is_null()
        {
            IEnumerable<TestEvent> events = null;

            var correlationId = fixture.Create<Guid>();

            await BusExtensions.RaiseManyEvents(mockBus.Object, events, correlationId);

            mockBus.Verify(p => p.RaiseEvent(It.IsAny<TestEvent>(), It.IsAny<Guid>()), Times.Never);

        }

        #endregion

        public class TestCommand : ICommand
        {
            public string StringValue { get; set; }
            public int IntValue { get; set; }
            public bool BoolValue { get; set; }

            public DateTimeOffset DateTimeOffsetValue { get; set; }
        }

        public class TestEvent : IEvent
        {
            public string StringValue { get; set; }
            public int IntValue { get; set; }
            public bool BoolValue { get; set; }

            public DateTimeOffset DateTimeOffsetValue { get; set; }
        }


    }
}