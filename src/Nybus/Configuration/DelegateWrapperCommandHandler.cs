using System;
using System.Threading.Tasks;

namespace Nybus.Configuration
{
    public class DelegateWrapperCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        private readonly CommandReceivedAsync<TCommand> _handler;

        public DelegateWrapperCommandHandler(CommandReceivedAsync<TCommand> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task HandleAsync(IDispatcher dispatcher, ICommandContext<TCommand> context)
        {
            return _handler.Invoke(dispatcher, context);
        }
    }
}