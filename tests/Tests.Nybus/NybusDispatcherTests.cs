using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class NybusDispatcherTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(NybusDispatcher).GetConstructors());
        }

        [Test, CustomAutoMoqData]
        public async Task Command_is_invoked_with_given_correlationId([Frozen] IBus bus, [Frozen] Message message, NybusDispatcher sut, FirstTestCommand testCommand, IDictionary<string, string> headers)
        {
            await sut.InvokeCommandAsync(testCommand, headers);

            Mock.Get(bus).Verify(p => p.InvokeCommandAsync(testCommand, message.Headers.CorrelationId, It.IsAny<IDictionary<string, string>>()), Times.Once);
        }

        [Test, CustomAutoMoqData]
        public async Task Event_is_raised_with_given_correlationId([Frozen] IBus bus, [Frozen] Message message, NybusDispatcher sut, FirstTestEvent testEvent, IDictionary<string, string> headers)
        {
            await sut.RaiseEventAsync(testEvent, headers);

            Mock.Get(bus).Verify(p => p.RaiseEventAsync(testEvent, message.Headers.CorrelationId, It.IsAny<IDictionary<string, string>>()), Times.Once);
        }
    }
}
