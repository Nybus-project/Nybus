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
    public class RabbitMQBusEngine : IBusEngine
    {
        private readonly HashSet<Type> _acceptedEventTypes = new HashSet<Type>();
        private readonly HashSet<Type> _acceptedCommandTypes = new HashSet<Type>();
        private readonly RabbitMqBusEngineOptions _options;

        public RabbitMQBusEngine(RabbitMqBusEngineOptions options)
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

            var eventQueue = _channel.QueueDeclare();

            foreach (var type in _acceptedEventTypes)
            {
                var exchangeName = GetExchangeNameForType(type);

                _channel.ExchangeDeclare(exchange: exchangeName, type: "fanout");

                _channel.ExchangeBind(destination: eventQueue.QueueName, source: exchangeName, routingKey: string.Empty);
            }

            var commandQueue = _channel.QueueDeclare(queue: _options.CommandQueueName, durable: true, exclusive: false, autoDelete: false);

            foreach (var type in _acceptedCommandTypes)
            {
                var exchangeName = GetExchangeNameForType(type);

                _channel.ExchangeDeclare(exchange: exchangeName, type: "fanout");

                _channel.ExchangeBind(destination: commandQueue.QueueName, source: exchangeName, routingKey: string.Empty);
            }

            //var asyncConsumer = new AsyncEventingBasicConsumer(_channel);
            //var asyncObservable = Observable.FromEvent<AsyncEventHandler<BasicDeliverEventArgs>, BasicDeliverEventArgs>(a => asyncConsumer.Received += a, a => asyncConsumer.Received -= a);

            var consumer = new EventingBasicConsumer(_channel);
            var observable = Observable.FromEventPattern<BasicDeliverEventArgs>(a => consumer.Received += a, a => consumer.Received -= a).Select(e => e.EventArgs);

            var messages = from incoming in observable
                           let message = GetMessage(incoming)
                           where message != null
                           select message;

            _channel.BasicConsume(eventQueue.QueueName, false, consumer);
            _channel.BasicConsume(commandQueue.QueueName, false, consumer);

            return messages;

            Message GetMessage(BasicDeliverEventArgs args)
            {
                var body = Encoding.GetEncoding(args.BasicProperties.ContentEncoding).GetString(args.Body);
                var messageTypeName = args.BasicProperties.Headers["Nybus:MessageType"] as string;

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

                message.MessageId = args.BasicProperties.Headers["Nybus:MessageId"] as string;
                message.Headers[Headers.CorrelationId] = args.BasicProperties.Headers[Nybus(Headers.CorrelationId)] as string;
                message.Headers[Headers.SentOn] = args.BasicProperties.Headers[Nybus(Headers.SentOn)] as string;
                message.Headers[Headers.RetryCount] = args.BasicProperties.Headers[Nybus(Headers.RetryCount)] as string;
                message.Headers[RabbitMqHeaders.DeliveryTag] = args.DeliveryTag.ToString();
                message.Headers[RabbitMqHeaders.MessageId] = args.BasicProperties.MessageId;

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
        }

        public void Stop()
        {
            _channel.Dispose();
            _connection.Dispose();
        }

        public Task SendCommandAsync<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand
        {
            throw new NotImplementedException();
        }

        public Task SendEventAsync<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent
        {
            throw new NotImplementedException();
        }

        public void SubscribeToCommand<TCommand>() where TCommand : class, ICommand
        {
            _acceptedCommandTypes.Add(typeof(TCommand));
        }

        public void SubscribeToEvent<TEvent>() where TEvent : class, IEvent
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

    public static class RabbitMqHeaders
    {
        public static readonly string MessageId = "RabbitMq:MessageId";
        public static readonly string DeliveryTag = "RabbitMq:DeliveryTag";
    }
}
