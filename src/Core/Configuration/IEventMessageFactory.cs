using System;

namespace Nybus.Configuration
{
    public interface IEventMessageFactory
    {
        EventMessage<TEvent> CreateMessage<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent;
    }

    public class DefaultEventMessageFactory : IEventMessageFactory
    {
        public EventMessage<TEvent> CreateMessage<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent
        {
            return new EventMessage<TEvent>(@event)
            {
                CorrelationId = correlationId
            };
        }
    }
}