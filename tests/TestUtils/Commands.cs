using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nybus;

namespace Tests
{
    public class FirstTestCommand : ICommand
    {
        public string Message { get; set; }
    }

    public class SecondTestCommand : ICommand { }

    public class FirstTestCommandHandler : ICommandHandler<FirstTestCommand>
    {
        public Task HandleAsync(IDispatcher dispatcher, ICommandContext<FirstTestCommand> incomingCommand)
        {
            throw new NotImplementedException();
        }
    }
}
