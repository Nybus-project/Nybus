using Nybus.Policies;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Nybus.Configuration
{
    public class NybusBusOptionsBuilder : INybusBusOptionsBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public NybusBusOptionsBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public NybusBusOptions Build()
        {
            ICommandErrorPolicy commandErrorPolicy = (ICommandErrorPolicy)_serviceProvider.GetRequiredService(_commandErrorPolicyType);
            IEventErrorPolicy eventErrorPolicy = (IEventErrorPolicy)_serviceProvider.GetRequiredService(_eventErrorPolicyType);

            return new NybusBusOptions
            {
                CommandErrorPolicy = commandErrorPolicy,
                EventErrorPolicy = eventErrorPolicy
            };
        }

        private Type _commandErrorPolicyType;
        private Type _eventErrorPolicyType;

        public void SetCommandErrorPolicy<TPolicy>() where TPolicy : ICommandErrorPolicy
        {
            _commandErrorPolicyType = typeof(TPolicy);
        }

        public void SetEventErrorPolicy<TPolicy>() where TPolicy : IEventErrorPolicy
        {
            _eventErrorPolicyType = typeof(TPolicy);
        }
    }
}
