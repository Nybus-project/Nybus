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

    public class FirstTestCommandHandler : ICommandHandler<FirstTestCommand>
    {
        public virtual Task HandleAsync(IDispatcher dispatcher, ICommandContext<FirstTestCommand> context)
        {
            throw new NotImplementedException();
        }
    }

    public class SecondTestCommand : ICommand { }

    public class SecondTestCommandHandler : ICommandHandler<SecondTestCommand>
    {
        private readonly CommandReceivedAsync<SecondTestCommand> _commandReceived;

        public SecondTestCommandHandler(CommandReceivedAsync<SecondTestCommand> commandReceived)
        {
            _commandReceived = commandReceived ?? throw new ArgumentNullException(nameof(commandReceived));
        }

        public virtual Task HandleAsync(IDispatcher dispatcher, ICommandContext<SecondTestCommand> context)
        {
            return _commandReceived(dispatcher, context);
        }
    }

    public class MixedTestCommandHandler : ICommandHandler<FirstTestCommand>, ICommandHandler<SecondTestCommand>
    {
        public Task HandleAsync(IDispatcher dispatcher, ICommandContext<FirstTestCommand> context)
        {
            throw new NotImplementedException();
        }

        public Task HandleAsync(IDispatcher dispatcher, ICommandContext<SecondTestCommand> context)
        {
            throw new NotImplementedException();
        }
    }
    
    public class ThirdTestCommand : ICommand
    {
        public string Message { get; set; }
    }
}

namespace Samples
{
    [Message("ThirdTestCommand", "Tests")]
    public class AttributeTestCommand : ICommand
    {
        public string Message { get; set; }
    }
}

public class NoNamespaceCommand : ICommand
{
    public string Message { get; set; }
}
