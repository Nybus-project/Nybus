using System;
using System.Text;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IBus
    {
        Task StartAsync();

        Task StopAsync();

        Task InvokeCommandAsync<TCommand>(TCommand command, Guid correlationId) where TCommand : class, ICommand;

        Task RaiseEventAsync<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent;

        void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent;
    }

    public delegate Task CommandReceived<TCommand>(ICommandContext<TCommand> context) where TCommand : class, ICommand;

    public delegate Task EventReceived<TEvent>(IEventContext<TEvent> context) where TEvent : class, IEvent;

}
