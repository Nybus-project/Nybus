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

        Task IBusEngine.SendCommandAsync<TCommand>(CommandMessage<TCommand> message)
        {
            throw new NotImplementedException();
        }

        Task IBusEngine.SendEventAsync<TEvent>(EventMessage<TEvent> message)
        {
            throw new NotImplementedException();
        }

        void IBusEngine.SubscribeToCommand<TCommand>()
        {
            throw new NotImplementedException();
        }

        void IBusEngine.SubscribeToEvent<TEvent>()
        {
            throw new NotImplementedException();
        }
    }
}
