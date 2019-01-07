using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
// ReSharper disable InvokeAsExtensionMethod

namespace Nybus
{
    public class InMemoryBusEngine : IBusEngine
    {
        private ISubject<Message> _sequenceOfMessages;
        private bool _isStarted;
        private readonly ISet<Type> _acceptedTypes = new HashSet<Type>();

        public Task<IObservable<Message>> StartAsync()
        {
            _sequenceOfMessages = new Subject<Message>();

            var commands = _sequenceOfMessages
                                .Where(m => m != null)
                                .Where(m => m.MessageType == MessageType.Command)
                                .Cast<CommandMessage>()
                                .Where(m => _acceptedTypes.Contains(m.Type))
                                .Cast<Message>();

            var events = _sequenceOfMessages
                                .Where(m => m != null)
                                .Where(m => m.MessageType == MessageType.Event)
                                .Cast<EventMessage>()
                                .Where(m => _acceptedTypes.Contains(m.Type))
                                .Cast<Message>();

            _isStarted = true;

            return Task.FromResult(Observable.Merge(commands, events));
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
            _acceptedTypes.Add(typeof(TCommand));
        }

        public void SubscribeToEvent<TEvent>() where TEvent : class, IEvent
        {
            _acceptedTypes.Add(typeof(TEvent));
        }

        public Task NotifySuccessAsync(Message message)
        {
            return Task.CompletedTask;
        }

        public Task NotifyFailAsync(Message message)
        {
            return Task.CompletedTask;
        }

        public bool IsTypeAccepted(Type type) => _acceptedTypes.Contains(type);
    }
}
