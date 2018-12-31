using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class ConnectionFactoryProvidersTests
    {
        [Test, AutoMoqData]
        public void ConnectionString_returns_ConnectionStringConnectionFactoryProvider(ConnectionFactoryProviders sut)
        {
            Assert.That(sut.ConnectionString, Is.InstanceOf<ConnectionStringConnectionFactoryProvider>());
        }

        [Test, AutoMoqData]
        public void ConnectionNode_returns_ConnectionNodeConnectionFactoryProvider(ConnectionFactoryProviders sut)
        {
            Assert.That(sut.ConnectionNode, Is.InstanceOf<ConnectionNodeConnectionFactoryProvider>());
        }
    }
}