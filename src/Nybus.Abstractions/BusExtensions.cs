using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nybus
{
    public static class BusExtensions
    {
        public static Task InvokeCommandAsync<TCommand>(this IBus bus, TCommand command) where TCommand : class, ICommand
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            return bus.InvokeCommandAsync(command, Guid.NewGuid());
        }

        public static Task RaiseEventAsync<TEvent>(this IBus bus, TEvent @event) where TEvent : class, IEvent
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            return bus.RaiseEventAsync(@event, Guid.NewGuid());
        }

        public static async Task InvokeManyCommandsAsync<TCommand>(this IBus bus, IEnumerable<TCommand> commands) where TCommand : class, ICommand
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            if (commands == null) return;

            await Task.WhenAll(commands.Select(bus.InvokeCommandAsync)).ConfigureAwait(false);
        }

        public static async Task InvokeManyCommandsAsync<TCommand>(this IBus bus, IEnumerable<TCommand> commands, Guid correlationId) where TCommand : class, ICommand
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            if (commands == null) return;

            await Task.WhenAll(commands.Select(c => bus.InvokeCommandAsync(c, correlationId))).ConfigureAwait(false);
        }

        public static async Task RaiseManyEventsAsync<TEvent>(this IBus bus, IEnumerable<TEvent> events) where TEvent : class, IEvent
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            if (events == null) return;

            await Task.WhenAll(events.Select(bus.RaiseEventAsync)).ConfigureAwait(false);
        }

        public static async Task RaiseManyEventsAsync<TEvent>(this IBus bus, IEnumerable<TEvent> events, Guid correlationId) where TEvent : class, IEvent
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            if (events == null) return;

            await Task.WhenAll(events.Select(e => bus.RaiseEventAsync(e, correlationId))).ConfigureAwait(false);
        }
    }
}
