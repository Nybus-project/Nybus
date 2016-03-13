using Moq;
using NUnit.Framework;
using Nybus.Logging;
using Ploeh.AutoFixture;

// ReSharper disable InvokeAsExtensionMethod

namespace Tests.Logging
{
    [TestFixture]
    public class LoggerFactoryExtensionsTests
    {
        private IFixture fixture;

        private Mock<ILoggerFactory> mockLoggerFactory;

        [SetUp]
        public void InitializeBase()
        {
            fixture = new Fixture();
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(p => p.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        }

        [Test]
        public void CreateCurrentClassLogger_creates_a_logger_with_this_class_name()
        {
            var logger = LoggerFactoryExtensions.CreateCurrentClassLogger(mockLoggerFactory.Object);

            mockLoggerFactory.Verify(p => p.CreateLogger(typeof(LoggerFactoryExtensionsTests).FullName), Times.Once);
        }
    }
}