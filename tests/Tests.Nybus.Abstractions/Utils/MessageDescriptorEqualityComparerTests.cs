using System.Collections.Generic;
using AutoFixture.NUnit3;
using NUnit.Framework;
using Nybus.Utils;

namespace Tests.Utils
{
    [TestFixture]
    public class MessageDescriptorEqualityComparerTests
    {
        [Test, CustomAutoMoqData]
        public void Item_is_equal_to_itself(IEqualityComparer<MessageDescriptor> sut, MessageDescriptor descriptor)
        {
            Assert.That(sut.Equals(descriptor, descriptor), Is.True);
        }

        [Test, CustomAutoMoqData]
        public void Null_is_equal_to_itself(IEqualityComparer<MessageDescriptor> sut)
        {
            Assert.That(sut.Equals(null, null), Is.True);
        }

        [Test, CustomAutoMoqData]
        public void Items_with_same_values_are_equal(
            IEqualityComparer<MessageDescriptor> sut, 
            [Frozen(Matching.ParameterName)] string name, // must have the same name as the MessageDescriptor.ctor parameter
            [Frozen(Matching.ParameterName)] string @namespace, // must have the same name as the MessageDescriptor.ctor parameter
            MessageDescriptor first, 
            MessageDescriptor second)
        {
            Assume.That(first.Name, Is.EqualTo(second.Name));
            Assume.That(first.Namespace, Is.EqualTo(second.Namespace));

            Assert.That(sut.Equals(first, second), Is.True);
        }

        [Test, CustomAutoMoqData]
        public void Item_is_not_equal_to_null(IEqualityComparer<MessageDescriptor> sut, MessageDescriptor descriptor)
        {
            Assert.That(sut.Equals(descriptor, null), Is.False);
        }

        [Test, CustomAutoMoqData]
        public void Null_is_not_equal_to_any_item(IEqualityComparer<MessageDescriptor> sut, MessageDescriptor descriptor)
        {
            Assert.That(sut.Equals(null, descriptor), Is.False);
        }

        [Test, CustomAutoMoqData]
        public void Items_with_same_values_have_same_hashcode(
            IEqualityComparer<MessageDescriptor> sut,
            [Frozen(Matching.ParameterName)] string name, // must have the same name as the MessageDescriptor.ctor parameter
            [Frozen(Matching.ParameterName)] string @namespace, // must have the same name as the MessageDescriptor.ctor parameter
            MessageDescriptor first,
            MessageDescriptor second)
        {
            Assume.That(first.Name, Is.EqualTo(second.Name));
            Assume.That(first.Namespace, Is.EqualTo(second.Namespace));

            Assert.That(sut.GetHashCode(first), Is.EqualTo(sut.GetHashCode(second)));
        }
    }
}