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

        public MassTransitBusEngine(MassTransitConnectionDescriptor connectionDescriptor, MassTransitOptions options)
        {
            if (connectionDescriptor == null) throw new ArgumentNullException(nameof(connectionDescriptor));
            if (options == null) throw new ArgumentNullException(nameof(options));
            _connectionDescriptor = connectionDescriptor;
            _options = options;
        }

        public MassTransitBusEngine(MassTransitConnectionDescriptor connectionDescriptor) : this (connectionDescriptor, new MassTransitOptions()) { }

        private void EnsureBusIsRunning()
        {
            if (_status == Status.New)
            {
                throw new InvalidOperationException("Broker not started");
            }
        }


        private Task SendMessage<TMessage>(IServiceBus bus, TMessage message) where TMessage : Message
        {
            EnsureBusIsRunning();
            bus.Publish(message);
            return Task.CompletedTask;
        }

        public Task SendCommand<TCommand>(CommandMessage<TCommand> message) where TCommand : class, ICommand
        {
            //EnsureBusIsRunning();

            //IServiceBus selectedServiceBus = _serviceBusses[0];

            //if (_serviceBusses.Count > 1)
            //{
            //    selectedServiceBus = _serviceBusses[1];
            //}

            return SendMessage(CommandServiceBus, message);
        }

        public Task SendEvent<TEvent>(EventMessage<TEvent> message) where TEvent : class, IEvent
        {
            return SendMessage(EventServiceBus, message);
        }

        #region SubscribeToCommand

        private readonly List<Action<SubscriptionBusServiceConfigurator>> _commandSubscriptions = new List<Action<SubscriptionBusServiceConfigurator>>();

        public void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand
        {
            _commandSubscriptions.Add(configurator => configurator.Handler<CommandMessage<TCommand>>((ctx, message) => HandleCommand(commandReceived, ctx).WaitAndUnwrapException()));
        }

        private Task HandleCommand<TCommand>(CommandReceived<TCommand> commandHandler, IConsumeContext<CommandMessage<TCommand>> context) where TCommand : class, ICommand
        {
            _options.Logger.Log(LogLevel.Trace, "Received command", new { commandType = typeof(TCommand).FullName, correlationId = context.Message.CorrelationId, retryCount = context.RetryCount });
            try
            {
                return commandHandler?.Invoke(context.Message);
            }
            catch (Exception ex)
            {
                _options.Logger.Log(LogLevel.Error, "Error while processing a command", new { commandType = typeof(TCommand).FullName, correlationId = context.Message.CorrelationId, retryCount = context.RetryCount, exception = ex, command = context.Message.Command });
                _options.CommandErrorStrategy.HandleError(context, ex);
            }

            return Task.CompletedTask;
        }

        #endregion

        #region SubscribeToEvent

        private readonly List<Action<SubscriptionBusServiceConfigurator>> _eventSubscriptions = new List<Action<SubscriptionBusServiceConfigurator>>();

        public void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent
        {
            _eventSubscriptions.Add(configurator => configurator.Handler<EventMessage<TEvent>>((ctx, message) => HandleEvent(eventReceived, ctx).WaitAndUnwrapException()));
        }

        private Task HandleEvent<TEvent>(EventReceived<TEvent> eventHandler, IConsumeContext<EventMessage<TEvent>> context) where TEvent : class, IEvent
        {
            _options.Logger.Log(LogLevel.Trace, "Received event", new { eventType = typeof(TEvent).FullName, correlationId = context.Message.CorrelationId, retryCount = context.RetryCount});
            try
            {
                return eventHandler?.Invoke(context.Message);
            }
            catch (Exception ex)
            {
                _options.Logger.Log(LogLevel.Error, "Error while processing an event", new { eventType = typeof(TEvent).FullName, correlationId = context.Message.CorrelationId, retryCount = context.RetryCount , exception = ex, @event = context.Message.Event });
                _options.EventErrorStrategy.HandleError(context, ex);
            }

            return Task.CompletedTask;
        }

        #endregion

        public Task Start()
        {
            _options.Logger.Log(LogLevel.Trace, "Bus engine starting");
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
                _options.Logger.Log(LogLevel.Fatal, "Bus engine failed to start", new {exception = ex});
                throw new Exception("Bus failed to start", ex);
            }

            _options.Logger.Log(LogLevel.Trace, "Bus engine started");
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _options.Logger.Log(LogLevel.Trace, "Bus engine stopping");

            foreach (var bus in _serviceBusses)
                bus.Dispose();

            _status = Status.New;

            _options.Logger.Log(LogLevel.Trace, "Bus engine stopped");

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
