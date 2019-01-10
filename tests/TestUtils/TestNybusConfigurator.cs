using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Nybus;
using Nybus.Configuration;
using Nybus.Utils;

namespace Tests
{
    public class TestNybusConfigurator : INybusConfigurator
    {
        public void UseConfiguration(Microsoft.Extensions.Configuration.IConfiguration configuration, string sectionName = "Nybus")
        {
            Configuration = configuration.GetSection(sectionName);
        }

        private readonly List<Action<IServiceCollection>> _serviceConfigurations = new List<Action<IServiceCollection>>();

        public void AddServiceConfiguration(Action<IServiceCollection> configurator)
        {
            _serviceConfigurations.Add(configurator);
        }

        public void ApplyServiceConfigurations(IServiceCollection services)
        {
            foreach (var sc in _serviceConfigurations)
                sc(services);
        }

        private readonly List<Action<ISubscriptionBuilder>> _subscriptionBuilders = new List<Action<ISubscriptionBuilder>>();

        public void AddSubscription(Action<ISubscriptionBuilder> configurator)
        {
            _subscriptionBuilders.Add(configurator);
        }

        public void ApplySubscriptions(ISubscriptionBuilder builder)
        {
            foreach (var sb in _subscriptionBuilders)
                sb(builder);
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; private set; }

        public void Configure(Action<INybusConfiguration> configuration)
        {
            throw new NotImplementedException();
        }
    }

    public class TestMessageDescriptorStore : IMessageDescriptorStore
    {
        private readonly HashSet<Type> _commandTypes = new HashSet<Type>();
        private readonly HashSet<Type> _eventTypes = new HashSet<Type>();

        public bool RegisterCommandType<TCommand>()
            where TCommand : class, ICommand
        {
            return _commandTypes.Add(typeof(TCommand));
        }

        public bool RegisterEventType<TEvent>()
            where TEvent : class, IEvent
        {
            return _eventTypes.Add(typeof(TEvent));
        }

        public bool FindCommandTypeForDescriptor(MessageDescriptor descriptor, out Type type)
        {
            type = _commandTypes.FirstOrDefault(i => i.Name == descriptor.Name && i.Namespace == descriptor.Namespace);
            return type != null;
        }

        public bool FindEventTypeForDescriptor(MessageDescriptor descriptor, out Type type)
        {
            type = _eventTypes.FirstOrDefault(i => i.Name == descriptor.Name && i.Namespace == descriptor.Namespace);
            return type != null;
        }

        public bool HasCommands()
        {
            return _commandTypes.Count > 0;
        }

        public bool HasEvents()
        {
            return _eventTypes.Count > 0;
        }

        public IEnumerable<Type> Commands => _commandTypes;

        public IEnumerable<Type> Events => _eventTypes;
    }
}
