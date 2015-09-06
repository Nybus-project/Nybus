using System.Threading.Tasks;
using Nybus;

namespace Tests.Container
{
    public class TestEvent : IEvent
    {
        
    }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public async Task Handle(EventContext<TestEvent> eventMessage)
        {
            
        }
    }
}