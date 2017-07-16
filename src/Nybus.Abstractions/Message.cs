using System.Collections.Generic;

namespace Nybus
{
    public abstract class Message
    {
        public HeaderBag Headers { get; set; }
    }

    public class HeaderBag : Dictionary<string, string> { }

    public class CommandMessage<TCommand> : Message where TCommand: class, ICommand
    {
        public TCommand Command { get; }
    }

    public class EventMessage<TEvent> : Message where TEvent : class, IEvent
    {
        public TEvent Event { get; }
    }
}
