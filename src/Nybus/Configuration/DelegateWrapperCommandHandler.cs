using System;
using System.Threading.Tasks;

namespace Nybus.Configuration {
    public class DelegateWrapperCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : class, ICommand
    {
        private readonly CommandReceived<TCommand> _handler;

        public DelegateWrapperCommandHandler(CommandReceived<TCommand> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task HandleAsync(IDispatcher dispatcher, ICommandContext<TCommand> incomingCommand)
        {
            return _handler.Invoke(dispatcher, incomingCommand);
        }
    }
}