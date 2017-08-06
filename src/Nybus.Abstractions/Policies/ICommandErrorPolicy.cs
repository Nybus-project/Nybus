using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nybus.Policies
{

    public interface ICommandErrorPolicy : IPolicy
    {
        Task HandleError<TCommand>(IBusEngine engine, Exception exception, CommandMessage<TCommand> message) where TCommand : class, ICommand;
    }
}
