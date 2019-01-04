using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Policies;
using Nybus.Utils;

namespace Tests.Policies
{
    [TestFixture]
    public class RetryErrorPolicyTests
    {
        [Test, AutoMoqData]
        public void Options_is_required(ILogger<RetryErrorPolicy> logger)
        {
            Assert.Throws<ArgumentNullException>(() => new RetryErrorPolicy(null, logger));
        }

        [Test, AutoMoqData]
        public void MaxRetries_cant_be_negative(ILogger<RetryErrorPolicy> logger, int retries)
        {
            var options = new RetryErrorPolicyOptions { MaxRetries = -retries };

            Assert.Throws<ArgumentOutOfRangeException>(() => new RetryErrorPolicy(options, logger));
        }

        [Test, AutoMoqData]
        public void Logger_is_required(RetryErrorPolicyOptions options)
        {
            Assert.Throws<ArgumentNullException>(() => new RetryErrorPolicy(options, null));
        }

        [Test, AutoMoqData]
        public async Task HandleError_notifies_engine_if_retry_count_equal_or_greater_than_maxRetries([Frozen] RetryErrorPolicyOptions options, RetryErrorPolicy sut, IBusEngine engine, Exception error, CommandMessage<FirstTestCommand> message)
        {
            message.Headers[Headers.RetryCount] = options.MaxRetries.Stringfy();

            await sut.HandleError(engine, error, message);

            Mock.Get(engine).Verify(p => p.NotifyFail(message));
        }

        [Test, AutoMoqData]
        public async Task HandleError_retries_if_retry_count_less_than_maxRetries([Frozen] RetryErrorPolicyOptions options, RetryErrorPolicy sut, IBusEngine engine, Exception error, CommandMessage<FirstTestCommand> message)
        {
            message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleError(engine, error, message);

            Mock.Get(engine).Verify(p => p.SendCommandAsync(message));
        }

        [Test, AutoMoqData]
        public async Task HandleError_increments_retry_count_if_retry_count_present([Frozen] RetryErrorPolicyOptions options, RetryErrorPolicy sut, IBusEngine engine, Exception error, CommandMessage<FirstTestCommand> message)
        {
            message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleError(engine, error, message);

            Assert.That(message.Headers.ContainsKey(Headers.RetryCount));
            Assert.That(message.Headers[Headers.RetryCount], Is.EqualTo((options.MaxRetries - 1).Stringfy()));
        }

        [Test, AutoMoqData]
        public async Task HandleError_retries_if_retry_count_not_present(RetryErrorPolicy sut, IBusEngine engine, Exception error, CommandMessage<FirstTestCommand> message)
        {
            await sut.HandleError(engine, error, message);

            Mock.Get(engine).Verify(p => p.SendCommandAsync(message));
        }

        [Test, AutoMoqData]
        public async Task HandleError_adds_retry_count_if_retry_count_not_present(RetryErrorPolicy sut, IBusEngine engine, Exception error, CommandMessage<FirstTestCommand> message)
        {
            await sut.HandleError(engine, error, message);

            Assert.That(message.Headers.ContainsKey(Headers.RetryCount));
        }

        [Test, AutoMoqData]
        public async Task HandleError_notifies_engine_if_retry_count_equal_or_greater_than_maxRetries([Frozen] RetryErrorPolicyOptions options, RetryErrorPolicy sut, IBusEngine engine, Exception error, EventMessage<FirstTestEvent> message)
        {
            message.Headers[Headers.RetryCount] = options.MaxRetries.Stringfy();

            await sut.HandleError(engine, error, message);

            Mock.Get(engine).Verify(p => p.NotifyFail(message));
        }

        [Test, AutoMoqData]
        public async Task HandleError_retries_if_retry_count_less_than_maxRetries([Frozen] RetryErrorPolicyOptions options, RetryErrorPolicy sut, IBusEngine engine, Exception error, EventMessage<FirstTestEvent> message)
        {
            message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleError(engine, error, message);

            Mock.Get(engine).Verify(p => p.SendEventAsync(message));
        }

        [Test, AutoMoqData]
        public async Task HandleError_retries_if_retry_count_not_present(RetryErrorPolicy sut, IBusEngine engine, Exception error, EventMessage<FirstTestEvent> message)
        {
            await sut.HandleError(engine, error, message);

            Mock.Get(engine).Verify(p => p.SendEventAsync(message));
        }

        [Test, AutoMoqData]
        public async Task HandleError_adds_retry_count_if_retry_count_not_present(RetryErrorPolicy sut, IBusEngine engine, Exception error, EventMessage<FirstTestEvent> message)
        {
            await sut.HandleError(engine, error, message);

            Assert.That(message.Headers.ContainsKey(Headers.RetryCount));
        }

        [Test, AutoMoqData]
        public async Task HandleError_increments_retry_count_if_retry_count_present([Frozen] RetryErrorPolicyOptions options, RetryErrorPolicy sut, IBusEngine engine, Exception error, EventMessage<FirstTestEvent> message)
        {
            message.Headers[Headers.RetryCount] = (options.MaxRetries - 2).Stringfy();

            await sut.HandleError(engine, error, message);

            Assert.That(message.Headers.ContainsKey(Headers.RetryCount));
            Assert.That(message.Headers[Headers.RetryCount], Is.EqualTo((options.MaxRetries - 1).Stringfy()));
        }
    }
}