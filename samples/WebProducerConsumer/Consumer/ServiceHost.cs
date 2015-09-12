using System;
using System.Threading.Tasks;
using Nybus;

namespace Consumer
{
    public class ServiceHost
    {
        private readonly IBus _bus;

        public ServiceHost(IBus bus)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            _bus = bus;
        }

        public Task Start()
        {
            return _bus.Start();
        }

        public Task Stop()
        {
            return _bus.Stop();
        }
    }
}