using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Nybus
{
    public class NybusBusBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly List<Action<IBus>> _subscriptions = new List<Action<IBus>>();

        public NybusBusBuilder(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IBus Build(IBusEngine engine)
        {
            var bus = new NybusBus(engine, _serviceProvider, _loggerFactory.CreateLogger<NybusBus>());

            foreach (var subscription in _subscriptions)
            {
                subscription(bus);
            }

            return bus;
        }

        public void SubscribeToCommand<TCommand>(CommandReceived<TCommand> commandReceived) where TCommand : class, ICommand
        {
            _subscriptions.Add(bus => bus.SubscribeToCommand(commandReceived));
        }

        public void SubscribeToCommand<TCommand, TCommandHandler>()
            where TCommand : class, ICommand
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            _subscriptions.Add(bus => bus.SubscribeToCommand<TCommand>(async (b, ctx) =>
            {
                TCommandHandler handler = _serviceProvider.GetRequiredService<TCommandHandler>();

                await handler.HandleAsync(b, ctx).ConfigureAwait(false);
            }));
        }

        public void SubscribeToEvent<TEvent, TEventHandler>()
            where TEvent : class, IEvent
            where TEventHandler : class, IEventHandler<TEvent>
        {
            _subscriptions.Add(bus => bus.SubscribeToEvent<TEvent>(async (b, ctx) =>
            {
                TEventHandler handler = _serviceProvider.GetRequiredService<TEventHandler>();

                await handler.HandleAsync(b, ctx).ConfigureAwait(false);
            }));
        }

        public void SubscribeToEvent<TEvent>(EventReceived<TEvent> eventReceived) where TEvent : class, IEvent
        {
            _subscriptions.Add(bus => bus.SubscribeToEvent(eventReceived));
        }
    }
}
