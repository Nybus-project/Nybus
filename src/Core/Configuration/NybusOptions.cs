using System;
using Nybus.Container;
using Nybus.Logging;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface INybusOptions
    {
        IContainer Container { get; }

        ILogger Logger { get; }

        ICommandMessageFactory CommandMessageFactory { get; }

        ICommandContextFactory CommandContextFactory { get; }

        IEventMessageFactory EventMessageFactory { get; }

        IEventContextFactory EventContextFactory { get; }

        ICorrelationIdGenerator CorrelationIdGenerator { get; }
    }

    public class NybusOptions : INybusOptions
    {
        public IContainer Container { get; set; } = new ActivatorContainer();

        public ILogger Logger { get; set; } = new NopLogger();

        public ICommandMessageFactory CommandMessageFactory { get; set; } = new DefaultCommandMessageFactory();

        public ICommandContextFactory CommandContextFactory { get; set; } = new DefaultCommandContextFactory(Clock.Default);

        public IEventMessageFactory EventMessageFactory { get; set; } = new DefaultEventMessageFactory();

        public IEventContextFactory EventContextFactory { get; set; } = new DefaultEventContextFactory(Clock.Default);

        public ICorrelationIdGenerator CorrelationIdGenerator { get; set; } = new NewGuidCorrelationIdGenerator();
    }
}