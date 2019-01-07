using System;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IBusEngine
    {
        Task SendCommandAsync<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand;

        Task SendEventAsync<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent;

        Task<IObservable<Message>> StartAsync();

        Task StopAsync();

        void SubscribeToCommand<TCommand>() where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent>() where TEvent : class, IEvent;

        Task NotifySuccessAsync(Message message);

        Task NotifyFailAsync(Message message);
    }

    public delegate Task MessageReceived(Message message);
}
