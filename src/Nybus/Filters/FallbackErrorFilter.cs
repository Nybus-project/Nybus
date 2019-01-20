using System;
using System.Threading.Tasks;

namespace Nybus.Filters
{
    public class FallbackErrorFilter : IErrorFilter
    {
        private readonly IBusEngine _engine;

        public FallbackErrorFilter(IBusEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public Task HandleErrorAsync<TCommand>(ICommandContext<TCommand> context, Exception exception, CommandErrorDelegate<TCommand> next)
            where TCommand : class, ICommand
        {
            return _engine.NotifyFailAsync(context.Message);
        }

        public Task HandleErrorAsync<TEvent>(IEventContext<TEvent> context, Exception exception, EventErrorDelegate<TEvent> next)
            where TEvent : class, IEvent
        {
            return _engine.NotifyFailAsync(context.Message);
        }
    }
}