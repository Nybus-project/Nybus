using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Nybus.Policies;

namespace Tests.Policies
{
    [TestFixture]
    public class NoopErrorPolicyProviderTests
    {
        [Test, CustomAutoMoqData]
        public void ProvideName_is_noop(NoopErrorPolicyProvider sut)
        {
            Assert.That(sut.ProviderName, Is.EqualTo("noop"));
        }

        [Test, CustomAutoMoqData]
        public void CreatePolicy_returns_NoopErrorPolicy_instance(NoopErrorPolicyProvider sut, IConfigurationSection configurationSection)
        {
            var policy = sut.CreatePolicy(configurationSection) as NoopErrorPolicy;

            Assert.That(policy, Is.Not.Null);
        }
    }
}