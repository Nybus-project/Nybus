using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.SubscriptionConfigurators;
using Nybus.Configuration;
using Nybus.Logging;
using Nybus.Utils;

namespace Nybus.MassTransit
{
    public class MassTransitBusEngine : IBusEngine
    {
        private readonly MassTransitConnectionDescriptor _connectionDescriptor;
        private readonly MassTransitOptions _options;
        private Status _status = Status.New;
        private readonly List<IServiceBus> _serviceBusses = new List<IServiceBus>();
        private readonly ILogger _logger;

        public MassTransitBusEngine(MassTransitConnectionDescriptor connectionDescriptor, MassTransitOptions options)
        {
            if (connectionDescriptor == null) throw new ArgumentNullException(nameof(connectionDescriptor));
            if (options == null) throw new ArgumentNullException(nameof(options));
            _connectionDescriptor = connectionDescriptor;
            _options = options;
            _logger = _options.LoggerFactory.CreateLogger(nameof(MassTransitBusEngine));
        }

        public MassTransitBusEngine(MassTransitConnectionDescriptor connectionDescriptor) : this (connectionDescriptor, new MassTransitOptions()) { }

        private void EnsureBusIsRunning()
        {
            if (_status == Status.New)
            {
                throw new InvalidOperationException("Broker not started");
            }
        }

        public Task SendCommand<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand
        {
            EnsureBusIsRunning();

            CommandServiceBus.Publish(message.Command, pc => _options.ContextManager.SetCommandMessageHeaders(message, pc));

            return Task.CompletedTask;
        }

        public Task SendEvent<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent
        {
            EnsureBusIsRunning();

            EventServiceBus.Publish(message.Event, pc => _options.ContextManager.SetEventMessageHeaders(message, pc));

            return Task.CompletedTask;
        }

        #region SubscribeToCommand

        private readonly List<Action<SubscriptionBusServiceConfigurator>> _commandSubscriptions = new List<Action<SubscriptionBusServiceConfigurator>>();

        public void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand
        {
            if (commandReceived == null)
            {
                throw new ArgumentNullException(nameof(commandReceived));
            }

            _commandSubscriptions.Add(configurator => configurator.Handler<TCommand>((ctx, body) => HandleCommand(commandReceived, ctx).WaitAndUnwrapException()));
        }

        private async Task HandleCommand<TCommand>(CommandReceived<TCommand> commandHandler, IConsumeContext<TCommand> context)
            where TCommand : class, ICommand
        {
            CommandMessage<TCommand> message = _options.ContextManager.CreateCommandMessage(context);

            _logger.LogVerbose(new { commandType = typeof(TCommand).FullName, correlationId = message.CorrelationId, retryCount = context.RetryCount, command = message.Command },
                arg => $"Received command of type {arg.commandType} with correlationId {arg.correlationId}. (Try n. {arg.retryCount}). Command: {arg.command.ToString()}");

            try
            {
                await commandHandler.Invoke(message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(new { commandType = typeof(TCommand).FullName, correlationId = message.CorrelationId, retryCount = context.RetryCount, command = context.Message }, ex,
                    (arg, e) => $"Error while processing event of type {arg.commandType} with correlationId {arg.correlationId}. (Try n. {arg.retryCount}). Command: {arg.command.ToString()}. Error: {e.Message}");

                var handled = await _options.CommandErrorStrategy.HandleError(context, ex).ConfigureAwait(false);

                if (!handled)
                {
                    throw ExceptionManager.PrepareForRethrow(ex);
                }
            }
        }

        #endregion

        #region SubscribeToEvent

        private readonly List<Action<SubscriptionBusServiceConfigurator>> _eventSubscriptions = new List<Action<SubscriptionBusServiceConfigurator>>();

        public void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent
        {
            if (eventReceived == null)
            {
                throw new ArgumentNullException(nameof(eventReceived));
            }

            _eventSubscriptions.Add(configurator => configurator.Handler<TEvent>((ctx, body) => HandleEvent(eventReceived, ctx).WaitAndUnwrapException()));
        }

        private async Task HandleEvent<TEvent>(EventReceived<TEvent> eventHandler, IConsumeContext<TEvent> context) where TEvent : class, IEvent
        {
            EventMessage<TEvent> message = _options.ContextManager.CreateEventMessage(context);

            _logger.LogVerbose(new { @event = message.Event, eventType = typeof(TEvent).FullName, correlationId = message.CorrelationId, retryCount = context.RetryCount },
                arg => $"Received event of type {arg.eventType} with correlationId {arg.correlationId}. (Try n. {arg.retryCount}). Event: {arg.@event.ToString()}");

            try
            {
                await eventHandler.Invoke(message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(new { eventType = typeof(TEvent).FullName, correlationId = message.CorrelationId, retryCount = context.RetryCount, @event = context.Message }, ex, 
                    (arg, e) =>$"Error while processing event of type {arg.eventType} with correlationId {arg.correlationId}. (Try n. {arg.retryCount}). Event: {arg.@event.ToString()}. Error: {e.Message}");

                var handled = await _options.EventErrorStrategy.HandleError(context, ex).ConfigureAwait(false);

                if (!handled)
                {
                    throw ExceptionManager.PrepareForRethrow(ex);
                }
            }
        }

        #endregion

        public Task Start()
        {
            _logger.LogVerbose("Bus engine starting");
            try
            {
                _serviceBusses.Add(_options.ServiceBusFactory.CreateServiceBus(_connectionDescriptor, _options.EventQueueStrategy, _eventSubscriptions));

                if (_commandSubscriptions.Any())
                {
                    _serviceBusses.Add(_options.ServiceBusFactory.CreateServiceBus(_connectionDescriptor, _options.CommandQueueStrategy, _commandSubscriptions));
                }

                _status = Status.Running;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Bus engine failed to start", ex);

                throw new Exception("Bus failed to start", ex);
            }

            _logger.LogVerbose("Bus engine started");
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _logger.LogVerbose("Bus engine stopping");

            foreach (var bus in _serviceBusses)
                bus.Dispose();

            _status = Status.New;

            _logger.LogVerbose("Bus engine stopped");

            return Task.CompletedTask;
        }

        private enum Status
        {
            New,
            Running
        }

        public IServiceBus EventServiceBus
        {
            get
            {
                EnsureBusIsRunning();
                return _serviceBusses[0];
            }
        }

        public IServiceBus CommandServiceBus
        {
            get
            {
                EnsureBusIsRunning();

                return _serviceBusses.Last();
            }
        }
    }
}
