namespace Nybus.Configuration
{
    public interface IEventMessageFactory
    {
        EventMessage<TEvent> CreateMessage<TEvent>(TEvent @event) where TEvent : class, IEvent;
    }

    public class DefaultEventMessageFactory : IEventMessageFactory
    {
        public EventMessage<TEvent> CreateMessage<TEvent>(TEvent @event) where TEvent : class, IEvent
        {
            return new EventMessage<TEvent>(@event);
        }
    }
}