using System;
using System.Threading.Tasks;
using Messages;
using Nybus;

namespace Consumer
{
    public class ReverseStringHandler : ICommandHandler<ReverseStringCommand>
    {
        private static readonly Random Random = new Random(DateTimeOffset.Now.Millisecond);

        private readonly IBus _bus;

        public ReverseStringHandler(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            _bus = bus;
        }

        public async Task Handle(CommandContext<ReverseStringCommand> message)
        {
            if (string.IsNullOrEmpty(message.Message.Value))
                return;

            var toWait = TimeSpan.FromMilliseconds(Random.Next(500, 5000));

            await Task.Delay(toWait);

            var resultEvent = new StringReversedEvent
            {
                Result = ReverseString(message.Message.Value),
                TimeSlept = toWait.TotalSeconds
            };

            await _bus.RaiseEvent(resultEvent);
        }

        private string ReverseString(string value)
        {
            var chars = value.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }
    }
}