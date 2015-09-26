using System.Security.Cryptography.X509Certificates;
using Castle.MicroKernel;
using Moq;
using NUnit.Framework;
using Nybus.Container;

namespace Tests.Container
{
    [TestFixture]
    public class WindsorBusContainerTests
    {
        private Mock<IKernel> mockKernel;

        [SetUp]
        public void Initialize()
        {
            mockKernel = new Mock<IKernel>();
        }

        private WindsorBusContainer CreateSystemUnderTest()
        {
            return new WindsorBusContainer(mockKernel.Object);
        }

        [Test]
        [ExpectedException]
        public void Kernel_is_required()
        {
            new WindsorBusContainer(null);
        }

        [Test]
        public void BeginScope_returns_a_new_scope()
        {
            var sut = CreateSystemUnderTest();

            var item = sut.BeginScope();

            Assert.That(item, Is.InstanceOf<WindsorScope>());
        }
    }
}