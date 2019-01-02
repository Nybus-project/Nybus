using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Nybus.Policies;

namespace Tests.Policies
{
    [TestFixture]
    public class RetryErrorPolicyProviderTests
    {
        [Test, AutoMoqData]
        public void LoggerFactory_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new RetryErrorPolicyProvider(null));
        }

        [Test, AutoMoqData]
        public void ProviderName_is_retry(RetryErrorPolicyProvider sut)
        {
            Assert.That(sut.ProviderName, Is.EqualTo("retry"));
        }

        [Test, AutoMoqData]
        public void CreatePolicy_returns_RetryErrorPolicy_instance(RetryErrorPolicyProvider sut, IConfigurationSection configurationSection, int maxRetries)
        {
            Mock.Get(configurationSection.GetSection("MaxRetries")).Setup(p => p.Value).Returns(maxRetries.ToString());

            var policy = sut.CreatePolicy(configurationSection) as RetryErrorPolicy;

            Assert.That(policy, Is.Not.Null);
            Assert.That(policy.MaxRetries, Is.EqualTo(maxRetries));
        }
    }
}
