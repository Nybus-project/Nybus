using System;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IBus
    {
        Task InvokeCommand<TCommand>(TCommand command) where TCommand : class, ICommand;

        Task InvokeCommand<TCommand>(TCommand command, Guid correlationId) where TCommand : class, ICommand;

        Task RaiseEvent<TEvent>(TEvent @event) where TEvent : class, IEvent;

        Task RaiseEvent<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent;

        Task Start();

        Task Stop();
    }
}