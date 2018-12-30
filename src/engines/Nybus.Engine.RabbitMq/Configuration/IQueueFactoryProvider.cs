using System;
using Microsoft.Extensions.Configuration;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface IQueueFactoryProvider
    {
        string ProviderName { get; }

        IQueueFactory CreateFactory(IConfigurationSection settings);
    }

    public class StaticQueueFactoryProvider : IQueueFactoryProvider
    {
        public string ProviderName { get; } = "static";

        public IQueueFactory CreateFactory(IConfigurationSection settings)
        {
            if (settings.TryGetValue("QueueName", out var queueName))
            {
                return new StaticQueueFactory(queueName);
            }

            throw new ArgumentNullException("QueueName", "QueueName setting is required");
        }
    }

    public class TemporaryQueueFactoryProvider : IQueueFactoryProvider
    {
        public string ProviderName { get; } = "temporary";

        public IQueueFactory CreateFactory(IConfigurationSection settings) => TemporaryQueueFactory.Instance;
    }
}