using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
// ReSharper disable InvokeAsExtensionMethod

namespace Tests
{
    [TestFixture]
    public class DispatcherExtensionsTests
    {
        [Test, AutoMoqData]
        public async Task InvokeManyCommandsAsync_forwards_each_command(IDispatcher dispatcher, FirstTestCommand[] testCommands)
        {
            await DispatcherExtensions.InvokeManyCommandsAsync(dispatcher, testCommands);

            Mock.Get(dispatcher).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>()), Times.Exactly(testCommands.Length));

        }

        [Test, AutoMoqData]
        public async Task InvokeManyCommandsAsync_handles_null_command_list(IDispatcher dispatcher)
        {
            await DispatcherExtensions.InvokeManyCommandsAsync<FirstTestCommand>(dispatcher, null);

            Mock.Get(dispatcher).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>()), Times.Never);
        }

        [Test, AutoMoqData]
        public void InvokeManyCommandsAsync_requires_bus(FirstTestCommand[] testCommands)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => DispatcherExtensions.InvokeManyCommandsAsync(null, testCommands));
        }

        [Test, AutoMoqData]
        public async Task RaiseManyEventsAsync_forwards_each_event(IDispatcher dispatcher, FirstTestEvent[] testEvents)
        {
            await DispatcherExtensions.RaiseManyEventsAsync(dispatcher, testEvents);

            Mock.Get(dispatcher).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>()), Times.Exactly(testEvents.Length));
        }

        [Test, AutoMoqData]
        public void RaiseManyEventsAsync_requires_bus(FirstTestEvent[] testEvents)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => DispatcherExtensions.RaiseManyEventsAsync(null, testEvents));
        }

        [Test, AutoMoqData]
        public async Task RaiseManyEventsAsync_handles_null_event_list(IDispatcher dispatcher)
        {
            await DispatcherExtensions.RaiseManyEventsAsync<FirstTestEvent>(dispatcher, null);

            Mock.Get(dispatcher).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>()), Times.Never);
        }
    }
}