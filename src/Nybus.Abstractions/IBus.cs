using System;
using System.Text;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IBus
    {
        Task StartAsync();

        Task StopAsync();

        Task InvokeCommandAsync<TCommand>(TCommand command, Guid correlationId) where TCommand : class, ICommand;

        Task RaiseEventAsync<TEvent>(TEvent @event, Guid correlationId) where TEvent : class, IEvent;
    }
}
