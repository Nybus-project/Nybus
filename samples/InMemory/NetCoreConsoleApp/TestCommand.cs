using System;
using System.Threading.Tasks;
using Nybus;

namespace NetCoreConsoleApp
{
    public class TestCommand : ICommand
    {
        public string Message { get; set; }

        public override string ToString() => Message;
    }

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public async Task HandleAsync(IDispatcher dispatcher, ICommandContext<TestCommand> context)
        {
            await dispatcher.RaiseEventAsync(new TestEvent
            {
                Message = $"Received: {context.Command.Message}"
            });
        }
    }
}