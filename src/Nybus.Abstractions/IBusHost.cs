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

        void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent;

        IBusExecutionEnvironment ExecutionEnvironment { get; }
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
