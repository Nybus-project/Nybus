using System.Threading.Tasks;
using Nybus;

namespace NetCoreRabbitMq.Handlers
{
    public class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task HandleAsync(IDispatcher dispatcher, ICommandContext<TestCommand> incomingCommand)
        {
            return Task.CompletedTask;
        }
    }
}