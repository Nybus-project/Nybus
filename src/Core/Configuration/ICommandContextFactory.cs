using System;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface ICommandContextFactory
    {
        CommandContext<TCommand> CreateContext<TCommand>(CommandMessage<TCommand> message, INybusOptions options) where TCommand : class, ICommand;
    }

    public class DefaultCommandContextFactory : ICommandContextFactory
    {
        private readonly IClock _clock;

        public DefaultCommandContextFactory(IClock clock)
        {
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            _clock = clock;
        }

        public CommandContext<TCommand> CreateContext<TCommand>(CommandMessage<TCommand> message, INybusOptions options) where TCommand : class, ICommand
        {
            return new CommandContext<TCommand>(message.Command, _clock.Now, message.CorrelationId);
        }
    }
}