using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Nybus.Logging;
using Ploeh.AutoFixture;
// ReSharper disable InvokeAsExtensionMethod

namespace Tests.Logging
{
    [TestFixture]
    public class LoggerExtensionsTests
    {
        [TestFixture]
        public abstract class LogTestBase
        {
            protected IFixture Fixture;

            protected Mock<ILogger> MockLogger;

            [SetUp]
            public void InitializeBase()
            {
                Fixture = new Fixture();
                Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

                MockLogger = new Mock<ILogger>();
            }
        }

        [TestFixture]
        public class LogTests : LogTestBase
        {
            public IEnumerable<LogLevel> GetAllLogLevels()
            {
                yield return LogLevel.Verbose;

                yield return LogLevel.Debug;

                yield return LogLevel.Information;

                yield return LogLevel.Warning;

                yield return LogLevel.Error;

                yield return LogLevel.Critical;
            }

            [Test]
            [TestCaseSource(nameof(GetAllLogLevels))]
            public void Log_Object_Error_uses_provided_formatter(LogLevel level)
            {
                var state = new
                {
                    text = Fixture.Create<string>()
                };

                var exception = Fixture.Create<Exception>();

                var formattedState = Fixture.Create<string>();

                bool isFormatted = false;

                LoggerExtensions.Log(MockLogger.Object, level, state, exception, (a, e) =>
                {
                    isFormatted = true;
                    return formattedState;
                });

                MockLogger.Verify(p=> p.Log(level, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.text)) && (string)d[LoggerExtensions.MessageKey] == formattedState), exception));

