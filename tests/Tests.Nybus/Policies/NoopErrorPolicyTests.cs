using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Policies;

namespace Tests.Policies
{
    [TestFixture]
    public class NoopErrorPolicyTests
    {
        [Test, AutoMoqData]
        public async Task HandleError_notifies_engine(NoopErrorPolicy sut, IBusEngine engine, Exception exception, CommandMessage<FirstTestCommand> message)
        {
            await sut.HandleError(engine, exception, message);

            Mock.Get(engine).Verify(p => p.NotifyFail(message));
        }

        [Test, AutoMoqData]
        public async Task HandleError_notifies_engine(NoopErrorPolicy sut, IBusEngine engine, Exception exception, EventMessage<FirstTestEvent> message)
        {
            await sut.HandleError(engine, exception, message);

            Mock.Get(engine).Verify(p => p.NotifyFail(message));
        }
    }
}