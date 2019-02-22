using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Nybus.Configuration
{
    public interface IExchangeManager
    {
        void EnsureExchangeExists(IModel model, string name, string exchangeType);
    }

    public class DefaultExchangeManager : IExchangeManager
    {
        private readonly ExchangeOptions _options;

        public DefaultExchangeManager(ExchangeOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void EnsureExchangeExists(IModel model, string name, string exchangeType)
        {
            model.ExchangeDeclare(name, exchangeType, _options.IsDurable, _options.IsAutoDelete, _options.Properties);
        }
    }

    public class ExchangeOptions
    {
        public bool IsDurable { get; set; }

        public bool IsAutoDelete { get; set; }

        public IDictionary<string, object> Properties { get; set; }
    }
}