using Moq;
using NUnit.Framework;
using Nybus.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Tests.Configuration
{
    [TestFixture]
    public class StaticQueueFactoryTests
    {
        [Test]
        public void QueueName_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new StaticQueueFactory(null));
        }

        [Test, AutoMoqData]
        public void Model_is_required_when_creating_a_queue(StaticQueueFactory sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.CreateQueue(null));
        }

        [Test, AutoMoqData]
        public void Created_queue_has_correct_name(StaticQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(sut.QueueName, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, AutoMoqData]
        public void Created_queue_is_durable(StaticQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(It.IsAny<string>(), true, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, AutoMoqData]
        public void Created_queue_is_not_exclusive(StaticQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), false, It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, AutoMoqData]
        public void Created_queue_is_not_auto_delete(StaticQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), false, It.IsAny<IDictionary<string, object>>()));
        }
    }
}
