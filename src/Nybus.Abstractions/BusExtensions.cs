using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nybus
{
    public static class BusExtensions
    {
        private static readonly IDictionary<string, string> EmptyHeaders = new Dictionary<string, string>();

        public static Task InvokeCommandAsync<TCommand>(this IBus bus, TCommand command) where TCommand : class, ICommand
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            return bus.InvokeCommandAsync(command, Guid.NewGuid(), EmptyHeaders);
        }

        public static Task RaiseEventAsync<TEvent>(this IBus bus, TEvent @event) where TEvent : class, IEvent
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            return bus.RaiseEventAsync(@event, Guid.NewGuid(), EmptyHeaders);
        }

        public static Task InvokeCommandAsync<TCommand>(this IBus bus, TCommand command, Guid correlationId) where TCommand : class, ICommand
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            return bus.InvokeCommandAsync(command, correlationId, EmptyHeaders);
        }

        public static Task RaiseEventAsync<TEvent>(this IBus bus, TEvent @event, Guid correlationId) where TEvent : class, IEvent
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            return bus.RaiseEventAsync(@event, correlationId, EmptyHeaders);
        }

        public static Task InvokeCommandAsync<TCommand>(this IBus bus, TCommand command, IDictionary<string, string> headers) where TCommand : class, ICommand
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            return bus.InvokeCommandAsync(command, Guid.NewGuid(), headers);
        }

        public static Task RaiseEventAsync<TEvent>(this IBus bus, TEvent @event, IDictionary<string, string> headers) where TEvent : class, IEvent
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            return bus.RaiseEventAsync(@event, Guid.NewGuid(), headers);
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

            await Task.WhenAll(commands.Select(c => bus.InvokeCommandAsync(c, correlationId, EmptyHeaders))).ConfigureAwait(false);
        }

        public static async Task InvokeManyCommandsAsync<TCommand>(this IBus bus, IEnumerable<TCommand> commands, IDictionary<string, string> headers) where TCommand : class, ICommand
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            if (commands == null) return;

            await Task.WhenAll(commands.Select(c => bus.InvokeCommandAsync(c, headers))).ConfigureAwait(false);
        }

        public static async Task InvokeManyCommandsAsync<TCommand>(this IBus bus, IEnumerable<TCommand> commands, Guid correlationId, IDictionary<string, string> headers) where TCommand : class, ICommand
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            if (commands == null) return;

            await Task.WhenAll(commands.Select(c => bus.InvokeCommandAsync(c, correlationId, headers))).ConfigureAwait(false);
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

            await Task.WhenAll(events.Select(e => bus.RaiseEventAsync(e, correlationId, EmptyHeaders))).ConfigureAwait(false);
        }

        public static async Task RaiseManyEventsAsync<TEvent>(this IBus bus, IEnumerable<TEvent> events, IDictionary<string, string> headers) where TEvent : class, IEvent
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            if (events == null) return;

            await Task.WhenAll(events.Select(e => bus.RaiseEventAsync(e, headers))).ConfigureAwait(false);
        }

        public static async Task RaiseManyEventsAsync<TEvent>(this IBus bus, IEnumerable<TEvent> events, Guid correlationId, IDictionary<string, string> headers) where TEvent : class, IEvent
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }

            if (events == null) return;

            await Task.WhenAll(events.Select(e => bus.RaiseEventAsync(e, correlationId, headers))).ConfigureAwait(false);
        }
    }
}
