using System;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IBusEngine
    {
        Task SendCommandAsync<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand;

        Task SendEventAsync<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent;

        IObservable<Message> Start();

        void Stop();

        void SubscribeToCommand<TCommand>() where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent>() where TEvent : class, IEvent;

        Task NotifySuccess(Message message);
    }

    public delegate Task MessageReceived(Message message);
}
