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
        public class LogVerboseTests : LogTestBase
        {
            [Test]
            public void String_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();

                LoggerExtensions.LogVerbose(MockLogger.Object, message);

                MockLogger.Verify(p => p.Log(LogLevel.Verbose, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), null, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogVerbose(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Verbose, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogVerbose(MockLogger.Object, state, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Verbose, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogVerbose(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Verbose, null, exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                LoggerExtensions.LogVerbose(MockLogger.Object, state);

                MockLogger.Verify(p => p.Log(LogLevel.Verbose, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), null, It.IsAny<MessageFormatter>()));
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

                MockLogger.Verify(p => p.Log(LogLevel.Warning, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), null, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogWarning(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Warning, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogWarning(MockLogger.Object, state, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Warning, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogWarning(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Warning, null, exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                LoggerExtensions.LogWarning(MockLogger.Object, state);

                MockLogger.Verify(p => p.Log(LogLevel.Warning, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), null, It.IsAny<MessageFormatter>()));
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

                MockLogger.Verify(p => p.Log(LogLevel.Debug, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), null, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogDebug(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Debug, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogDebug(MockLogger.Object, state, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Debug, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogDebug(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Debug, null, exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                LoggerExtensions.LogDebug(MockLogger.Object, state);

                MockLogger.Verify(p => p.Log(LogLevel.Debug, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), null, It.IsAny<MessageFormatter>()));
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

                MockLogger.Verify(p => p.Log(LogLevel.Information, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), null, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogInformation(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Information, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogInformation(MockLogger.Object, state, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Information, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogInformation(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Information, null, exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                LoggerExtensions.LogInformation(MockLogger.Object, state);

                MockLogger.Verify(p => p.Log(LogLevel.Information, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), null, It.IsAny<MessageFormatter>()));
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

                MockLogger.Verify(p => p.Log(LogLevel.Error, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), null, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogError(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Error, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogError(MockLogger.Object, state, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Error, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogError(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Error, null, exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                LoggerExtensions.LogError(MockLogger.Object, state);

                MockLogger.Verify(p => p.Log(LogLevel.Error, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), null, It.IsAny<MessageFormatter>()));
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

                MockLogger.Verify(p => p.Log(LogLevel.Critical, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), null, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void String_exception_is_forwarded_as_message()
            {
                string message = Fixture.Create<string>();
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogCritical(MockLogger.Object, message, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Critical, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey("message")), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_exception_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogCritical(MockLogger.Object, state, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Critical, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Exception_is_forwarded()
            {
                Exception exception = Fixture.Create<Exception>();

                LoggerExtensions.LogCritical(MockLogger.Object, exception);

                MockLogger.Verify(p => p.Log(LogLevel.Critical, null, exception, It.IsAny<MessageFormatter>()));
            }

            [Test]
            public void Object_is_forwarded_in_dictionary()
            {
                var state = new
                {
                    text = Fixture.Create<string>(),
                    message = Fixture.Create<string>()
                };

                LoggerExtensions.LogCritical(MockLogger.Object, state);

                MockLogger.Verify(p => p.Log(LogLevel.Critical, It.Is<IReadOnlyDictionary<string, object>>(d => d.ContainsKey(nameof(state.message)) && d.ContainsKey(nameof(state.text))), null, It.IsAny<MessageFormatter>()));
            }

        }

    }
}