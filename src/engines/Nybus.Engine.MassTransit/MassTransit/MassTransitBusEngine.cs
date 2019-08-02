using System;
using System.Threading.Tasks;

namespace Nybus.MassTransit
{
    public class MassTransitBusEngine : IBusEngine
    {
        public Task<IObservable<Message>> StartAsync() => throw new NotImplementedException();

        public Task StopAsync() => throw new NotImplementedException();

        public void SubscribeToCommand<TCommand>()
            where TCommand : class, ICommand
        {
            throw new NotImplementedException();
        }

        public void SubscribeToEvent<TEvent>()
            where TEvent : class, IEvent
        {
            throw new NotImplementedException();
        }

        public Task SendMessageAsync(Message message) => throw new NotImplementedException();

        public Task NotifySuccessAsync(Message message) => throw new NotImplementedException();

        public Task NotifyFailAsync(Message message) => throw new NotImplementedException();
    }
}
