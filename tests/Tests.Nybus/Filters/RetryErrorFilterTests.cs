﻿using System;
using System.Threading.Tasks;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Filters;
using Nybus.Utils;

namespace Tests.Filters {
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
        public async Task HandleError_forwards_to_next_if_retry_count_equal_or_greater_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context, CommandErrorDelegate<FirstTestCommand> next)
        {
            context.Message.Headers[Headers.RetryCount] = options.MaxRetries.Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).VerifyNoOtherCalls();

            Mock.Get(next).Verify(p => p(context, error));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_invokes_next_if_retry_count_equal_or_greater_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context, CommandErrorDelegate<FirstTestCommand> next)
        {
            context.Message.Headers[Headers.RetryCount] = options.MaxRetries.Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(next).Verify(p => p(context, error));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_retries_if_retry_count_less_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context, CommandErrorDelegate<FirstTestCommand> next)
        {
            context.Message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).Verify(p => p.SendMessageAsync(context.CommandMessage));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_increments_retry_count_if_retry_count_present([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context, CommandErrorDelegate<FirstTestCommand> next)
        {
            context.Message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Assert.That(context.Message.Headers.ContainsKey(Headers.RetryCount));
            Assert.That(context.Message.Headers[Headers.RetryCount], Is.EqualTo((options.MaxRetries - 1).Stringfy()));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_retries_if_retry_count_not_present([Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context, CommandErrorDelegate<FirstTestCommand> next)
        {
            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).Verify(p => p.SendMessageAsync(context.CommandMessage));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_adds_retry_count_if_retry_count_not_present([Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusCommandContext<FirstTestCommand> context, CommandErrorDelegate<FirstTestCommand> next)
        {
            await sut.HandleErrorAsync(context, error, next);

            Assert.That(context.Message.Headers.ContainsKey(Headers.RetryCount));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_forwards_to_next_if_retry_count_equal_or_greater_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context, EventErrorDelegate<FirstTestEvent> next)
        {
            context.Message.Headers[Headers.RetryCount] = options.MaxRetries.Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).VerifyNoOtherCalls();

            Mock.Get(next).Verify(p => p(context, error));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_invokes_next_if_retry_count_equal_or_greater_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context, EventErrorDelegate<FirstTestEvent> next)
        {
            context.Message.Headers[Headers.RetryCount] = options.MaxRetries.Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(next).Verify(p => p(context, error));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_retries_if_retry_count_less_than_maxRetries([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context, EventErrorDelegate<FirstTestEvent> next)
        {
            context.Message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).Verify(p => p.SendMessageAsync(context.EventMessage));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_increments_retry_count_if_retry_count_present([Frozen] RetryErrorFilterOptions options, [Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context, EventErrorDelegate<FirstTestEvent> next)
        {
            context.Message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleErrorAsync(context, error, next);

            Assert.That(context.Message.Headers.ContainsKey(Headers.RetryCount));
            Assert.That(context.Message.Headers[Headers.RetryCount], Is.EqualTo((options.MaxRetries - 1).Stringfy()));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_retries_if_retry_count_not_present([Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context, EventErrorDelegate<FirstTestEvent> next)
        {
            await sut.HandleErrorAsync(context, error, next);

            Mock.Get(engine).Verify(p => p.SendMessageAsync(context.EventMessage));
        }

        [Test, CustomAutoMoqData]
        public async Task HandleError_adds_retry_count_if_retry_count_not_present([Frozen] IBusEngine engine, RetryErrorFilter sut, Exception error, NybusEventContext<FirstTestEvent> context, EventErrorDelegate<FirstTestEvent> next)
        {
            await sut.HandleErrorAsync(context, error, next);

            Assert.That(context.Message.Headers.ContainsKey(Headers.RetryCount));
        }
    }
}