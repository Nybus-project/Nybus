using System;
using System.Threading.Tasks;

namespace Nybus
{
    public interface IBusEngine
    {
        Task SendMessage(Message message);

        void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived)
            where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived)
            where TEvent : class, IEvent;

        Task Start();

        Task Stop();
    }

    public delegate Task CommandReceived<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand;

    public delegate Task EventReceived<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent;

    public abstract class Message
    {
        protected Message()
        {
            CorrelationId = Guid.NewGuid();
        }

        public Guid CorrelationId { get; set; }
    }

    public class CommandMessage<TCommand> : Message
        where TCommand : class, ICommand
    {
        public CommandMessage(TCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            Command = command;
        }

        public TCommand Command { get; }
    }

    public class EventMessage<TEvent> : Message
        where TEvent : class, IEvent
    {
        public EventMessage(TEvent eventMessage)
        {
            if (eventMessage == null) throw new ArgumentNullException(nameof(eventMessage));
            Event = eventMessage;
        }

        public TEvent Event { get; }
    }

}