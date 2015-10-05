using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Nybus.Logging;
using Ploeh.AutoFixture;

namespace Tests.Logging
{
    [TestFixture]
    public class LoggerFactoryTests
    {
        private IFixture fixture;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();
        }

        private LoggerFactory CreateSystemUnderTest()
        {
            return new LoggerFactory();
        }

        [Test]
        public void GetProviders_returns_only_added_providers()
        {
            var sut = CreateSystemUnderTest();

            var provider = Mock.Of<ILoggerProvider>();

            sut.AddProvider(provider);

            var providers = sut.GetProviders();

            Assert.That(providers, Has.Exactly(1).InstanceOf<ILoggerProvider>());
            Assert.That(providers.Single(), Is.SameAs(provider));
        }

        [Test]
        public void Dispose_is_invoked_on_all_added_providers()
        {
            var sut = CreateSystemUnderTest();

            var mockProvider = new Mock<ILoggerProvider>();

            sut.AddProvider(mockProvider.Object);

            sut.Dispose();

            mockProvider.Verify(p => p.Dispose(), Times.Once);
        }

        [Test]
        public void Dispose_swallows_exceptions_thrown_by_providers()
        {
            var sut = CreateSystemUnderTest();

            var mockProvider = new Mock<ILoggerProvider>();

            mockProvider.Setup(p => p.Dispose()).Throws(Mock.Of<Exception>());

            sut.AddProvider(mockProvider.Object);

            sut.Dispose();
        }

        [Test]
        public void Providers_can_be_registered_at_any_time()
        {
            string logName = fixture.Create<string>();

            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(p => p.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            var mockProvider = new Mock<ILoggerProvider>();
            mockProvider.Setup(p => p.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var sut = CreateSystemUnderTest();

            var logger = sut.CreateLogger(logName);

            sut.AddProvider(mockProvider.Object);

            var level = fixture.Create<LogLevel>();
            IReadOnlyDictionary<string, object> dictionary = fixture.Create<Dictionary<string, object>>();
            Exception exception = Mock.Of<Exception>();

            logger.Log(level, dictionary, error: exception, formatter: null);

            mockProvider.Verify(p => p.CreateLogger(logName));

            mockLogger.Verify(p => p.Log(level, dictionary, exception, It.IsAny<MessageFormatter>()));
        }
    }
}