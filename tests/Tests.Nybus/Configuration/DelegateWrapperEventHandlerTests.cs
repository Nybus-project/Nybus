using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class DelegateWrapperEventHandlerTests
    {
        [Test]
        public void Handler_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateWrapperEventHandler<FirstTestEvent>(null));
        }

        [Test, CustomAutoMoqData]
        public async Task Handler_is_executed(IDispatcher dispatcher, IEventContext<FirstTestEvent> context, EventReceivedAsync<FirstTestEvent> handler)
        {
            var sut = new DelegateWrapperEventHandler<FirstTestEvent>(handler);

            await sut.HandleAsync(dispatcher, context);

            Mock.Get(handler).Verify(p => p(dispatcher, context), Times.Once);
        }

        [Test, CustomAutoMoqData]
        public void Handler_errors_are_not_caught(IDispatcher dispatcher, IEventContext<FirstTestEvent> context, Exception error, EventReceivedAsync<FirstTestEvent> handler)
        {
            Mock.Get(handler).Setup(p => p(It.IsAny<IDispatcher>(), It.IsAny<IEventContext<FirstTestEvent>>())).Throws(error);

            var sut = new DelegateWrapperEventHandler<FirstTestEvent>(handler);

            Assert.ThrowsAsync(error.GetType(), () => sut.HandleAsync(dispatcher, context));
        }
    }
}
