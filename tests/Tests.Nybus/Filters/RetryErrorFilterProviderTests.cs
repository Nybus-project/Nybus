using System;
using System.Threading.Tasks;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Filters;
using Nybus.Utils;

namespace Tests.Filters
{
    [TestFixture]
    public class RetryErrorFilterProviderTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(RetryErrorFilterProvider).GetConstructors());
        }

        [Test, AutoMoqData]
        public void ProviderName_is_correct(RetryErrorFilterProvider sut)
        {
            Assert.That(sut.ProviderName, Is.EqualTo("retry"));
        }

        [Test, AutoMoqData]
        public void CreateErrorFilter_returns_filter([Frozen] IServiceProvider serviceProvider, RetryErrorFilterProvider sut, IConfigurationSection settings, IBusEngine engine, ILogger<RetryErrorFilter> logger, int maxRetries)
        {
            Mock.Get(settings.GetSection("MaxRetries")).Setup(p => p.Value).Returns(maxRetries.ToString());

            Mock.Get(serviceProvider).Setup(p => p.GetService(typeof(IBusEngine))).Returns(engine);
            Mock.Get(serviceProvider).Setup(p => p.GetService(typeof(ILogger<RetryErrorFilter>))).Returns(logger);

            var filter = sut.CreateErrorFilter(settings);

            Assert.That(filter, Is.InstanceOf<RetryErrorFilter>());
        }
    }

    [TestFixture]
    public class RetryErrorFilterTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(RetryErrorFilter).GetConstructors());
        }

        [Test, AutoMoqData]
        public void MaxRetries_cant_be_negative(IBusEngine engine, ILogger<RetryErrorFilter> logger, int retries)
        {
            var options = new RetryErrorFilterOptions { MaxRetries = -retries };

            Assert.Throws<ArgumentOutOfRangeException>(() => new RetryErrorFilter(engine, options, logger));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_notifies_engine_if_retry_count_equal_or_greater_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context)
        {
            var next = Mock.Of<CommandErrorDelegate<FirstTestCommand>>();

            context.Message.Headers[Headers.RetryCount] = options.MaxRetries.Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).Verify(p => p.NotifyFailAsync(context.Message));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_invokes_next_if_retry_count_equal_or_greater_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context)
        {
            var next = Mock.Of<CommandErrorDelegate<FirstTestCommand>>();

            context.Message.Headers[Headers.RetryCount] = options.MaxRetries.Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(next).Verify(p => p(context, error));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_retries_if_retry_count_less_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context)
        {
            var next = Mock.Of<CommandErrorDelegate<FirstTestCommand>>();

            context.Message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).Verify(p => p.SendCommandAsync(context.CommandMessage));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_increments_retry_count_if_retry_count_present([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context)
        {
            var next = Mock.Of<CommandErrorDelegate<FirstTestCommand>>();

            context.Message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Assert.That(context.Message.Headers.ContainsKey(Headers.RetryCount));
            Assert.That(context.Message.Headers[Headers.RetryCount], Is.EqualTo((options.MaxRetries - 1).Stringfy()));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_retries_if_retry_count_not_present([Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context)
        {
            var next = Mock.Of<CommandErrorDelegate<FirstTestCommand>>();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).Verify(p => p.SendCommandAsync(context.CommandMessage));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_adds_retry_count_if_retry_count_not_present([Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context)
        {
            var next = Mock.Of<CommandErrorDelegate<FirstTestCommand>>();

            await sut.HandleErrorAsync(context, error, next);

            Assert.That(context.Message.Headers.ContainsKey(Headers.RetryCount));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_notifies_engine_if_retry_count_equal_or_greater_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context)
        {
            var next = Mock.Of<EventErrorDelegate<FirstTestEvent>>();

            context.Message.Headers[Headers.RetryCount] = options.MaxRetries.Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).Verify(p => p.NotifyFailAsync(context.Message));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_invokes_next_if_retry_count_equal_or_greater_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context)
        {
            var next = Mock.Of<EventErrorDelegate<FirstTestEvent>>();

            context.Message.Headers[Headers.RetryCount] = options.MaxRetries.Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(next).Verify(p => p(context, error));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_retries_if_retry_count_less_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context)
        {
            var next = Mock.Of<EventErrorDelegate<FirstTestEvent>>();

            context.Message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).Verify(p => p.SendEventAsync(context.EventMessage));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_increments_retry_count_if_retry_count_present([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context)
        {
            var next = Mock.Of<EventErrorDelegate<FirstTestEvent>>();

            context.Message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Assert.That(context.Message.Headers.ContainsKey(Headers.RetryCount));
            Assert.That(context.Message.Headers[Headers.RetryCount], Is.EqualTo((options.MaxRetries - 1).Stringfy()));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_retries_if_retry_count_not_present([Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context)
        {
            var next = Mock.Of<EventErrorDelegate<FirstTestEvent>>();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).Verify(p => p.SendEventAsync(context.EventMessage));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_adds_retry_count_if_retry_count_not_present([Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context)
        {
            var next = Mock.Of<EventErrorDelegate<FirstTestEvent>>();

            await sut.HandleErrorAsync(context, error, next);

            Assert.That(context.Message.Headers.ContainsKey(Headers.RetryCount));
        }
    }
}