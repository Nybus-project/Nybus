using System;
using System.Collections.Generic;

namespace Nybus
{
    public abstract class Message
    {
        public string MessageId { get; set; }

        public HeaderBag Headers { get; set; }

        public abstract MessageType MessageType { get; }

        public abstract Type Type { get; }
    }

    public enum MessageType
    {
        Command, Event
    }

    public class HeaderBag : Dictionary<string, string> { }

    public static class Headers
    {
        public static readonly string CorrelationId = nameof(CorrelationId);
        public static readonly string SentOn = nameof(SentOn);
        public static readonly string RetryCount = nameof(RetryCount);
    }

    public abstract class CommandMessage : Message
    {
        public override MessageType MessageType => MessageType.Command;
    }

    public class CommandMessage<TCommand> : CommandMessage where TCommand: class, ICommand
    {
        public TCommand Command { get; set; }

        public override Type Type => typeof(TCommand);
    }

    public abstract class EventMessage : Message
    {
        public override MessageType MessageType => MessageType.Event;
    }

    public class EventMessage<TEvent> : EventMessage where TEvent : class, IEvent
    {
        public TEvent Event { get; set; }

        public override Type Type => typeof(TEvent);
    }
}
