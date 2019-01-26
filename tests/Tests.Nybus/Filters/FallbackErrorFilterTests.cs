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
    public class FallbackErrorFilterTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(FallbackErrorFilter).GetConstructors());
        }

        [Test, AutoMoqData]
        public async Task HandleErrorAsync_notifies_engine_on_Command([Frozen] IBusEngine engine, FallbackErrorFilter sut, ICommandContext<FirstTestCommand> context, Exception exception, CommandErrorDelegate<FirstTestCommand> next)
        {
            await sut.HandleErrorAsync(context, exception, next);

            Mock.Get(engine).Verify(p => p.NotifyFailAsync(context.Message));
        }

        [Test, AutoMoqData]
        public async Task HandleErrorAsync_notifies_engine_on_Event([Frozen] IBusEngine engine, FallbackErrorFilter sut, IEventContext<FirstTestEvent> context, Exception exception, EventErrorDelegate<FirstTestEvent> next)
        {
            await sut.HandleErrorAsync(context, exception, next);

            Mock.Get(engine).Verify(p => p.NotifyFailAsync(context.Message));
        }
    }
}