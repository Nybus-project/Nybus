using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class NybusDispatcherTests
    {
        [Test, CustomAutoMoqData]
        public void Bus_is_required(Guid correlationId)
        {
            Assert.Throws<ArgumentNullException>(() => new NybusDispatcher(null, correlationId));
        }

        [Test, CustomAutoMoqData]
        public async Task Command_is_invoked_with_given_correlationId([Frozen] IBus bus, [Frozen] Guid correlationId, NybusDispatcher sut, FirstTestCommand testCommand)
        {
            await sut.InvokeCommandAsync(testCommand);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(testCommand, correlationId), Times.Once);
        }

        [Test, CustomAutoMoqData]
        public async Task Event_is_raised_with_given_correlationId([Frozen] IBus bus, [Frozen] Guid correlationId, NybusDispatcher sut, FirstTestEvent testEvent)
        {
            await sut.RaiseEventAsync(testEvent);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(testEvent, correlationId), Times.Once);
        }
    }
}
