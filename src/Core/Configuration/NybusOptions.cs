using Nybus.Container;

namespace Nybus.Configuration
{
    public class NybusOptions
    {
        public IContainer Container { get; set; } = new ActivatorContainer();

        public ILogger Logger { get; set; } = new NopLogger();

        public ICommandMessageFactory CommandMessageFactory { get; set; } = new DefaultCommandMessageFactory();

        public ICommandContextFactory CommandContextFactory { get; set; } = new DefaultCommandContextFactory();

        public IEventMessageFactory EventMessageFactory { get; set; } = new DefaultEventMessageFactory();

        public IEventContextFactory EventContextFactory { get; set; } = new DefaultEventContextFactory();
    }
}