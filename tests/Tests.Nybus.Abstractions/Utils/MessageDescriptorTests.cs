using System;
using System.Reflection;
using NUnit.Framework;
using Nybus;
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
        [InlineAutoMoqData(typeof(TestClass))]
        public void CreateFromType_uses_attribute_values(Type type)
        {
            var attribute = type.GetCustomAttribute<MessageAttribute>();

            Assume.That(attribute, Is.Not.Null);

            var sut = MessageDescriptor.CreateFromType(type);

            Assert.That(sut.Name, Is.EqualTo(attribute.Name));
            Assert.That(sut.Namespace, Is.EqualTo(attribute.Namespace));
        }

        [Test, AutoMoqData]
        public void MessageDescriptor_can_be_created_from_type(Type type)
        {
            var messageDescriptor = new MessageDescriptor(type);

            Assert.That(messageDescriptor.Name, Is.EqualTo(type.Name));
            Assert.That(messageDescriptor.Namespace, Is.EqualTo(type.Namespace));
        }

        [Test, AutoMoqData]
        public void MessageDescriptor_can_be_created_from_MessageAttribute(MessageAttribute attribute)
        {
            var messageDescriptor = new MessageDescriptor(attribute);

            Assert.That(messageDescriptor.Name, Is.EqualTo(attribute.Name));
            Assert.That(messageDescriptor.Namespace, Is.EqualTo(attribute.Namespace));
        }

        [Test, CustomAutoMoqData]
        public void TryParse_return_false_if_null()
        {
            var result = MessageDescriptor.TryParse(null, out var descriptor);

            Assert.That(result, Is.False);
            Assert.That(descriptor, Is.Null);
        }

        [Test, CustomAutoMoqData]
        public void TryParse_requires_correct_format(string descriptorName)
        {
            var result = MessageDescriptor.TryParse(descriptorName, out var descriptor);

            Assert.That(result, Is.False);
            Assert.That(descriptor, Is.Null);
        }
    }

    [Message("TestCommand", "Tests")]
    public class TestClass
    {

    }
}