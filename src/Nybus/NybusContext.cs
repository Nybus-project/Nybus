using Nybus.Utils;
using System;

namespace Nybus
{
    public abstract class NybusContext : IContext
    {
        protected NybusContext (Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            SentOn = message.Headers.SentOn;
            CorrelationId = message.Headers.CorrelationId;
            ReceivedOn = Clock.Default.Now;

            Message = message;
        }

        public DateTimeOffset SentOn { get; }

        public DateTimeOffset ReceivedOn { get; }

        public Guid CorrelationId { get; }

        public Message Message { get; }
    }

    public class NybusCommandContext<TCommand> : NybusContext, ICommandContext<TCommand> where TCommand : class, ICommand
    {
        public NybusCommandContext(CommandMessage<TCommand> commandMessage) : base(commandMessage)
        {
            Command = commandMessage.Command;
        }

        public TCommand Command { get; }

        public CommandMessage<TCommand> CommandMessage => Message as CommandMessage<TCommand>;
    }

    public class NybusEventContext<TEvent> : NybusContext, IEventContext<TEvent> where TEvent : class, IEvent
    {
        public NybusEventContext(EventMessage<TEvent> eventMessage) : base(eventMessage)
        {
            Event = eventMessage.Event;
        }

        public TEvent Event { get; }

        public EventMessage<TEvent> EventMessage => Message as EventMessage<TEvent>;
    }

}
