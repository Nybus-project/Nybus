using System.Threading.Tasks;
using Messages;
using Nybus;

namespace WebProducer.Handlers
{
    public class StringReversedEventHandler : IEventHandler<StringReversedEvent>
    {
        public Task Handle(EventContext<StringReversedEvent> eventMessage)
        {
            return Task.FromResult(0);
        }
    }
}