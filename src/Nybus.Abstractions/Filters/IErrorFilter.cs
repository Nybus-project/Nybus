using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nybus.Filters
{
    public interface ICommandErrorFilter
    {
        Task HandleErrorAsync<TCommand>(IBusEngine engine, ICommandContext<TCommand> context, Exception exception, CommandErrorDelegate<TCommand> next) where TCommand : class, ICommand;
    }

    public delegate Task CommandErrorDelegate<TCommand>(IBusEngine engine, ICommandContext<TCommand> context, Exception exception) where TCommand : class, ICommand;

    public interface IEventErrorFilter
    {
        Task HandleErrorAsync<TEvent>(IBusEngine engine, IEventContext<TEvent> context, Exception exception, EventErrorDelegate<TEvent> next) where TEvent : class, IEvent;
    }

    public delegate Task EventErrorDelegate<TEvent>(IBusEngine engine, IEventContext<TEvent> context, Exception exception) where TEvent : class, IEvent;
}
