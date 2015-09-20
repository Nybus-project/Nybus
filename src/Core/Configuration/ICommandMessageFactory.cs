using System;

namespace Nybus.Configuration
{
    public interface ICommandMessageFactory
    {
        CommandMessage<TCommand> CreateMessage<TCommand>(TCommand command, Guid correlationId) where TCommand : class, ICommand;
    }

    public class DefaultCommandMessageFactory : ICommandMessageFactory
    {
        public CommandMessage<TCommand> CreateMessage<TCommand>(TCommand command, Guid correlationId) where TCommand : class, ICommand
        {
            return new CommandMessage<TCommand>
            {
                Command = command,
                CorrelationId = correlationId
            };
        }
    }
}