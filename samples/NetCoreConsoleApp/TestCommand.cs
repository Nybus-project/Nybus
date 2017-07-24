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
        private readonly IBus _bus;

        public TestCommandHandler(IBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task HandleAsync(ICommandContext<TestCommand> incomingCommand)
        {
            await _bus.RaiseEventAsync(new TestEvent
            {
                Message = $"Received: {incomingCommand.Command.Message}"
            });
        }
    }
}