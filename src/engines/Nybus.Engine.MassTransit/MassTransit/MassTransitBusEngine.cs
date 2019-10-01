using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.RabbitMqTransport;

namespace Nybus.MassTransit
{
    public class MassTransitBusEngine : IBusEngine
    {
        private readonly List<Action<IRabbitMqBusFactoryConfigurator>> _busFactoryConfigurators = new List<Action<IRabbitMqBusFactoryConfigurator>>();

        private readonly Subject<Message> _messages = new Subject<Message>();

        private BusHandle _busHandle;
        private IBusControl _busControl;

        public async Task<IObservable<Message>> StartAsync()
        {
            if (_commandEndpointConfigurators.Any())
            {
                _busFactoryConfigurators.Add(ConfigureCommandQueue);
            }

            if (_eventEndpointConfigurators.Any())
            {
                _busFactoryConfigurators.Add(ConfigureEventQueue);
            }

            _busControl = RabbitMqBusFactory.Create(configurator =>
            {
                configurator.Host(new Uri("rabbitmq://localhost"), host =>
                {
                    host.Username("guest");
                    host.Password("guest");
                });

                foreach (var configuration in _busFactoryConfigurators)
                {
                    configuration(configurator);
                }
            });

            _busHandle = await _busControl.StartAsync();

            return _messages;
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

        public async Task StopAsync()
        {
            await _busHandle.StopAsync();
        }

        private readonly List<Action<IReceiveEndpointConfigurator>> _commandEndpointConfigurators = new List<Action<IReceiveEndpointConfigurator>>();

        private readonly IDictionary<string, TaskCompletionSource<object>> _processingMessages = new ConcurrentDictionary<string, TaskCompletionSource<object>>();

        public void SubscribeToCommand<TCommand>()
            where TCommand : class, ICommand
        {
            _commandEndpointConfigurators.Add(item => item.Handler<TCommand>(AddCommandMessage));
        }

        private async Task AddCommandMessage<TCommand>(ConsumeContext<TCommand> context)
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

        private readonly List<Action<IReceiveEndpointConfigurator>> _eventEndpointConfigurators = new List<Action<IReceiveEndpointConfigurator>>();

        public void SubscribeToEvent<TEvent>()
            where TEvent : class, IEvent
        {
            _eventEndpointConfigurators.Add(item => item.Handler<TEvent>(AddEventMessage));
        }

        private async Task AddEventMessage<TEvent>(ConsumeContext<TEvent> context)
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
