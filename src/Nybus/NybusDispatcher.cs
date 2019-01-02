using System;
using System.Threading.Tasks;

namespace Nybus
{
    public class NybusDispatcher : IDispatcher
    {
        public NybusDispatcher(IBus innerBus, Guid correlationId)
        {
            Bus = innerBus ?? throw new ArgumentNullException(nameof(innerBus));
            CorrelationId = correlationId;
        }

        public IBus Bus { get; }
        public Guid CorrelationId { get; }


        public Task InvokeCommandAsync<TCommand>(TCommand command)
            where TCommand : class, ICommand
        {
            return Bus.InvokeCommandAsync(command, CorrelationId);
        }

        public Task RaiseEventAsync<TEvent>(TEvent @event)
            where TEvent : class, IEvent
        {
            return Bus.RaiseEventAsync(@event, CorrelationId);
        }
    }
}