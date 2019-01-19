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

        private readonly ConcurrentDictionary<Type, Func<IContext, Exception, Task>> _errorHandlers = new ConcurrentDictionary<Type, Func<IContext, Exception, Task>>();

        public Task HandleCommandErrorAsync<TCommand>(IBusEngine engine, ICommandContext<TCommand> context, Exception error)
            where TCommand : class, ICommand
        {
            CommandErrorDelegate<TCommand> final = (c, ex) => Task.CompletedTask;

            if (ErrorPolicy != null)
            {
                final = (c, ex) => ErrorPolicy.HandleErrorAsync(engine, ex, context.Message as CommandMessage<TCommand>);
            }

            var handler = _errorHandlers.GetOrAdd(typeof(TCommand), CreateCommandErrorDelegate(final));
            return handler(context, error);
        }

        private Func<IContext, Exception, Task> CreateCommandErrorDelegate<TCommand>(CommandErrorDelegate<TCommand> final)
            where TCommand : class, ICommand
        {
            return (ctx, exception) =>
            {
                if (ctx is ICommandContext<TCommand> context)
                {
                    var chain = new List<CommandErrorDelegate<TCommand>>
                    {
                        final
                    };

                    foreach (var filter in CommandErrorFilters.Reverse())
                    {
                        var latest = chain.Last();
                        chain.Add((c, ex) => filter.HandleErrorAsync(c, ex, latest));
                    }

                    return chain.Last().Invoke(context, exception);
                }

                return Task.FromException(exception);
            };
        }

        public Task HandleEventErrorAsync<TEvent>(IBusEngine engine, IEventContext<TEvent> context, Exception error)
            where TEvent : class, IEvent
        {
            EventErrorDelegate<TEvent> final = (c, ex) => Task.CompletedTask;

            if (ErrorPolicy != null)
            {
                final = (c, ex) => ErrorPolicy.HandleErrorAsync(engine, ex, c.Message as EventMessage<TEvent>);
            }

            var handler = _errorHandlers.GetOrAdd(typeof(TEvent), CreateEventErrorDelegate(final));
            return handler(context, error);
        }

        private Func<IContext, Exception, Task> CreateEventErrorDelegate<TEvent>(EventErrorDelegate<TEvent> final)
            where TEvent : class, IEvent
        {
            return (ctx, exception) =>
            {
                if (ctx is IEventContext<TEvent> context)
                {
                    var chain = new List<EventErrorDelegate<TEvent>>
                    {
                        final
                    };

                    foreach (var filter in EventErrorFilters.Reverse())
                    {
                        var latest = chain.Last();
                        chain.Add((c, ex) => filter.HandleErrorAsync(c, ex, latest));
                    }

                    return chain.Last().Invoke(context, exception);
                }

                return Task.FromException(exception);
            };
        }

        // await _configuration.ErrorPolicy.HandleErrorAsync(_engine, ex, context.Message as CommandMessage<TCommand>).ConfigureAwait(false);
    }
}