using System;
using System.Threading.Tasks;

namespace Nybus
{
    public class Nybus : IBus
    {
        private readonly IBusEngine _engine;

        public Nybus(IBusEngine engine)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            _engine = engine;
        }

        public Task InvokeCommand<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            var message = new CommandMessage<TCommand>(command);
            return _engine.SendMessage(message);
        }

        public Task RaiseEvent<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            var message = new EventMessage<TEvent>(@event);
            return _engine.SendMessage(message);
        }

        public Task Start()
        {
            return _engine.Start();
        }

        public Task Stop()
        {
            return _engine.Stop();
        }
    }
}