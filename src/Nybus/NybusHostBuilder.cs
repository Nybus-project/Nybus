using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Nybus.Configuration;

namespace Nybus
{
    public class NybusHostBuilder : ISubscriptionBuilder
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IList<Action<IBusHost>> _subscriptions = new List<Action<IBusHost>>();

        public NybusHostBuilder(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public NybusHost BuildHost(IBusEngine engine, IServiceProvider serviceProvider, INybusConfiguration configuration)
        {
            var host = new NybusHost(engine, configuration, serviceProvider, _loggerFactory.CreateLogger<NybusHost>());

            foreach (var subscription in _subscriptions)
            {
                subscription(host);
            }

            return host;
        }

        public void SubscribeToCommand<TCommand>(Type commandHandlerType)
            where TCommand : class, ICommand
        {
            if (!typeof(ICommandHandler<TCommand>).GetTypeInfo().IsAssignableFrom(commandHandlerType.GetTypeInfo()))
            {
                throw new ArgumentException($"{commandHandlerType.FullName} does not implement the ICommandHandler<{typeof(TCommand).FullName}> interface", nameof(commandHandlerType));
            }

            _subscriptions.Add(host =>
            {
                host.SubscribeToCommand<TCommand>((dispatcher, context) => host.ExecutionEnvironment.ExecuteCommandHandler(dispatcher,context,commandHandlerType));
            });
        }

        public void SubscribeToEvent<TEvent>(Type eventHandlerType)
            where TEvent : class, IEvent
        {
            if (!typeof(IEventHandler<TEvent>).GetTypeInfo().IsAssignableFrom(eventHandlerType.GetTypeInfo()))
            {
                throw new ArgumentException($"{eventHandlerType.FullName} does not implement the IEventHandler<{typeof(TEvent).FullName}> interface", nameof(eventHandlerType));
            }

            _subscriptions.Add(host =>
            {
                host.SubscribeToEvent<TEvent>((dispatcher, context) => host.ExecutionEnvironment.ExecuteEventHandler(dispatcher, context, eventHandlerType));
            });
        }
    }
}
