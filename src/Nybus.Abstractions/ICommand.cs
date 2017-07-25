using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nybus
{

    public interface ICommand { }

    public interface ICommandHandler<TCommand> where TCommand : class, ICommand
    {
        Task HandleAsync(IBusDispatcher bus, ICommandContext<TCommand> incomingCommand);
    }

    public interface ICommandContext<TCommand> : IContext where TCommand : class, ICommand
    {
        TCommand Command { get; }
    }
}
