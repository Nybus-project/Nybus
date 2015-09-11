using System;
using System.Threading.Tasks;
using Nybus.Configuration;

namespace Nybus
{
    public class Nybus : IBus
    {
        private readonly IBusEngine _engine;
        private readonly NybusOptions _options;

        public Nybus(IBusEngine engine, NybusOptions options)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (options == null) throw new ArgumentNullException(nameof(options));

            _engine = engine;
            _options = options;
        }

        public Task InvokeCommand<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            var message = _options.CommandMessageFactory.CreateMessage(command);
            return _engine.SendMessage(message);
        }

        public Task RaiseEvent<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            var message = _options.EventMessageFactory.CreateMessage(@event);
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