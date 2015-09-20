using System;
using System.Threading.Tasks;
using Nybus;

namespace NLogSampleApp
{
    public class App
    {
        private readonly IBus _bus;

        public App(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            _bus = bus;
        }

        public async Task Execute()
        {
            var messageText = Guid.NewGuid().ToString("D");

            await _bus.InvokeCommand(new SendMessage { Message = messageText });
        }
    }
}