using System;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Nybus.Configuration
{
    public interface IConnectionFactoryProvider
    {
        IConnectionFactory CreateFactory(IConfigurationSection settings);
    }

    public interface IConnectionFactoryProviders
    {
        IConnectionFactoryProvider ConnectionString { get; }

        IConnectionFactoryProvider ConnectionNode { get; }
    }

    public class ConnectionFactoryProviders : IConnectionFactoryProviders
    {
        public IConnectionFactoryProvider ConnectionString { get; } = new ConnectionStringConnectionFactoryProvider();

        public IConnectionFactoryProvider ConnectionNode { get; } = new ConnectionNodeConnectionFactoryProvider();
    }

    public class ConnectionStringConnectionFactoryProvider : IConnectionFactoryProvider
    {
        public IConnectionFactory CreateFactory(IConfigurationSection settings)
        {
            var connectionStringBuilder = new DbConnectionStringBuilder
            {
                ConnectionString = settings.Value ?? throw new ArgumentNullException("ConnectionString", "ConnectionString value is missing")
            };

            return new ConnectionFactory
            {
                HostName = GetValueOrNull(connectionStringBuilder, "Hostname") ?? "localhost",
                UserName = GetValueOrNull(connectionStringBuilder, "Username") ?? "guest",
                Password = GetValueOrNull(connectionStringBuilder, "Password") ?? "guest",
                VirtualHost = GetValueOrNull(connectionStringBuilder, "VirtualHost") ?? "/",
            };
        }

        private string GetValueOrNull(DbConnectionStringBuilder connectionStringBuilder, string key)
        {
            if (connectionStringBuilder.ContainsKey(key))
            {
                return connectionStringBuilder[key] as string;
            }

            return null;
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