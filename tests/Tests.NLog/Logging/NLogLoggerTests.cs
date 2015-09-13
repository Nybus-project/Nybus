using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NLog;
using NUnit.Framework;
using Nybus.Logging;
using Ploeh.AutoFixture;
using ILogger = NLog.ILogger;
using LogLevel = Nybus.Logging.LogLevel;

namespace Tests.Logging
{
    [TestFixture]
    public class NLogLoggerTests
    {
        private IFixture fixture;
        private Mock<NLog.ILogger> mockLogger;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            mockLogger = new Mock<ILogger>();
        }

        [Test]
        [ExpectedException]
        public void Logger_is_required()
        {
            new NLogLogger(null);
        }

        private NLogLogger CreateSystemUnderTest()
        {
            return new NLogLogger(mockLogger.Object);
        }

        public IEnumerable<object> GetLogLevels()
        {
            yield return new object[] {LogLevel.Trace, NLog.LogLevel.Trace};

            yield return new object[] {LogLevel.Debug, NLog.LogLevel.Debug};

            yield return new object[] {LogLevel.Info, NLog.LogLevel.Info};

            yield return new object[] {LogLevel.Error, NLog.LogLevel.Error};

            yield return new object[] {LogLevel.Fatal, NLog.LogLevel.Fatal};

            yield return new object[] {LogLevel.Warn, NLog.LogLevel.Warn};

            yield return new object[] {(LogLevel) 0, NLog.LogLevel.Info};
        }

        [Test]
        [TestCaseSource(nameof(GetLogLevels))]
        public void Log_levels_are_correctly_converted(LogLevel level, NLog.LogLevel expected)
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<string>();

            sut.Log(level, message);

            mockLogger.Verify(p => p.Log(It.Is<LogEventInfo>(lei => lei.Level == expected)), Times.Once);
        }

        [Test]
        [TestCaseSource(nameof(GetLogLevels))]
        public async Task Async_Log_levels_are_correctly_converted(LogLevel level, NLog.LogLevel expected)
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<string>();

            await sut.LogAsync(level, message);

            mockLogger.Verify(p => p.Log(It.Is<LogEventInfo>(lei => lei.Level == expected)), Times.Once);
        }

        [Test]
        public async Task Data_is_added_as_properties()
        {
            var data = new {text = fixture.Create<string>()};

            var sut = CreateSystemUnderTest();

            var message = fixture.Create<string>();

            await sut.LogAsync(LogLevel.Info, message, data);

            mockLogger.Verify(p => p.Log(It.Is<LogEventInfo>(lei => string.Equals((string)lei.Properties[nameof(data.text)], data.text))), Times.Once);
        }

        [Test]
        public async Task Message_is_added_as_message()
        {
            var sut = CreateSystemUnderTest();

            var message = fixture.Create<string>();

            await sut.LogAsync(LogLevel.Info, message);

            mockLogger.Verify(p => p.Log(It.Is<LogEventInfo>(lei => string.Equals(message, lei.Message))), Times.Once);
        }

        [Test]
        public void Exception_information_are_added_to_log()
        {
            var sut = CreateSystemUnderTest();
            var message = fixture.Create<string>();

            try
            {
                Exception innerException = new Exception("This is an inner exception");
                throw new Exception("This is a test exception", innerException);
            }
            catch (Exception ex)
            {
                sut.Log(LogLevel.Error, message, new {exception = ex});
            }

            mockLogger.Verify(p => p.Log(It.Is<LogEventInfo>(lei => string.Equals(message, lei.Message))), Times.Once);
            mockLogger.Verify(p => p.Log(It.Is<LogEventInfo>(lei => string.Equals((string)lei.Properties["error-method"], nameof(Exception_information_are_added_to_log)))), Times.Once);
        }
    }
}
