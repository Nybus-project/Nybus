using System;
using System.Collections.Generic;
using System.Linq;
using Nybus.Utils;

namespace Nybus
{
    public abstract class Message
    {
        public string MessageId { get; set; }

        public HeaderBag Headers { get; set; }

        public abstract MessageType MessageType { get; }

        public abstract Type Type { get; }

        public MessageDescriptor Descriptor => new MessageDescriptor(Type);

        public object Item { get; protected set; }
    }

    public enum MessageType
    {
        Command, Event
    }

    public class HeaderBag : Dictionary<string, string>
    {
        public HeaderBag(IDictionary<string, string> headers) : base(headers, StringComparer.Ordinal) { }

        public HeaderBag() : this (new Dictionary<string, string>()) { }

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

        private static readonly string[] ValidHeaders = { MessageId, MessageType, CorrelationId, SentOn, RetryCount };

        public static bool IsNybus(string header) => ValidHeaders.Contains(header);
    }

    public abstract class CommandMessage : Message
    {
        public override MessageType MessageType => MessageType.Command;

        public void SetCommand(ICommand command) => Item = command ?? throw new ArgumentNullException(nameof(command));
    }

    public sealed class CommandMessage<TCommand> : CommandMessage where TCommand: class, ICommand
    {
        public TCommand Command
        {
            get => Item as TCommand;
            set => Item = value;
        }

        public override Type Type => typeof(TCommand);
    }

    public abstract class EventMessage : Message
    {
        public override MessageType MessageType => MessageType.Event;

        public void SetEvent(IEvent @event) => Item = @event ?? throw new ArgumentNullException(nameof(@event));
    }

    public sealed class EventMessage<TEvent> : EventMessage where TEvent : class, IEvent
    {
        public TEvent Event
        {
            get => Item as TEvent;
            set => Item = value;
        }

        public override Type Type => typeof(TEvent);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MessageAttribute : Attribute
    {
        public MessageAttribute(string name, string @namespace)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
        }

        public string Name { get; }

        public string Namespace { get; }
    }
}
