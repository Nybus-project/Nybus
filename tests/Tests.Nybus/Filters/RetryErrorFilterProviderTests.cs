using System;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Filters;

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
}