using System;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IEvent
    {

    }

    public interface IEventHandler<TEvent>
        where TEvent : class, IEvent
    {
        Task Handle(EventContext<TEvent> eventMessage);
    }

    public class EventContext<TEvent>
        where TEvent : class, IEvent
    {
        public EventContext(TEvent eventMessage, DateTimeOffset receivedOn)
        {
            Message = eventMessage;
            ReceivedOn = receivedOn;
        }

        public DateTimeOffset ReceivedOn { get; private set; }

        public TEvent Message { get; private set; }
    }

    public class DelegateEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : class, IEvent
    {
        private readonly Func<EventContext<TEvent>, Task> _handler;

        public DelegateEventHandler(Func<EventContext<TEvent>, Task> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            _handler = handler;
        }

        public async Task Handle(EventContext<TEvent> eventMessage)
        {
            await _handler(eventMessage);
        }
    }

}