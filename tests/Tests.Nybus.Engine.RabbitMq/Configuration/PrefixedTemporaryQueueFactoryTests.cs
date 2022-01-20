using System.Collections.Generic;
using AutoFixture.Idioms;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;
using RabbitMQ.Client;

namespace Tests.Configuration
{
    [TestFixture]
    public class PrefixedTemporaryQueueFactoryTests
    {
        [Test, CustomAutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(PrefixedTemporaryQueueFactory).GetConstructors());
        }

        [Test, CustomAutoMoqData]
        public void CreateQueue_throws_if_model_is_null(PrefixedTemporaryQueueFactory sut)
        {
            Assert.That(() => sut.CreateQueue(null), Throws.ArgumentNullException);
        }

        [Test, CustomAutoMoqData]
        public void CreateQueue_returns_queue_with_prefixed_name(PrefixedTemporaryQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(It.Is<string>(s => s.StartsWith($"{sut.Prefix}-")), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, CustomAutoMoqData]
        public void CreateQueue_returns_durable_queue(PrefixedTemporaryQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(It.IsAny<string>(), true, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, CustomAutoMoqData]
        public void CreateQueue_returns_exclusive_queue(PrefixedTemporaryQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), true, It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, CustomAutoMoqData]
        public void CreateQueue_returns_autoDelete_queue(PrefixedTemporaryQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), true, It.IsAny<IDictionary<string, object>>()));
        }
    }
}