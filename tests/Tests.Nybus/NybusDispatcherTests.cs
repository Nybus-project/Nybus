using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class NybusDispatcherTests
    {
        [Test, AutoMoqData]
        public void Bus_is_required(Guid correlationId)
        {
            Assert.Throws<ArgumentNullException>(() => new NybusDispatcher(null, correlationId));
        }

        [Test, AutoMoqData]
        public async Task Command_is_invoked_with_given_correlationId(NybusDispatcher sut, FirstTestCommand testCommand)
        {
            await sut.InvokeCommandAsync(testCommand);

            Mock.Get(sut.Bus).Verify(p => p.InvokeCommandAsync(testCommand, sut.CorrelationId), Times.Once);
        }

        [Test, AutoMoqData]
        public async Task Event_is_raised_with_given_correlationId(NybusDispatcher sut, FirstTestEvent testEvent)
        {
            await sut.RaiseEventAsync(testEvent);

            Mock.Get(sut.Bus).Verify(p => p.RaiseEventAsync(testEvent, sut.CorrelationId), Times.Once);
        }
    }
}
