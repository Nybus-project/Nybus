using System;
using System.Threading.Tasks;
using Messages;
using Nybus;

namespace Consumer
{
    public class ProduceItemHandler : ICommandHandler<ProduceItem>
    {
        private readonly IBus _bus;

        public ProduceItemHandler(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            _bus = bus;
        }

        public async Task Handle(CommandContext<ProduceItem> command)
        {
            Console.WriteLine($"Producing item {command.CommandMessage.ItemId}");

            await Task.Delay(TimeSpan.FromMilliseconds(200));

            await _bus.RaiseEvent(new ItemProduced
            {
                ItemId = command.CommandMessage.ItemId,
                Quantity = command.CommandMessage.Quantity
            });
        }
    }
}