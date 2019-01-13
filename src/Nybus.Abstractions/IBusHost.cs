using System;
using System.Text;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IBus
    {
        Task InvokeCommandAsync<TCommand>(TCommand command, Guid correlationId) where TCommand : class, ICommand;

        Task RaiseEventAsync<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent;
    }

    public interface IBusHost
    {
        Task StartAsync();

        Task StopAsync();

        IBus Bus { get; }
    }

    public delegate Task CommandReceived<TCommand>(IDispatcher dispatcher, ICommandContext<TCommand> context) where TCommand : class, ICommand;

    public delegate Task EventReceived<TEvent>(IDispatcher dispatcher, IEventContext<TEvent> context) where TEvent : class, IEvent;

    public interface IDispatcher
    {
        Task InvokeCommandAsync<TCommand>(TCommand command) where TCommand : class, ICommand;

        Task RaiseEventAsync<TEvent>(TEvent @event) where TEvent : class, IEvent;

    }

    public interface IBusExecutionEnvironment
    {
        Task ExecuteCommandHandlerAsync<TCommand>(IDispatcher dispatcher, ICommandContext<TCommand> context, Type handlerType)
            where TCommand : class, ICommand;

        Task ExecuteEventHandlerAsync<TEvent>(IDispatcher dispatcher, IEventContext<TEvent> context, Type handlerType)
            where TEvent : class, IEvent;
    }
}
