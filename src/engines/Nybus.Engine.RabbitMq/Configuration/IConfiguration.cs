using System.Text;
using RabbitMQ.Client;

namespace Nybus.Configuration
{
    public interface IConfiguration
    {
        IConnectionFactory ConnectionFactory { get; set; }

        IQueueFactory CommandQueueFactory { get; set; }

        IQueueFactory EventQueueFactory { get; set; }

        Encoding OutboundEncoding { get; set; }

        ISerializer Serializer { get; set; }
    }

    public class RabbitMqConfiguration : IConfiguration
    {
        public IConnectionFactory ConnectionFactory { get; set; }

        public IQueueFactory CommandQueueFactory { get; set; }

        public IQueueFactory EventQueueFactory { get; set; }

        public Encoding OutboundEncoding { get; set; }

        public ISerializer Serializer { get; set; } = new JsonSerializer();
    }
}