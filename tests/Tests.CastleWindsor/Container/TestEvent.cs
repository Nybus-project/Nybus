using System.Threading.Tasks;
using Nybus;

namespace Tests.Container
{
    public class TestEvent : IEvent
    {
        
    }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public Task Handle(EventContext<TestEvent> eventMessage)
        {
            return Task.FromResult(0);
        }
    }
}