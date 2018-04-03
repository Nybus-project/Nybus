using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Nybus
{
    public class RabbitMqBusEngine : IBusEngine
    {
        private readonly HashSet<Type> _acceptedEventTypes = new HashSet<Type>();
        private readonly HashSet<Type> _acceptedCommandTypes = new HashSet<Type>();
        private readonly RabbitMqBusEngineOptions _options;

        public RabbitMqBusEngine(RabbitMqBusEngineOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        private IConnection _connection;
        private IModel _channel;

        public IObservable<Message> Start()
        {
            var connectionFactory = new ConnectionFactory() { HostName = "localhost" };
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            var hasEvents = _acceptedEventTypes.Any();
            var hasCommands = _acceptedCommandTypes.Any();

            if (!hasEvents && !hasCommands)
            {
                return Observable.Never<Message>();
            }

            var observableConsumer = new ObservableConsumer(_channel);

            var messages = from incoming in observableConsumer
                           let message = GetMessage(incoming)
                           where message != null
                           select message;

            if (hasEvents)
            {
                var eventQueue = _channel.QueueDeclare();

                foreach (var type in _acceptedEventTypes)
                {
                    var exchangeName = GetExchangeNameForType(type);

                    _channel.ExchangeDeclare(exchange: exchangeName, type: "fanout");

                    _channel.ExchangeBind(destination: eventQueue.QueueName, source: exchangeName, routingKey: string.Empty);
                }

                observableConsumer.ConsumeFrom(eventQueue.QueueName);
            }

            if (hasCommands)
            {
                var commandQueue = _channel.QueueDeclare(queue: _options.CommandQueueName, durable: true, exclusive: false, autoDelete: false);

                foreach (var type in _acceptedCommandTypes)
                {
                    var exchangeName = GetExchangeNameForType(type);

                    _channel.ExchangeDeclare(exchange: exchangeName, type: "fanout");

                    _channel.QueueBind(queue: commandQueue.QueueName, exchange: exchangeName, routingKey: string.Empty);
                }

                observableConsumer.ConsumeFrom(commandQueue.QueueName);
            }

            return messages;

            Message GetMessage(BasicDeliverEventArgs args)
            {
                var encoding = Encoding.GetEncoding(args.BasicProperties.ContentEncoding);
                var body = encoding.GetString(args.Body);
                var messageTypeName = GetHeader(args.BasicProperties, "Nybus:MessageType", encoding);

                Message message = null;

                if (TryFindCommandByName(messageTypeName, out var commandType))
                {
                    var command = JsonConvert.DeserializeObject(body, commandType) as ICommand;
                    message = CreateCommandMessage(command);
                }
                else if (TryFindEventByName(messageTypeName, out var eventType))
                {
                    var @event = JsonConvert.DeserializeObject(body, eventType) as IEvent;
                    message = CreateEventMessage(@event);
                }
                else
                {
                    _channel.BasicNack(args.DeliveryTag, false, true);
                    return null;
                }

                message.MessageId = GetHeader(args.BasicProperties, "Nybus:MessageId", encoding);
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
                var message = Activator.CreateInstance(messageType);

                messageType.GetProperty("Command").SetValue(message, command);

                return message as CommandMessage;
            }

            EventMessage CreateEventMessage(IEvent @event)
            {
                var messageType = typeof(EventMessage<>).MakeGenericType(@event.GetType());
                var message = Activator.CreateInstance(messageType);

                messageType.GetProperty("Event").SetValue(message, @event);

                return message as EventMessage;
            }

            bool TryFindCommandByName(string commandTypeName, out Type type) => TryFindTypeByName(_acceptedCommandTypes, commandTypeName, out type);

            bool TryFindEventByName(string eventTypeName, out Type type) => TryFindTypeByName(_acceptedEventTypes, eventTypeName, out type);

            bool TryFindTypeByName(IEnumerable<Type> typeList, string typeName, out Type type)
            {
                type = typeList.FirstOrDefault(o => o.FullName == typeName);
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

        public void Stop()
        {
            _channel.Dispose();
            _connection.Dispose();
        }

        public Task SendCommandAsync<TCommand>(CommandMessage<TCommand> message)
            where TCommand : class, ICommand
        {
            var serialized = JsonConvert.SerializeObject(message.Command);
            var body = Encoding.UTF8.GetBytes(serialized);

            var properties = _channel.CreateBasicProperties();
            properties.ContentEncoding = Encoding.UTF8.WebName;
            properties.Headers = new Dictionary<string, object>
            {
                ["Nybus:MessageId"] = message.MessageId,
                ["Nybus:MessageType"] = message.CommandType.FullName,
                [Nybus(Headers.CorrelationId)] = message.Headers[Headers.CorrelationId],
                [Nybus(Headers.SentOn)] = message.Headers[Headers.SentOn]
            };

            if (message.Headers.ContainsKey(Headers.RetryCount))
            {
                properties.Headers[Nybus(Headers.RetryCount)] = message.Headers[Headers.RetryCount];
            }

            var exchangeName = GetExchangeNameForType(message.CommandType);

            _channel.ExchangeDeclare(exchange: exchangeName, type: "fanout");

            _channel.BasicPublish(exchange: exchangeName, routingKey: string.Empty, body: body, basicProperties: properties);

            return Task.CompletedTask;
        }

        public Task SendEventAsync<TEvent>(EventMessage<TEvent> message)
            where TEvent : class, IEvent
        {
            var serialized = JsonConvert.SerializeObject(message.Event);
            var body = Encoding.UTF8.GetBytes(serialized);

            var properties = _channel.CreateBasicProperties();
            properties.ContentEncoding = Encoding.UTF8.WebName;
            properties.Headers = new Dictionary<string, object>
            {
                ["Nybus:MessageId"] = message.MessageId,
                ["Nybus:MessageType"] = message.EventType.FullName,
                [Nybus(Headers.CorrelationId)] = message.Headers[Headers.CorrelationId],
                [Nybus(Headers.SentOn)] = message.Headers[Headers.SentOn]
            };

            if (message.Headers.ContainsKey(Headers.RetryCount))
            {
                properties.Headers[Nybus(Headers.RetryCount)] = message.Headers[Headers.RetryCount];
            }

            var exchangeName = GetExchangeNameForType(message.EventType);

            _channel.ExchangeDeclare(exchange: exchangeName, type: "fanout");

            _channel.BasicPublish(exchange: exchangeName, routingKey: string.Empty, body: body, basicProperties: properties);

            return Task.CompletedTask;
        }

        public void SubscribeToCommand<TCommand>()
            where TCommand : class, ICommand
        {
            _acceptedCommandTypes.Add(typeof(TCommand));
        }

        public void SubscribeToEvent<TEvent>()
            where TEvent : class, IEvent
        {
            _acceptedEventTypes.Add(typeof(TEvent));
        }

        public Task NotifySuccess(Message message)
        {
            if (message.Headers.TryGetValue(RabbitMqHeaders.DeliveryTag, out var headerValue) && ulong.TryParse(headerValue, out var deliveryTag))
            {
                _channel.BasicAck(deliveryTag, false);
            }

            return Task.CompletedTask;
        }

        public Task NotifyFail(Message message)
        {
            if (message.Headers.TryGetValue(RabbitMqHeaders.DeliveryTag, out var headerValue) && ulong.TryParse(headerValue, out var deliveryTag))
            {
                _channel.BasicNack(deliveryTag, false, true);
            }

            return Task.CompletedTask;
        }

        private static string GetExchangeNameForType(Type type) => type.FullName;

        private static string Nybus(string key) => $"Nybus:{key}";
    }

    public class RabbitMqBusEngineOptions
    {
        public string CommandQueueName { get; set; }

        public string EventQueueName { get; set; }
    }
}