using System.Text;
using RabbitMQ.Client;

namespace Nybus.Configuration
{
    public interface IConfiguration
    {
        IConnectionFactory ConnectionFactory { get; }

        IQueueFactory CommandQueueFactory { get; }

        IQueueFactory EventQueueFactory { get; }

        Encoding OutboundEncoding { get; }

        ISerializer Serializer { get; }
    }

    public class RabbitMqConfiguration : IConfiguration
    {
        public IConnectionFactory ConnectionFactory { get; set; } = new ConnectionFactory { HostName = "localhost" };

        public IQueueFactory CommandQueueFactory { get; set; } = new TemporaryQueueFactory();

        public IQueueFactory EventQueueFactory { get; set; } = new TemporaryQueueFactory();

        public Encoding OutboundEncoding { get; set; } = Encoding.UTF8;

        public ISerializer Serializer { get; set; } = new JsonSerializer();
    }
}