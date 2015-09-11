using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.SubscriptionConfigurators;
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


        public Task SendMessage(Message message)
        {
            EnsureBusIsRunning();
            _serviceBusses[0].Publish(message);
            return Task.FromResult(0);
        }

        #region SubscribeToCommand

        private readonly List<Action<SubscriptionBusServiceConfigurator>> _commandSubscriptions = new List<Action<SubscriptionBusServiceConfigurator>>();

        public void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand
        {
            _commandSubscriptions.Add(configurator => configurator.Handler<CommandMessage<TCommand>>((ctx, message) => HandleCommand(commandReceived, ctx).WaitAndUnwrapException()));
        }

        private Task HandleCommand<TCommand>(CommandReceived<TCommand> commandHandler, IConsumeContext<CommandMessage<TCommand>> context) where TCommand : class, ICommand
        {
            try
            {
                return commandHandler?.Invoke(context.Message);
            }
            catch (Exception ex)
            {
                _options.CommandErrorStrategy.HandleError(context, ex);
            }

            return Task.FromResult(0);
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
            try
            {
                return eventHandler?.Invoke(context.Message);
            }
            catch (Exception ex)
            {
                _options.EventErrorStrategy.HandleError(context, ex);
            }

            return Task.FromResult(0);
        }

        #endregion

        public Task Start()
        {
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
                throw new Exception("Bus failed to start", ex);
            }

            return Task.FromResult(0);
        }

        public Task Stop()
        {
            foreach (var bus in _serviceBusses)
                bus.Dispose();

            _status = Status.New;

            return Task.FromResult(0);
        }

        private enum Status
        {
            New,
            Running
        }
    }
}
