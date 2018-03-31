using Nybus.Policies;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Nybus.Configuration
{
    public class NybusBusOptionsBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public NybusBusOptionsBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public NybusBusOptions Build()
        {
            var errorPolicy = (IErrorPolicy)_serviceProvider.GetRequiredService(_errorPolicyType);

            return new NybusBusOptions
            {
                ErrorPolicy = errorPolicy,
            };
        }

        private Type _errorPolicyType;

        public void SetErrorPolicy<TPolicy>() where TPolicy : IErrorPolicy
        {
            _errorPolicyType = typeof(TPolicy);
        }
    }
}
