using System;
using NUnit.Framework;
using Nybus;
using Nybus.Utils;

namespace Tests
{
    [TestFixture]
    public class NybusCommandContextTests
    {
        [Test]
        public void Message_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new NybusCommandContext<FirstTestCommand>(null));
        }

        [Test, CustomAutoMoqData]
        public void CommandMessage_is_attached_to_context(CommandMessage<FirstTestCommand> commandMessage)
        {
            var sut = new NybusCommandContext<FirstTestCommand>(commandMessage);

            Assert.That(sut.CommandMessage, Is.SameAs(commandMessage));
        }

        [Test, CustomAutoMoqData]
        public void Command_is_attached_to_context(CommandMessage<FirstTestCommand> commandMessage)
        {
            var sut = new NybusCommandContext<FirstTestCommand>(commandMessage);

            Assert.That(sut.Command, Is.SameAs(commandMessage.Command));
        }

        [Test, CustomAutoMoqData]
        public void Message_correlationId_is_attached_to_context(CommandMessage<FirstTestCommand> commandMessage)
        {
            var sut = new NybusCommandContext<FirstTestCommand>(commandMessage);

            Assert.That(sut.CorrelationId, Is.EqualTo(commandMessage.Headers.CorrelationId));
        }

        [Test, CustomAutoMoqData]
        public void Message_SentOn_is_attached_to_context(CommandMessage<FirstTestCommand> commandMessage)
        {
            var sut = new NybusCommandContext<FirstTestCommand>(commandMessage);

            Assert.That(sut.SentOn, Is.EqualTo(commandMessage.Headers.SentOn));
        }

        [Test, CustomAutoMoqData]
        public void ReceivedOn_is_attached_to_context_from_clock(CommandMessage<FirstTestCommand> commandMessage, DateTimeOffset now)
        {
            Clock.SetTo(new TestClock(now));

            var sut = new NybusCommandContext<FirstTestCommand>(commandMessage);

            Assert.That(sut.ReceivedOn, Is.EqualTo(now));

            Clock.Reset();
        }
    }
}
