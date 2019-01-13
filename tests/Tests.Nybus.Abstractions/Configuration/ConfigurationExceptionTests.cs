using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Nybus.Configuration;

namespace Tests.Configuration
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
