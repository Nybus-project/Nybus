using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Nybus.Logging;
using Ploeh.AutoFixture;

namespace Tests.Logging
{
    [TestFixture]
    public class LoggerTests
    {
        private IFixture fixture;
        private Mock<ILoggerFactory> mockFactory;
        private string logName;

        private Mock<ILoggerProvider> mockProvider;
        private Mock<ILogger> mockLogger;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            mockFactory = new Mock<ILoggerFactory>();
            mockFactory.Setup(p => p.GetProviders()).Returns(new ILoggerProvider[0]);

            logName = fixture.Create<string>();


            mockLogger = new Mock<ILogger>();
            mockProvider = new Mock<ILoggerProvider>();
            mockProvider.Setup(p => p.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        }

        private IEnumerable<LogLevel> GetLogLevels()
        {
            yield return LogLevel.Verbose;

            yield return LogLevel.Debug;

            yield return LogLevel.Information;

            yield return LogLevel.Warning;

            yield return LogLevel.Error;

            yield return LogLevel.Critical;
        }
            
            [Test, ExpectedException]
        public void LoggerFactory_is_required()
        {
            new Logger(null, logName);
        }

        [Test, ExpectedException]
        public void Name_is_required()
        {
            new Logger(Mock.Of<ILoggerFactory>(), null);
        }

        private Logger CreateSystemUnderTest()
        {
            return new Logger(mockFactory.Object, logName);
        }

        [Test]
        public void When_adding_a_provider_a_new_logger_is_added()
        {
            var sut = CreateSystemUnderTest();

            sut.AddProvider(mockProvider.Object);

            mockProvider.Verify(p => p.CreateLogger(logName), Times.Once);
        }

        [Test]
        public void When_adding_a_provider_calls_to_Log_are_forwarded()
        {
            var sut = CreateSystemUnderTest();

            sut.AddProvider(mockProvider.Object);

            var level = fixture.Create<LogLevel>();
            var state = fixture.Create<Dictionary<string, object>>();
            var error = Mock.Of<Exception>();
            MessageFormatter formatter = null;

            mockLogger.Setup(p => p.IsEnabled(level)).Returns(true);

            sut.Log(level, state, error, formatter);

            mockLogger.Verify(p => p.Log(level, state, error, formatter));
        }

        [Test]
        [ExpectedException(typeof (AggregateException))]
        public void Log_throws_if_an_inner_logger_throws()
        {
            var sut = CreateSystemUnderTest();

            sut.AddProvider(mockProvider.Object);

            var level = fixture.Create<LogLevel>();
            var state = fixture.Create<Dictionary<string, object>>();
            var error = Mock.Of<Exception>();
            MessageFormatter formatter = null;

            mockLogger.Setup(p => p.IsEnabled(level)).Returns(true);
            mockLogger.Setup(p => p.Log(level, state, error, formatter)).Throws(Mock.Of<Exception>());

            sut.Log(level, state, error, formatter);
        }

        [Test]
        [TestCaseSource(nameof(GetLogLevels))]
        public void IsEnabled_is_forwarded_to_registered_loggers(LogLevel level)
        {
            var sut = CreateSystemUnderTest();

            sut.AddProvider(mockProvider.Object);

            sut.IsEnabled(level);

            mockLogger.Verify(p => p.IsEnabled(level));
        }

        [Test]
        [TestCaseSource(nameof(GetLogLevels))]
        public void IsEnabled_returns_true_if_one_logger_returns_true(LogLevel level)
        {
            var sut = CreateSystemUnderTest();

            sut.AddProvider(mockProvider.Object);

            mockLogger.Setup(p => p.IsEnabled(level)).Returns(true);

            var actual = sut.IsEnabled(level);

            Assert.That(actual, Is.True);
        }

        [Test]
        [TestCaseSource(nameof(GetLogLevels))]
        [ExpectedException(typeof (AggregateException))]
        public void IsEnabled_groups_exceptions(LogLevel level)
        {
            var sut = CreateSystemUnderTest();

            sut.AddProvider(mockProvider.Object);

            mockLogger.Setup(p => p.IsEnabled(level)).Throws(Mock.Of<Exception>());

            sut.IsEnabled(level);
        }

        [Test]
        public void New_creates_logger_from_provider()
        {
            mockFactory.Setup(p => p.GetProviders()).Returns(new[] {mockProvider.Object});

            var sut = new Logger(mockFactory.Object, logName);

            mockProvider.Verify(p => p.CreateLogger(logName));
        }

        [Test]
        public void IsEnabled_returns_false_if_lower_than_MinimumLevel()
        {
            mockFactory.SetupGet(p => p.MinimumLevel).Returns(LogLevel.Error);

            var sut = CreateSystemUnderTest();

            Assert.That(sut.IsEnabled(LogLevel.Information), Is.False);
        }
    }
}