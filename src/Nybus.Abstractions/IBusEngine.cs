using System.Threading.Tasks;

namespace Nybus
{
    public interface IBusEngine
    {
        Task SendCommand<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand;

        Task SendEvent<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent;

        void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent;
    }

    public delegate Task CommandReceived<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand;

    public delegate Task EventReceived<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent;
}
