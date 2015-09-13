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
        public EventContext(TEvent eventMessage, DateTimeOffset receivedOn, Guid correlationId)
        {
            Message = eventMessage;
            ReceivedOn = receivedOn;
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; set; }

        public DateTimeOffset ReceivedOn { get; private set; }

        public TEvent Message { get; private set; }
    }
}