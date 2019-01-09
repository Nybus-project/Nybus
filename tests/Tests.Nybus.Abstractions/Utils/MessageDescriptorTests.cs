using System;
using NUnit.Framework;
using Nybus.Utils;

namespace Tests.Utils
{
    [TestFixture]
    public class MessageDescriptorTests
    {
        [Test]
        public void CreateFromType_requires_type()
        {
            Assert.Throws<ArgumentNullException>(() => MessageDescriptor.CreateFromType(null));
        }

        [Test]
        public void CreateFromAttribute_requires_attribute()
        {
            Assert.Throws<ArgumentNullException>(() => MessageDescriptor.CreateFromAttribute(null));
        }

        [Test, AutoMoqData]
        public void TryParse_requires_correct_format(string descriptorName)
        {
            var result = MessageDescriptor.TryParse(descriptorName, out var descriptor);

            Assert.That(result, Is.False);
            Assert.That(descriptor, Is.Null);
        }
    }
}