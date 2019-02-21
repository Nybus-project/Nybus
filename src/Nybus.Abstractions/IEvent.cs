using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IEvent { }

    public interface IEventHandler { }

    public interface IEventHandler<TEvent> : IEventHandler where TEvent : class, IEvent
    {
        Task HandleAsync(IDispatcher dispatcher, IEventContext<TEvent> context);
    }
    
    public interface IEventContext<TEvent> : IContext where TEvent : class, IEvent
    {
        TEvent Event { get; }
    }
}
