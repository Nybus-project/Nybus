using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nybus.Configuration
{
    public class InMemoryBusEngine : IBusEngine
    {
        private readonly List<Message> _sentMessages = new List<Message>();
        private readonly Dictionary<Type, Delegate> _subscribedCommands = new Dictionary<Type, Delegate>();
        private readonly Dictionary<Type, Delegate> _subscribedEvents = new Dictionary<Type, Delegate>();

        private Task SendMessage<TMessage>(TMessage message) where TMessage : Message
        {
            _sentMessages.Add(message);
            return Task.CompletedTask;
        }

        public Task SendCommand<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand => SendMessage(message);

        public Task SendEvent<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent => SendMessage(message);

        public void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand
        {
            _subscribedCommands.Add(typeof(TCommand), commandReceived);
        }

        public void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent
        {
            _subscribedEvents.Add(typeof(TEvent), eventReceived);
        }

        public Task Start()
        {
            Status = EngineStatus.Started;
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            Status = EngineStatus.Stopped;
            return Task.CompletedTask;
        }

        public enum EngineStatus
        {
            Started, Stopped
        }

        public EngineStatus Status { get; private set; } = EngineStatus.Stopped;

        public async Task HandleEvent<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent
        {
            Delegate @delegate;

            if (!_subscribedEvents.TryGetValue(typeof (TEvent), out @delegate))
                return;

            EventReceived<TEvent> eventReceivedDelegate = (EventReceived<TEvent>) @delegate;

            await eventReceivedDelegate(message).ConfigureAwait(false);
        }

        public bool IsEventHandeld<TEvent>() where TEvent : class, IEvent
        {
            return _subscribedEvents.ContainsKey(typeof (TEvent));
        }

        public async Task HandleCommand<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand
        {
            Delegate @delegate;

            if (!_subscribedCommands.TryGetValue(typeof(TCommand), out @delegate))
                return;

            CommandReceived<TCommand> commandReceivedDelegate = (CommandReceived<TCommand>)@delegate;

            await commandReceivedDelegate(message).ConfigureAwait(false);
        }
        public bool IsCommandHandled<TCommand>() where TCommand : class, ICommand
        {
            return _subscribedCommands.ContainsKey(typeof(TCommand));
        }

        public IReadOnlyList<Message> SentMessages => _sentMessages;
    }
}