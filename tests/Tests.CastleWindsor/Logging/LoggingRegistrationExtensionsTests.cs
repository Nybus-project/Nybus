using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Moq;
using NUnit.Framework;
using Nybus.Logging;
using ILoggerFactory = Nybus.Logging.ILoggerFactory;
using ILogger = Nybus.Logging.ILogger;

namespace Tests.Logging
{
    [TestFixture]
    public class LoggingRegistrationExtensionsTests
    {
        private Mock<ILoggerFactory> mockLoggerFactory;
        private Mock<ILogger> mockLogger;

        [SetUp]
        public void Initialize()
        {
            mockLogger = new Mock<ILogger>();

            mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(p => p.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
        }

        [Test]
        public void CreateLoggerForTargetClass_creates_a_logger_with_proper_name()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Register(Component.For<ILoggerFactory>().Instance(mockLoggerFactory.Object));
            container.Register(Component.For<ILogger>().CreateLoggerForTargetClass().LifestyleTransient());
            container.Register(Component.For<LoggingTestClass>());

            var testClass = container.Resolve<LoggingTestClass>();

            Assert.That(testClass.Logger, Is.Not.Null);
            mockLoggerFactory.Verify(p => p.CreateLogger(typeof(LoggingTestClass).FullName));
        }
    }

    public class LoggingTestClass
    {
        public ILogger Logger { get; set; }
    }
}
