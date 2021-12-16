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

        public IReadOnlyDictionary<string, ObservableConsumer> Consumers => _consumers;

        public Task<IObservable<Message>> StartAsync()
        {
            _connection = _configuration.ConnectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            if (_configuration.UnackedMessageCountLimit.HasValue)
            {
                _channel.BasicQos(0, _configuration.UnackedMessageCountLimit.Value, true);
            }

            var hasEvents = _messageDescriptorStore.HasEvents();
            var hasCommands = _messageDescriptorStore.HasCommands();

            if (!hasEvents && !hasCommands)
            {
                return Task.FromResult(Observable.Never<Message>());
            }

            var queueToConsume = new List<string>();

            if (hasEvents)
            {
                var eventQueue = _configuration.EventQueueFactory.CreateQueue(_channel);

                foreach (var type in _messageDescriptorStore.Events)
                {
                    var exchangeName = MessageDescriptor.CreateFromType(type);

                    _configuration.EventExchangeManager.EnsureExchangeExists(_channel, exchangeName, FanOutExchangeType);

                    _channel.QueueBind(queue: eventQueue.QueueName, exchange: exchangeName, routingKey: string.Empty);
                }

                queueToConsume.Add(eventQueue.QueueName);
            }

            if (hasCommands)
            {
                var commandQueue = _configuration.CommandQueueFactory.CreateQueue(_channel);

                foreach (var type in _messageDescriptorStore.Commands)
                {
                    var exchangeName = MessageDescriptor.CreateFromType(type);

                    _configuration.CommandExchangeManager.EnsureExchangeExists(_channel, exchangeName, FanOutExchangeType);

                    _channel.QueueBind(queue: commandQueue.QueueName, exchange: exchangeName, routingKey: string.Empty);
                }

                queueToConsume.Add(commandQueue.QueueName);
            }

            var sequence = from queue in queueToConsume.ToObservable()
                           from args in SubscribeMessages(_channel, queue)
                           let message = GetMessage(args)
                           where message != null
                           select message;

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
                var messageTypeName = args.BasicProperties.GetHeader(Nybus(Headers.MessageType), encoding);

                Message message = null;

                if (!MessageDescriptor.TryParse(messageTypeName, out var descriptor))
                {
                    NackMessage(args.DeliveryTag);
                    return null;
                }

                if (_messageDescriptorStore.FindCommandTypeForDescriptor(descriptor, out var commandType))
                {
                    var command = _configuration.Serializer.DeserializeObject(args.Body, commandType, encoding) as ICommand;
                    message = CreateCommandMessage(command);
                }
                else if (_messageDescriptorStore.FindEventTypeForDescriptor(descriptor, out var eventType))
                {
                    var @event = _configuration.Serializer.DeserializeObject(args.Body, eventType, encoding) as IEvent;
                    message = CreateEventMessage(@event);
                }
                else
                {
                    NackMessage(args.DeliveryTag);
                    return null;
                }

                message.MessageId = args.BasicProperties.GetHeader(Nybus(Headers.MessageId), encoding);
                message.Headers = new HeaderBag
                {
                    [Headers.CorrelationId] = args.BasicProperties.GetHeader(Nybus(Headers.CorrelationId), encoding),
                    [Headers.SentOn] = args.BasicProperties.GetHeader(Nybus(Headers.SentOn), encoding),
                    [Headers.RetryCount] = args.BasicProperties.GetHeader(Nybus(Headers.RetryCount), encoding),
                    [RabbitMqHeaders.DeliveryTag] = args.DeliveryTag.ToString(),
                    [RabbitMqHeaders.MessageId] = args.BasicProperties.MessageId
                };

                foreach (var header in args.BasicProperties.Headers)
                {
                    if (header.Key.StartsWith("Custom:"))
                    {
                        var headerKey = ParseCustom(header.Key);
                        var value = args.BasicProperties.GetHeader(header.Key, encoding);

                        message.Headers.Add(headerKey, value);
                    }
                    else if (header.Key.StartsWith("RabbitMq:") && !message.Headers.ContainsKey(header.Key))
                    {
                        var value = args.BasicProperties.GetHeader(header.Key, encoding);

                        message.Headers.Add(header.Key, value);
                    }
                }

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
        }

        public Task StopAsync()
        {
            _channel.Dispose();
            _connection.Dispose();

            return Task.CompletedTask;
        }

        public void SubscribeToCommand<TCommand>()
            where TCommand : class, ICommand
        {
            _messageDescriptorStore.RegisterCommandType<TCommand>();
        }

        public void SubscribeToEvent<TEvent>()
            where TEvent : class, IEvent
        {
            _messageDescriptorStore.RegisterEventType<TEvent>();
        }

        public Task SendMessageAsync(Message message)
        {
            var type = message.Type;

            var body = _configuration.Serializer.SerializeObject(message.Item, _configuration.OutboundEncoding);

            var properties = GetBasicProperties(message);

            var exchangeName = MessageDescriptor.CreateFromType(type);

            var exchangeManager = GetExchangeManager();
            exchangeManager.EnsureExchangeExists(_channel, exchangeName, FanOutExchangeType);

            _channel.BasicPublish(exchange: exchangeName, routingKey: string.Empty, body: body, basicProperties: properties);

            return Task.CompletedTask;

            IExchangeManager GetExchangeManager()
            {
                if (message.MessageType == MessageType.Command)
                {
                    return _configuration.CommandExchangeManager;
                }

                if (message.MessageType == MessageType.Event)
                {
                    return _configuration.EventExchangeManager;
                }

                throw new NotSupportedException();
            }
        }

        public Task NotifySuccessAsync(Message message)
        {
            if (message.Headers.TryGetValue(RabbitMqHeaders.DeliveryTag, out var headerValue) && ulong.TryParse(headerValue, out var deliveryTag))
            {
                try
                {
                    AckMessage(deliveryTag);
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

        private void AckMessage(ulong deliveryTag)
        {
            _channel.BasicAck(deliveryTag, false);
            _processingMessages.TryRemoveItem(deliveryTag);
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

        public Task SendMessageToErrorQueueAsync(Message message)
        {
            var errorQueue = _configuration.ErrorQueueFactory.CreateQueue(_channel);
            
            var body = _configuration.Serializer.SerializeObject(message.Item, _configuration.OutboundEncoding);

            var properties = GetBasicProperties(message);

            _channel.BasicPublish(exchange: string.Empty, routingKey: errorQueue.QueueName, body: body, basicProperties: properties);

            return Task.CompletedTask;
        }

        private IBasicProperties GetBasicProperties(Message message)
        {
            var properties = _channel.CreateBasicProperties();
            properties.ContentEncoding = _configuration.OutboundEncoding.WebName;

            properties.Headers = new Dictionary<string, object>
            {
                [Nybus(Headers.MessageId)] = message.MessageId,
                [Nybus(Headers.MessageType)] = message.Descriptor.ToString()
            };

            foreach (var header in message.Headers)
            {
                if (Headers.IsNybus(header.Key))
                {
                    properties.Headers.Add(Nybus(header.Key), header.Value);
                }
                else if (RabbitMqHeaders.IsRabbitMq(header.Key))
                {
                    properties.Headers.Add(header.Key, header.Value);
                }
                else
                {
                    properties.Headers.Add(Custom(header.Key), header.Value);
                }
            }

            return properties;
        }

        private void NackMessage(ulong deliveryTag)
        {
            _channel.BasicNack(deliveryTag, false, false);
            _processingMessages.TryRemoveItem(deliveryTag);
        }

        private static string Nybus(string key) => $"Nybus:{key}";

        private static string Custom(string key) => $"Custom:{key}";

        private static string ParseCustom(string custom) => custom.Substring(custom.IndexOf(':') + 1);
    }
}
