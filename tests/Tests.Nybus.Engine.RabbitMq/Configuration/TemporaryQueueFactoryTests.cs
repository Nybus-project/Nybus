using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Nybus.Configuration;
using RabbitMQ.Client;

namespace Tests.Configuration
{
    [TestFixture]
    public class TemporaryQueueFactoryTests
    {
        [Test, CustomAutoMoqData]
        public void Model_is_required_when_creating_a_queue(TemporaryQueueFactory sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.CreateQueue(null));
        }

        [Test, CustomAutoMoqData]
        public void Created_queue_has_no_name(TemporaryQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(string.Empty, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, CustomAutoMoqData]
        public void Created_queue_is_durable(TemporaryQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(It.IsAny<string>(), true, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, CustomAutoMoqData]
        public void Created_queue_is_exclusive(TemporaryQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), true, It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, CustomAutoMoqData]
        public void Created_queue_is_auto_delete(TemporaryQueueFactory sut, IModel model)
        {
            sut.CreateQueue(model);

            Mock.Get(model).Verify(p => p.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), true, It.IsAny<IDictionary<string, object>>()));
        }

    }
}