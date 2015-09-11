using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface IEventContextFactory
    {
        EventContext<TEvent> CreateContext<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent;
    }

    public class DefaultEventContextFactory : IEventContextFactory
    {
        public EventContext<TEvent> CreateContext<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent
        {
            return new EventContext<TEvent>(message.Event, Clock.Default.Now);
        }
    }
}