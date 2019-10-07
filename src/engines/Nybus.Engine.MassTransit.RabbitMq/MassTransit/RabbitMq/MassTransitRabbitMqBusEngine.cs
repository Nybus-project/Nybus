using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace Nybus.MassTransit.RabbitMq
{
    public interface IMassTransitRabbitMqBusBuilder
    {
        void AddConfiguration(Action<IRabbitMqBusFactoryConfigurator> configuration);

        IBusControl BuildBus();

        void SubscribeToCommand<TCommand>(MessageHandler<TCommand> handler)
            where TCommand : class, ICommand;

        void SubscribeToEvent<TEvent>(MessageHandler<TEvent> handler)
            where TEvent : class, IEvent;
    }

    public class MassTransitRabbitMqBusBuilder : IMassTransitRabbitMqBusBuilder
    {
        private readonly IList<Action<IRabbitMqBusFactoryConfigurator>> _configurations = new List<Action<IRabbitMqBusFactoryConfigurator>>();
        private readonly IList<Action<IReceiveEndpointConfigurator>> _commandEndpointConfigurators = new List<Action<IReceiveEndpointConfigurator>>();
        private readonly IList<Action<IReceiveEndpointConfigurator>> _eventEndpointConfigurators = new List<Action<IReceiveEndpointConfigurator>>();

        public void AddConfiguration(Action<IRabbitMqBusFactoryConfigurator> configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _configurations.Add(configuration);
        }

        public IBusControl BuildBus()
        {
            if (_commandEndpointConfigurators.Any())
            {
                _configurations.Add(ConfigureCommandQueue);
            }

            if (_eventEndpointConfigurators.Any())
            {
                _configurations.Add(ConfigureEventQueue);
            }

            return RabbitMqBusFactory.Create(configurator =>
            {
                foreach (var configuration in _configurations)
                {
                    configuration(configurator);
                }
            });
        }

        private void ConfigureCommandQueue(IRabbitMqBusFactoryConfigurator configurator)
        {
            configurator.ReceiveEndpoint("command-queue", endpoint =>
            {
                foreach (var endpointConfigurator in _commandEndpointConfigurators)
                {
                    endpointConfigurator(endpoint);
                }
            });
        }

        private void ConfigureEventQueue(IRabbitMqBusFactoryConfigurator configurator)
        {
            configurator.ReceiveEndpoint("event-queue", endpoint =>
            {
                foreach (var endpointConfigurator in _eventEndpointConfigurators)
                {
                    endpointConfigurator(endpoint);
                }
            });
        }

        public void SubscribeToCommand<TCommand>(MessageHandler<TCommand> handler)
            where TCommand : class, ICommand
        {
            _commandEndpointConfigurators.Add(item => item.Handler(handler));
        }

        public void SubscribeToEvent<TEvent>(MessageHandler<TEvent> handler)
            where TEvent : class, IEvent
        {
            _eventEndpointConfigurators.Add(item => item.Handler(handler));
        }
    }

    public class MassTransitRabbitMqBusEngine : IBusEngine
    {
        private readonly IMassTransitRabbitMqBusBuilder _busBuilder;
        private readonly ISubject<Message> _messages = new Subject<Message>();
        private IBusControl _busControl;

        public MassTransitRabbitMqBusEngine(IMassTransitRabbitMqBusBuilder busBuilder)
        {
            _busBuilder = busBuilder ?? throw new ArgumentNullException(nameof(busBuilder));
        }

        public async Task<IObservable<Message>> StartAsync()
        {
            _busControl = _busBuilder.BuildBus();

            await _busControl.StartAsync().ConfigureAwait(false);

            return _messages;
        }

        public async Task StopAsync()
        {
            await _busControl.StopAsync();
        }

        private readonly IDictionary<string, TaskCompletionSource<object>> _processingMessages = new ConcurrentDictionary<string, TaskCompletionSource<object>>();

        public void SubscribeToCommand<TCommand>()
            where TCommand : class, ICommand
        {
            _busBuilder.SubscribeToCommand<TCommand>(ProcessCommandMessage);
        }

        private async Task ProcessCommandMessage<TCommand>(ConsumeContext<TCommand> context)
            where TCommand : class, ICommand
        {
            _messages.OnNext(CreateMessage());

            _processingMessages.Add(MessageId(context), new TaskCompletionSource<object>());

            await _processingMessages[MessageId(context)].Task;

            CommandMessage<TCommand> CreateMessage()
            {
                return new CommandMessage<TCommand>
                {
                    MessageId = MessageId(context),
                    Command = context.Message,
                    Headers = new HeaderBag
                    {
                        CorrelationId = context.CorrelationId ?? Guid.Empty,
                        SentOn = new DateTimeOffset(context.SentTime.GetValueOrDefault(), TimeSpan.Zero)
                    }
                };
            }
        }

        public void SubscribeToEvent<TEvent>()
            where TEvent : class, IEvent
        {
            _busBuilder.SubscribeToEvent<TEvent>(ProcessEventMessage);
        }

        private async Task ProcessEventMessage<TEvent>(ConsumeContext<TEvent> context)
            where TEvent : class, IEvent
        {
            _messages.OnNext(CreateMessage());

            _processingMessages.Add(MessageId(context), new TaskCompletionSource<object>());

            await _processingMessages[MessageId(context)].Task;

            EventMessage<TEvent> CreateMessage()
            {
                return new EventMessage<TEvent>
                {
                    MessageId = MessageId(context),
                    Event = context.Message,
                    Headers = new HeaderBag
                    {
                        CorrelationId = context.CorrelationId ?? Guid.Empty,
                        SentOn = new DateTimeOffset(context.SentTime.GetValueOrDefault(), TimeSpan.Zero)
                    }
                };
            }
        }

        private static string Nybus(string key) => $"Nybus:{key}";

        private static string MessageId(ConsumeContext context) => context.MessageId?.ToNewId().ToString();

        public Task SendMessageAsync(Message message)
        {
            return _busControl.Publish(message.Item, message.Type, context =>
            {
                context.CorrelationId = message.Headers.CorrelationId;
            });
        }

        public Task NotifySuccessAsync(Message message)
        {
            if (_processingMessages.TryGetValue(message.MessageId, out var tcs))
            {
                tcs.TrySetResult(null);
                _processingMessages.Remove(message.MessageId);
            }

            return Task.CompletedTask;
        }

        public Task NotifyFailAsync(Message message)
        {
            if (_processingMessages.TryGetValue(message.MessageId, out var tcs))
            {
                tcs.TrySetException(new Exception("An error occurred while processing the message"));
                _processingMessages.Remove(message.MessageId);
            }

            return Task.CompletedTask;
        }
    }
}
