using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nybus;

namespace Tests
{
    public class TestBusEngine : IBusEngine
    {
        public Task SendCommandAsync<TCommand>(CommandMessage<TCommand> message)
            where TCommand : class, ICommand
        {
            throw new NotImplementedException();
        }

        public Task SendEventAsync<TEvent>(EventMessage<TEvent> message)
            where TEvent : class, IEvent
        {
            throw new NotImplementedException();
        }

        public Task<IObservable<Message>> StartAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }

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

        public Task SendMessageAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task NotifySuccessAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task NotifyFailAsync(Message message)
        {
            throw new NotImplementedException();
        }
    }

}
