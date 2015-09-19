using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;
using Ploeh.AutoFixture;

namespace Tests.Configuration
{
    [TestFixture]
    public class RetryErrorStrategyTests
    {
        private IFixture fixture;

        [SetUp]
        public void Initialize()
        {
            fixture = new Fixture();
        }

        [Test]
        public async Task HandleError_schedules_retry_if_possible()
        {
            var sut = new RetryErrorStrategy(3);

            var mockContext = new Mock<IConsumeContext<TestMessage>>();
            mockContext.SetupGet(p => p.RetryCount).Returns(0);

            Exception error = fixture.Create<Exception>();
            
            bool handled = await sut.HandleError(mockContext.Object, error);

            mockContext.Verify(p => p.RetryLater(), Times.Once);

            Assert.That(handled, Is.True);
        }

        [Test]
        public async Task HandleError_returns_false_if_retry_no_possible()
        {
            var sut = new RetryErrorStrategy(3);

            var mockContext = new Mock<IConsumeContext<TestMessage>>();
            mockContext.SetupGet(p => p.RetryCount).Returns(3);

            Exception error = fixture.Create<Exception>();

            bool handled = await sut.HandleError(mockContext.Object, error);

            Assert.That(handled, Is.False);
        }
    }
}
