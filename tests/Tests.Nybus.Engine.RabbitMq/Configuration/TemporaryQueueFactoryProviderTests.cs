using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class TemporaryQueueFactoryProviderTests
    {
        [Test, CustomAutoMoqData]
        public void ProviderName_returns_temporary(TemporaryQueueFactoryProvider sut)
        {
            Assert.That(sut.ProviderName, Is.EqualTo("temporary"));
        }

        [Test, CustomAutoMoqData]
        public void CreateFactory_returns_TemporaryQueueFactory_instance(TemporaryQueueFactoryProvider sut, IConfigurationSection configuration)
        {
            Assert.That(sut.CreateFactory(configuration), Is.SameAs(TemporaryQueueFactory.Instance));
        }
    }
}