using Moq;
using NUnit.Framework;
using Nybus.Configuration;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class RabbitMqServiceBusFactoryTests
    {
        private IFixture fixture;

        private Mock<IQueueStrategy> mockQueueStrategy;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();

            mockQueueStrategy = new Mock<IQueueStrategy>();
        }

        private RabbitMqServiceBusFactory CreateSystemUnderTest()
        {
            return new RabbitMqServiceBusFactory(1);
        }
    }
}