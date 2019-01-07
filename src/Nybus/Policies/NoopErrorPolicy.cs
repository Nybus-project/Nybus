using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Nybus.Policies
{
    public class NoopErrorPolicyProvider : IErrorPolicyProvider
    {
        public string ProviderName => "noop";
        public IErrorPolicy CreatePolicy(IConfigurationSection configuration)
        {
            return new NoopErrorPolicy();
        }
    }

    public class NoopErrorPolicy : IErrorPolicy
    {
        public Task HandleErrorAsync<TCommand>(IBusEngine engine, Exception exception, CommandMessage<TCommand> message)
            where TCommand : class, ICommand
        {
            return engine.NotifyFailAsync(message);
        }

        public Task HandleErrorAsync<TEvent>(IBusEngine engine, Exception exception, EventMessage<TEvent> message)
            where TEvent : class, IEvent
        {
            return engine.NotifyFailAsync(message);
        }
    }
}