using System;
using System.Threading.Tasks;

namespace Nybus.Configuration
{
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