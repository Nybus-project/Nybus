using System;
using MassTransit;

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

        public EventMessage<TEvent> CreateEventMessage<TEvent>(IConsumeContext<TEvent> context) where TEvent : class, IEvent
        {
            return new EventMessage<TEvent>
            {
                CorrelationId = ExtractCorrelationId(context.Headers),
                Event = context.Message
            };
        }

        public void SetEventMessageHeaders<TEvent>(EventMessage<TEvent> message, IPublishContext<TEvent> context) where TEvent : class, IEvent
        {
            PersistCorrelationId(context, message.CorrelationId);
        }

        public CommandMessage<TCommand> CreateCommandMessage<TCommand>(IConsumeContext<TCommand> context) where TCommand : class, ICommand
        {
            return new CommandMessage<TCommand>
            {
                Command = context.Message,
                CorrelationId = ExtractCorrelationId(context.Headers)
            };
        }

        public void SetCommandMessageHeaders<TCommand>(CommandMessage<TCommand> message, IPublishContext<TCommand> context) where TCommand : class, ICommand
        {
            PersistCorrelationId(context, message.CorrelationId);
        }

        private Guid ExtractCorrelationId(IMessageHeaders messageHeaders)
        {
            return Guid.Parse(messageHeaders[CorrelationIdKey]);
        }

        private void PersistCorrelationId(ISendContext sendContext, Guid correlationId)
        {
            sendContext.SetHeader(CorrelationIdKey, correlationId.ToString("D"));
        }
    }
}