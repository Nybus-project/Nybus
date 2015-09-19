using System;
using System.Threading.Tasks;

namespace Nybus.Configuration
{
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
            await _handler(commandMessage).ConfigureAwait(false);
        }
    }
}