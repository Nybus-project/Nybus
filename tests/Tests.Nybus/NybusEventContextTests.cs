using System;
using NUnit.Framework;
using Nybus;
using Nybus.Utils;

namespace Tests
{
    [TestFixture]
    public class NybusEventContextTests
    {
        [Test]
        public void Message_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new NybusEventContext<FirstTestEvent>(null));
        }

        [Test, CustomAutoMoqData]
        public void EventMessage_is_attached_to_context(EventMessage<FirstTestEvent> eventMessage)
        {
            var sut = new NybusEventContext<FirstTestEvent>(eventMessage);

            Assert.That(sut.EventMessage, Is.SameAs(eventMessage));
        }

        [Test, CustomAutoMoqData]
        public void Command_is_attached_to_context(EventMessage<FirstTestEvent> eventMessage)
        {
            var sut = new NybusEventContext<FirstTestEvent>(eventMessage);

            Assert.That(sut.Event, Is.SameAs(eventMessage.Event));
        }

        [Test, CustomAutoMoqData]
        public void Message_correlationId_is_attached_to_context(EventMessage<FirstTestEvent> eventMessage)
        {
            var sut = new NybusEventContext<FirstTestEvent>(eventMessage);

            Assert.That(sut.CorrelationId, Is.EqualTo(eventMessage.Headers.CorrelationId));
        }

        [Test, CustomAutoMoqData]
        public void Message_SentOn_is_attached_to_context(EventMessage<FirstTestEvent> eventMessage)
        {
            var sut = new NybusEventContext<FirstTestEvent>(eventMessage);

            Assert.That(sut.SentOn, Is.EqualTo(eventMessage.Headers.SentOn));
        }

        [Test, CustomAutoMoqData]
        public void ReceivedOn_is_attached_to_context_from_clock(EventMessage<FirstTestEvent> eventMessage, DateTimeOffset now)
        {
            Clock.SetTo(new TestClock(now));

            var sut = new NybusEventContext<FirstTestEvent>(eventMessage);

            Assert.That(sut.ReceivedOn, Is.EqualTo(now));

            Clock.Reset();
        }
    }
}