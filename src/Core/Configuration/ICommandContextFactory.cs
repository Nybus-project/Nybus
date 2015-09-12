using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface ICommandContextFactory
    {
        CommandContext<TCommand> CreateContext<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand;
    }

    public class DefaultCommandContextFactory : ICommandContextFactory
    {
        public CommandContext<TCommand> CreateContext<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand
        {
            return new CommandContext<TCommand>(message.Command, Clock.Default.Now, message.CorrelationId);
        }
    }
}