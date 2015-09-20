using System;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface IEventContextFactory
    {
        EventContext<TEvent> CreateContext<TEvent>(EventMessage<TEvent> message, INybusOptions options) where TEvent : class, IEvent;
    }

    public class DefaultEventContextFactory : IEventContextFactory
    {
        private readonly IClock _clock;

        public DefaultEventContextFactory(IClock clock)
        {
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            _clock = clock;
        }

        public EventContext<TEvent> CreateContext<TEvent>(EventMessage<TEvent> message, INybusOptions options) where TEvent : class, IEvent
        {
            return new EventContext<TEvent>(message.Event, _clock.Now, message.CorrelationId);
        }
    }
}