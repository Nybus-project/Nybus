using System;
using Microsoft.Extensions.Configuration;
using Nybus.Utils;
// ReSharper disable ClassNeverInstantiated.Global

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

            // ReSharper disable once NotResolvedInText
            throw new ArgumentNullException("QueueName", "QueueName setting is required");
        }
    }

    public class TemporaryQueueFactoryProvider : IQueueFactoryProvider
    {
        public string ProviderName { get; } = "temporary";

        public IQueueFactory CreateFactory(IConfigurationSection settings) => TemporaryQueueFactory.Instance;
    }

    public class PrefixedTemporaryQueueFactoryProvider : IQueueFactoryProvider
    {
        public string ProviderName { get; } = "prefix";

        public IQueueFactory CreateFactory(IConfigurationSection settings)
        {
            if (settings.TryGetValue("Prefix", out var prefix))
            {
                return new PrefixedTemporaryQueueFactory(prefix);
            }

            // ReSharper disable once NotResolvedInText
            throw new ArgumentNullException("Prefix", "Prefix setting is required");
        }
    }
}