using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nybus.Utils;
using RabbitMQ.Client;

namespace Nybus.Configuration
{
    public interface IConfigurationFactory
    {
        IConfiguration Create(RabbitMqOptions options);
    }

    public class RabbitMqOptions
    {
        public IConfigurationSection ConnectionString { get; set; }

        public IConfigurationSection Connection { get; set; }

        public string OutboundEncoding { get; set; }

        public IConfigurationSection CommandQueue { get; set; }

        public IConfigurationSection EventQueue { get; set; }
    }

    public class ConfigurationFactory : IConfigurationFactory
    {
        private readonly ILogger<ConfigurationFactory> _logger;
        private readonly IReadOnlyDictionary<string, IQueueFactoryProvider> _queueFactoryProviders;

        public ConfigurationFactory(IEnumerable<IQueueFactoryProvider> queueFactoryProviders, ILogger<ConfigurationFactory> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queueFactoryProviders = queueFactoryProviders?.ToDictionary(k => k.ProviderName, StringComparer.OrdinalIgnoreCase) ?? throw new ArgumentNullException(nameof(queueFactoryProviders));
        }

        public IConfiguration Create(RabbitMqOptions options)
        {
            return new RabbitMqConfiguration
            {
                OutboundEncoding = GetOutboundEncoding(),
                CommandQueueFactory = GetQueueFactory(options.CommandQueue),
                EventQueueFactory = GetQueueFactory(options.EventQueue),
                ConnectionFactory = GetConnectionFactory()
            };

            IQueueFactory GetQueueFactory(IConfigurationSection section)
            {
                if (section != null && section.TryGetValue("ProviderName", out var providerName) && _queueFactoryProviders.TryGetValue(providerName, out var provider))
                {
                    return provider.CreateFactory(section);
                }

                return TemporaryQueueFactory.Instance;
            }

            Encoding GetOutboundEncoding()
            {
                if (options.OutboundEncoding != null)
                {
                    try
                    {
                        var encoding = Encoding.GetEncoding(options.OutboundEncoding);

                        return encoding;
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.LogError(new{outboundEncoding = options.OutboundEncoding}, ex,  (s,e) =>$"Encoding not valid: {s.outboundEncoding}");
                        throw new ConfigurationException($"Encoding not valid: {options.OutboundEncoding}", ex);
                    }
                }

                return Encoding.UTF8;
            }

            IConnectionFactory GetConnectionFactory()
            {
                if (options.ConnectionString.Exists())
                {
                    return DefaultConnectionFactoryProviders.ConnectionString.CreateFactory(options.ConnectionString);
                }

                if (options.Connection.Exists())
                {
                    return DefaultConnectionFactoryProviders.ConnectionNode.CreateFactory(options.Connection);
                }

                return new ConnectionFactory { HostName = "localhost" };
            }
        }
    }

    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
            
        }

        public ConfigurationException(string message) : base (message)
        {
            
        }
    }
}