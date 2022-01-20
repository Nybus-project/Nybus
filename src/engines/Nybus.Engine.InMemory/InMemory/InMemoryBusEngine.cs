using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Nybus.Utils;

// ReSharper disable InvokeAsExtensionMethod

namespace Nybus.InMemory
{
    public class InMemoryBusEngine : IBusEngine
    {
        private readonly IMessageDescriptorStore _messageDescriptorStore;
        private readonly IEnvelopeService _envelopeService;
        private ISubject<Envelope> _sequenceOfMessages;
        private bool _isStarted;
        private readonly ISet<Type> _acceptedTypes = new HashSet<Type>();

        public InMemoryBusEngine(IMessageDescriptorStore messageDescriptorStore, IEnvelopeService envelopeService)
        {
            _messageDescriptorStore = messageDescriptorStore ?? throw new ArgumentNullException(nameof(messageDescriptorStore));
            _envelopeService = envelopeService ?? throw new ArgumentNullException(nameof(envelopeService));
        }

        public Task<IObservable<Message>> StartAsync()
        {
            _sequenceOfMessages = new Subject<Envelope>();

            var commands = _sequenceOfMessages
                                .Where(m => m != null)
                                .Where(m => m.MessageType == MessageType.Command)
                                .Select(GetCommandMessage)
                                .Where(m => m != null)
                                .Cast<Message>();

            var events = _sequenceOfMessages
                                .Where(m => m != null)
                                .Where(m => m.MessageType == MessageType.Event)
                                .Select(GetEventMessage)
                                .Where(m => m != null)
                                .Cast<Message>();

            _isStarted = true;

            return Task.FromResult(Observable.Merge(commands, events));

            CommandMessage GetCommandMessage(Envelope incoming)
            {
                var incomingType = incoming.Type;

                if (_acceptedTypes.Contains(incomingType))
                {
                    return _envelopeService.CreateCommandMessage(incoming, incomingType);
                }

                if (_messageDescriptorStore.FindCommandTypeForDescriptor(incoming.Descriptor, out var outgoingType))
                {
                    return _envelopeService.CreateCommandMessage(incoming, outgoingType);
                }

                return null;
            }

            EventMessage GetEventMessage(Envelope incoming)
            {
                var incomingType = incoming.Type;

                if (_acceptedTypes.Contains(incomingType))
                {
                    return _envelopeService.CreateEventMessage(incoming, incomingType);
                }

                if (_messageDescriptorStore.FindEventTypeForDescriptor(incoming.Descriptor, out var outgoingType))
                {
                    return _envelopeService.CreateEventMessage(incoming, outgoingType);
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

        public void SubscribeToCommand<TCommand>() where TCommand : class, ICommand
        {
            _messageDescriptorStore.RegisterCommandType<TCommand>();
            _acceptedTypes.Add(typeof(TCommand));
        }

        public void SubscribeToEvent<TEvent>() where TEvent : class, IEvent
        {
            _messageDescriptorStore.RegisterEventType<TEvent>();
            _acceptedTypes.Add(typeof(TEvent));
        }

        public Task SendMessageAsync(Message message)
        {
            if (_isStarted)
            {
                var envelope = _envelopeService.CreateEnvelope(message);
                _sequenceOfMessages.OnNext(envelope);
            }

            return Task.CompletedTask;
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

        public Task SendMessageToErrorQueueAsync(Message message)
        {
            return Task.CompletedTask;
        }

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
