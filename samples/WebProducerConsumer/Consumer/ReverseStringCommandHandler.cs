using System;
using System.Threading.Tasks;
using Messages;
using Nybus;

namespace Consumer
{
    public class ReverseStringCommandHandler : ICommandHandler<ReverseString>
    {
        private static readonly Random Random = new Random(DateTimeOffset.Now.Millisecond);

        private readonly IBus _bus;

        public ReverseStringCommandHandler(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            _bus = bus;
        }

        public async Task Handle(CommandContext<ReverseString> message)
        {
            if (string.IsNullOrEmpty(message.Message.Value))
                return;

            var toWait = TimeSpan.FromMilliseconds(Random.Next(500, 5000));

            Console.WriteLine($"Received command to reverse \"{message.Message.Value}\". I will sleep for {toWait.TotalSeconds} seconds.");

            await Task.Delay(toWait);

            var reversed = ReverseString(message.Message.Value);

            var resultEvent = new StringReversed
            {
                Result = reversed,
                TimeSlept = toWait.TotalSeconds
            };

            Console.WriteLine($"I reversed \"{message.Message.Value}\" and I got \"{reversed}\"");

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