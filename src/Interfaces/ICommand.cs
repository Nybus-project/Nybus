using System;
using System.Threading.Tasks;

namespace Nybus
{
    public interface ICommand
    {

    }

    public interface ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        Task Handle(CommandContext<TCommand> commandMessage);
    }

    public class CommandContext<TCommand>
        where TCommand : class, ICommand
    {
        public CommandContext(TCommand message, DateTimeOffset receivedOn, Guid correlationId)
        {
            Message = message;
            ReceivedOn = receivedOn;
            CorrelationId = correlationId;
        }

        public TCommand Message { get; private set; }
        public DateTimeOffset ReceivedOn { get; private set; }
        public Guid CorrelationId { get; set; }
    }

    public class DelegateCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        private readonly Func<CommandContext<TCommand>, Task> _handler;

        public DelegateCommandHandler(Func<CommandContext<TCommand>, Task> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            _handler = handler;
        }

        public async Task Handle(CommandContext<TCommand> commandMessage)
        {
            await _handler(commandMessage);
        }
    }

}