using Nybus.Utils;
using System;

namespace Nybus
{
    public abstract class NybusContext : IContext
    {
        protected NybusContext (Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            SentOn = DateTimeOffset.Parse(message.Headers[Headers.SentOn]);
            CorrelationId = Guid.Parse(message.Headers[Headers.CorrelationId]);
            ReceivedOn = Clock.Default.Now;

            Message = message;
        }

        public DateTimeOffset SentOn { get; private set; }

        public DateTimeOffset ReceivedOn { get; private set; }

        public Guid CorrelationId { get; private set; }

        protected Message Message { get; private set; }
    }

    public class NybusCommandContext<TCommand> : NybusContext, ICommandContext<TCommand> where TCommand : class, ICommand
    {
        public NybusCommandContext(CommandMessage<TCommand> commandMessage) : base(commandMessage)
        {
            if (commandMessage == null) throw new ArgumentNullException(nameof(commandMessage));
            Command = commandMessage.Command;
        }

        public TCommand Command { get; private set; }
    }

    public class NybusEventContext<TEvent> : NybusContext, IEventContext<TEvent> where TEvent : class, IEvent
    {
        public NybusEventContext(EventMessage<TEvent> eventMessage) : base(eventMessage)
        {
            if (eventMessage == null) throw new ArgumentNullException(nameof(eventMessage));
            Event = eventMessage.Event;
        }

        public TEvent Event { get; private set; }
    }

}
