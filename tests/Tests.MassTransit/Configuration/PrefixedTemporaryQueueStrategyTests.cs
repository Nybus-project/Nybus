using NUnit.Framework;
using Nybus.Configuration;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class PrefixedTemporaryQueueStrategyTests
    {
        private IFixture fixture;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();
        }

        [Test, ExpectedException]
        public void Prefix_is_required()
        {
            new PrefixedTemporaryQueueStrategy(null);
        }

        [Test, ExpectedException]
        public void Prefix_cant_be_empty()
        {
            new PrefixedTemporaryQueueStrategy(string.Empty);
        }

        [Test]
        public void GetQueueName_returns_a_queue_with_give_prefix()
        {
            string prefix = fixture.Create<string>();

            var sut = new PrefixedTemporaryQueueStrategy(prefix);

            var value = sut.GetQueueName();

            Assert.That(value, Is.StringStarting(prefix));
        }

        [Test]
        public void GetQueueName_returns_a_temporary_queue()
        {
            string prefix = fixture.Create<string>();

            var sut = new PrefixedTemporaryQueueStrategy(prefix);

            var value = sut.GetQueueName();

            Assert.That(value, Is.StringEnding("?temporary=true"));
        }
    }
}