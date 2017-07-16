using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IEvent { }

    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        Task HandleAsync(IEventContext<TEvent> incomingEvent);
    }
    
    public interface IEventContext<TEvent> where TEvent : IEvent
    {
        TEvent Event { get; }

        DateTimeOffset ReceivedOn { get; }

        Guid CorrelationId { get; }
    }
}
