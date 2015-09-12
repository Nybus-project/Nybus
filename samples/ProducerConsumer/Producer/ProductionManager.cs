using System;
using System.Threading.Tasks;
using Messages;
using Nybus;

namespace Producer
{
    public class ProductionManager
    {
        private readonly IBus _bus;
        private readonly Random _random = new Random();

        public ProductionManager(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            _bus = bus;
        }

        public async Task Execute(int numberOfMessages)
        {
            for (int i = 0; i < numberOfMessages; i++)
            {
                await InvokeCommand(i);
            }
        }

        private async Task InvokeCommand(int i)
        {
            var command = new ProduceItem
            {
                ItemId = Guid.NewGuid(),
                Quantity = _random.Next(1, 200)
            };

            await _bus.InvokeCommand(command);

            Console.WriteLine($"{i}: Invoked production of {command.Quantity} items of product with ID {command.ItemId}");
        }

    }
}