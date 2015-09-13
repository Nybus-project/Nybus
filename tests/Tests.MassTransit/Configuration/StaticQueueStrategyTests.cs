using NUnit.Framework;
using Nybus.Configuration;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class StaticQueueStrategyTests
    {
        private IFixture fixture;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();
        }

        [Test, ExpectedException]
        public void QueueName_is_required()
        {
            new StaticQueueStrategy(null);
        }

        [Test]
        public void GetQueueName_returns_a_temporary_random_name()
        {
            string queueName = fixture.Create<string>();

            var sut = new StaticQueueStrategy(queueName);

            var value = sut.GetQueueName();

            Assert.That(value, Is.EqualTo(queueName));
        }
    }
}