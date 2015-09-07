using System.Threading.Tasks;
using Messages;
using Microsoft.AspNet.SignalR;
using Nybus;
using WebProducer.Hubs;

namespace WebProducer.Handlers
{
    public class StringReversedEventHandler : IEventHandler<StringReversedEvent>
    {
        public Task Handle(EventContext<StringReversedEvent> @event)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

            context.Clients.All.notify($"I received this reversed string: \"{@event.Message.Result}\". The consumer slept {@event.Message.TimeSlept} seconds");

            return Task.FromResult(0);
        }
    }
}