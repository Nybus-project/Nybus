using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Nybus.Configuration;
using RabbitMQ.Client;

namespace Tests.Configuration
{

    [TestFixture]
    public class RabbitMqConfigurationTests
    {
        [Test, AutoMoqData]
        public void Serializer_can_be_assigned(RabbitMqConfiguration sut, ISerializer serializer)
        {
            sut.Serializer = serializer;

            Assert.That(sut.Serializer, Is.SameAs(serializer));
        }

        [Test, AutoMoqData]
        public void Serializer_can_be_accessed(RabbitMqConfiguration sut)
        {
            Assert.That(sut.Serializer, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public void ConnectionFactory_can_be_assigned(RabbitMqConfiguration sut, IConnectionFactory connectionFactory)
        {
            sut.ConnectionFactory = connectionFactory;

            Assert.That(sut.ConnectionFactory, Is.SameAs(connectionFactory));
        }

        [Test, AutoMoqData]
        public void ConnectionFactory_can_be_accessed(RabbitMqConfiguration sut)
        {
            Assert.That(sut.ConnectionFactory, Is.Not.Null);
        }
    }
}
