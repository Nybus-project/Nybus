using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nybus
{

    public interface ICommand { }

    public interface ICommandHandler { }

    public interface ICommandHandler<TCommand> : ICommandHandler where TCommand : class, ICommand
    {
        Task HandleAsync(IDispatcher dispatcher, ICommandContext<TCommand> context);
    }

    public interface ICommandContext<TCommand> : IContext where TCommand : class, ICommand
    {
        TCommand Command { get; }
    }
}
