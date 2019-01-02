using System;
using System.Collections.Generic;
using Nybus.Utils;

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

    public class HeaderBag : Dictionary<string, string>
    {
        public DateTimeOffset SentOn
        {
            get => DateTimeOffset.Parse(this[Headers.SentOn]);
            set => this[Headers.SentOn] = value.Stringfy();
        }

        public Guid CorrelationId
        {
            get => Guid.Parse(this[Headers.CorrelationId]);
            set => this[Headers.CorrelationId] = value.Stringfy();
        }
    }

    public static class Headers
    {
        public static readonly string MessageId = nameof(MessageId);
        public static readonly string MessageType = nameof(MessageType);
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
