using System;
using System.Text;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IBusDispatcher
    {
        Task InvokeCommandAsync<TCommand>(TCommand command, Guid correlationId) where TCommand : class, ICommand;

        Task RaiseEventAsync<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent;

    }

    public interface IBus : IBusDispatcher
    {
        Task StartAsync();

        Task StopAsync();

        void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent;
    }

    public delegate Task CommandReceived<TCommand>(IBus bus, ICommandContext<TCommand> context) where TCommand : class, ICommand;

    public delegate Task EventReceived<TEvent>(IBus bus, IEventContext<TEvent> context) where TEvent : class, IEvent;

}
