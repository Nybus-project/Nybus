using System.Security.Cryptography.X509Certificates;
using Castle.MicroKernel;
using Moq;
using NUnit.Framework;
using Nybus;
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
        public void Resolve_invokes_kernel()
        {
            var sut= CreateSystemUnderTest();

            var item = sut.Resolve<IEventHandler<TestEvent>>();

            mockKernel.Verify(p => p.Resolve<IEventHandler<TestEvent>>(), Times.Once);
        }

        [Test]
        public void Release_invokes_kernel()
        {
            var sut = CreateSystemUnderTest();

            var item = Mock.Of<IEventHandler<TestEvent>>();

            sut.Release(item);

            mockKernel.Verify(p => p.ReleaseComponent(item), Times.Once);
        }
    }
}