using System;
using System.Threading.Tasks;

namespace Nybus.Policies
{
    public interface IEventErrorPolicy : IPolicy
    {
        Task HandleError<TEvent>(IBusEngine engine, Exception exception, EventMessage<TEvent> message) where TEvent : class, IEvent;
    }
}
