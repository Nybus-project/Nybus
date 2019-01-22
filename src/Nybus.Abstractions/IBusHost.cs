using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IBus
    {
        Task InvokeCommandAsync<TCommand>(TCommand command, Guid correlationId, IDictionary<string, string> headers) where TCommand : class, ICommand;

        Task RaiseEventAsync<TEvent>(TEvent @event, Guid correlationId, IDictionary<string, string> headers) where TEvent : class, IEvent;
    }

    public interface IBusHost
    {
        Task StartAsync();

        Task StopAsync();

        IBus Bus { get; }
    }

    public delegate Task CommandReceivedAsync<TCommand>(IDispatcher dispatcher, ICommandContext<TCommand> context) where TCommand : class, ICommand;

    public delegate void CommandReceived<TCommand>(IDispatcher dispatcher, ICommandContext<TCommand> context) where TCommand : class, ICommand;

    public delegate Task EventReceivedAsync<TEvent>(IDispatcher dispatcher, IEventContext<TEvent> context) where TEvent : class, IEvent;

    public delegate void EventReceived<TEvent>(IDispatcher dispatcher, IEventContext<TEvent> context) where TEvent : class, IEvent;

    public interface IDispatcher
    {
        Task InvokeCommandAsync<TCommand>(TCommand command, IDictionary<string, string> headers) where TCommand : class, ICommand;

        Task RaiseEventAsync<TEvent>(TEvent @event, IDictionary<string, string> headers) where TEvent : class, IEvent;

    }

    public interface IBusExecutionEnvironment
    {
        Task ExecuteCommandHandlerAsync<TCommand>(IDispatcher dispatcher, ICommandContext<TCommand> context, Type handlerType)
            where TCommand : class, ICommand;

        Task ExecuteEventHandlerAsync<TEvent>(IDispatcher dispatcher, IEventContext<TEvent> context, Type handlerType)
            where TEvent : class, IEvent;
    }
}
