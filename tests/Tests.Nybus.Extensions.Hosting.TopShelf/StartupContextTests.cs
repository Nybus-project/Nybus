using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Nybus;
using Topshelf.Runtime;

namespace Tests
{
    [TestFixture]
    public class StartupContextTests
    {
        [Test, AutoMoqData]
        public void Context_returns_given_configuration([Frozen] IConfigurationRoot configuration, StartupContext sut)
        {
            Assert.That(sut.Configuration, Is.SameAs(configuration));
        }

        [Test, AutoMoqData]
        public void Context_returns_given_settings([Frozen] HostSettings settings, StartupContext sut)
        {
            Assert.That(sut.Settings, Is.SameAs(settings));
        }
    }
}