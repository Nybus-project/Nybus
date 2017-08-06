using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nybus
{
    public class RabbitMQBusEngine : IBusEngine
    {
        public RabbitMQBusEngine()
        {

        }

        public IObservable<Message> Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public Task SendCommandAsync<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand
        {
            throw new NotImplementedException();
        }

        public Task SendEventAsync<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent
        {
            throw new NotImplementedException();
        }

        public void SubscribeToCommand<TCommand>() where TCommand : class, ICommand
        {
            throw new NotImplementedException();
        }

        public void SubscribeToEvent<TEvent>() where TEvent : class, IEvent
        {
            throw new NotImplementedException();
        }

        public Task NotifySuccess(Message message)
        {
            throw new NotImplementedException();
        }

        public Task NotifyFail(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
