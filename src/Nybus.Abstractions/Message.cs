using System.Collections.Generic;

namespace Nybus
{
    public abstract class Message
    {
        public HeaderBag Headers { get; set; }
    }

    public class HeaderBag : Dictionary<string, string> { }

    public static class Headers
    {
        public static readonly string CorrelationId = "CorrelationId";
        public static readonly string SentOn = "SentOn";
    }

    public class CommandMessage<TCommand> : Message where TCommand: class, ICommand
    {
        public TCommand Command { get; set; }
    }

    public class EventMessage<TEvent> : Message where TEvent : class, IEvent
    {
        public TEvent Event { get; set; }
    }
}
