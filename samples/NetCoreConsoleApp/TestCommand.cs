using System;
using System.Threading.Tasks;
using Nybus;

namespace NetCoreConsoleApp
{
    public class TestCommand : ICommand
    {
        public string Message { get; set; }
    }

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public async Task HandleAsync(IBusDispatcher bus, ICommandContext<TestCommand> incomingCommand)
        {
            await bus.RaiseEventAsync(new TestEvent
            {
                Message = $"Received: {incomingCommand.Command.Message}"
            });
        }
    }
}