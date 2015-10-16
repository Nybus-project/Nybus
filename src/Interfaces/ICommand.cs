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

        public DateTimeOffset InvokedOn { get; set; }
    }
}