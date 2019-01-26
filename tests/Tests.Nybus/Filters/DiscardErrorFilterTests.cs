using System;
using System.Threading.Tasks;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Filters;

namespace Tests.Filters
{
    [TestFixture]
    public class DiscardErrorFilterTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(DiscardErrorFilter).GetConstructors());
        }

        [Test, AutoMoqData]
        public async Task HandleErrorAsync_notifies_engine_on_Command([Frozen] IBusEngine engine, DiscardErrorFilter sut, ICommandContext<FirstTestCommand> context, Exception exception, CommandErrorDelegate<FirstTestCommand> next)
        {
            await sut.HandleErrorAsync(context, exception, next);

            Mock.Get(engine).Verify(p => p.NotifyFailAsync(context.Message));
        }

        [Test, AutoMoqData]
        public async Task HandleErrorAsync_notifies_engine_on_Event([Frozen] IBusEngine engine, DiscardErrorFilter sut, IEventContext<FirstTestEvent> context, Exception exception, EventErrorDelegate<FirstTestEvent> next)
        {
            await sut.HandleErrorAsync(context, exception, next);

            Mock.Get(engine).Verify(p => p.NotifyFailAsync(context.Message));
        }

        [Test, AutoMoqData]
        public async Task HandleErrorAsync_forwards_to_next_if_error_on_Command([Frozen] IBusEngine engine, DiscardErrorFilter sut, ICommandContext<FirstTestCommand> context, Exception exception, Exception discardException, CommandErrorDelegate<FirstTestCommand> next)
        {
            Mock.Get(engine).Setup(p => p.NotifyFailAsync(It.IsAny<Message>())).ThrowsAsync(discardException);

            await sut.HandleErrorAsync(context, exception, next);

            Mock.Get(next).Verify(p => p(context, exception));
        }

        [Test, AutoMoqData]
        public async Task HandleErrorAsync_forwards_to_next_if_error_on_Event([Frozen] IBusEngine engine, DiscardErrorFilter sut, IEventContext<FirstTestEvent> context, Exception exception, Exception discardException, EventErrorDelegate<FirstTestEvent> next)
        {
            Mock.Get(engine).Setup(p => p.NotifyFailAsync(It.IsAny<Message>())).ThrowsAsync(discardException);

            await sut.HandleErrorAsync(context, exception, next);

            Mock.Get(next).Verify(p => p(context, exception));
        }
    }
}
