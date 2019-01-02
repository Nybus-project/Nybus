using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class DelegateWrapperCommandHandlerTests
    {
        [Test]
        public void Handler_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new DelegateWrapperCommandHandler<FirstTestCommand>(null));
        }

        [Test, AutoMoqData]
        public async Task Handler_is_executed(IDispatcher dispatcher, ICommandContext<FirstTestCommand> context)
        {
            var handler = Mock.Of<CommandReceived<FirstTestCommand>>();

            var sut = new DelegateWrapperCommandHandler<FirstTestCommand>(handler);

            await sut.HandleAsync(dispatcher, context);

            Mock.Get(handler).Verify(p => p(dispatcher, context), Times.Once);
        }

        [Test, AutoMoqData]
        public void Handler_errors_are_not_caught(IDispatcher dispatcher, ICommandContext<FirstTestCommand> context, Exception error)
        {
            var handler = Mock.Of<CommandReceived<FirstTestCommand>>();
            Mock.Get(handler).Setup(p => p(It.IsAny<IDispatcher>(), It.IsAny<ICommandContext<FirstTestCommand>>())).Throws(error);

            var sut = new DelegateWrapperCommandHandler<FirstTestCommand>(handler);

            Assert.ThrowsAsync(error.GetType(), () => sut.HandleAsync(dispatcher, context));
        }
    }
}