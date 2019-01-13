using System;
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
        public async Task InvokeCommandAsync_generates_new_correlationId(IBus bus, FirstTestCommand testCommand)
        {
            await BusExtensions.InvokeCommandAsync(bus, testCommand);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(testCommand, It.IsAny<Guid>()));
        }

        [Test, CustomAutoMoqData]
        public void InvokeCommandAsync_requires_bus(FirstTestCommand testCommand)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.InvokeCommandAsync(null, testCommand));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseEventAsync_generates_new_correlationId(IBus bus, FirstTestEvent testEvent)
        {
            await BusExtensions.RaiseEventAsync(bus, testEvent);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(testEvent, It.IsAny<Guid>()));
        }

        [Test, CustomAutoMoqData]
        public void RaiseEventAsync_requires_bus(FirstTestEvent testEvent)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.RaiseEventAsync(null, testEvent));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_uses_given_correlationId_for_all_commands(IBus bus, FirstTestCommand[] testCommands, Guid correlationId)
        {
            await BusExtensions.InvokeManyCommandsAsync(bus, testCommands, correlationId);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), correlationId));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_uses_new_correlationId_for_each_command(IBus bus, FirstTestCommand[] testCommands)
        {
            await BusExtensions.InvokeManyCommandsAsync(bus, testCommands);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), It.IsAny<Guid>()));
        }

        [Test, CustomAutoMoqData]
        public void InvokeManyCommandsAsync_with_no_correlationId_requires_bus(FirstTestCommand[] testCommands)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.InvokeManyCommandsAsync(null, testCommands));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_with_correlationId_handles_null_command_list(IBus bus, Guid correlationId)
        {
            await BusExtensions.InvokeManyCommandsAsync<FirstTestCommand>(bus, null, correlationId);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_with_no_correlationId_handles_null_command_list(IBus bus)
        {
            await BusExtensions.InvokeManyCommandsAsync<FirstTestCommand>(bus, null);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public void InvokeManyCommandsAsync_with_correlationId_requires_bus(FirstTestCommand[] testCommands, Guid correlationId)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.InvokeManyCommandsAsync(null, testCommands, correlationId));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_uses_given_correlationId_for_all_events(IBus bus, FirstTestEvent[] testEvents, Guid correlationId)
        {
            await BusExtensions.RaiseManyEventsAsync(bus, testEvents, correlationId);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), correlationId));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_uses_new_correlationId_for_each_event(IBus bus, FirstTestEvent[] testEvents)
        {
            await BusExtensions.RaiseManyEventsAsync(bus, testEvents);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), It.IsAny<Guid>()));
        }

        [Test, CustomAutoMoqData]
        public void RaiseManyEventsAsync_with_no_correlationId_requires_bus(FirstTestEvent[] testEvents)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.RaiseManyEventsAsync(null, testEvents));
        }

        [Test, CustomAutoMoqData]
        public void RaiseManyEventsAsync_with_correlationId_requires_bus(FirstTestEvent[] testEvents, Guid correlationId)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => BusExtensions.RaiseManyEventsAsync(null, testEvents, correlationId));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_with_no_correlationId_handles_null_event_list(IBus bus)
        {
            await BusExtensions.RaiseManyEventsAsync<FirstTestEvent>(bus, null);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_with_correlationId_handles_null_event_list(IBus bus, Guid correlationId)
        {
            await BusExtensions.RaiseManyEventsAsync<FirstTestEvent>(bus, null, correlationId);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), It.IsAny<Guid>()), Times.Never);
        }
    }
}