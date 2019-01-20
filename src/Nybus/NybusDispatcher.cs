using System;
using System.Collections.Generic;
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

        public Task InvokeCommandAsync<TCommand>(TCommand command, IDictionary<string, string> headers)
            where TCommand : class, ICommand
        {
            return _bus.InvokeCommandAsync(command, _message.Headers.CorrelationId, headers);
        }

        public Task RaiseEventAsync<TEvent>(TEvent @event, IDictionary<string, string> headers)
            where TEvent : class, IEvent
        {
            return _bus.RaiseEventAsync(@event, _message.Headers.CorrelationId, headers);
        }
    }
}