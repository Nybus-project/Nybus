using System;
using NUnit.Framework;
using Nybus.Utils;

namespace Tests.Utils
{
    [TestFixture]
    public class MessageDescriptorStoreTests
    {
        [Test]
        [AutoMoqData]
        public void Same_command_type_wont_be_registered_twice(MessageDescriptorStore sut)
        {
            Assume.That(sut.RegisterCommandType<FirstTestCommand>(), Is.True);

            Assert.That(sut.RegisterCommandType<FirstTestCommand>(), Is.False);
        }

        [Test]
        [AutoMoqData]
        public void Command_type_can_be_found_by_its_descriptor(MessageDescriptorStore sut)
        {
            var descriptor = MessageDescriptor.CreateFromType(typeof(FirstTestCommand));

            sut.RegisterCommandType<FirstTestCommand>();

            var isFound = sut.FindCommandTypeForDescriptor(descriptor, out var typeFound);

            Assert.That(isFound, Is.True);
            Assert.That(typeFound, Is.EqualTo(typeof(FirstTestCommand)).Using<Type>((first, second) => string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        [AutoMoqData]
        public void Same_event_type_wont_be_registered_twice(MessageDescriptorStore sut)
        {
            Assume.That(sut.RegisterEventType<FirstTestEvent>(), Is.True);

            Assert.That(sut.RegisterEventType<FirstTestEvent>(), Is.False);
        }

        [Test]
        [AutoMoqData]
        public void Event_type_can_be_found_by_its_descriptor(MessageDescriptorStore sut)
        {
            var descriptor = MessageDescriptor.CreateFromType(typeof(FirstTestEvent));

            sut.RegisterEventType<FirstTestEvent>();

            var isFound = sut.FindEventTypeForDescriptor(descriptor, out var typeFound);

            Assert.That(isFound, Is.True);
            Assert.That(typeFound, Is.EqualTo(typeof(FirstTestEvent)).Using<Type>((first, second) => string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