                Assert.That(isFormatted, Is.True);
            }

            [Test]
            [TestCaseSource(nameof(GetAllLogLevels))]

            public void Log_string_adds_message(LogLevel level)
            {
                var message = Fixture.Create<string>();

                LoggerExtensions.Log(MockLogger.Object, level, message);

                MockLogger.Verify(p => p.Log(level, It.Is<IDictionary<string, object>>(d => (string) d[LoggerExtensions.MessageKey] == message), null));
            }

            [Test]
            [TestCaseSource(nameof(GetAllLogLevels))]
            public void Log_exception_adds_message(LogLevel level)
            {
                var exception = Fixture.Create<Exception>();

                LoggerExtensions.Log(MockLogger.Object, level, exception);

                MockLogger.Verify(p => p.Log(level, It.Is<IDictionary<string, object>>(d => (string) d[LoggerExtensions.MessageKey] == exception.ToString()), exception));
            }

            [Test]
            [TestCaseSource(nameof(GetAllLogLevels))]
            public void Log_object_uses_provided_formatter(LogLevel level)
            {
                var state = new
                {
                    text = Fixture.Create<string>()
                };

                var formattedState = Fixture.Create<string>();

                bool isFormatted = false;

                LoggerExtensions.Log(MockLogger.Object, level, state, a =>
                {
                    isFormatted = true;
                    return formattedState;
                });

                MockLogger.Verify(p => p.Log(level, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.text)) && (string)d[LoggerExtensions.MessageKey] == formattedState), null));

                Assert.That(isFormatted, Is.True);
            }

            [Test]
            [TestCaseSource(nameof(GetAllLogLevels))]
            public void Log_object_does_not_add_message_when_no_formatter_is_specified(LogLevel level)
            {
                var state = new
                {
                    text = Fixture.Create<string>()
                };


                LoggerExtensions.Log(MockLogger.Object, level, state, null);

                MockLogger.Verify(p => p.Log(level, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.text)) && !d.ContainsKey(LoggerExtensions.MessageKey)), null));

            }
        }

        [TestFixture]
        public class LogVerboseTests : LogTestBase
        {
            [Test]
            public void String_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();

                LoggerExtensions.LogVerbose(MockLogger.Object, message);

                MockLogger.Verify(p => p.Log(LogLevel.Verbose, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), null));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogVerbose(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Verbose, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), exception));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    data = Fixture.Create<string>()
                };

                var formattedState = Fixture.Create<string>();

                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogVerbose(MockLogger.Object, state, exception, (a, e) => formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Verbose, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.data)) && d.ContainsKey(nameof(state.text)) && (string) d[LoggerExtensions.MessageKey] == formattedState), exception));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogVerbose(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Verbose, It.Is<IDictionary<string, object>>(d => (string) d[LoggerExtensions.MessageKey] == exception.ToString()), exception));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    data = Fixture.Create<string>()
                };

                var formattedState = Fixture.Create<string>();

                LoggerExtensions.LogVerbose(MockLogger.Object, state, a => formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Verbose, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.data)) && d.ContainsKey(nameof(state.text)) && (string)d[LoggerExtensions.MessageKey] == formattedState), null));
            }
        }

        [TestFixture]
        public class LogWarningTests : LogTestBase
        {
            [Test]
            public void String_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();

                LoggerExtensions.LogWarning(MockLogger.Object, message);

                MockLogger.Verify(p => p.Log(LogLevel.Warning, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), null));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogWarning(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Warning, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), exception));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    data = Fixture.Create<string>()
                };

                Exception exception = Fixture.Create<Exception>();

                var formattedState = Fixture.Create<string>();

                LoggerExtensions.LogWarning(MockLogger.Object, state, exception, (a, e) => formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Warning, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.data)) && d.ContainsKey(nameof(state.text)) && (string)d[LoggerExtensions.MessageKey] == formattedState), exception));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogWarning(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Warning, It.Is<IDictionary<string, object>>(d => (string) d[LoggerExtensions.MessageKey] == exception.ToString()), exception));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    data = Fixture.Create<string>()
                };

                var formattedState = Fixture.Create<string>();

                LoggerExtensions.LogWarning(MockLogger.Object, state, a => formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Warning, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.data)) && d.ContainsKey(nameof(state.text)) && (string)d[LoggerExtensions.MessageKey] == formattedState), null));
            }

        }

        [TestFixture]
        public class LogDebugTests : LogTestBase
        {
            [Test]
            public void String_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();

                LoggerExtensions.LogDebug(MockLogger.Object, message);

                MockLogger.Verify(p => p.Log(LogLevel.Debug, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), null));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogDebug(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Debug, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), exception));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    data = Fixture.Create<string>()
                };

                Exception exception = Fixture.Create<Exception>();

                var formattedState = Fixture.Create<string>();

                LoggerExtensions.LogDebug(MockLogger.Object, state, exception, (a,e) => formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Debug, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.data)) && d.ContainsKey(nameof(state.text)) && (string)d[LoggerExtensions.MessageKey] == formattedState), exception));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogDebug(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Debug, It.Is<IDictionary<string, object>>(d => (string) d[LoggerExtensions.MessageKey] == exception.ToString()), exception));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                var formattedState = Fixture.Create<string>();

                LoggerExtensions.LogDebug(MockLogger.Object, state, a => formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Debug, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text)) && (string)d[LoggerExtensions.MessageKey]== formattedState), null));
            }

        }

        [TestFixture]
        public class LogInformationTests : LogTestBase
        {
            [Test]
            public void String_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();

                LoggerExtensions.LogInformation(MockLogger.Object, message);

                MockLogger.Verify(p => p.Log(LogLevel.Information, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), null));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogInformation(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Information, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), exception));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    data = Fixture.Create<string>()
                };

                var formattedState = Fixture.Create<string>();

                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogInformation(MockLogger.Object, state, exception,(a,e)=>formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Information, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.data)) && d.ContainsKey(nameof(state.text)) && (string) d[LoggerExtensions.MessageKey] == formattedState), exception));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogInformation(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Information, It.Is<IDictionary<string, object>>(d => (string) d[LoggerExtensions.MessageKey] == exception.ToString()), exception));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    data = Fixture.Create<string>()
                };

                var formattedState = Fixture.Create<string>();

                LoggerExtensions.LogInformation(MockLogger.Object, state, a => formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Information, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.data)) && d.ContainsKey(nameof(state.text))), null));
            }

        }

        [TestFixture]
        public class LogErrorTests : LogTestBase
        {
            [Test]
            public void String_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();

                LoggerExtensions.LogError(MockLogger.Object, message);

                MockLogger.Verify(p => p.Log(LogLevel.Error, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), null));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogError(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Error, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), exception));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    data = Fixture.Create<string>()
                };

                Exception exception = Fixture.Create<Exception>();

                var formattedState = Fixture.Create<string>();

                LoggerExtensions.LogError(MockLogger.Object, state, exception, (a,e)=>formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Error, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.data)) && d.ContainsKey(nameof(state.text)) && (string) d[LoggerExtensions.MessageKey] == formattedState), exception));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogError(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Error, It.Is<IDictionary<string, object>>(d => (string) d[LoggerExtensions.MessageKey] == exception.ToString()), exception));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    data = Fixture.Create<string>()
                };

                var formattedState = Fixture.Create<string>();

                LoggerExtensions.LogError(MockLogger.Object, state, a => formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Error, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.data)) && d.ContainsKey(nameof(state.text)) && (string) d[LoggerExtensions.MessageKey] == formattedState), null));
            }

        }

        [TestFixture]
        public class LogCriticalTests : LogTestBase
        {
            [Test]
            public void String_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();

                LoggerExtensions.LogCritical(MockLogger.Object, message);

                MockLogger.Verify(p => p.Log(LogLevel.Critical, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), null));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogCritical(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Critical, It.Is<IDictionary<string, object>>(d => d.ContainsKey(LoggerExtensions.MessageKey)), exception));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    data = Fixture.Create<string>()
                };

                Exception exception = Fixture.Create<Exception>();

                var formattedState = Fixture.Create<string>();

                LoggerExtensions.LogCritical(MockLogger.Object, state, exception, (a,e)=>formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Critical, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.data)) && d.ContainsKey(nameof(state.text)) && (string)d[LoggerExtensions.MessageKey] == formattedState), exception));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogCritical(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Critical, It.Is<IDictionary<string, object>>(d => (string) d[LoggerExtensions.MessageKey] == exception.ToString()), exception));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    data = Fixture.Create<string>()
                };

                var formattedState = Fixture.Create<string>();

                LoggerExtensions.LogCritical(MockLogger.Object, state, a => formattedState);

                MockLogger.Verify(p => p.Log(LogLevel.Critical, It.Is<IDictionary<string, object>>(d => d.ContainsKey(nameof(state.data)) && d.ContainsKey(nameof(state.text)) && (string)d[LoggerExtensions.MessageKey] == formattedState), null));
            }

        }

    }
}