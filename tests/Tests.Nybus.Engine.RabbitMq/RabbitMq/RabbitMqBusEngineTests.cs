using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Nybus;
using Nybus.Configuration;
using Nybus.RabbitMq;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Framing;

namespace Tests.RabbitMq
{
    [TestFixture]
    public class RabbitMqBusEngineTests
    {
        [Test]
        public void Configuration_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new RabbitMqBusEngine(null, Mock.Of<ILogger<RabbitMqBusEngine>>()));
        }

        [Test]
        public void Logger_is_required()
        {
            Assert.Throws<ArgumentNullException>(() => new RabbitMqBusEngine(Mock.Of<IRabbitMqConfiguration>(), null));
        }

        [Test, AutoMoqData]
        public void SubscribeToCommand_registers_command_type(RabbitMqBusEngine sut)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            Assert.That(sut.AcceptedCommandTypes, Contains.Item(typeof(FirstTestCommand)));
        }

        [Test, AutoMoqData]
        public void SubscribeToCommand_handles_multiple_commands(RabbitMqBusEngine sut)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            sut.SubscribeToCommand<SecondTestCommand>();

            Assert.That(sut.AcceptedCommandTypes, Has.Exactly(2).InstanceOf<Type>());
        }

        [Test, AutoMoqData]
        public void SubscribeToCommand_ignores_multiple_registrations_of_same_command(RabbitMqBusEngine sut)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            sut.SubscribeToCommand<FirstTestCommand>();

            Assert.That(sut.AcceptedCommandTypes, Has.Exactly(1).InstanceOf<Type>());
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_registers_event_type(RabbitMqBusEngine sut)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            Assert.That(sut.AcceptedEventTypes, Contains.Item(typeof(FirstTestEvent)));
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_handles_multiple_events(RabbitMqBusEngine sut)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            sut.SubscribeToEvent<SecondTestEvent>();

            Assert.That(sut.AcceptedEventTypes, Has.Exactly(2).InstanceOf<Type>());
        }

        [Test, AutoMoqData]
        public void SubscribeToEvent_ignores_multiple_registrations_of_same_event(RabbitMqBusEngine sut)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            sut.SubscribeToEvent<FirstTestEvent>();

            Assert.That(sut.AcceptedEventTypes, Has.Exactly(1).InstanceOf<Type>());
        }

        [Test, AutoMoqData]
        public void Empty_sequence_is_returned_if_no_subscription(RabbitMqBusEngine sut)
        {
            var sequence = sut.StartAsync().Result;

            var incomingMessages = sequence.DumpInList();

            Assert.That(incomingMessages, Is.Empty);
        }

        [Test, AutoMoqData]
        public void Commands_can_be_subscribed(RabbitMqBusEngine sut)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = sut.StartAsync().Result;

            var incomingMessages = sequence.DumpInList();

            Assert.That(incomingMessages, Is.Empty);
        }

        [Test, AutoMoqData]
        public void Events_can_be_subscribed(RabbitMqBusEngine sut)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            var sequence = sut.StartAsync().Result;

            var incomingMessages = sequence.DumpInList();

            Assert.That(incomingMessages, Is.Empty);
        }

        [Test, AutoMoqData]
        public void Commands_and_events_can_be_subscribed(RabbitMqBusEngine sut)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = sut.StartAsync().Result;

            var incomingMessages = sequence.DumpInList();

            Assert.That(incomingMessages, Is.Empty);
        }

        [Test, AutoMoqData]
        public void QueueFactory_is_invoked_when_a_event_is_registered([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            var sequence = sut.StartAsync().Result;

            Mock.Get(configuration.EventQueueFactory).Verify(p => p.CreateQueue(It.IsAny<IModel>()));

        }

        [Test, AutoMoqData]
        public void QueueFactory_is_invoked_when_a_command_is_registered([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = sut.StartAsync().Result;

            Mock.Get(configuration.CommandQueueFactory).Verify(p => p.CreateQueue(It.IsAny<IModel>()));

        }

        [Test, AutoMoqData]
        public void Exchange_is_declared_when_a_event_is_registered([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            var sequence = sut.StartAsync().Result;

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.ExchangeDeclare(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, AutoMoqData]
        public void Exchange_is_declared_when_a_command_is_registered([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = sut.StartAsync().Result;

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.ExchangeDeclare(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, AutoMoqData]
        public void Queue_is_bound_to_exchange_when_a_event_is_registered([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            var sequence = sut.StartAsync().Result;

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()));
        }

        [Test, AutoMoqData]
        public void Queue_is_bound_to_exchange_when_a_command_is_registered([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = sut.StartAsync().Result;

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()));
        }


        [Test, AutoMoqData]
        public void Event_consumer_is_exposed_when_sequence_is_subscribed(RabbitMqBusEngine sut)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            var sequence = sut.StartAsync().Result;

            sequence.Subscribe(_ => { }); // subscribes to the sequence but takes no action when items are published

            Assert.That(sut.Consumers.Count, Is.EqualTo(1));
        }

        [Test, AutoMoqData]
        public void Command_consumer_is_exposed_when_sequence_is_subscribed(RabbitMqBusEngine sut)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = sut.StartAsync().Result;

            sequence.Subscribe(_ => { }); // subscribes to the sequence but takes no action when items are published

            Assert.That(sut.Consumers.Count, Is.EqualTo(1));
        }

        [Test, AutoMoqData]
        public void Events_can_be_received([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, string messageId, Guid correlationId, FirstTestEvent @event)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            var sequence = sut.StartAsync().Result;

            var encoding = Encoding.UTF8;

            IBasicProperties properties = new BasicProperties
            {
                MessageId = messageId,
                ContentEncoding = encoding.WebName,
                Headers = new Dictionary<string, object>
                {
                    ["Nybus:MessageId"] = encoding.GetBytes(messageId),
                    ["Nybus:MessageType"] = encoding.GetBytes(@event.GetType().FullName),
                    ["Nybus:CorrelationId"] = correlationId.ToByteArray()
                }
            };

            var body = configuration.Serializer.SerializeObject(@event, encoding);

            var incomingMessages = sequence.DumpInList();

            sut.Consumers.First().Value.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            Assert.That(incomingMessages, Has.Exactly(1).InstanceOf<EventMessage<FirstTestEvent>>());

            var message = incomingMessages[0] as EventMessage<FirstTestEvent>;

            Assert.That(message, Is.Not.Null);
            Assert.That(message.MessageId, Is.EqualTo(messageId));
            Assert.That(message.MessageType, Is.EqualTo(MessageType.Event));
            Assert.That(message.Type, Is.EqualTo(@event.GetType()));
            Assert.That(message.Event, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public void Commands_can_be_received([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, string messageId, Guid correlationId, FirstTestCommand command)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = sut.StartAsync().Result;

            var encoding = Encoding.UTF8;

            IBasicProperties properties = new BasicProperties
            {
                MessageId = messageId,
                ContentEncoding = encoding.WebName,
                Headers = new Dictionary<string, object>
                {
                    ["Nybus:MessageId"] = encoding.GetBytes(messageId),
                    ["Nybus:MessageType"] = encoding.GetBytes(command.GetType().FullName),
                    ["Nybus:CorrelationId"] = correlationId.ToByteArray()
                }
            };

            var body = configuration.Serializer.SerializeObject(command, encoding);

            var incomingMessages = sequence.DumpInList();

            sut.Consumers.First().Value.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            Assert.That(incomingMessages, Has.Exactly(1).InstanceOf<CommandMessage<FirstTestCommand>>());

            var message = incomingMessages[0] as CommandMessage<FirstTestCommand>;

            Assert.That(message, Is.Not.Null);
            Assert.That(message.MessageId, Is.EqualTo(messageId));
            Assert.That(message.MessageType, Is.EqualTo(MessageType.Command));
            Assert.That(message.Type, Is.EqualTo(command.GetType()));
            Assert.That(message.Command, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public void Invalid_events_are_discarded([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, string messageId, Guid correlationId, FirstTestEvent @event)
        {
            // At least one subscription is needed to inject invalid messages
            sut.SubscribeToEvent<SecondTestEvent>();

            var sequence = sut.StartAsync().Result;

            var encoding = Encoding.UTF8;

            IBasicProperties properties = new BasicProperties
            {
                MessageId = messageId,
                ContentEncoding = encoding.WebName,
                Headers = new Dictionary<string, object>
                {
                    ["Nybus:MessageId"] = encoding.GetBytes(messageId),
                    ["Nybus:MessageType"] = encoding.GetBytes(@event.GetType().FullName),
                    ["Nybus:CorrelationId"] = correlationId.ToByteArray()
                }
            };

            var body = configuration.Serializer.SerializeObject(@event, encoding);

            var incomingMessages = sequence.DumpInList();

            sut.Consumers.First().Value.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            Assert.That(incomingMessages, Is.Empty);

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.BasicNack(deliveryTag, It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test, AutoMoqData]
        public void Invalid_commands_are_discarded([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, string messageId, Guid correlationId, FirstTestCommand command)
        {
            // At least one subscription is needed to inject invalid messages
            sut.SubscribeToCommand<SecondTestCommand>();

            var sequence = sut.StartAsync().Result;

            var encoding = Encoding.UTF8;

            IBasicProperties properties = new BasicProperties
            {
                MessageId = messageId,
                ContentEncoding = encoding.WebName,
                Headers = new Dictionary<string, object>
                {
                    ["Nybus:MessageId"] = encoding.GetBytes(messageId),
                    ["Nybus:MessageType"] = encoding.GetBytes(command.GetType().FullName),
                    ["Nybus:CorrelationId"] = correlationId.ToByteArray()
                }
            };

            var body = configuration.Serializer.SerializeObject(command, encoding);

            var incomingMessages = sequence.DumpInList();

            sut.Consumers.First().Value.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            Assert.That(incomingMessages, Is.Empty);

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.BasicNack(deliveryTag, It.IsAny<bool>(), It.IsAny<bool>()));
        }

        [Test, AutoMoqData]
        public async Task Engine_can_be_stopped([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut)
        {
            await sut.StartAsync();

            await sut.StopAsync();

            Mock.Get(configuration.ConnectionFactory.CreateConnection()).Verify(p => p.Dispose());
            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.Dispose());
        }

        [Test, AutoMoqData]
        public async Task Commands_can_be_sent([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, CommandMessage<FirstTestCommand> message)
        {
            await sut.StartAsync().ConfigureAwait(false);

            await sut.SendCommandAsync(message);

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()));
        }

        [Test, AutoMoqData]
        public async Task Arbitrary_headers_are_forwarded_when_sending_commands([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, CommandMessage<FirstTestCommand> message, string headerKey, string headerValue)
        {
            message.Headers.Add(headerKey, headerValue);

            await sut.StartAsync().ConfigureAwait(false);

            await sut.SendCommandAsync(message);

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.Is<IBasicProperties>(o => o.Headers.ContainsKey($"Nybus:{headerKey}")), It.IsAny<byte[]>()));
        }

        [Test, AutoMoqData]
        public async Task Events_can_be_sent([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, EventMessage<FirstTestEvent> message)
        {
            await sut.StartAsync().ConfigureAwait(false);
            
            await sut.SendEventAsync(message);

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()));

        }

        [Test, AutoMoqData]
        public async Task Arbitrary_headers_are_forwarded_when_sending_events([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, EventMessage<FirstTestEvent> message, string headerKey, string headerValue)
        {
            message.Headers.Add(headerKey, headerValue);

            await sut.StartAsync().ConfigureAwait(false);

            await sut.SendEventAsync(message);

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.Is<IBasicProperties>(o => o.Headers.ContainsKey($"Nybus:{headerKey}")), It.IsAny<byte[]>()));
        }

        [Test, AutoMoqData]
        public async Task NotifySuccess_acks_command_messages([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, string messageId, Guid correlationId, FirstTestCommand command)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = await sut.StartAsync().ConfigureAwait(false);

            var encoding = Encoding.UTF8;

            IBasicProperties properties = new BasicProperties
            {
                MessageId = messageId,
                ContentEncoding = encoding.WebName,
                Headers = new Dictionary<string, object>
                {
                    ["Nybus:MessageId"] = encoding.GetBytes(messageId),
                    ["Nybus:MessageType"] = encoding.GetBytes(command.GetType().FullName),
                    ["Nybus:CorrelationId"] = correlationId.ToByteArray()
                }
            };

            var body = configuration.Serializer.SerializeObject(command, encoding);

            var incomingMessages = sequence.DumpInList();

            sut.Consumers.First().Value.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            await sut.NotifySuccessAsync(incomingMessages.First());

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.BasicAck(deliveryTag, It.IsAny<bool>()));
        }

        [Test, AutoMoqData]
        public async Task NotifySuccess_acks_event_messages([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, string messageId, Guid correlationId, FirstTestEvent @event)
        {
            sut.SubscribeToEvent<FirstTestEvent>();

            var sequence = await sut.StartAsync().ConfigureAwait(false);

            var encoding = Encoding.UTF8;

            IBasicProperties properties = new BasicProperties
            {
                MessageId = messageId,
                ContentEncoding = encoding.WebName,
                Headers = new Dictionary<string, object>
                {
                    ["Nybus:MessageId"] = encoding.GetBytes(messageId),
                    ["Nybus:MessageType"] = encoding.GetBytes(@event.GetType().FullName),
                    ["Nybus:CorrelationId"] = correlationId.ToByteArray()
                }
            };

            var body = configuration.Serializer.SerializeObject(@event, encoding);

            var incomingMessages = sequence.DumpInList();

            sut.Consumers.First().Value.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            await sut.NotifySuccessAsync(incomingMessages.First());

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.BasicAck(deliveryTag, It.IsAny<bool>()));
        }

        [Test, AutoMoqData]
        public async Task NotifySuccess_can_handle_closed_connections([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, string messageId, Guid correlationId, FirstTestCommand command, ShutdownEventArgs shutdownEventArgs)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = await sut.StartAsync().ConfigureAwait(false);

            var encoding = Encoding.UTF8;

            IBasicProperties properties = new BasicProperties
            {
                MessageId = messageId,
                ContentEncoding = encoding.WebName,
                Headers = new Dictionary<string, object>
                {
                    ["Nybus:MessageId"] = encoding.GetBytes(messageId),
                    ["Nybus:MessageType"] = encoding.GetBytes(command.GetType().FullName),
                    ["Nybus:CorrelationId"] = correlationId.ToByteArray()
                }
            };

            var body = configuration.Serializer.SerializeObject(command, encoding);

            var incomingMessages = sequence.DumpInList();

            sut.Consumers.First().Value.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Setup(p => p.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>())).Throws(new AlreadyClosedException(shutdownEventArgs));

            await sut.NotifySuccessAsync(incomingMessages.First());

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.BasicAck(deliveryTag, It.IsAny<bool>()));
        }

        [Test, AutoMoqData]
        public async Task NotifyFail_nacks_command_messages([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, string messageId, Guid correlationId, FirstTestCommand command)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = await sut.StartAsync().ConfigureAwait(false);

            var encoding = Encoding.UTF8;

            IBasicProperties properties = new BasicProperties
            {
                MessageId = messageId,
                ContentEncoding = encoding.WebName,
                Headers = new Dictionary<string, object>
                {
                    ["Nybus:MessageId"] = encoding.GetBytes(messageId),
                    ["Nybus:MessageType"] = encoding.GetBytes(command.GetType().FullName),
                    ["Nybus:CorrelationId"] = correlationId.ToByteArray()
                }
            };

            var body = configuration.Serializer.SerializeObject(command, encoding);

            var incomingMessages = sequence.DumpInList();

            sut.Consumers.First().Value.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            await sut.NotifyFailAsync(incomingMessages.First());

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.BasicNack(deliveryTag, It.IsAny<bool>(), true));
        }

        [Test, AutoMoqData]
        public async Task NotifyFail_can_handle_closed_connections([Frozen] IRabbitMqConfiguration configuration, RabbitMqBusEngine sut, string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, string messageId, Guid correlationId, FirstTestCommand command, ShutdownEventArgs shutdownEventArgs)
        {
            sut.SubscribeToCommand<FirstTestCommand>();

            var sequence = await sut.StartAsync().ConfigureAwait(false);

            var encoding = Encoding.UTF8;

            IBasicProperties properties = new BasicProperties
            {
                MessageId = messageId,
                ContentEncoding = encoding.WebName,
                Headers = new Dictionary<string, object>
                {
                    ["Nybus:MessageId"] = encoding.GetBytes(messageId),
                    ["Nybus:MessageType"] = encoding.GetBytes(command.GetType().FullName),
                    ["Nybus:CorrelationId"] = correlationId.ToByteArray()
                }
            };

            var body = configuration.Serializer.SerializeObject(command, encoding);

            var incomingMessages = sequence.DumpInList();

            sut.Consumers.First().Value.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Setup(p => p.BasicNack(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>())).Throws(new AlreadyClosedException(shutdownEventArgs));

            await sut.NotifyFailAsync(incomingMessages.First());

            Mock.Get(configuration.ConnectionFactory.CreateConnection().CreateModel()).Verify(p => p.BasicNack(deliveryTag, It.IsAny<bool>(), It.IsAny<bool>()));
        }
    }
}