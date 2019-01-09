using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nybus.Configuration;
using Nybus.Utils;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Nybus.RabbitMq
{
    public class RabbitMqBusEngine : IBusEngine
    {
        private const string FanOutExchangeType = "fanout";
        private readonly ConcurrentList<ulong> _processingMessages = new ConcurrentList<ulong>();
        private readonly ILogger<RabbitMqBusEngine> _logger;
        private readonly Dictionary<string, ObservableConsumer> _consumers = new Dictionary<string, ObservableConsumer>(StringComparer.OrdinalIgnoreCase);
        private readonly IRabbitMqConfiguration _configuration;
        private readonly IMessageDescriptorStore _messageDescriptorStore;

        public RabbitMqBusEngine(IRabbitMqConfiguration configuration, IMessageDescriptorStore messageDescriptorStore, ILogger<RabbitMqBusEngine> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _messageDescriptorStore = messageDescriptorStore ?? throw new ArgumentNullException(nameof(messageDescriptorStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private IConnection _connection;
        private IModel _channel;

        public ISet<Type> AcceptedEventTypes { get; } = new HashSet<Type>();
        public ISet<Type> AcceptedCommandTypes { get; } = new HashSet<Type>();

        public IReadOnlyDictionary<string, ObservableConsumer> Consumers => _consumers;

        public Task<IObservable<Message>> StartAsync()
        {
            _connection = _configuration.ConnectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            var hasEvents = AcceptedEventTypes.Any();
            var hasCommands = AcceptedCommandTypes.Any();

            if (!hasEvents && !hasCommands)
            {
                return Task.FromResult(Observable.Never<Message>());
            }

            var queueToConsume = new List<string>();

            if (hasEvents)
            {
                var eventQueue = _configuration.EventQueueFactory.CreateQueue(_channel);

                foreach (var type in AcceptedEventTypes)
                {
                    var exchangeName = GetExchangeNameForType(type);

                    _channel.ExchangeDeclare(exchange: exchangeName, type: FanOutExchangeType);

                    _channel.QueueBind(queue: eventQueue.QueueName, exchange: exchangeName, routingKey: string.Empty);
                }

                queueToConsume.Add(eventQueue.QueueName);
            }

            if (hasCommands)
            {
                var commandQueue = _configuration.CommandQueueFactory.CreateQueue(_channel);

                foreach (var type in AcceptedCommandTypes)
                {
                    var exchangeName = GetExchangeNameForType(type);

                    _channel.ExchangeDeclare(exchange: exchangeName, type: FanOutExchangeType);

                    _channel.QueueBind(queue: commandQueue.QueueName, exchange: exchangeName, routingKey: string.Empty);
                }

                queueToConsume.Add(commandQueue.QueueName);
            }


            var sequence = Observable.Defer(() => from queue in queueToConsume.ToObservable()
                                                  from args in SubscribeMessages(_channel, queue)
                                                  let message = GetMessage(args)
                                                  where message != null
                                                  select message);

            return Task.FromResult(sequence);

            IObservable<BasicDeliverEventArgs> SubscribeMessages(IModel channel, string queueName)
            {
                var consumer = new ObservableConsumer(channel);
                consumer.ConsumeFrom(queueName);

                _consumers.Add(queueName, consumer);

                return consumer;
            }
                
            Message GetMessage(BasicDeliverEventArgs args)
            {
                _logger.LogTrace($"Received message {args.DeliveryTag}");

                _processingMessages.Add(args.DeliveryTag);

                var encoding = Encoding.GetEncoding(args.BasicProperties.ContentEncoding);
                var messageTypeName = GetHeader(args.BasicProperties, Nybus(Headers.MessageType), encoding);

                Message message = null;

                if (!MessageDescriptor.TryParse(messageTypeName, out var descriptor))
                {
                    NackMessage(args.DeliveryTag);
                    return null;
                }

                if (TryFindCommandByName(descriptor, out var commandType) || _messageDescriptorStore.TryGetTypeForDescriptor(descriptor, out commandType))
                {
                    var command = _configuration.Serializer.DeserializeObject(args.Body, commandType, encoding) as ICommand;
                    message = CreateCommandMessage(command);
                }
                else if (TryFindEventByName(descriptor, out var eventType) || _messageDescriptorStore.TryGetTypeForDescriptor(descriptor, out eventType))
                {
                    var @event = _configuration.Serializer.DeserializeObject(args.Body, eventType, encoding) as IEvent;
                    message = CreateEventMessage(@event);
                }
                else
                {
                    NackMessage(args.DeliveryTag);
                    return null;
                }

                message.MessageId = GetHeader(args.BasicProperties, Nybus(Headers.MessageId), encoding);
                message.Headers = new HeaderBag
                {
                    [Headers.CorrelationId] = GetHeader(args.BasicProperties, Nybus(Headers.CorrelationId), encoding),
                    [Headers.SentOn] = GetHeader(args.BasicProperties, Nybus(Headers.SentOn), encoding),
                    [Headers.RetryCount] = GetHeader(args.BasicProperties, Nybus(Headers.RetryCount), encoding),
                    [RabbitMqHeaders.DeliveryTag] = args.DeliveryTag.ToString(),
                    [RabbitMqHeaders.MessageId] = args.BasicProperties.MessageId
                };

                return message;
            }

            CommandMessage CreateCommandMessage(ICommand command)
            {
                var messageType = typeof(CommandMessage<>).MakeGenericType(command.GetType());
                var message = Activator.CreateInstance(messageType) as CommandMessage;

                message?.SetCommand(command);

                return message;
            }

            EventMessage CreateEventMessage(IEvent @event)
            {
                var messageType = typeof(EventMessage<>).MakeGenericType(@event.GetType());
                var message = Activator.CreateInstance(messageType) as EventMessage;

                message?.SetEvent(@event);

                return message;
            }

            bool TryFindCommandByName(MessageDescriptor descriptor, out Type type) => TryFindTypeByName(AcceptedCommandTypes, descriptor, out type);

            bool TryFindEventByName(MessageDescriptor descriptor, out Type type) => TryFindTypeByName(AcceptedEventTypes, descriptor, out type);

            bool TryFindTypeByName(ISet<Type> typeList, MessageDescriptor descriptor, out Type type)
            {
                type = typeList.FirstOrDefault(o => string.Equals(o.Name, descriptor.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(o.Namespace, descriptor.Namespace, StringComparison.OrdinalIgnoreCase));
                return type != null;
            }

            string GetHeader(IBasicProperties properties, string headerName, Encoding encoding)
            {
                if (properties.Headers.TryGetValue(headerName, out var value) && value is byte[] bytes)
                {
                    return encoding.GetString(bytes);
                }

                return null;
            }
        }

        public Task StopAsync()
        {
            _channel.Dispose();
            _connection.Dispose();

            return Task.CompletedTask;
        }

        public Task SendCommandAsync<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand 
            => SendItemAsync(message, message.Command);

        public Task SendEventAsync<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent 
            => SendItemAsync(message, message.Event);

        private Task SendItemAsync<T>(Message message, T item)
        {
            var type = typeof(T);

            var body = _configuration.Serializer.SerializeObject(item, _configuration.OutboundEncoding);

            var properties = _channel.CreateBasicProperties();
            properties.ContentEncoding = _configuration.OutboundEncoding.WebName;

            properties.Headers = new Dictionary<string, object>
            {
                [Nybus(Headers.MessageId)] = message.MessageId,
                [Nybus(Headers.MessageType)] = message.Descriptor.ToString()
            };

            foreach (var header in message.Headers)
            {
                properties.Headers.Add(Nybus(header.Key), header.Value);
            }

            var exchangeName = GetExchangeNameForType(type);

            _channel.ExchangeDeclare(exchange: exchangeName, type: FanOutExchangeType);

            _channel.BasicPublish(exchange: exchangeName, routingKey: string.Empty, body: body, basicProperties: properties);

            return Task.CompletedTask;

        }

        public void SubscribeToCommand<TCommand>()
            where TCommand : class, ICommand
        {
            _messageDescriptorStore.RegisterType(typeof(TCommand));
            AcceptedCommandTypes.Add(typeof(TCommand));
        }

        public void SubscribeToEvent<TEvent>()
            where TEvent : class, IEvent
        {
            _messageDescriptorStore.RegisterType(typeof(TEvent));
            AcceptedEventTypes.Add(typeof(TEvent));
        }

        public Task NotifySuccessAsync(Message message)
        {
            if (message.Headers.TryGetValue(RabbitMqHeaders.DeliveryTag, out var headerValue) && ulong.TryParse(headerValue, out var deliveryTag))
            {
                try
                {
                    _channel.BasicAck(deliveryTag, false);
                    _processingMessages.TryRemoveItem(deliveryTag);
                }
                catch (AlreadyClosedException ex)
                {
                    var state = new
                    {
                        message.MessageId,
                        DeliveryTag = deliveryTag
                    };

                    _logger.LogWarning(state, ex, (s,e) => $"Unable to ack message {s.MessageId} (delivery tag: {s.DeliveryTag})");
                }
            }
            
            return Task.CompletedTask;
        }

        public Task NotifyFailAsync(Message message)
        {
            if (message.Headers.TryGetValue(RabbitMqHeaders.DeliveryTag, out var headerValue) && ulong.TryParse(headerValue, out var deliveryTag))
            {
                try
                {
                    NackMessage(deliveryTag);
                }
                catch (AlreadyClosedException ex)
                {
                    var state = new
                    {
                        message.MessageId,
                        DeliveryTag = deliveryTag
                    };

                    _logger.LogWarning(state, ex, (s, e) => $"Unable to ack message {s.MessageId} (delivery tag: {s.DeliveryTag})");
                }
            }

            return Task.CompletedTask;
        }

        private void NackMessage(ulong deliveryTag)
        {
            _channel.BasicNack(deliveryTag, false, true);
            _processingMessages.TryRemoveItem(deliveryTag);
        }

        private static string GetExchangeNameForType(Type type) => type.FullName; //$"{type.Namespace}:{type.Name}";

        private static string Nybus(string key) => $"Nybus:{key}";
    }
}
