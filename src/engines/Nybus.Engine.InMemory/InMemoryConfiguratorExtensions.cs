using System;
using System.Collections.Generic;
using System.Text;
using Nybus.Configuration;

namespace Nybus
{
    public static class InMemoryConfiguratorExtensions
    {
        public static void UseInMemoryBusEngine(this INybusConfigurator configurator)
        {
            configurator.UseBusEngine<InMemoryBusEngine>();
        }
    }
}
