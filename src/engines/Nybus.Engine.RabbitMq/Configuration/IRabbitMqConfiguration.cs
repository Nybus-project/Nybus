using System.Text;
using RabbitMQ.Client;

namespace Nybus.Configuration
{
    public interface IRabbitMqConfiguration
    {
        IConnectionFactory ConnectionFactory { get; set; }

        IQueueFactory CommandQueueFactory { get; set; }

        IQueueFactory EventQueueFactory { get; set; }

        Encoding OutboundEncoding { get; set; }

        ISerializer Serializer { get; set; }

        IExchangeManager CommandExchangeManager { get; set; }

        IExchangeManager EventExchangeManager { get; set; }

        ushort? UnackedMessageCountLimit { get; set; }
    }

    public class RabbitMqConfiguration : IRabbitMqConfiguration
    {
        public RabbitMqOptions Options { get; set; }

        public IConnectionFactory ConnectionFactory { get; set; }

        public IQueueFactory CommandQueueFactory { get; set; }

        public IQueueFactory EventQueueFactory { get; set; }

        public Encoding OutboundEncoding { get; set; }

        public ISerializer Serializer { get; set; } = new JsonSerializer();

        public IExchangeManager CommandExchangeManager { get; set; }

        public IExchangeManager EventExchangeManager { get; set; }

        public ushort? UnackedMessageCountLimit { get; set; }

    }
}