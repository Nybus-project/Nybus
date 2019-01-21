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
    public class DispatcherExtensionsTests
    {
        [Test, CustomAutoMoqData]
        public void InvokeCommandAsync_requires_dispatcher(FirstTestCommand command)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => DispatcherExtensions.InvokeCommandAsync(null, command));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeCommandAsync_forwards_with_empty_header_set(IDispatcher dispatcher, FirstTestCommand command)
        {
            await DispatcherExtensions.InvokeCommandAsync(dispatcher, command);

            Mock.Get(dispatcher).Verify(p => p.InvokeCommandAsync(command, It.IsAny<IDictionary<string, string>>()));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_forwards_each_command(IDispatcher dispatcher, FirstTestCommand[] testCommands)
        {
            await DispatcherExtensions.InvokeManyCommandsAsync(dispatcher, testCommands);

            Mock.Get(dispatcher).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), It.IsAny<IDictionary<string, string>>()), Times.Exactly(testCommands.Length));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_forwards_each_command(IDispatcher dispatcher, FirstTestCommand[] testCommands, IDictionary<string, string> headers)
        {
            await DispatcherExtensions.InvokeManyCommandsAsync(dispatcher, testCommands, headers);

            Mock.Get(dispatcher).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), headers), Times.Exactly(testCommands.Length));
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_handles_null_command_list(IDispatcher dispatcher)
        {
            await DispatcherExtensions.InvokeManyCommandsAsync<FirstTestCommand>(dispatcher, null);

            Mock.Get(dispatcher).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), It.IsAny<IDictionary<string, string>>()), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public async Task InvokeManyCommandsAsync_handles_null_command_list(IDispatcher dispatcher, IDictionary<string, string> headers)
        {
            await DispatcherExtensions.InvokeManyCommandsAsync<FirstTestCommand>(dispatcher, null, headers);

            Mock.Get(dispatcher).Verify(p => p.InvokeCommandAsync(It.IsAny<FirstTestCommand>(), headers), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public void InvokeManyCommandsAsync_requires_dispatcher(FirstTestCommand[] testCommands)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => DispatcherExtensions.InvokeManyCommandsAsync(null, testCommands));
        }

        [Test, CustomAutoMoqData]
        public void InvokeManyCommandsAsync_requires_dispatcher(FirstTestCommand[] testCommands, IDictionary<string, string> headers)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => DispatcherExtensions.InvokeManyCommandsAsync(null, testCommands, headers));
        }

        [Test, CustomAutoMoqData]
        public void RaiseEventAsync_requires_dispatcher(FirstTestEvent testEvent)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => DispatcherExtensions.RaiseEventAsync(null, testEvent));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseEventAsync_forwards_with_empty_header_set(IDispatcher dispatcher, FirstTestEvent testEvent)
        {
            await DispatcherExtensions.RaiseEventAsync(dispatcher, testEvent);

            Mock.Get(dispatcher).Verify(p => p.RaiseEventAsync(testEvent, It.IsAny<IDictionary<string, string>>()));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_forwards_each_event(IDispatcher dispatcher, FirstTestEvent[] testEvents)
        {
            await DispatcherExtensions.RaiseManyEventsAsync(dispatcher, testEvents);

            Mock.Get(dispatcher).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), It.IsAny<IDictionary<string, string>>()), Times.Exactly(testEvents.Length));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_forwards_each_event(IDispatcher dispatcher, FirstTestEvent[] testEvents, IDictionary<string, string> headers)
        {
            await DispatcherExtensions.RaiseManyEventsAsync(dispatcher, testEvents, headers);

            Mock.Get(dispatcher).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), headers), Times.Exactly(testEvents.Length));
        }

        [Test, CustomAutoMoqData]
        public void RaiseManyEventsAsync_requires_dispatcher(FirstTestEvent[] testEvents)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => DispatcherExtensions.RaiseManyEventsAsync(null, testEvents));
        }

        [Test, CustomAutoMoqData]
        public void RaiseManyEventsAsync_requires_dispatcher(FirstTestEvent[] testEvents, IDictionary<string, string> headers)
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => DispatcherExtensions.RaiseManyEventsAsync(null, testEvents, headers));
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_handles_null_event_list(IDispatcher dispatcher)
        {
            await DispatcherExtensions.RaiseManyEventsAsync<FirstTestEvent>(dispatcher, null);

            Mock.Get(dispatcher).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), It.IsAny<IDictionary<string, string>>()), Times.Never);
        }

        [Test, CustomAutoMoqData]
        public async Task RaiseManyEventsAsync_handles_null_event_list(IDispatcher dispatcher, IDictionary<string, string> headers)
        {
            await DispatcherExtensions.RaiseManyEventsAsync<FirstTestEvent>(dispatcher, null, headers);

            Mock.Get(dispatcher).Verify(p => p.RaiseEventAsync(It.IsAny<FirstTestEvent>(), headers), Times.Never);
        }
    }
}