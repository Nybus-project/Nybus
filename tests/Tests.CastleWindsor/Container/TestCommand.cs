using System.Threading.Tasks;
using Nybus;

namespace Tests.Container
{
    public class TestCommand : ICommand
    {
        
    }

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task Handle(CommandContext<TestCommand> commandMessage)
        {
            return Task.FromResult(0);
        }
    }
}