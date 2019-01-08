using System;
using NUnit.Framework;
using Nybus.Utils;
using Samples;

namespace Tests.Utils
{
    [TestFixture]
    public class MessageDescriptorStoreTests
    {
        [Test]
        [InlineAutoMoqData(typeof(FirstTestCommand))]
        [InlineAutoMoqData(typeof(SecondTestCommand))]
        [InlineAutoMoqData(typeof(ThirdTestCommand))]
        [InlineAutoMoqData(typeof(AttributeTestCommand))]
        [InlineAutoMoqData(typeof(FirstTestEvent))]
        [InlineAutoMoqData(typeof(SecondTestEvent))]
        [InlineAutoMoqData(typeof(ThirdTestEvent))]
        [InlineAutoMoqData(typeof(AttributeTestEvent))]
        [AutoMoqData]
        public void Same_type_wont_be_registered_twice(Type type, MessageDescriptorStore sut)
        {
            Assume.That(sut.RegisterType(type), Is.True);

            Assert.That(sut.RegisterType(type), Is.False);
        }

        [Test]
        [InlineAutoMoqData(typeof(FirstTestCommand))]
        [InlineAutoMoqData(typeof(SecondTestCommand))]
        [InlineAutoMoqData(typeof(ThirdTestCommand))]
        [InlineAutoMoqData(typeof(FirstTestEvent))]
        [InlineAutoMoqData(typeof(SecondTestEvent))]
        [InlineAutoMoqData(typeof(ThirdTestEvent))]
        [AutoMoqData]
        public void Type_can_be_found_by_its_descriptor(Type type, MessageDescriptorStore sut)
        {
            var descriptor = MessageDescriptor.CreateFromType(type);

            sut.RegisterType(type);

            var isFound = sut.TryGetTypeForDescriptor(descriptor, out var typeFound);

            Assert.That(isFound, Is.True);
            Assert.That(typeFound, Is.EqualTo(type).Using<Type>((first, second) => string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        [InlineAutoMoqData(typeof(FirstTestCommand))]
        [InlineAutoMoqData(typeof(SecondTestCommand))]
        [InlineAutoMoqData(typeof(ThirdTestCommand))]
        [InlineAutoMoqData(typeof(FirstTestEvent))]
        [InlineAutoMoqData(typeof(SecondTestEvent))]
        [InlineAutoMoqData(typeof(ThirdTestEvent))]
        [AutoMoqData]
        public void Descriptor_can_be_found_by_its_type(Type type, MessageDescriptorStore sut)
        {
            sut.RegisterType(type);

            var isFound = sut.TryGetDescriptorForType(type, out var descriptorFound);

            Assert.That(isFound, Is.True);
            Assert.That(descriptorFound.Name, Is.EqualTo(type.Name));
            Assert.That(descriptorFound.Namespace, Is.EqualTo(type.Namespace));
        }
    }
}
