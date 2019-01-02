using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Nybus.Policies
{

    public interface IErrorPolicy
    {
        Task HandleError<TCommand>(IBusEngine engine, Exception exception, CommandMessage<TCommand> message) where TCommand : class, ICommand;

        Task HandleError<TEvent>(IBusEngine engine, Exception exception, EventMessage<TEvent> message) where TEvent : class, IEvent;
    }

    public interface IErrorPolicyProvider
    {
        string ProviderName { get; }

        IErrorPolicy CreatePolicy(IConfigurationSection configuration);
    }
}
