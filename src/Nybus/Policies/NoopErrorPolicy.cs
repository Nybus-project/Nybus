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
        public async Task HandleError<TCommand>(IBusEngine engine, Exception exception, CommandMessage<TCommand> message)
            where TCommand : class, ICommand
        {
            await engine.NotifyFail(message).ConfigureAwait(false);
        }

        public async Task HandleError<TEvent>(IBusEngine engine, Exception exception, EventMessage<TEvent> message)
            where TEvent : class, IEvent
        {
            await engine.NotifyFail(message).ConfigureAwait(false);
        }
    }
}