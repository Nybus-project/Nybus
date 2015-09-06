using System.Threading.Tasks;

namespace Nybus
{
    public interface IBus
    {
        Task InvokeCommand<TCommand>(TCommand command) where TCommand : class, ICommand;

        Task RaiseEvent<TEvent>(TEvent @event) where TEvent : class, IEvent;

        IHandle Start();
    }

    public interface IHandle
    {
        Task Stop();
    }

}