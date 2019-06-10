using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Nybus.Filters;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface INybusHostConfigurationFactory
    {
        NybusConfiguration CreateConfiguration(NybusHostOptions options);
    }

    public class NybusHostOptions
    {
        public IConfigurationSection[] ErrorFilters { get; set; }

        public IConfigurationSection[] CommandErrorFilters { get; set; }

        public IConfigurationSection[] EventErrorFilters { get; set; }
    }

    public class NybusHostConfigurationFactory : INybusHostConfigurationFactory
    {
        private readonly IErrorFilter _fallbackErrorFilter;
        private readonly IReadOnlyDictionary<string, IErrorFilterProvider> _errorFilterProvidersByName;

        public NybusHostConfigurationFactory(IEnumerable<IErrorFilterProvider> errorFilterProviders, DiscardErrorFilter fallbackErrorFilter)
        {
            _fallbackErrorFilter = fallbackErrorFilter ?? throw new ArgumentNullException(nameof(fallbackErrorFilter));
            _errorFilterProvidersByName = CreateDictionary(errorFilterProviders ?? throw new ArgumentNullException(nameof(errorFilterProviders)));
        }

        private static IReadOnlyDictionary<string, IErrorFilterProvider> CreateDictionary(IEnumerable<IErrorFilterProvider> providers)
        {
            var result = new Dictionary<string, IErrorFilterProvider>(StringComparer.OrdinalIgnoreCase);

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
            return new NybusConfiguration
            {
                FallbackErrorFilter = _fallbackErrorFilter,
                CommandErrorFilters = GetErrorFilters(options.CommandErrorFilters),
                EventErrorFilters = GetErrorFilters(options.EventErrorFilters)
            };

            IReadOnlyList<IErrorFilter> GetErrorFilters(IEnumerable<IConfigurationSection> sections) => sections
                                                                                                        .IfNull(options.ErrorFilters)
                                                                                                        .EmptyIfNull()
                                                                                                        .Select(GetErrorFilter)
                                                                                                        .NotNull()
                                                                                                        .ToArray();

            IErrorFilter GetErrorFilter(IConfigurationSection section)
            {
                if (section != null && section.TryGetValue("type", out var providerName) && _errorFilterProvidersByName.TryGetValue(providerName, out var provider))
                {
                    return provider.CreateErrorFilter(section);
                }

                return null;
            }
        }
    }

    public class NybusConfiguration : INybusConfiguration
    {
        public IReadOnlyList<IErrorFilter> CommandErrorFilters { get; set; } = new IErrorFilter[0];

        public IReadOnlyList<IErrorFilter> EventErrorFilters { get; set; } = new IErrorFilter[0];

        public IErrorFilter FallbackErrorFilter { get; set; }
    }
}