using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nybus;
using Nybus.Utils;

namespace Tests
{
    public class FirstTestEvent : IEvent { }

    public class SecondTestEvent : IEvent { }

    public class FirstTestEventHandler : IEventHandler<FirstTestEvent>
    {
        public Task HandleAsync(IDispatcher dispatcher, IEventContext<FirstTestEvent> incomingEvent)
        {
            throw new NotImplementedException();
        }
    }
}
