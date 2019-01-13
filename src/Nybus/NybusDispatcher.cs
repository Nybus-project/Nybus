using System;
using System.Threading.Tasks;

namespace Nybus
{
    public class NybusDispatcher : IDispatcher
    {
        private readonly IBus _bus;
        private readonly Message _message;

        public NybusDispatcher(IBus innerBus, Message message)
        {
            _bus = innerBus ?? throw new ArgumentNullException(nameof(innerBus));
            _message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public Task InvokeCommandAsync<TCommand>(TCommand command)
            where TCommand : class, ICommand
        {
            return _bus.InvokeCommandAsync(command, _message.Headers.CorrelationId);
        }

        public Task RaiseEventAsync<TEvent>(TEvent @event)
            where TEvent : class, IEvent
        {
            return _bus.RaiseEventAsync(@event, _message.Headers.CorrelationId);
        }
    }
}