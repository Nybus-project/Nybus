using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;

namespace Nybus.MassTransit
{
    public class MassTransitHandle : IHandle
    {
        private readonly IReadOnlyList<IServiceBus> _serviceBusses;

        public MassTransitHandle(IReadOnlyList<IServiceBus> serviceBusses)
        {
            if (serviceBusses == null)
            {
                throw new ArgumentNullException(nameof(serviceBusses));
            }
            _serviceBusses = serviceBusses;
        }

        public async Task Stop()
        {
            foreach (var bus in _serviceBusses)
                bus.Dispose();
        }
    }
}