using NUnit.Framework;
using Nybus.Configuration;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class TemporaryQueueStrategyTests
    {
        private IFixture fixture;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();
        }

        private TemporaryQueueStrategy CreateSystemUnderTest()
        {
            return new TemporaryQueueStrategy();
        }

        [Test]
        public void GetQueueName_returns_a_temporary_random_name()
        {
            var sut = CreateSystemUnderTest();

            var value = sut.GetQueueName();

            Assert.That(value, Is.EqualTo(TemporaryQueueStrategy.RabbitMqTemporaryQueueName));
        }
    }
}