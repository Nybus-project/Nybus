using System;
using System.Threading.Tasks;

namespace Nybus.Filters
{
    public interface IErrorFilter
    {
        Task HandleErrorAsync<TCommand>(ICommandContext<TCommand> context, Exception exception, CommandErrorDelegate<TCommand> next) where TCommand : class, ICommand;

        Task HandleErrorAsync<TEvent>(IEventContext<TEvent> context, Exception exception, EventErrorDelegate<TEvent> next) where TEvent : class, IEvent;
    }

    public delegate Task CommandErrorDelegate<TCommand>(ICommandContext<TCommand> context, Exception exception) where TCommand : class, ICommand;

    public delegate Task EventErrorDelegate<TEvent>(IEventContext<TEvent> context, Exception exception) where TEvent : class, IEvent;
}
