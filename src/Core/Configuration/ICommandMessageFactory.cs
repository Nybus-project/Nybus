namespace Nybus.Configuration
{
    public interface ICommandMessageFactory
    {
        CommandMessage<TCommand> CreateMessage<TCommand>(TCommand command) where TCommand : class, ICommand;
    }

    public class DefaultCommandMessageFactory : ICommandMessageFactory
    {
        public CommandMessage<TCommand> CreateMessage<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            return new CommandMessage<TCommand>(command);
        }
    }
}