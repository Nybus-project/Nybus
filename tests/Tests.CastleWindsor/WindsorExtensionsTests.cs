using Castle.MicroKernel;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Nybus.Container;
using Ploeh.AutoFixture;
// ReSharper disable InvokeAsExtensionMethod

namespace Tests
{
    [TestFixture]
    public class WindsorExtensionsTests
    {
        [TestFixture]
        public abstract class TestBase
        {
            protected IFixture Fixture;

            protected Mock<IKernel> MockKernel;

            [SetUp]
            public void InitializeBase()
            {
                Fixture = new Fixture();
                Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

                MockKernel = new Mock<IKernel>();
            }
        }

        [TestFixture]
        public class UseWindsorTests : TestBase
        {
            [Test, ExpectedException]
            public void Requires_Options()
            {
                WindsorExtensions.UseCastleWindsor(null, Mock.Of<IKernel>());
            }

            [Test, ExpectedException]
            public void Requires_Kernel()
            {
                var options = new NybusOptions();

                WindsorExtensions.UseCastleWindsor(options, null);
            }

            [Test]
            public void Options_are_configured()
            {
                var options = new NybusOptions();

                options.UseCastleWindsor(MockKernel.Object);

                Assert.That(options.Container, Is.InstanceOf<WindsorBusContainer>());

                WindsorBusContainer container = (WindsorBusContainer) options.Container;

                Assert.That(container.Kernel, Is.SameAs(MockKernel.Object));
            }
        }
    }
}