using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nybus;
using Nybus.Utils;

namespace Tests
{
    public class FirstTestEvent : IEvent { }

    public class FirstTestEventHandler : IEventHandler<FirstTestEvent>
    {
        public virtual Task HandleAsync(IDispatcher dispatcher, IEventContext<FirstTestEvent> context)
        {
            throw new NotImplementedException();
        }
    }

    public class SecondTestEvent : IEvent { }

    public class SecondTestEventHandler : IEventHandler<SecondTestEvent>
    {
        private readonly EventReceivedAsync<SecondTestEvent> _eventReceived;

        public SecondTestEventHandler(EventReceivedAsync<SecondTestEvent> eventReceived)
        {
            _eventReceived = eventReceived ?? throw new ArgumentNullException(nameof(eventReceived));
        }

        public virtual Task HandleAsync(IDispatcher dispatcher, IEventContext<SecondTestEvent> context)
        {
            return _eventReceived(dispatcher, context);
        }
    }

    public class MixedTestEventHandler : IEventHandler<FirstTestEvent>, IEventHandler<SecondTestEvent>
    {
        public Task HandleAsync(IDispatcher dispatcher, IEventContext<FirstTestEvent> context)
        {
            throw new NotImplementedException();
        }

        public Task HandleAsync(IDispatcher dispatcher, IEventContext<SecondTestEvent> context)
        {
            throw new NotImplementedException();
        }
    }

    public class MixedTestEventHandler : IEventHandler<FirstTestEvent>, IEventHandler<SecondTestEvent>
    {
        public Task HandleAsync(IDispatcher dispatcher, IEventContext<FirstTestEvent> context)
        {
            throw new NotImplementedException();
        }

        public Task HandleAsync(IDispatcher dispatcher, IEventContext<SecondTestEvent> context)
        {
            throw new NotImplementedException();
        }
    }

    public class MixedTestEventHandler : IEventHandler<FirstTestEvent>, IEventHandler<SecondTestEvent>
    {
        public Task HandleAsync(IDispatcher dispatcher, IEventContext<FirstTestEvent> incomingEvent)
        {
            throw new NotImplementedException();
        }

        public Task HandleAsync(IDispatcher dispatcher, IEventContext<SecondTestEvent> incomingEvent)
        {
            throw new NotImplementedException();
        }
    }

    public class ThirdTestEvent : IEvent
    {
        public string Message { get; set; }
    }
}

namespace Samples
{
    [Message("ThirdTestEvent", "Tests")]
    public class AttributeTestEvent : IEvent
    {
        public string Message { get; set; }
    }
}

public class NoNamespaceEvent : IEvent
{
    public string Message { get; set; }
}