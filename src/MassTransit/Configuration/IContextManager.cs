using System;
using MassTransit;
using Nybus.Utils;

namespace Nybus.Configuration
{
    public interface IContextManager
    {
        EventMessage<TEvent> CreateEventMessage<TEvent>(IConsumeContext<TEvent> context) 
            where TEvent : class, IEvent;

        void SetEventMessageHeaders<TEvent>(EventMessage<TEvent> message, IPublishContext<TEvent> context) 
            where TEvent : class, IEvent;

        CommandMessage<TCommand> CreateCommandMessage<TCommand>(IConsumeContext<TCommand> context)
            where TCommand : class, ICommand;

        void SetCommandMessageHeaders<TCommand>(CommandMessage<TCommand> message, IPublishContext<TCommand> context)
            where TCommand : class, ICommand;
    }

    public class RabbitMqContextManager : IContextManager
    {
        public const string CorrelationIdKey = "nybus:CorrelationId";
        public const string MessageSentKey = "nybus:MessageSent";

        public EventMessage<TEvent> CreateEventMessage<TEvent>(IConsumeContext<TEvent> context) where TEvent : class, IEvent
        {
            return new EventMessage<TEvent>
            {
                CorrelationId = ExtractCorrelationId(context.Headers),
                Event = context.Message,
                SentOn = ExtractMessageSentTime(context.Headers)
            };
        }

        public void SetEventMessageHeaders<TEvent>(EventMessage<TEvent> message, IPublishContext<TEvent> context) where TEvent : class, IEvent
        {
            PersistCorrelationId(context, message.CorrelationId);
            PersistMessageSentTime(context, message.SentOn);
        }

        public CommandMessage<TCommand> CreateCommandMessage<TCommand>(IConsumeContext<TCommand> context) where TCommand : class, ICommand
        {
            return new CommandMessage<TCommand>
            {
                Command = context.Message,
                CorrelationId = ExtractCorrelationId(context.Headers),
                SentOn = ExtractMessageSentTime(context.Headers)
            };
        }

        public void SetCommandMessageHeaders<TCommand>(CommandMessage<TCommand> message, IPublishContext<TCommand> context) where TCommand : class, ICommand
        {
            PersistCorrelationId(context, message.CorrelationId);
            PersistMessageSentTime(context, message.SentOn);
        }

        private Guid ExtractCorrelationId(IMessageHeaders messageHeaders)
        {
            var header = messageHeaders[CorrelationIdKey];

            if (header == null)
            {
                return Guid.NewGuid();
            }

            return Guid.Parse(header);
        }

        private void PersistCorrelationId(ISendContext sendContext, Guid correlationId)
        {
            sendContext.SetHeader(CorrelationIdKey, correlationId.ToString("D"));
        }

        private DateTimeOffset ExtractMessageSentTime(IMessageHeaders messageHeaders)
        {
            var header = messageHeaders[MessageSentKey];

            if (header == null)
            {
                return Clock.Default.Now;
            }

            return DateTimeOffset.Parse(header);
        }

        private void PersistMessageSentTime(ISendContext sendContext, DateTimeOffset sentTime)
        {
            sendContext.SetHeader(MessageSentKey, sentTime.ToString("O"));
        }
    }
}