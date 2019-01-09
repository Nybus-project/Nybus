using System;
using Nybus.Configuration;
using Nybus.Utils;

namespace Nybus.InMemory
{
    public interface IEnvelopeService
    {
        Envelope CreateEnvelope<T>(CommandMessage<T> message) where T : class, ICommand;

        Envelope CreateEnvelope<T>(EventMessage<T> message) where T : class, IEvent;

        CommandMessage CreateCommandMessage(Envelope envelope, Type commandType);

        EventMessage CreateEventMessage(Envelope envelope, Type eventType);
    }

    public class EnvelopeService : IEnvelopeService
    {
        private readonly ISerializer _serializer;

        public EnvelopeService(ISerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public Envelope CreateEnvelope<T>(CommandMessage<T> message)
            where T : class, ICommand
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return new Envelope
            {
                Headers = message.Headers,
                Content = _serializer.SerializeObject(message.Command),
                MessageType = MessageType.Command,
                MessageId = message.MessageId,
                Type = message.Type
            };
        }

        public Envelope CreateEnvelope<T>(EventMessage<T> message)
            where T : class, IEvent
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return new Envelope
            {
                Headers = message.Headers,
                Content = _serializer.SerializeObject(message.Event),
                MessageType = MessageType.Event,
                MessageId = message.MessageId,
                Type = message.Type
            };
        }

        public CommandMessage CreateCommandMessage(Envelope envelope, Type commandType)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            if (commandType == null)
            {
                throw new ArgumentNullException(nameof(commandType));
            }

            var command = _serializer.DeserializeObject(envelope.Content, commandType) as ICommand;

            var commandMessageType = typeof(CommandMessage<>).MakeGenericType(commandType);
            var commandMessage = (CommandMessage)Activator.CreateInstance(commandMessageType);

            commandMessage.SetCommand(command);
            commandMessage.Headers = envelope.Headers;
            commandMessage.MessageId = envelope.MessageId;

            return commandMessage;
        }

        public EventMessage CreateEventMessage(Envelope envelope, Type eventType)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            if (eventType == null)
            {
                throw new ArgumentNullException(nameof(eventType));
            }
            
            var @event = _serializer.DeserializeObject(envelope.Content, eventType) as IEvent;

            var eventMessageType = typeof(EventMessage<>).MakeGenericType(eventType);
            var eventMessage = (EventMessage)Activator.CreateInstance(eventMessageType);

            eventMessage.SetEvent(@event);
            eventMessage.Headers = envelope.Headers;
            eventMessage.MessageId = envelope.MessageId;

            return eventMessage;
        }
    }

    public class Envelope
    {
        public string MessageId { get; set; }

        public HeaderBag Headers { get; set; }

        public MessageType MessageType { get; set; }

        public Type Type { get; set; }

        public MessageDescriptor Descriptor => MessageDescriptor.CreateFromType(Type);

        public string Content { get; set; }
    }
}