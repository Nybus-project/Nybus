using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Nybus.Configuration
{
    public interface IConnectionFactoryProvider
    {
        IConnectionFactory CreateFactory(IConfigurationSection settings);
    }

    public static class DefaultConnectionFactoryProviders
    {
        public static readonly IConnectionFactoryProvider ConnectionString = new ConnectionStringConnectionFactoryProvider();
        public static readonly IConnectionFactoryProvider ConnectionNode = new ConnectionNodeConnectionFactoryProvider();
    }

    public class ConnectionStringConnectionFactoryProvider : IConnectionFactoryProvider
    {
        public IConnectionFactory CreateFactory(IConfigurationSection settings)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ConnectionNodeConnectionFactoryProvider : IConnectionFactoryProvider
    {
        public IConnectionFactory CreateFactory(IConfigurationSection settings)
        {
            return new ConnectionFactory
            {
                HostName = settings.GetValue<string>("HostName") ?? "localhost",
                UserName = settings.GetValue<string>("UserName") ?? "guest",
                Password = settings.GetValue<string>("Password") ?? "guest",
                VirtualHost = settings.GetValue<string>("VirtualHost") ?? "/"
            };
        }
    }
}