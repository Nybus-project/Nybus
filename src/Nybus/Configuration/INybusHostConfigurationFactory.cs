using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Nybus.Filters;
using Nybus.Policies;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface INybusHostConfigurationFactory
    {
        NybusConfiguration CreateConfiguration(NybusHostOptions options);
    }

    public class NybusHostOptions
    {
        public IConfigurationSection ErrorPolicy { get; set; }
    }

    public class NybusHostConfigurationFactory : INybusHostConfigurationFactory
    {
        private readonly IReadOnlyDictionary<string, IErrorPolicyProvider> _errorPolicyProviders;

        public NybusHostConfigurationFactory(IEnumerable<IErrorPolicyProvider> errorPolicyProviders)
        {
            _errorPolicyProviders = CreateDictionary(errorPolicyProviders ?? throw new ArgumentNullException(nameof(errorPolicyProviders)));
        }

        private IReadOnlyDictionary<string, IErrorPolicyProvider> CreateDictionary(IEnumerable<IErrorPolicyProvider> providers)
        {
            var result = new Dictionary<string, IErrorPolicyProvider>(StringComparer.OrdinalIgnoreCase);

            foreach (var provider in providers)
            {
                if (!result.ContainsKey(provider.ProviderName))
                {
                    result.Add(provider.ProviderName, provider);
                }
            }

            return result;
        }

        public NybusConfiguration CreateConfiguration(NybusHostOptions options)
        {
            var errorPolicy = GetErrorPolicy(options.ErrorPolicy);

            return new NybusConfiguration
            {
                ErrorPolicy = errorPolicy
            };

            IErrorPolicy GetErrorPolicy(IConfigurationSection section)
            {
                if (section != null && section.TryGetValue("ProviderName", out var providerName) && _errorPolicyProviders.TryGetValue(providerName, out var provider))
                {
                    return provider.CreatePolicy(section);
                }

                return new NoopErrorPolicy();
            }
        }
    }

    public class NybusConfiguration : INybusConfiguration
    {
        public IErrorPolicy ErrorPolicy { get; set; }

        public IReadOnlyList<ICommandErrorFilter> CommandErrorFilters { get; set; } = new ICommandErrorFilter[0];

        public IReadOnlyList<IEventErrorFilter> EventErrorFilters { get; set; } = new IEventErrorFilter[0];
    }
}