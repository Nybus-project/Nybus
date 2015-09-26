using System;
using Castle.MicroKernel;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Container;

namespace Tests.Container
{
    [TestFixture]
    public class WindsorScopeTests
    {
        private Mock<IKernel> mockKernel;
        private Mock<IDisposable> mockScope;

        [SetUp]
        public void Initialize()
        {
            mockKernel = new Mock<IKernel>();

            mockScope = new Mock<IDisposable>();
        }

        [Test]
        [ExpectedException]
        public void Kernel_is_required()
        {
            new WindsorScope(null, mockScope.Object);
        }

        [Test]
        [ExpectedException]
        public void Scope_is_required()
        {
            new WindsorScope(mockKernel.Object, null);
        }

        private WindsorScope CreateSystemUnderTest()
        {
            return new WindsorScope(mockKernel.Object, mockScope.Object);
        }

        [Test]
        public void Resolve_invokes_kernel()
        {
            var sut = CreateSystemUnderTest();

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

        [Test]
        public void Dispose_disposes_inner_scope()
        {
            var sut = CreateSystemUnderTest();

            sut.Dispose();

            mockScope.Verify(p => p.Dispose(), Times.Once);
        }
    }
}