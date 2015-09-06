using System.Threading.Tasks;
using Nybus;

namespace Tests.Container
{
    public class TestCommand : ICommand
    {
        
    }

    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public async Task Handle(CommandContext<TestCommand> commandMessage)
        {
            
        }
    }
}