using System;
using System.Threading.Tasks;

namespace Nybus
{
    public class NybusDispatcher : IDispatcher
    {
        private readonly IBus _bus;
        private readonly Guid _correlationId;

        public NybusDispatcher(IBus innerBus, Guid correlationId)
        {
            _bus = innerBus ?? throw new ArgumentNullException(nameof(innerBus));
            _correlationId = correlationId;
        }

        public Task InvokeCommandAsync<TCommand>(TCommand command)
            where TCommand : class, ICommand
        {
            return _bus.InvokeCommandAsync(command, _correlationId);
        }

        public Task RaiseEventAsync<TEvent>(TEvent @event)
            where TEvent : class, IEvent
        {
            return _bus.RaiseEventAsync(@event, _correlationId);
        }
    }
}