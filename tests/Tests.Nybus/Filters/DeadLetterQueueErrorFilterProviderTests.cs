using System;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Filters;

namespace Tests.Filters
{
    [TestFixture]
    public class DeadLetterQueueErrorFilterProviderTests
    {
        [Test, CustomAutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(DeadLetterQueueErrorFilterProvider).GetConstructors());
        }

        [Test, CustomAutoMoqData]
        public void ProviderName_is_correct(DeadLetterQueueErrorFilterProvider sut)
        {
            Assert.That(sut.ProviderName, Is.EqualTo("dead-letter-queue"));
        }

        [Test, CustomAutoMoqData]
        public void CreateErrorFilter_returns_filter(
            [Frozen] IServiceProvider serviceProvider,
            DeadLetterQueueErrorFilterProvider sut,
            IConfigurationSection configuration,
            IBusEngine busEngine,
            ILogger<DeadLetterQueueErrorFilter> logger)
        {
            Mock.Get(serviceProvider).Setup(s => s.GetService(typeof(IBusEngine))).Returns(busEngine);
            Mock.Get(serviceProvider).Setup(s => s.GetService(typeof(ILogger<DeadLetterQueueErrorFilter>))).Returns(logger);

            var result = sut.CreateErrorFilter(configuration);

            Assert.That(result, Is.InstanceOf<DeadLetterQueueErrorFilter>());
        }
    }
}