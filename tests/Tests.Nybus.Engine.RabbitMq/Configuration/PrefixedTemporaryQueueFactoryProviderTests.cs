using AutoFixture;
using AutoFixture.Idioms;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class PrefixedTemporaryQueueFactoryProviderTests
    {
        [Test, CustomAutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(PrefixedTemporaryQueueFactoryProvider).GetConstructors());
        }

        [Test, CustomAutoMoqData]
        public void ProviderName_returns_prefix(PrefixedTemporaryQueueFactoryProvider sut)
        {
            Assert.That(sut.ProviderName, Is.EqualTo("prefix"));
        }

        [Test, CustomAutoMoqData]
        public void CreateFactory_returns_PrefixedTemporaryQueueFactory(PrefixedTemporaryQueueFactoryProvider sut,
            IConfigurationSection configuration, string prefix)
        {
            Mock.Get(configuration.GetSection("Prefix")).SetupGet(p => p.Value).Returns(prefix);

            var result = sut.CreateFactory(configuration);

            Assert.That(result, Is.InstanceOf<PrefixedTemporaryQueueFactory>());
        }

        [Test, CustomAutoMoqData]
        public void CreateFactory_returns_PrefixedTemporaryQueueFactory_with_prefix(PrefixedTemporaryQueueFactoryProvider sut,
            IConfigurationSection configuration, string prefix)
        {
            Mock.Get(configuration.GetSection("Prefix")).SetupGet(p => p.Value).Returns(prefix);

            var result = sut.CreateFactory(configuration) as PrefixedTemporaryQueueFactory;

            Assert.That(result.Prefix, Is.EqualTo(prefix));
        }

        [Test, CustomAutoMoqData]
        public void CreateFactory_throws_if_no_prefix_is_specified(PrefixedTemporaryQueueFactoryProvider sut,
            IConfigurationSection configuration, string prefix)
        {
            Mock.Get(configuration.GetSection("Prefix")).SetupGet(p => p.Value).Returns((string)null);

            Assert.That(() => sut.CreateFactory(configuration), Throws.ArgumentNullException);
        }
    }
}