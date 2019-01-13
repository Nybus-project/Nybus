using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
{
    [TestFixture]
    public class ConnectionFactoryProvidersTests
    {
        [Test, CustomAutoMoqData]
        public void ConnectionString_returns_ConnectionStringConnectionFactoryProvider(ConnectionFactoryProviders sut)
        {
            Assert.That(sut.ConnectionString, Is.InstanceOf<ConnectionStringConnectionFactoryProvider>());
        }

        [Test, CustomAutoMoqData]
        public void ConnectionNode_returns_ConnectionNodeConnectionFactoryProvider(ConnectionFactoryProviders sut)
        {
            Assert.That(sut.ConnectionNode, Is.InstanceOf<ConnectionNodeConnectionFactoryProvider>());
        }
    }
}