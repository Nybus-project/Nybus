using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Nybus
{
    public interface IConfiguration
    {
        IConnectionFactory ConnectionFactory { get; }

        IQueueFactory CommandQueueFactory { get; }

        IQueueFactory EventQueueFactory { get; }

        Encoding OutboundEncoding { get; }

        ISerializer Serializer { get; }
    }

    public class RabbitMqBusEngineConfiguration : IConfiguration
    {
        public IConnectionFactory ConnectionFactory { get; set; } = new ConnectionFactory { HostName = "localhost" };

        public IQueueFactory CommandQueueFactory { get; set; }

        public IQueueFactory EventQueueFactory { get; set; } = new TemporaryQueueFactory();

        public Encoding OutboundEncoding { get; set; } = Encoding.UTF8;

        public ISerializer Serializer { get; set; } = new JsonSerializer();
    }

    public interface IQueueFactory
    {
        QueueDeclareOk CreateQueue(IModel model);
    }

    public class StaticQueueFactory : IQueueFactory
    {
        private readonly string _queueName;

        public StaticQueueFactory(string queueName)
        {
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        }

        public QueueDeclareOk CreateQueue(IModel model)
        {
            return model.QueueDeclare(_queueName, durable: Durable, exclusive: Exclusive, autoDelete: AutoDelete);
        }

        public bool Durable { get; } = true;

        public bool Exclusive { get; } = false;

        public bool AutoDelete { get; } = false;
    }

    public class TemporaryQueueFactory : IQueueFactory
    {
        public QueueDeclareOk CreateQueue(IModel model)
        {
            return model.QueueDeclare(durable: Durable);
        }

        public bool Durable { get; } = true;
    }

    public interface ISerializer
    {
        byte[] SerializeObject(object item, Encoding encoding);

        object DeserializeObject(byte[] bytes, Type type, Encoding encoding);
    }

    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        public JsonSerializer() : this(new JsonSerializerSettings())
        {
            
        }

        public JsonSerializer(JsonSerializerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public byte[] SerializeObject(object item, Encoding encoding)
        {
            var json = JsonConvert.SerializeObject(item, _settings);
            return encoding.GetBytes(json);
        }

        public object DeserializeObject(byte[] bytes, Type type, Encoding encoding)
        {
            var json = encoding.GetString(bytes);
            return JsonConvert.DeserializeObject(json, type, _settings);
        }
    }

}