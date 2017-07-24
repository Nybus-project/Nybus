using System;
using System.Threading.Tasks;
using Nybus;

namespace NetCoreConsoleApp
{
    public class TestEvent : IEvent
    {
        public string Message { get; set; }
    }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public Task HandleAsync(IEventContext<TestEvent> incomingEvent)
        {
            Console.WriteLine(incomingEvent.Event.Message);

            return Task.CompletedTask;
        }
    }
}