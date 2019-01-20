using System;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IBusEngine
    {
        Task<IObservable<Message>> StartAsync();

        Task StopAsync();

        void SubscribeToCommand<TCommand>() where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent>() where TEvent : class, IEvent;

        Task SendMessageAsync(Message message);

        Task NotifySuccessAsync(Message message);

        Task NotifyFailAsync(Message message);
    }
}
