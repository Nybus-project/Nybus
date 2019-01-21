using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
// ReSharper disable InvokeAsExtensionMethod

namespace Tests
{
    [TestFixture]
    public class BusExtensionsTests
    {
        [Test, CustomAutoMoqData]
        public async Task InvokeCommandAsync_forwards_to_bus(IBus bus, FirstTestCommand testCommand)
        {
            await BusExtensions.InvokeCommandAsync(bus, testCommand);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(testCommand, It.IsAny<Guid>(), It.IsAny<IDictionary<string, string>>()));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeCommandAsync_forwards_to_bus(IBus bus, FirstTestCommand testCommand, Guid correlationId)
        {
            await BusExtensions.InvokeCommandAsync(bus, testCommand, correlationId);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(testCommand, correlationId, It.IsAny<IDictionary<string, string>>()));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeCommandAsync_forwards_to_bus(IBus bus, FirstTestCommand testCommand, IDictionary<string, string> headers)
        {
            await BusExtensions.InvokeCommandAsync(bus, testCommand, headers);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(testCommand, It.IsAny<Guid>(), headers));
        }

        [Test, CustomAutoMoqData]
        public void InvokeCommandAsync_requires_bus(FirstTestCommand testCommand)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.InvokeCommandAsync(null, testCommand));
        }

        [Test, CustomAutoMoqData]
        public void InvokeCommandAsync_requires_bus(FirstTestCommand testCommand, Guid correlationId)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.InvokeCommandAsync(null, testCommand, correlationId));
        }

        [Test, CustomAutoMoqData]
        public void InvokeCommandAsync_requires_bus(FirstTestCommand testCommand, IDictionary<string, string> headers)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.InvokeCommandAsync(null, testCommand, headers));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseEventAsync_forwards_to_bus(IBus bus, FirstTestEvent testEvent)
        {
            await BusExtensions.RaiseEventAsync(bus, testEvent);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(testEvent, It.IsAny<Guid>(), It.IsAny<IDictionary<string, string>>()));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseEventAsync_forwards_to_bus(IBus bus, FirstTestEvent testEvent, Guid correlationId)
        {
            await BusExtensions.RaiseEventAsync(bus, testEvent, correlationId);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(testEvent, correlationId, It.IsAny<IDictionary<string, string>>()));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseEventAsync_forwards_to_bus(IBus bus, FirstTestEvent testEvent, IDictionary<string, string> headers)
        {
            await BusExtensions.RaiseEventAsync(bus, testEvent, headers);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(testEvent, It.IsAny<Guid>(), headers));
        }

        [Test, CustomAutoMoqData]
        public void RaiseEventAsync_requires_bus(FirstTestEvent testEvent)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.RaiseEventAsync(null, testEvent));
        }

        [Test, CustomAutoMoqData]
        public void RaiseEventAsync_requires_bus(FirstTestEvent testEvent, Guid correlationId)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.RaiseEventAsync(null, testEvent, correlationId));
        }

        [Test, CustomAutoMoqData]
        public void RaiseEventAsync_requires_bus(FirstTestEvent testEvent, IDictionary<string, string> headers)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.RaiseEventAsync(null, testEvent, headers));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_forwards_all_to_bus(IBus bus, FirstTestCommand[] testCommands)
        {
            await BusExtensions.InvokeManyCommandsAsync(bus, testCommands);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), It.IsAny<Guid>(), It.IsAny<IDictionary<string, string>>()), Times.Exactly(testCommands.Length));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_forwards_all_to_bus(IBus bus, FirstTestCommand[] testCommands, Guid correlationId)
        {
            await BusExtensions.InvokeManyCommandsAsync(bus, testCommands, correlationId);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), correlationId, It.IsAny<IDictionary<string, string>>()), Times.Exactly(testCommands.Length));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_forwards_all_to_bus(IBus bus, FirstTestCommand[] testCommands, IDictionary<string, string> headers)
        {
            await BusExtensions.InvokeManyCommandsAsync(bus, testCommands, headers);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), It.IsAny<Guid>(), headers), Times.Exactly(testCommands.Length));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_forwards_all_to_bus(IBus bus, FirstTestCommand[] testCommands, Guid correlationId, IDictionary<string, string> headers)
        {
            await BusExtensions.InvokeManyCommandsAsync(bus, testCommands, correlationId, headers);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), correlationId, headers), Times.Exactly(testCommands.Length));
        }

        [Test, CustomAutoMoqData]
        public void InvokeManyCommandsAsync_requires_bus(FirstTestCommand[] testCommands)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.InvokeManyCommandsAsync(null, testCommands));
        }

        [Test, CustomAutoMoqData]
        public void InvokeManyCommandsAsync_requires_bus(FirstTestCommand[] testCommands, Guid correlationId)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.InvokeManyCommandsAsync(null, testCommands, correlationId));
        }

        [Test, CustomAutoMoqData]
        public void InvokeManyCommandsAsync_requires_bus(FirstTestCommand[] testCommands, IDictionary<string, string> headers)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.InvokeManyCommandsAsync(null, testCommands, headers));
        }

        [Test, CustomAutoMoqData]
        public void InvokeManyCommandsAsync_requires_bus(FirstTestCommand[] testCommands, Guid correlationId, IDictionary<string, string> headers)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.InvokeManyCommandsAsync(null, testCommands, correlationId, headers));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_handles_null_command_list(IBus bus)
        {
            await BusExtensions.InvokeManyCommandsAsync<FirstTestCommand>(bus, null);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), It.IsAny<Guid>(), It.IsAny<IDictionary<string, string>>()), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_handles_null_command_list(IBus bus, Guid correlationId)
        {
            await BusExtensions.InvokeManyCommandsAsync<FirstTestCommand>(bus, null, correlationId);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), It.IsAny<Guid>(), It.IsAny<IDictionary<string, string>>()), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_handles_null_command_list(IBus bus, IDictionary<string, string> headers)
        {
            await BusExtensions.InvokeManyCommandsAsync<FirstTestCommand>(bus, null, headers);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), It.IsAny<Guid>(), It.IsAny<IDictionary<string, string>>()), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_handles_null_command_list(IBus bus, Guid correlationId, IDictionary<string, string> headers)
        {
            await BusExtensions.InvokeManyCommandsAsync<FirstTestCommand>(bus, null, correlationId, headers);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), It.IsAny<Guid>(), It.IsAny<IDictionary<string, string>>()), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_forwards_all_to_bus(IBus bus, FirstTestEvent[] testEvents)
        {
            await BusExtensions.RaiseManyEventsAsync(bus, testEvents);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), It.IsAny<Guid>(), It.IsAny<IDictionary<string, string>>()), Times.Exactly(testEvents.Length));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_forwards_all_to_bus(IBus bus, FirstTestEvent[] testEvents, Guid correlationId)
        {
            await BusExtensions.RaiseManyEventsAsync(bus, testEvents, correlationId);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), correlationId, It.IsAny<IDictionary<string, string>>()), Times.Exactly(testEvents.Length));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_forwards_all_to_bus(IBus bus, FirstTestEvent[] testEvents, IDictionary<string, string> headers)
        {
            await BusExtensions.RaiseManyEventsAsync(bus, testEvents, headers);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), It.IsAny<Guid>(), headers), Times.Exactly(testEvents.Length));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_forwards_all_to_bus(IBus bus, FirstTestEvent[] testEvents, Guid correlationId, IDictionary<string, string> headers)
        {
            await BusExtensions.RaiseManyEventsAsync(bus, testEvents, correlationId, headers);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), correlationId, headers), Times.Exactly(testEvents.Length));
        }

        [Test, CustomAutoMoqData]
        public void RaiseManyEventsAsync_requires_bus(FirstTestEvent[] testEvents)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.RaiseManyEventsAsync(null, testEvents));
        }

        [Test, CustomAutoMoqData]
        public void RaiseManyEventsAsync_requires_bus(FirstTestEvent[] testEvents, Guid correlationId)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.RaiseManyEventsAsync(null, testEvents, correlationId));
        }

        [Test, CustomAutoMoqData]
        public void RaiseManyEventsAsync_requires_bus(FirstTestEvent[] testEvents, IDictionary<string, string> headers)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.RaiseManyEventsAsync(null, testEvents, headers));
        }

        [Test, CustomAutoMoqData]
        public void RaiseManyEventsAsync_requires_bus(FirstTestEvent[] testEvents, Guid correlationId, IDictionary<string, string> headers)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.RaiseManyEventsAsync(null, testEvents, correlationId, headers));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_handles_null_event_list(IBus bus)
        {
            await BusExtensions.RaiseManyEventsAsync<FirstTestEvent>(bus, null);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), It.IsAny<Guid>(), It.IsAny<IDictionary<string, string>>()), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_handles_null_event_list(IBus bus, Guid correlationId)
        {
            await BusExtensions.RaiseManyEventsAsync<FirstTestEvent>(bus, null, correlationId);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), correlationId, It.IsAny<IDictionary<string, string>>()), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_handles_null_event_list(IBus bus, IDictionary<string, string> headers)
        {
            await BusExtensions.RaiseManyEventsAsync<FirstTestEvent>(bus, null, headers);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), It.IsAny<Guid>(), headers), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_handles_null_event_list(IBus bus, Guid correlationId, IDictionary<string, string> headers)
        {
            await BusExtensions.RaiseManyEventsAsync<FirstTestEvent>(bus, null, correlationId, headers);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), It.IsAny<Guid>(), headers), Times.Never);
        }
    }
}