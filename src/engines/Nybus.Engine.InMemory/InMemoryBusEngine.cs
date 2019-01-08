using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Nybus.Utils;

// ReSharper disable InvokeAsExtensionMethod

namespace Nybus
{
    public class InMemoryBusEngine : IBusEngine
    {
        private readonly IMessageDescriptorStore _messageDescriptorStore;
        private ISubject<Message> _sequenceOfMessages;
        private bool _isStarted;
        private readonly ISet<Type> _acceptedTypes = new HashSet<Type>();

        public InMemoryBusEngine(IMessageDescriptorStore messageDescriptorStore)
        {
            _messageDescriptorStore = messageDescriptorStore ?? throw new ArgumentNullException(nameof(messageDescriptorStore));
        }

        public Task<IObservable<Message>> StartAsync()
        {
            _sequenceOfMessages = new Subject<Message>();

            var commands = _sequenceOfMessages
                                .Where(m => m != null)
                                .Where(m => m.MessageType == MessageType.Command)
                                .Cast<CommandMessage>()
                                .Select(GetCommandMessage)
                                .Where(m => m != null)
                                .Cast<Message>();

            var events = _sequenceOfMessages
                                .Where(m => m != null)
                                .Where(m => m.MessageType == MessageType.Event)
                                .Cast<EventMessage>()
                                .Select(GetEventMessage)
                                .Where(m => m != null)
                                .Cast<Message>();

            _isStarted = true;

            return Task.FromResult(Observable.Merge(commands, events));

            CommandMessage GetCommandMessage<TMessage>(TMessage incoming) where TMessage : CommandMessage
            {
                var incomingType = incoming.Type;

                if (_acceptedTypes.Contains(incomingType))
                {
                    return incoming;
                }

                if (_messageDescriptorStore.TryGetTypeForDescriptor(incoming.Descriptor, out var outgoingType))
                {
                    var outgoingCommand = Activator.CreateInstance(outgoingType) as ICommand;

                    var outgoingMessageType = incoming.GetType().GetGenericTypeDefinition().MakeGenericType(outgoingType);
                    var outgoing = (CommandMessage) Activator.CreateInstance(outgoingMessageType);

                    outgoing.SetCommand(outgoingCommand);
                    outgoing.Headers = incoming.Headers;
                    outgoing.MessageId = incoming.MessageId;

                    return outgoing;
                }

                return null;
            }

            EventMessage GetEventMessage<TMessage>(TMessage incoming) where TMessage : EventMessage
            {
                var incomingType = incoming.Type;

                if (_acceptedTypes.Contains(incomingType))
                {
                    return incoming;
                }

                if (_messageDescriptorStore.TryGetTypeForDescriptor(incoming.Descriptor, out var outgoingType))
                {
                    var outgoingEvent = Activator.CreateInstance(outgoingType) as IEvent;

                    var outgoingMessageType = incoming.GetType().GetGenericTypeDefinition().MakeGenericType(outgoingType);
                    var outgoing = (EventMessage)Activator.CreateInstance(outgoingMessageType);

                    outgoing.SetEvent(outgoingEvent);
                    outgoing.Headers = incoming.Headers;
                    outgoing.MessageId = incoming.MessageId;

                    return outgoing;
                }

                return null;
            }
        }

        public Task StopAsync()
        {
            if (_isStarted)
            {
                _sequenceOfMessages.OnCompleted();
                _sequenceOfMessages = null;
            }

            return Task.CompletedTask;
        }

        public Task SendCommandAsync<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand
        {
            if (_isStarted)
            {
                _sequenceOfMessages.OnNext(message);
            }

            return Task.CompletedTask;
        }

        public Task SendEventAsync<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent
        {
            if (_isStarted)
            {
                _sequenceOfMessages.OnNext(message);
            }

            return Task.CompletedTask;
        }

        public void SubscribeToCommand<TCommand>() where TCommand : class, ICommand
        {
            _messageDescriptorStore.RegisterType(typeof(TCommand));
            _acceptedTypes.Add(typeof(TCommand));
        }

        public void SubscribeToEvent<TEvent>() where TEvent : class, IEvent
        {
            _messageDescriptorStore.RegisterType(typeof(TEvent));
            _acceptedTypes.Add(typeof(TEvent));
        }

        public Task NotifySuccessAsync(Message message)
        {
            OnMessageNotifySuccess?.Invoke(this, new MessageEventArgs(message));
            return Task.CompletedTask;
        }

        public Task NotifyFailAsync(Message message)
        {
            OnMessageNotifyFail?.Invoke(this, new MessageEventArgs(message));
            return Task.CompletedTask;
        }

        public bool IsTypeAccepted(Type type) => _acceptedTypes.Contains(type);

        public event EventHandler<MessageEventArgs> OnMessageNotifySuccess;

        public event EventHandler<MessageEventArgs> OnMessageNotifyFail;
    }

    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(Message message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public Message  Message { get; }
    }
}
