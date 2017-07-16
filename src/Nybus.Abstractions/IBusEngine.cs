using System.Threading.Tasks;

namespace Nybus
{
    public interface IBusEngine
    {
        Task SendCommandAsync<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand;

        Task SendEventAsync<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent;

        void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent;

        Task StartAsync();

        Task StopAsync();
    }

    public delegate Task CommandReceived<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand;

    public delegate Task EventReceived<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent;
}
