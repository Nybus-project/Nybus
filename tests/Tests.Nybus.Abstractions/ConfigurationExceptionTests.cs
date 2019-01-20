using System;
using NUnit.Framework;
using Nybus;

namespace Tests
{
    [TestFixture]
    public class ConfigurationExceptionTests
    {
        [Test, CustomAutoMoqData]
        public void ConfigurationException_can_be_built_with_message(string message)
        {
            var sut = new ConfigurationException(message);

            Assert.That(sut.Message, Is.EqualTo(message));
        }

        [Test, CustomAutoMoqData]
        public void ConfigurationException_can_be_built_with_message_and_innerException(string message, Exception innerException)
        {
            var sut = new ConfigurationException(message, innerException);

            Assert.That(sut.Message, Is.EqualTo(message));

            Assert.That(sut.InnerException, Is.SameAs(innerException));
        }
    }
}
