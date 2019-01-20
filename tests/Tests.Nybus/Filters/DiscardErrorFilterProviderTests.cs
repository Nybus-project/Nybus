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
    public class DiscardErrorFilterProviderTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(DiscardErrorFilterProvider).GetConstructors());
        }

        [Test, AutoMoqData]
        public void ProviderName_is_correct(DiscardErrorFilterProvider sut)
        {
            Assert.That(sut.ProviderName, Is.EqualTo("discard"));
        }

        [Test, AutoMoqData]
        public void CreateErrorFilter_returns_filter([Frozen] IServiceProvider serviceProvider, DiscardErrorFilterProvider sut, IConfigurationSection settings, IBusEngine engine, ILogger<DiscardErrorFilter> logger)
        {
            Mock.Get(serviceProvider).Setup(p => p.GetService(typeof(IBusEngine))).Returns(engine);
            Mock.Get(serviceProvider).Setup(p => p.GetService(typeof(ILogger<DiscardErrorFilter>))).Returns(logger);

            var filter = sut.CreateErrorFilter(settings);

            Assert.That(filter, Is.InstanceOf<DiscardErrorFilter>());
        }
    }
}