using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using Nybus.RabbitMq;
using RabbitMQ.Client;
// ReSharper disable InvokeAsExtensionMethod

namespace Tests.RabbitMq
{
    [TestFixture]
    public class BasicPropertiesExtensionsTests
    {
        [Test, CustomAutoMoqData]
        public void GetHeader_supports_bytes(IBasicProperties properties, string headerName, Encoding encoding, string value)
        {
            var values = new Dictionary<string, object>
            {
                [headerName] = encoding.GetBytes(value)
            };

            Mock.Get(properties).SetupGet(p => p.Headers).Returns(values);

            var result = BasicPropertiesExtensions.GetHeader(properties, headerName, encoding);

            Assert.That(result, Is.EqualTo(value));
        }

        [Test, CustomAutoMoqData]
        public void GetHeader_supports_strings(IBasicProperties properties, string headerName, Encoding encoding, string value)
        {
            var values = new Dictionary<string, object>
            {
                [headerName] = value
            };

            Mock.Get(properties).SetupGet(p => p.Headers).Returns(values);

            var result = BasicPropertiesExtensions.GetHeader(properties, headerName, encoding);

            Assert.That(result, Is.EqualTo(value));
        }

        [Test, CustomAutoMoqData]
        public void GetHeader_returns_null_if_unknown_type(IBasicProperties properties, string headerName, Encoding encoding, object value)
        {
            var values = new Dictionary<string, object>
            {
                [headerName] = value
            };

            Mock.Get(properties).SetupGet(p => p.Headers).Returns(values);

            var result = BasicPropertiesExtensions.GetHeader(properties, headerName, encoding);

            Assert.That(result, Is.Null);
        }
    }
}
